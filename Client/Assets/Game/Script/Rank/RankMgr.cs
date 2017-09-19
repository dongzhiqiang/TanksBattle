using LitJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class RankMgr : Singleton<RankMgr>
{
    public const string RANK_TYPE_GOLD_LEVEL = "goldLevel";
    public const string RANK_TYPE_ARENA = "arena";
    public const string RANK_TYPE_CORPS = "corps";
    public const string RANK_TYPE_FULL_POWER = "fullPower";
    public const string RANK_TYPE_REAL_POWER = "realPower";
    public const string RANK_TYPE_LEVEL_STAR = "levelStar";
    public const string RANK_TYPE_PREDICTOR = "predictor"; 

    //List<object>的object其实是RankItemWrapper，为了避免在使用UIScrollWrap时List<RankItemWrapper>转List<object>，就直接用object为元素类型
    //RankItemWrapper的rank，第一名值是0
    private Dictionary<string, List<object>> m_rankItems = new Dictionary<string, List<object>>();
    private Dictionary<string, object> m_myRankItem = new Dictionary<string, object>();
    private Dictionary<string, long> m_rankTimes = new Dictionary<string,long>();
    private Dictionary<string, int> m_myRankVals = new Dictionary<string,int>();

    public void Init()
    {
        UIMainCity.AddClick(enSystem.rank, () => { UIMgr.instance.Open<UIRank>(); });
    }

    public long GetRankDataUpTime(string type)
    {
        long val;
        if (m_rankTimes.TryGetValue(type, out val))
            return val;
        return 0;
    }

    public int GetRankDataItemCount(string type)
    {
        List<object> val;
        if (m_rankItems.TryGetValue(type, out val))
            return val.Count;
        return 0;        
    }

    public T GetMyRankDataItem<T>(string type) where T : class
    {
        object val;
        if (m_myRankItem.TryGetValue(type, out val))
            return (T)val;
        return null;
    }

    public int GetRankDataRankVal(string type)
    {
        int val;
        if (m_myRankVals.TryGetValue(type, out val))
            return val;
        return -1;
    }

    public List<object> GetRankList(string type)
    {
        List<object> val;
        if (m_rankItems.TryGetValue(type, out val))
            return val;
        return null;
    }

    private List<T> ConvertListElement<T>(List<object> items)
    {
        List<T> newItems = new List<T>();
        for (int i = 0; i < items.Count; ++i)
            newItems.Add((T)items[i]);
        return newItems;
    }

    private void ProcessRankData<ITEM_TYPE>(RankDataVo data)
    {
        List<ITEM_TYPE> items = null;

        if (data.start == 0)
        {
            //我的排行相关数据
            m_myRankVals[data.type] = data.myRank;
            m_myRankItem[data.type] = JsonMapper.ToObject<ITEM_TYPE>(data.myData);
        }

        if (!data.clientNew)
        {
            try
            {
                items = JsonMapper.ToObject<List<ITEM_TYPE>>(data.data);
            }
            catch (Exception e)
            {
                Debuger.LogError(e.Message);
            }
            finally
            {
                if (items == null)
                    items = new List<ITEM_TYPE>();
            }
            
            List<object> existsItems = m_rankItems.GetNewIfNo(data.type);

            if (data.start == 0)
            {
                //第一页才修改更新时间，因为，如果前N页是旧的，后续页是新的，而这个更新时间也是新的，那就导致第一页没法刷新
                m_rankTimes[data.type] = data.upTime;
                existsItems.Clear();
            }

            for (int i = 0; i < items.Count; ++i)
            {
                var item = new RankItemWrapper(data.start + i, items[i]);
                if (data.start + i < existsItems.Count)
                {
                    existsItems[data.start + i] = item;
                }
                else
                {
                    existsItems.Add(item);
                }
            }
        }
    }

    public void OnRetRankData(RankDataVo data)
    {
        switch (data.type)
        {
            case RANK_TYPE_GOLD_LEVEL:
                {
                    ProcessRankData<GoldLevelRankItem>(data);
                    UIMgr.instance.Get<UIGoldLevelRank>().OnUpdateData();
                }
                break;
            case RANK_TYPE_ARENA:
                {
                    ProcessRankData<ArenaRankItem>(data);
                    UIMgr.instance.Get<UIRank>().OnUpdateData(data.type);
                }
                break;
            case RANK_TYPE_CORPS:
                {
                    ProcessRankData<CorpsRankItem>(data);
                    UIMgr.instance.Get<UIRank>().OnUpdateData(data.type);
                }
                break;
            case RANK_TYPE_FULL_POWER:
                {
                    ProcessRankData<FullPowerRankItem>(data);
                    UIMgr.instance.Get<UIRank>().OnUpdateData(data.type);
                }
                break;
            case RANK_TYPE_REAL_POWER:
                {
                    ProcessRankData<RealPowerRankItem>(data);
                    UIMgr.instance.Get<UIRank>().OnUpdateData(data.type);
                }
                break;
            case RANK_TYPE_LEVEL_STAR:
                {
                    ProcessRankData<LevelStarRankItem>(data);
                    UIMgr.instance.Get<UIRank>().OnUpdateData(data.type);
                }
                break;
            case RANK_TYPE_PREDICTOR:
                {
                    ProcessRankData<PredictorRankItem>(data);
                    UIMgr.instance.Get<UIRank>().OnUpdateData(data.type);
                }
                break;
        }
    }

    public void OnRetRankValue(MyRankValueVo data)
    {
        m_myRankVals[data.type] = data.rankVal;
    }

    public void OnRetDoLikeRankItem(DoLikeRankItemRes data)
    {
        List<object> items = m_rankItems.GetNewIfNo(data.type);
        var find = false;
        var strKey = data.key;
        var intKey = StringUtil.ToInt(data.key);
        for (var i = 0; i < items.Count && !find; ++i)
        {
            var item = (RankItemWrapper)items[i];
            switch (data.type)
            {
                case RANK_TYPE_FULL_POWER:
                    {
                        var realItem = (FullPowerRankItem)item.data;
                        if (realItem.key == intKey)
                        {
                            find = true;
                            realItem.like = data.like;
                        }
                    }
                    break;
                case RANK_TYPE_REAL_POWER:
                    {
                        var realItem = (RealPowerRankItem)item.data;
                        if (realItem.key == intKey)
                        {
                            find = true;
                            realItem.like = data.like;
                        }
                    }
                    break;
            }
        }

        //刷新UI列表
        UIMgr.instance.Get<UIRank>().OnUpdateData(data.type);
    }
}