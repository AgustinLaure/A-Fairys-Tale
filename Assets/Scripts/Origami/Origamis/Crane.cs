using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class Crane : Origami
{
    [Header("References")]
    [SerializeField] private GameObject pivot;
    [SerializeField] private AreaTrigger playerTrigger;
    [SerializeField] private Trace startPath;
    [SerializeField] private Trace endPath;
    //[SerializeField] private Rigidbody rb;


    private InputHandler inputHandle;
    private InputAction cursorPosAction;
    private InputAction usePropAction;
    private EventBus eventBus;
    private bool aimAnimEnded = false;
    private bool throwAnimEnded = false;

    [Header("Configs")]

    //Time to reach objective
    [SerializeField] private float flySpeed;
    [SerializeField] private float flyAwaySpeed;
    [SerializeField] private float timeToDisappear;
    [SerializeField] private float traceFadeTime;

    private bool shouldLaunch = false;
    private const float epsilon = 1e-06f;

    public const string CraneTag = "Crane";

    private readonly Quaternion facingLeftRotation = Quaternion.Euler(0f, 0, 0f);
    private readonly Quaternion facingRightRotation = Quaternion.Euler(0f, 180f, 0f);
    private Plane levelPlane = new Plane(Vector3.forward, Vector3.zero);
    private const ForceMode launchForceMode = ForceMode.VelocityChange;

    private Vector3 currentTarget = Vector3.zero;

    private Vector3 startObjectivePos = Vector3.zero;
    private Vector3 endObjectivePos = Vector3.zero;

    private Coroutine throwCoroutine = null;

    private Coroutine flyCoroutine = null;

    private void Awake()
    {
        playerTrigger.OnTriggerEntered += HandleTriggerEnter;
    }

    protected override void Start()
    {
        base.Start();

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

    }

    private void Launch()
    {
        transform.parent = transform.parent.parent.parent;
    }

    public override void Enable()
    {
        origamiRenderer.SetActive(true);
        //EnableRigidBody(rb);
    }

    public override void Disable()
    {
        origamiRenderer.SetActive(false);
        //DisableRigidBody(rb);
    }

    public override void Action()
    {
        if (throwCoroutine == null)
        {
            throwCoroutine = StartCoroutine(ThrowCoroutine());
        }
    }

    public IEnumerator ThrowCoroutine()
    {
        yield return null;
        aimAnimEnded = false;
        eventBus.Raise<OnPlayerAim>();

        yield return new WaitUntil(() => aimAnimEnded);

        bool hasClicked = false;
        while (!hasClicked)
        {
            yield return null;

            cursorPosition = cursorPosAction.ReadValue<Vector2>();

            hasClicked = usePropAction.WasPressedThisFrame();
        }

        Vector3 cursorPos = GetMouseWorldPosition();

        startObjectivePos = cursorPos;
        startPath.SetPoints(transform.position, cursorPos);
        startPath.SetVisibility(true);

        hasClicked = false;

        while (!hasClicked)
        {
            yield return null;

            cursorPosition = cursorPosAction.ReadValue<Vector2>();

            hasClicked = usePropAction.WasPressedThisFrame();
        }

        cursorPos = GetMouseWorldPosition();
        endObjectivePos = cursorPos;
        endPath.SetPoints(startObjectivePos, cursorPos);
        endPath.SetVisibility(0, false);
        endPath.SetVisibility(true);

        currentTarget = startObjectivePos;

        eventBus.Raise<OnPlayerThrow>();
        yield return new WaitUntil(() => throwAnimEnded);

        Enable();
        Launch();

        if (flyCoroutine == null)
        {
            flyCoroutine = StartCoroutine(FlyCoroutine());
        }

        eventBus.Raise<OnOrigamiUsed>();

        sphereCollider.gameObject.layer = baseLayer;

        AddToCurrentLayer();

        throwCoroutine = null;
    }

    private IEnumerator FlyCoroutine()
    {
        Vector3 direction = Vector3.Normalize(currentTarget - pivot.transform.position);

        while (Vector3.Dot(currentTarget, direction) > Vector3.Dot(pivot.transform.position, direction))
        {
            pivot.transform.position += direction * (flySpeed * Time.deltaTime);
            yield return null;
        }

        pivot.transform.position = currentTarget;

        StartCoroutine(TraceFadeOut(startPath));

        currentTarget = endObjectivePos;

        direction = Vector3.Normalize(currentTarget - pivot.transform.position);

        while (Vector3.Dot(currentTarget, direction) > Vector3.Dot(pivot.transform.position, direction))
        {
            pivot.transform.position += direction * (flySpeed * Time.deltaTime);
            yield return null;
        }

        pivot.transform.position = currentTarget;

        StartCoroutine(TraceFadeOut(endPath));

        Vector3 randomDirection = Vector3.up;

        randomDirection.x = UnityEngine.Random.Range(-1f, 1f);
        randomDirection.Normalize();

        float t = 0f;

        while (t < timeToDisappear)
        {
            t += Time.deltaTime;

            pivot.transform.position += randomDirection * (flyAwaySpeed * Time.deltaTime);

            yield return null;
        }

        origamiPool.ReturnOrigami(gameObject, typeof(Crane));

        pivot.transform.localPosition = Vector3.zero;

        flyCoroutine = null;
    }

    private IEnumerator TraceFadeOut(Trace trace)
    {
        float t = 1f;

        while (t > 0f)
        {
            t -= Time.deltaTime / traceFadeTime;

            trace.SetPointsAlpha(t);

            yield return null;
        }

        trace.SetPointsAlpha(0);
        trace.SetVisibility(false);
    }

    private void HandleTriggerEnter(Collider collider)
    {
        //si es prop hace lo que tiene que hacer y si es agua crumplea
    }

    public void HandleAimAnimFinish()
    {
        aimAnimEnded = true;
    }

    public void HandleThrowAnimFinish()
    {
        throwAnimEnded = true;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        playerTrigger.OnTriggerEntered -= HandleTriggerEnter;
    }
}
