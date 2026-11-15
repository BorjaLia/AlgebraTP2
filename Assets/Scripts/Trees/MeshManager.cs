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
    }
}