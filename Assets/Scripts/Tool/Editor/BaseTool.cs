using UnityEngine;
using UnityEditor;
using System.Text;
using UnityObject = UnityEngine.Object;

public abstract class BaseTool<Data, Tool, Source, Clip> : EditorWindow where Data : BaseData where Tool : EditorWindow
    where Source : UnityObject where Clip : BaseClip<Source>
{
    protected int uiWidthLarge = 450;
    protected int uiWidthMiddle = 300;
    protected int uiWidthSmall = 200;
    protected int selection = 0;
    protected Vector2 SP1 = Vector2.zero;
    protected Vector2 SP2 = Vector2.zero;
    protected Source source = null;

    protected static Data data = null;

    protected static void CreateWindow(string title)
    {
        data = ScriptableObject.CreateInstance<Data>();
        data.LoadData();

        Tool window = GetWindow<Tool>(false, title);
        window.Show();
    }

    protected abstract void MakeClipItems();

    protected void MakeSource(ref Clip clip, string title)
    {
        if (source == null && clip.clipName != string.Empty)
        {
            source = ResourceManager.Load(clip.clipPath + clip.clipName) as Source;
        }
        EditorHelper.MakeObjectField<Source>(title, ref source, uiWidthLarge);
        if (source != null)
        {
            MakeSource(ref clip);
        }
        else
        {
            clip.clipPath = string.Empty;
            clip.clipName = string.Empty;
            //source = null;
        }
    }

    protected virtual void MakeSource(ref Clip clip)
    {
        clip.clipPath = EditorHelper.GetPath(source);
        clip.clipName = source.name;
    }

    private void OnGUI()
    {
        if (data == null)
            return;

        EditorGUILayout.BeginVertical();
        {
            //: 상단 Add, Copy, Remove 버튼 툴
            UnityObject source = this.source;
            EditorHelper.EditorToolTopLayer(data, ref selection, ref source, uiWidthMiddle);
            this.source = (Source)source;

            EditorGUILayout.BeginHorizontal();
            {
                //: 중간 클립 리스트
                EditorHelper.EditorToolListLayer(ref SP1, data, ref selection, ref source, uiWidthMiddle);
                this.source = (Source)source;

                //: 클립 설정
                EditorGUILayout.BeginVertical();
                {
                    //: 설정 부분도 스크롤기능이 가능하게
                    SP2 = EditorGUILayout.BeginScrollView(SP2);
                    {
                        if (data.GetDataCount() > 0)
                        {
                            EditorGUILayout.BeginVertical();
                            {
                                EditorGUILayout.Separator();
                                MakeClipItems();
                                EditorGUILayout.Separator();
                            }
                            EditorGUILayout.EndVertical();
                        }
                    }
                    EditorGUILayout.EndScrollView();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Separator();
        //: 하단
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Reload Setting"))
            {
                data = CreateInstance<Data>();
                data.LoadData();
                selection = 0;
                source = null;
            }
            if (GUILayout.Button("Save"))
            {
                data.SaveData();
                CreateEnumStructure();
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    protected abstract void CreateEnumStructure();

    protected void CreateEnumStructure(string className, string folderName)
    {
        string enumName = className;
        StringBuilder builder = new StringBuilder();
        builder.AppendLine();
        for (int i = 0; i < data.names.Length; i++)
        {
            if (data.names[i] != string.Empty)
            {
                if (data.names[i].ToLower().Contains("none") == false)
                    builder.AppendLine("    " + data.names[i] + " = " + i + ",");
            }
        }

        EditorHelper.CreateEnumStructure(folderName, enumName, builder);
    }
}