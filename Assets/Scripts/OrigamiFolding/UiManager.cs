using UnityEngine;
using UnityEngine.SceneManagement;

public class UiManager : MonoBehaviour
{
    FoldManager foldManager;

    public void Start()
    {
        foldManager = (ServiceLocator.Instance.GetService<GameManager>().levelManager as FoldingLevelManager).foldManager;
        if (foldManager != null) foldManager.uiManager = this;
    }

    public void OnDisable()
    {
        foldManager = (ServiceLocator.Instance.GetService<GameManager>().levelManager as FoldingLevelManager).foldManager;
        if (foldManager != null) foldManager.uiManager = null;
    }

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
