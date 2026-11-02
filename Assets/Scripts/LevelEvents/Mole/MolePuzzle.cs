using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MolePuzzle : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Mole[] moles;

    [Header("Configs")]
    [SerializeField] private MolePuzzleConfig molePuzzleConfig;
    [SerializeField] private bool isPuzzleActive = false;

    private int currentActivateMole = 0;
    private int molesAnimFinished = 0;

    private const int maxMoles = 5;

    private EventBus eventBus;

    private void Awake()
    {
        for (int i = 0; i < molePuzzleConfig.moleOneNeighbors.Length; i++)
        {
            moles[0].AddNeighbor(moles[molePuzzleConfig.moleOneNeighbors[i]]);
        }

        for (int i = 0; i < molePuzzleConfig.moleTwoNeighbors.Length; i++)
        {
            moles[1].AddNeighbor(moles[molePuzzleConfig.moleTwoNeighbors[i]]);
        }

        for (int i = 0; i < molePuzzleConfig.moleThreeNeighbors.Length; i++)
        {
            moles[2].AddNeighbor(moles[molePuzzleConfig.moleThreeNeighbors[i]]);
        }

        for (int i = 0; i < molePuzzleConfig.moleFourNeighbors.Length; i++)
        {
            moles[3].AddNeighbor(moles[molePuzzleConfig.moleFourNeighbors[i]]);
        }

        for (int i = 0; i < molePuzzleConfig.moleFiveNeighbors.Length; i++)
        {
            moles[4].AddNeighbor(moles[molePuzzleConfig.moleFiveNeighbors[i]]);
        }
    }

    private void Start()
    {
        eventBus = ServiceLocator.Instance.GetService<EventBus>();
        eventBus.Subscribe<OnActivateMole>((Action)HandleActivateMole);
        eventBus.Subscribe<OnMoleSwapped>((Action<OnMoleSwapped>)HandleMoleSwapped);
        eventBus.Subscribe<OnMoleShowAnimFinished>((Action)HandleMoleShowAnimFinish);
    }

    //TEST
    private void Update()
    {
        if (Keyboard.current.lKey.wasPressedThisFrame)
        {
            eventBus.Raise<OnActivateMole>();
        }
    }

    private void HandleActivateMole()
    {
        if (!isPuzzleActive)
        {
            moles[currentActivateMole].SwapState();
            currentActivateMole++;

            if (currentActivateMole >= maxMoles)
            {
                eventBus.Unsubscribe<OnActivateMole>((Action)HandleActivateMole);
            }
        }
    }

    private void HandleMoleShowAnimFinish()
    {
        if (!isPuzzleActive)
        {
            molesAnimFinished++;

            if (molesAnimFinished >= maxMoles)
            {
                foreach (Mole mole in moles)
                {
                    mole.SwapState();
                }

                for (int i = 0; i < moles.Length; i++)
                {
                    moles[i].SetState(molePuzzleConfig.startActivatedMoles[i]);
                    moles[i].SetShouldToggle = true;
                }

                isPuzzleActive = true;
            }
        }
    }

    private void HandleMoleSwapped(OnMoleSwapped data)
    {
        if (!data.isMoleShown && !isPuzzleActive)
        {
            for (int i = 0; i < currentActivateMole; i++)
            {
                moles[i].SetState(false);
            }

            currentActivateMole = 0;
            molesAnimFinished = 0;
        }
    }

    private void OnDestroy()
    {
        eventBus.Unsubscribe<OnActivateMole>((Action)HandleActivateMole);
        eventBus.Unsubscribe<OnMoleSwapped>((Action<OnMoleSwapped>)HandleMoleSwapped);
        eventBus.Unsubscribe<OnMoleShowAnimFinished>((Action)HandleMoleShowAnimFinish);
    }
}
