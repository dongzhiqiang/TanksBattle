using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Shop
{
    public int shopId;
    public int freshNum;
    public long lastRefreshTime;
    public List<Ware> wares;
}

public class Ware
{
    public int wareId;
    public int wareIndex;
    public bool isSold;
}
