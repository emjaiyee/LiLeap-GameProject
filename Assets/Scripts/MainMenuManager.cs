using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{

    public TextMeshProUGUI highScoreText;

    public TextMeshProUGUI spaceToStartText;

    public float bobSpeed = 3f;
    public float bobAmount = 5f;
    

    private Vector3 initialTextPosition;
    private RectTransform textRectTransform;
    
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
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlayGame();
        }

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
