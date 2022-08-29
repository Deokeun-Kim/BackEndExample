using System;
using BackEnd;
using UnityEngine;

public partial class BackEndManager
{
	public bool IsLogin { get; set; } // 로그인 여부 확인 Property

	#region CUSTOM ID
	/// <summary>
	/// 커스텀 아이디 로그인 및 닉네임 설정
	/// </summary>
	/// <param name="_ID">아이디</param>
	/// <param name="_PW">비밀번호</param>
	/// <param name="_onAction">계정 생성 성공여부 Delegate Function</param>
	/// <param name="_onNickNameEmpty"> 계정 닉네임 설정 Delegate Function </param>
	public void LoginCustomID(string _ID, string _PW, Action<bool> _onAction = null, Action _onNickNameEmpty = null)
	{
		Backend.BMember.CustomLogin(_ID, _PW, callback => {
			if (callback.IsSuccess() == false)
			{
				_onAction?.Invoke(callback.IsSuccess());
				return;
			}

			IsLogin = true;
			Debug.Log(ShowDebugLog("CustomLogin", callback));
			GetUserInfo((recvNick) =>
			{
				if (string.IsNullOrEmpty(recvNick) == true)
					_onNickNameEmpty?.Invoke();
				else
					_onAction?.Invoke(callback.IsSuccess());
				IsLogin = true;
			});
		});
	}

	/// <summary>
	/// 커스텀 아이디 생성
	/// </summary>
	/// <param name="_ID"></param>
	/// <param name="_PW"></param>
	/// <param name="_onAction"></param>
	public void CustomSignUp(string _ID, string _PW, Action<bool> _onAction = null)
	{
		Backend.BMember.CustomSignUp(_ID, _PW, callback => 
		{
			Debug.Log(ShowDebugLog("CustomSignUp", callback));
			_onAction?.Invoke(callback.IsSuccess());
		});
	}

	/// <summary>
	/// 커스텀 계정 정보에 이메일 추가 (ID / PW 찾기 가능)
	/// </summary>
	/// <param name="_email"></param>
	/// <param name="_onAction"></param>
	public void UpdateCustomEmail(string _email, Action<bool> _onAction = null)
	{
		Backend.BMember.UpdateCustomEmail(_email, (callback) =>
		{
			Debug.Log(ShowDebugLog("UpdateCustomEmail", callback));
			_onAction?.Invoke(callback.IsSuccess());
		});
	}

	/// <summary>
	/// 계정 정보의 이메일을 기반으로 ID 찾기
	/// </summary>
	/// <param name="_email"></param>
	/// <param name="_onAction"></param>
	public void FindCustomID(string _email, Action<bool> _onAction = null)
	{
		Backend.BMember.FindCustomID(_email, (callback) =>
		{
			Debug.Log(ShowDebugLog("FindCustomID", callback));
			_onAction?.Invoke(callback.IsSuccess());
		});
	}

	/// <summary>
	/// 현재 접속된 아이디의 비밀 번호 확인 ( 계정정보 수정 등 추가 보안사항 체크 시 필요할 것으로 예상 )
	/// </summary>
	/// <param name="_PW"></param>
	/// <param name="_onAction"></param>
	public void ComfirmCustomPW(string _PW, Action<bool> _onAction = null)
	{
		if (IsLogin == false)
		{
			_onAction?.Invoke(false);
			return;
		}

		Backend.BMember.ConfirmCustomPassword(_PW, (callback) =>
		{
			Debug.Log(ShowDebugLog("ComfirmCustomPW", callback));
			_onAction?.Invoke(callback.IsSuccess());
		});
	}

	/// <summary>
	/// 계정 정보의 이메일과 아이디를 기반으로 PW 초기화 (임시 PW 이메일로 전송) 
	/// </summary>
	/// <param name="_customID"></param>
	/// <param name="_userEmail"></param>
	/// <param name="_onAction"></param>
	public void ResetCustomPW(string _customID, string _userEmail,Action<bool> _onAction = null)
	{
		if (string.IsNullOrEmpty(_userEmail) == true || IsLogin == false)
		{
			_onAction?.Invoke(false);
			return;
		}

		Backend.BMember.ResetPassword(_customID, _userEmail, (callback) =>
		{
			// 이후 처리
			Debug.Log(ShowDebugLog("ResetCustomPW", callback));
			_onAction?.Invoke(callback.IsSuccess());
		});
	}
	#endregion CUSTOM ID

	#region GUEST ID
	/// <summary>
	/// 현재 Device에 게스트 계정이 생성되어 있는지 체크
	/// </summary>
	/// <returns></returns>
	public bool GetCheckGuestID()
	{
		string guestID = Backend.BMember.GetGuestID();
		return string.IsNullOrEmpty(guestID) == false;
	}

	/// <summary>
	/// 게스트 로그인 시도
	/// </summary>
	/// <param name="_action">성공여부 콜백 Delegate</param>
	/// <param name="_actionNickName"> Nickname Empty일때 닉네임 설정 </param>
	public void GuestLogin(Action<bool> _action, Action _actionNickName)
	{
		Backend.BMember.GuestLogin("", callback =>
		{
			Debug.Log(ShowDebugLog("GuestLogin", callback));
			if (callback.IsSuccess() == false)
			{
				_action?.Invoke(callback.IsSuccess());
				return;
			}

			GetUserInfo((recvNick) =>
			{
				if (string.IsNullOrEmpty(recvNick) == true)
					_actionNickName.Invoke();
				else
					_action?.Invoke(callback.IsSuccess());
				IsLogin = true;
			});
		});
	}

	/// <summary>
	/// 게스트 계정 삭제( 유예기간 없음 )
	/// </summary>
	public void DeleteGuestID()
	{
		Backend.BMember.DeleteGuestInfo();
	}
	#endregion GUEST ID

	#region ACCOUNT DELETE
	/// <summary>
	/// 로그인한 계정 삭제 (유예 시간 설정 가능)
	/// </summary>
	/// <param name="_graceHours"></param>
	/// <param name="_action"></param>
	public void AccountDelete(int _graceHours, Action<bool> _action)
	{
		if (IsLogin == false)
		{
			Debug.Log("로그인 후 회원 탈퇴 가능");
			return;
		}

		Backend.BMember.WithdrawAccount(_graceHours, callback =>
		{
			Debug.Log(ShowDebugLog("AccountDelete", callback));
			_action.Invoke(callback.IsSuccess());
		});
	}
	#endregion ACCOUNT DELETE

	#region UPDATE USER INFO
	/// <summary>
	/// 닉네임 생성 및 변경 함수
	/// </summary>
	/// <param name="_nickname"></param>
	/// <param name="_func"></param>
	public void UpdateNickname(string _nickname, Action<bool> _func)
	{
		Backend.BMember.UpdateNickname(_nickname, (callback) =>
		{
			Debug.Log(ShowDebugLog("UpdateNickname", callback));
			_func.Invoke(callback.IsSuccess());
		});
	}

	/// <summary>
	/// 닉네임 중복체크 함수
	/// </summary>
	/// <param name="_nickname"></param>
	/// <param name="_func"></param>
	public void CheckNickNameDuplication(string _nickname, Action<bool> _func)
	{
		Backend.BMember.CheckNicknameDuplication(_nickname, (callback) =>
		{
			Debug.Log(ShowDebugLog("CheckNickNameDuplication", callback));
			_func.Invoke(callback.IsSuccess());
		});
	}
	#endregion  UPDATE USER INFO

	/// <summary>
	/// 최근에 로그인한 아이디로 자동로그인 처리
	/// </summary>
	/// <param name="func"></param>
	public void BackendTokenLogin(Action<bool> func)
	{
		Backend.BMember.LoginWithTheBackendToken( (callback) =>
		{
			Debug.Log(ShowDebugLog("BackendTokenLogin", callback));
			func(callback.IsSuccess());
		});
	}
	/// <summary>
	/// 로그아웃
	/// </summary>
	/// <param name="func"></param>
	public void Logout(Action<bool> func)
	{
		Backend.BMember.Logout((callback) =>
		{
			Debug.Log(ShowDebugLog("Logout", callback));
			func(callback.IsSuccess());
		});
	}

	/// <summary>
	/// 유저 정보 확인 (닉네임 Delegate CallBack)
	/// </summary>
	/// <param name="_actionNickCheck"></param>
	private void GetUserInfo(Action<string> _actionNickCheck = null)
	{
		Backend.BMember.GetUserInfo((callback)=>
		{
			if (callback.IsSuccess() == false)
			{
				GetUserInfo();
				return;
			}

			LitJson.JsonData userInfoJson = callback.GetReturnValuetoJSON()["row"];
			UserInfo userInfo = new UserInfo();
			userInfo.gamerId = userInfoJson.GetJsonString("gamerId");
			userInfo.countryCode = userInfoJson.GetJsonString("countryCode");
			userInfo.nickname = userInfoJson.GetJsonString("nickname");
			userInfo.inDate = userInfoJson.GetJsonString("inDate");
			userInfo.emailForFindPassword = userInfoJson.GetJsonString("emailForFindPassword");
			userInfo.subscriptionType = userInfoJson.GetJsonString("subscriptionType");
			userInfo.federationId = userInfoJson.GetJsonString("federationId");
			Debug.Log(userInfo.ToString());
			_actionNickCheck?.Invoke(userInfo.nickname);
		});
	}

}

public class UserInfo
{
	public string gamerId; // 뒤끝서버 UserID ID
	public string countryCode; // 국가 코드
	public string nickname; // 닉네임
	public string inDate; // User inDate = 뒤끝서버 유저 Uniqe Key 
	public string emailForFindPassword; // 유저의 Email 주소
	public string subscriptionType; 
	public string federationId;

	public override string ToString()
	{
		return $"gamerId: {gamerId}\n" +
		$"countryCode: {countryCode}\n" +
		$"nickname: {nickname}\n" +
		$"inDate: {inDate}\n" +
		$"emailForFindPassword: {emailForFindPassword}\n" +
		$"subscriptionType: {subscriptionType}\n" +
		$"federationId: {federationId}\n";
	}
}