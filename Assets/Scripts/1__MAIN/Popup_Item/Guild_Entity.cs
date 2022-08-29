using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Guild_Entity : Entity<GuildInfoData>
{
	public Text text_GuildName;
	public Text text_MasterName;
	public Text text_MemberCount;
	public GameObject obj_Btn;

	public override void SetEntity(GuildInfoData _data)
	{
		entityData = _data;
		text_GuildName.text = entityData.guildName;
		text_MasterName.text = entityData.masterNickname;
		text_MemberCount.text = entityData.memberCount.ToString();

		if(gameObject.activeSelf == false)
			gameObject.SetActive(true);

		if (obj_Btn == null)
			return;

		if (obj_Btn.activeSelf == false)
			obj_Btn.SetActive(true);
	}

	public void GuildJoinBtnHide()
	{
		obj_Btn.SetActive(false);
	}

	public void DeleteMyGuildData()
	{
		entityData = null;
		text_GuildName.text = string.Empty;
		text_MasterName.text = string.Empty;
		text_MemberCount.text = string.Empty;
	}
}
