using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialPartitioning
{
    public class TreeCollisionManager : MonoBehaviour
    {
        public List<MeshTriangleTreeManager> trackedObjects;

        private List<TreeNode> overlapBufferA = new List<TreeNode>();
        private List<TreeNode> overlapBufferB = new List<TreeNode>();
        private List<ITreeElement> triangleBufferA = new List<ITreeElement>();
        private List<ITreeElement> triangleBufferB = new List<ITreeElement>();

        private void Update()
        {
        }

        private bool IsPreCheckValid(TreeNode nodeA, TreeNode nodeB) { throw new NotImplementedException(); }

        private bool CheckAABBOverlap(AABB boundsA, AABB boundsB) { throw new NotImplementedException(); }

        private void MapOverlappingQuadrants(Tree treeA, Tree treeB) { throw new NotImplementedException(); }

        private bool CheckNarrowPhaseTriangleCollision(List<ITreeElement> trianglesA, List<ITreeElement> trianglesB) { throw new NotImplementedException(); }

        private void LogCollision(ITreeElement elementA, ITreeElement elementB) { throw new NotImplementedException(); }
    }
}