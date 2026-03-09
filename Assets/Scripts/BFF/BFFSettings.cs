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

    [Header("Display")]
    [Tooltip("Show epoch count and entropy overlay")]
    public bool showHUD = true;
}
