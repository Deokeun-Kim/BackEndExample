using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Friend_Entity : Entity<FriendData>
{
	public UnityEngine.UI.Text text_Name;
	public GameObject[] obj_btnList;


	public override void SetEntity(FriendData _data)
	{
		if (_data == null)
			return;

		entityData = _data;

		if(gameObject.activeSelf == false)
			gameObject.SetActive(true);

		for (int i = 0; i < obj_btnList.Length; i++)
		{
			if (obj_btnList[i].gameObject.activeSelf == false)
				continue;
			obj_btnList[i].gameObject.SetActive(false);
		}

		text_Name.text = _data.nickname;

		switch (_data.typeData)
		{
			case FriendDataType.RandFriendList:
				obj_btnList[0].gameObject.SetActive(true);
				break;
			case FriendDataType.SendRequestList:
				obj_btnList[1].gameObject.SetActive(true);
				break;
			case FriendDataType.MyFriendList:
				obj_btnList[2].gameObject.SetActive(true);
				break;
			case FriendDataType.RecvRequestList:
				obj_btnList[3].gameObject.SetActive(true);
				break;
		}
	}
}
