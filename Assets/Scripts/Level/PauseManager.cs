using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{

    private LevelManager levelManager;
    

    void Awake()
    {
    }
    void Start()
    {
      levelManager = ServiceLocator.Instance.GetService<GameManager>().levelManager;
    }

    void Update()
    {
        
    }

    public void OnResume()
    {
        levelManager.TogglePause();
    }

    public void OnRetry()
    {

    }

    public void OnSettings()
    {

    }
    public void OnExit()
    {
        SceneManager.LoadSceneAsync("MainMenuTest");
    }

}
