using System.Collections.Generic;
using UnityEngine;

public class StepSequenceDisplay : MonoSingleton<StepSequenceDisplay>
{
    [Header("Layout Settings")]
    public float spacing = 2.5f;
    public float scaleMultiplier = 0.15f;

    [Header("Line Settings")]
    public float lineWidth = 0.4f;
    public Material lineMaterial;

    [Header("Colors")]
    public Color pendingColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    public Color activeColor = Color.white;
    public Color completedColor = Color.green;

    private List<ShapeBorderRenderer> stepIcons = new List<ShapeBorderRenderer>();

    public void RenderSequence(OrigamiFigure figure, int currentIndex)
    {
        if (figure == null) return;

        if (stepIcons.Count != figure.steps.Count)
        {
            ClearSequence();

            float totalWidth = (figure.steps.Count - 1) * spacing;
            float startX = -totalWidth / 2f;

            for (int i = 0; i < figure.steps.Count; i++)
            {
                GameObject go = new GameObject($"StepIcon_{i}");
                go.transform.SetParent(transform);

                go.transform.localPosition = new Vector3(startX + (i * spacing), 0, 0);
                go.transform.localScale = Vector3.one * scaleMultiplier;

                ShapeBorderRenderer sbr = go.AddComponent<ShapeBorderRenderer>();
                sbr.Setup(lineWidth * scaleMultiplier, lineMaterial);

                if (figure.steps[i].flipStep)
                    sbr.DrawShape(new List<Vector2> { new Vector2(0, 5), new Vector2(-5, 0), new Vector2(0, -5), new Vector2(5, 0) });
                else
                    sbr.DrawShape(figure.steps[i].targetShape);

                stepIcons.Add(sbr);
            }
        }

        for (int i = 0; i < stepIcons.Count; i++)
        {
            if (i < currentIndex) stepIcons[i].SetColor(completedColor);
            else if (i == currentIndex) stepIcons[i].SetColor(activeColor);
            else stepIcons[i].SetColor(pendingColor);
        }
    }

    public void ClearSequence()
    {
        foreach (var icon in stepIcons)
        {
            if (icon != null) Destroy(icon.gameObject);
        }
        stepIcons.Clear();
    }
}