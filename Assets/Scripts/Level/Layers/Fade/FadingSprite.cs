using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using System;


[RequireComponent(typeof(SpriteRenderer))]
public class FadingSprite : MonoBehaviour
{
    [SerializeField] public SpriteRenderer fadeLayerSprite;
    [SerializeField] private bool shouldntDarken;

    public bool teleportable = false;

    private int layerIndex = -1;
    private Color foreFadeColor;
    private Color backFadeColor;

    private Coroutine activeFadeCoroutine;

    private EventBus eventBus;
    private GameplayLevelManager levelManager;

    private void OnEnable()
    {
        if (layerIndex == -1) LateStart();
    }

    void Awake()
    {
        eventBus = ServiceLocator.Instance.GetService<EventBus>();
    }

    void Start()
    {
        levelManager = ServiceLocator.Instance.GetService<GameManager>().levelManager as GameplayLevelManager;

        eventBus.Subscribe<OnLayerChange>((Action<OnLayerChange>)FadeTransition);

        eventBus.Subscribe<OnLevelManagerLateStart>((Action)LateStart);

        if (fadeLayerSprite == null)
        {
            fadeLayerSprite = GetComponent<SpriteRenderer>();
        }

        if (fadeLayerSprite == null)
        {
            Debug.LogError("Could not find sprite renderer");
            return;
        }
    }

    void LateStart()
    {
        levelManager = ServiceLocator.Instance.GetService<GameManager>().levelManager as GameplayLevelManager;

        Transform parentSearch = this.transform;
        bool found = false;
        while (parentSearch != null && !found)
        {
            parentSearch = parentSearch.parent;
            if (parentSearch != null) found = parentSearch.GetComponent<LayerController>() != null;
        }

        if (parentSearch == null)
        {
            Debug.LogWarning("Could not find the layer controller for this sprite!");
            return;
        }

        LayerController controller = parentSearch.GetComponent<LayerController>();

        if (controller == null)
        {
            Debug.LogError("Could not find the layer controller for this sprite!");
            return;
        }

        layerIndex = controller.LayerIndex;

        foreFadeColor = controller.foreFadeColor;
        backFadeColor = controller.backFadeColor;

        if (layerIndex == -1) return;

        fadeLayerSprite.sortingLayerID = SortingLayer.NameToID("Layer " + layerIndex);

        if (!shouldntDarken)
        {
            Color targetColor = CalculateTargetColor(levelManager.GetCurrentLayerIndex);
            fadeLayerSprite.color = targetColor;

            fadeLayerSprite.enabled = (targetColor.a > 0.01f);
        }
    }

    private Color CalculateTargetColor(int activeLayerIndex)
    {
        int distance = Mathf.Abs(layerIndex - activeLayerIndex);
        float fadeMultiplier = levelManager.Layers.Count > 1 ? (float)distance / (levelManager.Layers.Count - 1) : 1f;

        if (layerIndex == activeLayerIndex)
        {
            return Color.white;
        }
        else if (layerIndex < activeLayerIndex)
        {
            return Color.Lerp(Color.white, foreFadeColor, 1.0f);
        }
        else
        {
            return Color.Lerp(Color.white, backFadeColor, fadeMultiplier);
        }
    }

    void FadeTransition(OnLayerChange layerChange)
    {
        if (teleportable) return;

        Debug.Log("Layer change event recieved on layer " + layerIndex);

        FadeFromLayer(layerChange.newLayer);
    }

    public void FadeFromLayer(int newLayerIndex)
    {
        if (gameObject.activeInHierarchy)
        {
            Color targetColor = CalculateTargetColor(newLayerIndex);

            if (activeFadeCoroutine != null)
            {
                StopCoroutine(activeFadeCoroutine);
            }

            activeFadeCoroutine = StartCoroutine(FadeCoroutine(targetColor));
        }
    }

    IEnumerator FadeCoroutine(Color targetColor)
    {
        if (!shouldntDarken)
        {
            fadeLayerSprite.enabled = true;
        }

        float progress = 0.0f;
        float duration = 1.0f;

        Color startColor = fadeLayerSprite.color;

        while (progress < 1.0f)
        {
            progress += Time.deltaTime / duration;

            fadeLayerSprite.color = Color.Lerp(startColor, targetColor, progress);

            yield return null;
        }

        fadeLayerSprite.color = targetColor;

        if (targetColor.a <= 0.01f)
        {
            fadeLayerSprite.enabled = false;
        }
    }

    void Update()
    {

    }

    private void OnDisable()
    {
        layerIndex = -1;
    }
}