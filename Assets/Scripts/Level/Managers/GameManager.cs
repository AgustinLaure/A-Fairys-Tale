using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public class GameManager : MonoSingleton<GameManager>
{
    //[SerializeField] private List<LevelData> levels;

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


        LoadScene(activeLevelData.sceneName);
    }

    public void LoadScene(string name)
    {
        Debug.Log("Load scene: " + name);

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
