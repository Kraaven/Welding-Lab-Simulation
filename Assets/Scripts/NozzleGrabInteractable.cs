using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class NozzleGrabInteractable : GrabInteractable
{
    public Transform weldOutput;
    public GameObject nozzleParticles;
    public GameObject WeldingMaterial;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip WeldingAudio;
    public AudioClip WeldingSuccess;
    public AudioClip WeldFailed;

    [Header("Haptics")]
    public float hapticIntensity = 0.5f;
    public float hapticDuration = 0.1f;
    public float hapticInterval = 0.25f;

    private const float weldThreshold = 0.000001f;
    private readonly Collider[] _detectedColliders = new Collider[5];

    private Collider plateCollider1;
    private Collider plateCollider2;

    private Vector3 lastWeldPosition;
    private float accumulatedSqrDistance = 0f;

    private bool WeldActive;
    private bool weldingStarted;
    private bool platesStillDetected;

    private Coroutine weldingCoroutine;
    private Coroutine hapticCoroutine;
    private List<GameObject> weldInstances = new List<GameObject>();
    private Transform clippedPlate;

    LayerMask plateLayerMask;

    protected override void Start()
    {
        base.Start();
        _objectOutline.OutlineColor = GameManager.instance.colorLibrary[GameManager.ColorType.PlateInteractable];
        plateLayerMask = LayerMask.GetMask("plate");
        nozzleParticles.SetActive(false);
    }

    protected override void OnActivated(ActivateEventArgs args)
    {
        base.OnActivated(args);
        WeldActive = true;
        weldingStarted = false;
        accumulatedSqrDistance = 0f;

        int count = Physics.OverlapSphereNonAlloc(weldOutput.position, 0.05f, _detectedColliders, plateLayerMask);
        int validCount = 0;
        bool clipped = false;

        for (int i = 0; i < count && validCount < 2; i++)
        {
            if (_detectedColliders[i] == null) continue;

            if (_detectedColliders[i].TryGetComponent(out MetalPlate plate))
            {
                if (validCount == 0) plateCollider1 = _detectedColliders[i];
                else if (validCount == 1) plateCollider2 = _detectedColliders[i];
                validCount++;
                if (plate.OnClip) clippedPlate = plate.transform;
                clipped = plate.OnClip || clipped;
            }
        }

        if (clipped && validCount == 2)
        {
            weldingStarted = true;
            lastWeldPosition = weldOutput.position;
            Debug.Log("Pair of plates found");
            nozzleParticles.SetActive(true);

            // Start welding audio loop
            if (audioSource && WeldingAudio)
            {
                audioSource.clip = WeldingAudio;
                audioSource.loop = true;
                audioSource.Play();
            }

            // Start haptic feedback
            if (hapticCoroutine == null)
                hapticCoroutine = StartCoroutine(SendHapticFeedback());

            weldingCoroutine = StartCoroutine(SpawnWeldMaterial());
        }
        else
        {
            plateCollider1 = null;
            plateCollider2 = null;
            Debug.Log("No valid target plates found.");
            nozzleParticles.SetActive(false);
        }
    }

    protected override void OnDeactivated(DeactivateEventArgs args)
    {
        base.OnDeactivated(args);

        if (weldingCoroutine != null)
        {
            StopCoroutine(weldingCoroutine);
            weldingCoroutine = null;
        }

        if (hapticCoroutine != null)
        {
            StopCoroutine(hapticCoroutine);
            hapticCoroutine = null;
        }

        // Stop looped welding audio
        if (audioSource && audioSource.isPlaying && audioSource.clip == WeldingAudio)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }

        if (weldingStarted && platesStillDetected && accumulatedSqrDistance >= weldThreshold)
        {
            Debug.Log("Plates Merged");

            var plat1 = plateCollider1.GetComponent<MetalPlate>();
            var plat2 = plateCollider2.GetComponent<MetalPlate>();

            if (plat1.OnClip)
                plat2.BindPlates(plat1);
            else
                plat1.BindPlates(plat2);

            MergeWeldMeshes();

            // Play success audio
            if (audioSource && WeldingSuccess)
                audioSource.PlayOneShot(WeldingSuccess);
        }
        else
        {
            Debug.Log("Did not weld for long enough");

            // Play failure audio
            if (audioSource && WeldFailed)
                audioSource.PlayOneShot(WeldFailed);

            StartCoroutine(DestroyWeldMaterials());
        }

        ResetNozzle();
    }

    public void ResetNozzle()
    {
        plateCollider1 = null;
        plateCollider2 = null;
        WeldActive = false;
        weldingStarted = false;
        accumulatedSqrDistance = 0f;
        nozzleParticles.SetActive(false);
        clippedPlate = null;
    }

    private void Update()
    {
        if (!WeldActive || !weldingStarted) return;

        Vector3 currentPosition = weldOutput.position;
        accumulatedSqrDistance += (currentPosition - lastWeldPosition).sqrMagnitude;
        lastWeldPosition = currentPosition;

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
                break;
            }
        }

        if (!platesStillDetected)
        {
            ResetNozzle();
        }
    }

    private IEnumerator SpawnWeldMaterial()
    {
        while (WeldActive && weldingStarted)
        {
            GameObject weld = Instantiate(WeldingMaterial, weldOutput.position, Random.rotation);
            float scale = Random.Range(0.009f, 0.006f);
            weld.transform.SetParent(clippedPlate);
            weld.transform.localScale = new Vector3(scale, scale, scale);

            weldInstances.Add(weld);
            yield return new WaitForSeconds(0.25f);
        }
    }

    private IEnumerator SendHapticFeedback()
    {
        while (WeldActive && weldingStarted)
        {
            if (interactorsSelecting[0] is UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor controllerInteractor)
            {
                controllerInteractor.SendHapticImpulse(hapticIntensity, hapticDuration);
            }
            yield return new WaitForSeconds(hapticInterval);
        }
    }

    private void MergeWeldMeshes()
    {
        if (weldInstances.Count == 0) return;

        CombineInstance[] combine = new CombineInstance[weldInstances.Count];
        for (int i = 0; i < weldInstances.Count; i++)
        {
            MeshFilter mf = weldInstances[i].GetComponent<MeshFilter>();
            if (mf == null) continue;

            combine[i].mesh = mf.sharedMesh;
            combine[i].transform = mf.transform.localToWorldMatrix;
        }

        GameObject merged = new GameObject("MergedWeld");
        merged.transform.SetParent(clippedPlate);
        merged.transform.position = Vector3.zero;
        merged.transform.rotation = Quaternion.identity;

        MeshFilter mergedFilter = merged.AddComponent<MeshFilter>();
        MeshRenderer mergedRenderer = merged.AddComponent<MeshRenderer>();

        Mesh finalMesh = new Mesh();
        finalMesh.CombineMeshes(combine);
        mergedFilter.mesh = finalMesh;

        mergedRenderer.sharedMaterial = GameManager.instance.ColdWeld;

        foreach (GameObject weld in weldInstances)
        {
            Destroy(weld);
        }

        weldInstances.Clear();
    }

    private IEnumerator DestroyWeldMaterials()
    {
        foreach (GameObject weld in weldInstances)
        {
            Destroy(weld);
            yield return new WaitForSeconds(0.1f);
        }
        weldInstances.Clear();
    }
}
