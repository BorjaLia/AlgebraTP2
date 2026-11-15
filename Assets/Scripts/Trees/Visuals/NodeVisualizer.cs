using UnityEngine;

namespace SpatialPartitioning
{
    public class SpatialNodeView : MonoBehaviour
    {
        [Header("Visual States")]
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private Material idleMaterial;
        [SerializeField] private Material updatingMaterial;
        [SerializeField] private Material collidingMaterial;
        public TreeNode LogicalNode { get; private set; }

        public void Initialize(TreeNode logicalNode)
        {
            LogicalNode = logicalNode;

            if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();
            SetIdle();

            Transform currentParent = transform.parent;
            transform.SetParent(null);

            transform.position = logicalNode.NodeBounds.Center;
            transform.localScale = logicalNode.NodeBounds.Extents * 2f;

            transform.SetParent(currentParent, true);
        }

        public void SetIdle() => meshRenderer.material = idleMaterial;
        public void SetUpdating() => meshRenderer.material = updatingMaterial;
        public void SetColliding() => meshRenderer.material = collidingMaterial;
    }
}