using System.Collections;
using UnityEngine;
using BackEnd;
using System;
using UnityEngine.SceneManagement;
using BackEnd.GlobalSupport;

public enum SceneType
{
    INTRO,
    MAIN,
    INGAME,
}

public partial class BackEndManager : MonoSingleton<BackEndManager>
{
    /// <summary>
    /// Initalized Property
    /// </summary>
    public bool isInitalized { get; set; }

    /// <summary>
    /// BackEnd Login ID Nick Name Property
    /// </summary>
    public string myNickName { get; private set; } = string.Empty;

    /// <summary>
    /// Backend Login ID InDate Property //InDate -> 뒤끝서버 유저 Unique Key
    /// </summary>
    public string myIndate { get; private set; } = string.Empty;

    /// <summary>
    /// BackEnd Debug Const String
    /// </summary>
    private const string DEBUG_ERROR = "statusCode : {0}\nErrorCode : {1}\nMessage : {2}";

    /// <summary>
    /// 매니저 초기화 : 뒤끝서버 초기화 콜백 실행
    /// </summary>
    public override void Initialized()
	{
        BackendReturnObject callback = Backend.Initialize(true);
        if (callback.IsSuccess() == true)
        {
            isInitalized = true;
            //StartCoroutine(nameof(R_BackEndAsyncPoll));
            #region Project CODE
            if (objLoadingWindow != null)
                obj_LoadingWindow = objLoadingWindow;

            if (objConfirmWindow != null)
            {
                obj_ConfirmWindow = objConfirmWindow;
                obj_ConfirmWindow.Initialized();
            }
            #endregion Project CODE
        }
        else
        {
            isInitalized = false;
        }
    }

	public void Update()
	{
        Backend.AsyncPoll();
    }

	/// <summary>
	/// 뒤끝서버 비동기 콜백 업데이트 코루틴 함수
	/// </summary>
	/// <returns></returns>
	private IEnumerator R_BackEndAsyncPoll()
	{
		while (isInitalized)
		{
            Backend.AsyncPoll();
            yield return null;
        }
    }
	
    /// <summary>
    /// 뒤끝 콘솔 App Status Check 함수
    /// </summary>
    /// <param name="actionStatus"></param>
    public void GetAppServerStatus(Action<bool> actionStatus)
    {
        Backend.Utils.GetServerStatus((callback) =>
        {
            string status = callback.GetReturnValuetoJSON()["serverStatus"].ToString();
            if (status.Equals("0") == true)
                actionStatus?.Invoke(true);
            else
                actionStatus?.Invoke(false);
        });
    }

     /// <summary>
     /// 뒤끝 콘솔에서 Version 정보 불러와, 현재 버전과 맞는지 체크
     /// </summary>
     /// <param name="actionNeedUpdated"></param>
    public void GetVersionInfo(Action<bool> actionNeedUpdated)
    {
        Backend.Utils.GetLatestVersion(callback =>
        {
            if (callback.IsSuccess() == false)
            {
                Debug.LogError("버전정보를 불러오는 데 실패하였습니다.\n" + callback);
                ShowConfirmWindow("버전정보를 불러오는 데 실패하였습니다.\n" + callback);
#if UNITY_EDITOR
                actionNeedUpdated?.Invoke(true);
#endif
                return;
            }

			string version = callback.GetReturnValuetoJSON()["version"].ToString();

            Version server = new Version(version);
            Version client = new Version(Application.version);

			var result = server.CompareTo(client);
            if (result == 0)
            {
                // 0 이면 두 버전이 일치
                actionNeedUpdated?.Invoke(true);
                return;
            }
            else if (result < 0)
            {
                // 0 미만이면 server 버전이 client 이전 버전
                // 검수를 넣었을 경우 여기에 해당된다.
                // ex) 검수버전 3.0.0, 라이브에 운용되고 있는 버전 2.0.0, 콘솔 버전 2.0.0
                actionNeedUpdated?.Invoke(true);
                return;
            }
            else
            {
                // 0보다 크면 server 버전이 client 이후 버전
                if (client == null)
                {
                    // 클라이언트가 null인 경우 예외처리
                    Debug.LogError("클라이언트 버전정보가 null 입니다.");
                    return;
                }
            }
            actionNeedUpdated?.Invoke(false);
        });
    }

    /// <summary>
    /// Backend Callback Object Debug Function
    /// </summary>
    /// <param name="_targetName"></param>
    /// <param name="_result"></param>
    /// <returns></returns>
    private string ShowDebugLog(string _targetName,BackendReturnObject _result = null)
    {
        if (_result == null)
            return $"{_targetName}";
        return string.Format("{0}\n{1}", _targetName, string.Format(DEBUG_ERROR, _result.GetStatusCode(), _result.GetErrorCode(), _result.GetMessage()));
    }

    #region Example Code
    public void SceneMove(SceneType _type)
    {
        switch (_type)
        {
            case SceneType.INTRO:
                SceneManager.LoadScene("0__INTRO");
                break;
            case SceneType.MAIN:
                SceneManager.LoadScene("1__MAIN");
                break;
            case SceneType.INGAME:
                SceneManager.LoadScene("1__MAIN");
                break;
        }
    }

    public CountryCode GetCountryCode(string _Code)
    {
        return CountryCodeDic.GetCountryName(_Code.ToUpper());
    }
    #endregion Example Code
}