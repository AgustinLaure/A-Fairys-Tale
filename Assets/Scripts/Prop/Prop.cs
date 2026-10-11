using UnityEngine;

public abstract class Prop : MonoBehaviour, IPickable
{
    private bool isPickable = true;

    public bool GetIsPickable { get { return isPickable; } }

    private static string propTag = "Prop";

    public static string GetPropTag { get { return propTag; } }

    public virtual void Action()
    {

    }

    public virtual void Discard()
    {

    }

    public abstract void Enable();

    public abstract void Disable();

    public void SetHighlight(bool state)
    {

    }

    protected void EnableRigidBody(Rigidbody rb)
    {
        rb.isKinematic = false;
        rb.detectCollisions = true;
    }

    protected void DisableRigidBody(Rigidbody rb)
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.isKinematic = true;
        rb.detectCollisions = false;
    }
}
