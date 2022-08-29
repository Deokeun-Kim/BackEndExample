using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Popup_Notices : BasePopup
{
	public Notices_Entity baseEntity;
	public List<Notices_Entity> poolEntityList;

	public override void Init()
	{
		baseEntity.gameObject.SetActive(false);
		popupType = popup_type.NOTICES;
	}

	public override void Show()
	{
		base.Show();
		BackEndManager.Instance.GetNoticeList(EntityList_SettingByData);
	}

	public override void Hide()
	{
		base.Hide();
		for (int i = 0; i < poolEntityList.Count; i++)
			poolEntityList[i].Hide();
	}

	protected Notices_Entity GetInActiveEntity(int Count)
	{
		if (poolEntityList.Count > Count)
			return poolEntityList[Count];

		Notices_Entity newEntity = Instantiate(baseEntity,baseEntity.transform.parent);
		poolEntityList.Add(newEntity);
		return newEntity;
	}

	private void EntityList_SettingByData(List<NoticeData> _dataList)
	{
		if (_dataList.Count <= 0)
		{
			BackEndManager.Instance.ShowConfirmWindow("공지사항이 없습니다.");
			return;
		}

		for (int i = 0; i < _dataList.Count; i++)
		{
			Notices_Entity item = GetInActiveEntity(i);
			item.SetEntity(_dataList[i]);
		}
	}

	public void OnClick_NoticeEntity(Notices_Entity _entity)
	{
		if (string.IsNullOrEmpty(_entity.GetEntityData().linkUrl) == true)
			return;

		Application.OpenURL(_entity.GetEntityData().linkUrl);
	}
}
