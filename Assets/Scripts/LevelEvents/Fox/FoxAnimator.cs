using System.Collections.Generic;
using UnityEngine;
using System;
using NUnit.Framework.Interfaces;

public class FoxAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;

    private enum State
    {
        Idle,
        IdleToRun,
        Run,
    }

    #region HashReferences

    private const string animatorStateVarName = "State";

    private const string idleStateName = "Idle";
    private const string idleToRunStateName = "IdleToRun";
    private const string runStateName = "Run";

    private Dictionary<State, int> statesAnimatorHash = new Dictionary<State, int>
    {
        [State.Idle] = Animator.StringToHash(idleStateName),
        [State.IdleToRun] = Animator.StringToHash(idleToRunStateName),
        [State.Run] = Animator.StringToHash(runStateName),
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

        IdleState idleState = new IdleState(this);
        eventBus.Subscribe<OnFoxAlert>((Action)idleState.OnIdleToWalk);

        IdleToRunState idleToRunState = new IdleToRunState(this);

        RunState runState = new RunState(this);

        Dictionary<Type, global::State> states = new Dictionary<Type, global::State>()
        {
            [typeof(IdleState)] = idleState,
            [typeof(IdleToRunState)] = idleToRunState,
            [typeof(RunState)] = runState,
        };

        fsm = new FSM(states);

        fsm.SetInitialState(typeof(IdleState));
    }

    private void Update()
    {
        fsm.Update();
    }


    private class IdleState : global::State
    {
        private FoxAnimator foxAnimator;

        public IdleState(FoxAnimator foxAnimator)
        {
            this.foxAnimator = foxAnimator;
        }

        public override void Enter()
        {
            foxAnimator.animator.SetInteger(foxAnimator.animatorStateHash, (int)State.Idle);
        }

        public override void Update()
        {

        }

        public override void Exit()
        {

        }

        public void OnIdleToWalk()
        {
            foxAnimator.fsm.TryChange<IdleState>(typeof(IdleToRunState));
        }
    }

    private class IdleToRunState : global::State
    {
        private FoxAnimator foxAnimator;

        public IdleToRunState(FoxAnimator foxAnimator)
        {
            this.foxAnimator = foxAnimator;
        }

        public override void Enter()
        {
            foxAnimator.animator.SetInteger(foxAnimator.animatorStateHash, (int)State.IdleToRun);
        }

        public override void Update()
        {
            if (AnimationUtils.GetAnimationEnded(foxAnimator.animator, foxAnimator.statesAnimatorHash[State.IdleToRun]))
            {
                foxAnimator.fsm.TryChange<IdleToRunState>(typeof(RunState));
            }
        }

        public override void Exit()
        {

        }
    }

    private class RunState : global::State
    {
        private FoxAnimator foxAnimator;

        public RunState(FoxAnimator foxAnimator)
        {
            this.foxAnimator = foxAnimator;
        }

        public override void Enter()
        {
            foxAnimator.animator.SetInteger(foxAnimator.animatorStateHash, (int)State.Run);
        }

        public override void Update()
        {
            
        }

        public override void Exit()
        {

        }
    }
}
