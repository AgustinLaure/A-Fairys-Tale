using UnityEngine;

public class TracePoint : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    public SpriteRenderer GetSpriteRenderer { get { return spriteRenderer; } }
}
