using UnityEngine;
using System;
using System.Xml;
using System.IO;

public class EffectData : BaseData
{
    private const string EFFECT = "effect";
    private const string CLIP = "clip";

    public EffectClip[] effectClips = new EffectClip[0];

    public EffectData()
    {
        xmlFilePath = "";
        xmlFileName = "effectData.xml";
        dataPath = "Data/effectData";
        clipPath = "Effects/";
    }

    public override void SaveData()
    {
        using (XmlTextWriter xml = new XmlTextWriter(xmlFilePath + xmlFileName, System.Text.Encoding.Unicode))
        {
            xml.WriteStartDocument();
            xml.WriteStartElement(EFFECT);
            xml.WriteElementString("length", GetDataCount().ToString());
            for (int i = 0; i < this.names.Length; i++)
            {
                EffectClip clip = effectClips[i];
                xml.WriteStartElement(CLIP);
                xml.WriteElementString("id", i.ToString());
                xml.WriteElementString("name", names[i]);
                xml.WriteElementString("effectType", clip.effectType.ToString());
                xml.WriteElementString("effectPath", clip.clipPath);
                xml.WriteElementString("effectName", clip.clipName);
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
            this.AddData("New Effect");
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
                            this.names = new string[length];
                            this.effectClips = new EffectClip[length];
                            break;
                        case "id":
                            currentID = int.Parse(reader.ReadString());
                            this.effectClips[currentID] = new EffectClip();
                            this.effectClips[currentID].realID = currentID;
                            break;
                        case "name":
                            this.names[currentID] = reader.ReadString();
                            break;
                        case "effectType":
                            this.effectClips[currentID].effectType = (EffectType)Enum.Parse(typeof(EffectType), reader.ReadString());
                            break;
                        case "effectName":
                            this.effectClips[currentID].clipName = reader.ReadString();
                            break;
                        case "effectPath":
                            this.effectClips[currentID].clipPath = reader.ReadString();
                            break;
                    }
                }
            }
        }
    }

    public override int AddData(string newName)
    {
        if (names == null)
        {
            names = new string[] { newName };
            effectClips = new EffectClip[] { new EffectClip() };
        }
        else
        {
            names = ArrayHelper.Add(newName, names);
            effectClips = ArrayHelper.Add(new EffectClip(), effectClips);
        }

        return GetDataCount();
    }

    public override void RemoveData(int index)
    {
        names = ArrayHelper.Remove(index, names);
        if (names.Length == 0)
            names = null;

        effectClips = ArrayHelper.Remove(index, effectClips);
    }

    public override void CopyData(int index)
    {
        names = ArrayHelper.Add(names[index], names);
        //effectClips = ArrayHelper.Add(effectClips[index], effectClips);
        effectClips = ArrayHelper.Add(GetCopyClip(index), effectClips);
    }

    public void ClearClips()
    {
        foreach (EffectClip clip in effectClips)
        {
            clip.Release();
        }
        effectClips = null;
        names = null;
    }

    public EffectClip GetCopyClip(int index)
    {
        if (index < 0 || index >= effectClips.Length)
            return null;

        EffectClip original = effectClips[index];
        EffectClip clip = new EffectClip();
        clip.clipFullPath = original.clipFullPath;
        clip.clipName = original.clipName;
        clip.clipPath = original.clipPath;
        clip.effectType = original.effectType;
        clip.realID = effectClips.Length;
        return clip;
    }

    public EffectClip GetClip(int index)
    {
        if (index < 0 || index >= effectClips.Length)
            return null;

        effectClips[index].PreLoad();
        return effectClips[index];
    }
}