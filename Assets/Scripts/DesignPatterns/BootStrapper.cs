using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BootStrapper
{
    private static ServiceLocator serviceLocator = ServiceLocator.Instance;


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Init()
    {
        serviceLocator.AddService(new CompositePool());
        serviceLocator.AddService(new EventBus());

        serviceLocator.AddServiceAsGameObject<InputHandler>(null);
        serviceLocator.AddServiceAsGameObject<GameManager>(null);

        CreateMonoSingletonGameObject<CheatManager>();
    }

    public static void CreateMonoSingletonGameObject<T>() where T : MonoBehaviour
    {
        Debug.Log("Add MonoSingleton");

        GameObject gameObjectHandle = new GameObject();
        gameObjectHandle.name = typeof(T).ToString();
        UnityEngine.Object.DontDestroyOnLoad(gameObjectHandle);

        gameObjectHandle.AddComponent<T>();

        return;
    }
}
