using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingScreen : MonoSingleton<LoadingScreen>
{
    [SerializeField] private GameObject loadingVisualPanel;

    private GameManager gameManager;
    public float extraLoadTime = 0.0f;

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

        StartCoroutine(WaitForLoading());

        loadingVisualPanel.SetActive(false);
    }

    IEnumerator WaitForLoading()
    {
        Debug.Log("Extra load time of: " + extraLoadTime);

        float loadTime = extraLoadTime;

        while(loadTime > 0.0f)
        {
            loadTime -= Time.deltaTime;
            yield return null;
        }

        Debug.Log("Extra load time reset");
        extraLoadTime = 0.0f;

        yield return null;
    }
}