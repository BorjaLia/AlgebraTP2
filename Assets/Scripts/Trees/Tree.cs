using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace SpatialPartitioning
{
    public class SpatialTree
    {
        public TreeNode RootNode { get; protected set; }
        public int MaxElementsPerNode { get; protected set; }
        public int MaxDepth { get; protected set; }
        public AABB TreeBounds { get; protected set; }
        public bool IsQuadtree { get; protected set; }

        public SpatialTree(int maxElementsPerNode, int maxDepth)
        {
            MaxElementsPerNode = maxElementsPerNode;
            MaxDepth = maxDepth;
        }

        public void Initialize(AABB bounds)
        {
            TreeBounds = bounds;

            IsQuadtree = (bounds.Extents.x == 0 || bounds.Extents.y == 0 || bounds.Extents.z == 0);

            RootNode = new TreeNode(bounds, 0, MaxElementsPerNode, MaxDepth);
        }

        public bool Insert(ITreeElement element)
        {
            if (RootNode == null)
            {
                Debug.LogWarning("SpatialTree is not initialized. Call Initialize() first.");
                return false;
            }

            if (!RootNode.NodeBounds.Intersects(element.Bounds))
            {
                return false;
            }

            return RootNode.Insert(element);
        }

        public void GetOverlappingNodes(SpatialTree otherTree, ref List<TreeNode> overlappingNodesBuffer)
        {
            if (RootNode == null || otherTree?.RootNode == null) return;

            overlappingNodesBuffer.Clear();

            FindOverlapsRecursive(RootNode, otherTree.RootNode, ref overlappingNodesBuffer);
        }

        private void FindOverlapsRecursive(TreeNode nodeA, TreeNode nodeB, ref List<TreeNode> buffer)
        {
            if (!nodeA.NodeBounds.Intersects(nodeB.NodeBounds)) return;

            if (nodeA.IsLeaf)
            {
                if (!buffer.Contains(nodeA))
                {
                    buffer.Add(nodeA);
                }
            }
            else
            {
                foreach (var childA in nodeA.Children)
                {
                    if (childA == null) continue;

                    if (nodeB.IsLeaf)
                    {
                        FindOverlapsRecursive(childA, nodeB, ref buffer);
                    }
                    else
                    {
                        foreach (var childB in nodeB.Children)
                        {
                            if (childB == null) continue;
                            FindOverlapsRecursive(childA, childB, ref buffer);
                        }
                    }
                }
            }
        }
    }
}