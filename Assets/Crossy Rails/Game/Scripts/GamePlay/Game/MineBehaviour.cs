using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineBehaviour : EnvironmentBehaviour
{
    public static MineBehaviour mineOneRef;
    public static MineBehaviour mineTwoRef;

    private CellBehaviour cellRef;

    public static Index2 FirstMineIndex
    {
        get { return new Index2(Mathf.RoundToInt(mineOneRef.transform.position.x), Mathf.RoundToInt(mineOneRef.transform.position.z)); }
    }

    public static Index2 SecondMineIndex
    {
        get { return new Index2(Mathf.RoundToInt(mineTwoRef.transform.position.x), Mathf.RoundToInt(mineTwoRef.transform.position.z)); }
    }

    public Index2 IndexPosition
    {
        get { return new Index2(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z)); }
    }

    protected override void Awake()
    {
        base.Awake();

        if (mineOneRef == null)
        {
            mineOneRef = this;
        }
        else if (mineTwoRef == null)
        {
            mineTwoRef = this;
        }
    }

    public void Init(CellBehaviour cellBehaviourRef)
    {
        cellRef = cellBehaviourRef;
    }

    protected override void OnEnable()
    {
        StartCoroutine(DelayedAppearingAnimation());

        LevelController.OnDisableEnvironment += OnEnvironmentHide;
    }

    public void UpdateRotation(Vector3 rotation)
    {
        transform.localEulerAngles = rotation;
    }

    private void OnEnvironmentHide()
    {
        animator.SetTrigger(hideParameter);

        if (cellRef != null)
        {
            cellRef.PlayUnderMineAnimation();
        }
    }
}