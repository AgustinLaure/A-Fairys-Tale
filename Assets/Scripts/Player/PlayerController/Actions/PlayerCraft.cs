using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerCraft : MonoBehaviour
{
    public enum Move
    {
        None = -1,
        Up,
        Down,
        Left,
        Right
    }

    private const int maxRecipes = 2;

    [Header("References")]
    [SerializeField] private Transform pivotTransform;
    [SerializeField] private Transform origamiSpawnPointTRS;
    private GameObject currentLayerGameObject;
    private OrigamiPool origamiPool;
    private ServiceLocator serviceLocator;

    private InputHandler inputHandler;
    private InputAction toggleCraftAction;
    private InputAction craftMovesAction;
    private InputAction dropAction;

    private EventBus eventBus;

    [Header("Config")]
    [SerializeField] private const int maxPossibleMoves = 7;
    [SerializeField] private Move[] aicraftRecipe;
    [SerializeField] private Move[] frogRecipe;

    private struct Recipe
    {
        public Move[] moves;
        public Type type;
    }

    private Recipe[] recipes = new Recipe[maxRecipes];

    private Move[] cachedMoves = new Move[maxPossibleMoves];
    private int cachedMovesPointer = 0;

    private bool isCrafting = false;

    private static string origamiPoolName = "OrigamiPool";

    private void Awake()
    {
        recipes[0].moves = aicraftRecipe;
        recipes[0].type = typeof(Aircraft);

        recipes[1].moves = frogRecipe;
        recipes[1].type = typeof(Frog);
    }

    private void Start()
    {
        serviceLocator = ServiceLocator.Instance;

        serviceLocator.AddServiceFromPrefab<OrigamiPool>(Resources.Load<GameObject>(origamiPoolName), false);

        origamiPool = serviceLocator.GetService<OrigamiPool>();

        eventBus = serviceLocator.GetService<EventBus>();
        inputHandler = serviceLocator.GetService<InputHandler>();

        toggleCraftAction = inputHandler.playerInput.actions["ToggleCraft"];
        craftMovesAction = inputHandler.playerInput.actions["CraftMoves"];
        dropAction = inputHandler.playerInput.actions["Drop"];

        eventBus.Subscribe<OnPlayerReparented>((Action<OnPlayerReparented>)HandlePlayerReparent);

        toggleCraftAction.performed += OnToggleCraft;
        craftMovesAction.performed += OnCraftMoves;
        dropAction.performed += OnDrop;
    }

    private void OnToggleCraft(InputAction.CallbackContext value)
    {
        if (enabled)
        {
            isCrafting = !isCrafting;

            eventBus.Raise<OnPlayerToggleCraft>(isCrafting);

            ResetCache();
        }
    }

    private void ResetCache()
    {
        for (int i = 0; i < maxPossibleMoves; i++)
        {
            cachedMoves[i] = Move.None;
        }

        eventBus.Raise<OnPlayerUpdateCraftMoves>(cachedMoves);

        cachedMovesPointer = 0;
    }

    private void OnCraftMoves(InputAction.CallbackContext value)
    {
        if (isCrafting)
        {
            Vector2 axis = value.ReadValue<Vector2>();

            Move move = Move.None;

            if (axis.x > 0f)
            {
                move = Move.Right;
            }
            else if (axis.x < 0f)
            {
                move = Move.Left;
            }
            else if (axis.y > 0f)
            {
                move = Move.Up;
            }
            else if (axis.y < 0f)
            {
                move = Move.Down;
            }

            AddMove(move);
        }
    }

    private void OnDrop(InputAction.CallbackContext value)
    {
        if (isCrafting)
        {
            ResetCache();
        }
    }

    private bool AddMove(Move move)
    {
        if (cachedMovesPointer >= maxPossibleMoves)
        {
            return false;
        }

        cachedMoves[cachedMovesPointer] = move;
        cachedMovesPointer++;

        for (int i = 0; i < maxRecipes; i++)
        {
            if (MatchesRecipe(recipes[i].moves, cachedMoves))
            {
                GameObject origami = origamiPool.GetOrigami(recipes[i].type, origamiSpawnPointTRS.position, pivotTransform.rotation, pivotTransform);

                Origami origamiComp = origami.GetComponent<Origami>();
                origamiComp.SetCurrentLayerGameObject = currentLayerGameObject;

                eventBus.Raise<OnPlayerCraftedOrigami>(origamiComp);

                isCrafting = false;
                ResetCache();

                break;
            }
        }

        eventBus.Raise<OnPlayerUpdateCraftMoves>(cachedMoves);

        return true;
    }

    private bool MatchesRecipe(Move[] recipe, Move[] moves)
    {
        int movesLength = 0;

        foreach (Move move in moves)
        {
            if (move != Move.None)
            {
                movesLength++;
            }
        }

        if (recipe.Length != movesLength)
        {
            return false;
        }

        bool matches = true;

        for (int i = 0; i < recipe.Length; i++)
        {
            if (recipe[i] != moves[i])
            {
                matches = false;
            }
        }

        return matches;
    }

    private void HandlePlayerReparent(OnPlayerReparented data)
    {
        currentLayerGameObject = data.currentLayer;
    }

    private void OnDisable()
    {
      if (isCrafting)
      {
          eventBus.Raise<OnPlayerToggleCraft>(false);
          isCrafting = false;
          ResetCache();
      }
    }

    private void OnDestroy()
    {
        serviceLocator.RemoveService<OrigamiPool>();

        toggleCraftAction.performed -= OnToggleCraft;
        craftMovesAction.performed -= OnCraftMoves;
        dropAction.performed -= OnDrop;
    }
}
