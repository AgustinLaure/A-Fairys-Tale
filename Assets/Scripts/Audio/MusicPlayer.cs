using UnityEngine;
using System;

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

        eventBus = ServiceLocator.Instance.GetService<EventBus>();
        OnRestart();
    }
    private void Start()
    {
        serviceLocator = ServiceLocator.Instance;
        eventBus = serviceLocator.GetService<EventBus>();

        MenuState.SetValue();
        music.Post(gameObject);
    }
    private void HandleSceneLoad(OnSceneLoaded data)
    {
        if (data.sceneName == "MainMenu")
        {
            MenuState.SetValue();
        }
        else if (data.sceneName.StartsWith("Folding"))
        {
            FoldState.SetValue();
            Debug.Log("Folding");
        }
        else if (data.sceneName.StartsWith("Level"))
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

    public void OnRestart()
    {
        eventBus.Subscribe<OnSceneLoaded>((Action<OnSceneLoaded>)HandleSceneLoad);
    }

    private void OnDestroy()
    {

    }
}
