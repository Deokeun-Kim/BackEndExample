using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Popup_Events : BasePopup
{
	public Event_Entity baseEntity;
	public List<Event_Entity> poolEntityList;

	public override void Init()
	{
		baseEntity.gameObject.SetActive(false);
		popupType = popup_type.EVENTS;
	}

	public override void Show()
	{
		base.Show();
		BackEndManager.Instance.GetEventList(EntityList_SettingByData);
	}

	public override void Hide()
	{
		base.Hide();
		for (int i = 0; i < poolEntityList.Count; i++)
			poolEntityList[i].Hide();
	}

	protected Event_Entity GetInActiveEntity(int Count)
	{
		if (poolEntityList.Count > Count)
			return poolEntityList[Count];

		Event_Entity newEntity = Instantiate(baseEntity,baseEntity.transform.parent);
		poolEntityList.Add(newEntity);
		return newEntity;
	}

	private void EntityList_SettingByData(List<EventData> _dataList)
	{
		if (_dataList.Count <= 0)
		{
			BackEndManager.Instance.ShowConfirmWindow("이벤트가 없습니다.");
			return;
		}

		for (int i = 0; i < _dataList.Count; i++)
		{
			Event_Entity item = GetInActiveEntity(i);
			item.SetEntity(_dataList[i]);
		}
	}

	public void OnClick_EventsEntity(Event_Entity _entity)
	{
		if (string.IsNullOrEmpty(_entity.GetEntityData().linkUrl) == true)
			return;

		Application.OpenURL(_entity.GetEntityData().linkUrl);
	}
}
