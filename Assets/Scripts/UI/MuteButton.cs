using UnityEngine;
using UnityEngine.UI;

public class MuteButton : MonoBehaviour
{
    [SerializeField] private Button button;
    private EventBus eventBus;
    private GameManager gameManager;

    [SerializeField] private Image image;

    [SerializeField] private Sprite unmutedImaged;
    [SerializeField] private Sprite mutedImaged;

    private void Awake()
    {
        button.onClick.AddListener(HandleClick);
    }

    private void Start()
    {
        ServiceLocator serviceLocator = ServiceLocator.Instance;

        eventBus = serviceLocator.GetService<EventBus>();
        gameManager = serviceLocator.GetService<GameManager>();

        image.sprite = gameManager.IsMuted ? mutedImaged : unmutedImaged;
    }

    private void HandleClick()
    {
        gameManager.HandleToggleMute();
        image.sprite = gameManager.IsMuted ? mutedImaged : unmutedImaged;
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(HandleClick);
    }
}
