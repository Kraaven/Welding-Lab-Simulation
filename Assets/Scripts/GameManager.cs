using System;
using System.Collections.Generic;
using Unity.XR.CoreUtils.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //Singleton
    public static GameManager instance;
    
    // References and Settings
    [Header("Settings")]
    public List<ColorEntry> gameManagerColorLib = new();
    public Dictionary<ColorType, Color> colorLibrary = new ();
    public bool CreateAttachTransforms;
    public Transform ClipTransform;
    public float attachThreshold;
    
    
    private void Awake()
    {
        instance = this;
        foreach (var item in gameManagerColorLib)
        {
            colorLibrary.Add(item.colorType, item.color);
        }
    }
    
    //Helper Classes
    public enum ColorType
    {
        GenericInteractable,
        SimpleInteractable,
        PlateInteractable
    }

    [Serializable]
    public struct ColorEntry
    {
        public ColorType colorType;
        public Color color;
    }
}
