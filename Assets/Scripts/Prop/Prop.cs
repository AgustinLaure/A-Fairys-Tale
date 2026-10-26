using UnityEngine;

public abstract class Prop : MonoBehaviour, IPickable
{
    [SerializeField] protected GameObject contourObject;
    [SerializeField] protected SpriteRenderer contourSpriteRenderer;
    [SerializeField] protected float contourLowestThickness;
    [SerializeField] protected float contourHighestThickness = 4.62f;
    [SerializeField] protected Rigidbody rb;

    protected Material contourMat;

    private bool isPickable = true;
    protected bool isHighlighted = false;

    public bool GetIsPickable { get { return isPickable; } }

    public virtual bool SetIsHighlighted { set { isHighlighted = value; } }

    private static string propTag = "Prop";

    protected const string contourThicknessReference = "_Thickness";

    public static string GetPropTag { get { return propTag; } }

    protected virtual void Start()
    {
        contourMat = contourSpriteRenderer.material;
    }

    public virtual void Action()
    {

    }

    public virtual void Discard()
    {

    }

    public abstract void Enable();

    public abstract void Disable();

    public virtual void SetHighlight(bool state)
    {
        float thickness = state ? contourHighestThickness : contourLowestThickness;
        contourObject.SetActive(state);
        contourMat.SetFloat(contourThicknessReference, thickness);
        isHighlighted = state;
    }

    public void EnableRigidBody()
    {
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
        }
    }

    public void DisableRigidBody()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.isKinematic = true;
            rb.detectCollisions = false;
        }
    }
}
