using UnityEngine;

namespace SpatialPartitioning
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(Renderer))]
    public class MeshTriangleTreeManager : MonoBehaviour
    {
        [Header("Tree Settings")]
        public int maxElementsPerNode = 10;
        public int maxDepth = 5;

        public Tree LocalSpaceTree { get; private set; }

        private Vector3[] cachedVertices;
        private int[] cachedTriangles;
        private MeshFilter meshFilter;
        private Renderer meshRenderer;

        private void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<Renderer>();

            if (meshFilter == null || meshFilter.sharedMesh == null)
            {
                Debug.LogWarning($"MeshTriangleTreeManager on {gameObject.name} requires a MeshFilter with a valid mesh.");
            }

            GenerateLocalTree();
        }

        private void GenerateLocalTree()
        {
            Mesh mesh = meshFilter.sharedMesh;
            cachedVertices = mesh.vertices;
            cachedTriangles = mesh.triangles;

            Bounds localBounds = mesh.bounds;
            AABB treeBounds = new AABB(localBounds.center, localBounds.extents);
            LocalSpaceTree = new Tree(treeBounds, maxElementsPerNode, maxDepth);

            for (int i = 0; i < cachedTriangles.Length; i += 3)
            {
                Vector3 v0 = cachedVertices[cachedTriangles[i]];
                Vector3 v1 = cachedVertices[cachedTriangles[i + 1]];
                Vector3 v2 = cachedVertices[cachedTriangles[i + 2]];

                Vector3 min = Vector3.Min(v0, Vector3.Min(v1, v2));
                Vector3 max = Vector3.Max(v0, Vector3.Max(v1, v2));
                Vector3 center = (min + max) / 2f;
                Vector3 extents = (max - min) / 2f;

                TriangleElement triElement = new TriangleElement(
                    new Triangle(v0, v1, v2),
                    new AABB(center, extents)
                );
                LocalSpaceTree.Insert(triElement);

                LocalSpaceTree.Insert(triElement);
            }

            Debug.Log($"Generated Local Tree for {gameObject.name} with {cachedTriangles.Length / 3} triangles.");
        }

        private void Start()
        {
            TreeCollisionManager collisionManager = ServiceLocator.Get<TreeCollisionManager>();
            if (collisionManager != null) collisionManager.RegisterElement(this);
        }

        private void OnDestroy()
        {
            TreeCollisionManager collisionManager = ServiceLocator.Get<TreeCollisionManager>();
            if (collisionManager != null) collisionManager.UnRegisterElement(this);
        }

        public AABB GetWorldAABB()
        {
            if (meshRenderer == null) return new AABB();

            Bounds unityBounds = meshRenderer.bounds;
            return new AABB(unityBounds.center, unityBounds.extents);
        }

        public AABB ConvertWorldBoundsToLocal(AABB foreignWorldBounds)
        {
            Vector3 positionOffset = transform.position;

            Vector3 localCenter = foreignWorldBounds.Center - positionOffset;

            return new AABB(localCenter, foreignWorldBounds.Extents);
        }
    }
}