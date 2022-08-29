using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class IntroManager : MonoBehaviour
{
	[Header(">>>EDITOR<<<")]
	public GameObject obj_Login;
	public GameObject obj_Start;
	public GameObject obj_NickNameWindow;
	public GameObject obj_CreteCustomWindow;
	public GameObject obj_LoginCustomWindow;
	public InputField input_LoginID;
	public InputField input_LoginPW;
	public InputField input_CreateID;
	public InputField input_CreatePW;
	public Button Btn_CustomCreate;
	public InputField input_NickName;
	public Button Btn_ChangeNick;

	private BackEndManager mybackEndManger;
	private BackEndManager backendManager 
	{ 
		get 
		{
			if (mybackEndManger == null)
				mybackEndManger = BackEndManager.Instance;
			return mybackEndManger;
		}
	}

	public IEnumerator Start()
	{
		bool isRecv = false;
		obj_NickNameWindow.SetActive(false);
		obj_CreteCustomWindow.SetActive(false);
		obj_LoginCustomWindow.SetActive(false);

		backendManager.GetAppServerStatus((isStatus) => 
		{
			isRecv = isStatus;
		});

		while (isRecv == false)
			yield return null;

#if !UNITY_EDITOR
		isRecv = false;
		backendManager.GetVersionInfo((isVersion) =>
		{
			isRecv = isVersion;
			if (isVersion == true)
				obj_Start.SetActive(true);
		});

		while (isRecv == false)
			yield return null;
#else
		obj_Start.SetActive(true);
#endif

	}

	public void OnClick_Start()
	{
		backendManager.BackendTokenLogin((isRecv) =>
		{
			if (isRecv == true)
				SceneMove_MainScene();
			else
			{
				obj_Start.SetActive(false);
				obj_Login.SetActive(true);
			}
		});
	}

	public void OnClick_GuestLogin()
	{
		backendManager.GuestLogin((isRecv)=>
		{
			if(isRecv == true)
				SceneMove_MainScene();
		}, OnClick_OpenWindowNickName);
	}

	#region WINDOW NICK NAME
	public void OnClick_OpenWindowNickName()
	{
		obj_CreteCustomWindow.SetActive(false);
		obj_LoginCustomWindow.SetActive(false);
		input_NickName.text = string.Empty;
		Btn_ChangeNick.interactable = false;
		obj_NickNameWindow.SetActive(true);
	}

	public void OnClick_NickCheck()
	{
		backendManager.CheckNickNameDuplication(input_NickName.text,
			(isRecv)=>Btn_ChangeNick.interactable = isRecv);
	}

	public void OnClick_NickNameUpdate()
	{
		backendManager.UpdateNickname(input_NickName.text, (isRecv) => 
		{
			if(isRecv == true)
				SceneMove_MainScene();
		});
	}

	public void OnChangeValue_NickName(string _value)
	{
		Btn_ChangeNick.interactable = false;
	}
	#endregion WINDOW NICK NAME

	#region CUSTOM UI
	public void OnClick_OpenWindowLoginCustomID()
	{
		input_LoginID.text = string.Empty;
		input_LoginPW.text = string.Empty;
		obj_LoginCustomWindow.SetActive(true);
		obj_CreteCustomWindow.SetActive(false);
	}

	public void OnClick_OpenWindowCreateCustomID()
	{
		input_CreateID.text = string.Empty;
		input_CreatePW.text = string.Empty;
		obj_LoginCustomWindow.SetActive(false);
		obj_CreteCustomWindow.SetActive(true);
	}

	public void OnClick_LoginCustomID()
	{
		if (string.IsNullOrEmpty(input_LoginID.text) || string.IsNullOrEmpty(input_LoginPW.text))
			return;

		backendManager.LoginCustomID(input_LoginID.text, input_LoginPW.text, (isRecv) =>
		{
			if (isRecv == true)
				SceneMove_MainScene();
		}, OnClick_OpenWindowNickName);
	}
	public void OnClick_CreateCustomID()
	{
		if (string.IsNullOrEmpty(input_CreateID.text) || string.IsNullOrEmpty(input_CreatePW.text))
			return;

		backendManager.CustomSignUp(input_CreateID.text, input_CreatePW.text, (isRecv) =>
		{
			if (isRecv == true)
			{
				obj_CreteCustomWindow.SetActive(false);
				obj_LoginCustomWindow.SetActive(true);
			}
		});
	}
	#endregion CUSTOM UI

	private void SceneMove_MainScene()
	{
		backendManager.SceneMove(SceneType.MAIN);
	}
}
