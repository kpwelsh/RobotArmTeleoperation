using System.Collections.Generic;
using UnityEngine;

namespace Utils {
    public static class MeshRendererExtensions {
        public static void AddMaterial(this MeshRenderer mr, Material mat) {
            var mats = new List<Material>(mr.materials);
            mats.Add(mat);
            mr.materials = mats.ToArray();
        }
        public static bool PopMaterial(this MeshRenderer mr) {
            var mats = new List<Material>(mr.materials);
            if (mats.Count == 0) return false;
            mats.RemoveAt(mats.Count - 1);
            mr.materials = mats.ToArray();
            return true;
        }
    }
}