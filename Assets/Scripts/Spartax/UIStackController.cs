﻿using System.Collections.Generic;
using UnityEngine;

public class UIStackController : UniqueElement
{
    public enum Type
    {
        SCREEN,
        POPUP
    }
    
    public Stack<UIController> _screenList;
    public Stack<UIController> _popupList;

    [SerializeField] private GameObject _screenParent;    
    [SerializeField] private GameObject _popupParent;


    #region Popup methods

    public UIController PushPopup(string path)
    {
        var screen = GetView(path);
        PushPopup(screen);
        return screen;
    }

    public void PushPopup(UIController screen)
    {
        Push(_popupList, screen);
    }

    #endregion

    #region Screen methods

    public UIController PushScreen(string path)
    {
        var screen = GetView(path);
        PushScreen(screen);
        return screen;
    }

    public void PushScreen(UIController screen)
    {
        Push(_screenList, screen);
    }

    #endregion

    public void Pop(UIController screen)
    {
        if (_popupList.Peek() == screen)
        {
            Pop(_popupList);
        }
        else if(_screenList.Peek() == screen)
        {
            Pop(_screenList);
        }
    }

    protected void Pop(Stack<UIController> list)
    {
        var screen = list.Pop();
        screen.OnDisappeared();
        Destroy(screen.gameObject);
    }

    protected void Push(Stack<UIController> list, UIController screen)
    {
        if (list.Count > 0)
        {
            var previous = list.Peek();
            previous.OnDisappeared();
            previous.gameObject.SetActive(false);
        }

        list.Push(screen);
        screen.gameObject.SetActive(true);
        screen.OnAppeared();
    }
    
    public UIController GetView(string path)
    {
        var screen = Resources.Load<GameObject>(path);
        var instance = Instantiate(screen, GetParent(screen.GetComponent<UIController>().Type).transform);
        return instance.GetComponent<UIController>();
    }

    protected GameObject GetParent(Type type)
    {
        if (type == Type.POPUP)
        {
            return _popupParent;
        }

        return _screenParent;
    }
}