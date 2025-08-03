using System;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class NozzleGrabInteractable : GrabInteractable
{
    public Transform weldOutput;

    private const float weldThreshold = 0.000001f;
    private readonly Collider[] _detectedColliders = new Collider[5];

    private Collider plateCollider1;
    private Collider plateCollider2;

    private Vector3 lastWeldPosition;
    private float accumulatedSqrDistance = 0f;

    private bool WeldActive;
    private bool weldingStarted;
    private bool platesStillDetected;
    
    LayerMask plateLayerMask;

    protected override void Start()
    {
        base.Start();
        _objectOutline.OutlineColor = GameManager.instance.colorLibrary[GameManager.ColorType.PlateInteractable];
        plateLayerMask = LayerMask.GetMask("plate");
    }

    protected override void OnActivated(ActivateEventArgs args)
    {
        base.OnActivated(args);
        WeldActive = true;
        weldingStarted = false;
        accumulatedSqrDistance = 0f;

        int count = Physics.OverlapSphereNonAlloc(weldOutput.position, 0.05f, _detectedColliders, plateLayerMask);
        int validCount = 0;

        String.Join(",", _detectedColliders
            .Take(5)
            .Where(c => c != null)
            .Select(c => c.GetComponent<MetalPlate>())
            .Where(mp => mp != null)
            .Distinct()
            .ToList());

        bool clipped = false;

        // Manual search: find first 2 MetalPlate colliders
        for (int i = 0; i < count && validCount < 2; i++)
        {
            if (_detectedColliders[i] == null) continue;
            
            if (_detectedColliders[i].TryGetComponent(out MetalPlate plate))
            {
                if (validCount == 0) plateCollider1 = _detectedColliders[i];
                else if (validCount == 1) plateCollider2 = _detectedColliders[i];
                validCount++;
                clipped = plate.OnClip || clipped;
            }
        }

        print(clipped);
        if (clipped && validCount == 2)
        {
            weldingStarted = true;
            lastWeldPosition = weldOutput.position;
            Debug.Log("Pair of plates found");
        }
        else
        {
            plateCollider1 = null;
            plateCollider2 = null;
            Debug.Log("No valid target plates found.");
        }
    }

    protected override void OnDeactivated(DeactivateEventArgs args)
    {
        base.OnDeactivated(args);

        if (weldingStarted && platesStillDetected && accumulatedSqrDistance >= weldThreshold)
        {
            Debug.Log("Plates Merged");

            var plat1 = plateCollider1.GetComponent<MetalPlate>();
            var plat2 = plateCollider2.GetComponent<MetalPlate>();
            if (plat1.OnClip)
            {
                plat2.BindPlates(plat1);
            }
            else
            {
                plat1.BindPlates(plat2);
            }
        }
        else
        {
            Debug.Log("Did not weld for long enough");
        }

        // Reset
        plateCollider1 = null;
        plateCollider2 = null;
        WeldActive = false;
        weldingStarted = false;
        accumulatedSqrDistance = 0f;
    }

    private void Update()
    {
        if (!WeldActive || !weldingStarted) return;

        // Movement tracking
        Vector3 currentPosition = weldOutput.position;
        accumulatedSqrDistance += (currentPosition - lastWeldPosition).sqrMagnitude;
        lastWeldPosition = currentPosition;

        // Check both plate colliders are still present
        platesStillDetected = false;
        int count = Physics.OverlapSphereNonAlloc(currentPosition, 0.05f, _detectedColliders);

        bool found1 = false, found2 = false;
        for (int i = 0; i < count; i++)
        {
            if (_detectedColliders[i] == null) continue;
            if (!found1 && ReferenceEquals(_detectedColliders[i], plateCollider1)) found1 = true;
            if (!found2 && ReferenceEquals(_detectedColliders[i], plateCollider2)) found2 = true;
            if (found1 && found2)
            {
                platesStillDetected = true;
                print("Plates still detected");
                break;
            }
        }
    }
}
