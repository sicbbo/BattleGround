using UnityEngine;
using UnityEditor;

public class EffectTool : BaseTool<EffectData, EffectTool, GameObject, EffectClip>
{
    [MenuItem("Tools/Effect Tool")]
    private static void Init()
    {
        CreateWindow("Effect Tool");
    }

    protected override void MakeClipItems()
    {
        EffectClip selectedClip = data.effectClips[selection];
        EditorHelper.MakeLabelField("ID", selection, uiWidthMiddle);
        EditorHelper.MakeTextField("Name", ref data.names[selection], uiWidthLarge);
        EditorHelper.MakeEnumPopup<EffectType>("EffectType", ref selectedClip.effectType, uiWidthMiddle);
        EditorGUILayout.Separator();
        MakeSource(ref selectedClip, "EffectObject");
    }

    protected override void CreateEnumStructure()
    {
        CreateEnumStructure("EffectList", "Effect");
    }
}