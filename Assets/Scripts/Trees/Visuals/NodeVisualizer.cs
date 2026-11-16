using UnityEngine;

namespace SpatialPartitioning
{
    public class NodeVisualizer : MonoBehaviour
    {
        [Header("Visual States")]
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private Material idleMaterial;
        [SerializeField] private Material updatingMaterial;
        [SerializeField] private Material collidingMaterial;

        [SerializeField] private float defaultHighlightDuration = 0.5f;

        [SerializeField] private float minVisualThickness = 0.1f;

        public TreeNode LogicalNode { get; private set; }

        private float highlightTimer = 0f;
        private Material pendingMaterial = null;

        public void Initialize(TreeNode logicalNode)
        {
            LogicalNode = logicalNode;

            if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();
            SetIdle();

            Transform currentParent = transform.parent;
            transform.SetParent(null);

            transform.position = logicalNode.NodeBounds.Center;

            Vector3 logicalScale = logicalNode.NodeBounds.Extents * 2f;

            transform.localScale = new Vector3(
                Mathf.Max(logicalScale.x, minVisualThickness),
                Mathf.Max(logicalScale.y, minVisualThickness),
                Mathf.Max(logicalScale.z, minVisualThickness)
            );

            transform.SetParent(currentParent, true);
        }

        private void Update()
        {
            if (highlightTimer > 0f)
            {
                highlightTimer -= Time.deltaTime;

                if (highlightTimer <= 0f && pendingMaterial != null)
                {
                    meshRenderer.material = pendingMaterial;
                    pendingMaterial = null;
                }
            }
        }
        public void SetIdle()
        {
            if (highlightTimer > 0f) pendingMaterial = idleMaterial;
            else meshRenderer.material = idleMaterial;
        }

        public void SetColliding()
        {
            if (highlightTimer > 0f) pendingMaterial = collidingMaterial;
            else meshRenderer.material = collidingMaterial;
        }

        public void SetUpdating(float customDuration = -1f)
        {
            meshRenderer.material = updatingMaterial;
            highlightTimer = customDuration > 0f ? customDuration : defaultHighlightDuration;
            pendingMaterial = null;
        }
    }
}