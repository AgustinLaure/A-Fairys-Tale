using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WaterFlowerAnimator : MonoBehaviour
{
    private enum State
    {
        Phase1,
        Phase1To2,
        Phase2,
        Phase2To1,
        Phase2To3,
        Phase3,
        Phase3To1
    }

    [Header("References")]
    [SerializeField] private Animator animator;

    [Header("Configs")]
    [SerializeField] private float phase1To2Time;
    [SerializeField] private float phase2To3Time;

    #region HashReferences

    private const string animatorStateVarName = "State";

    private const string phase1StateName = "Phase1";
    private const string phase1To2tateName = "Phase1To2";
    private const string phase2StateName = "Phase2";
    private const string phase2To1tateName = "Phase2To1";
    private const string phase2To3StateName = "Phase2To3";
    private const string phase3StateName = "Phase3";
    private const string phase3To2tateName = "Phase3To1";

    private Dictionary<State, int> statesAnimatorHash = new Dictionary<State, int>
    {
        [State.Phase1] = Animator.StringToHash(phase1StateName),
        [State.Phase1To2] = Animator.StringToHash(phase1To2tateName),
        [State.Phase2] = Animator.StringToHash(phase2StateName),
        [State.Phase2To1] = Animator.StringToHash(phase2To1tateName),
        [State.Phase2To3] = Animator.StringToHash(phase2To3StateName),
        [State.Phase3] = Animator.StringToHash(phase3StateName),
        [State.Phase3To1] = Animator.StringToHash(phase3To2tateName),
    };

    #endregion

    private EventBus eventBus;
    private FSM fsm;

    private int animatorStateHash = 0;

    public float GetPhase1To2Time { get { return phase1To2Time; } }
    public float GetPhase2To3Time { get { return phase2To3Time; } }


    private void Awake()
    {
        animatorStateHash = Animator.StringToHash(animatorStateVarName);
    }

    private void Start()
    {
        ServiceLocator serviceLocator = ServiceLocator.Instance;
        eventBus = serviceLocator.GetService<EventBus>();

        Phase1 phase1State = new Phase1(this);
        eventBus.Subscribe<OnWaterFlowerKnocked>((Action)phase1State.OnKnock);

        Phase1To2 phase1To2State = new Phase1To2(this);
        eventBus.Subscribe<OnWaterFlowerKnocked>((Action)phase1To2State.OnKnock);

        Phase2 phase2State = new Phase2(this);
        eventBus.Subscribe<OnWaterFlowerKnocked>((Action)phase2State.OnKnock);

        Phase2To1 phase2To1State = new Phase2To1(this);
        eventBus.Subscribe<OnWaterFlowerKnocked>((Action)phase2To1State.OnKnock);

        Phase2To3 phase2To3State = new Phase2To3(this);
        eventBus.Subscribe<OnWaterFlowerKnocked>((Action)phase2To3State.OnKnock);

        Phase3 phase3State = new Phase3(this);
        eventBus.Subscribe<OnWaterFlowerKnocked>((Action)phase3State.OnKnock);

        Phase3To1 phase3To1State = new Phase3To1(this);
        eventBus.Subscribe<OnWaterFlowerKnocked>((Action)phase3To1State.OnKnock);


        Dictionary<Type, global::State> states = new Dictionary<Type, global::State>()
        {
            [typeof(Phase1)] = phase1State,
            [typeof(Phase1To2)] = phase1To2State,
            [typeof(Phase2)] = phase2State,
            [typeof(Phase2To1)] = phase2To1State,
            [typeof(Phase2To3)] = phase2To3State,
            [typeof(Phase3)] = phase3State,
            [typeof(Phase3To1)] = phase3To1State,
        };

        fsm = new FSM(states);

        fsm.SetInitialState(typeof(Phase1));
    }

    private void Update()
    {
        fsm.Update();
    }

    private class Phase1 : global::State
    {
        private WaterFlowerAnimator birdAnimator;
        float timer = 0f;

        public Phase1(WaterFlowerAnimator birdAnimator)
        {
            this.birdAnimator = birdAnimator;
        }

        public override void Enter()
        {
            birdAnimator.animator.SetInteger(birdAnimator.animatorStateHash, (int)State.Phase1);
            timer = 0f;
        }

        public override void Update()
        {
            timer += Time.deltaTime;

            if (timer >= birdAnimator.GetPhase2To3Time)
            {
                birdAnimator.fsm.TryChange<Phase1>(typeof(Phase1To2));
            }
        }

        public override void Exit()
        {

        }

        public void OnKnock()
        {

        }
    }

    private class Phase1To2 : global::State
    {
        private WaterFlowerAnimator birdAnimator;

        public Phase1To2(WaterFlowerAnimator birdAnimator)
        {
            this.birdAnimator = birdAnimator;
        }

        public override void Enter()
        {
            birdAnimator.animator.SetInteger(birdAnimator.animatorStateHash, (int)State.Phase1To2);
        }

        public override void Update()
        {
            if (AnimationUtils.GetAnimationEnded(birdAnimator.animator, birdAnimator.statesAnimatorHash[State.Phase1To2]))
            {
                birdAnimator.fsm.TryChange<Phase1To2>(typeof(Phase2));
            }
        }

        public override void Exit()
        {

        }

        public void OnKnock()
        {
            birdAnimator.fsm.TryChange<Phase1To2>(typeof(Phase1));
        }
    }

    private class Phase2 : global::State
    {
        private WaterFlowerAnimator birdAnimator;
        private float timer = 0f;

        public Phase2(WaterFlowerAnimator birdAnimator)
        {
            this.birdAnimator = birdAnimator;
        }

        public override void Enter()
        {
            birdAnimator.animator.SetInteger(birdAnimator.animatorStateHash, (int)State.Phase2);
            timer = 0f;
        }

        public override void Update()
        {
            timer += Time.deltaTime;

            if (timer >= birdAnimator.GetPhase2To3Time)
            {
                birdAnimator.fsm.TryChange<Phase2>(typeof(Phase2To3));
            }
        }

        public override void Exit()
        {

        }
        public void OnKnock()
        {
            birdAnimator.fsm.TryChange<Phase2>(typeof(Phase2To1));
        }
    }

    private class Phase2To1 : global::State
    {
        private WaterFlowerAnimator birdAnimator;

        public Phase2To1(WaterFlowerAnimator birdAnimator)
        {
            this.birdAnimator = birdAnimator;
        }

        public override void Enter()
        {
            birdAnimator.animator.SetInteger(birdAnimator.animatorStateHash, (int)State.Phase2To1);
        }

        public override void Update()
        {
            if (AnimationUtils.GetAnimationEnded(birdAnimator.animator, birdAnimator.statesAnimatorHash[State.Phase2To1]))
            {
                birdAnimator.fsm.TryChange<Phase2To1>(typeof(Phase1));
            }
        }

        public override void Exit()
        {

        }
        public void OnKnock()
        {

        }
    }

    private class Phase2To3 : global::State
    {
        private WaterFlowerAnimator birdAnimator;

        public Phase2To3(WaterFlowerAnimator birdAnimator)
        {
            this.birdAnimator = birdAnimator;
        }

        public override void Enter()
        {
            birdAnimator.animator.SetInteger(birdAnimator.animatorStateHash, (int)State.Phase2To3);
        }

        public override void Update()
        {
            if (AnimationUtils.GetAnimationEnded(birdAnimator.animator, birdAnimator.statesAnimatorHash[State.Phase2To3]))
            {
                birdAnimator.fsm.TryChange<Phase2To3>(typeof(Phase3));
            }
        }

        public override void Exit()
        {

        }
        public void OnKnock()
        {
            birdAnimator.fsm.TryChange<Phase2To3>(typeof(Phase2To1));
        }
    }

    private class Phase3 : global::State
    {
        private WaterFlowerAnimator birdAnimator;

        public Phase3(WaterFlowerAnimator birdAnimator)
        {
            this.birdAnimator = birdAnimator;
        }

        public override void Enter()
        {
            birdAnimator.animator.SetInteger(birdAnimator.animatorStateHash, (int)State.Phase3);
            birdAnimator.eventBus.Raise<OnWaterFlowerFinalPhase>();
        }

        public override void Update()
        {

        }

        public override void Exit()
        {

        }
        public void OnKnock()
        {
            birdAnimator.fsm.TryChange<Phase3>(typeof(Phase3To1));
        }
    }

    private class Phase3To1 : global::State
    {
        private WaterFlowerAnimator birdAnimator;

        public Phase3To1(WaterFlowerAnimator birdAnimator)
        {
            this.birdAnimator = birdAnimator;
        }

        public override void Enter()
        {
            birdAnimator.animator.SetInteger(birdAnimator.animatorStateHash, (int)State.Phase3To1);
        }

        public override void Update()
        {
            if (AnimationUtils.GetAnimationEnded(birdAnimator.animator, birdAnimator.statesAnimatorHash[State.Phase3To1]))
            {
                birdAnimator.fsm.TryChange<Phase3To1>(typeof(Phase1));
            }
        }

        public override void Exit()
        {

        }
        public void OnKnock()
        {

        }
    }
}
