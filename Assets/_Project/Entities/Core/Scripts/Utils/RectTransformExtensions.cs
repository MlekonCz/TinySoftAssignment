using UnityEngine;

namespace Core.Utils
{
    public static class RectTransformExtensions
    {
        public static Rect ToScreenSpaceRect(this RectTransform transform)
        {
            Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
            var position = transform.position;
            Rect rect = new Rect(position.x, Screen.height - position.y, size.x, size.y);
            var pivot = transform.pivot;
            rect.x -= (pivot.x * size.x);
            rect.y -= ((1.0f - pivot.y) * size.y);
            return rect;
        }
    }
}