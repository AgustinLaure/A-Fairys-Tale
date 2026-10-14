using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using System;


[RequireComponent(typeof(SpriteRenderer))]
public class FadingSprite : MonoBehaviour
{
    [SerializeField] public SpriteRenderer fadeLayerSprite;

    public bool teleportable = false;

    private int layerIndex = -1;
    private Color fadeColor;

    private Coroutine activeFadeCoroutine;

    private EventBus eventBus;

    void Awake()
    {
        eventBus = ServiceLocator.Instance.GetService<EventBus>();
    }

    void Start()
    {
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
        Transform parentSearch = this.transform;
        bool found = false;
        while (parentSearch != null && !found)
        {
            parentSearch = parentSearch.parent;
            //Debug.Log("Looking at: " + parentSearch.name);
            found = parentSearch.GetComponent<LayerController>() != null;
        }

        if (parentSearch == null)
        {
            Debug.LogError("Could not find the layer controller for this sprite!");
            return;
        }

        LayerController controller = parentSearch.GetComponent<LayerController>();

        if (controller == null)
        {
            Debug.LogError("Could not find the layer controller for this sprite!");
            return;
        }

        layerIndex = controller.LayerIndex;
        fadeColor = controller.fadeColor;

        fadeLayerSprite.sortingLayerID = SortingLayer.NameToID("Layer " + layerIndex);
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
            bool visible = (layerIndex >= newLayerIndex);

            if (activeFadeCoroutine != null)
            {
                StopCoroutine(activeFadeCoroutine);
            }

            activeFadeCoroutine = StartCoroutine(FadeCorroutine(visible));
        }
    }

    IEnumerator FadeCorroutine(bool visible)
    {
        float progress = 0.0f;
        float duration = 1.0f;

        float fullAlpha = 1.0f;
        float halfAlpha = fadeColor.a;

        float startAlpha = fadeLayerSprite.color.a;
        float targetAlpha = visible ? fullAlpha : halfAlpha;

        //if (Mathf.Approximately(startAlpha, targetAlpha))
        //{
        //    yield break;
        //}

        Color currentColor = fadeLayerSprite.color;

        while (progress < 1.0f)
        {
            progress += Time.deltaTime / duration;

            currentColor.a = Mathf.Lerp(startAlpha, targetAlpha, progress);
            fadeLayerSprite.color = currentColor;

            yield return null;
        }

        currentColor.a = targetAlpha;
        fadeLayerSprite.color = currentColor;
    }

    void Update()
    {

    }
}
