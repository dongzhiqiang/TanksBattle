#region Header
/**
 * 名称：测试用
 
 * 日期：201x.xx.xx
 * 描述：新建类的时候建议用这个模板
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NetCore;
namespace TestNetCore
{
    public enum enTestPart
    {
        min,
        property,
        bag,
        task,
        level,
        max,
    }
    public enum enTestProp
    {
        min,
        name,//名字
        exp,//经验
        gold,//金币
        max,
    }

    //物品类
    public class TestGoods : SObject
    {
        //测试用的名字
        public static string[] GoodsName = new string[] { "诸神黄昏", "镇魂曲", "阿瓦隆" };

        public SInt goodsId = new SInt(0);
        public SInt num = new SInt(0);

        public string Name { get{return GoodsName[goodsId];}}
    }

    //任务类
    public class TestTask : SObject
    {
        public enum enState
        {
            doing,//正在做中
            finish,//完成了但是没有领取奖励
            get,//领取奖励了
            
        }
        public static string[] StateName = new string[] { "正在做","已完成","已领取" };
        public SInt taskId = new SInt(0);
        public SByte state = new SByte(0);
    }

    //关卡信息
    public class TestLevelInfo: SObject{
        public SInt levelId = new SInt(0);
        public SInt star = new SInt(0);//通关后的星星数
        public SInt enterCount = new SInt(0);//今天进入了几次
        public SDate lastEnterTime = new SDate();//最后一次打的时间

        public int EnterCount{
            get{
                if(lastEnterTime.Value.Ticks < System.DateTime.Today.Ticks) 
                    return 0;
                else
                    return enterCount;
            }
        }

        public void Add()
        {
            if (lastEnterTime.Value < System.DateTime.Today)
                enterCount.Value =1;
            else
                enterCount.Value +=1;
            lastEnterTime.Value = System.DateTime.Now;
        }
    }

    //部件基类
    public abstract class TestRolePart : SObject
    {
        public abstract enTestPart Type { get; }
        public abstract void Init();
    }

    //属性部件
    public class TestPropertyPart : TestRolePart
    {
        public SIntDictionary<SVariant> m_props = new SIntDictionary<SVariant>();
        public override enTestPart Type { get{return enTestPart.property;} }
        public override void Init()
        {
            m_props[(int)enTestProp.name] = new SVariant("test");
            m_props[(int)enTestProp.exp] = new SVariant(0);
            m_props[(int)enTestProp.gold] = new SVariant(50);
        }

        public SVariant Get(enTestProp prop) { return m_props[(int)prop];}
        public int GetInt(enTestProp prop) { return Get(prop); }
        public long GetLong(enTestProp prop) { return Get(prop); }
        public string GetStr(enTestProp prop) { return Get(prop); }

        public void Set(enTestProp prop, string str) { Get(prop).Value = str; }
        public void Set(enTestProp prop, int value) { Get(prop).Value = value; }
        public void Set(enTestProp prop, long value) { Get(prop).Value = value; }

        public void Add(enTestProp prop, int value) {
            SVariant v = Get(prop);
            v.Value = value + (int)v.Value;
        }
        public void Add(enTestProp prop, long value)
        {
            SVariant v = Get(prop);
            v.Value = value + (long)v.Value;
        }
    }
    
    //背包部件
    public class TestBagPart : TestRolePart
    {
        public SList<TestGoods> m_goods = new SList<TestGoods>();
        public override enTestPart Type { get { return enTestPart.bag; } }
        public override void Init() { }

        public TestGoods Get(int goodsId)
        {
            foreach(TestGoods goods in m_goods){
                if(goods.goodsId == goodsId)
                    return goods;
            }
            return null;
        }
        public void Add(int goodsId,int num)
        {
            TestGoods goods =Get(goodsId);
            if (goods == null)
            {
                goods = new TestGoods();
                goods.goodsId.Value = goodsId;
                m_goods.Add(goods);
            }

            goods.num.Value = goods.num +num;
            
        }

        public void Remove(TestGoods goods)
        {
            m_goods.Remove(goods);
        }
    }

   
    //任务部件
    public class TestTaskPart : TestRolePart
    {
        public SList<TestTask> m_tasks = new SList<TestTask>();
        public override enTestPart Type { get { return enTestPart.task; } }
        public override void Init() { }
    }

    //关卡部件
    public class TestLevelPart : TestRolePart
    {
        public SIntDictionary<TestLevelInfo> m_levelInfos = new SIntDictionary<TestLevelInfo>();
        public override enTestPart Type { get { return enTestPart.level; } }
        public override void Init() { }

        
    }

    //角色类
    public class TestRole : SObject
    {
        #region Fields
        public TestPropertyPart m_propPart = new TestPropertyPart();
        public TestBagPart m_bagPart = new TestBagPart();
        public TestTaskPart m_taskPart = new TestTaskPart();
        public TestLevelPart m_levelPart = new TestLevelPart(); 
        #endregion

        #region Constructors
        
        #endregion

        #region Private Methods
        
        #endregion
        public void Init()
        {
            m_propPart.Init();
            m_bagPart.Init();
            m_taskPart.Init();
            m_levelPart.Init();
        }
        
    }
}