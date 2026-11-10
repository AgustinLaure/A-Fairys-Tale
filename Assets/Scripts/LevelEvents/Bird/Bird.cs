using System.Collections;
using UnityEngine;

public class Bird : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AreaTrigger areaTrigger;
    private EventBus eventBus;

    [Header("Config")]
    [SerializeField] private float pushForce;
    [SerializeField] private float pushWaitTime;
    [SerializeField] private float pushStunTime;

    private int fullBodyLayer;

    private const string playerTag = "Player";
    private const ForceMode pushForceMode = ForceMode.VelocityChange;

    private Coroutine pushCoroutine = null;
    private WaitForSeconds pushWait;

    private void Awake()
    {
        areaTrigger.OnTriggerEntered += HandleCollision;
        pushWait = new WaitForSeconds(pushWaitTime);

        fullBodyLayer = LayerMask.NameToLayer("Trigger");
    }

    private void Start()
    {
        eventBus = ServiceLocator.Instance.GetService<EventBus>();
    }

    private IEnumerator PushCoroutine(Collider collider)
    {
        eventBus.Raise<OnBirdFlap>();

        yield return pushWait;

        eventBus.Raise<OnPushPlayer>((collider.transform.position - transform.position).normalized * pushForce, pushStunTime);

        pushCoroutine = null;
    }

    private void HandleCollision(Collider collider)
    {
        if (collider.transform.CompareTag(playerTag) && collider.gameObject.layer == fullBodyLayer)
        {
            if (pushCoroutine != null)
            {
                StopCoroutine(pushCoroutine);
            }

            pushCoroutine = StartCoroutine(PushCoroutine(collider));
        }
    }

    private void OnDestroy()
    {
        areaTrigger.OnTriggerEntered -= HandleCollision;
    }
}
