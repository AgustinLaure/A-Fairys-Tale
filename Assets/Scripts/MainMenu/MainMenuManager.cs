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
        foreach (LevelData level in levels)
        {
            GameObject newButton = Instantiate(buttonPrefab, buttonContainer);

            newButton.name = level.name + "Button";

            TMP_Text buttonText = newButton.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = level.levelName;
            }

            Button buttonComponent = newButton.GetComponent<Button>();

            buttonComponent.onClick.AddListener(() => OnLevelButtonClicked(level));
        }
    }

    private void OnLevelButtonClicked(LevelData clickedLevel)
    {
        gameManager.SetActiveLevelData(clickedLevel);
        gameManager.StartLevel();
    }
}