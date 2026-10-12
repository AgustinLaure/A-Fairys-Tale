using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPickUp : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BoxCollider pickUpArea;
    [SerializeField] private PlayerInventory inventory;
    private InputHandler inputHandle;
    private InputAction pickUpAction;
    private EventBus eventBus;

    [Header("Configs")]
    [SerializeField] private LayerMask propMask;

    private const int maxColliders = 20;

    private Collider[] propColliders = new Collider[maxColliders];

    private Prop currentHighlighted = null;

    private Coroutine checkUnhighlightCoroutine = null;

    private bool isLastObjectInside = false;

    private void Start()
    {
        ServiceLocator serviceLocator = ServiceLocator.Instance;

        eventBus = serviceLocator.GetService<EventBus>();

        inputHandle = serviceLocator.GetService<InputHandler>();
        pickUpAction = inputHandle.playerInput.actions["PickUp"];
        pickUpAction.performed += OnPickUp;
    }

    private void Update()
    {
        UpdateHighlight();
    }

    private void UpdateHighlight()
    {
        int hits = Physics.OverlapBoxNonAlloc(pickUpArea.bounds.center, pickUpArea.bounds.extents, propColliders, pickUpArea.transform.rotation, propMask);

        //for (int i = 0; i < hits; i++)
        //{
        //    Prop propCheck = propColliders[i].GetComponentInParent<Prop>();
        //
        //    if (propCheck != null && propColliders[i] == currentHighlighted)
        //    {
        //        return;
        //    }
        //}
        //
        //if (currentHighlighted != null)
        //{
        //    currentHighlighted.SetHighlight(false);
        //    currentHighlighted = null;
        //}
        //

        if (!isLastObjectInside)
        {
            if (hits < 1)
            {
                return;
            }

            Prop currentProp = null;

            if (inventory.GetCurrentProp == null)
            {
                for (int i = 0; i < hits; i++)
                {
                    currentProp = propColliders[i].GetComponentInParent<Prop>();

                    if (currentProp != null)
                    {
                        if (currentProp.GetIsPickable)
                        {
                            break;
                        }
                    }
                    else
                    {
                        currentProp = null;
                    }
                }

                currentHighlighted = currentProp;
                currentProp.SetHighlight(true);
                StartCoroutine(CheckUnhighlightCoroutine());
            }
        }
    }

    private IEnumerator CheckUnhighlightCoroutine()
    {
        isLastObjectInside = true;

        bool shouldBreak = false;

        while (!shouldBreak)
        {
            shouldBreak = true;

            int hits = Physics.OverlapBoxNonAlloc(pickUpArea.bounds.center, pickUpArea.bounds.extents, propColliders, pickUpArea.transform.rotation, propMask);

            if (hits < 1)
            {
                isLastObjectInside = false;
                checkUnhighlightCoroutine = null;
            }

            for (int i = 0; i < hits; i++)
            {
                Prop propCheck = propColliders[i].GetComponentInParent<Prop>();

                if (propCheck != null && propCheck == currentHighlighted)
                {
                    shouldBreak = false;
                }
            }

            if (shouldBreak)
            {
                if (currentHighlighted != null)
                {
                    currentHighlighted.SetHighlight(false);
                }

                currentHighlighted = null;
            }

            yield return null;
        }

        isLastObjectInside = false;

        checkUnhighlightCoroutine = null;
    }

    private void OnPickUp(InputAction.CallbackContext value)
    {
        if (currentHighlighted != null)
        {
            eventBus.Raise<OnPlayerPickUp>(currentHighlighted);
        }

        //if (Physics.CheckBox(pickUpArea.bounds.center, pickUpArea.bounds.extents, pickUpArea.transform.rotation, propMask))
        //{
        //    Physics.OverlapBoxNonAlloc(pickUpArea.bounds.center, pickUpArea.bounds.extents, propColliders, pickUpArea.transform.rotation, propMask);
        //
        //    Prop prop = propColliders[0].GetComponentInParent<Prop>();
        //
        //    if (prop.GetIsPickable)
        //    {
        //        eventBus.Raise<OnPlayerPickUp>(prop);
        //    }
        //}
    }

    private void OnDestroy()
    {
        pickUpAction.performed -= OnPickUp;
    }
}
