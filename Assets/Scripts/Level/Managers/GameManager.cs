using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private GameObject loadScreen;
    [SerializeField] private List<LevelData> levels;

    private LevelData activeLevelData;

    private LevelManager activeLevelManager;

    public LevelManager levelManager { get { return activeLevelManager; } }
    public LevelData levelData { get { return activeLevelData; } }
    public int GetMaxLevels { get { return levels.Count; } }


    private void Awake()
    {
    }

    void Start()
    {

    }

    void Update()
    {

    }

    public void SetActiveLevelManager(LevelManager levelManager)
    {
        activeLevelManager = levelManager;
        if (activeLevelManager == null)
        {
            Debug.Log("Leverl manager is null!");
            return;
        }
        Debug.Log("Added level manager" + activeLevelManager.name);
        Debug.Log("New " + (((levelManager is GameplayLevelManager)) ? "Gameplay " : "Folding "));
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

        //GameObject loadScreenObject = Instantiate(loadScreen);
        // DontDestroyOnLoad(loadScreenObject);

        Debug.Log("Corroutine");

        BootStrapper.OnReset();
        AsyncOperation operation = SceneManager.LoadSceneAsync(activeLevelData.sceneName);
        //StartCoroutine(HandleLoadScreen(operation, loadScreenObject));
    }

    //private IEnumerator HandleLoadScreen(AsyncOperation operation, GameObject loadScreenObject)
    //{
    //    TMP_Text loadingText = loadScreenObject.GetComponentInChildren<TMP_Text>();
    //    if (loadingText == null)
    //    {
    //        Debug.LogError("Couldn't find the text!");
    //        yield break;
    //    }

    //    loadingText.text = "Loading... 0%";

    //    while (!operation.isDone)
    //    {
    //        loadingText.text = "Loading... " + (int)(operation.progress * 100.0f) + "%";

    //        yield return null;
    //    }

    //    Destroy(loadScreenObject);
    //}
}
