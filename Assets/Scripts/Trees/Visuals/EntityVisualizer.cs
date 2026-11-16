using UnityEngine;

namespace SpatialPartitioning
{
    [RequireComponent(typeof(MeshTriangleTreeManager))]
    public class EntityVisualizer : MonoBehaviour
    {
        private MeshTriangleTreeManager treeManager;

        [Header("Macro Visualization (World Space)")]
        public Color sphereColor = Color.cyan;
        public Color aabbColor = new Color(0, 1, 1, 0.2f);

        [Header("Micro Visualization (Local Space)")]
        public bool drawMicroTreeBounds = true;
        public Color microTreeColor = new Color(0, 1, 0, 0.1f);

        public bool drawTriangles = true;
        public Color idleTriangleColor = new Color(1, 0, 1, 0.1f);
        public Color collidingTriangleColor = new Color(1, 0, 0, 0.1f);

        private void Awake()
        {
            treeManager = GetComponent<MeshTriangleTreeManager>();
        }

        private void OnDrawGizmosSelected()
        {
            if (treeManager == null) return;

            DrawMacroBounds();

            if (treeManager.LocalSpaceTree != null && treeManager.LocalSpaceTree.RootNode != null)
            {
                Matrix4x4 originalMatrix = Gizmos.matrix;

                Gizmos.matrix = transform.localToWorldMatrix;

                DrawMicroTreeRecursive(treeManager.LocalSpaceTree.RootNode);

                Gizmos.matrix = originalMatrix;
            }
        }

        private void DrawMacroBounds()
        {
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

        private void DrawMicroTreeRecursive(TreeNode node)
        {
            if (node == null) return;

            if (drawMicroTreeBounds)
            {
                Gizmos.color = microTreeColor;
                Gizmos.DrawWireCube(node.NodeBounds.Center, node.NodeBounds.Extents * 2f);
            }

            if (node.IsLeaf && node.Elements != null && drawTriangles)
            {
                foreach (var element in node.Elements)
                {
                    if (element is TriangleElement triElement)
                    {
                        if (treeManager.CollidingMicroElements.Contains(triElement))
                        {
                            Gizmos.color = collidingTriangleColor;
                        }
                        else
                        {
                            Gizmos.color = idleTriangleColor;
                        }

                        Gizmos.DrawLine(triElement.Triangle.V0, triElement.Triangle.V1);
                        Gizmos.DrawLine(triElement.Triangle.V1, triElement.Triangle.V2);
                        Gizmos.DrawLine(triElement.Triangle.V2, triElement.Triangle.V0);
                    }
                }
            }

            if (node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    DrawMicroTreeRecursive(child);
                }
            }
        }
    }
}