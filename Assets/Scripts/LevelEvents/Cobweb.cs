using UnityEngine;
using System.Collections.Generic;

public class Cobweb : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private List<BasicProp> items;

    private const string shakeAnimName = "Shake";
    private const string aircraftTag = "Aircraft";

    private void Awake()
    {

    }

    private void Start()
    {
        foreach (BasicProp item in items)
        {
            item.GetAreaCollider.OnColliderEntered += HandleAreaCollision;
        }
    }

    private void HandleAreaCollision(Collision collision)
    {
        if (collision.gameObject.CompareTag(aircraftTag))
        {
            animator.Play(shakeAnimName);

            ContactPoint contact = collision.GetContact(0);

            BasicProp collidedStick = contact.thisCollider.gameObject.GetComponentInParent<BasicProp>();

            collidedStick.GetAreaCollider.OnColliderEntered -= HandleAreaCollision;

            collidedStick.SetGravity(true);
            collidedStick.SetKinematic(false);

            items.Remove(collidedStick);
        }
    }
    private void OnDestroy()
    {
        foreach (BasicProp item in items)
        {
            item.GetAreaCollider.OnColliderEntered -= HandleAreaCollision;
        }
    }
}
