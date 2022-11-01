using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swipe : MonoBehaviour
{
    private int magnitude = 50;

    private bool tap, swipeLeft, swipeRight, swipeTop, swipeBottom;
    private bool isDragging = false;
    private Vector2 startTouch, swipeDelta;

    public Vector2 SwipeDelta { get { return swipeDelta; } }
    public bool SwipeLeft { get { return swipeLeft; } }
    public bool SwipeRight { get { return swipeRight; } }
    public bool SwipeTop { get { return swipeTop; } }
    public bool SwipeBottom { get { return swipeBottom; } }

    private void Update()
    {
        tap = swipeLeft = swipeRight = swipeTop = swipeBottom = false;

        #region Standalone
        if(Input.GetMouseButtonDown(0))
        {
            tap = true;
            isDragging = true;

            startTouch = Input.mousePosition;
        }
        else if(Input.GetMouseButtonUp(0))
        {
            Reset();
        }
        #endregion

        #region Mobile
        if(Input.touchCount != 0)
        {
            if(Input.touches[0].phase == TouchPhase.Began)
            {
                tap = true;
                isDragging = true;

                startTouch = Input.touches[0].position;
            }
            else if(Input.touches[0].phase == TouchPhase.Ended || Input.touches[0].phase == TouchPhase.Canceled)
            {
                Reset();
            }
        }
        #endregion

        if (isDragging)
        {
            swipeDelta = Vector2.zero;

            if (Input.touchCount != 0)
                swipeDelta = Input.touches[0].position - startTouch;
            else if (Input.GetMouseButton(0))
                swipeDelta = (Vector2)Input.mousePosition - startTouch;

            if (swipeDelta.magnitude > magnitude)
            {
                float x = swipeDelta.x;
                float y = swipeDelta.y;

                if(Mathf.Abs(x) > Mathf.Abs(y))
                {
                    if(x > 0)
                    {
                        swipeRight = true;
                    }
                    else
                    {
                        swipeLeft = true;
                    }
                }
                else
                {
                    if (y > 0)
                    {
                        swipeTop = true;
                    }
                    else
                    {
                        swipeBottom = true;
                    }
                }

                Reset();
            }
        }
    }

    private void Reset()
    {
        startTouch = swipeDelta = Vector2.zero;

        isDragging = false;
    }
}
