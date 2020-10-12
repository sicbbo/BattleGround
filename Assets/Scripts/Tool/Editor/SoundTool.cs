using UnityEngine;
using UnityEditor;
using System.Text;
using UnityObject = UnityEngine.Object;

public class SoundTool : BaseTool<SoundData, SoundTool, AudioClip, SoundClip>
{
    [MenuItem("Tools/Sound Tool")]
    private static void Init()
    {
        CreateWindow("Sound Tool");
    }

    protected override void MakeClipItems()
    {
        SoundClip sound = data.soundClips[selection];
        EditorHelper.MakeLabelField("ID", selection, uiWidthLarge);
        EditorHelper.MakeTextField("Name", ref data.names[selection], uiWidthLarge);
        EditorHelper.MakeEnumPopup<SoundPlayType>("PlayType", ref sound.playType, uiWidthLarge);
        EditorHelper.MakeFloatField("MaxVolume", ref sound.maxVolume, uiWidthLarge);
        EditorHelper.MakeToggle("LoopClip", ref sound.isLoop, uiWidthLarge);
        EditorGUILayout.Separator();
        MakeSource(ref sound, "AudioClip");
        EditorGUILayout.Separator();
        if (GUILayout.Button("Add Loop", GUILayout.Width(uiWidthMiddle)))
        {
            sound.AddLoop();
        }
        for (int i = 0; i < sound.checkTime.Length; i++)
        {
            EditorGUILayout.BeginVertical();
            {
                GUILayout.Label("Loop Step " + i, EditorStyles.boldLabel);
                if (GUILayout.Button("Remove", GUILayout.Width(uiWidthMiddle)))
                {
                    sound.RemoveLoop(i);
                }
                if (sound.checkTime.Length > i)
                {
                    EditorHelper.MakeFloatField("check Time", ref sound.checkTime[i], uiWidthMiddle);
                    EditorHelper.MakeFloatField("set Time", ref sound.setTime[i], uiWidthMiddle);
                }
            }
            EditorGUILayout.EndVertical();
        }
    }

    protected override void MakeSource(ref SoundClip clip)
    {
        base.MakeSource(ref clip);

        EditorHelper.MakeSlider("Pitch", ref clip.pitch, -3.0f, 3.0f, uiWidthLarge);
        EditorHelper.MakeSlider("Dopper", ref clip.dopplerLevel, 0.0f, 5.0f, uiWidthLarge);
        EditorHelper.MakeEnumPopup<AudioRolloffMode>("volume RollOff", ref clip.rolloffMode, uiWidthLarge);
        EditorHelper.MakeFloatField("min Distance", ref clip.minDistance, uiWidthLarge);
        EditorHelper.MakeFloatField("max Distance", ref clip.maxDistance, uiWidthLarge);
        EditorHelper.MakeSlider("PanLevel", ref clip.spatialBlend, 0.0f, 1.0f, uiWidthLarge);
    }

    protected override void CreateEnumStructure()
    {
        CreateEnumStructure("SoundList", "Sound");
    }
}