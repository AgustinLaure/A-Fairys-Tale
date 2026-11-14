using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    [SerializeField] AK.Wwise.Event music;
    private static MusicPlayer instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        music.Post(gameObject);
    }
}
