using System.Collections.Generic;
using UnityEngine;

namespace SpatialPartitioning
{
    public class Tree
    {
        public AABB TreeBounds { get; protected set; }
        public int MaxElementsPerNode { get; protected set; }
        public int MaxDepth { get; protected set; }
        public bool IsQuadtree { get; protected set; }
        public TreeNode RootNode { get; protected set; }

        public Tree(AABB bounds, int maxElements, int maxDepth)
        {
            MaxElementsPerNode = maxElements;
            MaxDepth = maxDepth;
            TreeBounds = bounds;

            IsQuadtree = (bounds.Extents.x == 0 || bounds.Extents.y == 0 || bounds.Extents.z == 0);
            RootNode = new TreeNode(bounds, 0, MaxElementsPerNode, MaxDepth);
        }

        public bool Insert(ITreeElement element)
        {
            if (RootNode == null)
            {
                return false;
            }

            if (!RootNode.NodeBounds.Intersects(element.Bounds))
            {
                return false;
            }

            return RootNode.Insert(element);
        }

        public void GetOverlappingNodes(Tree otherTree, ref List<TreeNode> overlappingNodesBuffer)
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

        public void ClearElements()
        {
            if (RootNode != null) RootNode.ClearElementsRecursive();
        }

        public void PruneEmptyBranches()
        {
            if (RootNode != null) RootNode.PruneEmptyBranches();
        }

        public TreeNode GetLeafContaining(Vector3 position)
        {
            if (RootNode != null && RootNode.NodeBounds.Contains(position))
            {
                return RootNode.GetLeafContainingRecursive(position);
            }
            return null;
        }

        public List<TreeNode> GetActiveLeaves()
        {
            List<TreeNode> activeLeaves = new List<TreeNode>();
            if (RootNode != null) RootNode.GetActiveLeavesRecursive(activeLeaves);
            return activeLeaves;
        }
    }
}