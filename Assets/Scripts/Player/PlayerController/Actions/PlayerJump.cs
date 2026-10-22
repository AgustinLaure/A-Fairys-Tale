using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerJump : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody rb;

    [Header("Config")]
    [SerializeField] private float jumpImpulse;
    [SerializeField] private float delay;
    [SerializeField] private float jumpCooldown;
    [SerializeField] private AK.Wwise.Event jump;

    private InputHandler inputHandle;
    private EventBus eventBus;
    private InputAction jumpAction;

    private ForceMode forceMode = ForceMode.Impulse;

    private bool isJumpRequested = false;

    private Vector3 jumpDirection = Vector3.up;

    private Coroutine jumpCoroutine = null;

    private bool isOnIce = false;

    private void Start()
    {
        ServiceLocator serviceLocator = ServiceLocator.Instance;
        eventBus = serviceLocator.GetService<EventBus>();
        eventBus.Subscribe<OnPlayerIcedChanged>((Action<OnPlayerIcedChanged>)HandleIsOnIceStateChanged);
        inputHandle = serviceLocator.GetService<InputHandler>();

        jumpAction = inputHandle.playerInput.actions["Jump"];

        jumpAction.performed += OnJump;
    }

    private void FixedUpdate()
    {
        if (isJumpRequested)
        {
            rb.AddForce(jumpDirection * jumpImpulse, forceMode);
            isJumpRequested = false;

            Debug.Log("Fixed jump update");

           //AkUnitySoundEngine.PostEvent(AK.EVENTS.PLAY_SLASH1,gameObject);
        }
    }

    private IEnumerator JumpCoroutine()
    {
        eventBus.Raise<OnPlayerJump>();

        yield return new WaitForSeconds(delay);

        isJumpRequested = true;

        yield return new WaitForSeconds(jumpCooldown);

        jumpCoroutine = null;
    }

    private void OnJump(InputAction.CallbackContext value)
    {
        if (jumpCoroutine == null && enabled == true && !isOnIce)
        {
            jumpCoroutine = StartCoroutine(JumpCoroutine());
        }
    }

    private void HandleIsOnIceStateChanged(OnPlayerIcedChanged data)
    {
        isOnIce = data.state;
    }

    private void OnDestroy()
    {
        jumpAction.performed -= OnJump;
    }
}