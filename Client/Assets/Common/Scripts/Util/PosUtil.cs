using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * *********************************************************
 * 名称：点获取器
 
 * 日期：2015.8.24
 * 描述：外部只需要获取点就可以了
 *       不需要关心点是变化的还是固定的
 * *********************************************************
 */

//有效轴
public enum enValidAxis
{
    
    horizontal,//水平面
    vertical,//只计算高度
    max
}

public abstract class Pos : IdType
{
    public enum enType
    {
        pos,//固定点
        tran,//游戏对象
        func,//函数
        //closestEnemy,最近的敌人
        //localTran，本地位置
        //relative,相对位置
        max
    }

    public abstract enType GetType();

    //是不是有效，有时候是无效的，比如敌人被杀死
    public abstract bool IsValid();

    public abstract Vector3 Get();
    public virtual Vector3 Get(enValidAxis va, Vector3 refPos)
    {
        return PosUtil.CalcValidAxis(va, Get(), refPos);
    }


}

public class PosPos : Pos
{
    Vector3 m_pos;
    
    public override enType GetType() { return  enType.pos;}

    //是不是有效，有时候是无效的，比如敌人被杀死
    public override bool IsValid() { return true; }

    public override Vector3 Get() { return m_pos; }

    public void SetPos(Vector3 pos) { m_pos = pos; }
}


public class PosTran : Pos
{
    Transform m_tran;

    
    public override enType GetType() { return enType.tran; }

    //是不是有效，有时候是无效的，比如敌人被杀死
    public override bool IsValid() { return m_tran != null; }

    public override Vector3 Get()
    {
        if (m_tran == null)
        {
            Debuger.LogError("从空的transform获取位置");
            return Vector3.zero;
        }
            
        return m_tran.position; 
    }

    public void SetTran(Transform t) { m_tran = t; }
}

public class PosFunc: Pos
{
    System.Func<Vector3> m_fun;

    
    public override enType GetType() { return enType.func; }

    //是不是有效，有时候是无效的，
    public override bool IsValid() { return m_fun != null; }

    public override Vector3 Get()
    {
        if (m_fun == null)
        {
            Debuger.LogError("空的回调函数");
            return Vector3.zero;
        }

        return m_fun();
    }

    public void SetFunc(System.Func<Vector3> f)
    {
        m_fun = f;
    }
}

public class PosUtil
{
    public static Vector3 CalcValidAxisRef(enValidAxis va, Vector3 p)
    {
        if (va == enValidAxis.horizontal)
        {
            Vector3 pos2d = p;
            pos2d.y = 0;
            return pos2d;
        }
        else if (va == enValidAxis.vertical)
        {
            return new Vector3(0, p.y, 0);
        }
        else
        {
            Debuger.LogError("未知的位置类型:" + va);
            return p;
        }
    }

    public static Vector3 CalcValidAxis(enValidAxis va, Vector3 p, Vector3 refPos)
    {
        if (va == enValidAxis.horizontal)
        {
            Vector3 pos2d = p;
            pos2d.y = refPos.y;
            return pos2d;
        }
        else if (va == enValidAxis.vertical)
        {
            return new Vector3(refPos.x, p.y, refPos.z);
        }
        else
        {
            Debuger.LogError("未知的位置类型:" + va);
            return p;
        }
    }

    const string Terrain_Name = "[TerrainObstruct]";

    //计算出贴地的位置
    public static Vector3 CaleByTerrains(Vector3 pos)
    {

        Ray ray = new Ray(new Vector3(pos.x, pos.y + 100, pos.z), Vector3.down);
        RaycastHit[] rhs = Physics.RaycastAll(ray, 1000);//, LayerMask.GetMask("Default") 注意这里用Default是无效的，正确的写法应该是 -1异或不想被cast的层
        float height = float.MinValue;
        for (int i = 0; i < rhs.Length; i++)
        {
            RaycastHit rh = rhs[i];
            if (rh.transform.name == Terrain_Name ||
                (rh.transform.parent != null && rh.transform.parent.name == Terrain_Name) ||
                (rh.transform.parent != null && rh.transform.parent.parent != null && rh.transform.parent.parent.name == Terrain_Name))
            {
                if (rh.point.y > height)
                    height = rh.point.y;
            }
        }

        if(height != float.MinValue)
        {
            return new Vector3(pos.x, height, pos.z);
        }

        Debuger.LogError("场景:{0} CaleByTerrains获取不到地表:{1}", Application.loadedLevelName, pos);
        return pos;
    }

    //计算出相机看到的地板
    public static Vector3 CaleByCamera(Camera ca)
    {
        
        RaycastHit[] rhs = Physics.RaycastAll(ca.transform.position, ca.transform.forward, 1000);   
        for (int i = 0; i < rhs.Length; i++)
        {
            RaycastHit rh = rhs[i];
            if (rh.transform.name.StartsWith("[Terrain"))
            {
                return rh.point;
            }
        }
        Debuger.LogError("相机找不到地板");

        return Vector3.zero;
    }
}