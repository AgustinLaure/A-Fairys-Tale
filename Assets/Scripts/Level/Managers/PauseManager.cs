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
        BootStrapper.OnReset();
        ServiceLocator.Instance.GetService<GameManager>().LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnToggleSound()
    {

    }

    public void OnNextLevel()
    {
        BootStrapper.OnReset();
        Debug.LogError("Next level functionality not implemented!");
    }

    public void OnExit()
    {
        BootStrapper.OnReset();
        ServiceLocator.Instance.GetService<GameManager>().LoadScene("MainMenu");
    }
}
