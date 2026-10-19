using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerToggleBookmark : MonoBehaviour
{
    private enum State
    {
        None,
        Unfold,
        Fold
    }

    #region Hashes

    private const string stateParameterName = "State";
    private const string unfoldAnimName = "Unfold";
    private const string foldAnimName = "Fold";

    private int stateHash = 0;

    private Dictionary<State, int> hashes = null;

    #endregion

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Image image;
    private InputHandler inputHandler;
    private InputAction toggleAction;

    [Header("Configs")]

    private FSM fsm;
    NoneState noneState = null;
    UnfoldState unfoldState = null;
    FoldState foldState = null;

    private const string toggleActionName = "ToggleBookmark";

    private void Awake()
    {
        stateHash = Animator.StringToHash(stateParameterName);

        hashes = new Dictionary<State, int>
        {
            [State.Unfold] = Animator.StringToHash(unfoldAnimName),
            [State.Fold] = Animator.StringToHash(foldAnimName),
        };
    }

    private void Start()
    {
        ServiceLocator serviceLocator = ServiceLocator.Instance;

        inputHandler = serviceLocator.GetService<InputHandler>();
        toggleAction = inputHandler.playerInput.actions[toggleActionName];

        noneState = new NoneState(this);
        toggleAction.performed += noneState.OnToggle;

        unfoldState = new UnfoldState(this);
        toggleAction.performed += unfoldState.OnToggle;

        foldState = new FoldState(this);

        Dictionary<Type, global::State> states = new Dictionary<Type, global::State>()
        {
            [typeof(NoneState)] = noneState,
            [typeof(UnfoldState)] = unfoldState,
            [typeof(FoldState)] = foldState,
        };

        fsm = new FSM(states);

        fsm.SetInitialState(typeof(NoneState));
    }

    private void Update()
    {
        fsm.Update();
    }

    private void OnDestroy()
    {
        toggleAction.performed -= noneState.OnToggle;
        toggleAction.performed -= unfoldState.OnToggle;
    }

    public class NoneState : global::State
    {
        private PlayerToggleBookmark bookmarkAnimator;

        public NoneState(PlayerToggleBookmark bookmarkAnimator)
        {
            this.bookmarkAnimator = bookmarkAnimator;
        }

        public override void Enter()
        {
            bookmarkAnimator.image.enabled = false;
            bookmarkAnimator.animator.SetInteger(bookmarkAnimator.stateHash, (int)PlayerToggleBookmark.State.None);
        }

        public override void Update()
        {

        }

        public override void Exit()
        {
            bookmarkAnimator.image.enabled = true;
        }

        public void OnToggle(InputAction.CallbackContext value)
        {
            bookmarkAnimator.fsm.TryChange<NoneState>(typeof(UnfoldState));
        }
    }

    public class UnfoldState : global::State
    {
        private PlayerToggleBookmark bookmarkAnimator;

        public UnfoldState(PlayerToggleBookmark bookmarkAnimator)
        {
            this.bookmarkAnimator = bookmarkAnimator;
        }

        public override void Enter()
        {
            bookmarkAnimator.animator.SetInteger(bookmarkAnimator.stateHash, (int)PlayerToggleBookmark.State.Unfold);
        }

        public override void Update()
        {

        }

        public override void Exit()
        {

        }
        public void OnToggle(InputAction.CallbackContext value)
        {
            if (AnimationUtils.GetAnimationEnded(bookmarkAnimator.animator, bookmarkAnimator.hashes[PlayerToggleBookmark.State.Unfold]))
            {
                bookmarkAnimator.fsm.TryChange<UnfoldState>(typeof(FoldState));
            }
        }
    }

    public class FoldState : global::State
    {
        private PlayerToggleBookmark bookmarkAnimator;

        public FoldState(PlayerToggleBookmark bookmarkAnimator)
        {
            this.bookmarkAnimator = bookmarkAnimator;
        }

        public override void Enter()
        {
            bookmarkAnimator.animator.SetInteger(bookmarkAnimator.stateHash, (int)PlayerToggleBookmark.State.Fold);
        }

        public override void Update()
        {
            if (AnimationUtils.GetAnimationEnded(bookmarkAnimator.animator, bookmarkAnimator.hashes[PlayerToggleBookmark.State.Fold]))
            {
                bookmarkAnimator.fsm.TryChange<FoldState>(typeof(NoneState));
            }
        }

        public override void Exit()
        {

        }
    }
}
