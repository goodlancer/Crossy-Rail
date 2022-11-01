using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon;

public class DelayedObjectDisabler : MonoBehaviour
{
    public float delay;

    private void OnEnable()
    {
        StartCoroutine(DelayedDisable(delay));
    }

    private IEnumerator DelayedDisable(float delay)
    {
        yield return new WaitForSeconds(delay);

        gameObject.SetActive(false);
    }
}
