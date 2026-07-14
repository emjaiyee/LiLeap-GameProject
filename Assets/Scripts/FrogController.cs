using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Drawing;

public class FrogController : MonoBehaviour
{
    public enum FrogState { Spinning, Leaping, Dead };
    public FrogState currentState = FrogState.Spinning;
    public float baseLeapSpeed = 5f;
    private Vector3 leapDirection;
    public Transform targetPad;
    public GameObject LilyPadPrefab;
    private Transform currentPad;
    public float maxJumpDistance = 8f;

    [Header ("Score System")]
    public TextMeshProUGUI scoreText;
    private int currentScore = 0;
    public float regularPopScale = 1.25f;
    public float frenzyPopScale = 1.6f;
    public float popReturnSpeed = 8f;
    public UnityEngine.Color normalScoreColor = UnityEngine.Color.white;
    public UnityEngine.Color frenzyScoreColor = new UnityEngine.Color(1f, 0.85f, 0f);

     public float baseSpinSpeed = 80f;
    public float maxSpinSpeed = 250f;
    public int difficultyCapScore = 10;
    public float basePadScale = 2.5f;
    public float minPadScale = 1.5f;

    public GameObject splashVFXPrefab;

    public float jumpScaleMultiplier = 1.5f;
    private Vector3 baseFrogScale;
    private float totalJumpDistance;

    public GameObject ripplePrefab;


    [Header("Combo System")]
    public float comboWindow = 0.8f;
    private float comboTimer;
    private int currentCombo = 0;
    private bool isTrackingCombo = false;
    public TextMeshProUGUI comboText;

    [Header ("Frenzy Mode")]
    public int frenzyThreshold = 5;
    public TrailRenderer leapTrail;
    public UnityEngine.Color frenzyRippleColor = new UnityEngine.Color(0f, 1f, 0.9f, 0.5f);
    public float frenzySpeedMultiplier = 1.35f;
    public float frenzyHitstopDuration = 0.05f;

    [Header("Shadow System")]
    public Transform frogShadow;
    public Vector3 shadowOffset = new Vector3(0.1f, -0.1f, 0f);
    public float shadowMaxDispersion = 2.5f;

    private Vector3 launchPadPosition;


    // Start is called before the first frame update
    void Start()
    {
        currentPad = transform.parent;
        
        baseFrogScale = new Vector3(0.5f, 0.5f, 1f);
        
        transform.SetParent(currentPad, false);
        transform.localScale = baseFrogScale;
        transform.localPosition = new Vector3(0, 0, -1f);

        if (frogShadow != null) frogShadow.gameObject.SetActive(false);

        SpawnNextPad();
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case FrogState.Spinning:
            HandleSpinningState();
            if (isTrackingCombo)
                {
                    comboTimer -= Time.deltaTime;
                    if (comboTimer <= 0f)
                    {
                        BreakCombo();
                    }
                }
            break;
            case FrogState.Leaping:
            HandleLeapingState();
            break;
            case FrogState.Dead:
            break;
        }

        HandleDynamicShadow();

        if (comboText != null && comboText.transform.localScale != Vector3.one)
        {
            comboText.transform.localScale = Vector3.MoveTowards(comboText.transform.localScale, Vector3.one, 2f * Time.deltaTime);
        }

        if (scoreText != null && scoreText.transform.localScale != Vector3.one)
        {
            scoreText.transform.localScale = Vector3.Lerp (scoreText.transform.localScale, Vector3.one, popReturnSpeed * Time.deltaTime);
        }
    }

    void HandleSpinningState()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            LaunchFrog();
        }
    }

    void LaunchFrog()
    {
        leapDirection = transform.up;

        AudioManager.Instance.PlaySFX(AudioManager.Instance.leapSound);

        if (ripplePrefab != null && targetPad != null)
        {
            Instantiate(ripplePrefab, currentPad.position, Quaternion.identity);
        }

        //JUMPING JUICE
        Vector2 frogPos2D = new Vector2(transform.position.x, transform.position.y);
        Vector2 targetPos2D = new Vector2(targetPad.position.x, targetPad.position.y);
        totalJumpDistance = Vector2.Distance(frogPos2D, targetPos2D);

        //SINKING HOOK
        if (currentPad != null)
        {
            RotatingPads padScript = currentPad.GetComponent<RotatingPads>();
            if (padScript != null)
            {
                padScript.StartSinking(this);
            }
        }

        if (isTrackingCombo && comboTimer > 0f)
        {
            currentCombo++;
            UpdateComboUI();
        }

        if (currentCombo >= frenzyThreshold && leapTrail != null)
        {
            leapTrail.enabled = true;
        }

        isTrackingCombo = false;

        if (currentPad != null)
        {
            launchPadPosition = currentPad.position;
        }
        else
        {
            launchPadPosition = transform.position;
        }

        transform.SetParent(null);
        currentState = FrogState.Leaping;
    }

    void HandleLeapingState()
    {

        float currentLeapSpeed = baseLeapSpeed;

        if (currentCombo >= frenzyThreshold)
        {
            currentLeapSpeed *= frenzySpeedMultiplier;
        }

        transform.position += leapDirection * currentLeapSpeed * Time.deltaTime;

        Vector2 frogPosition2D = new Vector2(transform.position.x, transform.position.y);
        Vector2 targetPadPosition2D = new Vector2(targetPad.position.x, targetPad.position.y);

        float distanceToPad = Vector2.Distance(frogPosition2D, targetPadPosition2D);

        if (totalJumpDistance > 0)
        {
            float progressNormalized = Mathf.Clamp01(distanceToPad / totalJumpDistance);
            float jumpCompletion = 1f - progressNormalized;
            float arcCurve = Mathf.Sin(jumpCompletion * Mathf.PI);

            float currentScaleModifier = Mathf.Lerp(1f, jumpScaleMultiplier, arcCurve);
            transform.localScale = baseFrogScale * targetPad.localScale.x * currentScaleModifier;
        }
        float dynamicLandingRadius = 0.5f * targetPad.localScale.x;

        if (distanceToPad < dynamicLandingRadius)
        {
            LandOnPad();
            return;
        }

        //FAIL STATE CHECK OLD
        // Vector2 currentPadPosition2D = new Vector2(currentPad.position.x, currentPad.position.y);

        //NEW FAIL STATE CHECK
        Vector2 currentPadPosition2D = new Vector2(launchPadPosition.x, launchPadPosition.y);
        float distanceFromLaunchPad = Vector2.Distance(frogPosition2D, currentPadPosition2D);

        if (distanceFromLaunchPad > maxJumpDistance)
        {
            StartCoroutine(HandleDeadState());
        }
    }

    void HandleDynamicShadow()
    {
        if (frogShadow == null) return;

        if (currentState != FrogState.Leaping)
        {
            if (frogShadow.gameObject.activeSelf) frogShadow.gameObject.SetActive(false);
            return;
        }
        else
        {
            if (!frogShadow.gameObject.activeSelf) frogShadow.gameObject.SetActive(true);
        }

        if (frogShadow.parent == transform)
        {
            frogShadow.SetParent(null);
        }

        float angle = Mathf.Atan2(leapDirection.y, leapDirection.x) * Mathf.Rad2Deg;

        frogShadow.rotation = Quaternion.Euler(0, 0, angle - 90f);

        float activePadScale = (targetPad != null) ? targetPad.localScale.x : 1f;
        frogShadow.localScale = baseFrogScale * activePadScale;

        if (totalJumpDistance > 0)
        {
            Vector2 frogPos2D = new Vector2(transform.position.x, transform.position.y);
            Vector2 targetPadPos2D = new Vector2(targetPad.position.x, targetPad.position.y);

            float distanceToPad = Vector2.Distance(frogPos2D, targetPadPos2D);

            float progressNormalized = Mathf.Clamp01(distanceToPad / totalJumpDistance);
            float arcCurve = Mathf.Sin((1f - progressNormalized) * Mathf.PI);

            float dynamicHeightOffset = 1f + (arcCurve * shadowMaxDispersion);

            Vector3 targetShadowPos = transform.position + (shadowOffset * dynamicHeightOffset);

            frogShadow.position = targetShadowPos;
        }
    }

    void LandOnPad()
    {
        currentState = FrogState.Spinning;
        transform.SetParent(targetPad);
        transform.localPosition = new Vector3(0, 0, -1f);

        transform.localScale = baseFrogScale;

        currentPad = targetPad;

        if (ripplePrefab != null)
        {
            GameObject landRipple = Instantiate(ripplePrefab, targetPad.position, Quaternion.identity);
            WaterRipple rippleScript = landRipple.GetComponent<WaterRipple>();

            if (rippleScript != null)
            {
                rippleScript.expansionSpeed = 4f;
                rippleScript.fadeSpeed = 3f;

                if (currentCombo >= frenzyThreshold)
                {
                    SpriteRenderer rippleSprite = landRipple.GetComponentInChildren<SpriteRenderer>();
                    if (rippleSprite != null)
                    {
                        rippleSprite.color = frenzyRippleColor;
                    }
                }
            }

            if (leapTrail != null)
            {
                leapTrail.enabled = false;
                leapTrail.Clear();
            }
        }

        RotatingPads padScript = targetPad.GetComponent<RotatingPads>();
        if (padScript != null)
        {
            padScript.ImpactSqueeze(0.8f);
            padScript.StartSinking(this);

            if (currentCombo >= frenzyThreshold)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.frenzyLandingSound);
            }
            else
            {
                float comboPitchFactor = 1f + (currentCombo * 0.1f);
                AudioManager.Instance.PlaySFXWithPitch(AudioManager.Instance.normalLandingSound, comboPitchFactor);
            }
        }

        CameraFollow camFollow = Camera.main.GetComponent<CameraFollow>();
        if (camFollow != null)
        {
            camFollow.TriggerShake(0.1f, 0.15f, 0.25f);
        }

        //SCORING LOGIC + COMBO
        
        // float difficultyRatio = Mathf.Clamp01((float)currentScore / difficultyCapScore);
        // comboWindow = Mathf.Lerp(1.8f, 1.0f, difficultyRatio);
        
        int scoreGained = 1 + Mathf.Max(0, currentCombo - 1);
        currentScore += scoreGained;

        if (scoreText != null) 
        {
            scoreText.text = currentScore.ToString();

            if (currentCombo >= frenzyThreshold)
            {
                scoreText.transform.localScale = Vector3.one * frenzyPopScale;
                scoreText.color = frenzyScoreColor;
            }
            else if (currentCombo > 0)
            {
                scoreText.transform.localScale = Vector3.one * regularPopScale;
                scoreText.color = normalScoreColor;
            }
        }

        comboTimer = comboWindow;
        isTrackingCombo = true;

        if (currentCombo >= frenzyThreshold)
        {
            StartCoroutine(TriggerHitstop(frenzyHitstopDuration));
        }

        SpawnNextPad();
    }

    System.Collections.IEnumerator TriggerHitstop(float duration)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }

    void SpawnNextPad()
    {

        //PROGRESSION LOGIC
        float difficultyRatio = Mathf.Clamp01((float)currentScore / difficultyCapScore);

        float currentSpinSpeed = Mathf.Lerp(baseSpinSpeed, maxSpinSpeed, difficultyRatio);

        float currentPadScale = Mathf.Lerp(basePadScale, minPadScale, difficultyRatio);
        UnityEngine.Vector3 newPadScale = new UnityEngine.Vector3(currentPadScale, currentPadScale, 1f);

        float minDistance = Mathf.Lerp(3.5f, 4.5f, difficultyRatio);
        float maxDistance = Mathf.Lerp(5.5f, 6.5f, difficultyRatio);
        float randomDistance = Random.Range(minDistance, maxDistance);

        float randomAngle = Random.Range(-45f, 45f) * Mathf.Deg2Rad;

        float offsetX = randomDistance * Mathf.Sin(randomAngle);
        float offsetY = randomDistance * Mathf.Cos(randomAngle);
        Vector3 spawnOffset = new Vector3(offsetX, offsetY, 0);

        Vector3 newPadPosition = currentPad.position + spawnOffset;

        //newPadPosition.z = 0f;

        GameObject newPadObject = Instantiate(LilyPadPrefab, newPadPosition, Quaternion.identity);

        newPadObject.transform.localScale = newPadScale;

        RotatingPads rotatingScript = newPadObject.GetComponent<RotatingPads>();
        if (rotatingScript != null)
        {
            rotatingScript.spinSpeed = currentSpinSpeed;

            if (Random.value < 0.5f)
            {
                rotatingScript.spinSpeed *= -1;
            }
        }

        targetPad = newPadObject.transform;
    }

    public void DrownFrog()
    {
        if (currentState == FrogState.Dead) return;

        StartCoroutine(HandleDeadState());
    }

    public System.Collections.IEnumerator HandleDeadState()
    {
        if (currentState == FrogState.Dead) yield break;
        currentState = FrogState.Dead;

        AudioManager.Instance.PlaySFX(AudioManager.Instance.drownSound);
        
        transform.SetParent(null);
        GetComponent<SpriteRenderer>().enabled = false;

        if (frogShadow != null) 
        {
            Destroy(frogShadow.gameObject);
        }

        HandleGameOverData();

        CameraFollow camFollow = Camera.main.GetComponent<CameraFollow>();
        if (camFollow != null)
        {
            camFollow.TriggerShake(0.25f, 0.2f, 3f);
        }
        
        if (splashVFXPrefab != null)
        {
            Instantiate(splashVFXPrefab, transform.position, Quaternion.identity);
        }
        yield return new WaitForSeconds(0.6f);

        Invoke("ReturnToMainMenu", 1.5f);
        // UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    void BreakCombo()
    {
        if (currentCombo > 0)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.comboBreakSound);
        }

        currentCombo = 0;
        isTrackingCombo = false;
        if (comboText != null) comboText.text = "";

        if (leapTrail != null)
        {
            leapTrail.enabled = false;
            leapTrail.Clear();
        }

        if (scoreText != null)
        {
            scoreText.color = normalScoreColor;
        }
    }

    void UpdateComboUI()
    {
        if (comboText != null && currentCombo > 1)
        {
            comboText.text = currentCombo + "x COMBO!";

            comboText.transform.localScale = Vector3.one * 1.3f;
        } 
    }

    private void HandleGameOverData()
    {
        int currentHighScore = PlayerPrefs.GetInt("HighScore", 0);

        if (currentScore > currentHighScore)
        {
            PlayerPrefs.SetInt("HighScore", currentScore);

            PlayerPrefs.Save();
        }
    }

    private void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
    
}
