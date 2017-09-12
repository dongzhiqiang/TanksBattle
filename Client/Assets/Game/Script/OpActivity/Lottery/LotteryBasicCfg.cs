using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class LotteryBasicCfg
{
    public int typeId;
    public string typeName;
    public string tipText;
    public string[] previewTabs;
    public int[] previewPools;
    public int chipItemId;
    public int freeBuyCnt;
    public int freeBuyCD;
    public int[] buyOneWithItemCost;
    public int[] buyTenWithItemCost;
    public int[] buyTenWithTicketCost;
    public int[][] buyOneGet;
    public int[][] buyTenGet;
    public int[] freeBuyOneGift;
    public int[] buyOneWithItemGift;
    public int[][] buyTenWithItemGift;
    public int[][] buyTenWithTicketGift;
    public int[][] buyOneWithItemFirstNGift;
    public int[][][] buyTenWithItemFirstNGift;

    public static Dictionary<int, LotteryBasicCfg> m_cfgs = new Dictionary<int, LotteryBasicCfg>();
    public static int ADVANCED_TYPE_ID = 1;
    public static int TOPLEVEL_TYPE_ID = 2;
    public static int SUBTYPE_BUY_ONE = 1;
    public static int SUBTYPE_BUY_TEN = 2;

    public static void Init()
    {
        m_cfgs = Csv.CsvUtil.Load<int, LotteryBasicCfg>("opActivity/lotteryBasicCfg", "typeId");
    }

    public static LotteryBasicCfg Get(int typeId)
    {
        LotteryBasicCfg cfg;
        m_cfgs.TryGetValue(typeId, out cfg);
        return cfg;
    }
}
