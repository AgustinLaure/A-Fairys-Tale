using System;
using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private GameObject loadingVisualPanel;

    private EventBus eventBus;
    private GameManager gameManager;

    void Start()
    {
        eventBus = ServiceLocator.Instance.GetService<EventBus>();
        gameManager = ServiceLocator.Instance.GetService<GameManager>();

        eventBus.Subscribe<OnSceneChange>((Action)StartLoadingScreen);

        if (gameManager != null)
        {
            gameManager.OnSceneChangeStart += StartLoadingScreen;
            //gameManager.OnSceneChangeEnd += StopLoadingScreen;
            Debug.Log($"LoadingScreen successfully subscribed to GameManager events.");
        }
        else
        {
            Debug.LogError($"LoadingScreen gameManager not found in ServiceLocator!");
        }

        StopLoadingScreen();
    }

    void StartLoadingScreen()
    {
        Debug.Log("Start Loading Screen");
        if (loadingVisualPanel != null)
        {
            loadingVisualPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Loading Visual Panel is missing!");
        }
    }

    void StopLoadingScreen()
    {
        Debug.Log("End Loading Screen");
        if (loadingVisualPanel != null)
        {
            loadingVisualPanel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (eventBus != null)
        {
            eventBus.Unsubscribe<OnSceneChange>((Action)StartLoadingScreen);
        }

        if (gameManager != null)
        {
            gameManager.OnSceneChangeStart -= StartLoadingScreen;
            //gameManager.OnSceneChangeEnd -= StopLoadingScreen;
        }
    }
}