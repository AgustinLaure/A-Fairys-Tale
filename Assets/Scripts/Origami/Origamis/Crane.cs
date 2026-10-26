using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class Crane : Origami
{
    [Header("References")]
    [SerializeField] private AreaTrigger playerTrigger;
    private InputHandler inputHandle;
    private InputAction cursorPosAction;
    private InputAction usePropAction;
    private EventBus eventBus;
    private Rigidbody rb;
    private bool aimAnimEnded = false;
    private bool throwAnimEnded = false;

    [Header("Configs")]
    [SerializeField] private float flySpeed;

    private bool shouldLaunch = false;

    private readonly Quaternion facingLeftRotation = Quaternion.Euler(0f, 0, 0f);
    private readonly Quaternion facingRightRotation = Quaternion.Euler(0f, 180f, 0f);
    private Plane levelPlane = new Plane(Vector3.forward, Vector3.zero);
    private const ForceMode launchForceMode = ForceMode.VelocityChange;


    private Coroutine throwCoroutine = null;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
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
        if (!isCrumpled)
        {
            

            
            
            
            

            
            
        }
    }

    private void FixedUpdate()
    {
        if (shouldLaunch)
        {
            rb.AddForce(Vector3.up * 10f, launchForceMode);
            shouldLaunch = false;
        }
    }

    private void Launch()
    {
        shouldLaunch = true;

        //Assumes player is child of layer and origami is child of pivot
        transform.parent = transform.parent.parent.parent;
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
       
       eventBus.Raise<OnPlayerThrow>();
       yield return new WaitUntil(() => throwAnimEnded);
       
       Enable();
       Launch();
       
       eventBus.Raise<OnOrigamiUsed>();
       
       sphereCollider.gameObject.layer = baseLayer;
       
       AddToCurrentLayer();
       
       throwCoroutine = null;
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
