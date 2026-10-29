using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance = null;
    [SerializeField] bool dontDestroyOnLoad = true;

    public static T Instance
    {
        get { return instance; }
    }

    private void Awake()
    {
        if (instance == null || instance == this)
        {
            instance = this as T;

            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }

            OnAwaken();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
            OnDestroyed();
        }
    }

    protected virtual void OnAwaken()
    {

    }

    protected virtual void OnDestroyed()
    {

    }
}