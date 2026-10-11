using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem;
using System;

public class LevelManager : MonoBehaviour
{
    private GameManager gameManager;

    private EventBus eventBus;

    [SerializeField] private GameObject playerSpawnPoint;
    [SerializeField] private GameObject pausePanel;

    [SerializeField] private Transform defaultCamPos;
    [SerializeField] private Transform currentCamPos;

    [SerializeField] private static float camTransitionTime = 1.0f;
    [SerializeField] private float camTransitionCurrentTimer = 0.0f;

     public Transform DefaultCamPos { get { return defaultCamPos; }}

    [SerializeField] private Camera cam;

    [SerializeField] private int currentLayerIndex = 0;

    [SerializeField] private List<LayerController> layers = new List<LayerController>();

    [SerializeField] public Color defaultFadeColor = Color.clear;

    private bool isPaused = false;

    public bool IsPaused { get { return isPaused; } }

    [SerializeField] private InputActionReference perspectiveAction; // temp

    public float GetCamTransitionProgress() {
        return (camTransitionCurrentTimer/camTransitionTime); 
    }

    void Awake()
    {
        if (layers.Count > 0)
        {
            Debug.LogWarning("Dont fill the layers manually!");
            layers.Clear();
        }

        eventBus = ServiceLocator.Instance.GetService<EventBus>();
        eventBus.Subscribe<OnDeathAreaCollision>((Action<OnDeathAreaCollision>)HandleDeathAreaCollision);

        gameManager = ServiceLocator.Instance.GetService<GameManager>();
        gameManager.SetActiveLevelManager(this);

        if (gameManager.levelManager == null)
        {
            Debug.LogWarning("Level should be started from gameManager!");
            gameManager.SetActiveLevelManager(this);
            Debug.LogWarning("Level data will be empty.");
        }

        if (pausePanel != null) pausePanel.SetActive(false);
    }
    void Start()
    {
        StartCoroutine(LateStart());
    }

    IEnumerator LateStart()
    {
        yield return new WaitForFixedUpdate();

        layers = layers.OrderBy(g => g.transform.position.z).ToList();

        for (int i = 0; i < layers.Count; i++)
        {
            layers[i].SetLayerIndex(i);
            layers[i].RecalculateIndexes();
        }
    }

    private void Update()
    {
        if (perspectiveAction.action.WasPressedThisFrame()) // temp
        {
            cam.orthographic = !cam.orthographic;
            cam.orthographicSize = 3;
        }
    }

    private void OnDestroy()
    {
        Time.timeScale = 1.0f;
        if (gameManager != null) gameManager.SetActiveLevelManager(null);
    }

    public void RegisterLayer(LayerController layer)
    {
        layers.Add(layer);
    }

    public void SetCurrentLayer(int layerIndex)
    {
        if (layerIndex < 0 || layerIndex > layers.Count - 1)
        {
            Debug.LogError("Layer " + layerIndex + " is out of range!");
            return;
        }

        if (currentLayerIndex > layerIndex)
        {
            layers[layerIndex].SetVisible(true);
        }
        else
        {
            layers[currentLayerIndex].SetVisible(false);
        }

        currentLayerIndex = layerIndex;

        MoveCameraTransition();
    }

    void MoveCameraTransition()
    {
        currentCamPos = cam.transform;
        camTransitionCurrentTimer = 0.0f;
        StartCoroutine(MoveCameraTransitionCorroutine());
    }

    IEnumerator MoveCameraTransitionCorroutine()
    {
        while (GetCamTransitionProgress() < 1.0f)
        {
            cam.transform.position = Vector3.Lerp(currentCamPos.position, layers[currentLayerIndex].camPos.position,GetCamTransitionProgress());
            camTransitionCurrentTimer += Time.deltaTime;
            yield return null;
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0.0f : 1.0f;
        eventBus.Raise<OnPause>(IsPaused);
        if (pausePanel != null) pausePanel.SetActive(IsPaused);
        Debug.Log("Game is " + (IsPaused ? "Unp" : "P") + "aused");
    }

    public void OnWin()
    {
        eventBus.Raise<OnWin>();
    }
    public void OnLose()
    {
        eventBus.Raise<OnLose>();
    }

    private void HandleDeathAreaCollision(OnDeathAreaCollision data)
    {
        Debug.Log(data.collider.tag);
    }
}
