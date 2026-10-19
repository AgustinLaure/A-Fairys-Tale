using System;
using System.Collections.Generic;
using UnityEngine;

public class BirdAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    private enum State
    {
        Idle,
        IdleToWet,
        Wet,
        Flapping
    }

    #region HashReferences

    private const string animatorStateVarName = "State";

    private const string idleStateName = "Idle";
    private const string idleToWetStateName = "IdleToWet";
    private const string wetStateName = "Wet";
    private const string flappingStateName = "Flapping";

    private Dictionary<State, int> statesAnimatorHash = new Dictionary<State, int>
    {
        [State.Idle] = Animator.StringToHash(idleStateName),
        [State.IdleToWet] = Animator.StringToHash(idleToWetStateName),
        [State.Wet] = Animator.StringToHash(wetStateName),
        [State.Flapping] = Animator.StringToHash(flappingStateName),
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
        eventBus.Subscribe<OnWaterFlowerFinalPhase>((Action)idleState.OnWet);
        eventBus.Subscribe<OnBirdFlap>((Action)idleState.OnFlap);

        IdleToWetState idleToWetState = new IdleToWetState(this);

        WetState wetState = new WetState(this);

        FlappingState flappingState = new FlappingState(this);

        Dictionary<Type, global::State> states = new Dictionary<Type, global::State>()
        {
            [typeof(IdleState)] = idleState,
            [typeof(IdleToWetState)] = idleToWetState,
            [typeof(WetState)] = wetState,
            [typeof(FlappingState)] = flappingState,
        };

        fsm = new FSM(states);

        fsm.SetInitialState(typeof(IdleState));
    }

    private class IdleState : global::State
    {
        private BirdAnimator birdAnimator;

        public IdleState(BirdAnimator birdAnimator)
        {
            this.birdAnimator = birdAnimator;
        }

        public override void Enter()
        {
            birdAnimator.animator.SetInteger(birdAnimator.animatorStateHash, (int)State.Idle);
        }

        public override void Update()
        {

        }

        public override void Exit()
        {

        }

        public void OnWet()
        {
            birdAnimator.fsm.TryChange<IdleState>(typeof(IdleToWetState));
        }

        public void OnFlap()
        {
            birdAnimator.fsm.TryChange<IdleState>(typeof(FlappingState));
        }
    }

    private class IdleToWetState : global::State
    {
        private BirdAnimator birdAnimator;

        public IdleToWetState(BirdAnimator birdAnimator)
        {
            this.birdAnimator = birdAnimator;
        }

        public override void Enter()
        {
            birdAnimator.animator.SetInteger(birdAnimator.animatorStateHash, (int)State.IdleToWet);
        }

        public override void Update()
        {
            if (AnimationUtils.GetAnimationEnded(birdAnimator.animator, birdAnimator.statesAnimatorHash[State.IdleToWet]))
            {
                birdAnimator.fsm.TryChange<IdleToWetState>(typeof(WetState));
            }
        }

        public override void Exit()
        {

        }
    }

    private class WetState : global::State
    {
        private BirdAnimator birdAnimator;

        public WetState(BirdAnimator birdAnimator)
        {
            this.birdAnimator = birdAnimator;
        }

        public override void Enter()
        {
            birdAnimator.animator.SetInteger(birdAnimator.animatorStateHash, (int)State.Wet);
        }

        public override void Update()
        {

        }

        public override void Exit()
        {

        }
    }

    private class FlappingState : global::State
    {
        private BirdAnimator birdAnimator;

        public FlappingState(BirdAnimator birdAnimator)
        {
            this.birdAnimator = birdAnimator;
        }

        public override void Enter()
        {
            birdAnimator.animator.SetInteger(birdAnimator.animatorStateHash, (int)State.Flapping);
        }

        public override void Update()
        {
            if (AnimationUtils.GetAnimationEnded(birdAnimator.animator, birdAnimator.statesAnimatorHash[State.Flapping]))
            {
                birdAnimator.fsm.TryChange<FlappingState>(typeof(IdleState));
            }
        }

        public override void Exit()
        {

        }
    }
}
