using UnityEngine;

// Orchestrates the BFF simulation — creates GPU buffers, dispatches kernels,
// wires the output texture to the display Quad, and renders the entropy HUD.
// Dispatch order each epoch: DiffuseTrail → StepEpoch → DepositTrail → UpdateColourMap
public class BFFSimulation : MonoBehaviour
{
    public BFFSettings   settings;
    public ComputeShader compute;

    ComputeBuffer tapeBuffer;
    RenderTexture displayTexture;
    RenderTexture trailMap;
    RenderTexture diffusedTrailMap;

    int stepKernel, colourKernel, depositKernel, diffuseKernel;
    int epochCount;

    ComputeBuffer entropyReadback;
    const int EntropySampleCount = 512;
    const int EntropyInterval    = 30;
    float     currentEntropy     = 8f;
    GUIStyle  hudStyle;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    void Start()     => Init();
    void OnDestroy() {
        tapeBuffer?.Release();
        displayTexture?.Release();
        trailMap?.Release();
        diffusedTrailMap?.Release();
        entropyReadback?.Release();
    }

    void FixedUpdate()
    {
        for (int i = 0; i < settings.stepsPerFrame; i++)
        {
            RunSimulation();
            epochCount++;
        }

        if (epochCount % EntropyInterval == 0)
            SampleEntropy();

        if (Input.GetKeyDown(KeyCode.S))
        {
            settings.displayMode = settings.displayMode == 1 ? 0 : 1;
            string name = settings.displayMode == 1 ? "SPECIES IDENTITY" : "INSTRUCTION CATEGORIES";
            Debug.Log($"Display mode: {name}");
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

        displayTexture = CreateRT(settings.width, settings.height, RenderTextureFormat.DefaultHDR);
        trailMap       = CreateRT(settings.width, settings.height, RenderTextureFormat.RHalf);
        diffusedTrailMap = CreateRT(settings.width, settings.height, RenderTextureFormat.RHalf);

        stepKernel    = compute.FindKernel("StepEpoch");
        colourKernel  = compute.FindKernel("UpdateColourMap");
        depositKernel = compute.FindKernel("DepositTrail");
        diffuseKernel = compute.FindKernel("DiffuseTrail");

        // Static uniforms
        compute.SetInt("width",            settings.width);
        compute.SetInt("height",           settings.height);
        compute.SetInt("tapeSize",         settings.tapeSize);
        compute.SetInt("instructionLimit", settings.instructionLimit);
        compute.SetInt("displayMode",      settings.displayMode);
        compute.SetFloat("mutationRate",   settings.mutationRate);

        // Buffers — bind to all kernels that need them
        int[] tapeKernels = { stepKernel, colourKernel, depositKernel };
        foreach (var k in tapeKernels)
            compute.SetBuffer(k, "TapeData", tapeBuffer);

        // Trail textures
        compute.SetTexture(diffuseKernel, "TrailMap",         trailMap);
        compute.SetTexture(diffuseKernel, "DiffusedTrailMap", diffusedTrailMap);
        compute.SetTexture(stepKernel,    "DiffusedTrailMap", diffusedTrailMap);
        compute.SetTexture(depositKernel, "TrailMap",         trailMap);
        compute.SetTexture(colourKernel,  "DiffusedTrailMap", diffusedTrailMap);
        compute.SetTexture(colourKernel,  "DisplayTexture",   displayTexture);

        GetComponentInChildren<MeshRenderer>().material.mainTexture = displayTexture;

        entropyReadback = new ComputeBuffer(EntropySampleCount * settings.tapeSize, sizeof(uint));

        hudStyle = new GUIStyle();
        hudStyle.fontSize = 18;
        hudStyle.normal.textColor = new Color(0.8f, 0.9f, 1f, 0.85f);
    }

    // ── Per-frame simulation ──────────────────────────────────────────────────

    void RunSimulation()
    {
        compute.SetFloat("time",               Time.fixedTime + epochCount * 0.001f);
        compute.SetInt("displayMode",          settings.displayMode);
        compute.SetFloat("trailWeight",        settings.trailWeight);
        compute.SetFloat("decayRate",          settings.decayRate);
        compute.SetFloat("diffuseRate",        settings.diffuseRate);
        compute.SetFloat("chemotaxisStrength", settings.chemotaxisStrength);

        int gx = Mathf.CeilToInt(settings.width  / 8f);
        int gy = Mathf.CeilToInt(settings.height / 8f);

        compute.Dispatch(diffuseKernel, gx, gy, 1);  // 1. spread+decay trail
        compute.Dispatch(stepKernel,    gx, gy, 1);  // 2. BFF step (trail-biased)
        compute.Dispatch(depositKernel, gx, gy, 1);  // 3. deposit new trail
        compute.Dispatch(colourKernel,  gx, gy, 1);  // 4. render
    }

    // ── Entropy sampling ──────────────────────────────────────────────────────

    void SampleEntropy()
    {
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
        GUI.Label(new Rect(12, 10, 500, 30),
            $"epoch  {epochCount:N0}    entropy  {currentEntropy:F2} / 8.00    [{modeName}]  S=toggle",
            hudStyle);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static RenderTexture CreateRT(int w, int h, RenderTextureFormat fmt)
    {
        var rt = new RenderTexture(w, h, 0, fmt)
        {
            enableRandomWrite = true,
            filterMode        = FilterMode.Bilinear,
            wrapMode          = TextureWrapMode.Clamp,
        };
        rt.Create();
        return rt;
    }
}
