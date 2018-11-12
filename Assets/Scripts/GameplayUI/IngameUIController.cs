﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IngameUIController : UIController
{
    [SerializeField] private List<MissionUIController> _missionUiControllers;
    [SerializeField] private TextMeshProUGUI _triesText;

    public override void OnAppeared()
    {
        ServicesManager.Instance.MissionsManager.InitUI(_missionUiControllers);
        InitTries();
    }

    void InitTries()
    {
        var levelData = ServicesManager.Instance.CurrentLevel();
        OnTryUpdate(levelData.AmountOfTries);
    }

    public void OnTryUpdate(int amount)
    {
        _triesText.text = amount.ToString();
    }

    public void OnPressBack()
    {
        //TODO. show pause popup
        
        SceneManager.LoadScene(ServicesManager.MainScene);
        ServicesManager.Instance.UIStackController.Pop(this);
    }
}