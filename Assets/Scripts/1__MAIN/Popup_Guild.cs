using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Popup_Guild : BasePopup
{
	public GameObject[] arr_BtnObj;
	public GameObject[] arr_Contents;
	public GameObject obj_GuildDataEmpty;
	public GameObject obj_EntityCountNon;
	public Guild_Entity myGuild;
	public GameObject obj_GuildCreate;
	public InputField inputfield_GuildName;
	public List<Guild_Entity> randItemList;


	public override void Init()
	{
		popupType = popup_type.GUILD;
	}

	public override void Show()
	{
		base.Show();
		OnClick_Tab(0);
	}

	public override void Hide()
	{
		base.Hide();
		myGuild.DeleteMyGuildData();
	}

	private void GuildCreateSuccess()
	{
		obj_GuildCreate.SetActive(false);
		BackEndManager.Instance.SetLoadingWindow();
		BackEndManager.Instance.GetGuildInfo(_action: GetMyGuildData);
	}

	private void GetMyGuildData(GuildInfoData _guildData)
	{
		BackEndManager.Instance.SetLoadingWindow(false);
		if (_guildData == null)
		{
			myGuild.gameObject.SetActive(false);
			obj_GuildDataEmpty.SetActive(true);
		}
		else
		{
			myGuild.SetEntity(_guildData);
			obj_GuildDataEmpty.SetActive(false);
		}
	}

	private void GetRandGuildList(List<GuildInfoData> _dataList)
	{
		if (_dataList == null || _dataList.Count == 0)
		{
			BackEndManager.Instance.SetLoadingWindow(false);
			obj_EntityCountNon.SetActive(true);
			return;
		}

		obj_EntityCountNon.SetActive(false);

		if (randItemList.Count > _dataList.Count)
		{
			for (int i = _dataList.Count; i < randItemList.Count; i++)
			{
				if (randItemList[i].gameObject.activeSelf == true)
					randItemList[i].gameObject.SetActive(false);
				else
					continue;
			}
		}
		else if (randItemList.Count < _dataList.Count)
		{
			int createCount = _dataList.Count - randItemList.Count;
			for (int i = 0; i < createCount; i++)
			{
				Guild_Entity newEntity = Instantiate(randItemList[0],randItemList[0].transform.parent);
				randItemList.Add(newEntity);
			}
		}

		for (int i = 0; i < _dataList.Count; i++)
		{
			randItemList[i].SetEntity(_dataList[i]);
			if(myGuild.GetEntityData() != null)
				randItemList[i].GuildJoinBtnHide();
		}

		BackEndManager.Instance.SetLoadingWindow(false);
	}

	public void OnClick_Tab(int _TabCount)
	{
		if (_TabCount == 0)
		{
			arr_Contents[0].SetActive(false);
			if (myGuild.GetEntityData() == null)
			{
				BackEndManager.Instance.SetLoadingWindow();
				BackEndManager.Instance.GetGuildInfo(_action: GetMyGuildData);
			}
			else
			{
				myGuild.gameObject.SetActive(true);
				obj_GuildDataEmpty.SetActive(false);
			}
		}
		else
		{
			arr_Contents[0].SetActive(true);
			BackEndManager.Instance.SetLoadingWindow();
			BackEndManager.Instance.GetRandGuildList(GetRandGuildList);
		}
	}

	public void OnClick_GuildLeave(Guild_Entity _entity)
	{
		BackEndManager.Instance.LeaveGuild((isRecv)=>
		{
			if (isRecv == true)
			{
				myGuild.DeleteMyGuildData();
				myGuild.gameObject.SetActive(false);
				obj_GuildDataEmpty.SetActive(true);
				BackEndManager.Instance.ShowConfirmWindow("길드에서 탈퇴하셨습니다.");
			}
			else
				BackEndManager.Instance.ShowConfirmWindow("길드 탈퇴 요청 실패.");
		});
	}

	public void OnClick_GuildJoin(Guild_Entity _entity)
	{
		BackEndManager.Instance.ApplyGuild(_entity.GetEntityData().guildinDate,
		(isRecv) =>
		{
			if (isRecv == true)
				BackEndManager.Instance.ShowConfirmWindow("길드 가입 신청 성공");
			else
				BackEndManager.Instance.ShowConfirmWindow("길드 가입 신청 실패");
		});
	}

	public void OnClick_GuildCreateWindowOpen()
	{
		obj_GuildCreate.SetActive(true);
	}

	public void OnClick_GuilCreate()
	{
		BackEndManager.Instance.SetLoadingWindow();
		BackEndManager.Instance.CreateGuild(inputfield_GuildName.text,(result)=> 
		{
			BackEndManager.Instance.SetLoadingWindow(false);
			switch (result)
			{
				case GuildCreateResult.Success:
					BackEndManager.Instance.SetGuildRegistrationValue(true);
					BackEndManager.Instance.ShowConfirmWindow("길드 생성 성공", GuildCreateSuccess);
					break;
				default:
					BackEndManager.Instance.ShowConfirmWindow("길드 생성 실패");
					break;
			}
		});
	}
}
