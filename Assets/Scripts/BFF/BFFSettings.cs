using UnityEngine;

// All tuneable BFF simulation parameters in one place.
// Create instances via Assets > Create > BFF Settings.
[CreateAssetMenu(menuName = "BFF/Settings")]
public class BFFSettings : ScriptableObject
{
    [Header("Grid")]
    public int width  = 512;
    public int height = 512;

    [Header("Virtual Machine")]
    [Tooltip("Bytes per cell tape — MUST be a power of 2, max 32")]
    public int  tapeSize         = 16;
    [Tooltip("Max BFF cycles per cell interaction per epoch")]
    public int  instructionLimit = 64;
    [Tooltip("BFF epochs dispatched per Unity FixedUpdate")]
    public int  stepsPerFrame    = 1;

    [Header("Evolution")]
    [Range(0f, 0.005f)]
    public float mutationRate = 0.00024f;

    [Header("Display colours — what each dominant instruction looks like")]
    public Color nullColour = Color.black;                                    // 0x00
    public Color moveColour = new Color(0.20f, 0.45f, 1.00f);                // < >
    public Color auxColour  = new Color(0.60f, 0.20f, 1.00f);                // { }
    public Color mathColour = new Color(0.20f, 0.85f, 0.35f);                // + -
    public Color copyColour = new Color(1.00f, 0.70f, 0.10f);                // . ,  replicators
    public Color loopColour = new Color(1.00f, 0.20f, 0.20f);                // [ ]  structure
    public Color dataColour = new Color(0.35f, 0.35f, 0.35f);                // other
}
