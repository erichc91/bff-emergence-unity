using UnityEngine;

// Screen-space bloom post-effect applied to the camera.
// Attach to the Main Camera; the bloom material is auto-created by BFFSceneCreator.
// Mirrors Lague's approach: keep post-processing self-contained and tweakable.
[RequireComponent(typeof(Camera))]
[ExecuteAlways]
public class BloomEffect : MonoBehaviour
{
    public Material bloomMaterial;

    [Range(0f, 1f)]  public float threshold  = 0.4f;
    [Range(0f, 5f)]  public float intensity   = 1.8f;
    [Range(0.5f, 4f)] public float blurSpread = 1.5f;
    [Range(1, 4)]    public int   blurPasses  = 2;

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (bloomMaterial == null) { Graphics.Blit(src, dest); return; }

        bloomMaterial.SetFloat("_Threshold",  threshold);
        bloomMaterial.SetFloat("_Intensity",  intensity);
        bloomMaterial.SetFloat("_BlurSpread", blurSpread);

        // Pass 0: extract bright regions
        var bright = RenderTexture.GetTemporary(src.descriptor);
        Graphics.Blit(src, bright, bloomMaterial, 0);

        // Ping-pong blur passes
        for (int i = 0; i < blurPasses; i++)
        {
            var blurH = RenderTexture.GetTemporary(src.descriptor);
            Graphics.Blit(bright, blurH, bloomMaterial, 1);
            RenderTexture.ReleaseTemporary(bright);

            bright = RenderTexture.GetTemporary(src.descriptor);
            Graphics.Blit(blurH, bright, bloomMaterial, 2);
            RenderTexture.ReleaseTemporary(blurH);
        }

        // Pass 3: composite bloom onto original
        bloomMaterial.SetTexture("_BloomTex", bright);
        Graphics.Blit(src, dest, bloomMaterial, 3);

        RenderTexture.ReleaseTemporary(bright);
    }
}
