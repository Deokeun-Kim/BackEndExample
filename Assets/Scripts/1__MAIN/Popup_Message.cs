using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Popup_Message : BasePopup
{
	public Message_Entity baseEntity;
	public List<Message_Entity> poolEntityList;

	public override void Init()
	{
		popupType = popup_type.MESSAGE;
	}

	public override void Show()
	{
		base.Show();
		BackEndManager.Instance.GetRecvMessageList(EntityList_SettingByData);
	}

	public override void Hide()
	{
		base.Hide();
	}

	protected Message_Entity GetInActiveEntity(int Count)
	{
		if (poolEntityList.Count > Count)
			return poolEntityList[Count];

		Message_Entity newEntity = Instantiate(baseEntity,baseEntity.transform.parent);
		poolEntityList.Add(newEntity);
		return newEntity;
	}

	private void EntityList_SettingByData(List<MessageData> _dataList)
	{
		if (_dataList.Count <= 0)
		{
			BackEndManager.Instance.ShowConfirmWindow("받은 쪽지가 없습니다.");
			return;
		}

		for (int i = 0; i < _dataList.Count; i++)
		{
			Message_Entity item = GetInActiveEntity(i);
			item.SetEntity(_dataList[i]);
		}
	}

	public void OnClick_Delete(Message_Entity _entity)
	{
		if (_entity.isRecvMessage == true)
			BackEndManager.Instance.DeleteRecvMessage(_entity.GetEntityData().inDate,
			(isRecv) =>
			{
				if (isRecv == true)
					_entity.gameObject.SetActive(false);
				else
					BackEndManager.Instance.ShowConfirmWindow("삭제 실패");
			});
		else
			BackEndManager.Instance.DeleteSendMessage(_entity.GetEntityData().inDate,
			(isRecv) =>
			{
				if (isRecv == true)
					_entity.gameObject.SetActive(false);
				else
					BackEndManager.Instance.ShowConfirmWindow("삭제 실패");
			});
	}
}
