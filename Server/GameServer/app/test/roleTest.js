"use strict";

var Promise = require("bluebird");
var appCfg = require("../../config");
var dateUtil = require("../libs/dateUtil");
var dbUtil = require("../libs/dbUtil");
var enProp = require("../logic/enumType/propDefine").enProp;
var roleMgr = require("../logic/role/roleMgr");
var guidGenerator = require("../libs/guidGenerator");
var initGameCfg = require("../main/initGameCfg");

var roleData = {
    props: {heroId:1,guid:guidGenerator.generateGUID(),createTime:dateUtil.getTimestamp(),roleId:"kratos",name:"test1",level:1,exp:0,curWeapon:0,stamina:100,advLv:1,star:1,camp:0},
    pets: [
        {
            props: {guid:guidGenerator.generateGUID(),createTime:dateUtil.getTimestamp(),roleId:"kratos",name:"test1",level:1,exp:0,curWeapon:0,stamina:100,advLv:1,star:1,camp:0},
            equips: [
                {equipId:10000, level:1},
                {equipId:10100, level:1},
                {equipId:10200, level:1},
                {equipId:10300, level:1},
                {equipId:10400, level:1},
                {equipId:10500, level:1},
                {equipId:10600, level:1},
                {equipId:10700, level:1},
                {equipId:10800, level:1},
                {equipId:10900, level:1}
            ]
        },
        {
            props: {guid:guidGenerator.generateGUID(),createTime:dateUtil.getTimestamp(),roleId:"kratos",name:"test2",level:1,exp:0,curWeapon:0,stamina:100,advLv:1,star:1,camp:0},
            equips: [
                {equipId:10000, level:1},
                {equipId:10100, level:1},
                {equipId:10200, level:1},
                {equipId:10300, level:1},
                {equipId:10400, level:1},
                {equipId:10500, level:1},
                {equipId:10600, level:1},
                {equipId:10700, level:1},
                {equipId:10800, level:1},
                {equipId:10900, level:1}
            ]
        },
        {
            props: {guid:guidGenerator.generateGUID(),createTime:dateUtil.getTimestamp(),roleId:"kratos",name:"test3",level:1,exp:0,curWeapon:0,stamina:100,advLv:1,star:1,camp:0},
            equips: [
                {equipId:10000, level:1},
                {equipId:10100, level:1},
                {equipId:10200, level:1},
                {equipId:10300, level:1},
                {equipId:10400, level:1},
                {equipId:10500, level:1},
                {equipId:10600, level:1},
                {equipId:10700, level:1},
                {equipId:10800, level:1},
                {equipId:10900, level:1}
            ]
        }
    ],
    equips: [
        {equipId:10000, level:1, advLv:1},
        {equipId:10100, level:1, advLv:1},
        {equipId:10200, level:1, advLv:1},
        {equipId:10300, level:1, advLv:1},
        {equipId:10400, level:1, advLv:1},
        {equipId:10500, level:1, advLv:1},
        {equipId:10600, level:1, advLv:1},
        {equipId:10700, level:1, advLv:1},
        {equipId:10800, level:1, advLv:1},
        {equipId:10900, level:1, advLv:1}
    ],
    items: [
        {itemId:20000, num:1},
        {itemId:20001, num:1},
        {itemId:20002, num:1}
    ]
};

module.exports = {
    setUp: function (callback) {
        console.log("\r\n===========\r\n");
        //用协程初始化
        //这个用法效率很低，正确的用法是var coFun = Promise.coroutine()一次，多次coFun()，但这里不频繁调用，所以暂时可以这样用
        Promise.coroutine(function * () {
            yield initGameCfg.doInit();
            //使用单独的数据库
            appCfg.dbUrl = "mongodb://127.0.0.1:27017/test?maxPoolSize=1&autoReconnect=true&w=1&wtimeoutMS=5000";
            //初始化数据库
            yield dbUtil.initDB();
            //先把集合删除，为了清空数据
            var col = dbUtil.getDB().collection("role");
            //先清数据
            yield col.drop();
            //插入数据
            col.updateOne({"props.guid":roleData.props.guid}, roleData, {upsert:true});
            //恢复heroId
            roleData.props.heroId = 1;
            //可以开测了
            callback();
        })();
    },
    tearDown: function (callback) {
        //用协程初始化
        //这个用法效率很低，正确的用法是var coFun = Promise.coroutine()一次，多次coFun()，但这里不频繁调用，所以暂时可以这样用
        Promise.coroutine(function * () {
            //关闭数据库
            yield dbUtil.closeDB();
            yield initGameCfg.doDestroy();
            //可以结束了
            callback();
        })();
    },
    testOne: function (test) {
        var role = roleMgr.createRole(roleData);

        if (!role)
        {
            test.ok(role !== null, "创建角色失败");
            test.done();
            return;
        }

        role.addListener(function(eventName, context, notifier) {
            console.log("eventName:" + eventName + ", context:" + context);
        }, "login");

        role.setNumber(enProp.level, 10);
        test.equal(role.getNumber(enProp.level), 10);

        role.startBatch();
        role.addNumber(enProp.level, 10);
        test.equal(role.getNumber(enProp.level), 20);

        role.addNumber(enProp.level, -10);
        test.equal(role.getNumber(enProp.level), 10);

        role.setString(enProp.name, "test11");
        test.equal(role.getString(enProp.name), "test11");
        role.endBatch();

        var petData = {
            props: {guid:guidGenerator.generateGUID(),createTime:dateUtil.getTimestamp(),roleId:"kratos",name:"testNew",level:1,exp:0,curWeapon:0,stamina:100,advLv:1,star:1,camp:0},
            equips: [
                {equipId:10000, level:1, advLv:1},
                {equipId:10100, level:1, advLv:1},
                {equipId:10200, level:1, advLv:1},
                {equipId:10300, level:1, advLv:1},
                {equipId:10400, level:1, advLv:1},
                {equipId:10500, level:1, advLv:1},
                {equipId:10600, level:1, advLv:1},
                {equipId:10700, level:1, advLv:1},
                {equipId:10800, level:1, advLv:1},
                {equipId:10900, level:1, advLv:1}
            ]
        };
        var petsPart = role.getPetsPart();
        petsPart.addPetWithData(petData);
        var petNew = petsPart.getPetByGUID(petData.props.guid);
        test.notEqual(petNew, null);
        var petOne = petsPart.getPetByIndex(0);
        var petOne2 = petsPart.getPetByGUID(petOne.getGUID());
        test.equal(petOne, petOne2);
        petsPart.removePetByGUID(petOne.getGUID(), true);
        petOne.setString(enProp.name, petOne.getString(enProp.name) + "_New");
        petsPart.addPetWithRole(petOne);

        var itemsPart = role.getItemsPart();
        var result = itemsPart.getItemNum(20000);
        test.equal(result, 1);
        result = itemsPart.getItemNum(30000);
        test.equal(result, 0);
        result = itemsPart.getItemNum(30001);
        test.equal(result, 0);
        itemsPart.addItem(20000, 10);
        itemsPart.addItem(30000, 10);
        itemsPart.addItem(30001, 10);
        result = itemsPart.getItemNum(20000);
        test.equal(result, 11);
        result = itemsPart.getItemNum(30000);
        test.equal(result, 10);
        result = itemsPart.getItemNum(30001);
        test.equal(result, 10);
        result = itemsPart.getItemByItemId(20000);
        test.notEqual(result, null);
        result = itemsPart.getItemByItemId(30000);
        test.equal(result, null);
        var addItems = {20000:10, 20001:10, 100001:22, 100002:22, 30000:10, 30001:10};
        itemsPart.addItems(addItems);
        itemsPart.addItem(100001, 10);
        itemsPart.addItem(100003, 10);
        var costItems = {20000:1, 20001:1, 20002:2, 100001:1, 100002:1, 100003:1, 30000:1, 30001:1};
        result = itemsPart.canCostItems(costItems);
        test.equal(result, false);
        delete costItems[20002];
        result = itemsPart.canCostItems(costItems);
        test.equal(result, true);
        itemsPart.costItems(costItems);
        result = itemsPart.canCostItem(20002, 2);
        test.equal(result, false);
        result = itemsPart.canCostItem(20002, 1);
        test.equal(result, true);
        itemsPart.addRewards(1001);
        //var itemData = {itemId:20003, num:11};
        //itemsPart.addItemWithData(itemData);
        //var itemNew = itemsPart.getItemByItemId(itemData.itemId);
        //test.notEqual(itemNew, null);
        //var itemOne = itemsPart.getItemByIndex(0);
        //var itemOne2 = itemsPart.getItemByItemId(itemOne.itemId);
        //test.equal(itemOne, itemOne2);
        //itemsPart.removeItemByItemId(itemOne.itemId, true);
        //itemOne.num += 100;
        //itemsPart.addItemWithItem(itemOne);
        //result = itemsPart.canCostItem(itemOne.itemId, 5);
        //test.equal(result, true);
        //var oldItemNum = itemsPart.getItemNum(itemOne.itemId);
        //result = itemsPart.costItem(itemOne.itemId, 5);
        //test.equal(result, true);
        //var newItemNum = itemsPart.getItemNum(itemOne.itemId);
        //test.equal(newItemNum, oldItemNum - 5);
        //result = itemsPart.canCostItem(itemOne.itemId, itemOne.num);
        //test.equal(result, true);
        //result = itemsPart.costItem(itemOne.itemId, itemOne.num);
        //test.equal(result, true);
        //newItemNum = itemsPart.getItemNum(itemOne.itemId);
        //test.equal(newItemNum, 0);
        //itemOne2 = itemsPart.getItemByItemId(itemOne.itemId);
        //test.equal(itemOne2, null);
        //result = itemsPart.canCostItems({20002:1, 20003:1});
        //test.equal(result, true);
        //result = itemsPart.costItems({20002:1, 20003:1});
        //itemOne2 = itemsPart.getItemByItemId(20002);
        //test.equal(itemOne2, null);
        //newItemNum = itemsPart.getItemNum(20003);
        //test.equal(newItemNum, 10);
        //result = itemsPart.addItem(40000, 1);
        //test.equal(result, true);
        //result = itemsPart.addItem(40000, 11);
        //test.equal(result, true);
        //result = itemsPart.addItems({100001:10,100002:10});
        //test.equal(result, true);
        //result = itemsPart.addItems({100001:10,100002:10});
        //test.equal(result, true);
        //itemsPart.removeItemsByItemIds([20001,20003], false, true);
        //var itemTemp = itemsPart.getItemByItemId(20000);
        //test.equal(itemTemp, null);
        //itemTemp = itemsPart.getItemByItemId(20001);
        //test.equal(itemTemp, null);
        //itemsPart.getItemByItemId(100001).num += 100;
        //itemsPart.getItemByItemId(40000).num += 100;
        //result = itemsPart.syncAndSaveItems([100001,40000], true);
        //test.equal(result, true);
        //result = itemsPart.addItemsWithDataArr([{itemId:100010,num:10},{itemId:100011,num:11},{itemId:100012,num:12}], true);
        //test.equal(result, true);
        //var itemTemp1 = itemsPart.getItemByItemId(100010);
        //test.notEqual(itemTemp1, null);
        //var itemTemp2 = itemsPart.getItemByItemId(100011);
        //test.notEqual(itemTemp2, null);
        //var itemTemp3 = itemsPart.getItemByItemId(100012);
        //test.notEqual(itemTemp3, null);
        //result = itemsPart.removeItemsByItemIds([100010,100011,100012], true, true);
        //test.equal(result, true);
        //result = itemsPart.addItemsWithItems([itemTemp1,itemTemp2,itemTemp3], true);
        //test.equal(result, true);

        var equipData = {equipId:10003, level:22, advLv:1};
        var equipsPart = role.getEquipsPart();
        equipsPart.addEquipWithData(equipData);
        var equipNew = equipsPart.getEquipByEquipId(equipData.equipId);
        test.notEqual(equipNew, null);
        var equipOne = equipsPart.getEquipByIndex(0);
        var equipOne2 = equipsPart.getEquipByEquipId(equipOne.equipId);
        test.equal(equipOne, equipOne2);
        equipsPart.removeEquipByEquipId(equipOne.equipId, true);
        equipOne.level += 100;
        equipsPart.addEquipWithEquip(equipOne);
        equipOne.level += 100;
        equipOne.syncAndSave();
        equipsPart.removeEquipByEquipId(10600);

        petOne.startBatch();
        petOne.setNumber(enProp.exp, 999);
        petOne.setNumber(enProp.level, 100);
        petOne.addNumber(enProp.level, 11);
        petOne.endBatch();
        test.equal(petOne.getNumber(enProp.exp), 999);
        test.equal(petOne.getNumber(enProp.level), 111);

        var petEquipData = {equipId:10003, level:22, advLv:1};
        var petEquipsPart = petOne.getEquipsPart();
        petEquipsPart.addEquipWithData(petEquipData);
        var petEquipNew = petEquipsPart.getEquipByEquipId(petEquipData.equipId);
        test.notEqual(petEquipNew, null);
        var petEquipOne = petEquipsPart.getEquipByIndex(0);
        var petEquipOne2 = petEquipsPart.getEquipByEquipId(petEquipOne.equipId);
        test.equal(petEquipOne, petEquipOne2);
        petEquipsPart.removeEquipByEquipId(petEquipOne.equipId, true);
        petEquipOne.level += 100;
        petEquipsPart.addEquipWithEquip(petEquipOne);
        petEquipOne.level += 100;
        petEquipOne.syncAndSave();
        petEquipsPart.removeEquipByEquipId(10600);

        console.log(role);
        console.log("======direct=====\r\n");
        console.log(JSON.stringify(role));
        console.log("======json=====\r\n");
        console.log(JSON.stringify(role.getDBData()));
        console.log("======json db=====\r\n");
        console.log(JSON.stringify(role.getPrivateNetData()));
        console.log("=====json private======\r\n");
        console.log(JSON.stringify(role.getPublicNetData()));
        console.log("=====json public======\r\n");
        console.log(JSON.stringify(role.getProtectNetData()));
        console.log("=====json protect======\r\n");
        role.fireEvent("login", "test");

        role.release();

        test.done();
    },
    //testTwo: function (test) {
    //    var role = roleMgr.createRole(roleData);
    //    var testCount = 1000000;
    //
    //    if (!role)
    //    {
    //        test.ok(role !== null, "创建角色失败");
    //        test.done();
    //        return;
    //    }
    //
    //    console.time("profile get db data");
    //    for (var i = 0; i < testCount; ++i)
    //    {
    //        role.getDBData();
    //    }
    //    console.timeEnd("profile get db data");
    //
    //    console.time("profile get private data");
    //    for (var i = 0; i < testCount; ++i)
    //    {
    //        role.getPrivateNetData();
    //    }
    //    console.timeEnd("profile get private data");
    //
    //    console.time("profile get protect data");
    //    for (var i = 0; i < testCount; ++i)
    //    {
    //        role.getProtectNetData();
    //    }
    //    console.timeEnd("profile get protect data");
    //
    //    console.time("profile get public data");
    //    for (var i = 0; i < testCount; ++i)
    //    {
    //        role.getPublicNetData();
    //    }
    //    console.timeEnd("profile get public data");
    //
    //    role.release();
    //    test.done();
    //},
    //testThree: function (test) {
    //    //为了不同步、不存盘，这里设置主角ID为0
    //    roleData.props.heroId = 0;
    //    var role = roleMgr.createRole(roleData);
    //    var propIds = Object.keys(enProp);
    //    propIds.removeValue("lastLogin");
    //    propIds.removeValue("loginCount");
    //    propIds.removeValue("channelId");
    //    propIds.removeValue("userId");
    //    var propCnt = propIds.length;
    //    var testCount = 2000;
    //
    //    if (!role)
    //    {
    //        test.ok(role !== null, "创建角色失败");
    //        test.done();
    //        return;
    //    }
    //
    //    console.time("profile set");
    //    for (var i = 0, len = testCount / propCnt; i < len; ++i)
    //    {
    //        for (var j = 0; j < propCnt; ++j)
    //            role.setNumber(enProp[propIds[j]], i);
    //    }
    //    console.timeEnd("profile set");
    //
    //    console.time("profile set batch");
    //    for (var i = 0, len = testCount / propCnt; i < len; ++i)
    //    {
    //        role.startBatch();
    //        for (var j = 0; j < propCnt; ++j)
    //            role.setNumber(enProp[propIds[j]], i);
    //        role.endBatch();
    //    }
    //    console.timeEnd("profile set batch");
    //
    //    console.time("profile set invalid");
    //    for (var i = 0; i < testCount; ++i)
    //    {
    //        role.setNumber(100000, i);
    //    }
    //    console.timeEnd("profile set invalid");
    //
    //    console.time("profile add");
    //    for (var i = 0, len = testCount / propCnt; i < len; ++i)
    //    {
    //        for (var j = 0; j < propCnt; ++j)
    //            role.addNumber(enProp[propIds[j]], i);
    //    }
    //    console.timeEnd("profile add");
    //
    //    console.time("profile add batch");
    //    for (var i = 0, len = testCount / propCnt; i < len; ++i)
    //    {
    //        role.startBatch();
    //        for (var j = 0; j < propCnt; ++j)
    //            role.addNumber(enProp[propIds[j]], i);
    //        role.endBatch();
    //    }
    //    console.timeEnd("profile add batch");
    //
    //    console.time("profile add invalid");
    //    for (var i = 0; i < testCount; ++i)
    //    {
    //        role.addNumber(100000, 1);
    //    }
    //    console.timeEnd("profile add invalid");
    //
    //    console.time("profile get number");
    //    for (var i = 0; i < testCount; ++i)
    //    {
    //        role.getNumber(enProp.level);
    //    }
    //    console.timeEnd("profile get number");
    //
    //    console.time("profile get string");
    //    for (var i = 0; i < testCount; ++i)
    //    {
    //        role.getString(enProp.level);
    //    }
    //    console.timeEnd("profile get string");
    //
    //    console.time("profile get number not exists");
    //    for (var i = 0; i < testCount; ++i)
    //    {
    //        role.getNumber(enProp.lastLogin);
    //    }
    //    console.timeEnd("profile get number not exists");
    //
    //    console.time("profile get string not exists");
    //    for (var i = 0; i < testCount; ++i)
    //    {
    //        role.getString(enProp.lastLogin);
    //    }
    //    console.timeEnd("profile get string not exists");
    //
    //    console.time("profile get number invalid");
    //    for (var i = 0; i < testCount; ++i)
    //    {
    //        role.getNumber(100000);
    //    }
    //    console.timeEnd("profile get number invalid");
    //
    //    console.time("profile get string invalid");
    //    for (var i = 0; i < testCount; ++i)
    //    {
    //        role.getString(100000);
    //    }
    //    console.timeEnd("profile get string invalid");
    //
    //    role.release();
    //    test.done();
    //},
    //testFour: function (test) {
    //    var role = roleMgr.createRole(roleData);
    //    var propIds = Object.keys(enProp);
    //    propIds.removeValue("lastLogin");
    //    propIds.removeValue("loginCount");
    //    propIds.removeValue("channelId");
    //    propIds.removeValue("userId");
    //    var propCnt = propIds.length;
    //    var testCount = 2000;
    //
    //    if (!role)
    //    {
    //        test.ok(role !== null, "创建角色失败");
    //        test.done();
    //        return;
    //    }
    //
    //    console.time("profile set");
    //    for (var i = 0, len = testCount / propCnt; i < len; ++i)
    //    {
    //        for (var j = 0; j < propCnt; ++j)
    //            role.setNumber(enProp[propIds[j]], i);
    //    }
    //    console.timeEnd("profile set");
    //
    //    console.time("profile set batch");
    //    for (var i = 0, len = testCount / propCnt; i < len; ++i)
    //    {
    //        role.startBatch();
    //        for (var j = 0; j < propCnt; ++j)
    //            role.setNumber(enProp[propIds[j]], i);
    //        role.endBatch();
    //    }
    //    console.timeEnd("profile set batch");
    //
    //    console.time("profile set invalid");
    //    for (var i = 0; i < testCount; ++i)
    //    {
    //        role.setNumber(100000, i);
    //    }
    //    console.timeEnd("profile set invalid");
    //
    //    console.time("profile add");
    //    for (var i = 0, len = testCount / propCnt; i < len; ++i)
    //    {
    //        for (var j = 0; j < propCnt; ++j)
    //            role.addNumber(enProp[propIds[j]], i);
    //    }
    //    console.timeEnd("profile add");
    //
    //    console.time("profile add batch");
    //    for (var i = 0, len = testCount / propCnt; i < len; ++i)
    //    {
    //        role.startBatch();
    //        for (var j = 0; j < propCnt; ++j)
    //            role.addNumber(enProp[propIds[j]], i);
    //        role.endBatch();
    //    }
    //    console.timeEnd("profile add batch");
    //
    //    console.time("profile add invalid");
    //    for (var i = 0; i < testCount; ++i)
    //    {
    //        role.addNumber(100000, 1);
    //    }
    //    console.timeEnd("profile add invalid");
    //
    //    console.time("profile get number");
    //    for (var i = 0; i < testCount; ++i)
    //    {
    //        role.getNumber(enProp.level);
    //    }
    //    console.timeEnd("profile get number");
    //
    //    console.time("profile get string");
    //    for (var i = 0; i < testCount; ++i)
    //    {
    //        role.getString(enProp.level);
    //    }
    //    console.timeEnd("profile get string");
    //
    //    console.time("profile get number not exists");
    //    for (var i = 0; i < testCount; ++i)
    //    {
    //        role.getNumber(enProp.lastLogin);
    //    }
    //    console.timeEnd("profile get number not exists");
    //
    //    console.time("profile get string not exists");
    //    for (var i = 0; i < testCount; ++i)
    //    {
    //        role.getString(enProp.lastLogin);
    //    }
    //    console.timeEnd("profile get string not exists");
    //
    //    console.time("profile get number invalid");
    //    for (var i = 0; i < testCount; ++i)
    //    {
    //        role.getNumber(100000);
    //    }
    //    console.timeEnd("profile get number invalid");
    //
    //    console.time("profile get string invalid");
    //    for (var i = 0; i < testCount; ++i)
    //    {
    //        role.getString(100000);
    //    }
    //    console.timeEnd("profile get string invalid");
    //
    //    role.release();
    //    test.done();
    //},
};