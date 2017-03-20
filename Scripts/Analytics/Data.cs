using UnityEngine;
using System.Collections;
using SimpleJSON;

public enum DataDraw { Position = 0, Pulse = 1, Dash = 2, Doppler =3 };

/// <summary>
/// PG07 Lucas Goes 
/// </summary>

[System.Serializable]
public class Data
{

    /// <summary>
    /// How long the player stays in the position
    /// </summary>
    public float dataSet = 0;

    /// <summary>
    /// Position of the player
    /// </summary>
    public Vector3 pos = new Vector3(0, 0, 0);

    /// <summary>
    /// How many times the player usege the pulse ability
    /// </summary>
    public float pulseUsage = 0;

    /// <summary>
    /// How many times the player use the dash ability
    /// </summary>
    public float dashUsage = 0;

    /// <summary>
    /// How many times the player usage the doppler ability
    /// </summary>
    public float dopplerUsage = 0;


    public JSONClass ToJson()
    {
        JSONClass json = new JSONClass();

        json["dataSet"] = new JSONData(this.dataSet);
        json["pos"] = new JSONClass();

        json["pos"]["x"] = new JSONData(this.pos.x);
        json["pos"]["y"] = new JSONData(this.pos.y);
        json["pos"]["z"] = new JSONData(this.pos.z);

        json["pulseUsage"] = new JSONData(this.pulseUsage);
        json["dashUsage"] = new JSONData(this.dashUsage);
        json["dopplerUsage"] = new JSONData(this.dopplerUsage);

        return json;
    }

    public void FromJson(JSONNode json)
    {
        this.dataSet = float.Parse(json["dataSet"].Value);
        this.pos = new Vector3(float.Parse(json["pos"]["x"].Value), float.Parse(json["pos"]["y"].Value), float.Parse(json["pos"]["z"].Value));
        this.pulseUsage = float.Parse(json["pulseUsage"].Value);
        this.dashUsage = float.Parse(json["dashUsage"].Value);
        this.dopplerUsage = float.Parse(json["dopplerUsage"].Value);
    }

    /// <summary>
    /// Empty constructor
    /// </summary>
    public Data()
    {

    }

    /// <summary>
    /// Setup pos
    /// </summary>
    /// <param name="dataSet">How long the player stays in the pos</param>
    /// <param name="pos">Pos in the array</param>
    public Data(float dataSet, Vector2 pos)
    {
        this.dataSet = dataSet;
        this.pos = pos;
    }

    /// <summary>
    /// Setup abilities
    /// </summary>
    public Data(float pulseUsage, float dashUsage, float dopplerUsage)
    {
        this.pulseUsage = pulseUsage;
        this.dashUsage = dashUsage;
        this.dopplerUsage = dopplerUsage;
    }

    /// <summary>
    /// Full Setup
    /// </summary>
    public Data(float dataSet, Vector2 pos, float pulseUsage, float dashUsage, float dopplerUsage)
    {
        this.dataSet = dataSet;
        this.pos = pos;
        this.pulseUsage = pulseUsage;
        this.dashUsage = dashUsage;
        this.dopplerUsage = dopplerUsage;
    }


    public static Data operator +(Data oldData, Data newData)
    {

        float _dataset = oldData.dataSet + newData.dataSet;

        float _pulse = oldData.pulseUsage + newData.pulseUsage;

        float _dash = oldData.dashUsage + newData.dashUsage;

        float _doppler = oldData.dopplerUsage + newData.dopplerUsage;

        return new Data(_dataset, oldData.pos, _pulse, _dash, _doppler);
    }



    /// <summary>
    /// Draw heatmap of the position
    /// </summary>
    /// <param name="draw">Which data you want to draw</param>
    /// <param name="gradient">gradient color</param>
    /// <param name="minMax">Min and max value from the Data array</param>
    /// <param name="drawPos">Where the data should be drawing</param>
    /// <param name="scaleHeight">Max Height of the cube</param>
    /// <param name="gridGran">Scale of the grid</param>
    /// <param name="scaleIndex">Normalized is 0 and Log is 1</param>
    public void DataGizmos(DataDraw draw, Gradient gradient, Data minValues, Data maxValues, Vector3 drawPos, float scaleHeight, float gridGran, int scaleIndex)
    {

        float normalize = 0;
        switch (draw)
        {
            case DataDraw.Position:
                if (dataSet == 0) return;
                
                normalize = Scale(minValues.dataSet, maxValues.dataSet, dataSet,scaleIndex);
                break;
            case DataDraw.Dash:
                if (dashUsage == 0) return;
                normalize = Scale(minValues.dashUsage, maxValues.dashUsage, dashUsage, scaleIndex);
                break;
            case DataDraw.Doppler:
                if (dopplerUsage == 0) return;
                normalize = Scale(minValues.dopplerUsage, maxValues.dopplerUsage, dopplerUsage, scaleIndex);
                break;
            case DataDraw.Pulse:
                if (pulseUsage == 0) return;
                normalize = Scale(minValues.pulseUsage, maxValues.pulseUsage, pulseUsage, scaleIndex);
                break;
        }
        float cubeheight = normalize * scaleHeight;
        Gizmos.color = gradient.Evaluate(normalize);
        Gizmos.DrawCube(drawPos, new Vector3(gridGran, cubeheight, gridGran));

    }

    float Scale(float min, float max, float val, int scaleIndex)
    {
        switch(scaleIndex)
        {
            case 0:
                return Mathf.InverseLerp(min, max, val);
            case 1:
                return Mathf.Log10(val);
            default:
                return -1;
        }
    }

    /// <summary>
    /// Normalize the data
    /// </summary>
    /// <param name="min">Minimum Value</param>
    /// <param name="max">Maximum Value</param>
    /// <param name="value">Actual Value</param>
    /// <returns>Normalized Value</returns>
    float Normalized(float min, float max, float value)
    {
        return (value - min) / (max - min);
    }
}
