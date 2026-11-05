using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    private GameManager gameManager;
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private GameObject buttonPrefab;

    [Header("Levels")]
    [SerializeField] private List<LevelData> levels;

    private void Start()
    {
        gameManager = ServiceLocator.Instance.GetService<GameManager>();

        GenerateLevelButtons();
    }

    private void GenerateLevelButtons()
    {
        //buttonComponent.onClick.AddListener(() => OnLevelButtonClicked(level));
    }

    private void OnLevelButtonClicked(LevelData clickedLevel)
    {
        gameManager.SetActiveLevelData(clickedLevel);
        gameManager.StartLevel();
    }
}