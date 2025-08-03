using System.Collections.Generic;
using UnityEngine;

public class AttachPointCreator : MonoBehaviour
{
    public int height;
    public int width;

    public float unitDivisionSize = 1f;
    public List<Vector3> attachPoints = new();

    public readonly float scaleFactor = 0.0005f;

    void Start()
    {
        HashSet<Vector3> uniquePoints = new();
        attachPoints.Clear();

        int horizontalDivs = Mathf.Max(1, Mathf.RoundToInt((2f * width) / unitDivisionSize));
        int verticalDivs = Mathf.Max(1, Mathf.RoundToInt((2f * height) / unitDivisionSize));

        for (int i = 0; i <= horizontalDivs; i++)
        {
            float t = i / (float)horizontalDivs;

            Vector3 top = new Vector3(Mathf.Lerp(-width, width, t), height, 0) * scaleFactor;
            Vector3 bottom = new Vector3(Mathf.Lerp(-width, width, t), -height, 0) * scaleFactor;

            TryAddUnique(top, uniquePoints);
            TryAddUnique(bottom, uniquePoints);
        }

        for (int i = 0; i <= verticalDivs; i++)
        {
            float t = i / (float)verticalDivs;

            Vector3 left = new Vector3(-width, Mathf.Lerp(-height, height, t), 0) * scaleFactor;
            Vector3 right = new Vector3(width, Mathf.Lerp(-height, height, t), 0) * scaleFactor;

            TryAddUnique(left, uniquePoints);
            TryAddUnique(right, uniquePoints);
        }

        attachPoints.AddRange(uniquePoints);
    }

    void TryAddUnique(Vector3 point, HashSet<Vector3> pointSet)
    {
        if (pointSet.Add(point) && GameManager.instance.CreateAttachTransforms)
        {
            CreateEmptyMarker(point);
        }
    }

    void CreateEmptyMarker(Vector3 localPosition)
    {
        GameObject marker = new GameObject("AttachPoint");
        marker.transform.parent = this.transform;
        marker.transform.localPosition = localPosition;
    }

    void Update()
    {
        // No updates required
    }
}
