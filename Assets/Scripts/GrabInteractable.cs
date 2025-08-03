using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Outline))]
public class GrabInteractable : XRGrabInteractable
{
    private Outline _objectOutline;

    private void Start()
    {
        _objectOutline = GetComponent<Outline>();
        _objectOutline.enabled = false;
        _objectOutline.OutlineColor = GameManager.instance.colorLibrary[GameManager.ColorType.GenericInteractable];
    }

    protected override void OnHoverEntered(HoverEnterEventArgs args)
    {
        base.OnHoverEntered(args);
        _objectOutline.enabled = true;
    }

    protected override void OnHoverExited(HoverExitEventArgs args)
    {
        base.OnHoverExited(args);
        _objectOutline.enabled = false;
    }
}
