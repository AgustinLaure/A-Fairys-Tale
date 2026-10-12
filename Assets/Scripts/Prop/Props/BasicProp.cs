using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class BasicProp : Prop
{
    [Header("References")]
    [SerializeField] private GameObject rendererObject;
    [SerializeField] private GameObject contourObject;
    [SerializeField] private GameObject colliderObject;
    [SerializeField] private SpriteRenderer contourSpriteRenderer;
    private Material contourMat;

    [Header("Configs")]
    [SerializeField] private float highlightTime = 2f;
    [SerializeField] private float lowestThickness;
    [SerializeField] private float highestThickness = 4.62f;
    [SerializeField] private float minTimeToHighlight = 10f;
    [SerializeField] private float maxTimeToHighlight = 20f;

    private AreaCollider areaCollider;
    private Rigidbody rb;

    private const ForceMode pushForceMode = ForceMode.Impulse;

    public AreaCollider GetAreaCollider { get { return areaCollider; } }

    private Coroutine highlightCoroutine = null;

    private const string thicknessReference = "_Thickness";

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (gameObject.TryGetComponent<AreaCollider>(out AreaCollider collider))
        {
            areaCollider = collider;
        }
        else
        {
            areaCollider = gameObject.AddComponent<AreaCollider>();
        }
    }

    private void Start()
    {
        contourMat = contourSpriteRenderer.material;
    }

    private void Update()
    {
        if (rendererObject.activeSelf && lowestThickness < highestThickness)
        {
            if (highlightCoroutine == null)
            {
                highlightCoroutine = StartCoroutine(HighlightCoroutine(Random.Range(minTimeToHighlight, maxTimeToHighlight)));
            }
        }
    }

    private IEnumerator HighlightCoroutine(float timeToHighlight)
    {
        yield return new WaitForSeconds(timeToHighlight);

        contourObject.SetActive(true);

        float t = 0f;
        float currentThickness = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / highlightTime;
            currentThickness = Mathf.Lerp(lowestThickness, highestThickness, t);
            contourMat.SetFloat(thicknessReference, currentThickness);

            yield return null;
        }

        contourMat.SetFloat(thicknessReference, highestThickness);

        t = 1f;

        while (t > 0f)
        {
            t -= Time.deltaTime / highlightTime;
            currentThickness = Mathf.Lerp(lowestThickness, highestThickness, t);
            contourMat.SetFloat(thicknessReference, currentThickness);

            yield return null;
        }

        contourMat.SetFloat(thicknessReference, lowestThickness);

        highlightCoroutine = null;
    }

    public override void Enable()
    {
        EnableRigidBody(rb);
        rendererObject.SetActive(true);
        colliderObject.SetActive(true);
    }

    public void Push(Vector3 push, Vector3 torque)
    {
        if (push != Vector3.zero)
        {
            rb.AddForce(push, pushForceMode);
        }

        if (torque != Vector3.zero)
        {
            rb.AddTorque(torque, pushForceMode);
        }
    }

    public override void Disable()
    {
        DisableRigidBody(rb);
        rendererObject.SetActive(false);
        colliderObject.SetActive(false);
    }

    public void SetGravity(bool state)
    {
        rb.useGravity = state;
    }

    public void SetKinematic(bool state)
    {
        rb.isKinematic = state;
    }

    public void EnablePhysics()
    {
        EnableRigidBody(rb);
    }

    public void DisablePhysics()
    {
        DisableRigidBody(rb);
    }
}
