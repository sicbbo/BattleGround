using UnityEngine;

public class BaseData : ScriptableObject
{
    protected string xmlFilePath = "";
    protected string xmlFileName = "effectData.xml";
    protected string dataPath = "Data/effectData";
    protected string clipPath = "Effects/";

    public const string dataDirectory = "/Resources/Data/"; //: 테이블 테이터가 있는 폴더의 주소.
    public string[] names = null;                           //: 데이터 이름 배열.

    public BaseData() { }

    public int GetDataCount()
    {
        int retValue = 0;
        if (this.names != null)
            retValue = this.names.Length;

        return retValue;
    }

    public string[] GetNameList(bool showID, string filterWord = "")
    {
        string[] retList = new string[0];
        if (this.names == null)
            return retList;

        retList = new string[this.names.Length];

        for (int i = 0; i < names.Length; i++)
        {
            if (filterWord != "")
                if (this.names[i].ToLower().Contains(filterWord.ToLower()) == false)
                    continue;

            if (showID == true)
                retList[i] = string.Format("{0} : {1}", i, this.names[i]);
            else
                retList[i] = this.names[i];
        }

        return retList;
    }

    public virtual void SaveData()
    {

    }

    public virtual void LoadData()
    {

    }

    public virtual int AddData(string newName)
    {
        return GetDataCount();
    }
    public virtual void RemoveData(int index)
    {

    }
    public virtual void CopyData(int index)
    {

    }
}