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

            CenterToVertexDistance = bounds.Extents.magnitude;
        }

        public void Subdivide()
        {
            IsLeaf = false;

            Vector3 newExtents = NodeBounds.Extents / 2f;

            float[] xDirs = NodeBounds.Extents.x == 0 ? new float[] { 0 } : new float[] { -1, 1 };
            float[] yDirs = NodeBounds.Extents.y == 0 ? new float[] { 0 } : new float[] { -1, 1 };
            float[] zDirs = NodeBounds.Extents.z == 0 ? new float[] { 0 } : new float[] { -1, 1 };

            int childCount = IsQuadtree ? 4 : 8;
            Children = new TreeNode[childCount];
            int index = 0;

            foreach (float x in xDirs)
            {
                foreach (float y in yDirs)
                {
                    foreach (float z in zDirs)
                    {
                        Vector3 childCenter = NodeBounds.Center + new Vector3(x * newExtents.x, y * newExtents.y, z * newExtents.z);
                        Children[index++] = new TreeNode(new AABB(childCenter, newExtents), Depth + 1, MaxElements, MaxDepth);
                    }
                }
            }
        }

        public bool Insert(ITreeElement element)
        {
            if (!NodeBounds.Intersects(element.Bounds))
                return false;

            if (IsLeaf)
            {
                Elements.Add(element);

                if (Elements.Count > MaxElements && Depth < MaxDepth)
                {
                    Subdivide();

                    foreach (var el in Elements)
                    {
                        foreach (var child in Children)
                        {
                            child.Insert(el);
                        }
                    }
                    Elements.Clear();
                }
                return true;
            }
            else
            {
                bool insertedAnywhere = false;
                foreach (var child in Children)
                {
                    if (child.Insert(element))
                    {
                        insertedAnywhere = true;
                    }
                }
                return insertedAnywhere;
            }
        }

        public void GetIntersectingNodes(AABB searchBounds, ref List<TreeNode> resultsBuffer)
        {
            if (!NodeBounds.Intersects(searchBounds)) return;

            if (IsLeaf)
            {
                resultsBuffer.Add(this);
            }
            else
            {
                foreach (var child in Children)
                {
                    child.GetIntersectingNodes(searchBounds, ref resultsBuffer);
                }
            }
        }

        public void ClearElementsRecursive()
        {
            if (IsLeaf)
            {
                Elements?.Clear();
            }
            else if (Children != null)
            {
                for (int i = 0; i < Children.Length; i++)
                {
                    Children[i].ClearElementsRecursive();
                }
            }
        }

        public bool PruneEmptyBranches()
        {
            if (IsLeaf)
            {
                return Elements == null || Elements.Count == 0;
            }

            bool allChildrenEmpty = true;
            if (Children != null)
            {
                for (int i = 0; i < Children.Length; i++)
                {
                    bool isChildEmpty = Children[i].PruneEmptyBranches();
                    if (!isChildEmpty) allChildrenEmpty = false;
                }

                if (allChildrenEmpty)
                {
                    Children = null;
                    IsLeaf = true;
                    if (Elements == null) Elements = new List<ITreeElement>();
                    return true;
                }
            }
            return false;
        }

        public TreeNode GetLeafContainingRecursive(Vector3 position)
        {
            if (IsLeaf) return this;

            if (Children != null)
            {
                foreach (var child in Children)
                {
                    if (child.NodeBounds.Contains(position))
                    {
                        return child.GetLeafContainingRecursive(position);
                    }
                }
            }
            return null;
        }

        public void GetActiveLeavesRecursive(List<TreeNode> resultList)
        {
            if (IsLeaf)
            {
                if (Elements != null && Elements.Count > 0)
                {
                    resultList.Add(this);
                }
            }
            else if (Children != null)
            {
                foreach (var child in Children)
                {
                    child.GetActiveLeavesRecursive(resultList);
                }
            }
        }
    }
}