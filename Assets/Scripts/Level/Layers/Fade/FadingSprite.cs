using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using System;


[RequireComponent (typeof(SpriteRenderer))]
public class FadingSprite : MonoBehaviour
{
    [SerializeField] public SpriteRenderer fadeLayerSprite;

    private int layerIndex = -1;
    private Color fadeColor;

    private EventBus eventBus;

    void Awake()
    {
        eventBus = ServiceLocator.Instance.GetService<EventBus>();
    }

    void Start()
    {
        eventBus.Subscribe<OnLayerChange>((Action<OnLayerChange>)FadeTransition);

        if (fadeLayerSprite == null)
        {
            fadeLayerSprite = GetComponent<SpriteRenderer>();
        }
        if (fadeLayerSprite == null) Debug.LogError("Could not find sprite renderer");

        Transform parentSearch = this.transform;
        bool found = false;
        while (parentSearch != null && !found)
        {
            parentSearch = parentSearch.parent;
            Debug.Log("Looking at: " +  parentSearch.name);
            found = parentSearch.GetComponent<LayerController>() != null;
        }

        if (parentSearch == null)
        {
            Debug.LogError("Could not find the layer controller for this sprite!");
            return;
        }

        LayerController controller = parentSearch.GetComponent<LayerController>();

        layerIndex = controller.LayerIndex;
        fadeColor = controller.fadeColor;
    }

    void FadeTransition(OnLayerChange layerChange)
    {
        if (layerChange.newLayer == layerIndex || layerChange.lastLayer == layerIndex)
        {
            Debug.Log("Layer change event accepted on layer " + layerIndex);
            StartCoroutine(FadeCorroutine(layerChange.newLayer == layerIndex));
        }
    }

    IEnumerator FadeCorroutine(bool visible)
    {
        float alpha = 0.0f;

        float progress = 0.0f;

        //Debug.Log("FadeColor alpha " + fadeColor.a);

        while (progress < 1.0f)
        {
            alpha = Mathf.Lerp(0.0f, fadeColor.a, progress);
            progress += Time.deltaTime;
            //Debug.Log("Alpha " + alpha + " progress: " + progress);

            fadeLayerSprite.color = new Color(fadeLayerSprite.color.r, fadeLayerSprite.color.g, fadeLayerSprite.color.b, (visible ? alpha : (1.0f - alpha)));
        }

        yield return null;
    }


    void Update()
    {
        
    }
}
