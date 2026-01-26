using System;
using Oculus.Interaction;
using UnityEngine;

public class Key : MonoBehaviour
{
    private Grabbable m_Grabbable;

    public bool Grabbed { get; set; } = false;

    private void Awake()
    {
        m_Grabbable = GetComponent<Grabbable>();

        m_Grabbable.WhenPointerEventRaised += OnPointerEvent;
    }

    private void OnPointerEvent(PointerEvent pointerEvent)
    {
        Grabbed = pointerEvent.Type switch
        {
            PointerEventType.Select => true,
            PointerEventType.Unselect => false,
            _ => Grabbed
        };
    }
}
