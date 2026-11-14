using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialPartitioning
{
    public class Tree
    {
        public TreeNode RootNode { get; protected set; }
        public int MaxElementsPerNode { get; protected set; }
        public int MaxDepth { get; protected set; }
        public AABB TreeBounds { get; protected set; }
        public bool IsQuadtree { get; protected set; }

        public void Initialize(AABB bounds) { throw new NotImplementedException(); }

        public bool Insert(ITreeElement element) { throw new NotImplementedException(); }

        public void GetOverlappingNodes(Tree otherTree, ref List<TreeNode> overlappingNodesBuffer) { throw new NotImplementedException(); }
    }
}