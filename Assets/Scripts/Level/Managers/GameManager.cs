using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using UnityEngine.SocialPlatforms.GameCenter;

public class GameManager : MonoSingleton<GameManager>
{
    private LevelData activeLevelData;

    private LevelManager activeLevelManager;

    public LevelManager levelManager { get { return activeLevelManager; } }
    public LevelData levelData { get { return activeLevelData; } }

    public bool IsMuted { set; get; }
    private EventBus eventBus;

    public Action OnSceneChangeStart;

    public LoadingScreen loadingScreen;

    private void Start()
    {
        eventBus = ServiceLocator.Instance.GetService<EventBus>();
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


        LoadScene(activeLevelData.sceneName, (activeLevelData.GetLevelType() == LevelType.Gameplay) ? GameplayLevelManager.extraLevelLoadTime : 0.0f);
    }

    public void LoadScene(string name, float loadTime = 0.0f)
    {
        Debug.Log("Load scene: " + name);

        loadingScreen.extraLoadTime = loadTime;

        StartCoroutine(LoadSceneAsync(name));
    }

    IEnumerator LoadSceneAsync(string name)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(name);
        loadingScreen.Show();

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            loadingScreen.UpdateProgress(progress);

            yield return null;
        }

        loadingScreen.Hide();
    }
}
