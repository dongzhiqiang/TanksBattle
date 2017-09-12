using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;




public class QualityCfg
{
    public int id;
    public string name;
    public string color;
    public string backgroundSquare;
    public string backgroundRound;

    public static Dictionary<int, QualityCfg> m_cfgs = new Dictionary<int, QualityCfg>();

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, QualityCfg>("other/quality", "id");

    }

    public static Color GetColor(int quality)
    {
        QualityCfg qualityCfg = m_cfgs[quality];
        return ToColor(qualityCfg.color);
    }

    public static Color ToColor(string colorName)
    {
        if (colorName.StartsWith("#"))
            colorName = colorName.Replace("#", string.Empty);
        var v = int.Parse(colorName, System.Globalization.NumberStyles.HexNumber);
        return new Color
        (
            Convert.ToByte((v >> 16) & 255) / (float)255,
            Convert.ToByte((v >> 8) & 255) / (float)255,
            Convert.ToByte((v >> 0) & 255) / (float)255
        );
    }  
}