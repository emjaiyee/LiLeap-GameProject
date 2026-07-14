using UnityEngine;

public class RotatingPads : MonoBehaviour
{

    public SpriteRenderer padRenderer;
    public Sprite[] lilyPadVariants;

    public float spinSpeed = 100f;

    public Transform padVisual;

    private Vector3 initialScale;
    private Vector3 currentVisualScale;
    private float squeezeSpeed = 10f;

    public float maxLifeTime = 30f;
    private float timeRemaining;
    private bool isSinking = false;

    private FrogController frogRef;

    public GameObject ripplePrefab;
    public float rippleInterval = 1.5f;
    private float rippleTimer;

    public float bobSpeed = 2f;
    public float pulseAmount = 0.04f;
    private float randomOffset;

    public GameObject smallSplashVFXPrefab;

    [Header("Shadow")]
    public Transform padShadow;
    public Vector3 shadowOffset = new Vector3(0.8f, -0.8f, 0f);
    public float shadowMinAlpha = 0.1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rippleTimer = Random.Range(0f, rippleInterval);

        if (padVisual == null) padVisual = transform;

        initialScale = padVisual.localScale;
        currentVisualScale = initialScale;

        timeRemaining = maxLifeTime;

        randomOffset = Random.Range (0f, 100f);

        if (padRenderer != null && lilyPadVariants != null && lilyPadVariants.Length > 0)
        {
            int randomIndex = Random.Range(0, lilyPadVariants.Length);
            padRenderer.sprite = lilyPadVariants[randomIndex];
        }
    
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, 0, spinSpeed * Time.deltaTime);

        rippleTimer -= Time.deltaTime;
        if (rippleTimer <= 0f)
        {
            if (ripplePrefab != null)
            {
                GameObject ripple = Instantiate(ripplePrefab, transform.position, Quaternion.identity);
                
                ripple.transform.localScale = padVisual.localScale * 0.5f;
            }
            rippleTimer = rippleInterval;
        }

        currentVisualScale = UnityEngine.Vector3.Lerp(currentVisualScale, initialScale, squeezeSpeed * Time.deltaTime);

        float lifeRatio = 1f;

        if (isSinking)
        {
            timeRemaining -= Time.deltaTime;
            lifeRatio = Mathf.Clamp01(timeRemaining / maxLifeTime);

            if (timeRemaining <= 0)
            {
                TriggerCollapse();
                return;
            }
        }

        padVisual.localPosition = Vector3.zero;

        float timeSeed = (Time.time * bobSpeed) + randomOffset;

        float scalePulse = Mathf.Sin(timeSeed) * pulseAmount;

        //padVisual.localPosition = new Vector3(0f, yOffset, 0f);

        //float scaleOffset = Mathf.Cos(timeSeed) * breathAmount;

        Vector3 finalScale = currentVisualScale * lifeRatio;

        finalScale.y += scalePulse;
        finalScale.x += scalePulse;

        padVisual.localScale = finalScale;

        if (padShadow != null)
        {

            padShadow.rotation = Quaternion.identity;

            float heightMultiplier = 1f + (scalePulse * 2f);
            
            padShadow.position = transform.position + (shadowOffset * heightMultiplier);

            Vector3 finalShadowScale = finalScale;
            if (!isSinking)
            {
                finalShadowScale.x += scalePulse * 0.5f;
                finalShadowScale.y += scalePulse * 0.5f;
            }
            padShadow.localScale = finalShadowScale;

            SpriteRenderer shadowSprite = padShadow.GetComponent<SpriteRenderer>();
            if (shadowSprite != null)
            {
                Color currentShadowColor = shadowSprite.color;
                currentShadowColor.a = Mathf.Lerp(shadowMinAlpha, 0.4f, lifeRatio);
                shadowSprite.color = currentShadowColor;
            }

            
        }

    }

    public void StartSinking(FrogController frog)
    {
        frogRef = frog;
        isSinking = true;
    }

    public void StopSinking()
    {
        isSinking = false;
    }

    private void TriggerCollapse()
    {
        isSinking = false;

        if (frogRef != null && transform.gameObject == frogRef.transform.parent?.gameObject)
        {
            frogRef.DrownFrog();
        }

        if (smallSplashVFXPrefab != null)
        {
            GameObject splashInstance = Instantiate(smallSplashVFXPrefab, transform.position, Quaternion.identity);

            Destroy(splashInstance, 1.5f);
        }

        Destroy(gameObject);
    }

    public float ImpactSqueeze(float intensity)
    {
        currentVisualScale = initialScale * intensity;
        return intensity;
    }

}
