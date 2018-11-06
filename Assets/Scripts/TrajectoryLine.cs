﻿using TouchScript.Gestures;
using System.Collections.Generic;
using UnityEngine;

public class TrajectoryLine : MonoBehaviour
{
    public class TrajectoryStepData
    {
        public Vector3 originPoint;
        public Vector3 endPoint;
        public Vector3 direction;
        public float distance;
    }

    public float MaxLineLength = 10f;
    public int MaxReboundTries = 5;
    public float CollisionHeight = 0.25f;

    [SerializeField] private PlayerController _playerController;
    [SerializeField] private GameObject _startDummy;
    [SerializeField] private GameObject _endDummy;
    [SerializeField] private GameObject _arrowModel;
    [SerializeField] float _directionAngleIncrement;
    [SerializeField] float _maxVerticalSizeScreenPercentage;
    [SerializeField] float _maxVerticalScale;

    List<TrajectoryStepData> _trajectorySteps = new List<TrajectoryStepData>();

    private int reboundLayerMask;

    Vector2 _initialDragPosition;
    Vector3 _initialPosition;
    float _curPower;

    public float Power { get { return _curPower; } }

    void Start()
    {
        reboundLayerMask = 1 << 9;
        ResetRotation();
        _initialPosition = transform.localPosition;
        FinishAiming();
    }

    public void StartNewAiming()
    {
        gameObject.SetActive(true);
    }

    public void FinishAiming()
    {
        gameObject.SetActive(false);
    }

    public void OnGestureStateChanged(Gesture sender)
    {
        switch(sender.State)
        {
            case Gesture.GestureState.Began:
                _initialDragPosition = sender.ScreenPosition;
                break;
            case Gesture.GestureState.Changed:
                MoveDirectionArrow(sender.ScreenPosition - sender.PreviousScreenPosition);
                UpdateArrowSize(sender.ScreenPosition);
                break;
            case Gesture.GestureState.Ended:
            case Gesture.GestureState.Failed:
            case Gesture.GestureState.Cancelled:
                ResetRotation();
                break;

        }
    }

    void UpdateArrowSize(Vector2 curPosition)
    {
        float yDistance = curPosition.y - _initialDragPosition.y;
        var verticalPercentage = yDistance / Display.main.renderingHeight;

        bool validVerticalDirection = true;
        if(verticalPercentage >= 0f)
        {
            validVerticalDirection = false;
        }

        verticalPercentage = Mathf.Abs(verticalPercentage);

        verticalPercentage = Mathf.Clamp(verticalPercentage, 0f, _maxVerticalSizeScreenPercentage);
        _curPower = (verticalPercentage / _maxVerticalSizeScreenPercentage);

        if(!validVerticalDirection)
        {
            _arrowModel.transform.localScale = Vector3.one;
            return;
        }

        var finalSize = verticalPercentage * _maxVerticalScale;
        _arrowModel.transform.localScale = new Vector3(1f + finalSize, 1f + finalSize, 1 + finalSize);
        //Debug.Log("DISTANCE " + verticalPercentage + "  SIZE " + verticalPercentage * _maxVerticalScale * -1f);
    }

    void MoveDirectionArrow(Vector2 deltaIncrement)
    {
        transform.localRotation *= Quaternion.Euler(0f, deltaIncrement.x * _directionAngleIncrement, 0f);
    }

    private Vector3 _originDirPos, _endDirPos;
    public Vector3 GetAimingDirection()
    {
        _originDirPos = _startDummy.transform.position;
        _originDirPos.y = CollisionHeight;

        _endDirPos = _endDummy.transform.position;
        _endDirPos.y = CollisionHeight;
        
        return (_originDirPos - _endDirPos).normalized;
    }

    void ResetRotation()
    {
        transform.localPosition = _initialPosition;
        transform.localRotation = Quaternion.identity;
    }
}