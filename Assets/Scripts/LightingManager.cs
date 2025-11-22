using UnityEngine;

public class LightingManager : MonoBehaviour
{
    [Header("Lighting Settings")]
    [SerializeField] private LightingPreset lightingPreset = LightingPreset.EarlyNight;

    [Header("Ambient Light")]
    [SerializeField] private Color ambientSkyColor = new Color(0.2f, 0.25f, 0.35f);
    [SerializeField] private Color ambientEquatorColor = new Color(0.15f, 0.2f, 0.3f);
    [SerializeField] private Color ambientGroundColor = new Color(0.1f, 0.1f, 0.15f);
    [SerializeField] private float ambientIntensity = 0.6f;

    [Header("Directional Light (Sun/Moon)")]
    [SerializeField] private Light directionalLight;
    [SerializeField] private Color lightColor = new Color(0.8f, 0.7f, 0.9f);
    [SerializeField] private float lightIntensity = 0.4f;
    [SerializeField] private Vector3 lightRotation = new Vector3(45f, -30f, 0f);

    [Header("Fog Settings")]
    [SerializeField] private bool enableFog = true;
    [SerializeField] private Color fogColor = new Color(0.15f, 0.2f, 0.3f);
    [SerializeField] private float fogDensity = 0.02f;
    [SerializeField] private FogMode fogMode = FogMode.ExponentialSquared;

    [Header("Skybox")]
    [SerializeField] private Color skyboxTint = new Color(0.3f, 0.35f, 0.5f);
    [SerializeField] private float skyboxExposure = 0.6f;

    public enum LightingPreset
    {
        Evening,
        EarlyNight,
        Night,
        Custom
    }

    private void Start()
    {
        ApplyLightingPreset();
        ApplyLightingSettings();
    }

    private void ApplyLightingPreset()
    {
        switch (lightingPreset)
        {
            case LightingPreset.Evening:
                // Warm evening colors (golden hour)
                ambientSkyColor = new Color(0.4f, 0.35f, 0.45f);
                ambientEquatorColor = new Color(0.35f, 0.3f, 0.4f);
                ambientGroundColor = new Color(0.2f, 0.15f, 0.2f);
                ambientIntensity = 0.8f;

                lightColor = new Color(1f, 0.85f, 0.7f);
                lightIntensity = 0.6f;
                lightRotation = new Vector3(20f, -45f, 0f);

                fogColor = new Color(0.3f, 0.25f, 0.35f);
                fogDensity = 0.015f;

                skyboxTint = new Color(0.5f, 0.45f, 0.6f);
                skyboxExposure = 0.8f;
                break;

            case LightingPreset.EarlyNight:
                // Cool blue-purple tones
                ambientSkyColor = new Color(0.2f, 0.25f, 0.35f);
                ambientEquatorColor = new Color(0.15f, 0.2f, 0.3f);
                ambientGroundColor = new Color(0.1f, 0.1f, 0.15f);
                ambientIntensity = 0.6f;

                lightColor = new Color(0.8f, 0.7f, 0.9f);
                lightIntensity = 0.4f;
                lightRotation = new Vector3(45f, -30f, 0f);

                fogColor = new Color(0.15f, 0.2f, 0.3f);
                fogDensity = 0.02f;

                skyboxTint = new Color(0.3f, 0.35f, 0.5f);
                skyboxExposure = 0.6f;
                break;

            case LightingPreset.Night:
                // Dark night atmosphere
                ambientSkyColor = new Color(0.1f, 0.15f, 0.25f);
                ambientEquatorColor = new Color(0.08f, 0.12f, 0.2f);
                ambientGroundColor = new Color(0.05f, 0.05f, 0.1f);
                ambientIntensity = 0.4f;

                lightColor = new Color(0.6f, 0.65f, 0.8f);
                lightIntensity = 0.2f;
                lightRotation = new Vector3(60f, -20f, 0f);

                fogColor = new Color(0.08f, 0.12f, 0.2f);
                fogDensity = 0.025f;

                skyboxTint = new Color(0.2f, 0.25f, 0.4f);
                skyboxExposure = 0.4f;
                break;

            case LightingPreset.Custom:
                // Use inspector values
                break;
        }
    }

    private void ApplyLightingSettings()
    {
        // Apply ambient lighting
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = ambientSkyColor;
        RenderSettings.ambientEquatorColor = ambientEquatorColor;
        RenderSettings.ambientGroundColor = ambientGroundColor;
        RenderSettings.ambientIntensity = ambientIntensity;

        // Apply directional light settings
        if (directionalLight == null)
        {
            // Try to find the main directional light
            Light[] lights = FindObjectsOfType<Light>();
            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    directionalLight = light;
                    break;
                }
            }
        }

        if (directionalLight != null)
        {
            directionalLight.color = lightColor;
            directionalLight.intensity = lightIntensity;
            directionalLight.transform.rotation = Quaternion.Euler(lightRotation);

            // Enable shadows for more realism
            directionalLight.shadows = LightShadows.Soft;
            directionalLight.shadowStrength = 0.8f;
        }
        else
        {
            Debug.LogWarning("LightingManager: No directional light found! Create one for better lighting.");
        }

        // Apply fog settings
        RenderSettings.fog = enableFog;
        if (enableFog)
        {
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogMode = fogMode;
            RenderSettings.fogDensity = fogDensity;
        }

        // Apply skybox settings
        if (RenderSettings.skybox != null)
        {
            RenderSettings.skybox.SetColor("_Tint", skyboxTint);
            RenderSettings.skybox.SetFloat("_Exposure", skyboxExposure);
        }

        Debug.Log($"LightingManager: Applied {lightingPreset} lighting preset");
    }

    // Call this if you want to change lighting at runtime
    public void SetLightingPreset(LightingPreset preset)
    {
        lightingPreset = preset;
        ApplyLightingPreset();
        ApplyLightingSettings();
    }

    // Optional: Smooth transition between presets
    public void TransitionToPreset(LightingPreset preset, float duration)
    {
        StartCoroutine(TransitionLightingCoroutine(preset, duration));
    }

    private System.Collections.IEnumerator TransitionLightingCoroutine(LightingPreset targetPreset, float duration)
    {
        // Store current values
        Color startAmbientSky = RenderSettings.ambientSkyColor;
        Color startAmbientEquator = RenderSettings.ambientEquatorColor;
        Color startAmbientGround = RenderSettings.ambientGroundColor;
        float startAmbientIntensity = RenderSettings.ambientIntensity;
        
        Color startLightColor = directionalLight != null ? directionalLight.color : Color.white;
        float startLightIntensity = directionalLight != null ? directionalLight.intensity : 1f;
        
        Color startFogColor = RenderSettings.fogColor;

        // Set target preset
        LightingPreset oldPreset = lightingPreset;
        lightingPreset = targetPreset;
        ApplyLightingPreset();

        // Store target values
        Color targetAmbientSky = ambientSkyColor;
        Color targetAmbientEquator = ambientEquatorColor;
        Color targetAmbientGround = ambientGroundColor;
        float targetAmbientIntensity = ambientIntensity;
        Color targetLightColor = lightColor;
        float targetLightIntensity = lightIntensity;
        Color targetFogColor = fogColor;

        // Restore old preset temporarily
        lightingPreset = oldPreset;
        ApplyLightingPreset();

        // Lerp over time
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            RenderSettings.ambientSkyColor = Color.Lerp(startAmbientSky, targetAmbientSky, t);
            RenderSettings.ambientEquatorColor = Color.Lerp(startAmbientEquator, targetAmbientEquator, t);
            RenderSettings.ambientGroundColor = Color.Lerp(startAmbientGround, targetAmbientGround, t);
            RenderSettings.ambientIntensity = Mathf.Lerp(startAmbientIntensity, targetAmbientIntensity, t);

            if (directionalLight != null)
            {
                directionalLight.color = Color.Lerp(startLightColor, targetLightColor, t);
                directionalLight.intensity = Mathf.Lerp(startLightIntensity, targetLightIntensity, t);
            }

            RenderSettings.fogColor = Color.Lerp(startFogColor, targetFogColor, t);

            yield return null;
        }

        // Apply final preset
        lightingPreset = targetPreset;
        ApplyLightingSettings();
    }

    // Editor helper - apply in edit mode
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            ApplyLightingSettings();
        }
    }
}

