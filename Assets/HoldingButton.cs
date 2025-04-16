using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoldingButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool isDown = false;
    public void OnPointerDown(PointerEventData eventData)
    {
        isDown = true;  
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDown = false;
    }
}
