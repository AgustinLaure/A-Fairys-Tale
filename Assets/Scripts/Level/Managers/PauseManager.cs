using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class PauseManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup controlsCanvasGroup;
    [SerializeField] private CanvasGroup pauseCanvasGroup;
    [SerializeField] private Image fade;

    [Header("Config")]
    [SerializeField] private float fadeTime;

    private Coroutine fadeCoroutine = null;

    private LevelManager levelManager;
    private GameManager gameManager;

    void Start()
    {
        levelManager = ServiceLocator.Instance.GetService<GameManager>().levelManager;
        gameManager = ServiceLocator.Instance.GetService<GameManager>();
    }

    public void OnResume()
    {
        levelManager.TogglePause();
    }

    public void OnToggleSound()
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

    public void OnControls()
    {
        if (fadeCoroutine == null)
        {
            StartCoroutine(FadeCoroutine(pauseCanvasGroup,controlsCanvasGroup));
        }
    }

    public void OnControlsBack()
    {
        if (fadeCoroutine == null)
        {
            StartCoroutine(FadeCoroutine(controlsCanvasGroup, pauseCanvasGroup));
        }
    }

    private IEnumerator FadeCoroutine(CanvasGroup from = null, CanvasGroup to = null)
    {
        float t = 0f;

        Color startingColor = fade.color;
        startingColor.a = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / fadeTime;
            startingColor.a = t;
            fade.color = startingColor;

            yield return null;
        }

        fade.color = startingColor;

        if (to != null)
        {
            UiUtils.SetCanvasActive(to, true);
        }
        if (from != null)
        {
            UiUtils.SetCanvasActive(from, false);
        }

        t = 1f;

        while (t > 0f)
        {
            t -= Time.unscaledDeltaTime / fadeTime;
            startingColor.a = t;
            fade.color = startingColor;

            yield return null;
        }

        fade.color = startingColor;

        fadeCoroutine = null;
    }

    public void OnNextLevel()
    {
        BootStrapper.OnReset();
        Debug.LogError("Next level functionality not implemented!");
    }

    public void OnExit()
    {
        BootStrapper.OnReset();
        ServiceLocator.Instance.GetService<GameManager>().LoadScene("MainMenu");
    }
}
