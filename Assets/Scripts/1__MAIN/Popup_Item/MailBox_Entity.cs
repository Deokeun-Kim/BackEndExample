using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class MailBox_Entity : Entity<MailPostData>
{
	public GameObject obj_ItemArea;
	public Text text_MailTitle;
	public Text text_SendName;
	public Text text_MailContents;

	public Text text_ItemName;
	public Text text_ItemValue;

	public bool isGetReward { get; set; }

	public override void SetEntity(MailPostData _mailData)
	{
		isGetReward = false;
		entityData = _mailData;
		gameObject.SetActive(true);

		if (entityData.items != null && entityData.items.Count > 0)
		{
			obj_ItemArea.SetActive(true);
			text_ItemName.text = entityData.items[0].memo;
			text_ItemValue.text = entityData.items[0].itemCount.ToString();
		}
		else
			obj_ItemArea.SetActive(false);

		text_SendName.text = entityData.nickname;
		text_MailTitle.text = entityData.title;
		text_MailContents.text = entityData.content;
	}

	public List<MailPostItemData> GetItemDataList()
	{
		isGetReward = true;
		Hide();
		return entityData.items;
	}
}
