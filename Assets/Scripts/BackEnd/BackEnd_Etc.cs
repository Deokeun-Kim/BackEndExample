using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public partial class BackEndManager
{
    [Header(">>>DEV<<<")]
    public GameObject objCanvas;
    public GameObject objLoadingWindow;
    public ConfirmWindow objConfirmWindow;
    public GameObject obj_LoadingWindow { private get; set; }
    public ConfirmWindow obj_ConfirmWindow { private get; set; }

    public void SetLoadingWindow(bool isOpen = true)
    {
        objCanvas?.SetActive(isOpen);
        obj_LoadingWindow?.SetActive(isOpen);
    }

    public void ShowWindow(string _Title = "", string _Contents = "", string _leftStr = "", string _rightStr = "", Action _left = null, Action _right = null)
    {
        obj_ConfirmWindow?.Show(_Title,_Contents,_leftStr,_rightStr,_left,_right);
    }

    public void ShowConfirmWindow(string _Contents, Action _action = null)
    {
        ShowWindow(_Title: "확인창", _Contents: _Contents, _leftStr: "확인", _left: _action);
    }

    public void ShowQuestionWindow(string _Contents, Action _left = null, Action _right = null)
    {
        ShowWindow(_Title: "확인창", _Contents: _Contents, _leftStr: "취소", _rightStr: "확인", _left: _left, _right: _right);
    }
}
