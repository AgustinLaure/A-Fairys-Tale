using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{

    private LevelManager levelManager;


    void Awake()
    {
      levelManager = ServiceLocator.Instance.GetService<GameManager>().levelManager;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
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
