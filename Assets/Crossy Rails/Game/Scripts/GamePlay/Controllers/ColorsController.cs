using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon.Core;

public class ColorsController : MonoBehaviour
{
    private static ColorsController instance;

    [Header("Settings")]
    public Color groundColor;
    public Color pathColor;
    public Color staticColor;

    [Header("References")]
    public Material groundMaterial;
    public Material pathCellMaterial;
    public Material staticCellMaterial;
    public Material invisibleCellMaterial;

    void Awake()
    {
        instance = this;
        invisibleCellMaterial.color = groundColor.SetAlpha(0f);
        groundMaterial.color = pathCellMaterial.color = staticCellMaterial.color = groundColor;
    }

    public static void AppearingAnimation(float time, Action onComplete)
    {
        instance.StartCoroutine(instance.AppearingAnimationCoroutine(time,onComplete));
    }

    private IEnumerator AppearingAnimationCoroutine(float time,Action onComplete)
    {
        float state = 0f;
        float timer = 0f;

        while (timer <= time)
        {
            state = timer / time;

            pathCellMaterial.color = Color.Lerp(groundColor, pathColor, state);
            staticCellMaterial.color = Color.Lerp(groundColor, staticColor, state);
            invisibleCellMaterial.color = groundColor.SetAlpha(1f - state);

            timer += Time.deltaTime;
            yield return null;
        }

        pathCellMaterial.color = Color.Lerp(groundColor, pathColor, 1f);
        staticCellMaterial.color = Color.Lerp(groundColor, staticColor, 1f);
        invisibleCellMaterial.color = groundColor.SetAlpha(0f);

        onComplete?.Invoke();
    }

    public static void HideAnimation(float time)
    {
        instance.StartCoroutine(instance.HideAnimationCoroutine(time));
    }

    private IEnumerator HideAnimationCoroutine(float time)
    {
        float state = 0f;
        float timer = 0f;

        while (timer <= time)
        {
            state = timer / time;

            pathCellMaterial.color = Color.Lerp(pathColor, groundColor, state);
            staticCellMaterial.color = Color.Lerp(staticColor, groundColor, state);
            invisibleCellMaterial.color = groundColor.SetAlpha(state);

            timer += Time.deltaTime;

            yield return null;
        }

        pathCellMaterial.color = Color.Lerp(pathColor, groundColor, 1f);
        staticCellMaterial.color = Color.Lerp(staticColor, groundColor, 1f);
        invisibleCellMaterial.color = groundColor.SetAlpha(1f);
    }
}