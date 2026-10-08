using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Mendeteksi swipe kiri/kanan pada background welcome screen.</summary>
public class WelcomeSwipeArea : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public System.Action onDragStart;
    public System.Action<float> onSwipe;

    Vector2 startPos;

    public void OnBeginDrag(PointerEventData e)
    {
        startPos = e.position;
        onDragStart?.Invoke();
    }

    public void OnDrag(PointerEventData e) { }

    public void OnEndDrag(PointerEventData e)
    {
        onSwipe?.Invoke(e.position.x - startPos.x);
    }
}
