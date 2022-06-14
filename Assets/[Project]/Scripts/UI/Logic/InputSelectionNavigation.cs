using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class InputSelectionNavigation : MonoBehaviour
{
    [SerializeField] private List<InputTypeSelection> input_ButtonsAnimationObjects = new List<InputTypeSelection>();


    private void Awake()
    {
        foreach (InputTypeSelection item in input_ButtonsAnimationObjects)
            item.Init();

    }

    public void OnSelection(int index)
    {
        input_ButtonsAnimationObjects[index].OnSelect();
    }

    public void OnDeselection(int index)
    {
        input_ButtonsAnimationObjects[index].OnDeselect();
    }

    [System.Serializable]
    public class InputTypeSelection
    {
        [SerializeField] private List<Ui_AnimationObject> animationObjects = new List<Ui_AnimationObject>();
        public void OnDeselect()
        {
            foreach (Ui_AnimationObject item in animationObjects)
                item.AnimateAll_Start();
        }

        public void OnSelect()
        {
            foreach (Ui_AnimationObject item in animationObjects)
                item.AnimateAll_To();
        }

        public void Init()
        {
            foreach (Ui_AnimationObject item in animationObjects)
                item.Init();
        }
    }
}

[System.Serializable]
public class Ui_AnimationObject
{
    public GameObject gameObject;
    public bool disableAfterToAnimation;
    public bool disableAfterFromAnimation;
    [Header("Position")]
    public Vector2 animateTo;
    public Vector2 animateScaleTo;
    public Vector3 animateRotateTo;

    [Header("Animation speed")]
    public float toSpeed;
    public float fromSpeed;

    [HideInInspector] public RectTransform rectTransform;
    [HideInInspector] public Vector2 startPosition;
    [HideInInspector] public Vector2 startSize;
    [HideInInspector] public Vector3 startRotation;
    public void Init()
    {
        rectTransform = gameObject.GetComponent<RectTransform>();
        startPosition = rectTransform.anchoredPosition;
        startSize = rectTransform.sizeDelta;
        startRotation = rectTransform.localEulerAngles;
    }

    public void AnimateAll_Start()
    {
        gameObject.SetActive(true);
        rectTransform.DOAnchorPos(startPosition, fromSpeed);
        rectTransform.DOSizeDelta(startSize, fromSpeed);
        rectTransform.DORotate(startRotation, fromSpeed).OnComplete(DisableAfterFrom);
    }

    public void AnimateAll_To()
    {
        gameObject.SetActive(true);
        rectTransform.DOAnchorPos(animateTo, toSpeed);
        rectTransform.DOSizeDelta(animateScaleTo, toSpeed);
        rectTransform.DORotate(animateRotateTo, toSpeed).OnComplete(DisableAfterTo);

    }

    #region Animate To
    public void Animate_ToSize()
    {
        gameObject.SetActive(true);
        rectTransform.DOSizeDelta(animateScaleTo, toSpeed).OnComplete(DisableAfterTo);
    }
    public void Animate_ToPosition()
    {
        gameObject.SetActive(true);
        rectTransform.DOAnchorPos(animateTo, toSpeed).OnComplete(DisableAfterTo);
    }
    public void Animate_ToRotation()
    {
        gameObject.SetActive(true);
        rectTransform.DORotate(animateRotateTo, toSpeed).OnComplete(DisableAfterTo);
    }
    #endregion

    #region Animate to Start
    public void Animate_ToStartSize()
    {
        gameObject.SetActive(true);
        rectTransform.DOSizeDelta(startSize, fromSpeed).OnComplete(DisableAfterFrom);
    }
    public void Animate_ToStartPosition()
    {
        gameObject.SetActive(true);
        rectTransform.DOAnchorPos(startPosition, fromSpeed).OnComplete(DisableAfterFrom);
    }
    public void Animate_ToStartRotation()
    {
        gameObject.SetActive(true);
        rectTransform.DORotate(startRotation, fromSpeed).OnComplete(DisableAfterFrom);
    }
    #endregion

    private void DisableAfterTo()
    {
        if (disableAfterToAnimation)
            gameObject.SetActive(false);
    }

    private void DisableAfterFrom()
    {
        if (disableAfterFromAnimation)
            gameObject.SetActive(false);
    }
}
