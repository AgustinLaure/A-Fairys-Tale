using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputHandler : MonoBehaviour
{
    public PlayerInput playerInput { get; private set; }

    private const string inputSystemActionsAssetHandle = "InputSystem_Actions";

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        playerInput.actions = Resources.Load<InputActionAsset>(inputSystemActionsAssetHandle);
    }
}
