using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Event_Entity : Entity<EventData>
{
	public GameObject obj_ItemArea;
	public RawImage image_BG;
	public RawImage image_Icon;
	public Text text_Title;
	public Text text_Contents;


	private IEnumerator crt_LoadBG;
	private IEnumerator crt_LoadIcon;

	public bool isGetReward { get; set; }

	public override void SetEntity(EventData _eventData)
	{
		isGetReward = false;
		entityData = _eventData;
		gameObject.SetActive(true);

		text_Title.text = entityData.title;
		text_Contents.text = entityData.content;

		crt_LoadIcon = image_Icon.R_GetLoadImage(entityData.popUpImageKey);
		crt_LoadBG = image_BG.R_GetLoadImage(entityData.contentImageKey);
		StartCoroutine(crt_LoadIcon);
		StartCoroutine(crt_LoadBG);
	}
}

