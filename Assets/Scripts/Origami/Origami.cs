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

    protected bool isCrumpled = false;

    private Coroutine crumpleCoroutine = null;
    protected float crumpleColliderSize = 1f;

    public GameObject SetCurrentLayerGameObject { get { return currentLayerGameObject; } set { currentLayerGameObject = value; } }

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
}
