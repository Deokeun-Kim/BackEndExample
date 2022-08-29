using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Message_Entity : Entity<MessageData>
{
	public Text text_NickName;
	public Text text_Content;
	public bool isRecvMessage;

	public Button btn_Show;

	public override void SetEntity(MessageData _data)
	{
		entityData = _data;
		if (entityData == null)
			return;

		if (BackEndManager.Instance.myNickName.Equals(_data.senderNickname) == true)
			isRecvMessage = false;
		else
			isRecvMessage = true;

		text_NickName.text = entityData.senderNickname;
		text_Content.text = entityData.content;
		gameObject.SetActive(true);
	}

	public void OnClick_ShowTextContents()
	{
		bool isShow = !text_Content.gameObject.activeSelf;
		text_Content.gameObject.SetActive(isShow);
		btn_Show.gameObject.SetActive(!isShow);
	}
}
