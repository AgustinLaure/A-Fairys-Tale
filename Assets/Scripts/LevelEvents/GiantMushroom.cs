using System.Collections;
using UnityEngine;

public class GiantMushroom : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private AreaCollider areaCollider;
    private EventBus eventBus;

    private const string idleAnimName = "Idle";
    private const string shakeAnimName = "Shake";
    private const string playerTag = "Player";

    private int idleAnimHash = 0;
    private int shakeAnimHash = 0;

    private Coroutine shakeCoroutine = null;

    private bool activationCooldown = false;

    private void Awake()
    {
        idleAnimHash = Animator.StringToHash(idleAnimName);
        shakeAnimHash = Animator.StringToHash(shakeAnimName);
    }

    private void Update()
    {
        activationCooldown = false;
    }

    private void Start()
    {
        eventBus = ServiceLocator.Instance.GetService<EventBus>();

        areaCollider.OnColliderEntered += HandleAreaCollider;
    }

    private void HandleAreaCollider(Collision collision)
    {
        if (collision.gameObject.CompareTag(playerTag) && !activationCooldown)
        {
            eventBus.Raise<OnActivateMole>();

            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
            }

            shakeCoroutine = StartCoroutine(ShakeCoroutine());

            activationCooldown = true;
        }
    }

    private IEnumerator ShakeCoroutine()
    {
        animator.Play(shakeAnimHash);

        bool shakeAnimationEnded = false;
        while (!shakeAnimationEnded)
        {
            shakeAnimationEnded = AnimationUtils.GetAnimationEnded(animator, shakeAnimHash);

            yield return null;
        }

        animator.Play(idleAnimHash);
    }

    private void OnDestroy()
    {
        areaCollider.OnColliderEntered -= HandleAreaCollider;
    }
}
