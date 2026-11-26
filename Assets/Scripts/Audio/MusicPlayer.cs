using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class MusicPlayer : MonoBehaviour
{
    private static MusicPlayer instance;

    private ServiceLocator serviceLocator;
    private EventBus eventBus;

    [SerializeField] private AK.Wwise.Event music;
    [SerializeField] private AK.Wwise.State MenuState;
    [SerializeField] private AK.Wwise.State FoldState;
    [SerializeField] private AK.Wwise.State PlayState;
    [SerializeField] private AK.Wwise.State WinState;
    [SerializeField] private AK.Wwise.State LoseState;

    private bool levelEnded;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        //Suscribimos el metodo HandleSceneLoad a la "Action" sceneLoaded de SceneManager
        SceneManager.sceneLoaded += HandleSceneLoad;
    }
    private void Start()
    {
        serviceLocator = ServiceLocator.Instance;
        eventBus = serviceLocator.GetService<EventBus>();

        music.Post(gameObject);

    }
    private void HandleSceneLoad(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
        {
            MenuState.SetValue();
        }
        else if (scene.name.StartsWith("Folding"))
        {
            FoldState.SetValue();
            Debug.Log("Folding");
        }
        else if (scene.name.StartsWith("Level"))
        {
            PlayState.SetValue();
            eventBus.Subscribe<OnWin>((Action)ChangeToWinState);
            eventBus.Subscribe<OnLose>((Action)ChangeToLoseState);
        }
    }
    private void ChangeToWinState()
    {
        if (levelEnded) return;
        levelEnded = true;
        WinState.SetValue();
    }
    private void ChangeToLoseState()
    {
        if (levelEnded) return;
        levelEnded = true;
        LoseState.SetValue();
    }
    private void OnDestroy()
    {
        //Desuscribimos el metodo HandleSceneLoad a la "Action" sceneLoaded de SceneManager.
        SceneManager.sceneLoaded -= HandleSceneLoad;
    }
}

