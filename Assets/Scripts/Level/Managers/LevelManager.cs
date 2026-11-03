using UnityEngine;

public class LevelManager : MonoBehaviour
{
    protected GameManager gameManager;

    protected EventBus eventBus;

    [SerializeField] protected LevelData levelData;

    protected bool isPaused = false;

    public bool IsPaused { get { return isPaused; } }


    public virtual void Awake()
    {
        gameManager = ServiceLocator.Instance.GetService<GameManager>();
        eventBus = ServiceLocator.Instance.GetService<EventBus>();

        gameManager.SetActiveLevelManager(this);

        if (gameManager.levelData != null)
        {
            levelData = gameManager.levelData;
        }
        else
        {
            Debug.LogWarning("Level started directly from scene! Using local LevelData fallback.");
            gameManager.SetActiveLevelData(levelData);
        }

        if (gameManager.levelManager == null)
        {
            Debug.LogWarning("Level should be started from gameManager!");
            gameManager.SetActiveLevelManager(this);
            Debug.LogWarning("Level data will be empty.");
        }
    }

    public virtual void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0.0f : 1.0f;
        eventBus.Raise<OnPause>(IsPaused);
        Debug.Log("Game is " + (IsPaused ? "Unp" : "P") + "aused");
    }
}
