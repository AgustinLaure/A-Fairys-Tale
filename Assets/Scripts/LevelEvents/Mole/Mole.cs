using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class Mole : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    private EventBus eventBus;

    [SerializeField] private bool isShown = true;

    private List<Mole> neighbors = new List<Mole>();

    //test
    [SerializeField] private Sprite onSprite;
    [SerializeField] private Sprite offSprite;
    [SerializeField] private Key toggleKey;

    public bool GetIsShown { get { return isShown; } }

    private void Start()
    {
        spriteRenderer.sprite = isShown ? onSprite : offSprite;

        eventBus = ServiceLocator.Instance.GetService<EventBus>();
    }

    private void Update()
    {
        if (Keyboard.current[toggleKey].wasPressedThisFrame)
        {
            if (isShown)
            {
                ToggleState();
            }
        }
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

        spriteRenderer.sprite = isShown ? onSprite : offSprite;
    }

    //Swap singular state
    public void SwapState()
    {
        isShown = !isShown;

        spriteRenderer.sprite = isShown ? onSprite : offSprite;

        eventBus.Raise<OnMoleSwapped>(isShown);

        //LOGICA DE SUBIR O BAJAR SEGUN CORRESPONDA
    }
}
