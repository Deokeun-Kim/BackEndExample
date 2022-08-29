/* * * * *
 *
 * ExportTool, ScriptExportTool 에서 참조로 사용되는 파일.
 * 수정시 위에 언급된 프로젝트들의 빌드가 필요할 수 있음.
 *
 * * * * */

using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System;
using System.Xml;

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
#endif

public class ExcelImporterAuto
{
	private static string libPath = "Assets/Scripts/BackEnd/DataTable/ExcelImporter/";

	public string filePath = string.Empty;
	private List<ExcelRowParameter> typeList = new List<ExcelRowParameter>();
	private List<ExcelSheetParameter> sheetList = new List<ExcelSheetParameter>();
	private string className = string.Empty;
	private string fileName = string.Empty;
	private static string s_key_prefix = "terasurware.exel-importer-maker.";

	private static ISheet nomalSheet;
	private static ISheet enumSheet;

	private static StringBuilder checkerDataBuilder = new StringBuilder();
	private static StringBuilder checkerParsingBuilder = new StringBuilder();

	private static string _workingDirectory;

	private class ExcelSheetParameter
	{
		public string sheetName;
		public bool isEnable;
	}

	private class ExcelRowParameter
	{
		public string name;
		public string valueType1;
		public string valueType2;
		public string valueType3;
		public bool isEnable;
		public bool isArray;
		public ExcelRowParameter nextArrayItem;
	}

	public static void SetApplicationDataPath(string _path)
	{
		_workingDirectory = _path;
	}

	public static string GetApplicationDataPath()
	{
#if UNITY_EDITOR
		return Application.dataPath;
#else
		return _workingDirectory;
#endif
	}

	public static List<string> ExcelEnumListTable(string _excelFilePath)
	{
		List<string> enumList = new List<string>();
		string excelFilePath = Directory.GetFiles(_excelFilePath, "EnumList.xlsx")[0];
		ExcelImporterAuto window = new ExcelImporterAuto();

		window.filePath = excelFilePath;
		window.fileName = Path.GetFileNameWithoutExtension(window.filePath);

		using (FileStream stream = File.Open(window.filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
		{
			IWorkbook book = null;
			if (Path.GetExtension(window.filePath) == ".xls")
			{
				book = new HSSFWorkbook(stream);
			}
			else
			{
				book = new XSSFWorkbook(stream);
			}

			ISheet sheet = book.GetSheetAt(0);
			IRow titleRow = sheet.GetRow(0);

			for (int i = 0; i < titleRow.LastCellNum; i++)
			{
				ExcelRowParameter parser = new ExcelRowParameter();
				string enumName = titleRow.GetCell(i).StringCellValue.ToLower();
				if (enumList.Contains(enumName) == false)
					enumList.Add(enumName);
				if (enumList.Contains(enumName.Replace("_", "")) == false)
					enumList.Add(enumName.Replace("_", ""));
			}
		}
		return enumList;
	}
	static Dictionary<string, List<string>> ExcelEnumListTable(List<ExcelSheetParameter> _sheetList, ref string _strError)
	{
		_strError = "";
		Dictionary<string, List<string>> dicList = new Dictionary<string, List<string>>();

		foreach (ExcelSheetParameter sheet in _sheetList)
		{
			IRow enumTypeNameRow = enumSheet.GetRow(0);

			for (int i = 0; i < enumTypeNameRow.LastCellNum; i++)
			{
				string enumTypeName = enumTypeNameRow.GetCell(i).StringCellValue;
				//enumTypeName = enumTypeName[0].ToString().ToUpper() + enumTypeName.Substring(1);

				List<string> enumList = null;
				dicList.TryGetValue(enumTypeName, out enumList);
				if (enumList == null)
				{
					enumList = new List<string>();
					dicList.Add(enumTypeName, enumList);
				}
				else
				{
					_strError = $"{_strError}\nDuplicate enum name. [ {enumTypeName} ]";
				}

				int macRowCount = enumSheet.LastRowNum;
				for (int j = 0; j < macRowCount + 1; j++)
				{
					IRow enumCellRow = enumSheet.GetRow(j + 1);
					if (enumCellRow != null)
					{
						if (enumCellRow.GetCell(i) == null)
							continue;

						string cellValue;

						if (enumCellRow.GetCell(i).CellType == CellType.Numeric)
							cellValue = enumCellRow.GetCell(i).NumericCellValue.ToString();
						else
							cellValue = enumCellRow.GetCell(i).StringCellValue;

						if (string.IsNullOrEmpty(cellValue) == false)
						{
							if (enumList.Contains(cellValue) == false)
								enumList.Add(cellValue);
							else
								_strError = $"{_strError}\nDuplicate enum type. [ {enumTypeName} ] - [ {cellValue} ]";
						}
						else
						{
							continue;
						}
					}
				}
			}
		}
		return dicList;
	}


	public static ExcelImporterAuto GetExcelImporterAuto(string _excelFilePath, string _className = null, bool _prefix = false)
	{
		ExcelImporterAuto window = new ExcelImporterAuto();

		window.filePath = _excelFilePath;
		window.fileName = Path.GetFileNameWithoutExtension(window.filePath);

		using (FileStream stream = File.Open(window.filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
		{
			IWorkbook book = null;
			if (Path.GetExtension(window.filePath) == ".xls")
			{
				book = new HSSFWorkbook(stream);
			}
			else
			{
				book = new XSSFWorkbook(stream);
			}

			ISheet s = book.GetSheetAt(0);
			ExcelSheetParameter sht = new ExcelSheetParameter();
			sht.sheetName = s.SheetName;
			sht.isEnable
#if UNITY_EDITOR
				= EditorPrefs.GetBool(s_key_prefix + window.fileName + ".sheet." + sht.sheetName, true);
#else
				= true;
#endif
			window.sheetList.Add(sht);

			if (Path.GetFileNameWithoutExtension(_excelFilePath) == "EnumList")
			{
				//enum sheet .. enumsheet only one.
				enumSheet = s;
			}
			else
			{
				nomalSheet = s;
			}

			ISheet sheet = book.GetSheetAt(0);
#if UNITY_EDITOR
			EditorPrefs.SetString(jsonParser_s_key_prefix, sheet.SheetName);
#endif

			if (string.IsNullOrEmpty(_className))
			{
#if UNITY_EDITOR
				window.className = EditorPrefs.GetString(s_key_prefix + window.fileName + ".className", "Entity_" + sheet.SheetName);
#endif
			}
			else
			{
				string tempClassName = _className;

				if (_prefix)
				{
					tempClassName = "Entity_" + tempClassName;
				}

				window.className = tempClassName;
			}
			//////////
			//////////

			IRow titleRow = sheet.GetRow(0);
			IRow dataRow = sheet.GetRow(1);

			for (int i = 0; i < titleRow.LastCellNum; i++)
			{
				ExcelRowParameter parser = new ExcelRowParameter();
				parser.name = titleRow.GetCell(i).StringCellValue;
				parser.isArray = parser.name.Contains("[]");
				if (parser.isArray)
				{
					parser.name = parser.name.Remove(parser.name.LastIndexOf("[]"));
				}

				ICell cell = dataRow.GetCell(i);

				if (cell.StringCellValue.Equals("memo") == true)
					continue;

				if (string.IsNullOrEmpty(parser.name))
					continue;

				if (cell.CellType != CellType.Unknown && cell.CellType != CellType.Blank && parser.name[0] != '$')
				{
					parser.isEnable = true;

					parser.valueType1 = cell.StringCellValue.Trim();
					int findStartOffset = cell.StringCellValue.IndexOf('(');
					int findEndOffset = cell.StringCellValue.IndexOf(')');
					if (findStartOffset != findEndOffset)
					{
						if (findStartOffset == -1 || findEndOffset == -1)
						{
#if UNITY_EDITOR
							Debug.LogError("Data Type () Error");
#endif
							return null;
						}
						parser.valueType1 = parser.valueType1.Remove(findStartOffset, findEndOffset - findStartOffset + 1);
						parser.valueType1 = parser.valueType1.Trim();
					}

					findStartOffset = cell.StringCellValue.IndexOf('[');
					findEndOffset = cell.StringCellValue.LastIndexOf(']');
					if (findStartOffset != findEndOffset)
					{
						if (findStartOffset == -1 || findEndOffset == -1)
						{
#if UNITY_EDITOR
							Debug.LogError("Data Type [] Error");
#endif
							return null;
						}

						parser.valueType2 = parser.valueType1.Substring(findStartOffset + 1, findEndOffset - findStartOffset - 1);
						parser.valueType2 = parser.valueType2.Trim();

						int findStartOffset2 = parser.valueType2.IndexOf('[');
						int findEndOffset2 = parser.valueType2.LastIndexOf(']');
						if (findStartOffset2 != findEndOffset2)
						{
							parser.valueType3 = parser.valueType2.Substring(findStartOffset2 + 1, findEndOffset2 - findStartOffset2 - 1);
							parser.valueType3 = parser.valueType3.Trim();

							parser.valueType2 = parser.valueType2.Remove(findStartOffset2, findEndOffset2 - findStartOffset2 + 1);
							parser.valueType2 = parser.valueType2.Trim();
						}

						parser.valueType1 = parser.valueType1.Remove(findStartOffset, findEndOffset - findStartOffset + 1);
						parser.valueType1 = parser.valueType1.Trim();
					}
				}

				window.typeList.Add(parser);
			}

#if UNITY_EDITOR
			EditorPrefs.SetString(s_key_prefix + window.fileName + ".className", window.className);
#endif
		}
		return window;
	}

	/// <summary>
	///  메인 함수
	/// </summary>
	public static void ExportExcelScript(string _excelFilePath, string _outputPath = null, string _className = null, bool _prefix = false)
	{
		string _libPath = "";
#if UNITY_EDITOR
		Debug.Log("_excelFilePath : " + _excelFilePath);
		Debug.Log("_outputPath : " + _outputPath);
		_libPath = libPath;
#endif
		ExcelImporterAuto window = GetExcelImporterAuto(_excelFilePath, _className, _prefix);


		/////////////////////////////
		//main func
		/////////////////////////////
		if (Path.GetFileNameWithoutExtension(_excelFilePath) == "EnumList")
		{
			ExcelEnumTable(_libPath, window.sheetList, _outputPath);
			ExcelEnumTableAsTypeScript(_libPath, window.sheetList, _outputPath);
		}
		else
		{
			ExportCustomEntity(_libPath, window.typeList, window.className, _outputPath);
			ExportJsonParser(_libPath, window.typeList, window.className, _outputPath);
			ExportCustomTableEntity(_libPath, window.typeList, window.className, _outputPath);
			ExportTypeScript(_libPath, window.typeList, window.fileName, "");
			string strError = "";
			//ExcelToJson(window.sheetList, window.typeList, window.className, "", ref strError);
#if UNITY_EDITOR
				if (string.IsNullOrEmpty(strError) == false)
					Debug.LogError(strError);
#endif
		}

#if UNITY_EDITOR
			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
#endif
	}

	public static void ExportExcelUserDBScript(string _excelFilePath, string _outputPath = null, string _className = null, bool _prefix = false)
	{
		string _libPath = "";
#if UNITY_EDITOR
		Debug.Log("_excelFilePath : " + _excelFilePath);
		Debug.Log("_outputPath : " + _outputPath);
		_libPath = libPath;
#endif
		ExcelImporterAuto window = GetExcelImporterAuto(_excelFilePath, _className, _prefix);


		/////////////////////////////
		//main func
		/////////////////////////////
		if (Path.GetFileNameWithoutExtension(_excelFilePath) == "EnumList")
		{
			ExcelEnumTable(_libPath, window.sheetList, _outputPath);
			ExcelEnumTableAsTypeScript(_libPath, window.sheetList, _outputPath);
		}
		else
		{
			ExportCustomUserDBEntity(_libPath, window.typeList, window.className, _outputPath);
			ExportUserDBJsonParser(_libPath, window.typeList, window.className, _outputPath);
			ExportCustomDBEntity(_libPath, window.typeList, window.className, _outputPath);
			string strError = "";
			//ExcelToJson(window.sheetList, window.typeList, window.className, "", ref strError);
#if UNITY_EDITOR
			if (string.IsNullOrEmpty(strError) == false)
				Debug.LogError(strError);
#endif
		}

#if UNITY_EDITOR
		AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
#endif
	}

	/// <summary>
	/// ExportJsonParser, 해당 엑셀을 읽고, ExportJsonParser.txt 탬플릿을 기준으로 자동으로 jsonParser를 생성
	/// </summary>   
	#region Create JsonParser
	private static string jsonParser_s_key_prefix = "ExcelJsonKey";

	static void ExportJsonParser(string _libPath, List<ExcelRowParameter> _typeList, string _className, string _outputPath)
	{
		string templateFilePath = _libPath + "Template/ExportJsonParser.txt";

		string importerTemplate = File.ReadAllText(templateFilePath);

		StringBuilder builder = new StringBuilder();

		int rowCount = 0;
		string tab = "            ";

		foreach (ExcelRowParameter row in _typeList)
		{
			if (row.isEnable)
			{
				if (!row.isArray)
				{
					builder.AppendLine();

					switch (row.valueType1)
					{
						case "bool":
							builder.AppendFormat(tab + "p.{0} = bool.Parse ( node [i] [\"{0}\"].ToString() );", (row.name).ToLower());
							break;
						case "double":
							builder.AppendFormat(tab + "p.{0} = double.Parse ( node [i] [\"{0}\"].ToString(), System.Globalization.CultureInfo.GetCultureInfo(\"en-US\") );", (row.name).ToLower());
							break;
						case "int":
							builder.AppendFormat(tab + "p.{0} = int.Parse ( node [i] [\"{0}\"].ToString() );", (row.name).ToLower());
							break;
						case "float":
							builder.AppendFormat(tab + "p.{0} = float.Parse ( node [i] [\"{0}\"].ToString(), System.Globalization.CultureInfo.GetCultureInfo(\"en-US\") );", (row.name).ToLower());
							break;
						case "time":
						case "string":
							builder.AppendFormat(tab + "p.{0} = node [i] [\"{0}\"].ToString();", (row.name).ToLower());
							break;
						case "enum":
							{
								string temp = string.Format("p.{0} = ({1})(System.Enum.Parse (typeof({1}), node [i] [\"{0}\"].ToString()));", (row.name).ToLower(), row.valueType2);
								builder.AppendFormat(tab + temp);
							}
							break;
						case "array":
							{
								switch (row.valueType2)
								{
									case "bool":
										{
											builder.AppendFormat(tab + "p.{0} = new bool[node [i] [\"{0}\"].Count];", (row.name).ToLower());
											builder.AppendLine();
											builder.AppendFormat(tab + "for (int j = 0; j < node [i] [\"{0}\"].Count; j++)", (row.name).ToLower());
											builder.AppendLine();
											builder.AppendFormat(tab + "    p.{0}[j] = bool.Parse(node [i] [\"{0}\"] [j]);", (row.name).ToLower());
										}
										break;
									case "double":
										{
											builder.AppendFormat(tab + "p.{0} = new float[node [i] [\"{0}\"].Count];", (row.name).ToLower());
											builder.AppendLine();
											builder.AppendFormat(tab + "for (int j = 0; j < node [i] [\"{0}\"].Count; j++)", (row.name).ToLower());
											builder.AppendLine();
											builder.AppendFormat(tab + "    p.{0}[j] = double.Parse( node [i] [\"{0}\"] [j], System.Globalization.CultureInfo.GetCultureInfo(\"en-US\") );", (row.name).ToLower());
										}
										break;
									case "int":
										{
											builder.AppendFormat(tab + "p.{0} = new int[node [i] [\"{0}\"].Count];", (row.name).ToLower());
											builder.AppendLine();
											builder.AppendFormat(tab + "for (int j = 0; j < node [i] [\"{0}\"].Count; j++)", (row.name).ToLower());
											builder.AppendLine();
											builder.AppendFormat(tab + "    p.{0}[j] = int.Parse(node [i] [\"{0}\"] [j]);", (row.name).ToLower());
										}
										break;
									case "float":
										{
											builder.AppendFormat(tab + "p.{0} = new float[node [i] [\"{0}\"].Count];", (row.name).ToLower());
											builder.AppendLine();
											builder.AppendFormat(tab + "for (int j = 0; j < node [i] [\"{0}\"].Count; j++)", (row.name).ToLower());
											builder.AppendLine();
											builder.AppendFormat(tab + "    p.{0}[j] = float.Parse( node [i] [\"{0}\"] [j], System.Globalization.CultureInfo.GetCultureInfo(\"en-US\") );", (row.name).ToLower());
										}
										break;
									case "string":
										{
											builder.AppendFormat(tab + "p.{0} = new string[node [i] [\"{0}\"].Count];", (row.name).ToLower());
											builder.AppendLine();
											builder.AppendFormat(tab + "for (int j = 0; j < node [i] [\"{0}\"].Count; j++)", (row.name).ToLower());
											builder.AppendLine();
											builder.AppendFormat(tab + "    p.{0}[j] = node [i] [\"{0}\"] [j];", (row.name).ToLower());
										}
										break;
									case "enum":
										{
											builder.AppendFormat(tab + "p.{0} = new {1}[node [i] [\"{0}\"].Count];", (row.name).ToLower(), row.valueType3);
											builder.AppendLine();
											builder.AppendFormat(tab + "for (int j = 0; j < node [i] [\"{0}\"].Count; j++)", (row.name).ToLower());
											builder.AppendLine();
											string temp = string.Format("   p.{0}[j] = ({1})(System.Enum.Parse (typeof({1}), node [i] [\"{0}\"] [j]));", (row.name).ToLower(), row.valueType3);
											builder.AppendFormat(tab + temp);
										}
										break;
								}
							}
							break;
					}
				}
			}
			rowCount += 1;
		}

		importerTemplate = importerTemplate.Replace("$ExcelData$", _className);
		importerTemplate = importerTemplate.Replace("$EXPORT_DATA$", builder.ToString());
		importerTemplate = importerTemplate.Replace("$ExportTemplate$", _className + "_Parser");

		if (string.IsNullOrEmpty(_outputPath))
		{
			Directory.CreateDirectory(_libPath + "Classes");
			File.WriteAllText(_libPath + "Classes/" + _className + "_Parser.cs", importerTemplate);
		}
		else
		{
			int index = _outputPath.LastIndexOf('/');
			string temp_outputPath = _outputPath.Remove(index);
			Directory.CreateDirectory(temp_outputPath);
			File.WriteAllText(_outputPath + _className + "_Parser.cs", importerTemplate);
		}
	}
	#endregion

	/// <summary>
	/// ExportCustomEntity, 해당 엑셀을 읽고, 템플릿을 기준으로 ( EntityTemplate.txt ), 데이터 클래스 자동 생성
	/// </summary>

	#region Custom Entity Data
	static void ExportCustomEntity(string _libPath, List<ExcelRowParameter> _typeList, string _className, string _outputPath)
	{
		string templateFilePath = _libPath + "Template/EntityTemplate.txt";
		string entittyTemplate = File.ReadAllText(templateFilePath);
		entittyTemplate = entittyTemplate.Replace("\r\n", "\n").Replace("\n", System.Environment.NewLine);
		StringBuilder builder = new StringBuilder();

		foreach (ExcelRowParameter row in _typeList)
		{
			if (row.isEnable)
			{
				if (!row.isArray)
				{
					builder.AppendLine();

					switch (row.valueType1)
					{
						case "enum":
							{
								string enumTypeName = row.name;
								enumTypeName = row.valueType2;

								builder.AppendFormat("      public {0} {1};", enumTypeName, (row.name).ToLower());
							}
							break;
						case "array":
							{
								builder.AppendFormat("      public {0}[] {1};", string.IsNullOrEmpty(row.valueType3) ? row.valueType2 : row.valueType3, row.name.ToLower());
							}
							break;
						case "time":
							{
								builder.AppendFormat("      public {0} {1};", "string", row.name.ToLower());
							}
							break;
						default:
							{
								builder.AppendFormat("      public {0} {1};", row.valueType1, row.name.ToLower());
							}
							break;
					}
				}
			}
		}

		entittyTemplate = entittyTemplate.Replace("$Types$", builder.ToString());
		entittyTemplate = entittyTemplate.Replace("$ExcelData$", _className);

		if (string.IsNullOrEmpty(_outputPath))
		{
			Directory.CreateDirectory(_libPath + "Classes");
			File.WriteAllText(_libPath + "Classes/" + _className + ".cs", entittyTemplate);
		}
		else
		{
			int index = _outputPath.LastIndexOf('/');

			string temp_outputPath = _outputPath.Remove(index);

			Directory.CreateDirectory(temp_outputPath);
			File.WriteAllText(_outputPath + _className + ".cs", entittyTemplate);
		}
	}
	#endregion Custom Entity Data

	#region ExportTypeScript
	//서버용 TypeSecript 생성
	static void ExportTypeScript(string _libPath, List<ExcelRowParameter> _typeList, string _className, string _outputPath)
	{
		string typescriptPath = "";
		try
		{
			string configFilePath = GetApplicationDataPath() + "/../Config/DataScript.xml";
			XmlDocument xml = new XmlDocument();
			xml.Load(configFilePath);

			//xml문서안의 모든 속성을 가져올수 있는 XmlElement입니다. (끝까지 가져옵니다.)
			typescriptPath = xml.SelectSingleNode("Path/TypeScript").InnerText;
		}
		catch (Exception e)
		{
#if UNITY_EDITOR
			Debug.LogWarning(e.Message);
#endif
		}

		if (typescriptPath == null || typescriptPath == "")
			return;

		_outputPath = typescriptPath;
		_className = _className + "Xls";

		string templateFilePath = _libPath + "Template/TypeScriptTemplate.txt";
		string entittyTemplate = File.ReadAllText(templateFilePath);
		entittyTemplate = entittyTemplate.Replace("\r\n", "\n").Replace("\n", System.Environment.NewLine);
		StringBuilder builder = new StringBuilder();

		foreach (ExcelRowParameter row in _typeList)
		{
			if (row.isEnable)
			{
				if (!row.isArray)
				{
					builder.AppendLine();

					string typescriptType = "";
					string initvalue = "";
					switch (row.valueType1)
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
						case "time":
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
						case "enum":
							{
								typescriptType = "EnumList." + row.valueType2;
								initvalue = "null";
							}
							break;
						case "array":
							{
								initvalue = "[]";
								switch (row.valueType2)
								{
									case "bool":
										{
											typescriptType = "boolean[]";
										}
										break;
									case "double":
									case "float":
									case "int":
										{
											typescriptType = "number[]";
										}
										break;
									case "string":
										{
											typescriptType = "string[]";
										}
										break;
									case "enum":
										{
											typescriptType = "EnumList." + row.valueType3 + "[]";
										}
										break;
								}
							}
							break;
					}
					builder.AppendFormat("	public {0} : {1} = {2};", row.name.ToLower(), typescriptType, initvalue);
				}
			}
		}

		entittyTemplate = entittyTemplate.Replace("$Types$", builder.ToString());
		entittyTemplate = entittyTemplate.Replace("$ExcelTable$", _className);

		builder.Remove(0, builder.Length);

		string tab = "            ";
		foreach (ExcelRowParameter row in _typeList)
		{
			if (row.isEnable)
			{
				if (!row.isArray)
				{
					builder.AppendLine();

					switch (row.valueType1)
					{
						case "enum":
							builder.AppendFormat(tab + "p.{0} = EnumList.{1}[data[i][\"{0}\"] as EnumList.typeOf{1}];", (row.name).ToLower(), row.valueType2);
							break;
						case "array":
							{
								//DB Module에 따라 파싱 형태가 다름
								builder.AppendFormat(tab + "p.{0} = JSON.parse(data[i][\"{0}\"]);", (row.name).ToLower());
							}
							break;
						case "time":
							builder.AppendFormat(tab + "p.{0} = new Date(data[i][\"{0}\"]).getTime();", (row.name).ToLower());
							break;
						default:
							builder.AppendFormat(tab + "p.{0} = data[i][\"{0}\"];", (row.name).ToLower());
							break;
					}
				}
			}
		}

		string parserName = _className + "_Parser";

		entittyTemplate = entittyTemplate.Replace("$EXPORT_DATA$", builder.ToString());
		entittyTemplate = entittyTemplate.Replace("$ExcelDataParser$", parserName);

		if (string.IsNullOrEmpty(_outputPath))
		{
			Directory.CreateDirectory(_libPath + "Classes");
			File.WriteAllText(_libPath + "Classes/" + _className + ".ts", entittyTemplate);
		}
		else
		{
			int index = _outputPath.LastIndexOf("\\");

			string temp_outputPath = _outputPath.Remove(index);

			Directory.CreateDirectory(temp_outputPath);
#if UNITY_EDITOR
			Debug.Log(_outputPath);
#endif

			File.WriteAllText(_outputPath + parserName + ".ts", entittyTemplate);
		}
	}
	#endregion ExportTypeScript

	#region EditEnumTableAsTypeScript

	static void ExcelEnumTableAsTypeScript(string _libPath, List<ExcelSheetParameter> _sheetList, string _outputPath)
	{
		string typescriptPath = "";
		try
		{
			string configFilePath = GetApplicationDataPath() + "/../Config/DataScript.xml";
			XmlDocument xml = new XmlDocument();
			xml.Load(configFilePath);

			//xml문서안의 모든 속성을 가져올수 있는 XmlElement입니다. (끝까지 가져옵니다.)
			typescriptPath = xml.SelectSingleNode("Path/TypeScript").InnerText;
		}
		catch (Exception e)
		{
#if UNITY_EDITOR
			Debug.LogWarning(e.Message);
#endif
		}

		if (typescriptPath == null || typescriptPath == "")
			return;

		_outputPath = typescriptPath;

		StringBuilder combineEnumFieldBuilder = new StringBuilder();

		List<StringBuilder> enumBuilderList = new List<StringBuilder>();
		List<string> typeofEnumTypeName = new List<string>();

		foreach (ExcelSheetParameter sheet in _sheetList)
		{
			IRow enumTypeNameRow = enumSheet.GetRow(0);

			for (int i = 0; i < enumTypeNameRow.LastCellNum; i++)
			{
				StringBuilder enumBuilder = new StringBuilder();

				string enumTypeName = enumTypeNameRow.GetCell(i).StringCellValue;
				//enumTypeName = enumTypeName[0].ToString().ToUpper() + enumTypeName.Substring(1);

				enumBuilder.Append("    export enum " + enumTypeName);
				enumBuilder.AppendLine();
				enumBuilder.Append("    {");

				enumBuilderList.Add(enumBuilder);
				typeofEnumTypeName.Add("    export declare type typeOf" + enumTypeName + " = keyof typeof EnumList." + enumTypeName + ";");
			}

			for (int i = 0; i < enumTypeNameRow.LastCellNum; i++)
			{
				int macRowCount = enumSheet.LastRowNum;
				for (int j = 0; j < macRowCount + 1; j++)
				{
					IRow enumCellRow = enumSheet.GetRow(j + 1);
					if (enumCellRow != null)
					{

						if (enumCellRow.GetCell(i) == null)
							continue;

						string cellValue;

						if (enumCellRow.GetCell(i).CellType == CellType.Numeric)
							cellValue = enumCellRow.GetCell(i).NumericCellValue.ToString();
						else
							cellValue = enumCellRow.GetCell(i).StringCellValue;


						if (string.IsNullOrEmpty(cellValue) == false)
						{
							enumBuilderList[i].AppendLine();

							if (j == 0)
							{
								enumBuilderList[i].Append
								("      " + cellValue + " = 0,");
							}
							else
							{

								enumBuilderList[i].Append
								("      " + cellValue + ",");
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

			for (int i = 0; i < enumBuilderList.Count; i++)
			{
				enumBuilderList[i].AppendLine();
				enumBuilderList[i].Append("    }");
				enumBuilderList[i].AppendLine();
				enumBuilderList[i].Append(typeofEnumTypeName[i]);
				if (i < enumBuilderList.Count - 1)
				{
					enumBuilderList[i].AppendLine();
					enumBuilderList[i].AppendLine();
				}
			}

			for (int i = 0; i < enumBuilderList.Count; i++)
			{
				combineEnumFieldBuilder.Append(enumBuilderList[i].ToString());
			}

			string templateFilePath = _libPath + "Template/TypeScriptEnumTemplate.txt";
			string enumTemplate = File.ReadAllText(templateFilePath);
			enumTemplate = enumTemplate.Replace("\r\n", "\n").Replace("\n", System.Environment.NewLine);

			enumTemplate = enumTemplate.Replace("$EnumTypes$", combineEnumFieldBuilder.ToString());

			if (string.IsNullOrEmpty(_outputPath))
			{
				Directory.CreateDirectory(_libPath + "Classes");
				File.WriteAllText(_libPath + "Classes/" + "EnumListTbl" + ".cs", enumTemplate);
			}
			else
			{
				int index = _outputPath.LastIndexOf("\\");
				string temp_outputPath = _outputPath.Remove(index);
				Directory.CreateDirectory(temp_outputPath);
				File.WriteAllText(_outputPath + "EnumListTbl" + ".ts", enumTemplate);
			}
		}
	}
	#endregion

	#region Custom DataTable
	static void ExportCustomTableEntity(string _libPath, List<ExcelRowParameter> _typeList, string _className, string _outputPath)
	{
		string templateFilePath = _libPath + "Template/DataTableTemplate.txt";
		string entittyTemplate = File.ReadAllText(templateFilePath);
		entittyTemplate = entittyTemplate.Replace("\r\n", "\n").Replace("\n", System.Environment.NewLine);

		string _className_Table = _className.Replace("Table","Chart");
		entittyTemplate = entittyTemplate.Replace("$ExcelClass$", _className);
		entittyTemplate = entittyTemplate.Replace("$ExcelData$", _className_Table);

		if (string.IsNullOrEmpty(_outputPath))
		{
			Directory.CreateDirectory(_libPath + "Classes");
			File.WriteAllText(_libPath + "Classes/" + _className_Table + ".cs", entittyTemplate);
		}
		else
		{
			int index = _outputPath.LastIndexOf('/');

			string temp_outputPath = _outputPath.Remove(index);

			Directory.CreateDirectory(temp_outputPath);
			File.WriteAllText(_outputPath + _className_Table + ".cs", entittyTemplate);
		}
	}
	#endregion Custom Entity Data

	#region EditEnumTable

	static void ExcelEnumTable(string _libPath, List<ExcelSheetParameter> _sheetList, string _outputPath)
	{
		StringBuilder combineEnumFieldBuilder = new StringBuilder();

		List<StringBuilder> enumBuilderList = new List<StringBuilder>();

		foreach (ExcelSheetParameter sheet in _sheetList)
		{
			IRow enumTypeNameRow = enumSheet.GetRow(0);

			for (int i = 0; i < enumTypeNameRow.LastCellNum; i++)
			{
				StringBuilder enumBuilder = new StringBuilder();

				string enumTypeName = enumTypeNameRow.GetCell(i).StringCellValue;
				//enumTypeName = enumTypeName[0].ToString().ToUpper() + enumTypeName.Substring(1);

				enumBuilder.Append("public enum " + enumTypeName);
				enumBuilder.AppendLine();
				enumBuilder.Append("{");

				enumBuilderList.Add(enumBuilder);
			}

			for (int i = 0; i < enumTypeNameRow.LastCellNum; i++)
			{
				int macRowCount = enumSheet.LastRowNum;
				for (int j = 0; j < macRowCount + 1; j++)
				{
					IRow enumCellRow = enumSheet.GetRow(j + 1);
					if (enumCellRow != null)
					{
						if (enumCellRow.GetCell(i) == null)
							continue;

						string cellValue;

						if (enumCellRow.GetCell(i).CellType == CellType.Numeric)
							cellValue = enumCellRow.GetCell(i).NumericCellValue.ToString();
						else
							cellValue = enumCellRow.GetCell(i).StringCellValue;
						if (string.IsNullOrEmpty(cellValue) == false)
						{
							enumBuilderList[i].AppendLine();

							if (j == 0)
							{

								enumBuilderList[i].Append
								("  " + cellValue + " = 0,");
							}
							else
							{

								enumBuilderList[i].Append
								("  " + cellValue + ",");
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

			for (int i = 0; i < enumBuilderList.Count; i++)
			{
				enumBuilderList[i].AppendLine();
				enumBuilderList[i].Append("}");
				enumBuilderList[i].AppendLine();
			}

			for (int i = 0; i < enumBuilderList.Count; i++)
			{
				combineEnumFieldBuilder.Append(enumBuilderList[i].ToString());
				combineEnumFieldBuilder.AppendLine();
			}

			string templateFilePath = _libPath + "Template/EnumTemplate.txt";
			string enumTemplate = File.ReadAllText(templateFilePath);
			enumTemplate = enumTemplate.Replace("\r\n", "\n").Replace("\n", System.Environment.NewLine);

			enumTemplate = enumTemplate.Replace("$EnumTypes$", combineEnumFieldBuilder.ToString());

			if (string.IsNullOrEmpty(_outputPath))
			{
				Directory.CreateDirectory(_libPath + "Classes");
				File.WriteAllText(_libPath + "Classes/" + "EnumListTbl" + ".cs", enumTemplate);
			}
			else
			{
				int index = _outputPath.LastIndexOf('/');
				string temp_outputPath = _outputPath.Remove(index);
				Directory.CreateDirectory(temp_outputPath);
				File.WriteAllText(_outputPath + "EnumListTbl" + ".cs", enumTemplate);
			}
		}
	}

	#endregion

	#region ExcelToJson
	/// <summary>
	/// ExcelToJson/ client Test, Only
	/// </summary>
	static bool ExcelToJson(List<ExcelSheetParameter> _sheetList, List<ExcelRowParameter> _typeList, string _className, string _outputPath, ref string strError)
	{
		strError = "";

		string outputJsonValue = string.Empty;

		List<int> idList = new List<int>();
		foreach (ExcelSheetParameter sheet in _sheetList)
		{
			int num = sheet.sheetName.IndexOf('_');

			if (num == -1)
			{
				IRow jsonTitleRow = nomalSheet.GetRow(0);

				StringBuilder jsonTextBuilder = new StringBuilder();

				jsonTextBuilder.Append("[");

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
					IRow jsonRow = nomalSheet.GetRow(i + 2);
					jsonRowList.Add(jsonRow);
				}

				////
				if (strBuilderList.Count > 0)
				{
					for (int i = 0; i < strBuilderList.Count; i++)
					{
						for (int j = 0; j < _typeList.Count; j++)
						{
							if (_typeList[j].name[0] == '$')
								continue;

							if (_typeList[j].name.ToLower().Equals("id") == true)
							{
								if (jsonRowList[i].GetCell(j).CellType == CellType.Formula)
								{
									jsonRowList[i].GetCell(j).SetCellType(CellType.String);
								}
								try
								{
									int id = int.Parse(jsonRowList[i].GetCell(j).ToString());
									if (idList.Contains(id) == true)
									{
										strError = $"Duplicate [sheetName : {sheet.sheetName}] [id : {id}]";
										return false;
									}
									idList.Add(id);
								}
								catch (Exception e)
								{
									strError = $"error id parse [{_className}] [sheetName : {sheet.sheetName}] [id : {jsonRowList[i].GetCell(j)}] [{i} : {j}]\n{e.Message}";
									return false;
								}
							}

							if (jsonRowList[i].GetCell(j) == null && _typeList[j].valueType1 == "string")
							{
								if (j == _typeList.Count - 1)
								{
									strBuilderList[i].Append(
									 "\"" + _typeList[j].name.ToLower() + "\":" +
									 "\"" + "" + "\""
									 );
								}
								else
								{
									strBuilderList[i].Append(
									 "\"" + _typeList[j].name.ToLower() + "\":" +
									 "\"" + "" + "\","
									 );
								}
							}
							else if (jsonRowList[i].GetCell(j) != null)
							{
								if (jsonRowList[i].GetCell(j).CellType == CellType.Formula)
								{
									jsonRowList[i].GetCell(j).SetCellType(CellType.String);
								}

								if (j == _typeList.Count - 1)
								{
									switch (_typeList[j].valueType1)
									{
										case "time":
										case "string":
										case "enum":
											{
												strBuilderList[i].Append(
												 "\"" + _typeList[j].name.ToLower() + "\":" +
												 "\"" + jsonRowList[i].GetCell(j).ToString() + "\""
												 );
											}
											break;
										case "bool":
											{
												strBuilderList[i].Append(
												 "\"" + _typeList[j].name.ToLower() + "\":" + jsonRowList[i].GetCell(j).ToString().ToLower()
												 );
											}
											break;
										default:
											{
												strBuilderList[i].Append(
												 "\"" + _typeList[j].name.ToLower() + "\":" + jsonRowList[i].GetCell(j).ToString()
												 );
											}
											break;
									}
								}
								else
								{
									switch (_typeList[j].valueType1)
									{
										case "time":
										case "string":
										case "enum":
											{
												strBuilderList[i].Append(
												 "\"" + _typeList[j].name.ToLower() + "\":" +
												 "\"" + jsonRowList[i].GetCell(j).ToString() + "\","
												 );
											}
											break;
										case "bool":
											{
												strBuilderList[i].Append(
												 "\"" + _typeList[j].name.ToLower() + "\":" + jsonRowList[i].GetCell(j).ToString().ToLower() + ","
												 );
											}
											break;
										default:
											{
												strBuilderList[i].Append(
												 "\"" + _typeList[j].name.ToLower() + "\":" + jsonRowList[i].GetCell(j).ToString() + ","
												 );
											}
											break;
									}
								}
							}
							else
							{
								strError = string.Format("[sheetName : {0}] {1} 컬럼 {2}번째 data 에러", sheet.sheetName, _typeList[j].name, i + 1);
								return false;

								strBuilderList[i].Append(
								"\"" + _typeList[j].name.ToLower() + "\":" +
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
						if (i < strBuilderList.Count - 1)
							strBuilderList[i].Append(",");

						jsonTextBuilder.Append(strBuilderList[i].ToString());
					}

					jsonTextBuilder.Append("]");

					outputJsonValue = jsonTextBuilder.ToString();
#if UNITY_EDITOR
					Debug.Log("output_Json : " + "\n" + outputJsonValue);
#endif
				}
			}
		}

		if (string.IsNullOrEmpty(_outputPath))
		{
			_outputPath = "excel/localTable/";
		}
		else
		{
			// 기획 데이터 Export용 경로.
			_outputPath += "/Json/";
		}

		if (!Directory.Exists(_outputPath))
			Directory.CreateDirectory(_outputPath);

		File.WriteAllText(_outputPath + _className + ".json", outputJsonValue);
		return true;
	}
	#endregion ExcelToJson

	#region ExcelToCSV
	static void ExcelToCSV(List<ExcelSheetParameter> _sheetList, string _filename, string _outputPath)
	{
		StringBuilder csvTextBuilder = new StringBuilder();
		foreach (ExcelSheetParameter sheet in _sheetList)
		{
			int num = sheet.sheetName.IndexOf('_');

			if (num == -1)
			{
				for (int i = nomalSheet.FirstRowNum; i <= nomalSheet.LastRowNum; i++)
				{
					IRow row = nomalSheet.GetRow(i);
					for (int j = row.FirstCellNum; j <= row.LastCellNum; j++)
					{
						if (row.GetCell(j) != null)
						{
							string value = row.GetCell(j).ToString();
							if (i >= nomalSheet.FirstRowNum + 2)
							{
								value = value.Replace("\"", "\"\"");
								value = value.Replace("[", "\"[");
								value = value.Replace("]", "]\"");
							}

							csvTextBuilder.Append(value);
							if (j != row.Cells.Count - 1)
							{
								csvTextBuilder.Append(",");
							}
						}
						else
						{
							if (j != row.Cells.Count - 1)
							{
								csvTextBuilder.Append(",");
							}
						}
					}

					csvTextBuilder.AppendLine();
				}
			}
		}

		// 기획 데이터 Export용 경로.
		_outputPath += "/Csv/";

		if (!Directory.Exists(_outputPath))
			Directory.CreateDirectory(_outputPath);

		File.WriteAllText(_outputPath + _filename + ".csv", csvTextBuilder.ToString(), Encoding.UTF8);
	}
	#endregion

#if !UNITY_EDITOR
	public static string ExportDataTable(string _targetPath, string _outputPath, string exportFileName)
	{
		string errorCode = "";

		ExcelImporterAuto window = GetExcelImporterAuto(_targetPath);

		if (Path.GetFileNameWithoutExtension(_targetPath) == "EnumList")
		{
		}
		else
		{
			bool res = NewMethod(_outputPath, exportFileName, ref errorCode, window);
			if (res == false)
				return errorCode;

			ExcelToCSV(window.sheetList, window.fileName, _outputPath);
		}

		return errorCode;
	}

	private static bool NewMethod(string _outputPath, string exportFileName, ref string errorCode, ExcelImporterAuto window)
	{
		return ExcelToJson(window.sheetList, window.typeList, exportFileName, _outputPath, ref errorCode);
	}
#endif

	#region ExportJsonCheckerLoader
	//서버용 TypeSecript 생성
	public static void ExportJsonCheckerLoader(string _excelFilePath, string _className, bool isLastFile)
	{
		if (_excelFilePath.Contains("EnumList"))
			return;

		string filename = Path.GetFileNameWithoutExtension(_excelFilePath);

		string typescriptPath = "";
		try
		{
			string configFilePath = GetApplicationDataPath() + "/../Config/DataScript.xml";
			XmlDocument xml = new XmlDocument();
			xml.Load(configFilePath);

			//xml문서안의 모든 속성을 가져올수 있는 XmlElement입니다. (끝까지 가져옵니다.)
			typescriptPath = xml.SelectSingleNode("Path/Checker").InnerText;
		}
		catch (Exception e)
		{
#if UNITY_EDITOR
			Debug.LogWarning(e.Message);
#endif
		}

		if (typescriptPath == null || typescriptPath == "")
			return;

		checkerDataBuilder.AppendFormat("	    private Dictionary<int, {0}.Param> {1};", _className, Char.ToLowerInvariant(filename[0]) + filename.Substring(1) + "Datas");
		checkerDataBuilder.AppendLine();

		checkerParsingBuilder.AppendFormat("	        {0} = {1}.Parsing(jsonFileToString(jsonDataPath, \"{2}.json\")).param;", Char.ToLowerInvariant(filename[0]) + filename.Substring(1) + "Datas",
										_className + "_Parser", "/" + _className);
		checkerParsingBuilder.AppendLine();

		if (isLastFile == false)
			return;

		string templateFilePath = libPath + "Template/JsonDataChecker.txt";
		string entittyTemplate = File.ReadAllText(templateFilePath);
		entittyTemplate = entittyTemplate.Replace("\r\n", "\n").Replace("\n", System.Environment.NewLine);

		entittyTemplate = entittyTemplate.Replace("$Datas$", checkerDataBuilder.ToString());
		entittyTemplate = entittyTemplate.Replace("$Parsing$", checkerParsingBuilder.ToString());

		int index = typescriptPath.LastIndexOf("\\");

		string temp_outputPath = typescriptPath.Remove(index);

		Directory.CreateDirectory(temp_outputPath);
		File.WriteAllText(typescriptPath + "CheckerLoader.cs", entittyTemplate);

		checkerDataBuilder.Length = 0;
		checkerParsingBuilder.Length = 0;
	}
	#endregion ExportTypeScript


	#region Excel to json Checker
	public static Dictionary<string, List<string>> GetEnumNameList(string _targetPath, ref string _error)
	{
#if !UNITY_EDITOR
		if (Path.GetFileNameWithoutExtension(_targetPath) == "EnumList")
		{
			ExcelImporterAuto window = GetExcelImporterAuto(_targetPath);
			return ExcelEnumListTable(window.sheetList, ref _error);
		}
		_targetPath = "[ ERROR ] Not found EnumList file.";
#endif
		return null;
	}
	public static string ExcelToJsonChecker(string _targetPath, Dictionary<string, List<string>> _enumList)
	{
#if !UNITY_EDITOR
		ExcelImporterAuto window = GetExcelImporterAuto(_targetPath);

		if (Path.GetFileNameWithoutExtension(_targetPath) == "EnumList")
		{
		}
		else
		{
			List<string> propertyNameList = new List<string>();
			foreach (ExcelRowParameter row in window.typeList)
			{
				if (row.isEnable)
				{
					if (!row.isArray)
					{
						if (propertyNameList.Contains(row.name) == false)
							propertyNameList.Add(row.name);
						else
						{
							return $"Duplicate property name. [ {_targetPath} ] - [ {row.name} ]";
						}

						if (row.valueType1.Equals("enum") == true)
						{
							if (_enumList.ContainsKey(row.valueType2) == false)
								return $"not found enum. [ {row.valueType2} ]";

							string res = ExcelToJsonChecker_Enum(window, _enumList);
							if (string.IsNullOrEmpty(res) == false)
								return res;
						}
						if (row.valueType1.Equals("array") == true && row.valueType2.Equals("enum") == true)
						{
							if (_enumList.ContainsKey(row.valueType3) == false)
								return $"not found enum. [ {row.valueType3} ]";

							string res = ExcelToJsonChecker_Enum(window, _enumList);
							if (string.IsNullOrEmpty(res) == false)
								return res;
						}
					}
				}
			}
		}
#endif
		return "";
	}
	public static string ExcelToJsonChecker_Enum(ExcelImporterAuto _window, Dictionary<string, List<string>> _enumList)
	{
		foreach (ExcelSheetParameter sheet in _window.sheetList)
		{
			IRow enumTypeNameRow = enumSheet.GetRow(0);

			for (int i = 0; i < enumTypeNameRow.LastCellNum; i++)
			{
				string enumTypeName = enumTypeNameRow.GetCell(i).StringCellValue;
				//enumTypeName = enumTypeName[0].ToString().ToUpper() + enumTypeName.Substring(1);

				List<string> list = null;
				_enumList.TryGetValue(enumTypeName, out list);
				if (list == null)
				{
					return $"not found enum. [ {enumTypeName} ]";
				}

				int macRowCount = enumSheet.LastRowNum;
				for (int j = 0; j < macRowCount + 1; j++)
				{
					IRow enumCellRow = enumSheet.GetRow(j + 1);
					if (enumCellRow != null)
					{
						if (enumCellRow.GetCell(i) == null)
							continue;

						string cellValue;

						if (enumCellRow.GetCell(i).CellType == CellType.Numeric)
							cellValue = enumCellRow.GetCell(i).NumericCellValue.ToString();
						else
							cellValue = enumCellRow.GetCell(i).StringCellValue;

						if (string.IsNullOrEmpty(cellValue) == false && list.Contains(cellValue) == false)
						{
							return $"Not found enum type. [ {enumTypeName} ] - [ {cellValue} ]"; 
						}
					}
				}
			}
		}
		return "";
	}


	#endregion

	#region UserDB
	static void ExportUserDBJsonParser(string _libPath, List<ExcelRowParameter> _typeList, string _className, string _outputPath)
	{
		string templateFilePath = _libPath + "Template/ExportUserDBJsonParser.txt";

		string importerTemplate = File.ReadAllText(templateFilePath);

		StringBuilder builder = new StringBuilder();

		int rowCount = 0;
		string tab = "            ";

		foreach (ExcelRowParameter row in _typeList)
		{
			if (row.isEnable)
			{
				if (!row.isArray)
				{
					builder.AppendLine();

					switch (row.valueType1)
					{
						case "bool":
							builder.AppendFormat(tab + "p.{0} = bool.Parse ( node [i] [\"{0}\"].ToString() );", (row.name).ToLower());
							break;
						case "double":
							builder.AppendFormat(tab + "p.{0} = double.Parse ( node [i] [\"{0}\"].ToString(), System.Globalization.CultureInfo.GetCultureInfo(\"en-US\") );", (row.name).ToLower());
							break;
						case "int":
							builder.AppendFormat(tab + "p.{0} = int.Parse ( node [i] [\"{0}\"].ToString() );", (row.name).ToLower());
							break;
						case "float":
							builder.AppendFormat(tab + "p.{0} = float.Parse ( node [i] [\"{0}\"].ToString(), System.Globalization.CultureInfo.GetCultureInfo(\"en-US\") );", (row.name).ToLower());
							break;
						case "time":
						case "string":
							builder.AppendFormat(tab + "p.{0} = node [i] [\"{0}\"].ToString();", (row.name).ToLower());
							break;
						case "enum":
							{
								string temp = string.Format("p.{0} = ({1})(System.Enum.Parse (typeof({1}), node [i] [\"{0}\"].ToString()));", (row.name).ToLower(), row.valueType2);
								builder.AppendFormat(tab + temp);
							}
							break;
						case "array":
							{
								switch (row.valueType2)
								{
									case "bool":
										{
											builder.AppendFormat(tab + "p.{0} = new bool[node [i] [\"{0}\"].Count];", (row.name).ToLower());
											builder.AppendLine();
											builder.AppendFormat(tab + "for (int j = 0; j < node [i] [\"{0}\"].Count; j++)", (row.name).ToLower());
											builder.AppendLine();
											builder.AppendFormat(tab + "    p.{0}[j] = bool.Parse(node [i] [\"{0}\"] [j]);", (row.name).ToLower());
										}
										break;
									case "double":
										{
											builder.AppendFormat(tab + "p.{0} = new float[node [i] [\"{0}\"].Count];", (row.name).ToLower());
											builder.AppendLine();
											builder.AppendFormat(tab + "for (int j = 0; j < node [i] [\"{0}\"].Count; j++)", (row.name).ToLower());
											builder.AppendLine();
											builder.AppendFormat(tab + "    p.{0}[j] = double.Parse( node [i] [\"{0}\"] [j], System.Globalization.CultureInfo.GetCultureInfo(\"en-US\") );", (row.name).ToLower());
										}
										break;
									case "int":
										{
											builder.AppendFormat(tab + "p.{0} = new int[node [i] [\"{0}\"].Count];", (row.name).ToLower());
											builder.AppendLine();
											builder.AppendFormat(tab + "for (int j = 0; j < node [i] [\"{0}\"].Count; j++)", (row.name).ToLower());
											builder.AppendLine();
											builder.AppendFormat(tab + "    p.{0}[j] = int.Parse(node [i] [\"{0}\"] [j]);", (row.name).ToLower());
										}
										break;
									case "float":
										{
											builder.AppendFormat(tab + "p.{0} = new float[node [i] [\"{0}\"].Count];", (row.name).ToLower());
											builder.AppendLine();
											builder.AppendFormat(tab + "for (int j = 0; j < node [i] [\"{0}\"].Count; j++)", (row.name).ToLower());
											builder.AppendLine();
											builder.AppendFormat(tab + "    p.{0}[j] = float.Parse( node [i] [\"{0}\"] [j], System.Globalization.CultureInfo.GetCultureInfo(\"en-US\") );", (row.name).ToLower());
										}
										break;
									case "string":
										{
											builder.AppendFormat(tab + "p.{0} = new string[node [i] [\"{0}\"].Count];", (row.name).ToLower());
											builder.AppendLine();
											builder.AppendFormat(tab + "for (int j = 0; j < node [i] [\"{0}\"].Count; j++)", (row.name).ToLower());
											builder.AppendLine();
											builder.AppendFormat(tab + "    p.{0}[j] = node [i] [\"{0}\"] [j];", (row.name).ToLower());
										}
										break;
									case "enum":
										{
											builder.AppendFormat(tab + "p.{0} = new {1}[node [i] [\"{0}\"].Count];", (row.name).ToLower(), row.valueType3);
											builder.AppendLine();
											builder.AppendFormat(tab + "for (int j = 0; j < node [i] [\"{0}\"].Count; j++)", (row.name).ToLower());
											builder.AppendLine();
											string temp = string.Format("   p.{0}[j] = ({1})(System.Enum.Parse (typeof({1}), node [i] [\"{0}\"] [j]));", (row.name).ToLower(), row.valueType3);
											builder.AppendFormat(tab + temp);
										}
										break;
								}
							}
							break;
					}
				}
			}
			rowCount += 1;
		}

		importerTemplate = importerTemplate.Replace("$ExcelData$", _className);
		importerTemplate = importerTemplate.Replace("$EXPORT_DATA$", builder.ToString());
		importerTemplate = importerTemplate.Replace("$ExportTemplate$", _className + "_Parser");

		if (string.IsNullOrEmpty(_outputPath))
		{
			Directory.CreateDirectory(_libPath + "Classes");
			File.WriteAllText(_libPath + "Classes/" + _className + "_Parser.cs", importerTemplate);
		}
		else
		{
			int index = _outputPath.LastIndexOf('/');
			string temp_outputPath = _outputPath.Remove(index);
			Directory.CreateDirectory(temp_outputPath);
			File.WriteAllText(_outputPath + _className + "_Parser.cs", importerTemplate);
		}
	}

	static void ExportCustomDBEntity(string _libPath, List<ExcelRowParameter> _typeList, string _className, string _outputPath)
	{
		string templateFilePath = _libPath + "Template/EntityTemplate.txt";
		string entittyTemplate = File.ReadAllText(templateFilePath);
		entittyTemplate = entittyTemplate.Replace("\r\n", "\n").Replace("\n", System.Environment.NewLine);
		StringBuilder builder = new StringBuilder();

		foreach (ExcelRowParameter row in _typeList)
		{
			if (row.isEnable)
			{
				if (!row.isArray)
				{
					builder.AppendLine();

					switch (row.valueType1)
					{
						case "enum":
							{
								string enumTypeName = row.name;
								enumTypeName = row.valueType2;

								builder.AppendFormat("      public {0} {1};", enumTypeName, (row.name).ToLower());
							}
							break;
						case "array":
							{
								builder.AppendFormat("      public {0}[] {1};", string.IsNullOrEmpty(row.valueType3) ? row.valueType2 : row.valueType3, row.name.ToLower());
							}
							break;
						case "time":
							{
								builder.AppendFormat("      public {0} {1};", "string", row.name.ToLower());
							}
							break;
						default:
							{
								builder.AppendFormat("      public {0} {1};", row.valueType1, row.name.ToLower());
							}
							break;
					}
				}
			}
		}

		entittyTemplate = entittyTemplate.Replace("$Types$", builder.ToString());
		entittyTemplate = entittyTemplate.Replace("$ExcelData$", _className);

		if (string.IsNullOrEmpty(_outputPath))
		{
			Directory.CreateDirectory(_libPath + "Classes");
			File.WriteAllText(_libPath + "Classes/" + _className + ".cs", entittyTemplate);
		}
		else
		{
			int index = _outputPath.LastIndexOf('/');

			string temp_outputPath = _outputPath.Remove(index);

			Directory.CreateDirectory(temp_outputPath);
			File.WriteAllText(_outputPath + _className + ".cs", entittyTemplate);
		}
	}

	static void ExportCustomUserDBEntity(string _libPath, List<ExcelRowParameter> _typeList, string _className, string _outputPath)
	{
		string templateFilePath = _libPath + "Template/DataTableTemplate.txt";
		string entittyTemplate = File.ReadAllText(templateFilePath);
		entittyTemplate = entittyTemplate.Replace("\r\n", "\n").Replace("\n", System.Environment.NewLine);

		string _className_Table = _className.Replace("Table","DB");
		entittyTemplate = entittyTemplate.Replace("$ExcelClass$", _className);
		entittyTemplate = entittyTemplate.Replace("$ExcelData$", _className_Table);

		if (string.IsNullOrEmpty(_outputPath))
		{
			Directory.CreateDirectory(_libPath + "Classes");
			File.WriteAllText(_libPath + "Classes/" + _className_Table + ".cs", entittyTemplate);
		}
		else
		{
			int index = _outputPath.LastIndexOf('/');

			string temp_outputPath = _outputPath.Remove(index);

			Directory.CreateDirectory(temp_outputPath);
			File.WriteAllText(_outputPath + _className_Table + ".cs", entittyTemplate);
		}
	}
	#endregion UserDB
}