using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class GlowingOutlineController : MonoBehaviour
{
    [Header("Outline Settings")]
    [SerializeField] private Color outlineColor = Color.red;
    [SerializeField] private float outlineWidth = 2.0f;
    
    [Header("Glow Settings")]
    [SerializeField] private float glowStrength = 1.0f;
    [SerializeField] private float glowSpeed = 2.0f;
    [SerializeField] private float glowIntensity = 1.0f;
    
    [Header("Rim Lighting")]
    [SerializeField] private Color rimColor = Color.cyan;
    [SerializeField] private float rimStrength = 1.0f;
    [SerializeField] private float fresnelPower = 2.0f;
    
    [Header("Animation")]
    [SerializeField] private bool enablePulsing = true;
    [SerializeField] private bool enableRimLighting = true;
    
    private Renderer objectRenderer;
    private Material material;
    private MaterialPropertyBlock propertyBlock;
    
    // Property IDs for better performance
    private static readonly int OutlineColorID = Shader.PropertyToID("_OutlineColor");
    private static readonly int OutlineWidthID = Shader.PropertyToID("_OutlineWidth");
    private static readonly int GlowStrengthID = Shader.PropertyToID("_GlowStrength");
    private static readonly int GlowSpeedID = Shader.PropertyToID("_GlowSpeed");
    private static readonly int GlowIntensityID = Shader.PropertyToID("_GlowIntensity");
    private static readonly int RimColorID = Shader.PropertyToID("_RimColor");
    private static readonly int RimStrengthID = Shader.PropertyToID("_RimStrength");
    private static readonly int FresnelPowerID = Shader.PropertyToID("_FresnelPower");
    
    void Awake()
    {
        objectRenderer = GetComponent<Renderer>();
        propertyBlock = new MaterialPropertyBlock();
        
        // Create a material instance to avoid affecting other objects
        material = new Material(objectRenderer.material);
        objectRenderer.material = material;
        
        // Apply initial settings
        UpdateShaderProperties();
    }
    
    void Update()
    {
        if (enablePulsing || enableRimLighting)
        {
            UpdateShaderProperties();
        }
    }
    
    void UpdateShaderProperties()
    {
        // Set all shader properties
        material.SetColor(OutlineColorID, outlineColor);
        material.SetFloat(OutlineWidthID, outlineWidth);
        material.SetFloat(GlowStrengthID, enablePulsing ? glowStrength : 0f);
        material.SetFloat(GlowSpeedID, glowSpeed);
        material.SetFloat(GlowIntensityID, glowIntensity);
        material.SetColor(RimColorID, enableRimLighting ? rimColor : Color.clear);
        material.SetFloat(RimStrengthID, rimStrength);
        material.SetFloat(FresnelPowerID, fresnelPower);
    }
    
    // Public methods to control the effect dynamically
    
    /// <summary>
    /// Set the outline color
    /// </summary>
    public void SetOutlineColor(Color color)
    {
        outlineColor = color;
        UpdateShaderProperties();
    }
    
    /// <summary>
    /// Set the outline width
    /// </summary>
    public void SetOutlineWidth(float width)
    {
        outlineWidth = Mathf.Clamp(width, 0f, 10f);
        UpdateShaderProperties();
    }
    
    /// <summary>
    /// Set the glow strength
    /// </summary>
    public void SetGlowStrength(float strength)
    {
        glowStrength = Mathf.Clamp(strength, 0f, 5f);
        UpdateShaderProperties();
    }
    
    /// <summary>
    /// Set the glow speed
    /// </summary>
    public void SetGlowSpeed(float speed)
    {
        glowSpeed = Mathf.Clamp(speed, 0f, 10f);
        UpdateShaderProperties();
    }
    
    /// <summary>
    /// Set the glow intensity
    /// </summary>
    public void SetGlowIntensity(float intensity)
    {
        glowIntensity = Mathf.Clamp(intensity, 0f, 3f);
        UpdateShaderProperties();
    }
    
    /// <summary>
    /// Set the rim color
    /// </summary>
    public void SetRimColor(Color color)
    {
        rimColor = color;
        UpdateShaderProperties();
    }
    
    /// <summary>
    /// Set the rim strength
    /// </summary>
    public void SetRimStrength(float strength)
    {
        rimStrength = Mathf.Clamp(strength, 0f, 3f);
        UpdateShaderProperties();
    }
    
    /// <summary>
    /// Enable or disable pulsing glow
    /// </summary>
    public void SetPulsingEnabled(bool enabled)
    {
        enablePulsing = enabled;
        UpdateShaderProperties();
    }
    
    /// <summary>
    /// Enable or disable rim lighting
    /// </summary>
    public void SetRimLightingEnabled(bool enabled)
    {
        enableRimLighting = enabled;
        UpdateShaderProperties();
    }
    
    /// <summary>
    /// Create a pulsing effect with custom parameters
    /// </summary>
    public void PulseGlow(float duration, float maxStrength)
    {
        StartCoroutine(PulseGlowCoroutine(duration, maxStrength));
    }
    
    private System.Collections.IEnumerator PulseGlowCoroutine(float duration, float maxStrength)
    {
        float originalStrength = glowStrength;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            float currentStrength = Mathf.Lerp(originalStrength, maxStrength, Mathf.Sin(progress * Mathf.PI));
            
            SetGlowStrength(currentStrength);
            yield return null;
        }
        
        SetGlowStrength(originalStrength);
    }
    
    /// <summary>
    /// Create a color transition effect
    /// </summary>
    public void TransitionOutlineColor(Color targetColor, float duration)
    {
        StartCoroutine(TransitionColorCoroutine(targetColor, duration));
    }
    
    private System.Collections.IEnumerator TransitionColorCoroutine(Color targetColor, float duration)
    {
        Color startColor = outlineColor;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            Color currentColor = Color.Lerp(startColor, targetColor, progress);
            
            SetOutlineColor(currentColor);
            yield return null;
        }
        
        SetOutlineColor(targetColor);
    }
    
    void OnDestroy()
    {
        // Clean up the material instance
        if (material != null)
        {
            DestroyImmediate(material);
        }
    }
    
    // Inspector validation
    void OnValidate()
    {
        if (Application.isPlaying && material != null)
        {
            UpdateShaderProperties();
        }
    }
} 