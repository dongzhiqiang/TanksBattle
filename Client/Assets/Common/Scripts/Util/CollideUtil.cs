/*
 * *********************************************************
 * 名称：碰撞计算工具
 
 * 日期：2015.9.11
 * 描述：
 *  一套2d的碰撞检测系统(支持简单判断3d的高度差)
 * *********************************************************
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public enum enRangeType{
    circle,//圆
    sector,//扇形
    rect,//矩形
    collider,//碰撞
}

public class RangeCfg
{
    public static string[] RangeTypeName = new string[] { "圆", "扇形", "矩形","碰撞" };

    public enRangeType type = enRangeType.circle;
    public float beginOffsetAngle= 0;//相对于source前方的偏移角度
    public float endOffsetAngle =0;
    public Vector3 begingOffsetPos = Vector3.zero;//相对于(source前方+offsetEuler)的偏移位置
    public Vector3 endOffsetPos = Vector3.zero;
    public float distance = 5;//水平距离
    public float heightLimit = 2;//垂直距离，-1表示不用判断了
    public float rectLimit = 3;//target到source前方延长线上的距离限制
    public float angleLimit = 90;//夹角限制
    

    //技能编辑器ui相关，也保存到配置表里吧
    public bool showArea=false;
    public bool showRange =false;


    //不计算半径
    bool ingoreRadius = false;
    public bool IngoreRadius { get{return ingoreRadius;}set{ingoreRadius = value;}}

    public void CopyFrom(RangeCfg cfg)
    {
        if(cfg==null)return;

        Util.Copy(cfg, this,BindingFlags.Public | BindingFlags.Instance);
    }
}


public class CollideUtil
{
    public static bool HitHeight(float sourceY, float targetY, float heightLimit)
    {
        return heightLimit == -1 || Mathf.Abs(sourceY - targetY) <= heightLimit;
    }

    //注意外部一定要传进来一个默认的碰撞点collidePos(比如target)，内部不一定会修改这个值
    public static bool Hit(Vector3 source, Vector3 dir, Vector3 target, float radius, RangeCfg cfg,float factor=0f)
    {
        float  offsetAngle;
        Vector3 offsetPos;
        if (factor == 0)
        {
            offsetAngle = cfg.beginOffsetAngle;
            offsetPos = cfg.begingOffsetPos;
        }
        else if (factor == 1f)
        {
            offsetAngle = cfg.endOffsetAngle;
            offsetPos = cfg.endOffsetPos;
        }
        else
        {
            offsetAngle = Mathf.Lerp(cfg.beginOffsetAngle, cfg.endOffsetAngle, factor);
            offsetPos = Vector3.Lerp(cfg.begingOffsetPos,cfg.endOffsetPos,factor);
        }

        //计算偏移后的点
        dir.y = 0;//y方向无效
        if (offsetAngle != 0)
            dir = Quaternion.Euler(0, offsetAngle,0) * dir;
        if (dir == Vector3.zero)//没有方向？？？
        {
            Debuger.LogError("没有方向");
            return false;
        }
        dir.y = 0;
        if (offsetPos != Vector3.zero)
            source += Quaternion.LookRotation(dir) * offsetPos;

        if (!HitHeight(source.y, target.y, cfg.heightLimit)) return false;

        //开始2d相关计算
        if (cfg.IngoreRadius)//有时候不希望计算半径
            radius = 0;
        target.y = source.y;
        Vector3 link = target - source;
        Vector2 source2d = new Vector2(source.x, source.z);
        Vector2 target2d = new Vector2(target.x, target.z);
        Vector2 link2d = new Vector2(link.x, link.z);
        Vector2 dir2d = new Vector2(dir.x, dir.z);
        float disSq = link2d.sqrMagnitude;
        float radSq = radius * radius;


        //如果source在target圆上那么就已经碰撞了
        if (disSq <= radSq)
            return true;

        //不同类型的碰撞检测
        if (cfg.type == enRangeType.circle )
        {
            //半径
            if (disSq >= cfg.distance * cfg.distance + radSq + 2 * cfg.distance * radius)//a>(b+c) => a^2>(b+c)^2
                return false;

            //计算碰撞的点
            //if (needCollidePos)
            //{
            //    Vector2 collide2d = source2d + link2d - link2d.normalized * radius;
            //    collidePos.Set(collide2d.x, collidePos.y, collide2d.y);
            //}
            return true;
        }
        else if (cfg.type == enRangeType.sector)
        {
            //半径
            if (disSq >= cfg.distance * cfg.distance + radius * radius + 2 * cfg.distance * radius)//a>(b+c) => a^2>(b+c)^2
                return false;

            //夹角
            float angle = Vector2.Angle(dir2d, link2d);
            if (angle <= cfg.angleLimit)//圆心在扇形内的情况，那么交点和圆一样计算
            {
                ////计算碰撞的点
                //if (needCollidePos)
                //{
                //    Vector2 collide2d = source2d + link2d - link2d.normalized * radius;
                //    collidePos.Set(collide2d.x, collidePos.y, collide2d.y);
                //}
                return true;
            }
            else if(radius==0)//优化下，如果为0，则判断下扇形内就足够了
            {
                return false;
            }
            else//圆心在扇形外的情况
            {
                //用相对于source方向同一边的扇形的边缘做碰撞检测
                Vector3 dirAngle;
                if (Vector3.Cross(dir, link).y > 0)
                    dirAngle = Quaternion.Euler(new Vector3(0, cfg.angleLimit, 0)) * dir;
                else
                    dirAngle = Quaternion.Euler(new Vector3(0, -cfg.angleLimit, 0)) * dir;
                if (dirAngle.y != 0)//检错下
                    Debuger.LogError("计算出来的方向不为2d的");
                //Debug.DrawRay(source + Vector3.up*1, dirAngle*20, Color.yellow, 3);

                //计算有没有交点
                Vector2 dirAngle2d = new Vector2(dirAngle.x, dirAngle.z);
                Vector2 project2d = Util.Project(link2d, dirAngle2d);//投影在方向上的点(相对于source)
                Vector2 verticalDir2d = link2d - project2d;//垂线，投影在角色方向上的点到target(相对于source)
                //Vector3 project = new Vector3(project2d.x,source.y ,project2d.y);
                //Debug.DrawRay(source + project + Vector3.up * 1, new Vector3(verticalDir2d.x, 0, verticalDir2d.y) , Color.red, 3);
                float vDirSq = verticalDir2d.sqrMagnitude;
                if (vDirSq > radSq)//投影点在圆外,那么碰撞不到
                    return false;
                if (vDirSq == radSq)//投影点在圆上
                {
                    if (angle < 90 && Vector2.Dot(project2d, dirAngle2d) < 0)//如果要求夹角是小于90的那么要判断方向是不是在主角前方
                        return false;
                    //if (needCollidePos)
                    //{
                    //    Vector2 collide2d = source2d + project2d;
                    //    collidePos.Set(collide2d.x, collidePos.y, collide2d.y);
                    //}
                    return true;
                }
                else //投影点在圆内
                {
                    if (angle < 90 && Vector2.Dot(project2d, dirAngle2d) < 0)//如果要求夹角是小于90的那么要判断方向是不是在主角前方
                        return false;

                    //if (needCollidePos)
                    //{
                    //    float d = Mathf.Sqrt(radSq - vDirSq);//交点到投影点的距离，勾股定理求第三边,这里按照之前的判断是肯定能成直角三角形的
                    //    Vector2 collide2d = source2d + project2d - project2d.normalized * d;
                    //    collidePos.Set(collide2d.x, collidePos.y, collide2d.y);
                    //}
                    return true;
                }
            }
        }
        else if (cfg.type == enRangeType.rect)
        {
            //先计算出到角色方向上的投影点
            Vector2 project2d = Util.Project(link2d, dir2d);//投影在方向上的点(相对于source)
            Vector2 verticalDir2d = link2d - project2d;//垂线，投影在角色方向上的点到target(相对于source)
            float vDirSq = verticalDir2d.sqrMagnitude;
            Vector3 project = new Vector3(project2d.x, source.y, project2d.y);
            Vector3 vertical = new Vector3(verticalDir2d.x, 0, verticalDir2d.y);
            //Debug.DrawRay(source +project+ Vector3.up * 1, vertical, Color.yellow, 3);

            //宽度限制
            if (vDirSq >= cfg.rectLimit * cfg.rectLimit + radSq + 2 * cfg.rectLimit * radius)//a>(b+c) => a^2>(b+c)^2
                return false;

            //长度限制
            float halfDis = cfg.distance / 2;
            //Vector2 center = source2d + dir2d.normalized*cfg.distance/2;//中心点，矩形的中心点
            //Vector2 linkCenter2d = target2d-source2d;            
            float hDir = Vector2.Dot(dir2d, project2d) < 0 ? project2d.magnitude + halfDis : Mathf.Abs(project2d.magnitude - halfDis);//算出投影点到中心点的距离
            float hDirSq = hDir * hDir;
            if (hDirSq >= halfDis * halfDis + radSq + 2 * halfDis * radius)//a>(b+c) => a^2>(b+c)^2
                return false;

            //计算碰撞的点
            //if (needCollidePos)
            //{
            //    Vector2 collide2d = source2d + link2d - link2d.normalized * radius;
            //    collidePos.Set(collide2d.x, collidePos.y, collide2d.y);
            //}
            return true;
        }
        else
        {
            Debuger.LogError("不能用ColliderUtil系统检测的类型:{0}", cfg.type);
        }

        return false;
    }

    //给定圆外一点和一个方向，返回值表示有没有相交
    //projectPos为投影点
    //如果点在圆内collidePos为圆心，返回0
    //如果与圆相交collidePos为离pos较近的交点，返回0
    //如果圆不相交collidePos为离射线最近的点，disDirSq为该到射线上的距离的平方
    //isFront表示是不是collidePos是不是在pos的正面
    public static bool HitByLineAndCircle(Vector2 pos, Vector2 dir, Vector2 center, float radius,out Vector2 projectPos, out Vector2 collidePos,  out bool isFront)
    {
        
        if (dir == Vector2.zero)//没有方向？？？
        {
            Debuger.LogError("没有方向");
            collidePos = pos;
            projectPos = center;
            isFront = false;
            return false;
        }
        Vector2 link = center - pos;
        float disSq = link.sqrMagnitude;
        //如果source的target圆上那么就已经碰撞了
        if (disSq <= radius * radius)
        {
            collidePos = center;
            projectPos = center;
            isFront = true;
            return true;
        }


        projectPos = Util.Project(link, dir);//投影在方向上的点(相对于source)
        Vector2 verticalDir = link - projectPos;//投影在角色方向上的点到target(相对于source)
        float disP2Sq = verticalDir.sqrMagnitude;
        float disRadSq = radius * radius;        
        bool isHit;
        if (disP2Sq == disRadSq)//投影点在圆上
        {
            collidePos = pos + projectPos;
            isHit = true;
        }
        if (disP2Sq < disRadSq)//投影点在圆内
        {
            float d = disP2Sq == 0 ? radius : Mathf.Sqrt(disRadSq - disP2Sq);//交点到投影点的距离，勾股定理求第三边
            collidePos = pos + projectPos - projectPos.normalized * d;
            isHit = true;
        }
        else//投影点在圆外
        {
            collidePos = pos + projectPos  + verticalDir - verticalDir.normalized * radius;
            isHit = false;
        }

        isFront = Vector2.Dot(dir, collidePos - pos) >= 0;//交点是不是在正面
        return isHit;        
    }

  

    static Transform ball;
    static Transform project;
    static Transform line;
    public static void TestProject(){
        if (ball == null)
            {
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                ball = go.transform;

                go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                line =go.transform;
                line.localScale  = new Vector3(0.1f,0.1f,50);

                go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                project = go.transform;
            }

            Vector2 p = Util.Project(new Vector2(ball.position.x,  ball.position.z), new Vector2(line.forward.x,  line.forward.z));
            project.position = new Vector3(p.x,0,p.y);
    }
    public static void Draw(RangeCfg cfg,DrawGL draw, Vector3 pos, Vector3 dir,Color clr,float factor = 0f)
    {
        if (!cfg.showRange )
            return;

        //碰撞类型的话画调线
        if(cfg.type == enRangeType.collider)
        {
            if (cfg.heightLimit <= 0)
                return;
           
            draw.DrawLine(clr * 0.7f, pos+Vector3.up* cfg.heightLimit, pos - Vector3.up * cfg.heightLimit);
            return;
        }

        //下面是其他类型

        //计算偏移后的点
        float offsetAngle;
        Vector3 offsetPos;
        if (factor == 0)
        {
            offsetAngle = cfg.beginOffsetAngle;
            offsetPos = cfg.begingOffsetPos;
        }
        else if (factor == 1f)
        {
            offsetAngle = cfg.endOffsetAngle;
            offsetPos = cfg.endOffsetPos;
        }
        else
        {
            offsetAngle = Mathf.Lerp(cfg.beginOffsetAngle, cfg.endOffsetAngle, factor);
            offsetPos = Vector3.Lerp(cfg.begingOffsetPos, cfg.endOffsetPos, factor);
        }

        //计算偏移后的点
        dir.y = 0;//y方向无效
        if (offsetAngle != 0)
            dir = Quaternion.Euler(0, offsetAngle, 0) * dir;
        if (dir == Vector3.zero)//没有方向？？？
        {
            return;
        }
        dir.y = 0;
        if (offsetPos != Vector3.zero)
            pos += Quaternion.LookRotation(dir) * offsetPos;

        float d2 = cfg.distance*2;
        if (cfg.type == enRangeType.circle)
        {
            Vector3 scale = new Vector3(d2, cfg.heightLimit <= 0 ? 3 : cfg.heightLimit * 2, d2);
            draw.DrawCylinder(clr * 0.7f, pos, scale, Quaternion.LookRotation(dir));
        }
        else if (cfg.type == enRangeType.sector)
        {
            Vector3 scale = new Vector3(d2, cfg.heightLimit <= 0 ? 3 : cfg.heightLimit * 2, d2);
            draw.DrawSectorCylinder(clr * 0.7f, cfg.angleLimit, pos, scale, Quaternion.LookRotation(dir));
        }
        else if (cfg.type == enRangeType.rect)
        {
            pos += dir.normalized*cfg.distance/2;
            Vector3 scale = new Vector3(cfg.rectLimit*2 , cfg.heightLimit <= 0 ? 3 : cfg.heightLimit * 2, cfg.distance);
            draw.DrawBox(clr*0.7f, pos, scale, Quaternion.LookRotation(dir));
        }

    }


}
