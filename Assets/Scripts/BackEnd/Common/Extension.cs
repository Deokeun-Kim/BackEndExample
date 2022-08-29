using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine.Networking;
using UnityEngine.UI;

public static class Extension
{
    public static T EnumParse<T>(this string _this)
    {
		T returnValue;
		try
		{
			returnValue = (T)Enum.Parse(typeof(T), _this);
		}
		catch (System.Exception)
		{
			returnValue = (T)Enum.Parse(typeof(T), "NONE");
		}
		return returnValue;
    }

	public static string GetJsonString(this LitJson.JsonData _Data, string _key)
	{
		if (_Data.ContainsKey(_key) == true)
		{
			if (_Data[_key] == null)
				return string.Empty;

			if (string.IsNullOrEmpty(_Data[_key].ToString()) == true)
				return string.Empty;

			return _Data[_key].ToString();
		}
		else
			return string.Empty;
	}

	public static string RemoveWhiteSpaces(this string str)
	{
		return Regex.Replace(str, @"\s+", String.Empty);
	}

	public static IEnumerator R_GetLoadImage(this RawImage _this, string _link)
	{
		if (string.IsNullOrEmpty(_link) == true)
			yield break;

		UnityWebRequest www = UnityWebRequestTexture.GetTexture(_link);
		yield return www.SendWebRequest();
		_this.texture = DownloadHandlerTexture.GetContent(www);
	}

}