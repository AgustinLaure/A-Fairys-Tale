using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    private LevelManager levelManager;

    void Start()
    {
      levelManager = ServiceLocator.Instance.GetService<GameManager>().levelManager;
    }

    public void OnResume()
    {
        levelManager.TogglePause();
    }

    public void OnRetry()
    {
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
    }

    public void OnToggleSound()
    {

    }

    public void OnNextLevel()
    {
        Debug.LogError("Next level functionality not implemented!");
    }

    public void OnExit()
    {
        SceneManager.LoadSceneAsync("MainMenu");
    }
}
