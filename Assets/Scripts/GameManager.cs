using System;
using System.Collections.Generic;
using Unity.XR.CoreUtils.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //Singleton
    public static GameManager instance;
    
    // ColorLib
    public List<ColorEntry> gameManagerColorLib = new();
    public Dictionary<ColorType, Color> colorLibrary = new ();
    
    
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
    }

    [Serializable]
    public struct ColorEntry
    {
        public ColorType colorType;
        public Color color;
    }
}
