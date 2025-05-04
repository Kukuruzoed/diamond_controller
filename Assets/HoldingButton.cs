using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoldingButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool isDown = false;
    public UnityEvent OnHoldEnded = new UnityEvent();
    public UnityEvent OnHoldStarted = new UnityEvent();
    public void OnPointerDown(PointerEventData eventData)
    {
        isDown = true;
        OnHoldStarted?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDown = false;
        OnHoldEnded?.Invoke();
    }
}
