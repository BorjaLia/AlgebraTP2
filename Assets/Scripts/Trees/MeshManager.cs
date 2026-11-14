using System;
using UnityEngine;

namespace SpatialPartitioning
{
    [RequireComponent(typeof(MeshFilter))]
    public class MeshTriangleTreeManager : MonoBehaviour
    {
        [Header("Tree Settings")]
        public int maxElementsPerNode = 10;
        public int maxDepth = 5;

        public Tree LocalSpaceTree { get; private set; }

        private Vector3[] cachedVertices;
        private int[] cachedTriangles;
        private MeshFilter meshFilter;

        private void Awake() { throw new NotImplementedException(); }

        public void InitializeTree() { throw new NotImplementedException(); }

        public AABB ConvertWorldBoundsToLocal(AABB worldBounds) { throw new NotImplementedException(); }
    }
}