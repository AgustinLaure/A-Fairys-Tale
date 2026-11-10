using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class MolePuzzle : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Mole[] moles;
    [SerializeField] private Prop prize;
    [SerializeField] private Transform prizeEndPos;

    [Header("Configs")]
    [SerializeField] private MolePuzzleConfig molePuzzleConfig;
    [SerializeField] private bool isPuzzleActive = false;
    [SerializeField] private float prizeShowTime;

    private int currentActivateMole = 0;
    private int molesAnimFinished = 0;

    private const int maxMoles = 5;

    private const ForceMode prizeForceMode = ForceMode.Force;

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
        prize.DisableRigidBody();

        eventBus = ServiceLocator.Instance.GetService<EventBus>();
        eventBus.Subscribe<OnActivateMole>((Action)HandleActivateMole);
        eventBus.Subscribe<OnMoleSwapped>((Action<OnMoleSwapped>)HandleMoleSwapped);
        eventBus.Subscribe<OnMoleShowAnimFinished>((Action)HandleMoleShowAnimFinish);

        if (isPuzzleActive)
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
        if (!data.isMoleShown)
        {
            if (!isPuzzleActive)
            {
                for (int i = 0; i < currentActivateMole; i++)
                {
                    moles[i].SetState(false);
                }

                currentActivateMole = 0;
                molesAnimFinished = 0;
            }
            else
            {
                CheckWon();
            }
        }
    }

    private void CheckWon()
    {
        int movingMoles = 0;

        foreach (Mole mole in moles)
        {
            if (mole.GetIsShown)
            {
                return;
            }
            else if (mole.GetIsMovingUp)
            {
                movingMoles++;
            }
        }

        if (movingMoles == 0)
        {
            StartCoroutine(LaunchPropCoroutine());
        }
    }

    private IEnumerator LaunchPropCoroutine()
    {
        float t = 0f;

        Vector3 startPos = prize.transform.position;

        while (t < 1f)
        {
            t += Time.deltaTime / prizeShowTime;

            prize.transform.position = Vector3.Lerp(startPos, prizeEndPos.position, t);

            yield return null;
        }

        prize.transform.position = prizeEndPos.position;

        prize.EnableRigidBody();
    }

    private void OnDestroy()
    {
        eventBus.Unsubscribe<OnActivateMole>((Action)HandleActivateMole);
        eventBus.Unsubscribe<OnMoleSwapped>((Action<OnMoleSwapped>)HandleMoleSwapped);
        eventBus.Unsubscribe<OnMoleShowAnimFinished>((Action)HandleMoleShowAnimFinish);
    }
}
