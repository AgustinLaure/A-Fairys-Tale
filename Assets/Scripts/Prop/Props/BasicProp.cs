using System.Collections;
using UnityEngine;

public class BasicProp : Prop
{
    [Header("References")]
    [SerializeField] private GameObject rendererObject;
    [SerializeField] private GameObject colliderObject;

    [Header("Configs")]
    [SerializeField] private float highlightTime = 2f;
    [SerializeField] private float minTimeToHighlight = 10f;
    [SerializeField] private float maxTimeToHighlight = 20f;

    private AreaCollider areaCollider;

    private const ForceMode pushForceMode = ForceMode.Impulse;

    public AreaCollider GetAreaCollider { get { return areaCollider; } }

    private Coroutine highlightCoroutine = null;


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

    private void Update()
    {
        if (rendererObject.activeSelf && contourLowestThickness < contourHighestThickness)
        {
            if (highlightCoroutine == null && contourSpriteRenderer.enabled)
            {
                highlightCoroutine = StartCoroutine(HighlightCoroutine(Random.Range(minTimeToHighlight, maxTimeToHighlight)));
            }
        }

        if (!contourSpriteRenderer.enabled)
        {
            if (highlightCoroutine != null)
            {
                StopCoroutine(highlightCoroutine);
                highlightCoroutine = null;
            }

            contourMat.SetFloat(contourThicknessReference, contourLowestThickness);
        }
    }

    public override void SetHighlight(bool state)
    {
        base.SetHighlight(state);

        if (highlightCoroutine != null)
        {
            StopCoroutine(highlightCoroutine);
            highlightCoroutine = null;
        }
    }
    private IEnumerator HighlightCoroutine(float timeToHighlight)
    {
        yield return new WaitForSeconds(timeToHighlight);

        isHighlighted = true;

        contourSpriteRenderer.enabled = true;
        //contourObject.SetActive(true);

        float t = 0f;
        float currentThickness = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / highlightTime;
            currentThickness = Mathf.Lerp(contourLowestThickness, contourHighestThickness, t);
            contourMat.SetFloat(contourThicknessReference, currentThickness);

            yield return null;
        }

        contourMat.SetFloat(contourThicknessReference, contourHighestThickness);

        t = 1f;

        while (t > 0f)
        {
            t -= Time.deltaTime / highlightTime;
            currentThickness = Mathf.Lerp(contourLowestThickness, contourHighestThickness, t);
            contourMat.SetFloat(contourThicknessReference, currentThickness);

            yield return null;
        }

        contourMat.SetFloat(contourThicknessReference, contourLowestThickness);

        isHighlighted = false;

        highlightCoroutine = null;
    }

    public override void Enable()
    {
        EnableRigidBody();
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
        DisableRigidBody();
        rendererObject.SetActive(false);
        contourSpriteRenderer.enabled = false;
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
        EnableRigidBody();
    }

    public void DisablePhysics()
    {
        DisableRigidBody();
    }
}
