using UnityEngine;
using System;
using System.Xml;
using System.IO;

public class SoundData : BaseData
{
    private const string SOUND = "sound";
    private const string CLIP = "clip";

    public SoundClip[] soundClips = new SoundClip[0];

    public SoundData()
    {
        xmlFilePath = "";
        xmlFileName = "soundData.xml";
        dataPath = "Data/soundData";
        clipPath = "Sounds/";
    }

    public override void SaveData()
    {
        using (XmlTextWriter xml = new XmlTextWriter(xmlFilePath + xmlFileName, System.Text.Encoding.Unicode))
        {
            xml.WriteStartDocument();
            xml.WriteStartElement(SOUND);
            xml.WriteElementString("length", GetDataCount().ToString());
            xml.WriteWhitespace("\n");

            for (int i = 0; i < this.names.Length; i++)
            {
                SoundClip clip = soundClips[i];
                xml.WriteStartElement(CLIP);
                xml.WriteElementString("id", i.ToString());
                xml.WriteElementString("name", names[i]);
                xml.WriteElementString("loops", clip.checkTime.Length.ToString());
                xml.WriteElementString("maxvol", clip.maxVolume.ToString());
                xml.WriteElementString("pitch", clip.pitch.ToString());
                xml.WriteElementString("dopperlevel", clip.dopplerLevel.ToString());
                xml.WriteElementString("rolloffmode", clip.rolloffMode.ToString());
                xml.WriteElementString("mindistance", clip.minDistance.ToString());
                xml.WriteElementString("maxdistance", clip.maxDistance.ToString());
                xml.WriteElementString("spatialblend", clip.spatialBlend.ToString());
                if (clip.isLoop == true)
                {
                    xml.WriteElementString("loop", "true");
                }
                xml.WriteElementString("clippath", clip.clipPath);
                xml.WriteElementString("clipname", clip.clipName);
                xml.WriteElementString("checktimecount", clip.checkTime.Length.ToString());
                string str = "";
                foreach(float t in clip.checkTime)
                {
                    str += t.ToString() + "/";
                }
                xml.WriteElementString("checktime", str);
                xml.WriteElementString("settimecount", clip.setTime.Length.ToString());
                str = "";
                foreach (float t in clip.setTime)
                {
                    str += t.ToString() + "/";
                }
                xml.WriteElementString("settime", str);
                xml.WriteElementString("type", clip.playType.ToString());
                xml.WriteEndElement();
            }

            xml.WriteEndElement();
            xml.WriteEndDocument();
        }
    }

    public override void LoadData()
    {
        this.xmlFilePath = string.Format("{0}{1}", Application.dataPath, dataDirectory);
        TextAsset asset = (TextAsset)ResourceManager.Load(dataPath);
        if (asset == null || asset.text == null)
        {
            this.AddData("New Sound");
            return;
        }

        using (XmlTextReader reader = new XmlTextReader(new StringReader(asset.text)))
        {
            int currentID = 0;
            while (reader.Read())
            {
                if (reader.IsStartElement())
                {
                    switch (reader.Name)
                    {
                        case "length":
                            int length = int.Parse(reader.ReadString());
                            names = new string[length];
                            soundClips = new SoundClip[length];
                            break;
                        case "clip":
                            break;
                        case "id":
                            currentID = int.Parse(reader.ReadString());
                            soundClips[currentID] = new SoundClip();
                            soundClips[currentID].realID = currentID;
                            break;
                        case "name":
                            names[currentID] = reader.ReadString();
                            break;
                        case "loops":
                            int count = int.Parse(reader.ReadString());
                            soundClips[currentID].checkTime = new float[count];
                            soundClips[currentID].setTime = new float[count];
                            break;
                        case "maxvol":
                            soundClips[currentID].maxVolume = float.Parse(reader.ReadString());
                            break;
                        case "pitch":
                            soundClips[currentID].pitch = float.Parse(reader.ReadString());
                            break;
                        case "dopperlevel":
                            soundClips[currentID].dopplerLevel = float.Parse(reader.ReadString());
                            break;
                        case "rolloffmode":
                            soundClips[currentID].rolloffMode = (AudioRolloffMode)Enum.Parse(typeof(AudioRolloffMode), reader.ReadString());
                            break;
                        case "mindistance":
                            soundClips[currentID].minDistance = float.Parse(reader.ReadString());
                            break;
                        case "maxdistance":
                            soundClips[currentID].maxDistance = float.Parse(reader.ReadString());
                            break;
                        case "spatialblend":
                            soundClips[currentID].spatialBlend = float.Parse(reader.ReadString());
                            break;
                        case "loop":
                            soundClips[currentID].isLoop = true;
                            break;
                        case "clippath":
                            soundClips[currentID].clipPath = reader.ReadString();
                            break;
                        case "clipname":
                            soundClips[currentID].clipName = reader.ReadString();
                            break;
                        case "checktimecount":
                            break;
                        case "checktime":
                            SetLoopTime(true, soundClips[currentID], reader.ReadString());
                            break;
                        case "settime":
                            SetLoopTime(false, soundClips[currentID], reader.ReadString());
                            break;
                        case "type":
                            soundClips[currentID].playType = (SoundPlayType)Enum.Parse(typeof(SoundPlayType), reader.ReadString());
                            break;
                    }
                }
            }
        }

        foreach(SoundClip clip in soundClips)
        {
            clip.PreLoad();
        }
    }

    private void SetLoopTime(bool isCheck, SoundClip clip, string timeString)
    {
        string[] time = timeString.Split('/');
        for (int i = 0; i < time.Length; i++)
        {
            if (time[i] != string.Empty)
            {
                if (isCheck == true)
                {
                    clip.checkTime[i] = float.Parse(time[i]);
                }
                else
                {
                    clip.setTime[i] = float.Parse(time[i]);
                }
            }
        }
    }

    public override int AddData(string newName)
    {
        if (names == null)
        {
            names = new string[] { newName };
            soundClips = new SoundClip[] { new SoundClip() };
        }
        else
        {
            names = ArrayHelper.Add(newName, names);
            soundClips = ArrayHelper.Add(new SoundClip(), soundClips);
        }

        return GetDataCount();
    }

    public override void RemoveData(int index)
    {
        names = ArrayHelper.Remove(index, names);
        if (names.Length == 0)
            names = null;

        soundClips = ArrayHelper.Remove(index, soundClips);
    }

    public override void CopyData(int index)
    {
        names = ArrayHelper.Add(names[index], names);
        //effectClips = ArrayHelper.Add(effectClips[index], effectClips);
        soundClips = ArrayHelper.Add(GetCopyClip(index), soundClips);
    }

    public SoundClip GetCopyClip(int index)
    {
        if (index < 0 || index >= soundClips.Length)
            return null;

        SoundClip original = soundClips[index];
        SoundClip clip = new SoundClip();
        clip.realID = soundClips.Length;
        clip.clipPath = original.clipPath;
        clip.clipName = original.clipName;
        clip.maxVolume = original.maxVolume;
        clip.pitch = original.pitch;
        clip.dopplerLevel = original.dopplerLevel;
        clip.rolloffMode = original.rolloffMode;
        clip.minDistance = original.minDistance;
        clip.maxDistance = original.maxDistance;
        clip.spatialBlend = original.spatialBlend;
        clip.isLoop = original.isLoop;
        clip.checkTime = new float[original.checkTime.Length];
        clip.setTime = new float[original.setTime.Length];
        clip.playType = original.playType;
        for (int i = 0; i < clip.checkTime.Length; i++)
        {
            clip.checkTime[i] = original.checkTime[i];
            clip.setTime[i] = original.setTime[i];
        }
        return clip;
    }
}