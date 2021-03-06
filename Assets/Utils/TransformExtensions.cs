using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Utils {
    public static class TransformExtensions {
        public static Matrix4x4 localToWorldUnscaled(this Transform trans) {
            return Matrix4x4.TRS(trans.position, trans.rotation, Vector3.one);
        }

        public static IEnumerable<Transform> AllChildren(this Transform trans) {
            yield return trans;
            foreach (Transform child in trans) {
                foreach (Transform c in child.AllChildren()) {
                    yield return c;
                }
            }
        }

        public static bool neq(this Transform a, Transform b, float positionThreshold, float rotationThreshold) {
            return ((Vector3)b.position - a.position).magnitude >= positionThreshold
                || Quaternion.Angle(b.rotation, a.rotation) >= rotationThreshold;
        }
    }
}