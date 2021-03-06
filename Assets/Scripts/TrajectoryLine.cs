﻿using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Xml.Schema;
using BlastyEvents;
using UnityEngine;

public class TrajectoryLine : MonoBehaviour
{
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private GameObject _startDummy;
    [SerializeField] private GameObject _endDummy;
    [SerializeField] private GameObject _arrowModel;
    [SerializeField] float _directionAngleIncrement;
    [SerializeField] float _maxVerticalSizeScreenPercentage;
    [SerializeField] float _maxVerticalScale;
    [SerializeField] float _minPowerToShoot = 0.02f;

    private int reboundLayerMask;

    Vector2 _initialDragPosition;
    Vector3 _initialPosition;
    [SerializeField] private float _curPower;
    [SerializeField] private float _minScreenPercentageToMove = 0.1f;

    private Action<float> OnPowerChanged;
    
    Vector2 _onScreenInitialDirection;
    private Vector2 _startBallForwardDirection;
    float _initialYRotation;

    public float Power { get { return _curPower; } }

    void Start()
    {
        transform.parent = null;
        reboundLayerMask = 1 << 9;
        ResetRotation();
        _initialPosition = transform.localPosition;
        FinishAiming();
        
        EventManager.Instance.StartListening(TouchEvent.EventName, OnPanUpdated);
        EventManager.Instance.StartListening(ShootEvent.EventName, PanFinished);
    }

    void LateUpdate()
    {
        transform.position = _playerController.transform.position;
    }

    private void PanFinished(BlastyEventData ev)
    {
        var shootEv = (ShootEventData) ev;

        if (shootEv.ValidShot)
        {
            ResetRotation();
            FinishAiming();
        }
    }


    private void OnPanUpdated(BlastyEventData ev)
    {
        var touchEventData = (TouchEventData) ev;

        if (touchEventData.PanType == TouchManager.PanType.World)
            return;
        
        switch (touchEventData.TouchState)
        {
            case TouchManager.TouchState.InitPan:
                _initialDragPosition = touchEventData.CurPosition;
                StartNewAiming();
                GetInitialDirectionOnScreenSpace();
                break;
            case TouchManager.TouchState.UpdatePan:
                MoveDirectionArrow(touchEventData);
                UpdateArrowSize(touchEventData.CurPosition);
                break;
            case TouchManager.TouchState.FinishPan:
                FinishAiming();
                if (_curPower < _minPowerToShoot)
                    return;
            
                var shootEventData = new ShootEventData();
                shootEventData.ValidShot = true;
                shootEventData.Power = 10f;
            
                EventManager.Instance.TriggerEvent(ShootEvent.EventName, shootEventData);

                OnPowerChanged(0f);
                break;
        }
    }

    public void StartNewAiming()
    {
        var startPoint = Camera.main.WorldToScreenPoint(_playerController.transform.position);
        var endPoint = Camera.main.WorldToScreenPoint(_playerController.transform.position + 
                                                         _playerController.transform.forward );

        _startBallForwardDirection = (endPoint - startPoint).normalized;
        gameObject.SetActive(true);
    }

    public void FinishAiming()
    {
        gameObject.SetActive(false);
    }

    void GetInitialDirectionOnScreenSpace()
    {
        var startPoint = Camera.main.WorldToScreenPoint(_playerController.transform.position);
        var endPoint = Camera.main.WorldToScreenPoint(_playerController.transform.position + _playerController.transform.forward * 5f);

        _initialYRotation = _playerController.transform.localRotation.y;
        _onScreenInitialDirection = (endPoint - startPoint).normalized;
    }

    void UpdateArrowSize(Vector2 curPosition)
    {
        float dragLength = Mathf.Abs(Vector2.Distance(curPosition, _initialDragPosition));
        var verticalPercentage = dragLength / Display.main.renderingHeight;

        //Debug.Log("DRAG LENGTH " + dragLength);

        verticalPercentage = Mathf.Abs(verticalPercentage);

        verticalPercentage = Mathf.Clamp(verticalPercentage, 0f, _maxVerticalSizeScreenPercentage);
        _curPower = (verticalPercentage / _maxVerticalSizeScreenPercentage);

        var visualTotal = (_curPower - _minPowerToShoot)/ (1f - _minPowerToShoot);
        
        OnPowerChanged(visualTotal);
        

        var finalSize = verticalPercentage * _maxVerticalScale;
        _arrowModel.transform.localScale = new Vector3(1f + finalSize, 1f + finalSize, 1 + finalSize);
        //Debug.Log("DISTANCE " + verticalPercentage + "  SIZE " + verticalPercentage * _maxVerticalScale * -1f);
    }

    
    void MoveDirectionArrow(TouchEventData touchEventData)
    {
        //Debug.Log("DISTANCE : " + touchEventData.TotalPanScreenPercentageSize);

        if (touchEventData.TotalPanScreenPercentageSize < _minScreenPercentageToMove)
        {
            return;
        }
        
        var angles = Vector2.SignedAngle(touchEventData.CurDirection, Vector2.up);
        angles *= _directionAngleIncrement;
        angles += 180f;
        //Debug.Log("ANGLES " + angles + "   DOT PROD ");

        UpdateRotation(angles);
    }

    public void UpdateRotation(float angles)
    {
        var rotation = Vector3.SignedAngle(Vector3.forward, Camera.main.transform.forward, Vector3.up);
        transform.localRotation = Quaternion.Euler(0f, rotation + angles, 0f);
    }
    
    private Vector3 _originDirPos, _endDirPos;
    public Vector3 GetAimingDirection()
    {
        return (_startDummy.transform.position - _endDummy.transform.position).normalized;
    }

    void ResetRotation()
    {
        transform.localPosition = _initialPosition;
        transform.localRotation = Quaternion.identity;
    }
    
    public void SubscribeToOnPowerChanged(Action<float> subscriber)
    {
        OnPowerChanged += subscriber;
    }
}