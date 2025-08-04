using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;


public class MetalPlate : MonoBehaviour
{
    public List<AttachPointCreator> attachedPlatesPoints = new();
    public bool OnClip = false;
    public AudioClip click;

    private Rigidbody rb;
    
    private List<MetalPlate> connectedPlates = new();
    AudioSource audioSource;

    void Start()
    {
        attachedPlatesPoints.Add(GetComponent<AttachPointCreator>());
        rb = GetComponent<Rigidbody>();
        audioSource = this.gameObject.AddComponent<AudioSource>();
    }

    public void SnapToClipPosition(Vector3 clipAttachPosition)
    {
        if (attachedPlatesPoints.Count == 0) return;

        Vector3 closestPlatePoint = Vector3.zero;
        float minSqrDistance = float.MaxValue;

        // Find the closest attach point on this plate to the clip position
        foreach (var creator in attachedPlatesPoints)
        {
            if (creator == null || creator.attachPoints == null) continue;

            foreach (var point in creator.attachPoints)
            {
                Vector3 worldPoint = creator.transform.TransformPoint(point);
                float sqrDist = (worldPoint - clipAttachPosition).sqrMagnitude;
                if (sqrDist < minSqrDistance)
                {
                    minSqrDistance = sqrDist;
                    closestPlatePoint = worldPoint;
                }
            }
        }

        Debug.Log($"Min distance to clip: {minSqrDistance}, threshold: {GameManager.instance.attachThreshold}");
    
        if (minSqrDistance > GameManager.instance.attachThreshold)
        {
            rb.isKinematic = false;
            return;
        }

        // Move the plate so its closest attach point aligns with the clip position
        Vector3 offset = clipAttachPosition - closestPlatePoint;
        transform.position += offset;

        SnapRotationTo90Degrees();
        rb.isKinematic = true;
    
        Debug.Log($"Snapped to clip position with offset: {offset}");
        audioSource.PlayOneShot(click);
    }

    private void SnapRotationTo90Degrees()
    {
        Vector3 euler = transform.rotation.eulerAngles;
        euler.x = Mathf.Round(euler.x / 90f) * 90f;
        euler.y = Mathf.Round(euler.y / 90f) * 90f;
        euler.z = Mathf.Round(euler.z / 90f) * 90f;
        transform.rotation = Quaternion.Euler(euler);
    }

    private void OnCollisionEnter(Collision other)
    {
        Debug.Log($"Collision with: {other.gameObject.name}");

        if (other.gameObject.CompareTag("Clip"))
        {
            rb.isKinematic = true;
            OnClip = true;
            foreach (var plate in connectedPlates) { plate.OnClip = true; }
            SnapToClipPosition(GameManager.instance.ClipTransform.position);
        }
        else if (other.gameObject.CompareTag("MetalPlate"))
        {
            var otherPlate = other.gameObject.GetComponent<MetalPlate>();
            if (otherPlate != null)
            {
                TrySnapToOtherPlate(otherPlate);
            }
        }
    }

    public void TrySnapToOtherPlate(MetalPlate otherPlate)
    {
        if (this.OnClip || otherPlate == null || otherPlate.attachedPlatesPoints.Count == 0)
            return;

        if (!otherPlate.OnClip)
        {
            Debug.Log("Other plate is not clipped, cannot snap");
            return;
        }
        
        Vector3 closestThisPoint = Vector3.zero;
        Vector3 closestOtherPoint = Vector3.zero;
        float minSqrDist = float.MaxValue;
        
        foreach (var thisCreator in attachedPlatesPoints)
        {
            if (thisCreator == null || thisCreator.attachPoints == null) continue;
            
            foreach (var thisPoint in thisCreator.attachPoints)
            {
                Vector3 thisWorld = thisCreator.transform.TransformPoint(thisPoint);

                foreach (var otherCreator in otherPlate.attachedPlatesPoints)
                {
                    if (otherCreator == null || otherCreator.attachPoints == null) continue;
                    
                    foreach (var otherPoint in otherCreator.attachPoints)
                    {
                        Vector3 otherWorld = otherCreator.transform.TransformPoint(otherPoint);
                        float sqrDist = (thisWorld - otherWorld).sqrMagnitude;

                        if (sqrDist < minSqrDist)
                        {
                            minSqrDist = sqrDist;
                            closestThisPoint = thisWorld;
                            closestOtherPoint = otherWorld;
                        }
                    }
                }
            }
        }

        Debug.Log($"Min distance between plates: {minSqrDist}, threshold: {GameManager.instance.attachThreshold}");

        if (minSqrDist < GameManager.instance.attachThreshold)
        {
            // Calculate offset to align attach points
            Vector3 offset = closestOtherPoint - closestThisPoint;
            transform.position += offset;
            
            // Snap rotation
            SnapRotationTo90Degrees();
            
            if (rb != null)
            {
                rb.isKinematic = true;
            }

            Debug.Log($"Plates snapped! Offset applied: {offset}");
            audioSource.PlayOneShot(click);
        }
    }

    public void BindPlates(MetalPlate clippedPlate)
    {
        if (OnClip) return;
        
        TrySnapToOtherPlate(clippedPlate);
        
        transform.SetParent(clippedPlate.transform);
        clippedPlate.attachedPlatesPoints.AddRange(attachedPlatesPoints);
        
        clippedPlate.connectedPlates.Add(this);
        clippedPlate.connectedPlates.AddRange(this.connectedPlates);
        
        GetComponent<Outline>().enabled = false;
        
        var clippedPlateComponent = clippedPlate.GetComponent<MetalPlateGrabInteractable>();
        var selfPlateComponent = this.GetComponent<MetalPlateGrabInteractable>();
        clippedPlateComponent.RefreshColliders();
        clippedPlateComponent.enabled = false;
        clippedPlateComponent.enabled = true;
        
        Destroy(selfPlateComponent);
        Destroy(gameObject.GetComponent<XRGeneralGrabTransformer>());
        Destroy(rb);
        
        GameManager.instance.XRInteractionManager.UnregisterInteractable((IXRInteractable)clippedPlateComponent);
        GameManager.instance.XRInteractionManager.RegisterInteractable((IXRInteractable)clippedPlateComponent);

        var Outline = clippedPlate.GetComponent<Outline>();
        Outline.enabled = false;
        Outline.enabled = true;
        
        
        

        Debug.Log($"Plate {gameObject.name} bound to {clippedPlate.gameObject.name}");
    }

    // Helper method to visualize attach points in Scene view
    private void OnDrawGizmosSelected()
    {
        if (attachedPlatesPoints == null) return;

        Gizmos.color = OnClip ? Color.green : Color.red;
        
        foreach (var creator in attachedPlatesPoints)
        {
            if (creator == null || creator.attachPoints == null) continue;
            
            foreach (var point in creator.attachPoints)
            {
                Vector3 worldPoint = creator.transform.TransformPoint(point);
                Gizmos.DrawWireSphere(worldPoint, 0.01f);
            }
        }
    }
}