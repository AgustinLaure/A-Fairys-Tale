using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Collision")]
    [SerializeField] private BoxCollider floorDetection;
    [SerializeField] private LayerMask groundLayer;

    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform pivot;
    [SerializeField] private PlayerHorizontalMovement playerHorizontalMovement;
    [SerializeField] private PlayerJump playerJump;
    [SerializeField] private PlayerPickUp playerPickUp;
    [SerializeField] private PlayerDrop playerDrop;
    [SerializeField] private PlayerUseProp playerUseProp;
    [SerializeField] private PlayerCraft playerCraft;
    private InputHandler inputHandle;
    private InputAction moveAction;

    private const float epsilon = 1e-05f;

    private EventBus eventBus;
    private FSM fsm;

    private const float fallStateChangeSpeed = 5f;

    private const ForceMode pushForceMode = ForceMode.Impulse;

    public State GetState { get { return fsm.GetCurrentState; } }


    private void Start()
    {
        ServiceLocator serviceLocator = ServiceLocator.Instance;

        eventBus = serviceLocator.GetService<EventBus>();

        eventBus.Subscribe<OnRotatePlayer>((Action<OnRotatePlayer>)HandlePlayerRotate);
        eventBus.Subscribe<OnPushPlayer>((Action<OnPushPlayer>)HandlePlayerPush);

        inputHandle = serviceLocator.GetService<InputHandler>();

        moveAction = inputHandle.playerInput.actions["Move"];

        IdleState idleState = new IdleState(this);
        idleState.actions = new List<MonoBehaviour>()
        {
            playerHorizontalMovement,
            playerJump,
            playerPickUp,
            playerDrop,
            playerUseProp,
            playerCraft
        };

        eventBus.Subscribe<OnPlayerPickUp>((Action<OnPlayerPickUp>)idleState.OnPickUp);
        eventBus.Subscribe<OnPlayerDrop>((Action)idleState.OnDrop);
        eventBus.Subscribe<OnPlayerToggleCraft>((Action<OnPlayerToggleCraft>)idleState.OnCraft);
        eventBus.Subscribe<OnPlayerAim>((Action)idleState.OnAircraftAim);
        eventBus.Subscribe<OnActionLockStateChanged>((Action<OnActionLockStateChanged>)idleState.OnActionLock);
        eventBus.Subscribe<OnPlayerIdlePlay>((Action)idleState.OnIdlePlay);

        MoveState moveState = new MoveState(this);
        moveState.actions = new List<MonoBehaviour>()
        {
        playerHorizontalMovement,
        playerJump
        };

        eventBus.Subscribe<OnPlayerPickUp>((Action<OnPlayerPickUp>)moveState.OnPickUp);
        eventBus.Subscribe<OnPlayerDrop>((Action)moveState.OnDrop);
        eventBus.Subscribe<OnActionLockStateChanged>((Action<OnActionLockStateChanged>)moveState.OnActionLock);
        eventBus.Subscribe<OnPlayerAim>((Action)moveState.OnAircraftAim);
        eventBus.Subscribe<OnPlayerToggleCraft>((Action<OnPlayerToggleCraft>)moveState.OnCraft);

        OnAirState onAirState = new OnAirState(this);
        onAirState.actions = new List<MonoBehaviour>()
        {
            playerHorizontalMovement
        };

        eventBus.Subscribe<OnActionLockStateChanged>((Action<OnActionLockStateChanged>)onAirState.OnActionLock);
        //eventBus.Subscribe<OnPlayerLandAnimFinished>((Action)onAirState.OnLand);

        ActionLockState actionLockState = new ActionLockState(this);
        actionLockState.actions = new List<MonoBehaviour>()
        {

        };

        eventBus.Subscribe<OnPlayerDropToIdleAnimFinished>((Action)actionLockState.OnDropFinished);
        eventBus.Subscribe<OnPlayerPickUpAnimFinished>((Action)actionLockState.OnPickUpFinished);
        eventBus.Subscribe<OnPlayerStoppedCrafting>((Action)actionLockState.OnPlayerStoppedCrafting);
        eventBus.Subscribe<OnActionLockStateChanged>((Action<OnActionLockStateChanged>)actionLockState.OnActionLock);

        CraftState craftState = new CraftState(this);
        craftState.actions = new List<MonoBehaviour>()
        {
            playerCraft
        };

        eventBus.Subscribe<OnPlayerStopCrafting>((Action)craftState.OnStopCrafting);

        AimState aimState = new AimState(this);
        aimState.actions = new List<MonoBehaviour>()
        {
            playerUseProp
        };

        eventBus.Subscribe<OnPlayerIdlePlay>((Action)aimState.OnAircraftLaunch);

        Dictionary<Type, State> states = new Dictionary<Type, State>()
        {
            [typeof(IdleState)] = idleState,
            [typeof(MoveState)] = moveState,
            [typeof(OnAirState)] = onAirState,
            [typeof(ActionLockState)] = actionLockState,
            [typeof(CraftState)] = craftState,
            [typeof(AimState)] = aimState
        };

        fsm = new FSM(states);

        fsm.SetInitialState(typeof(IdleState));
    }


    private void Update()
    {
        fsm.Update();
        //Debug.Log(fsm.GetCurrentState);
    }

    private void HandlePlayerPush(OnPushPlayer data)
    {
        rb.AddForce(data.push, pushForceMode);
    }

    private void HandlePlayerRotate(OnRotatePlayer data)
    {
        pivot.rotation = data.rotation;
    }

    public bool GetIsOnAir()
    {
        return !Physics.CheckBox(floorDetection.bounds.center, floorDetection.bounds.extents, floorDetection.transform.rotation, groundLayer);
    }

    private void OnDestroy()
    {

    }

    private class IdleState : State
    {
        private PlayerController playerController;
        private float horizontalInput = 0f;

        public IdleState(PlayerController playerController)
        {
            this.playerController = playerController;
        }

        public override void Enter()
        {
            playerController.playerPickUp.enabled = false;
            playerController.playerDrop.enabled = false;
            playerController.playerUseProp.enabled = false;
           // playerController.playerCraft.enabled = false;
        }

        public override void Update()
        {
            if (playerController.GetIsOnAir())
            {
                playerController.fsm.TryChange<IdleState>(typeof(OnAirState));
            }

            horizontalInput = playerController.moveAction.ReadValue<Vector2>().x;

            if (horizontalInput * horizontalInput > epsilon * epsilon)
            {
                playerController.fsm.TryChange<IdleState>(typeof(MoveState));
            }
        }

        public override void Exit()
        {

        }

        public void OnPickUp(OnPlayerPickUp data)
        {
            playerController.fsm.TryChange<IdleState>(typeof(ActionLockState));
        }

        public void OnDrop()
        {
            playerController.fsm.TryChange<IdleState>(typeof(ActionLockState));
        }

        public void OnCraft(OnPlayerToggleCraft data)
        {
            if (data.isCrafting)
            {
                playerController.fsm.TryChange<IdleState>(typeof(CraftState));
            }
        }

        public void OnAircraftAim()
        {
            playerController.fsm.TryChange<IdleState>(typeof(AimState));
        }

        public void OnActionLock(OnActionLockStateChanged data)
        {
            if (data.state)
            {
                playerController.fsm.TryChange<IdleState>(typeof(ActionLockState));
            }
        }

        public void OnIdlePlay()
        {
            playerController.playerPickUp.enabled = true;
            playerController.playerDrop.enabled = true;
            playerController.playerUseProp.enabled = true;
           // playerController.playerCraft.enabled = true;
        }
    }

    private class MoveState : State
    {
        private PlayerController playerController;
        private float horizontalInput = 0f;

        public MoveState(PlayerController playerController)
        {
            this.playerController = playerController;
        }

        public override void Enter()
        {

        }

        public override void Update()
        {
            if (playerController.GetIsOnAir())
            {
                playerController.fsm.TryChange<MoveState>(typeof(OnAirState));
            }

            horizontalInput = playerController.moveAction.ReadValue<Vector2>().x;

            if (horizontalInput * horizontalInput < epsilon * epsilon && Mathf.Abs(playerController.rb.linearVelocity.x) < epsilon && Mathf.Abs(playerController.rb.linearVelocity.y) < epsilon)
            {
                playerController.fsm.TryChange<MoveState>(typeof(IdleState));
            }
        }

        public override void Exit()
        {

        }

        public void OnPickUp(OnPlayerPickUp data)
        {
            playerController.fsm.TryChange<MoveState>(typeof(ActionLockState));
        }

        public void OnDrop()
        {
            playerController.fsm.TryChange<MoveState>(typeof(ActionLockState));
        }

        public void OnActionLock(OnActionLockStateChanged data)
        {
            if (data.state)
            {
                playerController.fsm.TryChange<MoveState>(typeof(ActionLockState));
            }
        }

        public void OnAircraftAim()
        {
            playerController.fsm.TryChange<MoveState>(typeof(AimState));
        }

        public void OnCraft(OnPlayerToggleCraft data)
        {
            if (data.isCrafting)
            {
                playerController.fsm.TryChange<MoveState>(typeof(CraftState));
            }
        }
    }

    private class OnAirState : State
    {
        private PlayerController playerController;

        public OnAirState(PlayerController playerController)
        {
            this.playerController = playerController;
        }

        public override void Enter()
        {

        }

        public override void Update()
        {
            if (!playerController.GetIsOnAir())
            {
                playerController.fsm.TryChange<OnAirState>(typeof(IdleState));
            }
        }

        public override void Exit()
        {

        }

        public void OnActionLock(OnActionLockStateChanged data)
        {
            if (data.state)
            {
                playerController.fsm.TryChange<OnAirState>(typeof(ActionLockState));
            }
        }

        //public void OnLand()
        //{
        //    if (Mathf.Abs(playerController.moveAction.ReadValue<Vector2>().x) > 0f)
        //    {
        //        playerController.fsm.TryChange<OnAirState>(typeof(MoveState));
        //    }
        //    else
        //    {
        //        playerController.fsm.TryChange<OnAirState>(typeof(IdleState));
        //    }
        //}
    }

    private class ActionLockState : State
    {
        private PlayerController playerController;

        public ActionLockState(PlayerController playerController)
        {
            this.playerController = playerController;
        }

        public override void Enter()
        {

        }

        public override void Update()
        {
            if (playerController.GetIsOnAir())
            {
                //playerController.fsm.TryChange<ActionLockState>(typeof(OnAirState));
            }
        }

        public override void Exit()
        {

        }

        public void OnPickUpFinished()
        {
            playerController.fsm.TryChange<ActionLockState>(typeof(IdleState));
        }

        public void OnDropFinished()
        {
            playerController.fsm.TryChange<ActionLockState>(typeof(IdleState));
        }

        public void OnPlayerStoppedCrafting()
        {
            playerController.fsm.TryChange<ActionLockState>(typeof(IdleState));
        }

        public void OnActionLock(OnActionLockStateChanged data)
        {
            if (!data.state)
            {
                playerController.fsm.TryChange<ActionLockState>(typeof(IdleState));
            }
        }
    }

    private class CraftState : State
    {
        private PlayerController playerController;

        public CraftState(PlayerController playerController)
        {
            this.playerController = playerController;
        }

        public override void Enter()
        {

        }

        public override void Update()
        {
            if (playerController.GetIsOnAir())
            {
                playerController.fsm.TryChange<CraftState>(typeof(OnAirState));
            }
        }

        public override void Exit()
        {

        }
        public void OnStopCrafting()
        {
            playerController.fsm.TryChange<CraftState>(typeof(ActionLockState));
        }
    }

    private class AimState : State
    {
        private PlayerController playerController;

        public AimState(PlayerController playerController)
        {
            this.playerController = playerController;
        }

        public override void Enter()
        {

        }

        public override void Update()
        {
            if (playerController.GetIsOnAir())
            {
                playerController.fsm.TryChange<AimState>(typeof(OnAirState));
            }
        }

        public override void Exit()
        {

        }

        public void OnAircraftLaunch()
        {
            playerController.fsm.TryChange<AimState>(typeof(IdleState));
        }
    }
}
