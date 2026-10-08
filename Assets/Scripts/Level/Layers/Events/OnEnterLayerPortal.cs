using UnityEngine;

public class OnEnterLayerPortal : IEvent
{
    private Teletransportable tp;
    private Transform target;

    public void Set(params object[] data)
    {
        tp = (Teletransportable)data[0];
        target = (Transform)data[1];
        Debug.LogWarning("EnterPortalEvent: tp " + tp + " target " + target );
    }
    public void Reset()
    {
    }
}