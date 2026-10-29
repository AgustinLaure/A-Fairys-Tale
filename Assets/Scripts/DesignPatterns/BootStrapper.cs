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

        //CreateMonoSingletonGameObject<CheatManager>();
    }

    public static T CreateMonoSingletonGameObject<T>() where T : MonoBehaviour
    {
        T existingInstance = UnityEngine.Object.FindFirstObjectByType<T>();
        if (existingInstance != null)
        {
            Debug.Log($"{typeof(T)} already exists in the scene. Skipping bootstrapper creation.");
            return existingInstance;
        }

        Debug.Log("Add MonoSingleton");

        GameObject gameObjectHandle = new GameObject();
        gameObjectHandle.name = typeof(T).ToString();
        UnityEngine.Object.DontDestroyOnLoad(gameObjectHandle);

        T component = gameObjectHandle.AddComponent<T>();

        return component;
    }
}
