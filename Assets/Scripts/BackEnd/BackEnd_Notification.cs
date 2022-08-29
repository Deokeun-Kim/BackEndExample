using BackEnd;
using UnityEngine;
using System;

public enum NotificationType
{
	RequestFriend,
	RejectFriend,
	ApplyGuild,
	RejectGuild,
	RecvMessage,
	RecvMail,
}

public partial class BackEndManager
{
	private Action<bool,string,string> actionUserJoin;
	private Action<string,string> actionFriendJoin;
	private Action actionRequestFriend;
	private Action actionRejectFriend;
	private Action actionApplyGuild;
	private Action actionRejectGuild;
	private Action actionRecvMessage;
	private Action actionRecvMail;

	public void InitNotification()
	{
		Backend.Notification.OnAuthorize = (bool Result, string Reason) => {
			Debug.Log(string.Format("Notification.Connect \n Result : {0} \n Reason : {1}", Result, Reason));
		};

		Backend.Notification.OnDisConnect = (string Reason) => {
			Debug.Log(string.Format("Notification.OnDisConnect \n Reason : {0}", Reason));
		};

		Backend.Notification.OnFriendConnected = (string inDate, string nickname) => {
			Debug.Log(nickname + "님이 연결되었습니다");
			actionFriendJoin?.Invoke(inDate, nickname);
		};

		Backend.Notification.OnIsConnectUser = (bool isConnect, string nickName, string gamerIndate) => {
			Debug.Log($"{nickName} / {gamerIndate} 접속 여부 확인 : " + isConnect);
			actionUserJoin?.Invoke(isConnect, nickName, gamerIndate);
		};

		Backend.Notification.OnReceivedFriendRequest = () => {
			Debug.Log("친구 요청이 도착했습니다!");
			actionRequestFriend?.Invoke();
		};

		Backend.Notification.OnRejectedFriendRequest = () => {
			Debug.Log("친구 요청이 거절당했습니다...");
			actionRejectFriend?.Invoke();
		};

		Backend.Notification.OnReceivedGuildApplicant = () => {
			Debug.Log("새 길드 가입 신청이 도착했습니다!");
			actionApplyGuild?.Invoke();
		};

		Backend.Notification.OnRejectedGuildJoin = () => {
			Debug.Log("길드 가입 신청이 거절당했습니다...");
			actionRejectGuild?.Invoke();
		};

		Backend.Notification.OnReceivedMessage = () => {
			Debug.Log("새 쪽지가 도착했습니다!");
			actionRecvMessage?.Invoke();
		};

		Backend.Notification.OnReceivedUserPost = () => {
			Debug.Log("새 유저 우편이 도착했습니다!");
			actionRecvMail?.Invoke();
		};
	}

	public void Connect_Notification()
	{
		Backend.Notification.Connect();
	}

	public void DisConnect_Notification()
	{
		Backend.Notification.DisConnect();
	}

	public void SetNotification_JoinUser(string _userInDate)
	{
		Backend.Notification.UserIsConnectByIndate(_userInDate);
	}

	public void SetNotification_UserJoinAction(Action<bool, string, string> _actionUserJoin)
	{
		actionUserJoin = _actionUserJoin;
	}

	public void SetNotification_FriendJoinAction(Action<string, string> _actionFriendJoin)
	{
		actionFriendJoin = _actionFriendJoin;
	}

	public void SetNotification_FriendRequest(Action _actionRejectFriend)
	{
		actionRejectFriend = _actionRejectFriend;
	}

	public void SetNotification_NoticeType(NotificationType _type, Action _action)
	{
		switch (_type)
		{
			case NotificationType.RequestFriend:
				actionRequestFriend = _action;
				break;
			case NotificationType.RejectFriend:
				actionRejectFriend = _action;
				break;
			case NotificationType.ApplyGuild:
				actionApplyGuild = _action;
				break;
			case NotificationType.RejectGuild:
				actionRejectGuild = _action;
				break;
			case NotificationType.RecvMessage:
				actionRecvMessage = _action;
				break;
			case NotificationType.RecvMail:
				actionRecvMail = _action;
				break;
		}
	}
}
