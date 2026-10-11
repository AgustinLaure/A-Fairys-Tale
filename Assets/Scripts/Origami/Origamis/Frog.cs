using System;
using System.Collections;
using UnityEngine;

public class Frog : Origami
{
    [Header("References")]
    [SerializeField] private AreaTrigger playerTrigger;
    private EventBus eventBus;
    private Rigidbody rb;

    [Header("Configs")]
    [SerializeField] private float selfLaunchForce;
    [SerializeField] private float playerPushForce;
    [SerializeField] private float pushPlayerCooldown;

    private bool dropAnimFinished = false;
    private bool shouldLaunch = false;

    private Coroutine dropCoroutine = null;

    private const ForceMode selfLaunchForceMode = ForceMode.Impulse;
    private const ForceMode playerPushForceMode = ForceMode.Impulse;

    private const string playerTag = "Player";

    private Coroutine pushPlayerCoroutine = null;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerTrigger.OnTriggerEntered += HandlePlayerTrigger;
    }

    private void Start()
    {
        baseLayer = LayerMask.NameToLayer("Prop");

        eventBus = ServiceLocator.Instance.GetService<EventBus>();
        eventBus.Subscribe<OnPlayerDropAnimFinished>((Action)HandleDropAnimFinish);
    }

    private void FixedUpdate()
    {
        if (shouldLaunch)
        {
            rb.AddForce(transform.right * selfLaunchForce, selfLaunchForceMode);
            shouldLaunch = false;
        }
    }

    private void Launch()
    {
        shouldLaunch = true;
    }

    public override void Action()
    {
        if (dropCoroutine == null)
        {
            dropCoroutine = StartCoroutine(DropCoroutine());
        }
    }

    private IEnumerator DropCoroutine()
    {
        eventBus.Raise<OnPlayerTryDrop>();

        dropAnimFinished = false;

        yield return new WaitUntil(() => dropAnimFinished);

        eventBus.Raise<OnOrigamiUsed>();

        Enable();
        Launch();

        AddToCurrentLayer();

        dropCoroutine = null;
    }

    public override void Enable()
    {
        origamiRenderer.SetActive(true);
        EnableRigidBody(rb);
    }

    public override void Disable()
    {
        origamiRenderer.SetActive(false);
        DisableRigidBody(rb);
    }

    private IEnumerator PushPlayerCoroutine(Collider collider)
    {
        if (collider.CompareTag(playerTag))
        {
            Vector3 direction;

            //bool isLeft = collider.transform.position.x < transform.position.x;
            //bool isFacingLeft = collider.transform.rotation.y > 0f;
            //
            //float distanceX = transform.position.x - transform.position.x;
            //
            //direction = collider.getCon;
            //
            //if (isLeft == isFacingLeft)
            //{
            //    direction.x *= -1f;
            //}


            //if (isLeft != isFacingLeft)
            //{
            //    direction = Vector3.Normalize(Vector3.Reflect(Vector3.Normalize(transform.position - collider.transform.position), transform.up));
            //}
            //else
            //{
            //    direction = Vector3.Normalize(collider.transform.position - transform.position);
            //}

            Vector3 rawDirection = collider.transform.position - transform.position;
            rawDirection.z = 0f;

            direction = rawDirection.normalized;

            direction = Vector3.Normalize(collider.transform.position - transform.position);

            eventBus.Raise<OnPushPlayer>(direction * playerPushForce);
            eventBus.Raise<OnPlayerJump>();
        }

        yield return new WaitForSeconds(pushPlayerCooldown);

        pushPlayerCoroutine = null;
    }

    public void HandleDropAnimFinish()
    {
        dropAnimFinished = true;
    }

    private void HandlePlayerTrigger(Collider collider)
    {
        if (pushPlayerCoroutine == null)
        {
            pushPlayerCoroutine = StartCoroutine(PushPlayerCoroutine(collider));
        }
    }
}
