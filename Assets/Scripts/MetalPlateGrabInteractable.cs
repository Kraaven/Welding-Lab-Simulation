using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class MetalPlateGrabInteractable : GrabInteractable
{
    private MetalPlate _metalPlate;
    private Rigidbody _rigidbody;
    protected override void Start()
    {
        base.Start();
        _objectOutline.OutlineColor = GameManager.instance.colorLibrary[GameManager.ColorType.PlateInteractable];
        _metalPlate = GetComponent<MetalPlate>();
        _rigidbody = GetComponent<Rigidbody>();
    }

    protected override void OnSelectExiting(SelectExitEventArgs args)
    {
        base.OnSelectExiting(args);
        _rigidbody.isKinematic = false;
        _metalPlate.OnClip = false;
    }
}
