using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingScreen : MonoSingleton<LoadingScreen>
{
    [SerializeField] private GameObject loadingVisualPanel;

    private GameManager gameManager;
    public float extraLoadTime = 0.0f;

    public Coroutine WaitForLoadingCoroutine { get; private set; }

    void Start()
    {
        gameManager = ServiceLocator.Instance.GetService<GameManager>();

        DontDestroyOnLoad(gameObject);

        this.transform.SetParent(gameManager.transform);
        gameManager.loadingScreen = this;
    }

    public void UpdateProgress(float progress)
    {

    }


    public void Show()
    {
        loadingVisualPanel.SetActive(true);
    }

    public void Hide()
    {
        WaitForLoadingCoroutine = StartCoroutine(WaitForLoading());
    }

    IEnumerator WaitForLoading()
    {
        Debug.Log("Extra load time of: " + extraLoadTime);

        float loadTime = extraLoadTime;

        while (loadTime > 0.0f)
        {
            loadTime -= Time.unscaledDeltaTime;
            yield return null;
        }

        Debug.Log("Extra load time reset");
        extraLoadTime = 0.0f;

        loadingVisualPanel.SetActive(false);

        WaitForLoadingCoroutine = null;
    }
}