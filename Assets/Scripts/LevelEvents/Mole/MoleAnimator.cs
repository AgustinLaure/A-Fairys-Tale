using NUnit.Framework.Interfaces;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MoleAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Mole selfMole;

    private enum State
    {
        Show,
        Hide,
    }

    #region HashReferences

    private const string animatorStateVarName = "State";

    private const string showStateName = "Show";
    private const string hideStateName = "Hide";

    private Dictionary<State, int> statesAnimatorHash = new Dictionary<State, int>
    {
        [State.Show] = Animator.StringToHash(showStateName),
        [State.Hide] = Animator.StringToHash(hideStateName),
    };

    #endregion

    private EventBus eventBus;
    private FSM fsm;

    private int animatorStateHash = 0;

    private void Awake()
    {
        animatorStateHash = Animator.StringToHash(animatorStateVarName);
    }

    private void Start()
    {
        ServiceLocator serviceLocator = ServiceLocator.Instance;
        eventBus = serviceLocator.GetService<EventBus>();

        ShowState showState = new ShowState(this);
        eventBus.Subscribe<OnMoleHide>((Action<OnMoleHide>)showState.OnSwapped);

        HideState hideState = new HideState(this);
        eventBus.Subscribe<OnMoleShow>((Action<OnMoleShow>)hideState.OnSwapped);

        Dictionary<Type, global::State> states = new Dictionary<Type, global::State>()
        {
            [typeof(ShowState)] = showState,
            [typeof(HideState)] = hideState,
        };

        fsm = new FSM(states);

        fsm.SetInitialState(typeof(HideState));
    }

    private void Update()
    {
        fsm.Update();
    }

    private class ShowState : global::State
    {
        private MoleAnimator moleAnimator;

        public ShowState(MoleAnimator moleAnimator)
        {
            this.moleAnimator = moleAnimator;
        }

        public override void Enter()
        {
            moleAnimator.animator.SetInteger(moleAnimator.animatorStateHash, (int)State.Show);
        }

        public override void Update()
        {
           
        }

        public override void Exit()
        {

        }

        public void OnSwapped(OnMoleHide data)
        {
            if (data.mole == moleAnimator.selfMole)
            {
                moleAnimator.fsm.TryChange<ShowState>(typeof(HideState));
            }
        }
    }

    private class HideState : global::State
    {
        private MoleAnimator moleAnimator;

        public HideState(MoleAnimator moleAnimator)
        {
            this.moleAnimator = moleAnimator;
        }

        public override void Enter()
        {
            moleAnimator.animator.SetInteger(moleAnimator.animatorStateHash, (int)State.Hide);
        }

        public override void Update()
        {

        }

        public override void Exit()
        {

        }

        public void OnSwapped(OnMoleShow data)
        {
            if (data.mole == moleAnimator.selfMole)
            {
                moleAnimator.fsm.TryChange<HideState>(typeof(ShowState));
            }
        }
    }
}
