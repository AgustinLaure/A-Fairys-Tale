using System;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

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

        gameManager.SetActiveLevelManager(this);
        gameManager.SetActiveLevelData(levelData);

        eventBus = ServiceLocator.Instance.GetService<EventBus>();

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
