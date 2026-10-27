using System;
using System.Collections;
using UnityEngine;

public class Fox : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject pivot;
    [SerializeField] private Transform backLayerRunPoint;
    [SerializeField] private Transform bunnyLayerStartRunPoint;
    [SerializeField] private Transform bunnyLayerEndRunPoint;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private EventBus eventBus;

    [Header("Configs")]
    [SerializeField] private float backLayerRunPointTime;
    [SerializeField] private float bunnyLayerRunPointTime;

    private const string layer0Name = "Layer 0";

    private void Start()
    {
        eventBus = ServiceLocator.Instance.GetService<EventBus>();
        eventBus.Subscribe<OnFoxAlert>((Action)HandleAlert);
    }

    private IEnumerator ChaseCoroutine()
    {
        float t = 0f;
        Vector3 startPos = pivot.transform.position;

        while (t < 1f)
        {
            t += Time.deltaTime / backLayerRunPointTime;

            pivot.transform.position = Vector3.Lerp(startPos, backLayerRunPoint.position, t);

            yield return null;
        }

        pivot.transform.position = bunnyLayerStartRunPoint.position;

        t = 0f;
        startPos = pivot.transform.position;
        spriteRenderer.sortingLayerName = layer0Name;

        while (t < 1f)
        {
            t += Time.deltaTime / bunnyLayerRunPointTime;

            pivot.transform.position = Vector3.Lerp(startPos, bunnyLayerEndRunPoint.position, t);

            yield return null;
        }

        Destroy(gameObject);
    }

    private void HandleAlert()
    {
        StartCoroutine(ChaseCoroutine());
    }
}
