using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerUseProp : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInventory inventory;
    private InputHandler inputHandle;
    private InputAction usePropAction;

#if UNITY_EDITOR
    private bool isGameFocusedThisFrame = false;
#endif

    private void Start()
    {
        inputHandle = ServiceLocator.Instance.GetService<InputHandler>();
        usePropAction = inputHandle.playerInput.actions["UseProp"];

        usePropAction.performed += OnUseProp;
    }

#if UNITY_EDITOR
    private void OnApplicationFocus(bool focus)
    {
        isGameFocusedThisFrame = focus;
    }
#endif

    private void Update()
    {
#if UNITY_EDITOR
        if (isGameFocusedThisFrame)
        {
            isGameFocusedThisFrame = false;
        }
#endif
    }

    private void OnUseProp(InputAction.CallbackContext value)
    {
        if (enabled)
        {
            Prop currentProp = inventory.GetCurrentProp;

            bool canPerformAction = currentProp != null;

#if UNITY_EDITOR
            canPerformAction = !isGameFocusedThisFrame && currentProp != null;
#endif
            if (canPerformAction)
            {
                currentProp.Action();
            }
        }
    }

    private void OnDestroy()
    {
        usePropAction.performed -= OnUseProp;
    }
}
