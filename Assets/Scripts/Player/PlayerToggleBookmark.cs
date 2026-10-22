using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerToggleBookmark : MonoBehaviour
{
    private enum State
    {
        None = -1,
        Unfold,
        Fold
    }

    #region Hashes

    private const string stateParameterName = "State";
    private const string oneUnfoldAnimName = "OneUnfold";
    private const string oneFoldAnimName = "OneFold";
    private const string twoUnfoldAnimName = "TwoUnfold";
    private const string twoFoldAnimName = "TwoFold";

    private int stateHash = 0;

    private int[] hashes = null;

    #endregion

    [Header("References")]
    [SerializeField] private Animator animator;
    private InputHandler inputHandler;
    private InputAction toggleAction;
    private GameManager gameManager = null;

    [Header("Configs")]

    private FSM fsm;
    NoneState noneState = null;
    UnfoldState unfoldState = null;
    FoldState foldState = null;

    private const string toggleActionName = "ToggleBookmark";

    private void Awake()
    {
        stateHash = Animator.StringToHash(stateParameterName);

        hashes = new int[]
        {
            Animator.StringToHash(oneUnfoldAnimName),
            Animator.StringToHash(oneFoldAnimName),
            Animator.StringToHash(twoUnfoldAnimName),
            Animator.StringToHash(twoFoldAnimName),
        };
    }

    private void Start()
    {
        ServiceLocator serviceLocator = ServiceLocator.Instance;

        gameManager = serviceLocator.GetService<GameManager>();

        inputHandler = serviceLocator.GetService<InputHandler>();
        toggleAction = inputHandler.playerInput.actions[toggleActionName];

        int hashesAdd = 0;
        hashesAdd = (int)((GameplayLevelData)gameManager.levelData).level;

        noneState = new NoneState(this);
        toggleAction.performed += noneState.OnToggle;

        unfoldState = new UnfoldState(this, hashes[2 * hashesAdd], hashesAdd);
        toggleAction.performed += unfoldState.OnToggle;

        foldState = new FoldState(this, hashes[2 * hashesAdd + 1], hashesAdd);

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
            bookmarkAnimator.animator.SetInteger(bookmarkAnimator.stateHash, (int)PlayerToggleBookmark.State.None);
        }

        public override void Update()
        {

        }

        public override void Exit()
        {

        }

        public void OnToggle(InputAction.CallbackContext value)
        {
            bookmarkAnimator.fsm.TryChange<NoneState>(typeof(UnfoldState));
        }
    }

    public class UnfoldState : global::State
    {
        private PlayerToggleBookmark bookmarkAnimator;
        private int animHash = 0;
        private int hashesAdd = 0;

        public UnfoldState(PlayerToggleBookmark bookmarkAnimator, int animHash, int hashAdd)
        {
            this.bookmarkAnimator = bookmarkAnimator;
            this.animHash = animHash;
            this.hashesAdd = hashAdd;
        }

        public override void Enter()
        {
            bookmarkAnimator.animator.SetInteger(bookmarkAnimator.stateHash, 2 * hashesAdd);
        }

        public override void Update()
        {

        }

        public override void Exit()
        {

        }
        public void OnToggle(InputAction.CallbackContext value)
        {
            if (AnimationUtils.GetAnimationEnded(bookmarkAnimator.animator, animHash))
            {
                bookmarkAnimator.fsm.TryChange<UnfoldState>(typeof(FoldState));
            }
        }
    }

    public class FoldState : global::State
    {
        private PlayerToggleBookmark bookmarkAnimator;
        private int animHash = 0;
        private int hashesAdd = 0;

        public FoldState(PlayerToggleBookmark bookmarkAnimator, int animHash, int hashAdd)
        {
            this.bookmarkAnimator = bookmarkAnimator;
            this.animHash = animHash;
            this.hashesAdd = hashAdd;
        }

        public override void Enter()
        {
            bookmarkAnimator.animator.SetInteger(bookmarkAnimator.stateHash, 2 * hashesAdd + 1);
        }

        public override void Update()
        {
            if (AnimationUtils.GetAnimationEnded(bookmarkAnimator.animator, animHash))
            {
                bookmarkAnimator.fsm.TryChange<FoldState>(typeof(NoneState));
            }
        }

        public override void Exit()
        {

        }
    }
}
