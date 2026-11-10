using System;
using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private GameObject loadingVisualPanel;

    private GameManager gameManager;

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
        loadingVisualPanel.SetActive(false);
    }
}