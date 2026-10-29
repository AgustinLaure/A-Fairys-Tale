using UnityEngine;
using UnityEngine.SceneManagement;

public class UiManager : MonoBehaviour
{
    public void OnRetry()
    {
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name,LoadSceneMode.Single);
        Debug.LogError("Reloading scenes is currently broken");
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
