using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StepSequenceDisplay : MonoSingleton<StepSequenceDisplay>
{
    [Header("Behavior")]
    public bool keepCurrentStepFirst = true;
    public float slideSpeed = 8f;

    [Header("Layout Settings")]
    public float spacing = 2.5f;
    public float scaleMultiplier = 0.15f;

    [Header("Visual Settings")]
    public float lineWidth = 0.4f;
    public Material lineMaterial;
    public Material paperMaterial;
    public float fillAlphaMultiplier = 0.6f;

    [Header("Colors")]
    public Color pendingColor = new Color(0.5f, 0.5f, 0.5f, 0.75f);
    public Color activeColor = Color.white;
    public Color completedColor = new Color(0.25f, 0.25f, 0.25f, 0.25f);

    private class StepIconData
    {
        public Transform transform;
        public ShapeRenderer renderer;
    }

    private List<StepIconData> stepIcons = new List<StepIconData>();
    private List<Vector3> targetPositions = new List<Vector3>();

    private void Update()
    {
        for (int i = 0; i < stepIcons.Count; i++)
        {
            if (stepIcons[i].transform != null)
            {
                stepIcons[i].transform.localPosition = Vector3.Lerp(
                    stepIcons[i].transform.localPosition,
                    targetPositions[i],
                    Time.deltaTime * slideSpeed
                );
            }
        }
    }

    public void RenderSequence(OrigamiFigure figure, int currentObjectiveIndex)
    {
        if (figure == null) return;

        int visualStepCount = figure.steps.Count(s => !s.flipStep);

        bool isInitialSetup = false;

        if (stepIcons.Count != visualStepCount)
        {
            ClearSequence();
            isInitialSetup = true;

            for (int i = 0; i < figure.steps.Count; i++)
            {
                if (figure.steps[i].flipStep) continue;

                GameObject gameObject = new GameObject($"StepIcon_{stepIcons.Count}");
                gameObject.transform.SetParent(transform);
                gameObject.transform.localScale = Vector3.one * scaleMultiplier;

                ShapeRenderer shapeRenderer = gameObject.AddComponent<ShapeRenderer>();
                shapeRenderer.Setup(paperMaterial, lineMaterial, lineWidth * scaleMultiplier);

                shapeRenderer.DrawShape(figure.steps[i].targetShape, null, 0.01f);

                stepIcons.Add(new StepIconData { transform = gameObject.transform, renderer = shapeRenderer });
                targetPositions.Add(Vector3.zero);
            }
        }

        int activeIconIndex = 0;

        for (int i = 0; i < currentObjectiveIndex && i < figure.steps.Count; i++)
        {
            if (!figure.steps[i].flipStep) activeIconIndex++;
        }

        float totalWidth = (stepIcons.Count - 1) * spacing;
        float defaultStartX = -totalWidth / 2f;

        for (int i = 0; i < stepIcons.Count; i++)
        {
            float targetX = keepCurrentStepFirst
                ? (i - activeIconIndex) * spacing
                : defaultStartX + (i * spacing);

            targetPositions[i] = new Vector3(targetX, 0, 0);

            if (isInitialSetup)
            {
                stepIcons[i].transform.localPosition = targetPositions[i];
            }

            Color stateColor;
            if (i < activeIconIndex) stateColor = completedColor;
            else if (i == activeIconIndex) stateColor = activeColor;
            else stateColor = pendingColor;

            stepIcons[i].renderer.SetColor(stateColor, fillAlphaMultiplier);
        }
    }

    public void ClearSequence()
    {
        foreach (var icon in stepIcons)
        {
            if (icon.transform != null) Destroy(icon.transform.gameObject);
        }
        stepIcons.Clear();
        targetPositions.Clear();
    }
}