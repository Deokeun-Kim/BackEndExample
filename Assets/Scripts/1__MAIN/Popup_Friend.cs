using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Popup_Friend : BasePopup
{
	public GameObject obj_EntityCountNon;
	public GameObject obj_Message;
	public UnityEngine.UI.InputField inputField_Message;
	public UnityEngine.UI.Button[] arr_Btn;
	public List<Friend_Entity> randItemList;

	private Friend_Entity selectFriend;

	public override void Init()
	{
		popupType = popup_type.FRIEND;
	}

	public override void Show()
	{
		base.Show();
		obj_Message.SetActive(false);
		OnClick_GetList(0);
	}

	public override void Hide()
	{
		base.Hide();
	}

	public void OnClick_GetList(int i)
	{
		BackEndManager.Instance.SetLoadingWindow();

		arr_Btn[i].interactable = false;

		for (int k = 0; k < arr_Btn.Length; k++)
		{
			if (arr_Btn[k].interactable == true || i == k)
				continue;
			arr_Btn[k].interactable = true;
		}

		if (i == 0)
			BackEndManager.Instance.GetFriendList(GetDataList);
		else if (i == 1)
			BackEndManager.Instance.GetSendFriendRequestList(GetDataList);
		else if (i == 2)
			BackEndManager.Instance.GetRecvFriendRequestList(GetDataList);
		else if (i == 3)
			BackEndManager.Instance.GetRandUserInfo(GetDataList);
	}

	private void GetDataList(List<FriendData> _list)
	{
		if (_list == null || _list.Count == 0)
		{
			BackEndManager.Instance.SetLoadingWindow(false);
			obj_EntityCountNon.SetActive(true);
			return;
		}

		obj_EntityCountNon.SetActive(false);

		if (randItemList.Count > _list.Count)
		{
			for (int i = _list.Count; i < randItemList.Count; i++)
			{
				if (randItemList[i].gameObject.activeSelf == true)
					randItemList[i].gameObject.SetActive(false);
				else
					continue;
			}
		}
		else if(randItemList.Count < _list.Count)
		{
			int createCount = _list.Count - randItemList.Count;
			for (int i = 0; i < createCount; i++)
			{
				Friend_Entity newEntity = Instantiate(randItemList[0],randItemList[0].transform.parent);
				randItemList.Add(newEntity);
			}
		}

		for (int i = 0; i < _list.Count; i++)
			randItemList[i].SetEntity(_list[i]);

		BackEndManager.Instance.SetLoadingWindow(false);
	}

	public void OnClick_SendMessage(Friend_Entity _entity)
	{
		selectFriend = _entity;
		obj_Message.SetActive(true);
	}

	public void OnClick_SendMessage()
	{
		BackEndManager.Instance.SendMessage(selectFriend.GetEntityData().inDate, inputField_Message.text,
			(isRecv)=> 
			{
				if(isRecv)
					BackEndManager.Instance.ShowConfirmWindow("메시지 전송 성공");
				else
					BackEndManager.Instance.ShowConfirmWindow("메시지 전송 실패");

				obj_Message.SetActive(false);
			});
	}

	public void OnClick_RequestAccept(Friend_Entity _entity)
	{
		BackEndManager.Instance.AcceptFriend(_entity.GetEntityData().inDate, (isRecv) =>
		 {
			 if (isRecv.IsSuccess() == true)
			 {
				 _entity.Hide();
				 BackEndManager.Instance.ShowConfirmWindow("친구 수락");
			 }
			 else
				 BackEndManager.Instance.ShowConfirmWindow("친구 수락 실패");
		 });
	}

	public void OnClick_RequestReject(Friend_Entity _entity)
	{
		BackEndManager.Instance.RejectFriend(_entity.GetEntityData().inDate, (isRecv) =>
		{
			if (isRecv.IsSuccess() == true)
			{
				_entity.Hide();
				BackEndManager.Instance.ShowConfirmWindow("친구 신청 거절 성공");
			}
			else
				BackEndManager.Instance.ShowConfirmWindow("친구 신청 거절 실패");
		});
	}

	public void OnClick_DeleteFriend(Friend_Entity _entity)
	{
		BackEndManager.Instance.DeleteFriend(_entity.GetEntityData().inDate, (isRecv) =>
		{
			if (isRecv == true)
			{
				_entity.Hide();
				BackEndManager.Instance.ShowConfirmWindow("친구 삭제 성공");
			}
			else
				BackEndManager.Instance.ShowConfirmWindow("친구 삭제 실패");
		});
	}

	public void OnClick_RequestCancel(Friend_Entity _entity)
	{
		BackEndManager.Instance.CancelFriendRequest(_entity.GetEntityData().inDate, (isRecv) =>
		{
			if (isRecv.IsSuccess() == true)
			{
				_entity.Hide();
				BackEndManager.Instance.ShowConfirmWindow("친구 신청 취소 성공");
			}
			else
				BackEndManager.Instance.ShowConfirmWindow("친구 신청 취소 실패");
		});
	}

	public void OnClick_RequestFriend(Friend_Entity _entity)
	{
		BackEndManager.Instance.RequestFriend(_entity.GetEntityData().inDate, (isRecv) =>
		{
			if (isRecv.IsSuccess() == true)
			{
				_entity.Hide();
				BackEndManager.Instance.ShowConfirmWindow("친구 신청 성공");
			}
			else
				BackEndManager.Instance.ShowConfirmWindow("친구 신청 실패");
		});
	}
}