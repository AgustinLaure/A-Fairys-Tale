using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using UnityEngine.Rendering.Universal;
using Unity.Content;
using Unity.VisualScripting.Dependencies.NCalc;

public class ResultManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image fade;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private CanvasGroup continueCanvasGroup;

    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject loseScreen;

    private GameManager gameManager;
    private EventBus eventBus = null;

    [Header("Configs")]
    [SerializeField] private float fadeTime;
    [SerializeField] private float unFadeTime;
    [SerializeField] private float nonClickableAlpha;

    private const string mainMenuSceneName = "MainMenu";

    private Coroutine fadeCoroutine = null;

    private void Awake()
    {
        mainMenuButton.onClick.AddListener(HandleMainMenuButton);
        restartButton.onClick.AddListener(HandleRestartButton);
        continueButton.onClick.AddListener(HandleContinueButton);
    }
    private void Start()
    {
        ServiceLocator serviceLocator = ServiceLocator.Instance;

        eventBus = serviceLocator.GetService<EventBus>();
        gameManager = serviceLocator.GetService<GameManager>();

        eventBus.Subscribe<OnWin>((Action)HandleWin);
        eventBus.Subscribe<OnLose>((Action)HandleLose);
    }

    private void HandleWin()
    {
        gameObject.SetActive(true);
        winScreen.SetActive(true);

        if ((int)(gameManager.levelData as GameplayLevelData).level >= gameManager.GetMaxLevels - 1)
        {
            continueCanvasGroup.alpha = nonClickableAlpha;
            continueCanvasGroup.blocksRaycasts = false;
            continueCanvasGroup.interactable = false;
        }

        if (fadeCoroutine == null)
        {
            fadeCoroutine = StartCoroutine(FadeCoroutine());
        }
    }

    private void HandleLose()
    {
        gameObject.SetActive(true);
        loseScreen.SetActive(true);

        continueCanvasGroup.alpha = nonClickableAlpha;
        continueCanvasGroup.blocksRaycasts = false;
        continueCanvasGroup.interactable = false;

        if (fadeCoroutine == null)
        {
            fadeCoroutine = StartCoroutine(FadeCoroutine());
        }
    }

    private IEnumerator FadeCoroutine()
    {
        float t = 0f;

        Color newColor = fade.color;

        while (t < 1)
        {
            t += Time.deltaTime / fadeTime;
            newColor.a = t;
            fade.color = newColor;

            yield return null;
        }

        newColor.a = 1;
        fade.color = newColor;

        UiUtils.SetCanvasActive(canvasGroup, true);

        t = 1f;

        while (t > 0f)
        {
            t -= Time.deltaTime / unFadeTime;
            newColor.a = t;
            fade.color = newColor;

            yield return null;
        }

        newColor.a = 0;
        fade.color = newColor;

        fadeCoroutine = null;
    }

    private void HandleMainMenuButton()
    {
        BootStrapper.OnReset();
        SceneManager.LoadSceneAsync(mainMenuSceneName);
    }

    private void HandleRestartButton()
    {
        BootStrapper.OnReset();
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
    }
    private void HandleContinueButton()
    {

    }

    private void OnDestroy()
    {
        mainMenuButton.onClick.RemoveListener(HandleMainMenuButton);
        restartButton.onClick.RemoveListener(HandleRestartButton);
        continueButton.onClick.RemoveListener(HandleContinueButton);

        eventBus.Subscribe<OnWin>((Action)HandleWin);
        eventBus.Subscribe<OnLose>((Action)HandleLose);
    }
}
