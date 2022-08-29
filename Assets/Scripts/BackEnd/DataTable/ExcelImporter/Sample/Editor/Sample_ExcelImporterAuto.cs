using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//public class Sample_ExcelImporterAuto : EditorWindow
//{
//    static string excelFilePath = "Assets/Adalib/ExcelImporter/Sample/SampleExcelFile/Item.xlsx";
//    static string excelFilePath2 = "Assets/Adalib/ExcelImporter/Sample/SampleExcelFile/PartsItem.xlsx";
//	static string outputPath = "Assets/SampleOutput/";

//    [MenuItem("Custom/AutoExcel/Item Excel")]
//    static void SampleFunc()
//    {
//        //prefix => "Entity_"
//        ExcelImporterAuto.ExportExcelScript(excelFilePath, outputPath, "AutoSample");

//        //non prefix
//        //ExcellImporterAuto.ExportExcellScript(excelFilePath, outputPath, "AutoSample", false);
//    }

//    [MenuItem("Custom/AutoExcel/PartsItem Excel")]
//    static void SampleFunc2()
//    {
//        //prefix => "Entity_"
//		ExcelImporterAuto.ExportExcelScript(excelFilePath2, outputPath, "PartsItemSample");

//        //non prefix
//        //ExcellImporterAuto.ExportExcellScript(excelFilePath, outputPath, "AutoSample", false);
//    }
//}