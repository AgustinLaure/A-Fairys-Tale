using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class WinEntrance : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AreaTrigger areaTrigger;
    [SerializeField] private GameObject interactMessage;
    [SerializeField] private PlayerInventory playerInventory;
    private EventBus eventBus = null;

    [Header("Config")]
    [SerializeField] private string winPropTag;

    private const string playerTag = "Player";

    private InputHandler inputHandler;
    private InputAction dropAction;

    private bool canUse = false;

    private void Awake()
    {
        areaTrigger.OnTriggerEntered += HandleEnterAreaTrigger;
        areaTrigger.OnTriggerExited += HandleExitAreaTrigger;
    }

    private void Start()
    {
        ServiceLocator serviceLocator = ServiceLocator.Instance;
        eventBus = serviceLocator.GetService<EventBus>();
        inputHandler = serviceLocator.GetService<InputHandler>();

        dropAction = inputHandler.playerInput.actions["UseKey"];
        dropAction.performed += OnUse;
    }

    private void OnUse(InputAction.CallbackContext value)
    {
        Prop currentProp = playerInventory.GetCurrentProp;

        if (currentProp != null)
        {
            if (canUse)
            {
                eventBus.Raise<OnWin>();
            }
        }
    }

    private void HandleEnterAreaTrigger(Collider collider)
    {
        Prop currentProp = playerInventory.GetCurrentProp;

        if (currentProp != null)
        {
            if (currentProp.CompareTag(winPropTag) && collider.CompareTag(playerTag))
            {
                interactMessage.SetActive(true);
                canUse = true;
            }
        }
    }

    private void HandleExitAreaTrigger(Collider collider)
    {
        if (collider.CompareTag(playerTag))
        {
            interactMessage.SetActive(false);
            canUse = false;
        }
    }

    private void OnDestroy()
    {
        areaTrigger.OnTriggerEntered -= HandleEnterAreaTrigger;
        areaTrigger.OnTriggerExited -= HandleExitAreaTrigger;
        dropAction.performed -= OnUse;
    }
}
