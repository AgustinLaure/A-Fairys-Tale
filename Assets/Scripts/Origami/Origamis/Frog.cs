using System;
using System.Collections;
using UnityEngine;

public class Frog : Origami
{
    [Header("References")]
    [SerializeField] private AreaTrigger playerTrigger;
    [SerializeField] private AreaCollider bodyCollider;
    [SerializeField] private GameObject playerCollider;
    private EventBus eventBus;

    [Header("Configs")]
    [SerializeField] private float selfLaunchForce;
    [SerializeField] private float playerVerticalPushForce;
    [SerializeField] private float playerHorizontalPushForce;
    [SerializeField] private float pushPlayerCooldown;

    private bool dropAnimFinished = false;
    private bool shouldLaunch = false;

    private Coroutine dropCoroutine = null;

    private const ForceMode selfLaunchForceMode = ForceMode.Impulse;
    private const ForceMode playerPushForceMode = ForceMode.Impulse;

    private const string playerTag = "Player";
    private const string groundTag = "Ground";

    private Coroutine pushPlayerCoroutine = null;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerTrigger.OnTriggerEntered += HandlePlayerTrigger;
    }

    protected override void Start()
    {
        base.Start();

        baseLayer = LayerMask.NameToLayer("Prop");

        eventBus = ServiceLocator.Instance.GetService<EventBus>();
        eventBus.Subscribe<OnPlayerDropAnimFinished>((Action)HandleDropAnimFinish);

        bodyCollider.OnColliderEntered += HandleTouchFloor;
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
        EnableRigidBody();
    }

    public override void Disable()
    {
        origamiRenderer.SetActive(false);
        DisableRigidBody();
    }

    private IEnumerator PushPlayerCoroutine(Collider collider)
    {
        Vector3 direction = Vector3.zero;

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

        if (collider.GetComponentInParent<PlayerController>().GetIsOnAir())
        {
            direction = Vector3.up;
            eventBus.Raise<OnPushPlayer>(direction * playerVerticalPushForce);
            eventBus.Raise<OnPlayerJump>();
        }
        else
        {
            direction.x = Mathf.Sign(transform.position.x - collider.transform.position.x);
            eventBus.Raise<OnPushPlayer>(direction * playerHorizontalPushForce);
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
            if (collider.CompareTag(playerTag))
            {
                pushPlayerCoroutine = StartCoroutine(PushPlayerCoroutine(collider));
            }
        }
    }

    private void HandleTouchFloor(Collision collision)
    {
        if (collision.gameObject.CompareTag(groundTag))
        {
            playerCollider.SetActive(true);
        }
    }

    private void OnDestroy()
    {
        bodyCollider.OnColliderEntered -= HandleTouchFloor;
    }
}
