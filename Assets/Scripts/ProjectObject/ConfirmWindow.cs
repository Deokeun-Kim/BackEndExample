using UnityEngine.UI;
using UnityEngine;
using System;

public class ConfirmWindow : MonoBehaviour
{
    public GameObject obj_Parent;
    public Text text_Title;
    public Text text_Contents;
    public Text text_Left;
    public Text text_Right;
    public Button btn_Left;
    public Button btn_Right;

    private Action action_Left;
    private Action action_Right;

	public void Initialized()
	{
        obj_Parent = transform.parent.gameObject;
        btn_Left.onClick.AddListener(OnClick_LeftAction);
        btn_Right.onClick.AddListener(OnClick_RightAction);
    }

	public void Show(string _Title = "", string _Contents = "", string _leftStr = "", string _rightStr = "", Action _left = null, Action _right = null)
    {
        obj_Parent.SetActive(true);
        text_Title.text = _Title;
        text_Contents.text = _Contents;

        action_Left = _left;
        action_Right = _right;

        if (string.IsNullOrEmpty(_leftStr) == true)
            btn_Left.gameObject.SetActive(false);
        else
            btn_Left.gameObject.SetActive(true);

        if (string.IsNullOrEmpty(_rightStr) == true)
            btn_Right.gameObject.SetActive(false);
        else
            btn_Right.gameObject.SetActive(true);

        text_Left.text = _leftStr;
        text_Right.text = _rightStr;

        gameObject.SetActive(true);
    }

    public void Show(string _Contents, Action _action)
    {
        Show(_Title: "확인창", _Contents: _Contents, _leftStr: "확인", _left: _action);
    }

    public void Show(string _Contents, Action _left, Action _right)
    {
        Show(_Title: "확인창", _Contents: _Contents, _leftStr: "취소", _rightStr: "확인", _left: _left, _right: _right);
    }

    private void OnClick_LeftAction()
    {
        action_Left?.Invoke();
        Hide();
    }

    private void OnClick_RightAction()
    {
        action_Right?.Invoke();
        Hide();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
        obj_Parent.SetActive(false);
    }
}
