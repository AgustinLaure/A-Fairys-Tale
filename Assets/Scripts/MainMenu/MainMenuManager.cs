using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class MainMenuManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LevelData[] levels;
    [SerializeField] private Button[] levelButtons;
    [SerializeField] private Button controlsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button creditsBackButton;
    [SerializeField] private Button controlsBackButton;
    [SerializeField] private Button muteButton;

    [SerializeField] private CanvasGroup mainMenuCanvasGroup;
    [SerializeField] private CanvasGroup creditsCanvasGroup;
    [SerializeField] private CanvasGroup controlsCanvasGroup;

    [SerializeField] private Image fade;

    [SerializeField] private VideoPlayer creditsVideoPlayer;

    private GameManager gameManager;

    [Header("Config")]
    [SerializeField] private float fadeTime;
    [SerializeField] private float creditsTimeToPlayVideo;

    private WaitForSeconds creditsTimer;

    private bool isMuted = false;

    private Coroutine fadeCoroutine = null;
    private Coroutine creditsCoroutine = null;

    private void Awake()
    {
        creditsTimer = new WaitForSeconds(creditsTimeToPlayVideo);
    }

    private void Start()
    {
        gameManager = ServiceLocator.Instance.GetService<GameManager>();

        for (int i = 0; i < levels.Length; i++)
        {
            LevelData currentLevel = levels[i];

            levelButtons[i].onClick.AddListener(() => OnLevelButtonClicked(currentLevel));
        }

        controlsButton.onClick.AddListener(HandleControlsPressed);
        creditsButton.onClick.AddListener(HandleCreditsPressed);
        muteButton.onClick.AddListener(HandleMutePressed);
        creditsBackButton.onClick.AddListener(HandleCreditsBackPressed);
        controlsBackButton.onClick.AddListener(HandleControlsBackPressed);
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

    private void HandleMutePressed()
    {
        isMuted = !isMuted;

        if (isMuted)
        {
            AkUnitySoundEngine.SetState("Global_Audio_State", "Muted");
        }
        else
        {
            AkUnitySoundEngine.SetState("Global_Audio_State", "Normal");
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
    }
}