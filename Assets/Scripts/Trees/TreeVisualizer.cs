using System;
using UnityEngine;

namespace SpatialPartitioning
{
    public class TreeVisualizer : MonoBehaviour
    {
        [Header("Target References")]
        public MeshTriangleTreeManager targetTreeManager;
        public TreeCollisionManager targetCollisionManager;

        [Header("Visual Settings")]
        public Color boundsColor = Color.green;
        public Color overlappingQuadrantColor = Color.yellow;
        public Color collisionPointColor = Color.red;
        public bool drawLeafNodesOnly = true;

        private void OnDrawGizmos()
        {
            if (targetTreeManager == null || targetTreeManager.LocalSpaceTree == null) return;
        }

        private void DrawNode(TreeNode node)
        {
            throw new NotImplementedException();
        }
    }
}