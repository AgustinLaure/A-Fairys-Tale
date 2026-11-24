using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PageTransition : MonoBehaviour
{
    [Header("Page Flip")]
    [SerializeField] private RectTransform coverTransform;
    [SerializeField] private float pageAnimationTime = 1.0f;
    [SerializeField] private float endAngle = -180f;

    [Header("Main zoom")]
    [SerializeField] private RectTransform mainTransform;
    [SerializeField] private float mainAnimationTime = 1.0f;

    [Header("Cover Slide")]
    [SerializeField] private float coverSlideOffsetX = -100f;
    [SerializeField] private float coverTargetScale = 1.25f;
    [SerializeField] private bool disableCoverAfterSlide = true;

    [Header("Transition")]
    [SerializeField] private float transitionWaitTime = 0.25f;

    private Action mainZoomAction;

    private bool activated = false;

    private void Start()
    {
        if (coverTransform == null)
        {
            Debug.LogError("No rect selected");
        }
    }

    private void OnEnable()
    {
        mainZoomAction += HandleMainZoom;
    }
    private void OnDisable()
    {
        mainZoomAction -= HandleMainZoom;
    }

    private void Update()
    {
        if (activated) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            StartCoroutine(PageFlipCoroutine());
            activated = true;
        }
    }

    private IEnumerator PageFlipCoroutine()
    {
        float elapsed = 0f;

        Quaternion startingQuat = coverTransform.localRotation;
        Quaternion endingQuat = Quaternion.Euler(0f, endAngle, 0f);

        while (elapsed < pageAnimationTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / pageAnimationTime);

            float easedT = Mathf.SmoothStep(0f, 1f, t);

            coverTransform.localRotation = Quaternion.Slerp(startingQuat, endingQuat, easedT);
            yield return null;
        }
        coverTransform.localRotation = endingQuat;

        yield return new WaitForSeconds(transitionWaitTime);

        mainZoomAction?.Invoke();
    }

    void HandleMainZoom()
    {
        StartCoroutine(MainZoomCoroutine());
    }

    private IEnumerator MainZoomCoroutine()
    {
        float elapsed = 0f;

        Vector3 startScale = new Vector3(mainTransform.localScale.x, mainTransform.localScale.y, 1f);
        Vector3 endScale = Vector3.one;

        float coverStartScale = coverTransform.localScale.x;
        Vector2 coverStartPos = coverTransform.anchoredPosition;
        Vector2 coverEndPos = coverStartPos + new Vector2(coverSlideOffsetX, 0f);

        while (elapsed < mainAnimationTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / mainAnimationTime);
            float easedT = Mathf.SmoothStep(0f, 1f, t);

            mainTransform.localScale = Vector3.Lerp(startScale, endScale, easedT);
            float coverScaleValue = Mathf.Lerp(coverStartScale, coverTargetScale, easedT);
            coverTransform.localScale = new Vector2(coverScaleValue, coverScaleValue);
            coverTransform.anchoredPosition = Vector2.Lerp(coverStartPos, coverEndPos, easedT);

            yield return null;
        }

        mainTransform.localScale = endScale;
        coverTransform.anchoredPosition = coverEndPos;

        if (disableCoverAfterSlide)
        {
            coverTransform.gameObject.SetActive(false);
        }
    }
}
