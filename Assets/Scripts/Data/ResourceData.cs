using UnityEngine;
using System.Collections;
using System;

[CreateAssetMenu()]
public class ResourceData : NoiseData
{
    public string Name;

    public Color Colour;

    [Range(0f, 1f)]
    public float Threshold;
}
