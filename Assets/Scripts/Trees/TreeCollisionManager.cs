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

        private void Awake()
        {
            ServiceLocator.Register(this);
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

                    if (!boundsA.Intersects(boundsB)) continue;

                    if (CheckTriangleTrees(elementA, elementB))
                    {
                        Debug.Log($"<color=red>Collision Detected:</color> {elementA.name} and {elementB.name}");
                        hasCollision = true;
                    }
                }
            }
            return hasCollision;
        }

        private bool CheckTriangleTrees(MeshTriangleTreeManager objA, MeshTriangleTreeManager objB)
        {
            return true;
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