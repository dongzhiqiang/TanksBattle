function printLog(str)
{
    print(str);
}

printLog("检查全局数据里不存在的角色")
var roleList = db.roleList.find().toArray();
for (var i = 0; i < roleList.length; ++i)
{
    var roleInfo = roleList[i];
    var otherDb = db.getSiblingDB("gamedb" + roleInfo.serverId);
    if (otherDb.role.count({"props.heroId":roleInfo.heroId}) <= 0)
    {
        printLog("不存的角色" + roleInfo.heroId);
        db.roleList.remove({heroId:roleInfo.heroId});
    }
}

printLog("检查全局数据里没添加的角色")
var dbNames = db.getMongo().getDBNames();
for (var i = 0; i < dbNames.length; ++i)
{
    var dbName = dbNames[i];
    var prefix = dbName.slice(0, 6);
    var serverId = parseInt(dbName.slice(6));
    if (prefix === "gamedb")
    {
        var otherDb = db.getSiblingDB(dbName);
        var roleDataList = otherDb.role.find({}, {props : 1}).toArray();
        for (var j = 0; j < roleDataList.length; ++j)
        {
            var roleData = roleDataList[j];
            var props = roleData.props;
            if (db.roleList.count({heroId : props.heroId}) <= 0)
            {
                printLog(props.heroId);
                db.roleList.insert(
                {
                    channelId : props.channelId,
                    userId : props.userId,
                    guid : props.guid,
                    name : props.name,
                    level : props.level,
                    roleId : "kratos",
                    heroId : props.heroId,
                    serverId : serverId,
                    lastLogin : Math.floor(Date.now() / 1000)
                });
            }
        }
    }
}