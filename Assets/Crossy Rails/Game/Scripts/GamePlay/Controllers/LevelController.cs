using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon;
using Watermelon.Core;

public class LevelController : MonoBehaviour
{
    private static LevelController instance;

    [Header("Settings")]
    public int levelsOffsetZ = 2;

    [Header("Touch settins")]
    public float minSwipeLength = 35f;
    public float minSwipeSpeed = 10f;

    [Header("References")]
    public Transform topGroundTransform;
    public Transform bottomGroundTransform;
    public Transform rightGroundTransform;
    public Transform leftGroundTransform;

    public delegate void CellEvent();
    public static event CellEvent OnFinalAnimation;
    public static event CellEvent OnDisableEnvironment;

    private Level level;
    private Pool cellsPool;
    private Pool movableEnvironment;
    private Pool staticEnvironment;
    private Pool railsPool;
    private Pool goldItemsPool;
    private Pool minesPool;

    private List<List<CellBehaviour>> playgroud;
    private List<List<int>> pathMatrix;
    private List<Vector3> playerMovePath;
    private List<Vector3> playerStartPath;
    private List<Vector3> playerFinalPath;

    private bool isTouchActive = false;
    private bool hasMines = false;
    private bool minesPassed = false;
    private int goldBarsAmount = 0;
    private int goldBarsPicked = 0;

    private Vector3 prevInput;
    private Vector3 startInput;
    private Vector3 firstMinePosition;

    private Swipe swipeControls;

    public static bool HasMines
    {
        get { return instance.hasMines; }
        set { instance.hasMines = value; }
    }

    public static int GoldBarsAmount
    {
        get { return instance.goldBarsAmount; }
        set { instance.goldBarsAmount = value; }
    }

    private void Awake()
    {
        instance = this;

        cellsPool = PoolManager.GetPoolByName("Cell");
        movableEnvironment = PoolManager.GetPoolByName("MovableEnvironment");
        staticEnvironment = PoolManager.GetPoolByName("StaticCellEnvironment");
        goldItemsPool = PoolManager.GetPoolByName("GoldItem");
        minesPool = PoolManager.GetPoolByName("MineObject");
        railsPool = PoolManager.GetPoolByName("RailsBehaviour");


        swipeControls = gameObject.AddComponent<Swipe>();
    }

    #region LevelLoading

    public static void Load(Level levelToLoad)
    {
        instance.hasMines = false;
        instance.minesPassed = false;
        instance.goldBarsAmount = 0;
        instance.goldBarsPicked = 0;

        instance.level = levelToLoad;
        instance.LoadLevel();

        instance.isTouchActive = true;

        instance.CheckForLevelComplete();
    }

    private void LoadLevel()
    {
        playgroud = new List<List<CellBehaviour>>();
        playerMovePath = new List<Vector3>();
        cellsPool.ReturnToPoolEverything();
        movableEnvironment.ReturnToPoolEverything();
        staticEnvironment.ReturnToPoolEverything();
        railsPool.ReturnToPoolEverything();
        goldItemsPool.ReturnToPoolEverything(true);
        minesPool.ReturnToPoolEverything();

        int xLength = level.itemsShuffled.Length;
        int zLength = level.itemsShuffled[0].ints.Length;

        for (int i = 0; i < xLength; i++)
        {
            List<CellBehaviour> currentLine = new List<CellBehaviour>();
            for (int j = 0; j < zLength; j++)
            {
                CellBehaviour newCell = cellsPool.GetPooledObject(new Vector3(i, 0f, j)).GetComponent<CellBehaviour>();
                newCell.Init((CellType)level.itemsShuffled[i].ints[j], (CellBehaviour.ExtraProperty)level.extraProps[i].ints[j], level);

                currentLine.Add(newCell);
            }

            playgroud.Add(currentLine);
        }

        UpdatePathCellsGraphics();
        SpawnEnvironment();
        ColorsController.AppearingAnimation(0.2f, () =>
        {
            playgroud[CellBehaviour.EmptyCellPosition.x][CellBehaviour.EmptyCellPosition.y].DisableRenderer();
        });

        OnLevelLoaded();
    }

    private void SpawnEnvironment()
    {
        playerStartPath = new List<Vector3>();
        playerFinalPath = new List<Vector3>();
        int biggerLevelSide = level.size.y > level.size.x ? level.size.y : level.size.x;
        int topEnvironmentSize = 3 + biggerLevelSide;
        int bottomEnvironmentSize = biggerLevelSide > 3 ? biggerLevelSide : 4;
        int sideEnvironmentSize = 3;

        topGroundTransform.localScale = new Vector3(level.size.x, 1f, topEnvironmentSize);
        topGroundTransform.position = new Vector3(level.size.x * 0.5f - 0.5f, -0.501f, level.size.y - 0.5f + topGroundTransform.localScale.z * 0.5f);

        bottomGroundTransform.localScale = new Vector3(level.size.x, 1f, bottomEnvironmentSize);
        bottomGroundTransform.position = new Vector3(level.size.x * 0.5f - 0.5f, -0.501f, -0.5f - bottomGroundTransform.localScale.z * 0.5f);

        rightGroundTransform.localScale = new Vector3(sideEnvironmentSize, 1f, topGroundTransform.localScale.z + bottomGroundTransform.localScale.z + level.size.y);
        rightGroundTransform.position = new Vector3(level.size.x - 0.5f + rightGroundTransform.localScale.x * 0.5f, -0.501f, level.size.y * 0.5f + 1f);

        leftGroundTransform.localScale = new Vector3(sideEnvironmentSize, 1f, topGroundTransform.localScale.z + bottomGroundTransform.localScale.z + level.size.y);
        leftGroundTransform.position = new Vector3(-0.5f - leftGroundTransform.localScale.x * 0.5f, -0.501f, level.size.y * 0.5f + 1f);

        // bottom path and environment
        Index2 pathPosition = level.start + Index2.down;
        for (int i = 0; i < bottomEnvironmentSize; i++)
        {
            // path
            Index2 firstOccupiedPosition = pathPosition;
            RailsBehaviour railsBehaviour = railsPool.GetPooledObject(pathPosition.ToVector3XZ()).GetComponent<RailsBehaviour>().Init(Rails.Straight, new Vector3(0f, 0f, 0f), null);
            playerStartPath.Add(pathPosition.ToVector3XZ());

            // path turn
            if (Random.Range(0f, 1f) < 0.3f && i != 0)
            {
                if (Random.Range(0, 2) == 0)
                {
                    railsBehaviour.Init(Rails.Turn, new Vector3(0f, 270f, 0f), null);

                    pathPosition += Index2.left;
                    railsPool.GetPooledObject(pathPosition.ToVector3XZ()).GetComponent<RailsBehaviour>().Init(Rails.Turn, new Vector3(0f, 90f, 0f), null);

                    playerStartPath.Add(pathPosition.ToVector3XZ());
                }
                else
                {
                    railsBehaviour.Init(Rails.Turn, new Vector3(0f, 0f, 0f), null);

                    pathPosition += Index2.right;
                    railsPool.GetPooledObject(pathPosition.ToVector3XZ()).GetComponent<RailsBehaviour>().Init(Rails.Turn, new Vector3(0f, 180f, 0f), null);

                    playerStartPath.Add(pathPosition.ToVector3XZ());
                }
            }

            // environment
            for (int j = -sideEnvironmentSize; j < level.size.x + sideEnvironmentSize; j++)
            {
                if (j != firstOccupiedPosition.x && j != pathPosition.x)
                {
                    if (Random.Range(0f, 1f) < 0.6f)
                    {
                        Transform envTransform = (Random.Range(0, 2) == 0 ? movableEnvironment : staticEnvironment).GetPooledObject(new Vector3(j, 0f, pathPosition.y)).transform;
                        envTransform.SetParent(null);
                        envTransform.localEulerAngles = Vector3.up * UnityEngine.Random.Range(0, 4) * 90f;
                    }
                }
            }

            pathPosition += Index2.down;
        }
        playerStartPath.Reverse();


        // top path and environment
        pathPosition = level.finish + Index2.up;
        for (int i = 0; i < topEnvironmentSize; i++)
        {
            // path
            Index2 firstOccupiedPosition = pathPosition;
            RailsBehaviour railsBehaviour = railsPool.GetPooledObject(pathPosition.ToVector3XZ()).GetComponent<RailsBehaviour>().Init(Rails.Straight, new Vector3(0f, 0f, 0f), null);
            playerFinalPath.Add(pathPosition.ToVector3XZ());

            // path turn
            if (Random.Range(0f, 1f) < 0.3f)
            {
                if (Random.Range(0, 2) == 0)
                {
                    railsBehaviour.Init(Rails.Turn, new Vector3(0f, 180f, 0f), null);

                    pathPosition += Index2.left;
                    railsPool.GetPooledObject(pathPosition.ToVector3XZ()).GetComponent<RailsBehaviour>().Init(Rails.Turn, new Vector3(0f, 0f, 0f), null);
                    playerFinalPath.Add(pathPosition.ToVector3XZ());
                }
                else
                {
                    railsBehaviour.Init(Rails.Turn, new Vector3(0f, 90f, 0f), null);

                    pathPosition += Index2.right;
                    railsPool.GetPooledObject(pathPosition.ToVector3XZ()).GetComponent<RailsBehaviour>().Init(Rails.Turn, new Vector3(0f, 270f, 0f), null);
                    playerFinalPath.Add(pathPosition.ToVector3XZ());
                }
            }


            // environment
            for (int j = -sideEnvironmentSize; j < level.size.x + sideEnvironmentSize; j++)
            {
                if (j != firstOccupiedPosition.x && j != pathPosition.x)
                {
                    if (Random.Range(0f, 1f) < 0.6f)
                    {
                        Transform envTransform = (Random.Range(0, 2) == 0 ? movableEnvironment : staticEnvironment).GetPooledObject(new Vector3(j, 0f, pathPosition.y)).transform;
                        envTransform.SetParent(null);
                        envTransform.localEulerAngles = Vector3.up * UnityEngine.Random.Range(0, 4) * 90f;
                    }
                }
            }

            pathPosition += Index2.up;
        }

        // side environment
        for (int i = -sideEnvironmentSize; i < level.size.x + sideEnvironmentSize; i++)
        {
            for (int j = 0; j < level.size.y; j++)
            {
                if ((i < 0 || i >= level.size.x))
                {
                    if (Random.Range(0f, 1f) < 0.6f)
                    {
                        Transform envTransform = (Random.Range(0, 2) == 0 ? movableEnvironment : staticEnvironment).GetPooledObject(new Vector3(i, 0f, j)).transform;
                        envTransform.SetParent(null);
                        envTransform.localEulerAngles = Vector3.up * UnityEngine.Random.Range(0, 4) * 90f;
                    }
                }
            }
        }
    }

    private void UpdatePathCellsGraphics()
    {
        int xLength = level.size.x;
        int zLength = level.size.y;

        for (int i = 0; i < xLength; i++)
        {
            for (int j = 0; j < zLength; j++)
            {
                if (playgroud[i][j].Type == CellType.Rails)
                {
                    UpdateGraphicsForCell(new Index2(i, j));
                }
            }
        }
    }

    private void UpdateGraphicsForCell(Index2 index)
    {
        if (index.x < 0 || index.x >= level.size.x || index.y < 0 || index.y >= level.size.y || playgroud[index.x][index.y].Type != CellType.Rails)
            return;

        int i = index.x;
        int j = index.y;
        int neighboursCount = 0;
        bool top = false;
        bool right = false;
        bool bottom = false;
        bool left = false;


        if ((j < level.size.y - 1 && playgroud[i][j + 1].Type == CellType.Rails) || new Index2(i, j) == level.finish)
        {
            neighboursCount++;
            top = true;
        }

        if (i < level.size.x - 1 && playgroud[i + 1][j].Type == CellType.Rails)
        {
            neighboursCount++;
            right = true;
        }

        if ((j > 0 && playgroud[i][j - 1].Type == CellType.Rails) || new Index2(i, j) == level.start)
        {
            neighboursCount++;
            bottom = true;
        }

        if (i > 0 && playgroud[i - 1][j].Type == CellType.Rails)
        {
            neighboursCount++;
            left = true;
        }

        if (neighboursCount == 0 || neighboursCount == 4)
        {
            playgroud[i][j].UpdateRailsObject(Rails.Cross4, Vector3.zero);
        }

        if (neighboursCount == 2)
        {
            // straight road check
            if (top && bottom)
            {
                playgroud[i][j].UpdateRailsObject(Rails.Straight, Vector3.zero);
            }
            else if (right && left)
            {
                playgroud[i][j].UpdateRailsObject(Rails.Straight, Vector3.zero.SetY(90));
            }
            else
            {
                // turn check
                Vector3 rotation = Vector3.zero;

                if (right && bottom)
                {
                    rotation = rotation.SetY(90f);
                }
                else if (bottom && left)
                {
                    rotation = rotation.SetY(180f);
                }
                else if (left && top)
                {
                    rotation = rotation.SetY(270f);
                }

                playgroud[i][j].UpdateRailsObject(Rails.Turn, rotation);
            }
        }

        if (neighboursCount == 3)
        {
            Vector3 rotation = Vector3.zero;

            if (!top)
            {
                rotation = rotation.SetY(90f);
            }
            else if (!right)
            {
                rotation = rotation.SetY(180f);
            }
            else if (!bottom)
            {
                rotation = rotation.SetY(270f);
            }

            playgroud[i][j].UpdateRailsObject(Rails.Cross3, rotation);
        }

        if (neighboursCount == 1)
        {
            if (top || bottom)
            {
                playgroud[i][j].UpdateRailsObject(Rails.Straight, Vector3.zero);
            }
            else if (right || left)
            {
                playgroud[i][j].UpdateRailsObject(Rails.Straight, Vector3.zero.SetY(90));
            }
        }
    }

    private void OnLevelLoaded()
    {
        Vector3 levelCenter = new Vector3(level.size.x * 0.5f - 0.5f, 0f, level.size.y * 0.5f - 0.5f);
        CameraController.Init(levelCenter, level.size, GameController.CurrentLevelNumber != 1);

        PlayerController.Initialize(playerStartPath);

        if (hasMines)
        {
            UIController.instance.ShowMinesTutorialPanel();
        }

        if (goldBarsAmount > 0)
        {
            UIController.instance.ShowGoldTutorialPanel();
        }
    }

    #endregion

    #region Input and Movement

    private void Update()
    {
        if (!isTouchActive)
            return;

        if (swipeControls.SwipeRight)
        {
            OnSwipe(new Index2(1, 0));
        }
        else if (swipeControls.SwipeLeft)
        {
            OnSwipe(new Index2(-1, 0));
        }
        else if (swipeControls.SwipeTop)
        {
            OnSwipe(new Index2(0, 1));
        }
        else if (swipeControls.SwipeBottom)
        {
            OnSwipe(new Index2(0, -1));
        }

#if UNITY_EDITOR

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            OnSwipe(Index2.up);
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            OnSwipe(Index2.right);
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            OnSwipe(Index2.down);
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            OnSwipe(Index2.left);
        }
#endif
    }

    private void ResetStartTouchPostion()
    {
        startInput = Input.mousePosition;
        prevInput = Input.mousePosition;
    }

    private void OnSwipe(Index2 direction)
    {
        ResetStartTouchPostion();

        if (IsMoveAlowed(direction) && isTouchActive)
        {
            isTouchActive = false;
            Move(direction);
        }

#if UNITY_EDITOR
        UIController.instance.HideMenu();
#endif
    }

    private bool IsMoveAlowed(Index2 direction)
    {
        Index2 movableItemIndex = CellBehaviour.EmptyCellPosition - direction;

        if (movableItemIndex.x >= 0 && movableItemIndex.x < level.size.x && movableItemIndex.y >= 0 && movableItemIndex.y < level.size.y)
        {
            return playgroud[movableItemIndex.x][movableItemIndex.y].IsMovable;
        }

        return false;
    }

    private void Move(Index2 direction)
    {
        Index2 emptyCellPos = CellBehaviour.EmptyCellPosition;
        Index2 movingItemPos = emptyCellPos - direction;

        CellBehaviour movingItem = playgroud[movingItemPos.x][movingItemPos.y];
        playgroud[movingItemPos.x][movingItemPos.y] = playgroud[emptyCellPos.x][emptyCellPos.y];
        playgroud[emptyCellPos.x][emptyCellPos.y] = movingItem;

        movingItem.Move(direction, OnCellMoved);
        CellBehaviour.EmptyCellPosition = movingItemPos;

        UpdateGraphicsForCell(emptyCellPos + direction);
        UpdateGraphicsForCell(emptyCellPos + direction * -2);

        Index2 index = new Index2(direction.y, direction.x);
        UpdateGraphicsForCell(emptyCellPos + index);
        UpdateGraphicsForCell(emptyCellPos + index * -1);

        UpdateGraphicsForCell(movingItemPos + index);
        UpdateGraphicsForCell(movingItemPos + index * -1);

        UpdateGraphicsForCell(emptyCellPos);

        AudioController.PlaySound(AudioController.Settings.sounds.swipeClip, AudioController.AudioType.Sound, 0.8f);
    }

    private void OnCellMoved()
    {
        isTouchActive = true;

        if (hasMines)
        {
            UpdateMinesRotation(false);
        }

        CheckForLevelComplete();
    }

    private void UpdateMinesRotation(bool forFinalPath)
    {
        UpdateRotationForMine(MineBehaviour.mineOneRef, forFinalPath);
        UpdateRotationForMine(MineBehaviour.mineTwoRef, forFinalPath);
    }

    private void UpdateRotationForMine(MineBehaviour mine, bool forFinalPath)
    {
        Index2 mineIndex = mine.IndexPosition;

        if (UpdateRotationForNeigbour(mine, mineIndex + Index2.down, Vector3.zero, forFinalPath))
        {
            return;
        }

        if (UpdateRotationForNeigbour(mine, mineIndex + Index2.right, Vector3.zero.SetY(270f), forFinalPath))
        {
            return;
        }

        if (UpdateRotationForNeigbour(mine, mineIndex + Index2.left, Vector3.zero.SetY(90f), forFinalPath))
        {
            return;
        }

        if (UpdateRotationForNeigbour(mine, mineIndex + Index2.up, Vector3.zero.SetY(180f), forFinalPath))
        {
            return;
        }

    }

    public bool UpdateRotationForNeigbour(MineBehaviour mine, Index2 neighbourIndex, Vector3 rotation, bool forFinalPath)
    {
        // if neighbour is in level range
        if (neighbourIndex.x >= 0 && neighbourIndex.x < level.size.x && neighbourIndex.y >= 0 && neighbourIndex.y < level.size.y)
        {
            // if it is a rails
            if (playgroud[neighbourIndex.x][neighbourIndex.y].Type == CellType.Rails)
            {
                // if it's a check for final path - mine should turn only for rails on final path
                if (forFinalPath)
                {
                    // checking if neighbour is on players path
                    bool isRoadOnPath = false;
                    for (int i = 0; i < playerMovePath.Count; i++)
                    {
                        if (playerMovePath[i] == neighbourIndex.ToVector3XZ())
                        {
                            isRoadOnPath = true;
                            break;
                        }
                    }

                    if (!isRoadOnPath)
                        return false;
                }

                mine.UpdateRotation(rotation);
                return true;
            }
        }

        return false;
    }

    #endregion

    #region Level Logic

    private void CheckForLevelComplete()
    {
        if (IsLevelComplete())
        {
            LevelComplete();
        }
    }

    private bool IsLevelComplete()
    {
        pathMatrix = new List<List<int>>();

        for (int i = 0; i < level.size.x + 2; i++)
        {
            List<int> currentLine = new List<int>();

            for (int j = 0; j < level.size.y + 2; j++)
            {
                currentLine.Add(-1);
            }

            pathMatrix.Add(currentLine);
        }

        if (playgroud[level.finish.x][level.finish.y].Type != CellType.Rails)
        {
            return false;
        }

        goldBarsPicked = 0;
        minesPassed = false;

        // recursive searching for way from finish to start
        bool isPathCompleted = FoundStartCell(playgroud[level.finish.x][level.finish.y]);

        // if not all gold items picked
        if (isPathCompleted && goldBarsAmount != goldBarsPicked)
        {
            HighlightNotPickedItems();
        }

        return isPathCompleted && goldBarsAmount == goldBarsPicked;
    }

    private bool FoundStartCell(CellBehaviour currentCell)
    {
        playerMovePath.Clear();
        pathMatrix[currentCell.IndexPosition.x][currentCell.IndexPosition.y] = 0;

        if (currentCell.HasGoldBar)
        {
            goldBarsPicked++;
        }

        if (hasMines && currentCell.IsMine && !minesPassed)
        {
            Index2 otherMineIndex = GetOtherMineIndex(currentCell.IndexPosition);
            firstMinePosition = otherMineIndex.ToVector3XZ();
            bool result = CheckNeightbour(otherMineIndex);
            if (result)
                return true;
        }
        else
        {
            bool hasNeighboursGold = HasNeightboursGold(currentCell);

            Index2 neighbour = currentCell.IndexPosition + Index2.right;
            if (!hasNeighboursGold || (neighbour.x < level.size.x && hasNeighboursGold && playgroud[neighbour.x][neighbour.y].HasGoldBar))
            {
                bool result = CheckNeightbour(neighbour);
                if (result)
                    return true;
            }

            neighbour = currentCell.IndexPosition + Index2.left;
            if (!hasNeighboursGold || (neighbour.x >= 0 && hasNeighboursGold && playgroud[neighbour.x][neighbour.y].HasGoldBar))
            {
                bool result = CheckNeightbour(neighbour);
                if (result)
                    return true;
            }

            neighbour = currentCell.IndexPosition + Index2.down;
            if (!hasNeighboursGold || (neighbour.y >= 0 && hasNeighboursGold && playgroud[neighbour.x][neighbour.y].HasGoldBar))
            {
                bool result = CheckNeightbour(neighbour);
                if (result)
                    return true;
            }

            neighbour = currentCell.IndexPosition + Index2.up;
            if (!hasNeighboursGold || (neighbour.y < level.size.y && hasNeighboursGold && playgroud[neighbour.x][neighbour.y].HasGoldBar))
            {
                bool result = CheckNeightbour(neighbour);
                if (result)
                    return true;
            }
        }

        goldBarsPicked = 0;
        minesPassed = false;
        return false;
    }

    private bool CheckNeightbour(Index2 neighbourIndex)
    {
        if (neighbourIndex.x >= 0 && neighbourIndex.x < level.size.x && neighbourIndex.y >= 0 && neighbourIndex.y < level.size.y)
        {
            if (playgroud[neighbourIndex.x][neighbourIndex.y].Type == CellType.Rails && pathMatrix[neighbourIndex.x][neighbourIndex.y] == -1)
            {
                if (neighbourIndex == level.start)
                {
                    playerMovePath.Add(neighbourIndex.ToVector3XZ());
                    return true;
                }
                else
                {
                    bool result = FoundStartCell(playgroud[neighbourIndex.x][neighbourIndex.y]);
                    if (result)
                    {
                        playerMovePath.Add(neighbourIndex.ToVector3XZ());
                    }

                    return result;
                }
            }
        }

        return false;
    }

    private bool HasNeightboursGold(CellBehaviour currentCell)
    {
        Index2 neighbour = currentCell.IndexPosition + Index2.right;
        if (neighbour.x < level.size.x && playgroud[neighbour.x][neighbour.y].HasGoldBar && pathMatrix[neighbour.x][neighbour.y] == -1)
        {
            return true;
        }

        neighbour = currentCell.IndexPosition + Index2.down;

        if (neighbour.y >= 0 && playgroud[neighbour.x][neighbour.y].HasGoldBar && pathMatrix[neighbour.x][neighbour.y] == -1)
        {
            return true;
        }

        neighbour = currentCell.IndexPosition + Index2.left;
        if (neighbour.x >= 0 && playgroud[neighbour.x][neighbour.y].HasGoldBar && pathMatrix[neighbour.x][neighbour.y] == -1)
        {
            return true;
        }

        neighbour = currentCell.IndexPosition + Index2.up;
        if (neighbour.y < level.size.y && playgroud[neighbour.x][neighbour.y].HasGoldBar && pathMatrix[neighbour.x][neighbour.y] == -1)
        {
            return true;
        }

        return false;
    }

    private void LevelComplete()
    {
        isTouchActive = false;

        StartCoroutine(LevelCompleteAnimation());
    }

    private Index2 GetOtherMineIndex(Index2 firstMineIndex)
    {
        minesPassed = true;
        if (firstMineIndex == MineBehaviour.FirstMineIndex)
        {
            return MineBehaviour.SecondMineIndex;
        }
        else
        {
            return MineBehaviour.FirstMineIndex;
        }
    }

    #endregion

    #region Animation

    private IEnumerator LevelCompleteAnimation()
    {
        playgroud[CellBehaviour.EmptyCellPosition.x][CellBehaviour.EmptyCellPosition.y].EnableRenderer();

        WaitForSeconds delay = new WaitForSeconds(0.1f);
        Index2 finishIndex = level.finish + Index2.one;

        OnFinalAnimation?.Invoke();

        if (hasMines)
        {
            UpdateMinesRotation(true);
        }

        yield return new WaitForSeconds(0.3f);
        ColorsController.HideAnimation(0.3f);
        yield return new WaitForSeconds(0.2f);

        UIController.instance.ShowLevelComplete();

        if (hasMines)
        {
            int firstMineIndex = playerMovePath.FindIndex(v => v == firstMinePosition);
            List<Vector3> beforeMinePath = playerMovePath.GetRange(0, firstMineIndex + 1);
            List<Vector3> afterMinePath = playerMovePath.GetRange(firstMineIndex + 1, playerMovePath.Count - firstMineIndex - 1);

            afterMinePath.Add(level.finish.ToVector3XZ());
            afterMinePath.AddRange(playerFinalPath);

            yield return PlayerController.MoveWithMinesCoroutine(beforeMinePath, afterMinePath);
        }
        else
        {
            playerMovePath.Add(level.finish.ToVector3XZ());
            playerMovePath.AddRange(playerFinalPath);
            yield return PlayerController.MoveCoroutine(playerMovePath);
        }

        OnDisableEnvironment?.Invoke();

        UIController.instance.HideLevelComplete();
        yield return new WaitForSeconds(1f);


        movableEnvironment.ReturnToPoolEverything();
        staticEnvironment.ReturnToPoolEverything();

        railsPool.ReturnToPoolEverything();

        GameController.OnLevelComplete();
    }

    private void HighlightNotPickedItems()
    {
        List<CellBehaviour> cellsWithGold = new List<CellBehaviour>();

        int xLength = level.itemsShuffled.Length;
        int zLength = level.itemsShuffled[0].ints.Length;

        for (int i = 0; i < xLength; i++)
        {
            for (int j = 0; j < zLength; j++)
            {
                if (playgroud[i][j].HasGoldBar)
                {
                    cellsWithGold.Add(playgroud[i][j]);
                }
            }
        }

        for (int i = 0; i < cellsWithGold.Count; i++)
        {
            Vector3 currentCellPosition = cellsWithGold[i].transform.position;
            bool isPicked = false;

            for (int j = 0; j < playerMovePath.Count && !isPicked; j++)
            {
                if (currentCellPosition == playerMovePath[j])
                {
                    isPicked = true;
                }
            }

            if (!isPicked)
            {
                cellsWithGold[i].HighlightGoldItem();
            }
        }

    }

    #endregion
}