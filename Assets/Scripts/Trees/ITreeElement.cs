using UnityEngine;

namespace SpatialPartitioning
{
    public struct AABB
    {
        public Vector3 Center { get; set; }
        public Vector3 Extents { get; set; }

        public Vector3 Min => Center - Extents;
        public Vector3 Max => Center + Extents;

        public AABB(Vector3 center, Vector3 extents)
        {
            Center = center;
            Extents = extents;
        }

        public bool Intersects(AABB other)
        {
            return (Mathf.Abs(Center.x - other.Center.x) <= (Extents.x + other.Extents.x)) &&
                   (Mathf.Abs(Center.y - other.Center.y) <= (Extents.y + other.Extents.y)) &&
                   (Mathf.Abs(Center.z - other.Center.z) <= (Extents.z + other.Extents.z));
        }

        public bool Contains(Vector3 point)
        {
            return (Mathf.Abs(Center.x - point.x) <= Extents.x) &&
                   (Mathf.Abs(Center.y - point.y) <= Extents.y) &&
                   (Mathf.Abs(Center.z - point.z) <= Extents.z);
        }
    }

    public struct Triangle
    {
        public Vector3 V0, V1, V2;

        public Triangle(Vector3 v0, Vector3 v1, Vector3 v2)
        {
            V0 = v0;
            V1 = v1;
            V2 = v2;
        }
    }

    public interface ITreeElement
    {
        AABB Bounds { get; }
        Vector3 Center { get; }
    }

    public class TriangleElement : ITreeElement
    {
        public AABB Bounds { get; set; }
        public Vector3 Center => Bounds.Center;
        public Triangle Triangle { get; set; }

        public TriangleElement(Triangle triangle, AABB bounds)
        {
            Triangle = triangle;
            Bounds = bounds;
        }
    }

    public class ObjectElement : ITreeElement
    {
        public AABB Bounds { get; set; }
        public Vector3 Center => Bounds.Center;
        public MeshTriangleTreeManager TreeManager { get; set; }

        public ObjectElement(MeshTriangleTreeManager manager, AABB bounds)
        {
            TreeManager = manager;
            Bounds = bounds;
        }
    }
}