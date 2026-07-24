using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class MainMenuManager : MonoBehaviour
{

    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI spaceToStartText;

    [SerializeField] private RectTransform transitionPanel;
    [SerializeField] private float transitionSpeed = 2f; 
    [SerializeField] private float minSwipeDistance = 100f;

    public float bobSpeed = 3f;
    public float bobAmount = 5f;
    

    private Vector3 initialTextPosition;
    private Vector2 touchStartPosition;
    private bool isTransitioning = false;
    
    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
        TouchSimulation.Enable();
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
        TouchSimulation.Disable();
    }
    
    void Start()
    {

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(AudioManager.Instance.mainMenuMusic, true);
        }

        int savedHighScore = PlayerPrefs.GetInt("HighScore", 0);

        if (highScoreText != null)
        {
            highScoreText.text = "HIGHSCORE: " + savedHighScore.ToString();
        }

        if (spaceToStartText != null)
        {
            
            initialTextPosition = spaceToStartText.transform.localPosition;
        }
    }

    void Update()
    {
        // //New input system touch detection
        // if (Touch.activeTouches.Count > 0)
        // {
        //     Debug.Log("Touch detected in MainMenuManager Update");
        //     Touch currentTouch = Touch.activeTouches[0];

        //     if (currentTouch.phase == UnityEngine.InputSystem.TouchPhase.Began)
        //     {
        //         Debug.Log("Touch began, starting game");
        //         PlayGame();
        //     }
        // }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlayGame();
        }
        //if (isTransitioning) return;
        
        //HandleTouchInput();
        SpaceToStartJuice();
    }

    // private void HandleTouchInput()
    // {
    //     if (Touch.activeTouches.Count > 0)
    //     {
    //         Touch currentTouch = Touch.activeTouches[0];

    //         if (currentTouch.phase == UnityEngine.InputSystem.TouchPhase.Began)
    //         {
    //             touchStartPosition = currentTouch.screenPosition;
    //         }
    //         else if (currentTouch.phase == UnityEngine.InputSystem.TouchPhase.Ended)
    //         {
    //             Vector2 touchEndPosition = currentTouch.screenPosition;
    //             float swipeDistance = touchEndPosition.y - touchStartPosition.y;

    //             if (swipeDistance >= minSwipeDistance)
    //             {
    //                 // Swipe detected
    //                 PlayGame();
    //             }
    //         }
    //     }
    //     // Initial fallback but it does not respond
    //     else if (Pointer.current != null)
    //     {
    //         if (Pointer.current.press.wasPressedThisFrame)
    //         {
    //             touchStartPosition = Pointer.current.position.ReadValue();
    //         }
    //         else if (Pointer.current.press.wasReleasedThisFrame)
    //         {
    //             Vector2 touchEndPosition = Pointer.current.position.ReadValue();
    //             float verticalDistance = touchEndPosition.y - touchStartPosition.y;

    //             if (verticalDistance > minSwipeDistance)
    //             {
    //                 PlayGame();
    //             }
    //         }
    //     }
    // }

    public void PlayGame()
    {

        this.enabled = false;

        StartCoroutine(TransitionToGameScene());
    }

    private System.Collections.IEnumerator TransitionToGameScene()
    {
        isTransitioning = true;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("PondScene");
        asyncLoad.allowSceneActivation = false;

        Vector2 startPosition = new Vector2(0, -Screen.height);
        Vector2 endPosition = Vector2.zero;
        float elapsedTime = 0f;

        if (transitionPanel != null)
        {
            transitionPanel.anchoredPosition = startPosition;

            while (elapsedTime < 1f)
            {
                elapsedTime += Time.deltaTime * transitionSpeed;
                transitionPanel.anchoredPosition = Vector2.Lerp(startPosition, endPosition, elapsedTime);
                yield return null;
            }
            transitionPanel.anchoredPosition = endPosition;
        }

        while(asyncLoad.progress < 0.9f)
        {
            yield return null;
        }
        asyncLoad.allowSceneActivation = true;
    }

    void SpaceToStartJuice()
    {
        if (spaceToStartText == null) return;

        float offset = Mathf.Sin(Time.time * bobSpeed) * bobAmount;
        spaceToStartText.transform.localPosition = new Vector3(
            initialTextPosition.x,
            initialTextPosition.y + offset,
            initialTextPosition.z
        );

    }

    
}
