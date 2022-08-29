using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class MainManager : MonoBehaviour
{
	public Text text_NickName;

	public List<BasePopup> popupList;
	private Dictionary<popup_type,BasePopup> popupDic;
	private BasePopup curPopup;
	private popup_type curPopupType;

	private void Awake()
	{
		text_NickName.text = BackEnd.Backend.UserNickName;
		BackEndManager.Instance.GetChartList(null);

		Initialized();
	}

	public void OnClick_Logout()
	{
		BackEndManager.Instance.Logout((isRecv) =>
		{
			if (isRecv)
				BackEndManager.Instance.SceneMove(SceneType.INTRO);
		});
	}

	public void Initialized()
	{
		popupDic = new Dictionary<popup_type, BasePopup>();
		for (int i = 0; i < popupList.Count; i++)
		{
			popupList[i].gameObject.SetActive(false);
			popupList[i].Init();
			if (popupDic.ContainsKey(popupList[i].popupType) == true)
			{
				Debug.LogError($"Has in Key Dictionary : {popupList[i].popupType}");
				continue;
			}
			popupDic.Add(popupList[i].popupType, popupList[i]);
		}
	}

	private void ShowPopup(popup_type _type)
	{
		if (popupDic.ContainsKey(_type) == false)
		{
			Debug.LogError($"Hasn't in Key Dictionary : {_type.ToString()}");
			return;
		}

		if (curPopup != null)
			curPopup.Hide();

		curPopup = popupDic[_type];
		curPopup.Show();
	}

	public void ShowPopup(string _type)
	{
		bool isSuccess = false;

		try
		{
			curPopupType = (popup_type)Enum.Parse(typeof(popup_type), _type);
			isSuccess = true;
		}
		catch (Exception e)
		{
			Debug.LogError(e.Message);
		}

		if(isSuccess == true)
			ShowPopup(curPopupType);
	}
}
