using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;

public class GameManager : MonoBehaviour
{
    [SerializeField] private List<LevelData> levels;

    private LevelData activeLevelData;

    private LevelManager activeLevelManager;

    public LevelManager levelManager { get { return activeLevelManager; } }
    public LevelData levelData { get { return activeLevelData; } }
    public int GetMaxLevels { get { return levels.Count; } }

    public bool IsMuted { set; get; }
    private EventBus eventBus;

    public Action OnSceneChangeStart;


    private bool loadSceneQueue = false;
    private string queueSceneName = "";

    public AsyncOperation currentOperation;

    private void Start()
    {
        eventBus = ServiceLocator.Instance.GetService<EventBus>();
    }

    private void Update()
    {
        if (loadSceneQueue)
        {
            currentOperation = SceneManager.LoadSceneAsync(queueSceneName);
            loadSceneQueue = false;
            queueSceneName = "";

            //BootStrapper.OnReset();
        }
    }

    public void SetActiveLevelManager(LevelManager levelManager)
    {
        activeLevelManager = levelManager;
        if (activeLevelManager == null)
        {
            Debug.Log("Level manager is null!");
            return;
        }
        Debug.Log("Added level manager" + activeLevelManager.name);
        Debug.Log("Its a " + (((levelManager is GameplayLevelManager)) ? "Gameplay " : "Folding " + " manager"));
    }

    public void SetActiveLevelData(LevelData levelData)
    {
        activeLevelData = levelData;
        if (activeLevelData != null) Debug.Log("Changed level to " + activeLevelData.levelName);
    }

    public LevelData GetLevelAt(int at)
    {
        if (at <= 0 || at >= levels.Count)
        {
            Debug.LogError("Tried to access a non existing level");
            return null;
        }

        return levels[at];
    }

    public void StartLevel()
    {
        if (activeLevelData == null)
        {
            Debug.LogError("No level set! Cant start");
            return;
        }
        if (activeLevelData.GetLevelType() == LevelType.None)
        {
            Debug.LogError("Level Data is invalid!");
            return;
        }

        Debug.Log("Started level " + activeLevelData.levelName);


        LoadScene(activeLevelData.sceneName);
    }

    public void LoadScene(string name)
    {
        Debug.Log("Load scene: " + name);

        OnSceneChangeStart?.Invoke();
        //eventBus.Raise<OnSceneChange>(queueSceneName);

        queueSceneName = name;
        loadSceneQueue = true;
    }
}
