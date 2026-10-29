using UnityEngine;

public class TracePoint : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private AreaTrigger areaTrigger;
    [SerializeField] private int id = 0;

    public int GetId { get { return id; } }

    public int SetId { set { id = value; } }

    public SpriteRenderer GetSpriteRenderer { get { return spriteRenderer; } }

    private void OnEnable()
    {
        spriteRenderer.enabled = true;
    }

    private void Awake()
    {
        areaTrigger.OnTriggerEntered += HandleAreaTrigger;
    }

    private void HandleAreaTrigger(Collider collider)
    {
        if (collider.CompareTag(Crane.CraneTag))
        {
            if (id < 0 || collider.GetComponentInParent<Crane>().GetCurrentId == id)
            {
                spriteRenderer.sprite = null;
                spriteRenderer.enabled = false;
            }
        }
    }

    private void OnDestroy()
    {
        areaTrigger.OnTriggerEntered -= HandleAreaTrigger;
    }
}
