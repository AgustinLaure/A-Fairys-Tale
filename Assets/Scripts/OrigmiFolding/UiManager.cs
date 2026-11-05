using UnityEngine;
using UnityEngine.SceneManagement;

public class UiManager : MonoBehaviour
{
    public void OnRetry()
    {
        BootStrapper.OnReset();
        ServiceLocator.Instance.GetService<GameManager>().LoadScene(SceneManager.GetActiveScene().name);
        Debug.LogError("Reloading scenes is currently broken");
    }

    public void OnToggleSound()
    {

    }

    public void OnNextLevel()
    {
        ServiceLocator.Instance.GetService<GameManager>().levelManager.GoToNextLevel();
    }

    public void OnExit()
    {
        BootStrapper.OnReset();
        ServiceLocator.Instance.GetService<GameManager>().LoadScene("MainMenu");
    }
}
