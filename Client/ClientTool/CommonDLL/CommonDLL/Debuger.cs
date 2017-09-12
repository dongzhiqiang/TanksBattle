using UnityEngine;
using System;
using System.Collections.Generic;


public class Debuger
{
    public static void Log(string format)
    {
        Debug.Log(format);
    }

    public static void Log(string format, params object[] ps)
    {
        Debug.Log(string.Format(format, ps));
    }

    public static void LogError(string format)
    {
        Debug.LogError(format);
    }

    public static void LogError(string format, params object[] ps)
    {
        Debug.LogError(string.Format(format, ps));
    }
}