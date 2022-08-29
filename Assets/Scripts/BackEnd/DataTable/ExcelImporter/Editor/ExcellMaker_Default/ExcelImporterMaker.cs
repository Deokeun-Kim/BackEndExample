using UnityEngine;
using UnityEditor;
using System.IO;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using System.Collections.Generic;
using System.Text;

public class ExcelImporterMaker : EditorWindow
{
	#region GUI   

	Vector2 curretScroll = Vector2.zero;

    void OnGUI()
    {
        GUILayout.Label("makeing importer", EditorStyles.boldLabel);
        className = EditorGUILayout.TextField("class name", className);
        sepalateSheet = EditorGUILayout.Toggle("sepalate sheet", sepalateSheet);

        EditorPrefs.SetBool(s_key_prefix + fileName + ".separateSheet", sepalateSheet);

        if (GUILayout.Button("create"))
        {
            EditorPrefs.SetString(s_key_prefix + fileName + ".className", className);

            /////////////////////////////
            //main func
            /////////////////////////////
            ExportCustomEntity();
            //EnumTable();
            ExportJsonParser();
            ExportTypeScript();
            ExcelToJson();
            /////////////////////////////
            /////////////////////////////

            AssetDatabase.ImportAsset(filePath);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            Close();
        }

        // selecting sheets
        EditorGUILayout.LabelField("sheet settings");
        EditorGUILayout.BeginVertical("box");
        foreach (ExcelSheetParameter sheet in sheetList)
        {
            GUILayout.BeginHorizontal();
            sheet.isEnable = EditorGUILayout.BeginToggleGroup("enable", sheet.isEnable);
            EditorGUILayout.LabelField(sheet.sheetName);
            EditorGUILayout.EndToggleGroup();
            EditorPrefs.SetBool(s_key_prefix + fileName + ".sheet." + sheet.sheetName, sheet.isEnable);
            GUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();

        // selecting parameters
        EditorGUILayout.LabelField("parameter settings");
        curretScroll = EditorGUILayout.BeginScrollView(curretScroll);
        EditorGUILayout.BeginVertical("box");
        string lastCellName = string.Empty;
        foreach (ExcelRowParameter cell in typeList)
        {
            if (cell.isArray && lastCellName != null && cell.name.Equals(lastCellName))
            {
                continue;
            }

            cell.isEnable = EditorGUILayout.BeginToggleGroup("enable", cell.isEnable);
            if (cell.isArray)
            {
                EditorGUILayout.LabelField("---[array]---");
            }
            GUILayout.BeginHorizontal();
            cell.name = EditorGUILayout.TextField(cell.name);
            GUILayout.EndHorizontal();

            EditorGUILayout.EndToggleGroup();
            lastCellName = cell.name;
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();

    }   
    #endregion GUI
  
    private const string libPath = "Assets/Adalib/ExcelImporter/";

    private string filePath = string.Empty;
    private bool sepalateSheet = false;
    private List<ExcelRowParameter> typeList = new List<ExcelRowParameter>();
    private List<ExcelSheetParameter> sheetList = new List<ExcelSheetParameter>();
    private string className = string.Empty;
    private string fileName = string.Empty;
    private static string s_key_prefix = "terasurware.exel-importer-maker.";

	private static ISheet nomalSheet;
    private static ISheet enumSheet;

	private class ExcelSheetParameter
    {
        public string sheetName;
        public bool isEnable;
    }

    private class ExcelRowParameter
    {
        public string name;
        public string valueType;
        public bool isEnable;
        public bool isArray;
        public ExcelRowParameter nextArrayItem;
    }
   
	/// <summary>
    ///  메인 함수
    /// </summary>
    //[MenuItem("Assets/XLS Import Settings...")]
    static void ExportExcelToAssetbundle()
    {
        foreach (Object obj in Selection.objects)
        {
			ExcelImporterMaker window = ScriptableObject.CreateInstance<ExcelImporterMaker>();
            window.filePath = AssetDatabase.GetAssetPath(obj);
            window.fileName = Path.GetFileNameWithoutExtension(window.filePath);
		
		
			using (FileStream stream = File.Open (window.filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
				IWorkbook book = null;
				if (Path.GetExtension (window.filePath) == ".xls") {
					book = new HSSFWorkbook(stream);
				} else {
					book = new XSSFWorkbook(stream);
				}

                book.MissingCellPolicy = MissingCellPolicy.CREATE_NULL_AS_BLANK;

                for (int i = 0; i < book.NumberOfSheets; ++i)
                {
                    ISheet s = book.GetSheetAt(i);
                    ExcelSheetParameter sht = new ExcelSheetParameter();
                    sht.sheetName = s.SheetName;
                    sht.isEnable = EditorPrefs.GetBool(s_key_prefix + window.fileName + ".sheet." + sht.sheetName, true);
                    window.sheetList.Add(sht);

					int num = s.SheetName.IndexOf ('_');

					if (num != -1) {
						//enum sheet .. enumsheet only one.
						enumSheet = s;
					}
					else {
						nomalSheet = s;
					}
                }

             	ISheet sheet = book.GetSheetAt(0);

				EditorPrefs.SetString (jsonParser_s_key_prefix, sheet.SheetName);

                window.className = EditorPrefs.GetString(s_key_prefix + window.fileName + ".className", "Entity_" + sheet.SheetName);

                window.sepalateSheet = EditorPrefs.GetBool(s_key_prefix + window.fileName + ".separateSheet");

                IRow titleRow = sheet.GetRow(0);
                IRow dataRow = sheet.GetRow(1);

                for (int i=0; i < titleRow.LastCellNum; i++)
                {
                    ExcelRowParameter parser = new ExcelRowParameter();
                    parser.name = titleRow.GetCell(i).StringCellValue;
                    parser.isArray = parser.name.Contains("[]");
                    if (parser.isArray)
                    {
                        parser.name = parser.name.Remove(parser.name.LastIndexOf("[]"));
                    }

                    ICell cell = dataRow.GetCell(i);

                    if (cell.CellType != CellType.Unknown && cell.CellType != CellType.Blank)
                    {
                        parser.isEnable = true;

						switch (cell.StringCellValue) {

						case "int":
							parser.valueType = cell.StringCellValue;
							break;

						case "double":
							parser.valueType = cell.StringCellValue;
							break;
						
						case "float":
							parser.valueType = cell.StringCellValue;
							break;

						case "string":
							parser.valueType = cell.StringCellValue;
							break;

						case "bool":
							parser.valueType = cell.StringCellValue;
							break;

                        case "Object":
                            parser.valueType = cell.StringCellValue;
                            break;

						case "enum":
							parser.valueType = cell.StringCellValue;
							break;

						}
                    }
				
                    window.typeList.Add(parser);
                }
			
                window.Show();
            }
        }
    }

	/// <summary>
	/// ExportJsonParser, 해당 엑셀을 읽고, ExportJsonParser.txt 탬플릿을 기준으로 자동으로 jsonParser를 생성
	/// </summary>   
	#region Create JsonParser
    private static string jsonParser_s_key_prefix = "ExcelJsonKey";

    void ExportJsonParser( string _outputPath = null )
	{
		if (sepalateSheet == false) {

			Debug.LogError ("JsonParser, Require sepalateSheet == true");
			return;
		}

        string templateFilePath = libPath + "Template/ExportJsonParser.txt";

		string importerTemplate = File.ReadAllText(templateFilePath);

		StringBuilder builder = new StringBuilder();

		int rowCount = 0;
		string tab = "            ";						

		//string steetName = EditorPrefs.GetString (jsonParser_s_key_prefix);

		foreach (ExcelRowParameter row in typeList)
		{
			if (row.isEnable)
			{
				if (!row.isArray)
				{
					builder.AppendLine();

					if (row.valueType == "enum") {

						string enumTypeName = row.name;
						enumTypeName = enumTypeName [0].ToString ().ToUpper() + enumTypeName.Substring (1);

						builder.AppendFormat (tab + "p.{0} = ({2}) (System.Enum.Parse (typeof({1}), node [i] [\"{0}\"].Value));", (row.name).ToLower(), enumTypeName);

					}
					else {

						switch (row.valueType) {

						case "bool":
							builder.AppendFormat (tab + "p.{0} = bool.Parse ( node [i] [\"{0}\"].Value );", (row.name).ToLower());
							break;
						case "double":
							builder.AppendFormat (tab + "p.{0} = double.Parse ( node [i] [\"{0}\"].Value, System.Globalization.CultureInfo.GetCultureInfo(\"en-US\") );", (row.name).ToLower());
							break;
						case "int":
							builder.AppendFormat (tab + "p.{0} = int.Parse ( node [i] [\"{0}\"].Value );", (row.name).ToLower());
							break;
						case "float":
							builder.AppendFormat (tab + "p.{0} = float.Parse ( node [i] [\"{0}\"].Value, System.Globalization.CultureInfo.GetCultureInfo(\"en-US\") );", (row.name).ToLower());
							break;
						case "string":
							builder.AppendFormat (tab + "p.{0} = node [i] [\"{0}\"].Value;", (row.name).ToLower());
							break;
						}
					}
				}
			}
			rowCount += 1;
		}

		//importerTemplate = importerTemplate.Replace("$SteetClassName$", steetName);
		importerTemplate = importerTemplate.Replace("$ExcelData$", className);
		importerTemplate = importerTemplate.Replace("$EXPORT_DATA$", builder.ToString());
        importerTemplate = importerTemplate.Replace("$ExportTemplate$", className + "_Parser");
      
        if (string.IsNullOrEmpty(_outputPath)){
            Directory.CreateDirectory(libPath + "Classes");
            File.WriteAllText(libPath + "Classes/" + className + "_Parser.cs", importerTemplate);
        }
        else{
            int index = _outputPath.LastIndexOf('/');
            string temp_outputPath = _outputPath.Remove(index);
            Directory.CreateDirectory(temp_outputPath);
            File.WriteAllText(_outputPath + className + "_Parser.cs", importerTemplate);
        }
	}
    #endregion

    /// <summary>
    /// ExportCustomEntity, 해당 엑셀을 읽고, 템플릿을 기준으로 ( EntityTemplate.txt ), 데이터 클래스 자동 생성
    /// </summary>   
    #region Custom Entity Data
    void ExportCustomEntity( string _outputPath = null )
	{
		if (sepalateSheet == false) {

			Debug.LogError ("CustomEntity, Require sepalateSheet == true");
			return;
		}

        string templateFilePath = libPath + "Template/EntityTemplate.txt";
		string entittyTemplate = File.ReadAllText(templateFilePath);
		entittyTemplate = entittyTemplate.Replace("\r\n", "\n").Replace("\n", System.Environment.NewLine);
		StringBuilder builder = new StringBuilder();

		foreach (ExcelRowParameter row in typeList)
		{
			if (row.isEnable)
			{
				if (!row.isArray)
				{
					builder.AppendLine();

					if (row.valueType == "enum") {

						string enumTypeName = row.name;
						enumTypeName = enumTypeName [0].ToString ().ToUpper() + enumTypeName.Substring (1);

						builder.AppendFormat("		public {0} {1};", enumTypeName, (row.name).ToLower());
					}
					else {
						builder.AppendFormat("		public {0} {1};", row.valueType, row.name);
					}
				}
			}
		}

		entittyTemplate = entittyTemplate.Replace("$Types$", builder.ToString());
		entittyTemplate = entittyTemplate.Replace("$ExcelData$", className);

        if (string.IsNullOrEmpty(_outputPath)){
            Directory.CreateDirectory(libPath + "Classes");
            File.WriteAllText(libPath + "Classes/" + className + ".cs", entittyTemplate);
        }
        else{
            int index = _outputPath.LastIndexOf('/');

            string temp_outputPath = _outputPath.Remove(index);

            Directory.CreateDirectory(temp_outputPath);
            Debug.Log(_outputPath);
            File.WriteAllText(_outputPath + className + ".cs", entittyTemplate);
        }
	}
    #endregion Custom Entity Data

    #region ExportTypeScript
    //서버용 TypeSecript 생성
    void ExportTypeScript(string _outputPath = null)
    {
        if (sepalateSheet == false)
        {

            Debug.LogError("CustomEntity, Require sepalateSheet == true");
            return;
        }

        string templateFilePath = libPath + "Template/TypeScriptTemplate.txt";
        string entittyTemplate = File.ReadAllText(templateFilePath);
        entittyTemplate = entittyTemplate.Replace("\r\n", "\n").Replace("\n", System.Environment.NewLine);
        StringBuilder builder = new StringBuilder();

        foreach (ExcelRowParameter row in typeList)
        {
            if (row.isEnable)
            {
                if (!row.isArray)
                {
                    builder.AppendLine();

                    if (row.valueType == "enum")
                    {
                        string enumTypeName = row.name;
                        enumTypeName = enumTypeName[0].ToString().ToUpper() + enumTypeName.Substring(1);

                        builder.AppendFormat("		public {0} : {1};", (row.name).ToLower(), enumTypeName);
                    }
                    else
                    {
                        string typescriptType = "";
                        string initvalue = "";
                        switch (row.valueType)
                        {
                            case "bool":
                                {
                                    typescriptType = "boolean";
                                    initvalue = "false";
                                }
                                break;
                            case "double":
                            case "float":
                                {
                                    typescriptType = "number";
                                    initvalue = "0.0";
                                }
                                break;
                            case "int":
                                {
                                    typescriptType = "number";
                                    initvalue = "0";
                                }
                                break;
                            case "string":
                                {
                                    typescriptType = "string";
                                    initvalue = "\"\"";
                                }
                                break;
                        }
                        builder.AppendFormat("	public {0} : {1} = {2};", row.name, typescriptType, initvalue);
                    }
                }
            }
        }

        entittyTemplate = entittyTemplate.Replace("$Types$", builder.ToString());
        entittyTemplate = entittyTemplate.Replace("$ExcelTable$", className);

        builder.Remove(0, builder.Length);

        string tab = "            ";
        foreach (ExcelRowParameter row in typeList)
        {
            if (row.isEnable)
            {
                if (!row.isArray)
                {
                    builder.AppendLine();

                    if (row.valueType == "enum")
                    {
                        //string enumTypeName = row.name;
                        //enumTypeName = enumTypeName[0].ToString().ToUpper() + enumTypeName.Substring(1);

                        //builder.AppendFormat(tab + "p.{0} = ({2}) (System.Enum.Parse (typeof({2}), node [\"{1}\"] [i] [\"{0}\"].Value));", (row.name).ToLower(), steetName, enumTypeName);
                    }
                    else
                    {
                        switch (row.valueType)
                        {

                            case "bool":
                                builder.AppendFormat(tab + "p.{0} = Boolean(data[i][\"{0}\"]);", (row.name).ToLower());
                                break;
                            case "double":
                            case "int":
                            case "float":
                                builder.AppendFormat(tab + "p.{0} = Number(data[i][\"{0}\"]);", (row.name).ToLower());
                                break;
                            case "string":
                                builder.AppendFormat(tab + "p.{0} = String(data[i][\"{0}\"]);", (row.name).ToLower());
                                break;
                        }
                    }
                }
            }
        }

        entittyTemplate = entittyTemplate.Replace("$EXPORT_DATA$", builder.ToString());
        entittyTemplate = entittyTemplate.Replace("$ExcelDataParser$", className + "_Parser");

        if (string.IsNullOrEmpty(_outputPath))
        {
            Directory.CreateDirectory(libPath + "Classes");
            File.WriteAllText(libPath + "Classes/" + className + ".ts", entittyTemplate);
        }
        else
        {
            int index = _outputPath.LastIndexOf('/');

            string temp_outputPath = _outputPath.Remove(index);

            Directory.CreateDirectory(temp_outputPath);
            Debug.Log(_outputPath);
            File.WriteAllText(_outputPath + className + ".ts", entittyTemplate);
        }
    }
    #endregion ExportTypeScript

    #region EditEnumTable
    StringBuilder combineEnumFieldBuilder = new StringBuilder();

    void EnumTable( string _outputPath = null )
	{
		if (sepalateSheet == false) {

			Debug.LogError ("CustomEntity, Require sepalateSheet == true");
			return;
		}
			
		bool isEnum = false;

		List<StringBuilder> enumBuilderList = new List<StringBuilder> ();

		foreach (ExcelSheetParameter sheet in sheetList) {

			bool isEnumSheet = sheet.sheetName.Contains ("Enum_");

			if ( isEnumSheet ) {

				//create enum
				isEnum = true;

				IRow enumTypeNameRow = enumSheet.GetRow(0);

				for ( int i = 0; i < enumTypeNameRow.LastCellNum; i ++ )
				{
					StringBuilder enumBuilder = new StringBuilder();

					string enumTypeName = enumTypeNameRow.GetCell(i).StringCellValue;
					enumTypeName = enumTypeName [0].ToString ().ToUpper() + enumTypeName.Substring (1);

					enumBuilder.Append("public enum " + enumTypeName); 
					enumBuilder.AppendLine();
					enumBuilder.Append("{");
					enumBuilder.AppendLine();

					enumBuilderList.Add( enumBuilder );
				}

				for ( int i = 0; i < enumSheet.LastRowNum; i ++ )
				{
					IRow enumCellRow = enumSheet.GetRow(i+1);

					Debug.Log( enumTypeNameRow.LastCellNum );
					Debug.Log( enumCellRow.RowNum );

					if( enumCellRow != null )
					{
						int macRowCount = enumSheet.LastRowNum;

						for (int j = 0; j < macRowCount+1; j ++)
						{
                            if (enumCellRow.GetCell(j) != null)
                            {

                                enumBuilderList[j].AppendLine();

                                if (i == 0)
                                {

                                    enumBuilderList[j].Append
                                    ("  " + enumCellRow.GetCell(j).StringCellValue + "= 1,");
                                }
                                else
                                {

                                    enumBuilderList[j].Append
                                    ("  " + enumCellRow.GetCell(j).StringCellValue + ",");
                                }
                            }
                            else
                            {
                                //null field
                                continue;
                            }
						}
					}
				}
			}
			else {
				//not enum_field
			}

			if( isEnum)
			{
				for (int i = 0; i < enumBuilderList.Count; i ++)
				{
					enumBuilderList[i].AppendLine();
					enumBuilderList[i].Append("}");
					enumBuilderList[i].AppendLine();
				}

				for (int i = 0; i < enumBuilderList.Count; i ++)
				{
					combineEnumFieldBuilder.Append( enumBuilderList[i].ToString());
					combineEnumFieldBuilder.AppendLine();
				}

                string templateFilePath = libPath + "Template/EnumTemplate.txt";
				string enumTemplate = File.ReadAllText(templateFilePath);
				enumTemplate = enumTemplate.Replace("\r\n", "\n").Replace("\n", System.Environment.NewLine);
				//StringBuilder builder = new StringBuilder();

				enumTemplate = enumTemplate.Replace ("$EnumTypes$", combineEnumFieldBuilder.ToString ());

                if (string.IsNullOrEmpty(_outputPath)){
                    Directory.CreateDirectory(libPath + "Classes");
                    File.WriteAllText(libPath + "Classes/" + "Define" + ".cs", enumTemplate);
                }
                else{
                    int index = _outputPath.LastIndexOf('/');
                    string temp_outputPath = _outputPath.Remove(index);
                    Directory.CreateDirectory(temp_outputPath);
                    File.WriteAllText(_outputPath + "Define" + ".cs", enumTemplate);
                }
			}
		}
	}
    #endregion

    /// <summary>
    /// ToJson/ client Test, Only
    /// </summary>
    #region ExcelToJson
    void ExcelToJson()
    {
        string outputJsonValue = string.Empty;

        if (sepalateSheet == false)
        {
            return;
        }

        foreach (ExcelSheetParameter sheet in sheetList)
        {
            int num = sheet.sheetName.IndexOf('_');

            if (num == -1)
            {
                IRow jsonTitleRow = nomalSheet.GetRow(0);

                List<string> typeNameList = new List<string>();
                StringBuilder jsonTextBuilder = new StringBuilder();

                for (int i = 0; i < jsonTitleRow.Cells.Count; i++)
                {
                    string type = jsonTitleRow.Cells[i].StringCellValue;
                    typeNameList.Add(type);
                }

                //start str write
                //jsonTextBuilder.Append("{");
                //jsonTextBuilder.AppendLine();
                //jsonTextBuilder.Append("\"" + nomalSheet.SheetName + "\":");
                jsonTextBuilder.Append("[");

                //Debug.Log(nomalSheet.LastRowNum);
                //Debug.Log(typeNameList.Count);

                List<StringBuilder> strBuilderList = new List<StringBuilder>();

                int listIndex = 0;

                for (int i = 0; i < nomalSheet.LastRowNum + 1 - 2; i++)
                {
                    StringBuilder strBuilder = new StringBuilder();
                    strBuilderList.Add(strBuilder);
                    strBuilderList[listIndex].Append("{");
                    strBuilderList[listIndex].AppendLine();

                    listIndex++;
                }

                List<IRow> jsonRowList = new List<IRow>();

                for (int i = 0; i < nomalSheet.LastRowNum + 1 - 2; i++)
                {
                    //Debug.Log(i);
                    IRow jsonRow = nomalSheet.GetRow(i + 2);
                    jsonRowList.Add(jsonRow);
                }

                ////
                if (strBuilderList.Count > 0)
                {
                    for (int i = 0; i < strBuilderList.Count; i++)
                    {
                        //Debug.Log(i);
                        for (int j = 0; j < typeNameList.Count; j++)
                        {
                            if (jsonRowList[i].GetCell(j) != null)
                            {
                                if (jsonRowList[i].GetCell(j).CellType == CellType.Formula)
                                {
                                    jsonRowList[i].GetCell(j).SetCellType(CellType.String);
                                }

                                if (j == typeNameList.Count - 1)
                                {
                                    strBuilderList[i].Append(
                                   "\"" + typeNameList[j].ToLower() + "\":" +
                                   "\"" + jsonRowList[i].GetCell(j).ToString() + "\""
                                   );
                                }
                                else
                                {
                                    strBuilderList[i].Append(
                                    "\"" + typeNameList[j].ToLower() + "\":" +
                                    "\"" + jsonRowList[i].GetCell(j).ToString() + "\","
                                    );
                                }
                            }
                            else
                            {
                                strBuilderList[i].Append(
                                "\"" + typeNameList[j].ToLower() + "\":" +
                                "\"" + "" + "\","
                                );
                            }
                        }
                    }

                    for (int i = 0; i < strBuilderList.Count; i++)
                    {

                        strBuilderList[i].AppendLine();
                        strBuilderList[i].Append("}");
                        strBuilderList[i].AppendLine();
                        if( i < strBuilderList.Count -1 )
                            strBuilderList[i].Append(",");

                        jsonTextBuilder.Append(strBuilderList[i].ToString());
                    }

                    //jsonTextBuilder.AppendLine();
                    jsonTextBuilder.Append("]");
                    //jsonTextBuilder.AppendLine();
                    //jsonTextBuilder.Append("}");

                    outputJsonValue = jsonTextBuilder.ToString();
                    Debug.Log("output_Json : " + "\n" + outputJsonValue);
                }
            }
        }

        Directory.CreateDirectory("Assets/Resources/Json/");
        File.WriteAllText("Assets/Resources/Json/" + className + ".json", outputJsonValue);
    }
    #endregion ExcelToJson
}