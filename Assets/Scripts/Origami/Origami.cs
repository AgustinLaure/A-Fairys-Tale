using System.Collections;
using UnityEngine;

public abstract class Origami : Prop
{
    [Header("OrigamiRefs")]
    [SerializeField] protected GameObject origamiRenderer;
    [SerializeField] protected AreaCollider areaCollider;
    [SerializeField] protected SphereCollider sphereCollider;
    [SerializeField] protected GameObject baseForm;
    [SerializeField] private GameObject crumpledForm;
    [SerializeField] private Animator animator;
    protected GameObject currentLayerGameObject = null;

    protected int baseLayer = 0;
    protected int noPlayerColLayer = 0;

    protected string crumpleClipName;

    protected Camera mainCamera;
    protected Vector2 cursorPosition = Vector2.zero;

    protected bool isCrumpled = false;

    private Coroutine crumpleCoroutine = null;
    protected float crumpleColliderSize = 1f;

    private const string waterTag = "Water";

    private const string crumpleClipNameConst = "Crumple";

    public GameObject SetCurrentLayerGameObject { get { return currentLayerGameObject; } set { currentLayerGameObject = value; } }

    public static string GetOrigamiTag { get { return "Origami"; } }

    protected override void Start()
    {
        base.Start();

        crumpleClipName = crumpleClipNameConst;

        mainCamera = Camera.main;

        areaCollider.OnColliderEntered += HandleAreaCollision;
    }

    protected virtual void Crumple()
    {
        if (crumpleCoroutine == null)
        {
            crumpleCoroutine = StartCoroutine(CrumpleCoroutine());
        }
    }

    private IEnumerator CrumpleCoroutine()
    {
        isCrumpled = true;

        animator.Play(crumpleClipName);

        yield return new WaitUntil(() => AnimationUtils.GetCurrentAnimationNormalizedTime(animator) >= 1f);

        sphereCollider.GetComponent<SphereCollider>().radius = crumpleColliderSize;

        baseForm.SetActive(false);
        crumpledForm.SetActive(true);

        sphereCollider.gameObject.layer = baseLayer;

        crumpleCoroutine = null;
    }

    protected void AddToCurrentLayer()
    {
        if (currentLayerGameObject != null)
        {
            transform.SetParent(currentLayerGameObject.transform, true);
        }
        else
        {
            transform.SetParent(null, true);
        }
    }

    protected Vector3 GetMouseWorldPosition()
    {
        Vector2 screenPos = cursorPosition;

        Ray ray = mainCamera.ScreenPointToRay(screenPos);

        Plane currentLevelPlane = new Plane(Vector3.forward, new Vector3(0, 0, transform.position.z));

        if (currentLevelPlane.Raycast(ray, out float enterDistance))
        {
            return ray.GetPoint(enterDistance);
        }

        return Vector3.zero;
    }

    private void HandleAreaCollision(Collision collision)
    {
        if (collision.gameObject.CompareTag(waterTag))
        {
            Crumple();
        }
    }

    protected virtual void OnDestroy()
    {
        areaCollider.OnColliderEntered -= HandleAreaCollision;
    }
}
