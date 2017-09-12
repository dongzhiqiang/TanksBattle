using System;
using System.Collections;
using System.Collections.Generic;

public class XMLAttributes
{
    public XMLAttributes()
    {

    }

    ~XMLAttributes()
    {

    }

    public void add(string attrName, string attrValue)
    {
        d_attrs[attrName] = attrValue;
    }

    public void remove(ref string attrName)
    {
        d_attrs.Remove(attrName);
    }

    public bool exists(string attrName)
    {
        return d_attrs.ContainsKey(attrName);
    }

    public int getCount()
    {
        return d_attrs.Count;
    }

    public string getValue(string attrName)
    {
        string name = "";
        d_attrs.TryGetValue(attrName, out name);
        return name;
    }

    public string getValueAsString(string attrName)
    {
        string name = "";
        if (d_attrs.TryGetValue(attrName, out name) == false)
            return "";

        return name;
    }

    public string getValueAsString(string attrName, string def)
    {
        string name = "";
        if (d_attrs.TryGetValue(attrName, out name) == false)
            return def;

        return name;
    }

    public bool getValueAsBool(string attrName, bool def)
    {
        string name = "";
        if (d_attrs.TryGetValue(attrName, out name) == false)
            return def;

        bool value = def;
        if (bool.TryParse(name, out value) == false)
            return def;

        return value;
    }

    public int getValueAsInteger(string attrName, int def)
    {
        string name = "";
        if (d_attrs.TryGetValue(attrName, out name) == false)
            return def;

        int value = def;
        if (int.TryParse(name, out value) == false)
            return def;

        return value;
    }

    public float getValueAsFloat(string attrName, float def)
    {
        string name = "";
        if (d_attrs.TryGetValue(attrName, out name) == false)
            return def;

        float value = def;
        if (float.TryParse(name, out value) == false)
            return def;

        return value;
    }

    private Dictionary<string, string> d_attrs = new Dictionary<string, string>();
};