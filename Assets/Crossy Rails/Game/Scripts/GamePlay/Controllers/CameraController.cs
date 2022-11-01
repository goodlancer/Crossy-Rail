using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon;
using Watermelon.Core;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    private static CameraController instance;

    public float oneUnitMoveTime = 0.1f;

    private Camera cameraRef;

    private static float widthToDepthRelation = 1f;
    private static float heightToWidthRelation = 1f;

    private void Awake()
    {
        instance = this;
        cameraRef = GetComponent<Camera>();

        Vector3 position1 = cameraRef.ScreenToWorldPoint(Vector3.zero.SetZ(transform.position.y));
        Vector3 position2 = cameraRef.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height).SetZ(transform.position.y));

        widthToDepthRelation = ((position2.x - position1.x) / (position2.z - position1.z));
        heightToWidthRelation = transform.position.y / (position2.x - position1.x);
    }

    public static void Init(Vector3 levelCenter, Index2 levelSize, bool smothTransition = true)
    {
        float cameraHeight = 1;

        float playgroundWidth = (levelSize.x > levelSize.y ? levelSize.x : levelSize.y) + 1.5f;
        cameraHeight = playgroundWidth * heightToWidthRelation;

        float zOffset = cameraHeight * Mathf.Tan((90f - instance.transform.eulerAngles.x) * Mathf.Deg2Rad);

        Vector3 position = levelCenter.SetY(cameraHeight).AddToZ(-zOffset);

        float moveDistance = (instance.transform.position - position).magnitude;

        if (moveDistance != 0)
        {
            if (smothTransition)
            {
                instance.transform.DOMove(position, moveDistance * instance.oneUnitMoveTime);
            }
            else
            {
                instance.transform.position = position;
            }
        }
    }
}