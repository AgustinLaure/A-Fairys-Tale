using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private GameObject loadScreen;

    private LevelData activeLevelData;
    
    private LevelManager activeLevelManager;

    public LevelManager levelManager { get { return activeLevelManager; } }
    public LevelData levelData { get { return activeLevelData; } }

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
        Debug.Log("New " + (((levelManager is GameplayLevelManager)) ? "Gameplay ":"Folding " ));
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

        //GameObject loadScreenObject = Instantiate(loadScreen);
       // DontDestroyOnLoad(loadScreenObject);

        Debug.Log("Corroutine");

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
