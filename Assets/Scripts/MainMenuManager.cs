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

        if (isTransitioning) return;
        

        SpaceToStartJuice();
    }

    // Update is called once per frame
    public void PlayGame()
    {

        this.enabled = false;

        SceneManager.LoadScene("PondScene");
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
