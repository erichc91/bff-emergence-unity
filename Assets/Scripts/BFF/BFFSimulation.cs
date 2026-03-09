using UnityEngine;

// Orchestrates the BFF simulation — creates GPU buffers, dispatches kernels,
// wires the output texture to the display Quad, and renders the entropy HUD.
// Mirrors Lague's Simulation.cs conventions exactly.
public class BFFSimulation : MonoBehaviour
{
    public BFFSettings   settings;
    public ComputeShader compute;

    ComputeBuffer  tapeBuffer;
    RenderTexture  displayTexture;

    int stepKernel, colourKernel;
    int epochCount;

    // Entropy sampling — read back a small slice of tape each N frames
    ComputeBuffer entropyReadback;
    const int     EntropySampleCount = 512;
    const int     EntropyInterval    = 30;   // frames between readbacks
    float         currentEntropy     = 8f;
    GUIStyle      hudStyle;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Start()     => Init();
    void OnDestroy() { tapeBuffer?.Release(); displayTexture?.Release(); entropyReadback?.Release(); }

    void FixedUpdate()
    {
        for (int i = 0; i < settings.stepsPerFrame; i++)
        {
            RunSimulation();
            epochCount++;
        }

        if (epochCount % EntropyInterval == 0)
            SampleEntropy();

        // S key: toggle species identity / instruction category view
        if (Input.GetKeyDown(KeyCode.S))
        {
            settings.displayMode = settings.displayMode == 1 ? 0 : 1;
            string modeName = settings.displayMode == 1 ? "SPECIES IDENTITY" : "INSTRUCTION CATEGORIES";
            Debug.Log($"Display mode: {modeName}");
        }
    }

    // ── Initialisation ────────────────────────────────────────────────────────

    void Init()
    {
        epochCount = 0;

        int cellCount = settings.width * settings.height;
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

        compute.SetInt("width",            settings.width);
        compute.SetInt("height",           settings.height);
        compute.SetInt("tapeSize",         settings.tapeSize);
        compute.SetInt("instructionLimit", settings.instructionLimit);
        compute.SetInt("displayMode",      settings.displayMode);
        compute.SetFloat("mutationRate",   settings.mutationRate);

        compute.SetBuffer(stepKernel,    "TapeData", tapeBuffer);
        compute.SetBuffer(colourKernel,  "TapeData", tapeBuffer);
        compute.SetTexture(colourKernel, "DisplayTexture", displayTexture);

        GetComponentInChildren<MeshRenderer>().material.mainTexture = displayTexture;

        entropyReadback = new ComputeBuffer(EntropySampleCount * settings.tapeSize, sizeof(uint));

        hudStyle = new GUIStyle();
        hudStyle.fontSize  = 18;
        hudStyle.normal.textColor = new Color(0.8f, 0.9f, 1f, 0.85f);
    }

    // ── Per-frame simulation ──────────────────────────────────────────────────

    void RunSimulation()
    {
        compute.SetFloat("time", Time.fixedTime + epochCount * 0.001f);
        compute.SetInt("displayMode", settings.displayMode);

        int gx = Mathf.CeilToInt(settings.width  / 8f);
        int gy = Mathf.CeilToInt(settings.height / 8f);

        compute.Dispatch(stepKernel,   gx, gy, 1);
        compute.Dispatch(colourKernel, gx, gy, 1);
    }

    // ── Entropy HUD ───────────────────────────────────────────────────────────

    void SampleEntropy()
    {
        // Read a random strip of cells from the GPU buffer (CPU-side sample)
        uint[] sample = new uint[EntropySampleCount * settings.tapeSize];
        tapeBuffer.GetData(sample, 0, 0, sample.Length);

        int[] freq = new int[256];
        foreach (uint b in sample) freq[b & 255]++;

        float entropy = 0f;
        float total   = sample.Length;
        for (int i = 0; i < 256; i++)
        {
            if (freq[i] == 0) continue;
            float p = freq[i] / total;
            entropy -= p * Mathf.Log(p, 2f);
        }
        currentEntropy = entropy;
    }

    void OnGUI()
    {
        if (!settings.showHUD) return;
        string modeName = settings.displayMode == 1 ? "species" : "categories";
        GUI.Label(new Rect(12, 10, 400, 30),
            $"epoch  {epochCount:N0}    entropy  {currentEntropy:F2} / 8.00    [{modeName}]  S=toggle",
            hudStyle);
    }
}
