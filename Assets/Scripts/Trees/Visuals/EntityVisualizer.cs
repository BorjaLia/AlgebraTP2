using UnityEngine;

namespace SpatialPartitioning
{
    [RequireComponent(typeof(MeshTriangleTreeManager))]
    public class EntityVisualizer : MonoBehaviour
    {
        private MeshTriangleTreeManager treeManager;

        public Color sphereColor;
        public Color aabbColor;

        private void Awake()
        {
            treeManager = GetComponent<MeshTriangleTreeManager>();
        }

        private void OnDrawGizmosSelected()
        {
            if (treeManager == null) return;

            AABB worldBounds = treeManager.GetWorldAABB();

            TreeCollisionManager collisionManager = null;
            if (Application.isPlaying)
            {
                collisionManager = ServiceLocator.Get<TreeCollisionManager>();
            }

            float multiplier = collisionManager != null ? collisionManager.validityRadiusMultiplier : 2f;
            float checkRadius = worldBounds.Extents.magnitude * multiplier;
            Gizmos.color = sphereColor;
            Gizmos.DrawWireSphere(worldBounds.Center, checkRadius);

            Gizmos.color = aabbColor;
            Gizmos.DrawWireCube(worldBounds.Center, worldBounds.Extents * 2f);
        }
    }
}