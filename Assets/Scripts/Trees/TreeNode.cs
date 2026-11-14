using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialPartitioning
{
    public class TreeNode
    {
        public AABB NodeBounds { get; protected set; }
        public int Depth { get; protected set; }
        public int MaxElements { get; protected set; }
        public int MaxDepth { get; protected set; }
        public bool IsLeaf { get; protected set; }

        public List<ITreeElement> Elements { get; protected set; }
        public TreeNode[] Children { get; protected set; }

        public float CenterToVertexDistance { get; protected set; }
        public bool IsQuadtree { get; protected set; }

        public TreeNode(AABB bounds, int depth, int maxElements, int maxDepth)
        {
            NodeBounds = bounds;
            Depth = depth;
            MaxElements = maxElements;
            MaxDepth = maxDepth;
            IsLeaf = true;
            Elements = new List<ITreeElement>();

            IsQuadtree = (bounds.Extents.x == 0 || bounds.Extents.y == 0 || bounds.Extents.z == 0);
        }

        public void Subdivide() { throw new NotImplementedException(); }

        public bool Insert(ITreeElement element) { throw new NotImplementedException(); }

        public void GetIntersectingNodes(AABB searchBounds, ref List<TreeNode> resultsBuffer) { throw new NotImplementedException(); }
    }
}