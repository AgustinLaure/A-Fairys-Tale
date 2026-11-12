using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class MainMenuManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LevelData[] foldLevels;
    [SerializeField] private LevelData[] levels;
    [SerializeField] private Button[] foldLevelButtons;
    [SerializeField] private Button controlsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button creditsBackButton;
    [SerializeField] private Button controlsBackButton;
    [SerializeField] private Button birdPlayButton;
    [SerializeField] private Button foxPlayButton;
    [SerializeField] private Button birdContinueButton;
    [SerializeField] private Button foxCotinueButton;
    [SerializeField] private Button muteButton;

    [SerializeField] private CanvasGroup mainMenuCanvasGroup;
    [SerializeField] private CanvasGroup creditsCanvasGroup;
    [SerializeField] private CanvasGroup controlsCanvasGroup;
    [SerializeField] private CanvasGroup birdCanvasGroup;
    [SerializeField] private CanvasGroup foxCanvasGroup;

    [SerializeField] private Image fade;

    [SerializeField] private VideoPlayer creditsVideoPlayer;

    private GameManager gameManager;

    [Header("Config")]
    [SerializeField] private float fadeTime;
    [SerializeField] private float creditsTimeToPlayVideo;

    private WaitForSeconds creditsTimer;

    private Coroutine fadeCoroutine = null;
    private Coroutine creditsCoroutine = null;

    private void Awake()
    {
        creditsTimer = new WaitForSeconds(creditsTimeToPlayVideo);
    }

    private void Start()
    {
        for (int i = 0; i < foldLevels.Length; i++)
        {
            LevelData currentLevel = foldLevels[i];

            foldLevelButtons[i].onClick.AddListener(() => OnLevelButtonClicked(currentLevel));
        }

        gameManager = ServiceLocator.Instance.GetService<GameManager>();

        controlsButton.onClick.AddListener(HandleControlsPressed);
        creditsButton.onClick.AddListener(HandleCreditsPressed);
        muteButton.onClick.AddListener(HandleMutePressed);
        creditsBackButton.onClick.AddListener(HandleCreditsBackPressed);
        controlsBackButton.onClick.AddListener(HandleControlsBackPressed);
        birdPlayButton.onClick.AddListener(HandleBirdPlay);
        foxPlayButton.onClick.AddListener(HandleFoxPlay);
        birdContinueButton.onClick.AddListener(HandleBirdContinueButton);
        foxCotinueButton.onClick.AddListener(HandleFoxContinueButton);

        creditsVideoPlayer.Play();
        creditsVideoPlayer.Stop();
    }

    private void HandleBirdPlay()
    {
        if (fadeCoroutine == null)
        {
            StartCoroutine(FadeCoroutine(mainMenuCanvasGroup, birdCanvasGroup));
        }
    }

    private void HandleFoxPlay()
    {
        if (fadeCoroutine == null)
        {
            StartCoroutine(FadeCoroutine(mainMenuCanvasGroup, foxCanvasGroup));
        }
    }

    private void HandleControlsPressed()
    {
        if (fadeCoroutine == null)
        {
            StartCoroutine(FadeCoroutine(mainMenuCanvasGroup, controlsCanvasGroup));
        }
    }

    private void HandleCreditsPressed()
    {
        if (fadeCoroutine == null)
        {
            StartCoroutine(FadeCoroutine(mainMenuCanvasGroup, creditsCanvasGroup));

            if (creditsCoroutine != null)
            {
                StopCoroutine(creditsCoroutine);
            }

            StartCoroutine(CreditsPlayVideoCoroutine());
        }
    }

    private void HandleControlsBackPressed()
    {
        if (fadeCoroutine == null)
        {
            StartCoroutine(FadeCoroutine(controlsCanvasGroup, mainMenuCanvasGroup));
        }
    }

    private void HandleCreditsBackPressed()
    {
        if (fadeCoroutine == null)
        {
            StartCoroutine(FadeCoroutine(creditsCanvasGroup, mainMenuCanvasGroup));
        }
    }

    private void HandleBirdContinueButton()
    {
        OnLevelButtonClicked(levels[0]);
    }

    private void HandleFoxContinueButton()
    {
        OnLevelButtonClicked(levels[1]);
    }

    private void HandleMutePressed()
    {
        gameManager.IsMuted = !gameManager.IsMuted;

        if (gameManager.IsMuted)
        {
            //AkUnitySoundEngine.SetState("Global_Audio_State", "Muted");
        }
        else
        {
            //AkUnitySoundEngine.SetState("Global_Audio_State", "Normal");
        }
    }

    private IEnumerator CreditsPlayVideoCoroutine()
    {
        creditsVideoPlayer.time = 0f;

        yield return creditsTimer;

        creditsVideoPlayer.Play();

        creditsCoroutine = null;
    }

    private IEnumerator FadeCoroutine(CanvasGroup from, CanvasGroup to)
    {
        float t = 0f;

        Color startingColor = fade.color;
        startingColor.a = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / fadeTime;
            startingColor.a = t;
            fade.color = startingColor;

            yield return null;
        }

        fade.color = startingColor;

        UiUtils.SetCanvasActive(from, false);
        UiUtils.SetCanvasActive(to, true);

        t = 1f;

        while (t > 0f)
        {
            t -= Time.deltaTime / fadeTime;
            startingColor.a = t;
            fade.color = startingColor;

            yield return null;
        }

        fade.color = startingColor;

        fadeCoroutine = null;
    }

    private void OnLevelButtonClicked(LevelData clickedLevel)
    {
        gameManager.SetActiveLevelData(clickedLevel);
        gameManager.StartLevel();
    }

    private void OnDestroy()
    {
        controlsButton.onClick.RemoveListener(HandleControlsPressed);
        creditsButton.onClick.RemoveListener(HandleCreditsPressed);
        muteButton.onClick.RemoveListener(HandleMutePressed);
        creditsBackButton.onClick.RemoveListener(HandleCreditsBackPressed);
        controlsBackButton.onClick.RemoveListener(HandleControlsBackPressed);
        birdPlayButton.onClick.RemoveListener(HandleBirdPlay);
        foxPlayButton.onClick.RemoveListener(HandleFoxPlay);
        birdContinueButton.onClick.RemoveListener(HandleBirdContinueButton);
        foxCotinueButton.onClick.RemoveListener(HandleFoxContinueButton);
    }
}