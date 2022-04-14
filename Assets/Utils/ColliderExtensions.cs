using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Utils;

namespace Utils {
    public static class ColliderExtensions
    {

        private static List<Vector3> Corners = new List<Vector3>{
            new Vector3(-1,  1,  1),
            new Vector3(-1, -1,  1),
            new Vector3(-1,  1, -1),
            new Vector3(-1, -1, -1),

            new Vector3(1,  1,  1),
            new Vector3(1, -1,  1),
            new Vector3(1,  1, -1),
            new Vector3(1, -1, -1),
        };

        public static bool IsConvex(this Collider c) {
            return c is BoxCollider
                || c is SphereCollider
                || c is CapsuleCollider
                || (bool)(c as MeshCollider)?.convex;
        }


        public static bool Contains(this BoxCollider container, BoxCollider containee) {
            List<float> indices = new List<float>{-1, 0, 1};

            foreach (var corner in Corners) {
                if (!container.Contains(containee.RelativeToWorld(corner)))
                    return false;
            }

            return true;
        }
        public static Vector3 RelativeToWorld(this BoxCollider collider, Vector3 p) {
            p = Vector3.Scale(collider.size, p);
            p += collider.center;
            return collider.transform.localToWorldMatrix * p;
        }

        public static Vector3 WorldToRelative(this BoxCollider collider, Vector3 p) {
            p = collider.transform.worldToLocalMatrix * p;
            p -= collider.center;
            Vector3 invScale = new Vector3(
                1f / collider.size.x,
                1f / collider.size.y,
                1f / collider.size.z
            );
            return Vector3.Scale(invScale, p);
        }

        public static bool Contains(this BoxCollider collider, Vector3 p) {
            p = collider.WorldToRelative(p);
            return Mathf.Abs(p.x) <= 1f
                && Mathf.Abs(p.y) <= 1f
                && Mathf.Abs(p.z) <= 1f;
        }
    }

}