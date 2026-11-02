using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEditor;

public class Mole : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private AreaTrigger areaTrigger;
    private EventBus eventBus;

    [SerializeField] private bool isShown = true;

    private List<Mole> neighbors = new List<Mole>();

    private const string playerTag = "Player";


    public bool GetIsShown { get { return isShown; } }

    private void Awake()
    {
        areaTrigger.OnTriggerEntered += HandleCollision;
    }

    private void Start()
    {
        eventBus = ServiceLocator.Instance.GetService<EventBus>();
        eventBus.Raise<OnMoleSwapped>(isShown, this);
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
        isShown = state;
    }

    //Swap singular state
    public void SwapState()
    {
        isShown = !isShown;

        eventBus.Raise<OnMoleSwapped>(isShown, this);
    }

    private void HandleCollision(Collider collider)
    {
        if (collider.CompareTag(playerTag) && isShown)
        {
            ToggleState();
        }
    }

    private void OnDestroy()
    {
        areaTrigger.OnTriggerEntered -= HandleCollision;
    }
}
