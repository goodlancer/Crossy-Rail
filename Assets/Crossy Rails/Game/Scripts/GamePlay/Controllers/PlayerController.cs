using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon;

[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    private static PlayerController instance;

    public float movementSpeed;

    private Animator animator;
    private Transform transformRef;

    private List<Vector3> movePoints;

    private bool lookingForward;
    private bool pauseLookingForward;
    private bool movementEnabled;

    private int enterMineParameter;
    private int exitMineParameter;
    private int nextPointIndex;


    private void Awake()
    {
        instance = this;
        transformRef = transform;

        animator = GetComponent<Animator>();
        enterMineParameter = Animator.StringToHash("EnterMine");
        exitMineParameter = Animator.StringToHash("ExitMine");
    }

    public static void Initialize(List<Vector3> startMovementPath)
    {
        instance.transformRef.position = startMovementPath[0];
        instance.gameObject.SetActive(true);

        instance.StartCoroutine(instance.MovementCoroutine(startMovementPath));
    }

    public static void Move(List<Vector3> positions)
    {
        instance.StartCoroutine(instance.MovementCoroutine(positions));
    }

    //private void FixedUpdate()
    //{
    //    if (!movementEnabled)
    //        return;

    //    float distanceDelta = movementSpeed * Time.fixedDeltaTime;
    //    float distanceToNextPoint = (movePoints[nextPointIndex] - transformRef.position).magnitude;

    //    while ((distanceDelta > distanceToNextPoint || GetPoint(nextPointIndex).z < transformRef.position.z + 0.2f) && movementEnabled)
    //    {
    //        nextPointIndex++;
    //        distanceToNextPoint = (GetPoint(nextPointIndex) - transformRef.position).magnitude;
    //    }

    //    Vector3 positionsDelta = GetPoint(nextPointIndex) - GetPoint(nextPointIndex - 1);
    //    Vector3 newPosition = transformRef.position + positionsDelta.normalized * distanceDelta;
    //    newPosition = Vector3.Lerp(transformRef.position, newPosition, 0.3f);
    //    newPosition = newPosition.SetX(Mathf.Lerp(newPosition.x, GetPoint(nextPointIndex).x, 1f - Mathf.Clamp01(Mathf.Abs(positionsDelta.x) * 10)));

    //    //transformRef.LookAt(GetPoint(nextPointIndex + 1));

    //    transformRef.position = newPosition;
    //    transformRef.rotation = Quaternion.Lerp(transformRef.rotation, Quaternion.LookRotation(positionsDelta), rotateSpeed * Time.deltaTime);
    //}

    public static IEnumerator MoveCoroutine(List<Vector3> positions)
    {
        return instance.MovementCoroutine(positions);
    }

    public IEnumerator MovementCoroutine(List<Vector3> positions)
    {
        AudioCase audioCase = AudioController.PlaySmartSound(AudioController.Settings.sounds.cartMovementClip);
        audioCase.source.loop = true;

        StartCoroutine(LookForwardCoroutine());

        positions.Insert(0, transformRef.position);

        for (int i = 1; i < positions.Count; i++)
        {
            float positionsDelta = Vector3.Magnitude(positions[i] - positions[i - 1]);
            float moveTime = 0f;
            if (positionsDelta != 0)
            {
                moveTime = positionsDelta / movementSpeed;

                transformRef.DOMove(positions[i], moveTime);

                yield return new WaitForSeconds(moveTime);
            }
        }

        lookingForward = false;
        audioCase.Stop();
    }

    private IEnumerator LookForwardCoroutine()
    {
        lookingForward = true;
        Vector3 prevPos = transformRef.position;

        while (lookingForward)
        {
            if (!pauseLookingForward)
            {

                Vector3 positionDelta = (transformRef.position - prevPos).normalized;
                if (positionDelta != Vector3.zero)
                {
                    transformRef.rotation = Quaternion.Lerp(transformRef.rotation, Quaternion.LookRotation(positionDelta), 20 * Time.deltaTime);
                }

                prevPos = transformRef.position;
            }

            yield return null;
        }
    }

    public static IEnumerator MoveWithMinesCoroutine(List<Vector3> beforeMinePath, List<Vector3> afterMinePath)
    {
        return instance.MovementWithMinesCoroutine(beforeMinePath, afterMinePath);
    }

    public IEnumerator MovementWithMinesCoroutine(List<Vector3> beforeMinePath, List<Vector3> afterMinePath)
    {
        AudioCase audioCase = AudioController.PlaySmartSound(AudioController.Settings.sounds.cartMovementClip);
        audioCase.source.loop = true;

        StartCoroutine(LookForwardCoroutine());

        beforeMinePath.Insert(0, transformRef.position);
        //beforeMinePath[beforeMinePath.Count - 1] = beforeMinePath[beforeMinePath.Count - 2] + (beforeMinePath[beforeMinePath.Count - 1] - beforeMinePath[beforeMinePath.Count - 2]) * 0.3f;

        for (int i = 1; i < beforeMinePath.Count; i++)
        {
            if (i == beforeMinePath.Count - 1)
            {
                animator.SetTrigger(enterMineParameter);
            }

            float positionsDelta = Vector3.Magnitude(beforeMinePath[i] - beforeMinePath[i - 1]);
            float moveTime = 0f;
            if (positionsDelta != 0)
            {
                moveTime = positionsDelta / movementSpeed;

                transformRef.DOMove(beforeMinePath[i], moveTime);

                yield return new WaitForSeconds(moveTime);
            }
        }
        pauseLookingForward = true;

        yield return new WaitForSeconds(0.4f);

        transformRef.position = afterMinePath[0];
        pauseLookingForward = false;
        animator.SetTrigger(exitMineParameter);

        yield return new WaitForSeconds(0.3f);

        for (int i = 1; i < afterMinePath.Count; i++)
        {
            float positionsDelta = Vector3.Magnitude(afterMinePath[i] - afterMinePath[i - 1]);
            float moveTime = 0f;
            if (positionsDelta != 0)
            {
                moveTime = positionsDelta / movementSpeed;

                transformRef.DOMove(afterMinePath[i], moveTime);

                yield return new WaitForSeconds(moveTime);
            }
        }

        lookingForward = false;

        audioCase.Stop();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PathHighlight"))
        {
            other.GetComponent<PathHighlight>().Disable();
        }
    }
}