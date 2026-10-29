using System.Collections.Generic;
using UnityEngine;
using System;


public class BunnyAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    private enum State
    {
        Idle,
        Walk,
        Alert,
        Watch
    }

    #region HashReferences

    private const string animatorStateVarName = "State";

    private const string idleStateName = "Idle";
    private const string walkStateName = "Walk";
    private const string alertStateName = "Alert";
    private const string watchStateName = "Watch";

    private Dictionary<State, int> statesAnimatorHash = new Dictionary<State, int>
    {
        [State.Idle] = Animator.StringToHash(idleStateName),
        [State.Walk] = Animator.StringToHash(walkStateName),
        [State.Alert] = Animator.StringToHash(alertStateName),
        [State.Watch] = Animator.StringToHash(watchStateName),
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
        eventBus.Subscribe<OnBunnyWalk>((Action)idleState.OnWalk);
        eventBus.Subscribe<OnBunnyAlert>((Action)idleState.OnAlert);
        eventBus.Subscribe<OnBunnyWatch>((Action)idleState.OnWatch);

        WalkState walkState = new WalkState(this);
        eventBus.Subscribe<OnBunnyIdle>((Action)walkState.OnIdle);
        eventBus.Subscribe<OnBunnyAlert>((Action)walkState.OnAlert);
        eventBus.Subscribe<OnBunnyWatch>((Action)walkState.OnWatch);

        AlertState alertState = new AlertState(this);
        eventBus.Subscribe<OnBunnyIdle>((Action)alertState.OnIdle);
        eventBus.Subscribe<OnBunnyWalk>((Action)alertState.OnWalk);
        eventBus.Subscribe<OnBunnyWatch>((Action)alertState.OnWatch);

        WatchState watchState = new WatchState(this);
        eventBus.Subscribe<OnBunnyIdle>((Action)watchState.OnIdle);
        eventBus.Subscribe<OnBunnyWalk>((Action)watchState.OnWalk);
        eventBus.Subscribe<OnBunnyAlert>((Action)watchState.OnAlert);

        Dictionary<Type, global::State> states = new Dictionary<Type, global::State>()
        {
            [typeof(IdleState)] = idleState,
            [typeof(WalkState)] = walkState,
            [typeof(AlertState)] = alertState,
            [typeof(WatchState)] = watchState,
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
        private BunnyAnimator bunnyAnimator;

        public IdleState(BunnyAnimator bunnyAnimator)
        {
            this.bunnyAnimator = bunnyAnimator;
        }

        public override void Enter()
        {
            bunnyAnimator.animator.SetInteger(bunnyAnimator.animatorStateHash, (int)State.Idle);
        }

        public override void Update()
        {

        }

        public override void Exit()
        {

        }

        public void OnWalk()
        {
            bunnyAnimator.fsm.TryChange<IdleState>(typeof(WalkState));
        }

        public void OnAlert()
        {
            bunnyAnimator.fsm.TryChange<IdleState>(typeof(AlertState));
        }

        public void OnWatch()
        {
            bunnyAnimator.fsm.TryChange<IdleState>(typeof(WatchState));
        }
    }

    private class WalkState : global::State
    {
        private BunnyAnimator bunnyAnimator;

        public WalkState(BunnyAnimator bunnyAnimator)
        {
            this.bunnyAnimator = bunnyAnimator;
        }

        public override void Enter()
        {
            bunnyAnimator.animator.SetInteger(bunnyAnimator.animatorStateHash, (int)State.Walk);
        }

        public override void Update()
        {

        }

        public override void Exit()
        {

        }

        public void OnIdle()
        {
            bunnyAnimator.fsm.TryChange<WalkState>(typeof(IdleState));
        }
        public void OnAlert()
        {
            bunnyAnimator.fsm.TryChange<WalkState>(typeof(AlertState));
        }

        public void OnWatch()
        {
            bunnyAnimator.fsm.TryChange<WalkState>(typeof(WatchState));
        }
    }

    private class AlertState : global::State
    {
        private BunnyAnimator bunnyAnimator;

        public AlertState(BunnyAnimator bunnyAnimator)
        {
            this.bunnyAnimator = bunnyAnimator;
        }

        public override void Enter()
        {
            bunnyAnimator.animator.SetInteger(bunnyAnimator.animatorStateHash, (int)State.Alert);
        }

        public override void Update()
        {
            if (AnimationUtils.GetAnimationEnded(bunnyAnimator.animator, bunnyAnimator.statesAnimatorHash[State.Alert]))
            {
                bunnyAnimator.fsm.TryChange<AlertState>(typeof(WalkState));
            }
        }

        public override void Exit()
        {

        }

        public void OnIdle()
        {
            bunnyAnimator.fsm.TryChange<AlertState>(typeof(IdleState));
        }
        public void OnWalk()
        {
            bunnyAnimator.fsm.TryChange<AlertState>(typeof(WalkState));
        }

        public void OnWatch()
        {
            bunnyAnimator.fsm.TryChange<AlertState>(typeof(WatchState));
        }
    }

    private class WatchState : global::State
    {
        private BunnyAnimator bunnyAnimator;

        public WatchState(BunnyAnimator bunnyAnimator)
        {
            this.bunnyAnimator = bunnyAnimator;
        }

        public override void Enter()
        {
            bunnyAnimator.animator.SetInteger(bunnyAnimator.animatorStateHash, (int)State.Watch);
        }

        public override void Update()
        {

        }

        public override void Exit()
        {

        }

        public void OnIdle()
        {
            bunnyAnimator.fsm.TryChange<WatchState>(typeof(IdleState));
        }
        public void OnWalk()
        {
            bunnyAnimator.fsm.TryChange<WatchState>(typeof(WalkState));
        }

        public void OnAlert()
        {
            bunnyAnimator.fsm.TryChange<WatchState>(typeof(AlertState));
        }
    }
}
