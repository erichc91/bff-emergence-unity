using UnityEngine;

// Orchestrates the BFF simulation — creates GPU buffers, dispatches kernels,
// wires the output texture to the display Quad.  Mirrors Lague's Simulation.cs.
public class BFFSimulation : MonoBehaviour
{
    public BFFSettings   settings;
    public ComputeShader compute;

    ComputeBuffer  tapeBuffer;
    RenderTexture  displayTexture;

    int stepKernel, colourKernel;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Start()  => Init();
    void OnDestroy() { tapeBuffer?.Release(); displayTexture?.Release(); }

    void FixedUpdate()
    {
        for (int i = 0; i < settings.stepsPerFrame; i++)
            RunSimulation();
    }

    // ── Initialisation ────────────────────────────────────────────────────────

    void Init()
    {
        int cellCount = settings.width * settings.height;

        // One uint per byte — simpler indexing in HLSL than a packed format
        tapeBuffer = new ComputeBuffer(cellCount * settings.tapeSize, sizeof(uint));

        uint[] init = new uint[cellCount * settings.tapeSize];
        var rng = new System.Random();
        for (int i = 0; i < init.Length; i++)
            init[i] = (uint)rng.Next(256);
        tapeBuffer.SetData(init);

        displayTexture = new RenderTexture(settings.width, settings.height, 0,
            RenderTextureFormat.DefaultHDR)
        {
            enableRandomWrite = true,
            filterMode        = FilterMode.Point,
            wrapMode          = TextureWrapMode.Clamp,
        };
        displayTexture.Create();

        stepKernel   = compute.FindKernel("StepEpoch");
        colourKernel = compute.FindKernel("UpdateColourMap");

        // Static uniforms (set once; only time changes per-frame)
        compute.SetInt("width",            settings.width);
        compute.SetInt("height",           settings.height);
        compute.SetInt("tapeSize",         settings.tapeSize);
        compute.SetInt("instructionLimit", settings.instructionLimit);
        compute.SetFloat("mutationRate",   settings.mutationRate);

        compute.SetBuffer(stepKernel,   "TapeData", tapeBuffer);
        compute.SetBuffer(colourKernel, "TapeData", tapeBuffer);
        compute.SetTexture(colourKernel, "DisplayTexture", displayTexture);

        SetColourUniforms();

        // Wire texture to the Quad child so it displays immediately on Play
        GetComponentInChildren<MeshRenderer>().material.mainTexture = displayTexture;
    }

    // ── Per-frame simulation ──────────────────────────────────────────────────

    void RunSimulation()
    {
        compute.SetFloat("time", Time.fixedTime);

        int gx = Mathf.CeilToInt(settings.width  / 8f);
        int gy = Mathf.CeilToInt(settings.height / 8f);

        compute.Dispatch(stepKernel,   gx, gy, 1);
        compute.Dispatch(colourKernel, gx, gy, 1);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    void SetColourUniforms()
    {
        var colours = new Vector4[7]
        {
            settings.nullColour,
            settings.moveColour,
            settings.auxColour,
            settings.mathColour,
            settings.copyColour,
            settings.loopColour,
            settings.dataColour,
        };
        compute.SetVectorArray("categoryColours", colours);
    }
}
