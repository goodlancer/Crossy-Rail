#pragma warning disable 0649

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathHighlight : MonoBehaviour
{
    [SerializeField]
    private Renderer[] renderers;


    public Index2 IndexPosition
    {
        get { return new Index2((int)transform.position.x, (int)transform.position.z); }
    }


    private Animator animatorRef;
    private MaterialPropertyBlock propertyBlock;

    private int toLowerPositionProperty;

    private bool highlightState = false;

    public void Awake()
    {
        animatorRef = GetComponent<Animator>();
        propertyBlock = new MaterialPropertyBlock();

        toLowerPositionProperty = Animator.StringToHash("ToLowPosition");

        highlightState = true;
        Highlight(false);
    }

    public void Highlight(bool state)
    {
        if (highlightState != state)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor("_EmissionColor", state ? new Color(0.8f, 0.8f, 0.8f) : new Color(0.57f, 0.57f, 0.57f));
                renderers[i].SetPropertyBlock(propertyBlock);
            }

            highlightState = state;
        }
    }

    public void PutDown()
    {
        animatorRef.SetTrigger(toLowerPositionProperty);
    }

    public void Disable()
    {
        gameObject.SetActive(false);
    }
}