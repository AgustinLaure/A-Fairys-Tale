using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LevelData[] levels;
    [SerializeField] private Button[] levelButtons;
    [SerializeField] private Button controlsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button muteButton;

    private GameManager gameManager;

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
    }

    private void HandleControlsPressed()
    {
        Debug.Log("funcioino");
    }

    private void HandleCreditsPressed()
    {
        Debug.Log("funcioino");
    }

    private void HandleMutePressed()
    {
        Debug.Log("funcioino");
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
    }
}