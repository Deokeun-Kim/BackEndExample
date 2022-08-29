using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Networking;

public class Notices_Entity : Entity<NoticeData>
{
	public RawImage obj_NoticeImage;
	public Text text_Title;
	public Text text_Contents;

	public override void SetEntity(NoticeData _data)
	{
		entityData = _data;
		if (entityData != null)
		{
			gameObject.SetActive(true);
			text_Title.text = entityData.title;
			text_Contents.text = entityData.contents;
			StartCoroutine(nameof(GetTexture));
		}
		else
			Hide();
	}
	public override void Hide()
	{
		StopCoroutine(nameof(GetTexture));
		base.Hide();
	}

	private IEnumerator GetTexture()
	{
		if (string.IsNullOrEmpty(entityData.imageKey) == true)
			yield break;

		UnityWebRequest www = UnityWebRequestTexture.GetTexture(entityData.imageKey);
		Debug.Log(www.url);
		yield return www.SendWebRequest();
		obj_NoticeImage.texture = DownloadHandlerTexture.GetContent(www);
	}
}
