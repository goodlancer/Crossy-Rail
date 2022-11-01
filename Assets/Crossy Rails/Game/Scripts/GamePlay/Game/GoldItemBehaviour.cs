using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class GoldItemBehaviour : MonoBehaviour
{
    private Animator animator;

    private int collectParameter;
    private int highlightParameter;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        collectParameter = Animator.StringToHash("Collect");
        highlightParameter = Animator.StringToHash("Highlight");
    }

    private void OnEnable()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Player"))
        {
            StartCoroutine(GoldPickAnimationCoroutine(other.transform));
        }
    }

    private IEnumerator GoldPickAnimationCoroutine(Transform playerTransform)
    {
        animator.SetTrigger(collectParameter);

        bool distanceDecreasing = true;
        float prevDistance = 1000f;

        while (distanceDecreasing)
        {
            float dist = Vector3.SqrMagnitude(transform.position - playerTransform.transform.position);
            
            distanceDecreasing = dist < prevDistance;
            prevDistance = dist;

            yield return null;
        }

        transform.SetParent(playerTransform.transform);
    }

    public void HighlightItem()
    {
        animator.SetTrigger(highlightParameter);
    }
}