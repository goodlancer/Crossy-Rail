#pragma warning disable 0649

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon;

public class CellBehaviour : MonoBehaviour
{
    #region Fields and Properties
    [SerializeField]
    private MeshRenderer meshRenderer;
    public RailsBehaviour railsBehaviourRef;

    [SerializeField, Space(5)]
    private Material groundMaterial;
    [SerializeField]
    private Material pathMaterial;
    [SerializeField]
    private Material staticGroundMaterial;
    [SerializeField]
    private Material invisibleMaterial;

    private GoldItemBehaviour goldItemRef;

    private Animator animator;
    private CellType type;
    private ExtraProperty extraProperty;

    private int finalAnimationParameter;
    private int moveLeftParameter;
    private int moveRightParameter;
    private int moveUpParameter;
    private int moveDownParameter;

    public enum RoadType
    {
        //Isolated,
        Straight,
        Turn,
        Cross3,
        Cross4,
    }

    public enum ExtraProperty
    {
        Normal = 0,
        Gold = 1,
        Mine = 2,
    }

    public CellType Type
    {
        get { return type; }
    }


    public bool IsMine
    {
        get { return extraProperty == ExtraProperty.Mine; }
    }

    public bool HasGoldBar
    {
        get { return extraProperty == ExtraProperty.Gold; }
    }


    public bool IsMovable
    {
        get { return (type == CellType.Movable || type == CellType.Rails) && extraProperty == ExtraProperty.Normal; }
    }

    //index in playground array
    public Index2 IndexPosition
    {
        get { return new Index2(Mathf.RoundToInt(transformRef.position.x), Mathf.RoundToInt(transformRef.position.z)); }
    }
    public Vector3 Position
    {
        get { return transformRef.position; }
    }

    private static CellBehaviour emptyCell;
    public static Index2 EmptyCellPosition
    {
        get { return emptyCell.IndexPosition; }
        set { emptyCell.transformRef.position = new Vector3(value.x, 0f, value.y); }
    }

    private Level level;
    private Transform transformRef;
    private Vector3 finalPosition;
    private Pool movableEnvironmentPool;
    private Pool staticEnvironmentPool;
    private Pool railsPool;
    private Pool goldItemsPool;
    private Pool minesPool;

    private Action onMoveCompleteAction;

    #endregion

    private void Awake()
    {
        transformRef = transform;

        animator = GetComponent<Animator>();
        finalAnimationParameter = Animator.StringToHash("Final");
        moveLeftParameter = Animator.StringToHash("MoveLeft");
        moveRightParameter = Animator.StringToHash("MoveRight");
        moveUpParameter = Animator.StringToHash("MoveUp");
        moveDownParameter = Animator.StringToHash("MoveDown");

        movableEnvironmentPool = PoolManager.GetPoolByName("MovableEnvironment");
        staticEnvironmentPool = PoolManager.GetPoolByName("StaticCellEnvironment");
        goldItemsPool = PoolManager.GetPoolByName("GoldItem");
        minesPool = PoolManager.GetPoolByName("MineObject");
        railsPool = PoolManager.GetPoolByName("RailsBehaviour");
    }

    public void Init(CellType cellType, ExtraProperty extraProp, Level levelRef)
    {
        LevelController.OnFinalAnimation += PlayFinalAnimation;
        transformRef.localScale = Vector3.one;

        level = levelRef;
        type = cellType;
        extraProperty = extraProp;

        meshRenderer.gameObject.SetActive(true);
        railsBehaviourRef.gameObject.SetActive(true);

        if (cellType == CellType.Movable)
        {
            railsBehaviourRef.Init(Rails.None, Vector3.zero, transformRef);

            meshRenderer.material = pathMaterial;
        }
        else if (cellType == CellType.Rails)
        {
            railsBehaviourRef.Init(Rails.None, Vector3.zero, transformRef);
            LevelController.OnDisableEnvironment += DisableRailObject;
            meshRenderer.material = pathMaterial;
        }
        else if (cellType == CellType.Empty)
        {
            railsBehaviourRef.Init(Rails.None, Vector3.zero);
            meshRenderer.material = invisibleMaterial;
            emptyCell = this;
        }
        else if (cellType == CellType.Static)
        {
            railsBehaviourRef.Init(Rails.None, Vector3.zero);
            meshRenderer.material = groundMaterial;

            Transform envTransform = staticEnvironmentPool.GetPooledObject(transformRef.position + new Vector3(0f, 0f, 0f)).transform;
            envTransform.SetParent(transformRef);
            envTransform.localEulerAngles = Vector3.up * UnityEngine.Random.Range(0, 4) * 90f;
        }

        if (extraProperty == ExtraProperty.Gold)
        {
            goldItemRef = goldItemsPool.GetPooledObject(transformRef.position).GetComponent<GoldItemBehaviour>();
            LevelController.GoldBarsAmount++;
        }

        if (extraProperty == ExtraProperty.Mine)
        {
            minesPool.GetPooledObject(transformRef.position).GetComponent<MineBehaviour>().Init(this);
            LevelController.HasMines = true;

            meshRenderer.gameObject.SetActive(false);
            railsBehaviourRef.gameObject.SetActive(false);
        }
    }

    public void Move(Index2 direction, Action onComplete)
    {
        animator.SetTrigger(GetMoveParameter(direction));

        finalPosition = transformRef.position + direction.ToVector3XZ();
        onMoveCompleteAction = onComplete;
    }

    public void UpdateRailsObject(Rails type, Vector3 rotation)
    {
        railsBehaviourRef.Init(type, rotation);
    }

    private int GetMoveParameter(Index2 direction)
    {
        if (direction == Index2.left)
        {
            return moveLeftParameter;
        }
        else if (direction == Index2.right)
        {
            return moveRightParameter;
        }
        else if (direction == Index2.up)
        {
            return moveUpParameter;
        }
        else
        {
            return moveDownParameter;
        }
    }

    public void OnMovementComplete()
    {
        onMoveCompleteAction?.Invoke();

        StartCoroutine(DelayedResetPosition());
    }

    private IEnumerator DelayedResetPosition()
    {
        yield return null;
        transformRef.position = new Vector3(Mathf.RoundToInt(finalPosition.x), 0f, Mathf.RoundToInt(finalPosition.z));
    }

    public void PlayUnderMineAnimation()
    {
        meshRenderer.material = invisibleMaterial;
        meshRenderer.gameObject.SetActive(true);
        PlayFinalAnimation();
    }

    public void EnableRenderer()
    {
        meshRenderer.gameObject.SetActive(true);
    }

    public void DisableRenderer()
    {
        meshRenderer.gameObject.SetActive(false);
    }

    private void DisableRailObject()
    {
        LevelController.OnDisableEnvironment -= DisableRailObject;
    }


    public void PlayFinalAnimation()
    {
        animator.SetTrigger(finalAnimationParameter);
        LevelController.OnFinalAnimation -= PlayFinalAnimation;
    }

    public void HighlightGoldItem()
    {
        goldItemRef.HighlightItem();
    }

    private void OnDisable()
    {
        LevelController.OnFinalAnimation -= PlayFinalAnimation;
        LevelController.OnDisableEnvironment -= DisableRailObject;
    }
}