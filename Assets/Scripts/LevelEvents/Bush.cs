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

    private AreaCollider areaCollider;

    private const string shakeAnimName = "Shake";

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

        Quaternion randomRotation = Quaternion.Euler(0f, 0f, Random.Range(0, 361));

        Spawn(randomRotation);

        if (summonsSquirrel)
        {
            eventBus.Raise<OnSummonSquirrel>();
        }

        yield return new WaitForSeconds(shakeCooldown);

        shakeCoroutine = null;
    }
    private void OnDestroy()
    {
        areaCollider.OnColliderEntered -= HandleAircraftCollision;
    }
}
