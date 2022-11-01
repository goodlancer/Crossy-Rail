using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnvironmentBehaviour : MonoBehaviour
{
    protected Animator animator;
    protected int appearParameter;
    protected int hideParameter;

    protected virtual void Awake()
    {
        appearParameter = Animator.StringToHash("Appear");
        hideParameter = Animator.StringToHash("Hide");
        animator = GetComponent<Animator>();
    }

    protected virtual void OnEnable()
    {
        StartCoroutine(DelayedAppearingAnimation());

        LevelController.OnDisableEnvironment += OnEnvironmentHide;
    }

    protected virtual void OnDisable()
    {
        LevelController.OnDisableEnvironment -= OnEnvironmentHide;
    }

    protected virtual void OnDestroy()
    {
        LevelController.OnDisableEnvironment -= OnEnvironmentHide;
    }

    protected IEnumerator DelayedAppearingAnimation()
    {
        yield return new WaitForSeconds(Random.Range(0f, 0.7f));

        animator.SetTrigger(appearParameter);
    }

    private void OnEnvironmentHide()
    {
        animator.SetTrigger(hideParameter);
    }
}