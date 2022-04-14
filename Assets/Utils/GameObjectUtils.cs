using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utils {
    public static partial class Utils {
        public static bool IsNull(GameObject go) {
            return (go == null || go.Equals(null));
        }
    }
    public static class GameObjectExtensions {
        public static Rigidbody findRigidBody(this GameObject go) {
            Rigidbody rb = go.GetComponent<Rigidbody>();
            while (rb == null && go.transform.parent != null) {
                go = go.transform.parent.gameObject;
                rb = go.GetComponent<Rigidbody>();
            }
            return rb;
        }
        
        public static GameObject getRoot(this GameObject go) {
            while (go.transform.parent != null) {
                go = go.transform.parent.gameObject;
            }
            return go;
        }
        public static bool HasParent(this GameObject go, GameObject parent) {
            while (go != null) {
                if (go == parent) return true;
                go = go.transform.parent?.gameObject;
            }
            return false;
        }

        public static string FullName(this GameObject go, GameObject root = null) {
            List<string> names = new List<string>();
            while (go != root && go != null) {
                names.Add(go.name);
                go = go.transform.parent?.gameObject;
            }
            names.Reverse();
            return string.Join('.', names);
        }
    }
}
