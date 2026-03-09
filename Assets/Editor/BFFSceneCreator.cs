// BFFSceneCreator.cs — Editor utility
// Creates the BFF scene, bloom material, default + A/B/C/D preset assets.
// Run once: Tools > BFF > Create Scene

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BFFSceneCreator
{
    [MenuItem("Tools/BFF/Create Scene")]
    public static void CreateScene()
    {
        AssetDatabase.CreateFolder("Assets", "Settings");

        // ── Bloom material ────────────────────────────────────────────────────
        var bloomShader = Shader.Find("Hidden/BFFBloom");
        Material bloomMat = null;
        if (bloomShader != null)
        {
            bloomMat = new Material(bloomShader);
            AssetDatabase.CreateAsset(bloomMat, "Assets/Settings/BloomMaterial.mat");
        }
        else
            Debug.LogWarning("Hidden/BFFBloom shader not found — bloom disabled.");

        // ── Presets ───────────────────────────────────────────────────────────
        CreatePreset("A_Exploration",  512,  512, 16, 64,  1,  0.00048f, true);
        CreatePreset("B_FastEvolve",   512,  512, 16, 32, 10,  0.00096f, true);
        CreatePreset("C_Spatial",      512,  512, 16, 96,  3,  0.00012f, true);
        CreatePreset("D_Cinematic",   1024, 1024, 16, 64,  1,  0.00024f, true);
        var defaultSettings = CreatePreset("Default", 512, 512, 16, 64, 5, 0.00024f, true);

        // ── New scene ─────────────────────────────────────────────────────────
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ── Camera + bloom ────────────────────────────────────────────────────
        var camGO = new GameObject("Main Camera");
        var cam   = camGO.AddComponent<Camera>();
        cam.clearFlags      = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.orthographic    = true;
        cam.orthographicSize = 5f;
        cam.transform.position = new Vector3(0, 0, -10);
        cam.allowHDR = true;
        camGO.AddComponent<AudioListener>();

        var bloom = camGO.AddComponent<BloomEffect>();
        if (bloomMat != null) bloom.bloomMaterial = bloomMat;

        // ── Display Quad ──────────────────────────────────────────────────────
        var quadGO = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quadGO.name = "Display";
        quadGO.transform.localScale = new Vector3(10, 10, 1);
        Object.DestroyImmediate(quadGO.GetComponent<MeshCollider>());

        var dispMat = new Material(Shader.Find("Unlit/Texture"));
        quadGO.GetComponent<MeshRenderer>().sharedMaterial = dispMat;
        AssetDatabase.CreateAsset(dispMat, "Assets/Settings/DisplayMaterial.mat");

        // ── Simulation GameObject ─────────────────────────────────────────────
        var simGO = new GameObject("Simulation");
        quadGO.transform.SetParent(simGO.transform);

        var sim = simGO.AddComponent<BFFSimulation>();
        sim.settings = defaultSettings;

        var cs = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/Shaders/BFFSim.compute");
        if (cs == null) Debug.LogWarning("BFFSim.compute not found — assign manually.");
        else            sim.compute = cs;

        // ── Save scene ────────────────────────────────────────────────────────
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/BFF.unity");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("BFF scene created.  Presets A/B/C/D in Assets/Settings/.  Press Play.");
    }

    static BFFSettings CreatePreset(string name, int w, int h, int tape,
                                    int limit, int steps, float mut, bool hud)
    {
        var path = $"Assets/Settings/{name}.asset";
        var existing = AssetDatabase.LoadAssetAtPath<BFFSettings>(path);
        if (existing != null) return existing;

        var s = ScriptableObject.CreateInstance<BFFSettings>();
        s.width            = w;
        s.height           = h;
        s.tapeSize         = tape;
        s.instructionLimit = limit;
        s.stepsPerFrame    = steps;
        s.mutationRate     = mut;
        s.showHUD          = hud;
        AssetDatabase.CreateAsset(s, path);
        return s;
    }
}
#endif
