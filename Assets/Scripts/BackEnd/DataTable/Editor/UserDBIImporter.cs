using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class UserDBIImporter : EditorWindow
{
	private static string excelUserDBFilePath = "Assets/../excel/UserDB/";
	private static string outputUserDBPath = "Assets/Scripts/BackEnd/DataTable/UserDB/";

	private string[] allExcelFiles;
	private string[] selectedExcelFiles;
	private bool[] toggleSelectExcelFiles;
	private bool selectAll;
	private Vector2 scrollPos = Vector2.zero;


	[MenuItem("Tools/- Excel UserDB/일괄 추출", priority = 200)]
	static void ImportXls()
	{
		string[] fileNames = Directory.GetFiles(excelUserDBFilePath, "*.xlsx");
		foreach (string excelFileName in fileNames)
		{
			ExcelImporterAuto.ExportExcelUserDBScript(excelFileName, outputUserDBPath, Path.GetFileNameWithoutExtension(excelFileName), false);
		}

		foreach (string excelFileName in fileNames)
		{
			ExcelImporterAuto.ExportJsonCheckerLoader(excelFileName, Path.GetFileNameWithoutExtension(excelFileName), excelFileName == fileNames[fileNames.Length - 1]);
		}
		AssetDatabase.Refresh();
	}

	[MenuItem("Tools/- Excel UserDB/선택 추출...", priority = 201)]
	static void CreateWindow()
	{
		EditorWindow this_wnd = GetWindow(typeof(UserDBIImporter));
		this_wnd.titleContent = new GUIContent("UserDBIImporter");
	}

	private void OnGUI()
	{
		EditorGUILayout.Space();

		using (EditorGUILayout.HorizontalScope hs = new EditorGUILayout.HorizontalScope())
		{
			if (GUILayout.Button("Excel UserDB 저장소에서 가져오기"))
			{
				allExcelFiles = Directory.GetFiles(excelUserDBFilePath, "*.xlsx");
				int count = allExcelFiles.Length;
				if (count > 0)
				{
					selectedExcelFiles = new string[count];
					toggleSelectExcelFiles = new bool[count];
					System.Array.Copy(allExcelFiles, selectedExcelFiles, count);
				}
			}

			if (GUILayout.Button("로컬파일 사용"))
			{
				allExcelFiles = Directory.GetFiles(excelUserDBFilePath, "*.xlsx");
				int count = allExcelFiles.Length;
				if (count > 0)
				{
					selectedExcelFiles = new string[count];
					toggleSelectExcelFiles = new bool[count];
					System.Array.Copy(allExcelFiles, selectedExcelFiles, count);
				}
			}
		}

		if (allExcelFiles == null)
			return;

		EditorGUILayout.Separator();

		if (GUILayout.Toggle(selectAll, "Select All") != selectAll)
		{
			selectAll = !selectAll;
			for (int i = 0; i != toggleSelectExcelFiles.Length; ++i)
			{
				toggleSelectExcelFiles[i] = selectAll;
			}
		}

		EditorGUILayout.Space();
		using (EditorGUILayout.ScrollViewScope sv1 = new EditorGUILayout.ScrollViewScope(scrollPos, "box"))
		{
			scrollPos = sv1.scrollPosition;

			EditorGUILayout.Separator();

			for (int i = 0; i != selectedExcelFiles.Length; ++i)
			{
				toggleSelectExcelFiles[i] = GUILayout.Toggle(toggleSelectExcelFiles[i], Path.GetFileName(selectedExcelFiles[i]));
			}

			if (selectAll == true)
			{
				if (toggleSelectExcelFiles.Where(p => p == false).Count() > 0)
				{
					selectAll = GUILayout.Toggle(false, "Select All");
				}
			}

			EditorGUILayout.Separator();
		}

		if (GUILayout.Button("선택파일 추출"))
		{
			for (int i = 0; i != selectedExcelFiles.Length; ++i)
			{
				if (toggleSelectExcelFiles[i] == false)
					continue;

				string excelfileName = selectedExcelFiles[i];
				ExcelImporterAuto.ExportExcelUserDBScript(excelfileName, outputUserDBPath, Path.GetFileNameWithoutExtension(excelfileName), false);
			}

			foreach (string excelFileName in allExcelFiles)
			{
				ExcelImporterAuto.ExportJsonCheckerLoader(excelFileName, Path.GetFileNameWithoutExtension(excelFileName), excelFileName == allExcelFiles[allExcelFiles.Length - 1]);
			}
			AssetDatabase.Refresh();
		}
		EditorGUILayout.Separator();
	}
}

