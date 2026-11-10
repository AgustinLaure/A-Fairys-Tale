using System.Collections;
using UnityEngine;

public class Bush : PropSpawner
{
    [Header("References")]
    [SerializeField] private Animator animator;
    private EventBus eventBus;

    [Header("Config")]
    [SerializeField] private float shakeCooldown;
    [SerializeField] private bool summonsSquirrel;
    [SerializeField] private int maxDrops;

    private AreaCollider areaCollider;

    private const string shakeAnimName = "Shake";

    private int drops = 0;

    private bool wasSquirrelSummoned = false;

    private Coroutine shakeCoroutine = null;

    protected override void Awake()
    {
        base.Awake();

        areaCollider = GetComponent<AreaCollider>();

        areaCollider.OnColliderEntered += HandleAircraftCollision;
    }

    private void Start()
    {
        eventBus = ServiceLocator.Instance.GetService<EventBus>();
    }

    private void HandleAircraftCollision(Collision collision)
    {
        if (collision.gameObject.GetComponentInParent<Aircraft>() != null)
        {
            if (shakeCoroutine == null)
            {
                shakeCoroutine = StartCoroutine(ShakeCoroutine());
            }
        }
    }

    private IEnumerator ShakeCoroutine()
    {
        animator.Play(shakeAnimName);

        if (drops < maxDrops && !summonsSquirrel)
        {
            Quaternion randomRotation = Quaternion.Euler(0f, 0f, Random.Range(0, 361));
            Spawn(randomRotation);

            drops++;
        }

        if (summonsSquirrel && !wasSquirrelSummoned)
        {
            eventBus.Raise<OnSummonSquirrel>();
            wasSquirrelSummoned = true;
        }

        yield return new WaitForSeconds(shakeCooldown);

        shakeCoroutine = null;
    }
    private void OnDestroy()
    {
        areaCollider.OnColliderEntered -= HandleAircraftCollision;
    }
}
