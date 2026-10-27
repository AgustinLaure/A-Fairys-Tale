using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHorizontalMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform baseTransform;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private CapsuleCollider legsCollider;
    [SerializeField] private PhysicsMaterial noFrictionMat;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private EventBus eventBus;

    [Header("Movement")]
    [SerializeField] private float acceleration;
    [SerializeField] private float terminalVelocity;
    [SerializeField] private float icedMultiplier;
    [SerializeField] private float timePushed;

    private const ForceMode walkForceMode = ForceMode.Acceleration;

    private Vector2 playerAxisInput = Vector2.zero;
    private InputAction moveAction;

    private PhysicsMaterial originalLegsColliderMat;

    private readonly Quaternion facingLeftRotation = Quaternion.Euler(0f, 0, 0f);
    private readonly Quaternion facingRightRotation = Quaternion.Euler(0f, 180f, 0f);

    private bool isOnIce = false;
    private bool isPushed = false;

    private Coroutine pushCoroutine = null;

    private GameplayLevelManager levelManager;

    private void Start()
    {
        ServiceLocator serviceLocator = ServiceLocator.Instance;

        eventBus = serviceLocator.GetService<EventBus>();
        eventBus.Subscribe<OnPlayerIcedChanged>((Action<OnPlayerIcedChanged>)HandleIsOnIceStateChanged);
        eventBus.Subscribe<OnPushPlayer>((Action<OnPushPlayer>)HandlePlayerPush);

        moveAction = serviceLocator.GetService<InputHandler>().playerInput.actions["Move"];

        originalLegsColliderMat = legsCollider.sharedMaterial;

        levelManager = serviceLocator.GetService<GameManager>().levelManager as GameplayLevelManager;
    }

    private void Update()
    {
        playerAxisInput = moveAction.ReadValue<Vector2>();

        UpdateFacingDirection();
    }

    private void FixedUpdate()
    {
        if (playerAxisInput.x != 0f)
        {
            if ((playerAxisInput.x > 0f && rb.linearVelocity.x < terminalVelocity) || (playerAxisInput.x < 0f && rb.linearVelocity.x > -terminalVelocity))
            {
                float trueAccel = isOnIce ? acceleration * icedMultiplier : acceleration;

                rb.AddForce(new Vector3(playerAxisInput.x * trueAccel, 0f, 0f), walkForceMode);
                eventBus.Raise<OnPlayerMovedHorizontally>(playerAxisInput.x);
            }
        }

        //rb.linearVelocity = new Vector3(Mathf.Clamp(rb.linearVelocity.x, -terminalVelocity, terminalVelocity), rb.linearVelocity.y, rb.linearVelocity.z);

        if (playerAxisInput.magnitude <= 0f)
        {
            if (!isOnIce && !isPushed)
            {
                SetLegsColliderSharedMaterial(originalLegsColliderMat);
            }
        }
        else
        {
            SetLegsColliderSharedMaterial(noFrictionMat);
        }
    }

    private void SetLegsColliderSharedMaterial(PhysicsMaterial physicsMaterial)
    {
        if (legsCollider.sharedMaterial != physicsMaterial)
        {
            legsCollider.sharedMaterial = physicsMaterial;
        }
    }

    private void UpdateFacingDirection()
    {
        float horizontalInput = playerAxisInput.x;

        if (horizontalInput > 0)
        {
            baseTransform.rotation = facingLeftRotation;
        }
        else if (horizontalInput < 0)
        {
            baseTransform.rotation = facingRightRotation;
        }
    }

    private void HandleIsOnIceStateChanged(OnPlayerIcedChanged data)
    {
        isOnIce = data.state;
    }

    private void HandlePlayerPush(OnPushPlayer data)
    {
        if (pushCoroutine != null)
        {
            StopCoroutine(pushCoroutine);
        }

        pushCoroutine = StartCoroutine(PushCoroutine());
    }

    private IEnumerator PushCoroutine()
    {
        isPushed = true;

        SetLegsColliderSharedMaterial(noFrictionMat);

        yield return new WaitForSeconds(timePushed);

        isPushed = false;

        pushCoroutine = null;
    }

    private void OnDisable()
    {
        SetLegsColliderSharedMaterial(originalLegsColliderMat);
    }
}
