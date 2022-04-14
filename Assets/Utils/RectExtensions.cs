using UnityEngine;

namespace Utils {
    public static class RectExtensions {
        public static Vector2 GetRelative(this Rect rect, Vector2 p) {
            return Vector2.Scale(
                new Vector2(1f / rect.size.x, 1f / rect.size.y),
                new Vector2(p.x, p.y) - rect.position
            );
        }
    }
}
