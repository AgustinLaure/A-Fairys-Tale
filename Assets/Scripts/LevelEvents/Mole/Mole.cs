using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;

public class Mole : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private AreaTrigger areaTrigger;
    [SerializeField] private Transform pivot;
    [SerializeField] private Transform lowestPoint;
    [SerializeField] private Transform highestPoint;
    private EventBus eventBus;

    [Header("Config")]
    [SerializeField] private float moveTime;
    [SerializeField] private bool isShown;

    private bool isMovingUp = false;

    private List<Mole> neighbors = new List<Mole>();

    private const string playerTag = "Player";

    private Coroutine moveCoroutine = null;

    private bool shouldToggle = false;

    public bool GetIsShown { get { return isShown; } }

    public bool GetIsMovingUp { get { return isMovingUp; } }

    public bool SetShouldToggle { set { shouldToggle = value; } }

    private void Awake()
    {
        areaTrigger.OnTriggerEntered += HandleCollision;
    }

    private void Start()
    {
        eventBus = ServiceLocator.Instance.GetService<EventBus>();
        eventBus.Raise<OnMoleSwapped>(isShown, this);
    }

    private void Update()
    {
        //Debug.Log(isMovingUp);
    }

    public void AddNeighbor(Mole mole)
    {
        neighbors.Add(mole);
    }

    //Toggle its own state and neighbors
    public void ToggleState()
    {
        SwapState();

        foreach (Mole neighbour in neighbors)
        {
            neighbour.SwapState();
        }
    }

    public void SetState(bool state)
    {
        if (state != isShown)
        {
            SwapState();
        }
    }

    private IEnumerator MoveCoroutine(Transform target)
    {
        float t = 0f;

        Vector3 startPos = pivot.position;

        while (t < 1f)
        {
            t += Time.deltaTime / moveTime;

            pivot.position = Vector3.Lerp(startPos, target.position, t);

            yield return null;
        }

        pivot.position = target.position;

        if (isShown)
        {
            eventBus.Raise<OnMoleShowAnimFinished>();
        }

        eventBus.Raise<OnMoleSwapped>(isShown, this);

        isMovingUp = false;

        moveCoroutine = null;
    }

    //Swap singular state
    public void SwapState()
    {
        isShown = !isShown;

        if (isShown)
        {
            isMovingUp = true;
        }

        if (isShown)
        {
            eventBus.Raise<OnMoleShow>(this);
        }
        else
        {
            eventBus.Raise<OnMoleHide>(this);
        }

        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }

        moveCoroutine = StartCoroutine(MoveCoroutine(isShown ? highestPoint : lowestPoint));
    }

    private void HandleCollision(Collider collider)
    {
        if (collider.CompareTag(playerTag) && isShown)
        {
            if (shouldToggle)
            {
                ToggleState();
            }
            else
            {
                SwapState();
            }
        }
    }

    private void OnDestroy()
    {
        areaTrigger.OnTriggerEntered -= HandleCollision;
    }
}
