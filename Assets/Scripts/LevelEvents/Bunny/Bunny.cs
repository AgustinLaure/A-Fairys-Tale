using System.Collections;
using UnityEngine;

public class Bunny : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject pivot;
    [SerializeField] private AreaTrigger carrotTrigger;
    [SerializeField] private AreaTrigger foxTrigger;
    [SerializeField] private Transform watchHolePoint;
    [SerializeField] private Transform watchGrassPoint;
    [SerializeField] private Transform runPoint;
    private EventBus eventBus;

    [Header("Configs")]
    [SerializeField] private float reachWatchHoleTime;
    [SerializeField] private float reachWatchGrassTime;
    [SerializeField] private float reachRunPointTime;
    [SerializeField] private float watchingTime;

    private WaitForSeconds watchingTimer;

    private bool isStartled = false;

    private const string carrotTag = "Carrot";
    private const string foxTag = "Fox";

    private void Awake()
    {
        carrotTrigger.OnTriggerEntered += HandleCarrotTrigger;
        foxTrigger.OnTriggerEntered += HandleFoxTrigger;

        watchingTimer = new WaitForSeconds(watchingTime);
    }

    private void Start()
    {
        eventBus = ServiceLocator.Instance.GetService<EventBus>();
    }

    private IEnumerator SequenceCoroutine()
    {
        float t = 0f;

        Vector3 startPos = pivot.transform.position;

        while (t < 1f)
        {
            t += Time.deltaTime / reachWatchHoleTime;

            pivot.transform.position = Vector3.Lerp(startPos, watchHolePoint.position, t);

            yield return null;
        }

        pivot.transform.position = watchHolePoint.position;

        eventBus.Raise<OnBunnyWatch>();

        yield return watchingTimer;

        eventBus.Raise<OnBunnyWalk>();

        startPos = pivot.transform.position;
        t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / reachWatchGrassTime;

            pivot.transform.position = Vector3.Lerp(startPos, watchGrassPoint.position, t);

            yield return null;
        }

        eventBus.Raise<OnBunnyWatch>();
        eventBus.Raise<OnFoxAlert>();

        while (!isStartled)
        {
            yield return null;
        }

        eventBus.Raise<OnBunnyWalk>();

        startPos = pivot.transform.position;
        t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / reachRunPointTime;

            pivot.transform.position = Vector3.Lerp(startPos, runPoint.position, t);

            yield return null;
        }

        Destroy(gameObject);
    }

    private void HandleCarrotTrigger(Collider collider)
    {
        if (collider.CompareTag(carrotTag))
        {
            StartCoroutine(SequenceCoroutine());
        }
    }

    private void HandleFoxTrigger(Collider collider)
    {
        if (collider.CompareTag(foxTag))
        {
            isStartled = true;
        }
    }

    private void OnDestroy()
    {
        carrotTrigger.OnTriggerEntered -= HandleCarrotTrigger;
        foxTrigger.OnTriggerEntered -= HandleFoxTrigger;
    }
}
