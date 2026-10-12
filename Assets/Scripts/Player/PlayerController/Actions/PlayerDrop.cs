using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDrop : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform dropPoint;
    [SerializeField] private PlayerInventory playerInventory;
    private GameObject currentLayerGameObject;
    private InputHandler inputHandle;
    private InputAction dropAction;
    private EventBus eventBus;

    private bool dropAnimFinished = false;

    private Coroutine dropCoroutine = null;

    private void Start()
    {
        ServiceLocator serviceLocator = ServiceLocator.Instance;

        eventBus = serviceLocator.GetService<EventBus>();

        eventBus.Subscribe<OnPlayerDropAnimFinished>((Action)HandlePlayerDropAnimFinish);
        eventBus.Subscribe<OnPlayerTryDrop>((Action)HandlePlayerTryDrop);
        eventBus.Subscribe<OnPlayerReparented>((Action<OnPlayerReparented>)HandlePlayerReparent);

        inputHandle = serviceLocator.GetService<InputHandler>();
        dropAction = inputHandle.playerInput.actions["Drop"];
        dropAction.performed += OnDrop;
    }

    private void OnDrop(InputAction.CallbackContext value)
    {
        if (enabled)
        {
            if (playerInventory.GetCurrentProp != null)
            {
                if (dropCoroutine == null)
                {
                    dropCoroutine = StartCoroutine(DropCoroutine());
                }
            }
        }
    }

    private IEnumerator DropCoroutine()
    {
        dropAnimFinished = false;

        eventBus.Raise<OnPlayerDrop>();

        Prop currentProp = playerInventory.GetCurrentProp;

        while (!dropAnimFinished)
        {
            yield return null;

            if (currentProp != null)
            {
                currentProp.transform.position = dropPoint.transform.position;
            }
        }

        if (currentLayerGameObject != null)
        {
            //Sets current layer as parent assuming parent is Action and parent.parent is layer
            currentProp.transform.SetParent(currentLayerGameObject.transform, true);
        }

        eventBus.Raise<OnPlayerDropped>();

        dropCoroutine = null;
    }

    private void HandlePlayerTryDrop()
    {
        Prop currentProp = playerInventory.GetCurrentProp;

        if (currentProp != null)
        {
            if (dropCoroutine == null)
            {
                dropCoroutine = StartCoroutine(DropCoroutine());
            }
        }
    }

    private void HandlePlayerDropAnimFinish()
    {
        Prop currentProp = playerInventory.GetCurrentProp;

        if (currentProp != null)
        {
            currentProp.transform.position = dropPoint.position;
            currentProp.Enable();

            dropAnimFinished = true;
            currentProp = null;
        }
    }

    private void HandlePlayerReparent(OnPlayerReparented data)
    {
        currentLayerGameObject = data.currentLayer;
    }

    private void OnDestroy()
    {
        dropAction.performed -= OnDrop;
    }
}
