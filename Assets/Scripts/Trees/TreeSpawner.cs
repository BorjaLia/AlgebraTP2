using System.Collections.Generic;
using UnityEngine;

namespace SpatialPartitioning
{
    public class TreeSpawnerManager : MonoBehaviour
    {
        [Header("References")]
        public TreeCollisionManager collisionManager;

        [Header("Global Tree Parameters")]
        public int maxElementsPerNode = 4;
        public int maxDepth = 8;

        [Header("Spawning Configuration")]
        public GameObject[] objectPrefabs;
        public int spawnCount = 17;
        public float spawnRadius = 10f;

        private List<MeshTriangleTreeManager> spawnedTrees = new List<MeshTriangleTreeManager>();

        private void Start()
        {
            SpawnTrees();
        }

        private void SpawnTrees()
        {
            if (objectPrefabs == null || objectPrefabs.Length == 0)
            {
                Debug.LogWarning("TreeSpawnerManager: No prefabs assigned to spawn.");
                return;
            }

            for (int i = 0; i < spawnCount; i++)
            {
                GameObject randomPrefab = objectPrefabs[Random.Range(0, objectPrefabs.Length)];
                Vector3 randomPos = transform.position + (Random.insideUnitSphere * spawnRadius);
                GameObject spawnedObj = Instantiate(randomPrefab, randomPos, Quaternion.identity, transform);
                spawnedObj.name = $"SpatialTree_Obj_{i}";

                MeshTriangleTreeManager treeManager = spawnedObj.GetComponent<MeshTriangleTreeManager>();
                if (treeManager == null)
                {
                    treeManager = spawnedObj.AddComponent<MeshTriangleTreeManager>();
                }

                treeManager.maxElementsPerNode = this.maxElementsPerNode;
                treeManager.maxDepth = this.maxDepth;

                treeManager.InitializeTree();

                spawnedTrees.Add(treeManager);

                if (collisionManager != null)
                {
                    if (collisionManager.trackedObjects == null)
                        collisionManager.trackedObjects = new List<MeshTriangleTreeManager>();

                    collisionManager.trackedObjects.Add(treeManager);
                }
            }

            Debug.Log($"Successfully spawned and configured {spawnedTrees.Count} spatial trees.");
        }
    }
}