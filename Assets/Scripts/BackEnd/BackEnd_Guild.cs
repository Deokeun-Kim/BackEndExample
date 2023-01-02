using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using System;
using LitJson;

/// <summary>
/// 길드 생성에 따른 결과 string Enum 값
/// </summary>
public enum GuildCreateResult
{
    Success,
    BadParameterException,
    UndefinedParameterException,
    ForbiddenError,
    DuplicatedParameterException,
    PreconditionFailed,
}

public partial class BackEndManager
{
    [HideInInspector] GuildInfoData myGuildInfoData;
    [HideInInspector] private string guildOffset = string.Empty;
    [HideInInspector] private int indexLoadCount = 10;
    [HideInInspector] private int indexRandLoadCount = 10;

    #region GUILD CREATE & APPLY & LEAVE
    /// <summary>
    /// 길드 생성.
    /// </summary>
    /// <param name="_guildName"></param>
    /// <param name="_action"></param>
    public void CreateGuild(string _guildName, Action<GuildCreateResult> _action = null)
    {
        Param goods = new Param();

		// XXX: Input Guild Goods Base Value (예제)
		goods.Add("level", 0);
		goods.Add("buf", 1);

		Backend.Guild.CreateGuildV3(_guildName, goods.Count>0?goods.Count:2, (callback) =>
        {
            GuildCreateResult result = GuildCreateResult.Success;
            try
			{
                string errorCode = callback.GetErrorCode();
                if (string.IsNullOrEmpty(errorCode) == true)
                    result = GuildCreateResult.Success;
                else
                    result = (GuildCreateResult)Enum.Parse(typeof(GuildCreateResult), errorCode);
            }
			catch (Exception e)
			{
                Debug.LogError(e.Message);
            }

            Debug.Log(ShowDebugLog("CreateGuild", callback));
            _action?.Invoke(result);
        });
    }

    /// <summary>
    /// 길드 가입 신청
    /// </summary>
    /// <param name="_GuildInDate"></param>
    /// <param name="_action"></param>
    public void ApplyGuild(string _GuildInDate, Action<bool> _action = null)
    {
        Backend.Guild.ApplyGuildV3(_GuildInDate, (callback) =>
        {
            Debug.Log(ShowDebugLog("ApplyGuild", callback));
            _action?.Invoke(callback.IsSuccess());
        });
    }

    /// <summary>
    /// 길드 나가기
    /// </summary>
    /// <param name="_action"></param>
    public void LeaveGuild(Action<bool> _action = null)
    {
        Backend.Guild.WithdrawGuildV3((callback) =>
        {
            Debug.Log(ShowDebugLog("LeaveGuild", callback));
            _action?.Invoke(callback.IsSuccess());
        });
    }

    #endregion GUILD CREATE & APPLY & LEAVE

    #region GUILD INFO

    #region INFO LIST
    /// <summary>
    /// 모든 길드 리스트 조회(guildOffset)
    /// </summary>
    /// <param name="_action"></param>
    public void GetGuildList(Action<List<GuildInfoData>> _action)
    {
        if (string.IsNullOrEmpty(guildOffset) == true)
        {
            Backend.Guild.GetGuildListV3(indexLoadCount, (callback) =>
            {
                Result_GetGuildList(callback, _action);
            });
        }
        else
        {
            Backend.Guild.GetGuildListV3(indexLoadCount, guildOffset, (callback) =>
            {
                Result_GetGuildList(callback, _action);
            });
        }
    }

    /// <summary>
    /// 길드리스트 offset 변수 초기화.
    /// </summary>
    public void ResetOffset()
    {
        guildOffset = string.Empty;
    }

    /// <summary>
    /// Guild List Json Data -> GuildInfoData Parsing
    /// </summary>
    /// <param name="_result"></param>
    /// <param name="_action"></param>
    private void Result_GetGuildList(BackendReturnObject _result, Action<List<GuildInfoData>> _action = null)
    {
        Debug.Log(ShowDebugLog("GetGuildList", _result));

        List<GuildInfoData> guildDataList = new List<GuildInfoData>();
        if (_result.IsSuccess())
        {
            guildOffset = _result.FirstKeystring();
            JsonData json = _result.FlattenRows();
            for (int i = 0; i < json.Count; i++)
            {
                GuildInfoData tempData = new GuildInfoData(json[i]);
                guildDataList.Add(tempData);
            }
        }
        _action?.Invoke(guildDataList);
    }

    /// <summary>
    /// 길드 리스트 랜덤 조회 (indexLoadCount 수 만큼 로드)
    /// </summary>
    /// <param name="_action"></param>
    public void GetRandGuildList(Action<List<GuildInfoData>> _action)
    {
        Backend.Guild.GetRandomGuildInfoV3(indexLoadCount, (callback) =>
        {
            List<GuildInfoData> guildDataList = new List<GuildInfoData>();
            if (callback.IsSuccess())
            {
                JsonData json = callback.FlattenRows();
                for (int i = 0; i < json.Count; i++)
                {
                    GuildInfoData tempData = new GuildInfoData(json[i]);
                    guildDataList.Add(tempData);
                }
            }
            _action?.Invoke(guildDataList);
        });
    }

    /// <summary>
    /// 길드 리스트 랜덤 조회 ( MetaData / Gap 값 기준으로 indexloadCount 수 만큼 로드 )
    /// </summary>
    /// <param name="_metaData"></param>
    /// <param name="gap"></param>
    /// <param name="_action"></param>
    public void GetRandGuildListByMetaData(string _metaData,int gap,Action<List<GuildInfoData>> _action = null)
    {
        if (myGuildInfoData == null || myGuildInfoData.metaDataDic.ContainsKey(_metaData) == false)
        {
            Debug.Log(ShowDebugLog("GetRandGuildListByMetaData"));
            _action?.Invoke(null);
            return;
        }

        // 랜덤 조회 기능 중 기준이 되는 값을 내 길드의 메타데이터로 설정.
        int value = myGuildInfoData.metaDataDic[_metaData];
        Backend.RandomInfo.GetRandomData(RandomType.Guild, _metaData, value, gap, indexRandLoadCount, callback =>
        {
            Debug.Log(ShowDebugLog("GetRandGuildListByMetaData", callback));
            List<GuildInfoData> guildDataList = new List<GuildInfoData>();
            if (callback.IsSuccess())
            {
                JsonData json = callback.FlattenRows();
                for (int i = 0; i < json.Count; i++)
                {
                    GuildInfoData tempData = new GuildInfoData(json[i]);
                    guildDataList.Add(tempData);
                }
            }
            _action?.Invoke(guildDataList);
        });
    }
    #endregion INFO LIST

    #region INFO ONE GUILD
    /// <summary>
    /// 해당 길드의 InDate 값 받아오기.
    /// </summary>
    /// <param name="_action"></param>
    public void GetGuildInDate(string _guildName,Action<string> _action = null)
    {
        Backend.Guild.GetGuildIndateByGuildNameV3(_guildName, (callback) =>
        {
            string guildIndate = string.Empty;
            if (callback.IsSuccess())
                guildIndate = callback.GetReturnValuetoJSON()["guildInDate"]["S"].ToString();
            ShowDebugLog("GetGuildInDate", callback);
            _action?.Invoke(guildIndate);
        });
    }

    /// <summary>
    /// InDate 값으로 길드 정보 조회
    /// </summary>
    /// <param name="_otherGuildinDate"></param>
    /// <param name="_action"></param>
    public void GetGuildInfo(string _otherGuildinDate = "",Action<GuildInfoData> _action = null)
    {
        if (string.IsNullOrEmpty(_otherGuildinDate) == true)
        {
            // My Guild Info Return;
            Backend.Guild.GetMyGuildInfoV3((callback) =>
            {
                GuildInfoData tempData = null;
                if (callback.IsSuccess())
                {
                    JsonData json = callback.GetFlattenJSON();
                    tempData = new GuildInfoData(json["guild"]);
                }
                Debug.Log(ShowDebugLog("GetMyGuildInfo", callback));
                myGuildInfoData = tempData;
                _action?.Invoke(tempData);
            });
        }
        else
        {
            Backend.Guild.GetGuildInfoV3(_otherGuildinDate, callback =>
            {
                GuildInfoData tempData = null;
                if (callback.IsSuccess())
                {
                    JsonData json = callback.GetFlattenJSON();
                    tempData = new GuildInfoData(json["guild"]);
                }
                Debug.Log(ShowDebugLog("GetOtherGuildInfo", callback));
                _action?.Invoke(tempData);
            });
        }
    }

    /// <summary>
    /// Guild InDate값으로 해당 길드의 맴버 리스트 조회
    /// </summary>
    /// <param name="_guildinDate"></param>
    /// <param name="_action"></param>
    public void GetGuildMemberList(string _guildinDate, Action<List<GuildMemberData>> _action = null)
    {
        Backend.Guild.GetGuildMemberListV3(_guildinDate, guildOffset, (callback) =>
        {
            List<GuildMemberData> resultList = new List<GuildMemberData>();
            if (callback.IsSuccess())
            {
                JsonData guildMemberJson = callback.FlattenRows();
                for (int i = 0; i < guildMemberJson.Count; i++)
                {
                    GuildMemberData newData = new GuildMemberData(guildMemberJson[i]);
                    resultList.Add(newData);
                }
            }
            Debug.Log(ShowDebugLog("GetGuildMemberList", callback));
            _action?.Invoke(resultList);
        });
    }
    #endregion INFO ONE GUILD

    #endregion GUILD INFO

    #region GUILD GOODS

    /// <summary>
    /// 길드 재화 기부하기
    /// </summary>
    /// <param name="_type"></param>
    /// <param name="_amount"></param>
    /// <param name="_action"></param>
    public void ContributeGuildGoods(goodsType _type, int _amount, Action<bool> _action = null)
    {
        Backend.Guild.ContributeGoodsV3(_type, _amount, (callback) =>
        {
            Debug.Log(ShowDebugLog("ContributeGuildGoods", callback));
            _action?.Invoke(callback.IsSuccess());
        });
    }

    /// <summary>
    /// 내가 속한 길드의 재화 정보 리스트 조회
    /// </summary>
    /// <param name="_action"></param>
    public void GetMyGuildGoodsInfo(Action<Dictionary<string, GuildGoodsInfo>> _action = null)
    {
        Backend.Guild.GetMyGuildGoodsV3((callback) =>
        {
            Debug.Log(ShowDebugLog("GetMyGuildGoodsInfo", callback));
            _action?.Invoke(GetJsonByGuildGoodsInfo(callback));
        });
    }

    /// <summary>
    /// Guild InDate 정보로 해당 길드의 재화 정보 리스트 조회.
    /// </summary>
    /// <param name="_guildInDate"></param>
    /// <param name="_action"></param>
    public void GetOtherGuildGoodsInfo(string _guildInDate, Action<Dictionary<string, GuildGoodsInfo>> _action = null)
    {
        Backend.Guild.GetGuildGoodsByIndateV3(_guildInDate, (callback) =>
        {
            Debug.Log(ShowDebugLog("GetOtherGuildGoodsInfo", callback));
            _action?.Invoke(GetJsonByGuildGoodsInfo(callback));
        });
    }

    /// <summary>
    /// 뒤끝 Callback 객체를 길드재화 정보 딕셔너리로 변환
    /// </summary>
    /// <param name="_callback"></param>
    /// <returns></returns>
    private Dictionary<string, GuildGoodsInfo> GetJsonByGuildGoodsInfo(BackendReturnObject _callback)
    {
        Dictionary<string, GuildGoodsInfo> goodsDictionary = null;
        if (_callback.IsSuccess())
        {
            goodsDictionary = new Dictionary<string, GuildGoodsInfo>();
            JsonData goodsJson = _callback.GetFlattenJSON()["goods"];
            foreach (var column in goodsJson.Keys)
            {
                if (column.Contains("totalGoods"))
                {
                    GuildGoodsInfo guildGoodsInfo = new GuildGoodsInfo(goodsJson,column);
                    goodsDictionary.Add(column, guildGoodsInfo);
                }
            }
        }
        return goodsDictionary;
    }

    #endregion GUILD GOODS

    #region ONLY GUILD MASTER

    /// <summary>
    /// 길드 마스터 변경
    /// </summary>
    /// <param name="_userinDate"></param>
    /// <param name="_action"></param>
    public void NominateGuildMaster(string _userinDate, Action<bool> _action = null)
    {
        Backend.Guild.NominateMasterV3(_userinDate, (callback) =>
        {
            Debug.Log(ShowDebugLog($"NominateGuildMaster ({_userinDate})", callback));
            _action?.Invoke(callback.IsSuccess());
        });
    }

    /// <summary>
    /// 부 길드 마스터 위임
    /// </summary>
    /// <param name="_userinDate"></param>
    /// <param name="_action"></param>
    public void NominateViceMaster(string _userinDate, Action<bool> _action = null)
    {
        Backend.Guild.NominateViceMasterV3(_userinDate, (callback) =>
        {
            Debug.Log(ShowDebugLog($"NominateViceMaster ({_userinDate})", callback));
            _action?.Invoke(callback.IsSuccess());
        });
    }

    /// <summary>
    /// 부 길마 권한 해제
    /// </summary>
    /// <param name="_userinDate"></param>
    /// <param name="_action"></param>
    public void ReleaseViceMaster(string _userinDate, Action<bool> _action = null)
    {
        Backend.Guild.ReleaseViceMasterV3(_userinDate, (callback) =>
        {
            Debug.Log(ShowDebugLog($"ReleaseViceMaster ({_userinDate})", callback));
            _action?.Invoke(callback.IsSuccess());
        });
    }

    /// <summary>
    /// 길드 재화 사용
    /// </summary>
    /// <param name="_type"></param>
    /// <param name="_amount"></param>
    /// <param name="_action"></param>
    public void UseGuildGoods(goodsType _type, int _amount, Action<bool> _action = null)
    {
        Backend.Guild.UseGoodsV3(_type, -_amount, (callback) =>
        {
            Debug.Log(ShowDebugLog($"UseGuildGoods ({_type})", callback));
            _action?.Invoke(callback.IsSuccess());
        });
    }

    /// <summary>
    /// 길드 가입 설정 변경 (즉시 가입 or 승인가입) -> true : 즉시가입.
    /// </summary>
    /// <param name="_immediateRegistration"></param>
    /// <param name="_action"></param>
    public void SetGuildRegistrationValue(bool _immediateRegistration, Action<bool> _action = null)
    {
        Backend.Guild.SetRegistrationValueV3(_immediateRegistration, (callback) =>  // 즉시 가입 설정
        {
            Debug.Log(ShowDebugLog($"SetGuildRegistrationValue ({_immediateRegistration})", callback));
            _action?.Invoke(callback.IsSuccess());
        });
    }

    /// <summary>
    /// 길드의 국가 정보 설정
    /// </summary>
    /// <param name="_contryCode"></param>
    /// <param name="_action"></param>
    public void ContryUpdateGuild(BackEnd.GlobalSupport.CountryCode _contryCode, Action<bool> _action = null)
    {
        Backend.Guild.UpdateCountryCodeV3(_contryCode, (callback) =>
        {
            Debug.Log(ShowDebugLog($"ContryUpdateGuild ({_contryCode})", callback));
            _action?.Invoke(callback.IsSuccess());
        });
    }

    #endregion ONLY GUILD MASTER

    #region MASTER & VICE MASTER

    /// <summary>
    /// 길드 가입 신청 리스트 조회
    /// </summary>
    /// <param name="_indexLoadCount"> 해당값 만큼 조회 </param>
    /// <param name="_action"></param>
    public void GetGuildApplyList(int _indexLoadCount, Action<List<ApplyUserData>> _action = null)
    {
        Backend.Guild.GetApplicantsV3(_indexLoadCount, (callback) =>
        {
            Debug.Log(ShowDebugLog($"GetGuildApplyList", callback));
            List<ApplyUserData> resultList = new List<ApplyUserData>();
            if (callback.IsSuccess())
            {
                JsonData applyUserJson = callback.FlattenRows();
				for (int i = 0; i < applyUserJson.Count; i++)
				{
                    ApplyUserData applyuserData = new ApplyUserData(applyUserJson[i]);
                    resultList.Add(applyuserData);
                }
            }
            _action?.Invoke(resultList);
        });
    }

    /// <summary>
    /// 길드 가입 승인 
    /// </summary>
    /// <param name="_userInDate"></param>
    /// <param name="_action"></param>
    public void ApprovalGuildMember(string _userInDate, Action<bool> _action = null)
    {
        Backend.Guild.ApproveApplicantV3(_userInDate, (callback) =>
        {
            Debug.Log(ShowDebugLog($"ApprovalGuildMember ({_userInDate})", callback));
            _action?.Invoke(callback.IsSuccess());
        });
    }

    /// <summary>
    /// 길드 가입 거부
    /// </summary>
    /// <param name="_userInDate"></param>
    /// <param name="_action"></param>
    public void RejectGuildMember(string _userInDate, Action<bool> _action = null)
    {
        Backend.Guild.RejectApplicantV3(_userInDate, (callback) =>
        {
            Debug.Log(ShowDebugLog($"RejectGuildMember ({_userInDate})", callback));
            _action?.Invoke(callback.IsSuccess());
        });
    }

    /// <summary>
    /// 길드 회원 추방.
    /// </summary>
    /// <param name="_userInDate"></param>
    /// <param name="_action"></param>
    public void ExpelGuildMember(string _userInDate, Action<bool> _action = null)
    {
        Backend.Guild.ExpelMemberV3("gamerIndate", (callback) =>
        {
            Debug.Log(ShowDebugLog($"ExpelGuildMember ({_userInDate})", callback));
            _action?.Invoke(callback.IsSuccess());
        });
    }

    #endregion MASTER & VICE MASTER

    /// <summary>
    /// 길드 메타데이터 정보 업데이트 (길드 레벨 등 사용가능할것으로 예상)
    /// </summary>
    /// <param name="_metaDataKey"></param>
    /// <param name="addValue"></param>
    /// <param name="_action"></param>
    public void UpdateGuildMetaData(string _metaDataKey,int addValue,Action<bool> _action = null)
    {
        if (myGuildInfoData == null || myGuildInfoData.metaDataDic.ContainsKey(_metaDataKey) == false)
        {
            Debug.Log(ShowDebugLog("GetRandGuildListByMetaData"));
            _action?.Invoke(false);
            return;
        }

        Param updateParam = new Param();
        int updateValue = myGuildInfoData.metaDataDic[_metaDataKey]+ addValue;
        updateParam.Add($"metadata_{_metaDataKey}", updateValue);

        Backend.Guild.ModifyGuildV3(updateParam, (callback) =>
        {
            Debug.Log(ShowDebugLog("UpdateGuildMetaData", callback));
            _action?.Invoke(callback.IsSuccess());
        });
    }
}

public class GuildInfoData
{
    public int memberCount;
    public Dictionary<string, string> viceMasterList = new Dictionary<string, string>();
    public string masterNickname;
    public string masterInDate;
    public string guildName;
    public string guildinDate;
    public int goodsCount;
    public bool immediateRegistration;
    public string countryCode;
    public Dictionary<string, int> metaDataDic = new Dictionary<string, int>();

    public GuildInfoData()
    {
    }

    public GuildInfoData(JsonData _Data)
    {
        memberCount = Int32.Parse(_Data["memberCount"].ToString());
        masterNickname = _Data["masterNickname"].ToString();
        masterInDate = _Data["masterInDate"].ToString();
        guildName = _Data["guildName"].ToString();
        guildinDate = _Data["inDate"].ToString();
        goodsCount = Int32.Parse(_Data["goodsCount"].ToString());

        if (_Data.ContainsKey("_immediateRegistration"))
            immediateRegistration = _Data["_immediateRegistration"].ToString() == "true" ? true : false;

        if (_Data.ContainsKey("_countryCode"))
            countryCode = _Data["_countryCode"].ToString();


        JsonData viceListJson = _Data["viceMasterList"];
        for (int j = 0; j < viceListJson.Count; j++)
            viceMasterList.Add(viceListJson[j]["inDate"].ToString(), viceListJson[j]["nickname"].ToString());

        foreach (var metaData in _Data.Keys)
        {
            if (metaData.Contains("metadata_"))
            {
                string[] key = metaData.Split('_');
                if (key.Length > 1)
                    metaDataDic.Add(key[1], int.Parse(_Data[metaData].ToString()));
            }
        }
    }

    public override string ToString()
    {
        string viceMasterString = string.Empty;
        foreach (var li in viceMasterList)
        {
            viceMasterString += $"부길드마스터 : {li.Value}({li.Key})\n";
        }

        return $"memberCount : {memberCount}\n" +
        $"guildName : {guildName}\n" +
        $"guildinDate : {guildinDate}\n" +
        $"masterNickname : {masterNickname}\n" +
        $"masterInDate : {masterInDate}\n" +
        $"goodsCount : {goodsCount}\n" +
        $"immediateRegistration : {immediateRegistration}\n" +
        $"countryCode : {countryCode}\n" +
        viceMasterString;
    }
};

public class GuildMemberData
{
    public string nickname;
    public string inDate;
    public string gamerInDate;
    public string position;
    public string lastLogin;
    public Dictionary<string, int> totalGoodsAmount = new Dictionary<string, int>();

    public GuildMemberData(JsonData _Data)
    {
        nickname = _Data["nickname"].ToString();
        inDate = _Data["inDate"].ToString();
        gamerInDate = _Data["gamerInDate"].ToString();
        lastLogin = _Data["lastLogin"].ToString();
        position = _Data["position"].ToString();

        foreach (var goods in _Data.Keys)
        {
            if (goods.Contains("totalGoods"))
            {
                totalGoodsAmount.Add(goods, Int32.Parse(_Data[goods].ToString()));
            }
        }

    }

    public override string ToString()
    {
        string goodsString = string.Empty;
        foreach (var dic in totalGoodsAmount)
        {
            goodsString += $"{dic.Key} : {dic.Value}\n";
        }
        return $"nickname : {nickname}\n" +
        $"inDate : {inDate}\n" +
        $"gamerInDate : {gamerInDate}\n" +
        $"position : {position}\n" +
        $"lastLogin : {lastLogin}\n" +
        goodsString;
    }
};

public class GuildGoodsInfo
{
    public int totalGoodsAmount;
    public List<UserGoodsData> userList;

    public GuildGoodsInfo(JsonData _data,string _column)
    {
        userList = new List<UserGoodsData>();
        totalGoodsAmount = Int32.Parse(_data[_column].ToString());

        string goodsNum = _column.Replace("totalGoods", "");
        goodsNum = goodsNum.Replace("Amount", "");

        string goodsName = "goods" + goodsNum + "UserList";
        JsonData userListJson = _data[goodsName];
		for (int i = 0; i < userListJson.Count; i++)
		{
            UserGoodsData userListData = new UserGoodsData(userListJson[i]);
            userList.Add(userListData);
        }
    }

    public override string ToString()
    {
        string userString = string.Empty;
        for (int i = 0; i < userList.Count; i++)
        {
            userString += userList[i].ToString() + "\n";
        }
        return $"[totalGoodsAmount : {totalGoodsAmount}]\n" +
        $"{userString}\n";
    }
}

public class UserGoodsData
{
    public int usingTotalAmount;
    public int totalAmount;
    public string inDate;
    public string nickname;
    public string updatedAt;

    public UserGoodsData(JsonData _data)
    {
        inDate = _data["inDate"].ToString();
        nickname = _data["nickname"].ToString();
        if (_data.ContainsKey("usingTotalAmount"))
            usingTotalAmount = Int32.Parse(_data["usingTotalAmount"].ToString());
        totalAmount = Int32.Parse(_data["totalAmount"].ToString());
        updatedAt = _data["updatedAt"].ToString();
    }

    public override string ToString()
    {
        return $"\tnickname : {nickname}\n" +
        $"\tinDate : {inDate}\n" +
        $"\ttotalAmount : {totalAmount}\n" +
        $"\tusingTotalAmount : {usingTotalAmount}\n" +
        $"\tupdatedAt : {updatedAt}\n";
    }
}

public class ApplyUserData
{
    public string inDate;
    public string nickname;

    public ApplyUserData(JsonData _data)
    {
        if (_data.ContainsKey("nickname"))
            nickname = _data["nickname"].ToString();
        inDate = _data["inDate"].ToString();
    }

    public override string ToString()
    {
        return $"nickname : {nickname}\ninDate : {inDate}\n";
    }
}