#pragma warning disable 0649

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Watermelon;

public class SettingsButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField]
    private GameObject panelBackground;
    [SerializeField]
    private RectTransform panelRectTransform;
    [SerializeField]
    private CanvasGroup buttonsCanvasGroup;
    
    [SerializeField]
    private Vector2 defaultSize = new Vector2(170, 170);
    [SerializeField]
    private Vector2 openedSizeOneItem = new Vector2(250, 170);
    [SerializeField]
    private Vector2 openedSizeTwoItems = new Vector2(417, 170);

    private bool isOpened = false;
    private bool isOpening = false;

    private Vector3 openedSize;

    public bool IsOpened
    {
        get { return isOpened; }
    }

    private TweenCase openCase;

    private void Awake()
    {
        openedSize = (AudioController.Settings.isAudioEnabled && AudioController.Settings.isMusicEnabled) ? openedSizeTwoItems : openedSizeOneItem;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isOpened)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    public void Open()
    {
        if (isOpening)
            return;

        if (!isOpened)
        {
            AudioController.PlaySound(AudioController.Settings.sounds.buttonClip, AudioController.AudioType.Sound, 1f);
            panelBackground.SetActive(true);

            buttonsCanvasGroup.alpha = 0;

            openCase = panelRectTransform.DOSize(openedSize, 0.2f).OnComplete(delegate
            {
                buttonsCanvasGroup.DOFade(1, 0.1f);

                isOpened = true;
                isOpening = false;
            });

            isOpening = true;
        }
    }

    public void Close()
    {
        if (isOpening)
            return;

        if (isOpened)
        {
            AudioController.PlaySound(AudioController.Settings.sounds.buttonClip, AudioController.AudioType.Sound, 1f);
            buttonsCanvasGroup.DOFade(0, 0.1f);

            openCase = panelRectTransform.DOSize(defaultSize, 0.1f).OnComplete(delegate
            {
                panelBackground.SetActive(false);

                isOpened = false;
                isOpening = false;
            });

            isOpening = true;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
    }
}
