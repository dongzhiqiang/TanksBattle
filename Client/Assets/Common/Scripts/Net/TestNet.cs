using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NetCore;

public class TestNet : MonoBehaviour {
#if !ART_DEBUG
    
    
	// Use this for initialization
	void Start () {
	    NetMgr.instance.Init();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    //用于单元测试
    [ContextMenu("TestMessage")]
    void TestMessage()
    {
       
    }

    

    string ip = "127.0.0.1";
    void OnGUI()
    {
        

        //做下分辨率适配，以免控件在不同设备上太小
        float height = 640f;
        float width = Screen.width * height / Screen.height;
        float s = Screen.height / height;

        //适配字体
        int oldLabelFontSize = GUI.skin.label.fontSize;
        int oldButtonFontSize = GUI.skin.button.fontSize;
        int oldToggleFontSize = GUI.skin.button.fontSize;
        GUI.skin.label.fontSize = (int)(24 * s);
        GUI.skin.button.fontSize = (int)(24 * s);
        GUI.skin.toggle.fontSize = (int)(24 * s);


        if (NetMgr.instance.State == enConnectorState.Connecting)
        {
            GUILayout.Label("连接中");
            return;
        }
        if (NetMgr.instance.State == enConnectorState.NotConnect)
        {
            ip = GUILayout.TextField(ip);
            if (GUILayout.Button("连接"))
            {
                NetMgr.instance.Connect(ip, 11111);
            }
            return;
        }
        
        //断开
        if (GUILayout.Button("断开"))
        {
            NetMgr.instance.Close();
            
        }

        //发送
        if (GUILayout.Button("发送byte"))
            NetMgr.instance.Send(10, 6, (byte)5);

        if (GUILayout.Button("发送short"))
            NetMgr.instance.Send(10, 6, (short)30000);

        if (GUILayout.Button("发送i"))
            NetMgr.instance.Send(10, 6, (int)1147483647);

        if (GUILayout.Button("发送l"))
            NetMgr.instance.Send(10, 6, (long)-15241147481647);

        if (GUILayout.Button("发送f"))
            NetMgr.instance.Send(10, 6, (float)-172.5684);

        if (GUILayout.Button("发送bool"))
            NetMgr.instance.Send(10, 6, false);

        if (GUILayout.Button("发送date"))
            NetMgr.instance.Send(10, 6, System.DateTime.Now);//System.DateTime.Now  new System.DateTime(1970, 1, 1,0,0,59)

        if (GUILayout.Button("发送字符串"))
            NetMgr.instance.Send(10, 6, "AccountFacadeImpl");

        if (GUILayout.Button("发送枚举"))
            NetMgr.instance.Send(10, 6, AccountState.BLOCK);

        if (GUILayout.Button("发送对象2"))
        {
            AccountVo vo = new AccountVo();
            /** 账户编号 */
	        vo.id=1008654259;
	        /** 账号 */
	        vo.name="名字五个字";
	        /** 创建时间 */
	        vo.createdOn = System.DateTime.Now;
	        /** 状态 */
	        vo.state = AccountState.BLOCK;

	        /** 最后登录时间 */
            vo.loginOn=System.DateTime.Now;
	        /** 最后登出时间 */
            vo.logoutOn=System.DateTime.Now;

	        /** 当天累计时间 */
	        vo.timeByDay=999999999;
	        /** 累计在线时间 */
            vo.timeByTotal=55555;

	        /** 累计在线天数(从0开始) */
	        vo.dayByTotal=111;
	        /** 连续登录天数(从0开始) */
	        vo.dayByContinuous=222;

	        /** 是否在线状态 */
	        vo.online=true;
	        
            LoginInfoVo vo2 = new LoginInfoVo();
            vo2.account = vo;
            NetMgr.instance.Send(10, 6, vo2);
        }

        if (GUILayout.Button("测试数组"))
        {
            
            string[] arr = new string[] { "测试数组", "测试数组4", "测试数组3", "测试数组2" };
            NetMgr.instance.Send(10, 6, arr);
        }

        if (GUILayout.Button("测试map"))
        {
            AccountVo vo = new AccountVo();
            /** 账户编号 */
	        vo.id=1008654259;
	        /** 账号 */
	        vo.name="名字五个字";
	        /** 创建时间 */
	        vo.createdOn = System.DateTime.Now;
	        /** 状态 */
	        vo.state = AccountState.BLOCK;

	        /** 最后登录时间 */
            vo.loginOn=System.DateTime.Now;
	        /** 最后登出时间 */
            vo.logoutOn=System.DateTime.Now;

	        /** 当天累计时间 */
	        vo.timeByDay=999999999;
	        /** 累计在线时间 */
            vo.timeByTotal=55555;

	        /** 累计在线天数(从0开始) */
	        vo.dayByTotal=111;
	        /** 连续登录天数(从0开始) */
	        vo.dayByContinuous=222;

	        /** 是否在线状态 */
	        vo.online=true;

            Dictionary<int, AccountVo> map = new Dictionary<int, AccountVo>();
            map[10086] = vo;
            map[9527] = vo;
            
            NetMgr.instance.Send(10, 6, map);
        }

        if (GUILayout.Button("测试Collection"))
        {
            List<string> l = new List<string>() { "测试数组", "测试数组4", "测试数组3", "测试数组2" };             
            NetMgr.instance.Send(10, 6, l);
        }

        //还原字体
        GUI.skin.label.fontSize = oldLabelFontSize;
        GUI.skin.button.fontSize = oldButtonFontSize;
        GUI.skin.toggle.fontSize = oldToggleFontSize;
    }

    void OnApplicationQuit()
    {
        NetMgr.instance.Dispose();
    }

    public enum AccountState
    {
        /** 正常 */
        NORMAL,
        /** 锁定 */
        BLOCK,
        /** 清理 */
        CLEAN,
    }
    public class WalletVo
    {

        /** 铜币 */
        public int copper;
        /** 元宝 */
        public  int gold;
        /** 礼券 */
        public  int gift;
        /** 内币 */
        public  int inter;
        /** 玉石 */
        public int jade;
        /** 贡献 */
        public int donate;
        /** 荣誉 */
        public  int honour;

    }

    public class AccountVo {

	    /** 账户编号 */
	    public long id;
	    /** 账号 */
	    public string name;
	    /** 创建时间 */
	    public System.DateTime createdOn;
	    /** 状态 */
	    public AccountState state;

	    /** 最后登录时间 */
        public System.DateTime loginOn;
	    /** 最后登出时间 */
        public System.DateTime logoutOn;

	    /** 当天累计时间 */
	    public long timeByDay;
	    /** 累计在线时间 */
        public long timeByTotal;

	    /** 累计在线天数(从0开始) */
	    public int dayByTotal;
	    /** 连续登录天数(从0开始) */
	    public int dayByContinuous;

	    /** 是否在线状态 */
	    public bool online;
	    
    }

    public class LoginInfoVo {

	    /** 用户的帐号信息 */
	    public AccountVo account;

    }

    public class Animal
    {
        public int a;
    }
    public class Dog : Animal
    {
        public int d;
    }
#endif
}
