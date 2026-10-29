using UnityEngine;
using System.Collections.Generic;

public class Trace : MonoBehaviour
{
    private const int maxPoints = 2;

    [Header("References")]
    [SerializeField] private GameObject rendererGameObject;
    [SerializeField] private Transform startGameObject;
    [SerializeField] private Transform endGameObject;
    [SerializeField] private GameObject pointPrefab;

    [Header("Configs")]
    [SerializeField] private float pointRadiusMult;
    [SerializeField] private float pointDistance;
    [SerializeField] private bool isFirstVisible;

    private List<TracePoint> points = new List<TracePoint>();

    public Vector3 SetStart
    {
        set
        {
            startGameObject.position = value;
            UpdateTrace();
        }
    }

    public Vector3 SetEnd
    {
        set
        {
            endGameObject.position = value;
            UpdateTrace();
        }
    }

    public void SetPoints(Vector3 start, Vector3 end)
    {
        startGameObject.transform.position = start;
        endGameObject.transform.position = end;

        UpdateTrace();
    }

    public void SetVisibility(int index, bool state)
    {
        points[index].gameObject.SetActive(state);
    }

    public void SetLayer()
    {

    }

    public void SetPointsAlpha(float alpha)
    {
        foreach (TracePoint tracePoint in points)
        {
            SpriteRenderer spriteRenderer = tracePoint.GetSpriteRenderer;

            Color newColor = spriteRenderer.color;
            newColor.a = alpha;

            spriteRenderer.color = newColor;
        }
    }

    private void Start()
    {
        if (startGameObject.position != Vector3.zero || endGameObject.position != Vector3.zero)
        {
            UpdateTrace();
        }
    }

    public void SetVisibility(bool state)
    {
        rendererGameObject.SetActive(state);
    }

    private void UpdateTrace()
    {
        if (points.Count > 0)
        {
            foreach (TracePoint point in points)
            {
                Destroy(point.gameObject);
            }

            points.Clear();
        }

        Vector3 endPosition = endGameObject.position;

        Vector3 direction = Vector3.Normalize(endPosition - startGameObject.position);

        Vector3 currentPoint = startGameObject.position;

        Vector3 up = Vector3.Cross(Vector3.forward, direction);

        while (Vector3.Dot(endPosition, direction) > Vector3.Dot(currentPoint, direction))
        {
            GameObject currentPointObject = Instantiate(pointPrefab, rendererGameObject.transform);

            currentPointObject.transform.localScale = new Vector3(pointRadiusMult, pointRadiusMult, pointRadiusMult);
            currentPointObject.transform.position = currentPoint;
            currentPointObject.transform.rotation = Quaternion.LookRotation(Vector3.forward, up);

            currentPoint += direction * pointDistance;

            points.Add(currentPointObject.GetComponent<TracePoint>());
        }

        if (points.Count > 0)
        {
            points[0].enabled = isFirstVisible;
        }

        endGameObject.transform.localScale = new Vector3(pointRadiusMult, pointRadiusMult, pointRadiusMult);
        endGameObject.transform.rotation = Quaternion.LookRotation(Vector3.forward, up);
    }
}
