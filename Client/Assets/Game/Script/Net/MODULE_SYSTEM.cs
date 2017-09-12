using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class MODULE_SYSTEM
{
    public const int CMD_SET_TEACH_DATA = 1;    //通知服务器保存引导的数据
    public const int PUSH_ADD_OR_UPDATE_SYSTEM = -1;   //添加或更新系统
    public const int PUSH_SET_TEACH_DATA = -2;    //主要用于GM命令同步修改后的值
    public const int PUSH_CLEAR_TEACH_DATA = -3;  //主要用于GM命令同步清空操作
    public const int PUSH_ADD_OR_UPDATE_SYSTEMS = -4;   //添加或更新系统(多个)
}

public class RESULT_CODE_SYSTEM : RESULT_CODE
{
    public const int BAD_TEACH_DATA = 1;
    public const int TEACH_KEYS_MAX = 2;
}


public class AddOrUpdateSystemVo
{
    public bool isAdd;
    public SystemVo system;
}


public class SystemVo
{
    public SystemVo()
    {
    }

    public SystemVo(int systemId, bool active)
    {
        this.systemId = systemId;
        this.active = active;
    }

    public int systemId;     //系统ID
    public bool active;       //是否激活
}

public class AddOrUpdateSystemsVo
{
    public List<SystemVo> systems;
}

public class SetTeachDataVo
{
    public string key;
    public int val;
}

public class PushSetTeachDataVo
{
    public string key;
    public int val;
}