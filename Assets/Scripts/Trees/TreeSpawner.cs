using System.Collections.Generic;
using UnityEngine;

namespace SpatialPartitioning
{
    public class TreeSpawnerManager : MonoBehaviour
    {
        [Header("Tree Settings")]
        public Vector3 worldSize = new Vector3(100, 100, 100);
        public int maxElementsPerNode = 4;
        public int maxTreeDepth = 5;

        [Header("Visual Prefabs")]
        public SpatialNodeView quadrantPrefab;
        public int maxVisualDepth = 3;

        [Header("Entities")]
        public GameObject[] objectPrefabs;
        public int spawnAmount = 10;

        private Tree macroTree;
        private Dictionary<TreeNode, SpatialNodeView> nodeViewLookup = new Dictionary<TreeNode, SpatialNodeView>();
        private Transform macroTreeRootTransform;

        private void Awake()
        {
            ServiceLocator.Register(this);
        }

        private void Start()
        {
            AABB worldBounds = new AABB(Vector3.zero, worldSize / 2f);
            macroTree = new Tree(worldBounds, maxElementsPerNode, maxTreeDepth);

            GameObject rootObj = new GameObject("MacroTree_Root");
            rootObj.transform.SetParent(this.transform);
            macroTreeRootTransform = rootObj.transform;

            TreeCollisionManager collisionManager = ServiceLocator.Get<TreeCollisionManager>();
            collisionManager.WorldTree = macroTree;

            SpawnEntities(collisionManager);

            SyncVisualTree();

            InitialReparent(collisionManager);
        }

        private void SpawnEntities(TreeCollisionManager collisionManager)
        {
            if (objectPrefabs == null || objectPrefabs.Length == 0) return;

            for (int i = 0; i < spawnAmount; i++)
            {
                GameObject prefab = objectPrefabs[Random.Range(0, objectPrefabs.Length)];
                Vector3 randomPosition = new Vector3(
                    Random.Range(-worldSize.x / 2f, worldSize.x / 2f),
                    Random.Range(-worldSize.y / 2f, worldSize.y / 2f),
                    Random.Range(-worldSize.z / 2f, worldSize.z / 2f)
                );

                GameObject spawnedEntity = Instantiate(prefab, randomPosition, Quaternion.identity);
                spawnedEntity.name = $"Entity_{i}";

                MeshTriangleTreeManager treeManager = spawnedEntity.GetComponent<MeshTriangleTreeManager>();
                if (treeManager != null)
                {
                    collisionManager.RegisterElement(treeManager);

                    AABB objBounds = treeManager.GetWorldAABB();
                    macroTree.Insert(new ObjectElement { TreeManager = treeManager, Bounds = objBounds, Center = objBounds.Center });
                }
            }
        }

        public void SyncVisualTree()
        {
            if (macroTree != null && macroTree.RootNode != null)
            {
                EnsureVisuals(macroTree.RootNode, macroTreeRootTransform, 0);
            }
        }

        private void EnsureVisuals(TreeNode logicalNode, Transform parentTransform, int currentDepth)
        {
            if (currentDepth > maxVisualDepth || logicalNode == null) return;

            if (!nodeViewLookup.ContainsKey(logicalNode))
            {
                SpatialNodeView nodeView = Instantiate(quadrantPrefab, parentTransform);
                nodeView.gameObject.name = $"Quadrant_Depth_{currentDepth}";
                nodeView.Initialize(logicalNode);
                nodeViewLookup.Add(logicalNode, nodeView);
            }

            if (!logicalNode.IsLeaf && logicalNode.Children != null)
            {
                Transform currentViewTransform = nodeViewLookup[logicalNode].transform;
                foreach (var childNode in logicalNode.Children)
                {
                    EnsureVisuals(childNode, currentViewTransform, currentDepth + 1);
                }
            }
        }

        private void InitialReparent(TreeCollisionManager collisionManager)
        {
            foreach (var element in collisionManager.ActiveElements)
            {
                TreeNode leafNode = macroTree.GetLeafContaining(element.GetWorldAABB().Center);
                if (leafNode != null)
                {
                    SpatialNodeView view = GetViewForNode(leafNode);
                    if (view != null) element.transform.SetParent(view.transform,true);
                }
            }
        }

        public SpatialNodeView GetViewForNode(TreeNode node)
        {
            nodeViewLookup.TryGetValue(node, out SpatialNodeView view);
            return view;
        }
    }
}