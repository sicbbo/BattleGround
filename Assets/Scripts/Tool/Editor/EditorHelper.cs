using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using UnityObject = UnityEngine.Object;

public class EditorHelper
{

	/// <summary>
	/// 경로 계산 함수.
	/// </summary>
	/// <param name="p_clip"></param>
	/// <returns></returns>
	public static string GetPath(UnityEngine.Object p_clip)
	{
		string retString = string.Empty;
		retString = AssetDatabase.GetAssetPath(p_clip);
		string[] path_node = retString.Split('/'); //Assets/9.ResourcesData/Resources/Sound/BGM.wav
		bool findResource = false;
		for (int i = 0; i < path_node.Length - 1; i++)
		{
			if (findResource == false)
			{
				if (path_node[i] == "Resources")
				{
					findResource = true;
					retString = string.Empty;
				}
			}
			else
			{
				retString += path_node[i] + "/";
			}

		}

		return retString;
	}

	/// <summary>
	/// Data 리스트를 enum structure로 뽑아주는 함수.
	/// </summary>
	public static void CreateEnumStructure(string folderName, string enumName, StringBuilder data)
	{
		string templateFilePath = "Assets/Editor/EnumTemplate.txt";

		string entittyTemplate = File.ReadAllText(templateFilePath);

		entittyTemplate = entittyTemplate.Replace("$DATA$", data.ToString());
		entittyTemplate = entittyTemplate.Replace("$ENUM$", enumName);
		string folderPath = "Assets/Scripts/GameData/" + folderName + "/";
		if (Directory.Exists(folderPath) == false)
		{
			Directory.CreateDirectory(folderPath);
		}

		string FilePath = folderPath + enumName + ".cs";
		if (File.Exists(FilePath))
		{
			File.Delete(FilePath);
		}
		File.WriteAllText(FilePath, entittyTemplate);
	}

	public static void EditorToolTopLayer(BaseData data, ref int selection, ref UnityObject source, int uiWidth)
    {
		EditorGUILayout.BeginHorizontal();
        {
			if (GUILayout.Button("Add", GUILayout.Width(uiWidth)))
            {
				data.AddData("New Data");
				selection = data.GetDataCount() - 1;
				source = null;
            }
			if (GUILayout.Button("Copy", GUILayout.Width(uiWidth)))
            {
				data.CopyData(selection);
				selection = data.GetDataCount() - 1;
				source = null;
            }
			if (data.GetDataCount() > 1)
            {
				if (GUILayout.Button("Remove", GUILayout.Width(uiWidth)))
                {
					data.RemoveData(selection);
					selection = selection + 1;
					source = null;
                }
            }

			if (selection > data.GetDataCount() - 1)
            {
				selection = data.GetDataCount() - 1;
            }
        }
		EditorGUILayout.EndHorizontal();
    }

	public static void EditorToolListLayer(ref Vector2 scrollPosition, BaseData data, ref int selection, ref UnityObject source, int uiWidth)
    {
		EditorGUILayout.BeginVertical(GUILayout.Width(uiWidth));
        {
			EditorGUILayout.Separator();
			EditorGUILayout.BeginVertical("box");
            {
				scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                {
					if (data.GetDataCount() > 0)
                    {
						int lastSelection = selection;
						selection = GUILayout.SelectionGrid(selection, data.GetNameList(true), 1);
						if (lastSelection != selection)
							source = null;
                    }
                }
				EditorGUILayout.EndScrollView();
            }
			EditorGUILayout.EndVertical();
        }
		EditorGUILayout.EndVertical();
    }

	public static void MakeLabelField(string title, int content, int width)
    {
		EditorGUILayout.LabelField(title, content.ToString(), GUILayout.Width(width));
	}

	public static void MakeTextField(string title, ref string content, int width)
    {
		content = EditorGUILayout.TextField(title, content, GUILayout.Width(width));
	}
	
	public static void MakeFloatField(string title, ref float value, int width)
    {
		value = EditorGUILayout.FloatField(title, value, GUILayout.Width(width));
	}

	public static void MakeEnumPopup<T>(string title, ref T type, int width) where T : Enum
    {
		type = (T)EditorGUILayout.EnumPopup(title, type, GUILayout.Width(width));
	}

	public static void MakeObjectField<T>(string title, ref T source, int width) where T : UnityEngine.Object
	{
		source = (T)EditorGUILayout.ObjectField(title, source, typeof(T), false, GUILayout.Width(width));
	}

	public static void MakeToggle(string title, ref bool flag, int width)
    {
		flag = EditorGUILayout.Toggle(title, flag, GUILayout.Width(width));
    }

	public static void MakeSlider(string title, ref float value, float min, float max, int width)
    {
		value = EditorGUILayout.Slider(title, value, min, max, GUILayout.Width(width));
	}
}
