// BFFSceneCreator.cs — Editor utility
// Creates the BFF scene, default settings asset, and wires everything up.
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
        // ── New scene ─────────────────────────────────────────────────────────
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ── Camera ────────────────────────────────────────────────────────────
        var camGO = new GameObject("Main Camera");
        var cam   = camGO.AddComponent<Camera>();
        cam.clearFlags       = CameraClearFlags.SolidColor;
        cam.backgroundColor  = Color.black;
        cam.orthographic     = true;
        cam.orthographicSize = 5f;
        cam.transform.position = new Vector3(0, 0, -10);
        camGO.AddComponent<AudioListener>();

        // ── Display Quad ──────────────────────────────────────────────────────
        var quadGO = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quadGO.name = "Display";
        quadGO.transform.localScale = new Vector3(10, 10, 1);
        Object.DestroyImmediate(quadGO.GetComponent<MeshCollider>());

        var mat = new Material(Shader.Find("Unlit/Texture"));
        quadGO.GetComponent<MeshRenderer>().sharedMaterial = mat;
        AssetDatabase.CreateAsset(mat, "Assets/Settings/DisplayMaterial.mat");

        // ── Simulation GameObject ─────────────────────────────────────────────
        var simGO = new GameObject("Simulation");
        quadGO.transform.parent = simGO.transform;

        var sim = simGO.AddComponent<BFFSimulation>();

        // ── Compute shader reference ──────────────────────────────────────────
        var cs = AssetDatabase.LoadAssetAtPath<ComputeShader>("Assets/Shaders/BFFSim.compute");
        if (cs == null)
            Debug.LogWarning("BFFSim.compute not found — assign it manually in the Inspector.");
        else
            sim.compute = cs;

        // ── Default settings asset ────────────────────────────────────────────
        var settingsPath = "Assets/Settings/Default.asset";
        var existing = AssetDatabase.LoadAssetAtPath<BFFSettings>(settingsPath);
        if (existing == null)
        {
            var s = ScriptableObject.CreateInstance<BFFSettings>();
            AssetDatabase.CreateAsset(s, settingsPath);
            AssetDatabase.SaveAssets();
            existing = s;
        }
        sim.settings = existing;

        // ── Save scene ────────────────────────────────────────────────────────
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/BFF.unity");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("BFF scene created. Press Play to run the simulation.");
    }
}
#endif
