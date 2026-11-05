using UnityEngine;

public class OnSceneChange : IEvent
{
    //public string newScene;

    public void Set(params object[] data)
    {
        //newScene = (string)data[0];

        //Debug.Log("Changed scene: to " + newScene);
    }

    public void Reset()
    {
        //newScene = "null";
    }
}