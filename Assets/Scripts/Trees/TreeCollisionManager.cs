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
                WorldTree.Insert(new ObjectElement(element, element.GetWorldAABB()));
            }

            WorldTree.PruneEmptyBranches();

            spawner.SyncVisualTree();

            foreach (var element in ActiveElements)
            {
                TreeNode leafNode = WorldTree.GetLeafContaining(element.GetWorldAABB().Center);
                if (leafNode != null)
                {
                    NodeVisualizer view = spawner.GetViewForNode(leafNode);
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
            NodeVisualizer view = spawner.GetViewForNode(leaf);

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
            int count = elements.Count;

            for (int i = 0; i < count; i++)
            {
                var objA = (ObjectElement)elements[i];
                var elementA = objA.TreeManager;
                var boundsA = elementA.GetWorldAABB();
                float radiusA = boundsA.Extents.magnitude * validityRadiusMultiplier;

                for (int j = i + 1; j < count; j++)
                {
                    var objB = (ObjectElement)elements[j];
                    var elementB = objB.TreeManager;
                    var boundsB = elementB.GetWorldAABB();
                    float radiusB = boundsB.Extents.magnitude * validityRadiusMultiplier;

                    if (!PassesRadiusCheck(boundsA, radiusA, boundsB, radiusB)) continue;
                    Debug.Log($"<color=yellow>Radius check</color> {elementA.name} and {elementB.name}");

                    if (!PassesAABBCheck(boundsA, boundsB)) continue;
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

        private bool PassesRadiusCheck(AABB boundsA, float radiusA, AABB boundsB, float radiusB)
        {
            float sqrDistance = (boundsA.Center - boundsB.Center).sqrMagnitude;
            float combinedRadius = radiusA + radiusB;

            return sqrDistance <= (combinedRadius * combinedRadius);
        }

        private bool PassesAABBCheck(AABB boundsA, AABB boundsB)
        {
            return boundsA.Intersects(boundsB);
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
            var triElementA = elementA as TriangleElement;
            var triElementB = elementB as TriangleElement;

            if (triElementA == null || triElementB == null) return false;

            Triangle triA = triElementA.Triangle;
            Triangle triB = triElementB.Triangle;

            Vector3 a0 = transformA.TransformPoint(triA.V0);
            Vector3 a1 = transformA.TransformPoint(triA.V1);
            Vector3 a2 = transformA.TransformPoint(triA.V2);

            Vector3 b0 = transformB.TransformPoint(triB.V0);
            Vector3 b1 = transformB.TransformPoint(triB.V1);
            Vector3 b2 = transformB.TransformPoint(triB.V2);

            if (CheckTriangleAgainstPlane(a0, a1, a2, b0, b1, b2)) return true;
            if (CheckTriangleAgainstPlane(b0, b1, b2, a0, a1, a2)) return true;

            return false;
        }

        private bool CheckTriangleAgainstPlane(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 t0, Vector3 t1, Vector3 t2)
        {
            Vector3 normal = Vector3.Cross(p1 - p0, p2 - p0).normalized;

            float d0 = Vector3.Dot(normal, t0 - p0);
            float d1 = Vector3.Dot(normal, t1 - p0);
            float d2 = Vector3.Dot(normal, t2 - p0);

            if ((d0 > 0 && d1 > 0 && d2 > 0) || (d0 < 0 && d1 < 0 && d2 < 0))
                return false;

            List<Vector3> intersectionPoints = new List<Vector3>();

            if (d0 * d1 < 0) intersectionPoints.Add(Vector3.Lerp(t0, t1, d0 / (d0 - d1)));
            else if (d0 == 0) intersectionPoints.Add(t0);

            if (d1 * d2 < 0) intersectionPoints.Add(Vector3.Lerp(t1, t2, d1 / (d1 - d2)));
            else if (d1 == 0) intersectionPoints.Add(t1);

            if (d2 * d0 < 0) intersectionPoints.Add(Vector3.Lerp(t2, t0, d2 / (d2 - d0)));
            else if (d2 == 0) intersectionPoints.Add(t2);

            float baseArea = GetTriangleArea(p0, p1, p2);
            foreach (var point in intersectionPoints)
            {
                float area1 = GetTriangleArea(p0, p1, point);
                float area2 = GetTriangleArea(p1, p2, point);
                float area3 = GetTriangleArea(p2, p0, point);

                if (Mathf.Abs(baseArea - (area1 + area2 + area3)) <= 0.001f)
                {
                    return true;
                }
            }

            return false;
        }

        private float GetTriangleArea(Vector3 a, Vector3 b, Vector3 c)
        {
            return 0.5f * Vector3.Cross(b - a, c - a).magnitude;
        }

        private void ResetAllVisuals(TreeNode node, TreeSpawnerManager spawner)
        {
            if (node == null) return;

            NodeVisualizer view = spawner.GetViewForNode(node);
            if (view != null) view.SetIdle();

            if (node.Children != null)
            {
                foreach (var child in node.Children) ResetAllVisuals(child, spawner);
            }
        }
    }
}