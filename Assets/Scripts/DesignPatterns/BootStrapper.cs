
using System;
using UnityEditor;
using UnityEngine;

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

        serviceLocator.AddServiceFromPrefab<MusicPlayer>(Resources.Load<GameObject>("MusicManager"), true);

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

    public static void OnReset()
    {
        EventBus eventBus = serviceLocator.GetService<EventBus>();
        MusicPlayer musicPlayer = serviceLocator.GetService<MusicPlayer>();

        serviceLocator.GetService<CompositePool>().Clear();
        eventBus.Clear();

        musicPlayer.OnRestart();
    }
}
