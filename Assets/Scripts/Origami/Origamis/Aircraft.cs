using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Aircraft : Origami
{
    [Header("References")]
    private InputHandler inputHandle;
    private InputAction cursorPosAction;
    private InputAction usePropAction;

    private Camera mainCamera;
    private Rigidbody rb;
    private EventBus eventBus;
    private bool aimAnimEnded = false;
    private bool throwAnimEnded = false;

    [Header("Config")]
    [SerializeField] private float impulse;
    [SerializeField] private float accel;
    [SerializeField] private float accelDuration;
    [SerializeField] private float timeToEnablePlayerCollision;

    private Vector3 direction;
    private bool shouldLaunch = false;

    private Coroutine throwCoroutine = null;

    private float accelDurationTimer = 0f;

    private Vector2 cursorPosition = Vector2.zero;

    private readonly Quaternion facingLeftRotation = Quaternion.Euler(0f, 0, 0f);
    private readonly Quaternion facingRightRotation = Quaternion.Euler(0f, 180f, 0f);
    private Plane levelPlane = new Plane(Vector3.forward, Vector3.zero);

    private const ForceMode launchForceMode = ForceMode.VelocityChange;
    private const ForceMode accelForceMode = ForceMode.Acceleration;

    private const string crumpleClipNameConst = "Crumple";

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        areaCollider.OnColliderEntered += HandleCollision;

        crumpleColliderSize = 0.06f;
    }

    protected override void Start()
    {
        base.Start();

        baseLayer = LayerMask.NameToLayer("Prop");
        crumpleClipName = crumpleClipNameConst;
        mainCamera = Camera.main;

        ServiceLocator serviceLocator = ServiceLocator.Instance;

        inputHandle = serviceLocator.GetService<InputHandler>();

        cursorPosAction = inputHandle.playerInput.actions["CursorPos"];
        usePropAction = inputHandle.playerInput.actions["UseProp"];

        eventBus = serviceLocator.GetService<EventBus>();
        eventBus.Subscribe<OnPlayerAimAnimFinished>((Action)HandleAimAnimFinish);
        eventBus.Subscribe<OnPlayerThrowAnimFinished>((Action)HandleThrowAnimFinish);
    }

    private void Update()
    {
        if (!isCrumpled)
        {
            Vector2 linearVelocity = rb.linearVelocity;

            if (linearVelocity.x != 0f || linearVelocity.y != 0f)
            {
                Vector3 linear = linearVelocity;
                Vector3 newUp = Vector3.Cross(linear, Vector3.forward);

                transform.rotation = Quaternion.LookRotation(linear, newUp);
            }
        }
    }

    private void FixedUpdate()
    {
        if (shouldLaunch)
        {
            rb.AddForce(direction * impulse, launchForceMode);
            shouldLaunch = false;
        }

        accelDurationTimer += Time.deltaTime;

        if (accelDurationTimer < accelDuration)
        {
            float thisFrameAccel = accel - (accelDurationTimer / accelDuration);

            rb.AddForce(direction * thisFrameAccel, accelForceMode);
        }
    }

    private void HandleCollision(Collision collision)
    {
        if (!isCrumpled)
        {
            Crumple();
        }
    }

    private void Launch()
    {
        shouldLaunch = true;

        //Assumes player is child of layer and origami is child of pivot
        transform.parent = transform.parent.parent.parent;
    }

    public override void Action()
    {
        if (throwCoroutine == null)
        {
            throwCoroutine = StartCoroutine(ThrowCoroutine());
        }
    }

    public override void Enable()
    {
        origamiRenderer.SetActive(true);
        EnableRigidBody(rb);
    }

    public Vector3 GetMouseWorldPosition()
    {
        Vector2 screenPos = cursorPosition;

        Ray ray = mainCamera.ScreenPointToRay(screenPos);

        Plane currentLevelPlane = new Plane(Vector3.forward, new Vector3(0, 0, transform.position.z));

        if (currentLevelPlane.Raycast(ray, out float enterDistance))
        {
            return ray.GetPoint(enterDistance);
        }

        return Vector3.zero;
    }

    private void HandleRotation()
    {
        Vector3 mouseWorldPosition = GetMouseWorldPosition();
        Quaternion desiredRotation = Quaternion.identity;

        if (mouseWorldPosition.x > transform.position.x)
        {
            desiredRotation = facingLeftRotation;
        }
        else
        {
            desiredRotation = facingRightRotation;
        }

        eventBus.Raise<OnRotatePlayer>(desiredRotation);
    }

    public override void Disable()
    {
        origamiRenderer.SetActive(false);
        DisableRigidBody(rb);
    }

    public void HandleAimAnimFinish()
    {
        aimAnimEnded = true;
    }

    public void HandleThrowAnimFinish()
    {
        throwAnimEnded = true;
    }

    public IEnumerator ThrowCoroutine()
    {
        aimAnimEnded = false;
        eventBus.Raise<OnPlayerAim>();

        yield return new WaitUntil(() => aimAnimEnded);

        bool hasClicked = false;
        while (!hasClicked)
        {
            yield return null;

            cursorPosition = cursorPosAction.ReadValue<Vector2>();
            HandleRotation();

            hasClicked = usePropAction.WasPressedThisFrame();
        }

        Vector3 cursorPos = GetMouseWorldPosition();

        eventBus.Raise<OnPlayerThrow>();
        yield return new WaitUntil(() => throwAnimEnded);

        direction = Vector3.Normalize(cursorPos - transform.position);

        Enable();
        Launch();

        eventBus.Raise<OnOrigamiUsed>();

        yield return new WaitForSeconds(timeToEnablePlayerCollision);

        sphereCollider.gameObject.layer = baseLayer;

        AddToCurrentLayer();

        throwCoroutine = null;
    }

    private void OnDestroy()
    {
        areaCollider.OnColliderEntered -= HandleCollision;
    }
}


