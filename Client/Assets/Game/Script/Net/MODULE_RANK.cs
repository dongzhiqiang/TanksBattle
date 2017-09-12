using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class MODULE_RANK
{
    public const int CMD_REQUEST_RANK = 1;
    public const int CMD_REQ_MY_RANK_VAL = 2;
    public const int CMD_DO_LIKE = 3;
}

public class RESULT_CODE_RANK : RESULT_CODE
{
    public const int RANK_TYPE_WRONG = 1;    //排行类型不对
    public const int RANK_TYPE_DO_LIKE_FAIL = 2;    //点赞失败
}

public class RequestRankDataVo
{
    public string type; //排行类型
    public long time;   //客户端数据时间
    public int start;   //从第几行开始（基于0）
    public int len;     //取数据行数
    public int myRank; //获取我的排行信息（取首页时有效）
}

public class ReqMyRankValueVo
{
    public string type; //排行类型    
}
public class RankDataVo
{
    public string type;     //排行类型
    public bool clientNew;   //客户端数据是否本来就是最新的
    public long upTime;     //数据更新时间
    public string data;     //排行数据的json化字符串
    public int myRank;      //我的排名，如果不在排名内，则是-1
    public string myData;   //我在排行里的数据
    public string extra;    //额外数据的的json化字符串
    public int start;       //数据从第几行开始
    public int reqLen;      //原本要请求的数据量
    public int total;       //总数据行数
}

public class MyRankValueVo
{
    public string type;     //排行类型
    public int rankVal;     //排名
}

public class GoldLevelRankItem
{
    public int key;
    public string name;
    public int level;
    public int power;
    public int score;
    public string corpsName;
}

public class AllPetPowerRankItem
{
    public int key;
    public string name;
    public int level;
    public int petNum;
    public int power;
}

public class ArenaRankItem
{
    public int key;
    public string name;
    public int level;
    public int power;
    public int score;
    public string roleId;
    public string pet1Guid;
    public string pet1RoleId;
    public string pet2Guid;
    public string pet2RoleId;
}

public class CorpsRankItem
{
    public int key;
    public string name;
    public int level;
    public string president;
    public int power;
}

public class PredictorRankItem
{
    public int key;
    public string name;
    public int level;
    public int maxLayer;
    public int passTime;
}

public class FullPowerRankItem
{
    public int key;
    public string name;
    public int level;
    public int power;
    public int like;
}

//出战的主角、神侍的战斗力的和
public class RealPowerRankItem
{
    public int key;
    public string name;
    public int level;
    public int power;
    public int like;
}

public class LevelStarRankItem
{
    public int key;
    public string name;
    public int level;
    public int starNum;
}

public class PetPowerRankItem
{
    public string key;
    public int heroId;    
    public string petName;
    public string heroName;
    public int power;
    public int like;
}

public class RankItemWrapper
{
    public int rank;
    public object data;

    public RankItemWrapper(int rank, object data)
    {
        this.rank = rank;
        this.data = data;
    }
}

public class DoLikeRankItemReq
{
    public string type; //排行类型
    public string key;  //排行主键
}

public class DoLikeRankItemRes
{
    public string type; //排行类型
    public string key;  //排行主键
    public int like;    //最新点赞数
}