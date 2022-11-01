using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class RailsBehaviour : MonoBehaviour
{
    private Animator animator;

    public GameObject cross3Ref;
    public GameObject cross4Ref;
    public GameObject straightRef;
    public GameObject turnRef;

    //private Pool cross3RailsPool;
    //private Pool cross4RailsPool;
    //private Pool straightRailsPool;
    //private Pool turnRailsPool;

    private int appearingParameter;
    private int hideParameter;

    public Rails currentRailsType;


    private void Awake()
    {
        animator = GetComponent<Animator>();

        appearingParameter = Animator.StringToHash("Appear");
        hideParameter = Animator.StringToHash("Hide");

        //cross3RailsPool = PoolManager.GetPoolByName("Cross3");
        //cross4RailsPool = PoolManager.GetPoolByName("Cross4");
        //straightRailsPool = PoolManager.GetPoolByName("Straight");
        //turnRailsPool = PoolManager.GetPoolByName("Turn");

        currentRailsType = Rails.None;
    }

    public void OnEnable()
    {
        LevelController.OnDisableEnvironment += Hide;
        animator.SetTrigger(appearingParameter);
    }

    public RailsBehaviour Init(Rails railsType, Vector3 rotation)
    {
        Init(railsType, rotation, null, false);

        return this;
    }

    public RailsBehaviour Init(Rails railsType, Vector3 rotation, Transform parrent)
    {
        Init(railsType, rotation, parrent, true);

        return this;
    }

    private void Init(Rails railsType, Vector3 rotation, Transform parrent, bool reinitParrent)
    {
        //if (railsRef != null)
        //{
        //    railsRef.transform.SetParent(null);
        //    railsRef.SetActive(false);
        //}

        DeactivateRailsGraphics();

        if (reinitParrent)
        {
            transform.SetParent(parrent);
        }

        currentRailsType = railsType;
        if (railsType == Rails.None)
            return;

        ActivateRailsGraphics(railsType);

        transform.eulerAngles = rotation;

        //railsRef = GetRails(railsType);

        //railsRef.transform.eulerAngles = rotation;
        //railsRef.transform.SetParent(transform);
    }

    private void ActivateRailsGraphics(Rails railsType)
    {
        if (railsType == Rails.Straight)
        {
            straightRef.SetActive(true);
        }
        else if (railsType == Rails.Turn)
        {
            turnRef.SetActive(true);
        }
        else if (railsType == Rails.Cross3)
        {
            cross3Ref.SetActive(true);
        }
        else if(railsType == Rails.Cross4)
        {
            cross4Ref.SetActive(true);
        }
    }

    public void DeactivateRailsGraphics()
    {
        cross3Ref.SetActive(false);
        cross4Ref.SetActive(false);
        straightRef.SetActive(false);
        turnRef.SetActive(false);
    }

    private void Hide()
    {
        animator.SetTrigger(hideParameter);
        LevelController.OnDisableEnvironment -= Hide;
    }

    private void OnDisable()
    {
        DeactivateRailsGraphics();
        LevelController.OnDisableEnvironment -= Hide;
    }
}