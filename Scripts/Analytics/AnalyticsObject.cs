using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using SimpleJSON;

public enum DataType {JSON, Binary };

/// <summary>
/// PG07 Lucas Goes 
/// </summary>

[System.Serializable]
public class AnalyticsObject  {

    const string fileName = "LG-Analytics";

    public string levelName = "";

    public int gameId = -1;

    public Data maxValues = new Data();
    public Data minValues = new Data();

    public Data[][] dataMap;

    public int SizeX, SizeZ;


    public AnalyticsObject(int SizeX,int SizeZ)
    {
        this.SizeX = SizeX;
        this.SizeZ = SizeZ;

        dataMap = new Data[SizeX][];
        for(int i = 0; i <  SizeX; i++)
        {
            dataMap[i] = new Data[SizeZ];
            for (int j = 0; j < SizeZ; j++)
            {
                dataMap[i][j] = new Data();
            }
        }
    }

    public void SetData(Data _data, int x, int z)
    {
        dataMap[x][z] += _data;

        CheckData(dataMap[x][z]);
    }


    /// <summary>
    /// Check the Greater and smaller values to make latter make the normalization
    /// </summary>
    /// <param name="_data"></param>
    void CheckData(Data _data)
    {
        System.Func<float, float, float> checkGreater = (x, y) => (x>y?x:y);

        maxValues.dataSet = checkGreater(maxValues.dataSet, _data.dataSet);
        maxValues.dashUsage = checkGreater(maxValues.dashUsage, _data.dashUsage);
        maxValues.pulseUsage = checkGreater(maxValues.pulseUsage, _data.pulseUsage);
        maxValues.dopplerUsage = checkGreater(maxValues.dopplerUsage, _data.dopplerUsage);

        System.Func<float, float, float> checkSmaller = (x, y) => (y == 0 ? x : x < y && x >0? x:y);

        minValues.dataSet = checkSmaller(minValues.dataSet, _data.dataSet);
        minValues.dashUsage = checkSmaller(minValues.dashUsage, _data.dashUsage);
        minValues.pulseUsage = checkSmaller(minValues.pulseUsage, _data.pulseUsage);
        minValues.dopplerUsage = checkSmaller(minValues.dopplerUsage, _data.dopplerUsage);

    }


    int GenerateGameID()
    {
        return gameId = 0;
    }


    public void DrawData(DataDraw data, Gradient gradient, Transform transform, int scaleIndex,float scaleHeight = 1,float gridGran = 1)
    {
        if(Application.isPlaying)
        {
            for(int i = 0; i < SizeX; i++)
            {
                for (int j = 0; j < SizeZ; j++)
                {
                    Vector3 drawPos = (transform.position + new Vector3((i * gridGran),0 ,(j * gridGran)));
                    dataMap[i][j].DataGizmos(data, gradient, minValues, maxValues, drawPos, scaleHeight, gridGran,scaleIndex);
                }
            }
        }
    }

    public static int GetGreaterGameID(DataType type)
    {
        if(type == DataType.Binary)
        {
            string path = Application.persistentDataPath + "*.data";
            string[] filePaths = Directory.GetFiles(Application.persistentDataPath, "*.data",
                                     SearchOption.AllDirectories);
            return GetGreater(type, filePaths);

        }
        else if (type == DataType.JSON)
        {
            string path = Application.persistentDataPath + "*.json";
            string[] filePaths = Directory.GetFiles(Application.persistentDataPath, "*.json",
                                     SearchOption.AllDirectories);
            return GetGreater(type, filePaths);
        }

        Debug.LogError("Directory empty or files corrupted");

        return 0;
    }

    private static int GetGreater(DataType type, string[] filePaths)
    {
        int max = -1;
        foreach (string filepath in filePaths)
        {
            int temp = LoadData(type, filepath).gameId;
            if (temp > max)
            {
                max = temp;
            }
        }
        return max;
    }

    #region Save Data
    public void SaveData(DataType type)
    {
        switch(type)
        {
            case DataType.JSON:
                SaveJson();
                break;
            case DataType.Binary:
                SaveBinary();
                break;
            default:
                Debug.LogError("The data type provided don't have a function.");
                break;
        }
    }

    void SaveJson()
    {
        string path = Application.persistentDataPath + "/" + fileName + GenerateGameID() + ".json";
        StreamWriter file = new StreamWriter(path);

        JSONNode jsonObject = new JSONClass();

        jsonObject["levelName"] = new JSONData(this.levelName);
        jsonObject["gameId"] = new JSONData(this.gameId);
        jsonObject["minValues"] = this.minValues.ToJson();
        jsonObject["maxValues"] = this.maxValues.ToJson();
        JSONArray jsonData = new JSONArray();
        for (int i = 0; i < SizeX; i++)
        {
            for (int j = 0; j < SizeZ; j++)
            {
                jsonData[i][j] = dataMap[i][j].ToJson();
            }
        }
        jsonObject["dataMap"] = jsonData;

        jsonObject["SizeX"] = new JSONData(this.SizeX);
        jsonObject["SizeZ"] = new JSONData(this.SizeZ);



        file.Write(jsonObject.ToString());


        file.Close();
    }

    void SaveBinary()
    {
        FileStream file = new FileStream(Application.persistentDataPath + "/" + fileName + GenerateGameID()  + ".data", FileMode.Create, FileAccess.Write, FileShare.Write);
        BinaryFormatter binary = new BinaryFormatter();
        binary.Serialize(file, this);
        file.Close();
    }
    #endregion

    #region Load Data
    public static AnalyticsObject LoadData(DataType type, int gameID)
    {
        switch (type)
        {
            case DataType.JSON:
                return LoadJson(gameID);
             
            case DataType.Binary:
                return LoadBinary(gameID);
               
            default:
                Debug.LogError("The data type provided don't have a function.");
                return null;
        }
    }
    public static AnalyticsObject LoadData(DataType type, string filePath)
    {
        switch (type)
        {
            case DataType.JSON:
                return LoadJson(filePath);

            case DataType.Binary:
                return LoadBinary(filePath);

            default:
                Debug.LogError("The data type provided don't have a function.");
                return null;
        }
    }

    static AnalyticsObject LoadJson(int id)
    {
        AnalyticsObject ao = null;
        if (File.Exists(Application.persistentDataPath + "/" + fileName + id + ".json"))
        {
            StreamReader file = new StreamReader(Application.persistentDataPath + "/" + fileName + id + ".json");
            string dataFile = file.ReadToEnd();
            file.Close();
            JSONNode jsonObject = JSONNode.Parse(dataFile);
            int X = int.Parse(jsonObject["SizeX"].Value);
            int Z = int.Parse(jsonObject["SizeZ"].Value);
            ao = new AnalyticsObject(X, Z);

            
            ao.levelName = jsonObject["levelName"].Value;

            ao.gameId = int.Parse(jsonObject["gameId"].Value);
            ao.minValues = new Data();
            ao.minValues.FromJson(jsonObject["minValues"]);
            ao.maxValues = new Data();
            ao.maxValues.FromJson(jsonObject["maxValues"]);
            ao.dataMap = new Data[X][];
            for (int i = 0; i < X; i++)
            {
                ao.dataMap[i] = new Data[Z];
                for (int j = 0; j < Z; j++)
                {
                    ao.dataMap[i][j] = new Data();
                    ao.dataMap[i][j].FromJson(jsonObject["dataMap"][i][j]);
                }
            }
            
        }
        
        return ao;
    }
    static AnalyticsObject LoadJson(string path)
    {
        AnalyticsObject ao = null;
        if (File.Exists(path))
        {
            StreamReader file = new StreamReader(path);
            string dataFile = file.ReadToEnd();
            file.Close();
            JSONNode jsonObject = JSONNode.Parse(dataFile);
            int X = int.Parse(jsonObject["SizeX"].Value);
            int Z = int.Parse(jsonObject["SizeZ"].Value);
            ao = new AnalyticsObject(X, Z);


            ao.levelName = jsonObject["levelName"].Value;

            ao.gameId = int.Parse(jsonObject["gameId"].Value);
            ao.minValues = new Data();
            ao.minValues.FromJson(jsonObject["minValues"]);
            ao.maxValues = new Data();
            ao.maxValues.FromJson(jsonObject["maxValues"]);
            ao.dataMap = new Data[X][];
            for (int i = 0; i < X; i++)
            {
                ao.dataMap[i] = new Data[Z];
                for (int j = 0; j < Z; j++)
                {
                    ao.dataMap[i][j] = new Data();
                    ao.dataMap[i][j].FromJson(jsonObject["dataMap"][i][j]);
                }
            }
        }

        return ao;
    }

    static AnalyticsObject LoadBinary(int id)
    {
        AnalyticsObject ao = null;
        if (File.Exists(Application.persistentDataPath + "/" + fileName + id + ".data"))
        {
            FileStream file = new FileStream(Application.persistentDataPath + "/" + fileName + id + ".data", FileMode.Open, FileAccess.Read, FileShare.Read);
            BinaryFormatter binary = new BinaryFormatter();
            ao = binary.Deserialize(file) as AnalyticsObject;
            file.Close();
        }
        return ao;
        
    }

    static AnalyticsObject LoadBinary(string path)
    {
        AnalyticsObject ao = null;
        if (File.Exists(path))
        {
            FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            BinaryFormatter binary = new BinaryFormatter();
            ao = binary.Deserialize(file) as AnalyticsObject;
            file.Close();
        }
        return ao;

    }

    #endregion

}
