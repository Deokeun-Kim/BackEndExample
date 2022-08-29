using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 메일 박스 팝업
/// </summary>
public class Popup_MailBox : BasePopup
{
	public MailBox_Entity baseEntity;
	public List<MailBox_Entity> poolEntityList;

	public UnityEngine.UI.Button btn_GetAllReward;
	private List<MailPostData> postDataList;
	private MailBox_Entity targetEntity;

	public override void Init()
	{
		popupType = popup_type.MAILBOX;
		baseEntity.Hide();
		poolEntityList = new List<MailBox_Entity>();
		btn_GetAllReward.onClick.AddListener(OnClick_GetRewardAllMailBoxEntity);
	}

	public override void Show()
	{
		base.Show();
		BackEndManager.Instance.GetAllPostList(EmptyList_SettingByData);
	}

	public override void Hide()
	{
		base.Hide();
		for (int i = 0; i < poolEntityList.Count; i++)
			poolEntityList[i].Hide();
	}

	protected MailBox_Entity GetInActiveEntity(int Count)
	{
		if (poolEntityList.Count > Count)
			return poolEntityList[Count];

		MailBox_Entity newEntity = Instantiate(baseEntity,baseEntity.transform.parent);
		poolEntityList.Add(newEntity);
		return newEntity;
	}

	#region OnClick_Entity

	public void OnClick_GetRewardMailBoxEntity(MailBox_Entity _targetEntity)
	{
		targetEntity = _targetEntity;
		BackEndManager.Instance.GetRecvMail(targetEntity.GetEntityData(), GetRewardMailBoxEntity);
	}

	public void OnClick_GetRewardAllMailBoxEntity()
	{
		BackEndManager.Instance.GetAllRecvMail(GetRewardAllMailBoxEntity);
	}

	#endregion OnClick_Entity

	private void EmptyList_SettingByData(List<MailPostData> _postData)
	{
		if (_postData.Count <= 0)
		{
			BackEndManager.Instance.ShowConfirmWindow("수신할 메일이 없습니다.");
			return;
		}

		btn_GetAllReward.gameObject.SetActive(true);

		postDataList = new List<MailPostData>(_postData);
		for (int i = 0; i < postDataList.Count; i++)
		{
			MailBox_Entity item = GetInActiveEntity(i);
			item.SetEntity(postDataList[i]);
		}
	}

	private void GetRewardMailBoxEntity(bool isRecv)
	{
		if (isRecv == false)
		{
			BackEndManager.Instance.ShowConfirmWindow("메일 수령에 실패하였습니다");
			targetEntity = null;
			return;
		}
		else
		{
			List<MailPostItemData> rewardData = targetEntity.GetItemDataList();
			Dictionary<string,int> itemRewardInfo = new Dictionary<string, int>();
			for (int i = 0; i < rewardData.Count; i++)
			{
				if (itemRewardInfo.ContainsKey(rewardData[i].memo) == false)
					itemRewardInfo.Add(rewardData[i].memo, rewardData[i].itemCount);
				else
					itemRewardInfo[rewardData[i].memo] += rewardData[i].itemCount;
			}

			string rewardString = $"아이템 보상 수령 \n {string.Join(",", itemRewardInfo.Keys.ToList())}" +
				$"\n{string.Join(",", itemRewardInfo.Values.ToList())}";

			BackEndManager.Instance.ShowConfirmWindow($"{rewardString}");
		}
	}

	private void GetRewardAllMailBoxEntity(bool isRecv)
	{
		if (isRecv == false)
		{
			BackEndManager.Instance.ShowConfirmWindow("메일 수령에 실패하였습니다");
			return;
		}

		btn_GetAllReward.gameObject.SetActive(false);

		Dictionary<string,int> itemRewardInfo = new Dictionary<string, int>();
		for (int i = 0; i < poolEntityList.Count; i++)
		{
			if (poolEntityList[i].isGetReward == true)
				continue;

			List<MailPostItemData> rewardData = poolEntityList[i].GetItemDataList();
			for (int k = 0; k < rewardData.Count; k++)
			{
				if (itemRewardInfo.ContainsKey(rewardData[k].memo) == false)
					itemRewardInfo.Add(rewardData[k].memo, rewardData[k].itemCount);
				else
					itemRewardInfo[rewardData[k].memo] += rewardData[k].itemCount;
			}
		}

		string rewardString = $"아이템 보상 수령 \n {string.Join(",", itemRewardInfo.Keys.ToList())}" +
				$"\n{string.Join(",", itemRewardInfo.Values.ToList())}";

		BackEndManager.Instance.ShowConfirmWindow($"{rewardString}");
	}
}