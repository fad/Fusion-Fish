using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class VFXManager : MonoBehaviour
{
    [Header("Damage Vignette")]
    [SerializeField]
    private ScriptableRendererFeature damageVignette;
    
    [SerializeField]
    private Material damageVignetteMaterial;

    [SerializeField]
    private float hurtDisplayTime = 1f;
    
    [SerializeField]
    private float hurtFadeOutTime = 1f;

    [SerializeField]
    private float vignetteIntensityStartAmount = 1.6f;
    
    [SerializeField]
    private float vignettePowerStartAmount = 1.75f;

    [SerializeField]
    private float vignettePowerEndAmount = 8f;

    [SerializeField]
    private AnimationCurve fadeOutCurve;
    
    public static VFXManager Instance;
    
    private static int _vignetteIntensity = Shader.PropertyToID("_VignetteIntensity");
    private static int _vignettePower = Shader.PropertyToID("_VignettePower");
    
    private WaitForSeconds _hurtDisplayTime;
    private IEnumerator _currentVignetteCoroutine;
    
    
    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        damageVignette.SetActive(false);
        _hurtDisplayTime = new WaitForSeconds(hurtDisplayTime);
    }
    
    public void ShowHurtVignette()
    {
        if(_currentVignetteCoroutine != null) StopCoroutine(_currentVignetteCoroutine);
        
        _currentVignetteCoroutine = Hurt();
        StartCoroutine(_currentVignetteCoroutine);
    }

    private IEnumerator Hurt()
    {
        damageVignette.SetActive(true);
        damageVignetteMaterial.SetFloat(_vignetteIntensity, vignetteIntensityStartAmount);
        damageVignetteMaterial.SetFloat(_vignettePower, vignettePowerStartAmount);
        
        yield return _hurtDisplayTime;
        
        // Fade out the vignette
        float elapsedTime = 0f;
        float currentIntensity = vignetteIntensityStartAmount;
        float currentPower = vignettePowerStartAmount;
        
        while (elapsedTime < hurtFadeOutTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / hurtFadeOutTime;
            float fadeAmount = fadeOutCurve.Evaluate(t);

            
            currentIntensity = Mathf.Lerp(currentIntensity, 0f, 1-fadeAmount);
            currentPower = Mathf.Lerp(currentPower, vignettePowerEndAmount, 1-fadeAmount);
            
            Debug.Log($"Fade Amount: {fadeAmount}\nElapsed Time: {elapsedTime}\nT: {t}\nCurrent Intensity: {currentIntensity}\nCurrent Power: {currentPower}");
            
            damageVignetteMaterial.SetFloat(_vignetteIntensity, currentIntensity);
            damageVignetteMaterial.SetFloat(_vignettePower, currentPower);
            yield return null;
        }
        
        damageVignetteMaterial.SetFloat(_vignetteIntensity, 0f);
        damageVignetteMaterial.SetFloat(_vignettePower, vignettePowerEndAmount);
        
        damageVignette.SetActive(false);
    }
}
