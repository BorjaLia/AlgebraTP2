using System.Collections.Generic;
using UnityEngine;

namespace SpatialPartitioning
{
    public class TreeCollisionManager : MonoBehaviour
    {
        [Header("Tick Settings")]
        public float updatesPerSecond = 2f;

        [Header("Collision Settings")]
        public float validityRadiusMultiplier = 2f;

        public Tree WorldTree { get; set; }

        public List<MeshTriangleTreeManager> ActiveElements { get; private set; } = new List<MeshTriangleTreeManager>();

        private float tickAccumulator = 0f;

        private List<TreeNode> activeLeaves = new List<TreeNode>();
        private int currentLeafIndex = 0;

        private List<TreeNode> overlappingLeavesBuffer = new List<TreeNode>();
        private HashSet<ITreeElement> uniqueElementsBufferA = new HashSet<ITreeElement>();
        private HashSet<ITreeElement> uniqueElementsBufferB = new HashSet<ITreeElement>();

        private void Awake()
        {
            ServiceLocator.Register(this);
        }
        private void Start()
        {
            tickAccumulator = (1f / updatesPerSecond);
        }

        public void RegisterElement(MeshTriangleTreeManager element)
        {
            if (!ActiveElements.Contains(element)) ActiveElements.Add(element);
        }

        public void UnRegisterElement(MeshTriangleTreeManager element)
        {
            if (!ActiveElements.Contains(element)) ActiveElements.Remove(element);
        }

        private void Update()
        {
            if (updatesPerSecond <= 0f || WorldTree == null) return;

            tickAccumulator += Time.deltaTime;
            float cycleDuration = 1f / updatesPerSecond;

            float timePerStep = (activeLeaves.Count > 0) ? (cycleDuration / activeLeaves.Count) : cycleDuration;

            if (tickAccumulator >= timePerStep)
            {
                tickAccumulator -= timePerStep;

                if (activeLeaves.Count == 0 || currentLeafIndex >= activeLeaves.Count)
                {
                    BeginNewTickCycle();
                }

                if (activeLeaves.Count > 0)
                {
                    float trueTimePerLeaf = cycleDuration / activeLeaves.Count;
                    ProcessNextLeaf(trueTimePerLeaf);
                }
            }
        }

        private void BeginNewTickCycle()
        {
            TreeSpawnerManager spawner = ServiceLocator.Get<TreeSpawnerManager>();

            WorldTree.ClearElements();
            ResetAllVisuals(WorldTree.RootNode, spawner);

            foreach (var element in ActiveElements)
            {
                AABB objBounds = element.GetWorldAABB();
                ITreeElement objElement = new ObjectElement
                {
                    TreeManager = element,
                    Bounds = objBounds,
                    Center = objBounds.Center
                };
                WorldTree.Insert(objElement);
            }

            WorldTree.PruneEmptyBranches();

            spawner.SyncVisualTree();

            foreach (var element in ActiveElements)
            {
                TreeNode leafNode = WorldTree.GetLeafContaining(element.GetWorldAABB().Center);
                if (leafNode != null)
                {
                    SpatialNodeView view = spawner.GetViewForNode(leafNode);
                    if (view != null) element.transform.SetParent(view.transform, true);
                }
            }

            activeLeaves = WorldTree.GetActiveLeaves();
            currentLeafIndex = 0;
        }

        private void ProcessNextLeaf(float timePerLeaf)
        {
            TreeSpawnerManager spawner = ServiceLocator.Get<TreeSpawnerManager>();
            TreeNode leaf = activeLeaves[currentLeafIndex];
            SpatialNodeView view = spawner.GetViewForNode(leaf);

            if (view != null) view.SetUpdating(timePerLeaf);

            bool collisionFound = ProcessLeafCollisions(leaf);

            if (view != null)
            {
                if (collisionFound) view.SetColliding();
                else view.SetIdle();
            }

            currentLeafIndex++;
        }

        private bool ProcessLeafCollisions(TreeNode leaf)
        {
            bool hasCollision = false;
            var elements = leaf.Elements;

            for (int i = 0; i < elements.Count; i++)
            {
                for (int j = i + 1; j < elements.Count; j++)
                {
                    var objA = (ObjectElement)elements[i];
                    var objB = (ObjectElement)elements[j];

                    var elementA = objA.TreeManager;
                    var elementB = objB.TreeManager;

                    var boundsA = elementA.GetWorldAABB();
                    var boundsB = elementB.GetWorldAABB();

                    float distance = Vector3.Distance(boundsA.Center, boundsB.Center);
                    float radiusA = boundsA.Extents.magnitude * validityRadiusMultiplier;
                    float radiusB = boundsB.Extents.magnitude * validityRadiusMultiplier;

                    if (distance > (radiusA + radiusB)) continue;

                    Debug.Log($"<color=yellow>Radius check</color> {elementA.name} and {elementB.name}");

                    if (!boundsA.Intersects(boundsB)) continue;

                    Debug.Log($"<color=orange>AABB check</color> {elementA.name} and {elementB.name}");

                    if (CheckTriangleTrees(elementA, elementB))
                    {
                        Debug.Log($"<color=red>Triangle check</color> {elementA.name} and {elementB.name}");
                        hasCollision = true;
                    }
                }
            }
            return hasCollision;
        }

        private bool CheckTriangleTrees(MeshTriangleTreeManager objA, MeshTriangleTreeManager objB)
        {
            var boundsA = objA.GetWorldAABB();
            var boundsB = objB.GetWorldAABB();

            AABB searchBoundsInA = objA.ConvertWorldBoundsToLocal(boundsB);
            AABB searchBoundsInB = objB.ConvertWorldBoundsToLocal(boundsA);

            uniqueElementsBufferA.Clear();
            overlappingLeavesBuffer.Clear();

            if (objA.LocalSpaceTree != null && objA.LocalSpaceTree.RootNode != null)
            {
                objA.LocalSpaceTree.RootNode.GetIntersectingNodes(searchBoundsInA, ref overlappingLeavesBuffer);
                foreach (var leaf in overlappingLeavesBuffer)
                {
                    foreach (var element in leaf.Elements) uniqueElementsBufferA.Add(element);
                }
            }

            uniqueElementsBufferB.Clear();
            overlappingLeavesBuffer.Clear();

            if (objB.LocalSpaceTree != null && objB.LocalSpaceTree.RootNode != null)
            {
                objB.LocalSpaceTree.RootNode.GetIntersectingNodes(searchBoundsInB, ref overlappingLeavesBuffer);
                foreach (var leaf in overlappingLeavesBuffer)
                {
                    foreach (var element in leaf.Elements) uniqueElementsBufferB.Add(element);
                }
            }

            foreach (var elementA in uniqueElementsBufferA)
            {
                foreach (var elementB in uniqueElementsBufferB)
                {
                    if (EvaluateTriangleOverlap(elementA, elementB, objA.transform, objB.transform))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool EvaluateTriangleOverlap(ITreeElement elementA, ITreeElement elementB, Transform transformA, Transform transformB)
        {
            if (elementA.Triangles == null || elementB.Triangles == null) return false;

            foreach (var triA in elementA.Triangles)
            {
                Vector3[] worldTriA = new Vector3[] {
                    transformA.TransformPoint(triA.V0),
                    transformA.TransformPoint(triA.V1),
                    transformA.TransformPoint(triA.V2)
                };

                foreach (var triB in elementB.Triangles)
                {
                    Vector3[] worldTriB = new Vector3[] {
                        transformB.TransformPoint(triB.V0),
                        transformB.TransformPoint(triB.V1),
                        transformB.TransformPoint(triB.V2)
                    };

                    if (TrianglesIntersectSAT(worldTriA, worldTriB))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool TrianglesIntersectSAT(Vector3[] triA, Vector3[] triB)
        {
            Vector3 edgeA0 = triA[1] - triA[0];
            Vector3 edgeA1 = triA[2] - triA[1];
            Vector3 edgeA2 = triA[0] - triA[2];

            Vector3 edgeB0 = triB[1] - triB[0];
            Vector3 edgeB1 = triB[2] - triB[1];
            Vector3 edgeB2 = triB[0] - triB[2];

            Vector3[] axes = new Vector3[11];

            axes[0] = Vector3.Cross(edgeA0, edgeA1);
            axes[1] = Vector3.Cross(edgeB0, edgeB1);

            axes[2] = Vector3.Cross(edgeA0, edgeB0);
            axes[3] = Vector3.Cross(edgeA0, edgeB1);
            axes[4] = Vector3.Cross(edgeA0, edgeB2);
            axes[5] = Vector3.Cross(edgeA1, edgeB0);
            axes[6] = Vector3.Cross(edgeA1, edgeB1);
            axes[7] = Vector3.Cross(edgeA1, edgeB2);
            axes[8] = Vector3.Cross(edgeA2, edgeB0);
            axes[9] = Vector3.Cross(edgeA2, edgeB1);
            axes[10] = Vector3.Cross(edgeA2, edgeB2);

            foreach (Vector3 axis in axes)
            {
                if (axis.sqrMagnitude < 0.0001f) continue;

                if (!OverlapOnAxis(triA, triB, axis)) return false;
            }

            return true;
        }

        private bool OverlapOnAxis(Vector3[] triA, Vector3[] triB, Vector3 axis)
        {
            float minA = float.MaxValue, maxA = float.MinValue;

            for (int i = 0; i < 3; i++)
            {
                float projection = Vector3.Dot(triA[i], axis);
                if (projection < minA) minA = projection;
                if (projection > maxA) maxA = projection;
            }

            float minB = float.MaxValue, maxB = float.MinValue;

            for (int i = 0; i < 3; i++)
            {
                float projection = Vector3.Dot(triB[i], axis);
                if (projection < minB) minB = projection;
                if (projection > maxB) maxB = projection;
            }

            return maxA >= minB && maxB >= minA;
        }

        private void ResetAllVisuals(TreeNode node, TreeSpawnerManager spawner)
        {
            if (node == null) return;

            SpatialNodeView view = spawner.GetViewForNode(node);
            if (view != null) view.SetIdle();

            if (node.Children != null)
            {
                foreach (var child in node.Children) ResetAllVisuals(child, spawner);
            }
        }
    }
}