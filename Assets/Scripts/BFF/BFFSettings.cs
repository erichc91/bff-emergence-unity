using UnityEngine;

// All tuneable BFF simulation parameters in one place.
// Create instances via Assets > Create > BFF Settings.
// Swap presets live in the Inspector during Play — changes take effect next frame.
[CreateAssetMenu(menuName = "BFF/Settings")]
public class BFFSettings : ScriptableObject
{
    [Header("Grid")]
    public int width  = 512;
    public int height = 512;

    [Header("Virtual Machine")]
    [Tooltip("Bytes per cell tape — MUST be a power of 2, max 32")]
    public int  tapeSize         = 16;
    [Tooltip("Max BFF cycles per cell interaction")]
    public int  instructionLimit = 64;
    [Tooltip("BFF epochs dispatched per Unity FixedUpdate — raise for faster evolution")]
    public int  stepsPerFrame    = 5;

    [Header("Evolution")]
    [Range(0f, 0.005f)]
    public float mutationRate = 0.00024f;

    [Header("Chemotaxis (Trail Layer)")]
    [Tooltip("How much signal active cells deposit each epoch")]
    [Range(0f, 5f)]  public float trailWeight        = 1.5f;
    [Tooltip("Fraction of trail lost each epoch — higher = shorter memory")]
    [Range(0f, 0.1f)] public float decayRate         = 0.015f;
    [Tooltip("How far trail spreads to neighbours each epoch")]
    [Range(0f, 1f)]  public float diffuseRate        = 0.25f;
    [Tooltip("0 = fully random neighbours  1 = always follow trail")]
    [Range(0f, 1f)]  public float chemotaxisStrength = 0.65f;

    [Header("Display")]
    [Tooltip("Show epoch count and entropy overlay")]
    public bool showHUD = true;

    [Tooltip("0 = instruction categories (type view)  |  1 = species identity (territory view)")]
    public int displayMode = 1;
}
