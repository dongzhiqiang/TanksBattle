"use strict";

var appUtil = require("../libs/appUtil");
var snappy = require("../libs/snappy");
var IOBuffer = require("../libs/ioBuffer").IOBuffer;
var ProtocolCoder = require("../libs/protocolCoder");
var ArrayList = require("../libs/arrayList").ArrayList;
var BaseProxy = require("../libs/protocolProxy/baseProxy").BaseProxy;
var EnumType = require("../libs/enumType").EnumType;
var TestEnum = require("../logic/enumType/testEnum").TestEnum;
var JSON_COMPRESS_LEN = require("../libs/message").JSON_COMPRESS_LEN;

var testForCount = 1000000;

module.exports = {
    setUp: function (callback) {
        console.log("===========");
        callback();
    },
    tearDown: function (callback) {
        callback();
    },
    //testOne: function (test) {
    //    var s = "AccountRequest";
    //    console.log(s.hashCode());
    //
    //    var item1 = TestEnum.enItem1;
    //    var item2 = TestEnum.enItem2;
    //    console.log("=========");
    //    console.log(Object.isSubClass(TestEnum, EnumType));
    //    console.log("=========");
    //    console.log(EnumType.isEnumType(item1));
    //    console.log("=========");
    //    //Function.name暂时是非ES标准
    //    console.log(TestEnum.name);
    //    console.log("=========");
    //    console.log(item1.constructor);
    //    console.log("=========");
    //    console.log(item1.constructor.prototype);
    //    console.log("=========");
    //    console.log(TestEnum.getByLabel("enItem1"));
    //    console.log(TestEnum.getByLabel("enItem2"));
    //    console.log(TestEnum.getByValue(1));
    //    console.log(TestEnum.getByValue(2));
    //    console.log("=========");
    //    if (item1 !== item2)
    //        console.log("item1 !== item2");
    //    if (item1 === TestEnum.enItem1)
    //        console.log("item1 === TestEnum.item1");
    //    if (item1 !== TestEnum.enItem2)
    //        console.log("item1 !== TestEnum.item2");
    //    if (item2 !== TestEnum.enItem1)
    //        console.log("item1 !== TestEnum.item1");
    //    if (item2 === TestEnum.enItem2)
    //        console.log("item2 === TestEnum.item2");
    //    console.log("=========");
    //    test.done();
    //},
    //testTwo: function (test) {
    //    var ioBuf = new IOBuffer();
    //    BaseProxy.writeVarUInt(ioBuf, Math.pow(2, 7) - 1);
    //    BaseProxy.writeVarUInt(ioBuf, Math.pow(2, 15) - 1);
    //    BaseProxy.writeVarUInt(ioBuf, Math.pow(2, 23) - 1);
    //    BaseProxy.writeVarUInt(ioBuf, Math.pow(2, 31) - 1);
    //    BaseProxy.writeVarUInt(ioBuf, Math.pow(2, 39) - 1);
    //    BaseProxy.writeVarUInt(ioBuf, Math.pow(2, 47) - 1);
    //    BaseProxy.writeVarUInt(ioBuf, Math.pow(2, 53));
    //    var ret = BaseProxy.readVarUInt(ioBuf);
    //    console.log(ret);
    //    test.equal(ret, Math.pow(2, 7) - 1);
    //    ret = BaseProxy.readVarUInt(ioBuf);
    //    console.log(ret);
    //    test.equal(ret, Math.pow(2, 15) - 1);
    //    ret = BaseProxy.readVarUInt(ioBuf);
    //    console.log(ret);
    //    test.equal(ret, Math.pow(2, 23) - 1);
    //    ret = BaseProxy.readVarUInt(ioBuf);
    //    console.log(ret);
    //    test.equal(ret, Math.pow(2, 31) - 1);
    //    ret = BaseProxy.readVarUInt(ioBuf);
    //    console.log(ret);
    //    test.equal(ret, Math.pow(2, 39) - 1);
    //    ret = BaseProxy.readVarUInt(ioBuf);
    //    console.log(ret);
    //    test.equal(ret, Math.pow(2, 47) - 1);
    //    ret = BaseProxy.readVarUInt(ioBuf);
    //    console.log(ret);
    //    test.equal(ret, Math.pow(2, 53));
    //    test.done();
    //},
    //testTree: function (test) {
    //    var ioBuf = new IOBuffer();
    //    console.time("ProfileIOB");
    //    for (let i = 0; i < testForCount; ++i)
    //    {
    //        ioBuf.writeUInt8(Math.pow(2, 7) - 1);
    //        ioBuf.writeUInt16(Math.pow(2, 15) - 1);
    //        ioBuf.writeUInt24(Math.pow(2, 23) - 1);
    //        ioBuf.writeUInt32(Math.pow(2, 31) - 1);
    //        ioBuf.writeUInt40(Math.pow(2, 39) - 1);
    //        ioBuf.writeUInt48(Math.pow(2, 47) - 1);
    //        ioBuf.writeUInt56(Math.pow(2, 53));
    //        let ret = ioBuf.readUInt8();
    //        ret = ioBuf.readUInt16();
    //        ret = ioBuf.readUInt24();
    //        ret = ioBuf.readUInt32();
    //        ret = ioBuf.readUInt40();
    //        ret = ioBuf.readUInt48();
    //        ret = ioBuf.readUInt56();
    //    }
    //    console.timeEnd("ProfileIOB");
    //    console.time("ProfileCoder");
    //    for (let i = 0; i < testForCount; ++i)
    //    {
    //        BaseProxy.writeVarUInt(ioBuf, Math.pow(2, 7) - 1);
    //        BaseProxy.writeVarUInt(ioBuf, Math.pow(2, 15) - 1);
    //        BaseProxy.writeVarUInt(ioBuf, Math.pow(2, 23) - 1);
    //        BaseProxy.writeVarUInt(ioBuf, Math.pow(2, 31) - 1);
    //        BaseProxy.writeVarUInt(ioBuf, Math.pow(2, 39) - 1);
    //        BaseProxy.writeVarUInt(ioBuf, Math.pow(2, 47) - 1);
    //        BaseProxy.writeVarUInt(ioBuf, Math.pow(2, 53));
    //        let ret = BaseProxy.readVarUInt(ioBuf);
    //        ret = BaseProxy.readVarUInt(ioBuf);
    //        ret = BaseProxy.readVarUInt(ioBuf);
    //        ret = BaseProxy.readVarUInt(ioBuf);
    //        ret = BaseProxy.readVarUInt(ioBuf);
    //        ret = BaseProxy.readVarUInt(ioBuf);
    //        ret = BaseProxy.readVarUInt(ioBuf);
    //    }
    //    console.timeEnd("ProfileCoder");
    //    test.done();
    //},
    //testFour: function (test) {
    //    var Message = require("../logic/netMessage/testMessage").Message;
    //    var Component = require("../logic/netMessage/testMessage").Component;
    //    ProtocolCoder.instance.registerType(Message);
    //    var typeDef1 = ProtocolCoder.instance.getTypeDef(Message);
    //    var typeDef2 = ProtocolCoder.instance.getTypeDef(Message.name.hashCode());
    //    test.deepEqual(typeDef1, typeDef2);
    //    typeDef1 = ProtocolCoder.instance.getTypeDef(Component);
    //    typeDef2 = ProtocolCoder.instance.getTypeDef(Component.name.hashCode());
    //    test.deepEqual(typeDef1, typeDef2);
    //    ProtocolCoder.instance.registerEnum(TestEnum);
    //    var enumDef1 = ProtocolCoder.instance.getEnumDef(TestEnum);
    //    var enumDef2 = ProtocolCoder.instance.getEnumDef(TestEnum.name.hashCode());
    //    test.deepEqual(enumDef1, enumDef2);
    //    test.done();
    //},
    //testFive: function (test) {
    //    var Message = require("../logic/netMessage/testMessage").Message;
    //    var Component = require("../logic/netMessage/testMessage").Component;
    //    var a = new Component();
    //    a.propString = "广州";
    //    a.propNumber = 123456;
    //    a.propArray = ["1","2","3"];
    //    a.propEnum = TestEnum.enItem2;
    //    var l = new ArrayList();
    //    l.push(a);
    //    l.push(a);
    //    l.push(a);
    //    var m = new Map();
    //    m.set("a", a);
    //    m.set("b", a);
    //    m.set("c", a);
    //    var r = new Message();
    //    r.propString     = "default";
    //    r.propString2    = "default";
    //    r.propNumber     = 123;
    //    r.propBoolean    = true;
    //    r.propNull       = null;
    //    r.propSubObj     = a;
    //    r.propSubObj2    = a;
    //    r.propArray      = [a,a,a,a];
    //    r.propArray2     = r.propArray;
    //    r.propList       = l;
    //    r.propList2      = r.propList;
    //    r.propMap        = m;
    //    r.propMap2       = r.propMap;
    //    r.propBuf        = new Buffer("123456");
    //    r.propBuf2       = r.propBuf;
    //    r.propDate       = new Date("2015-12-20 0:0:0");
    //    r.propEnum      = TestEnum.enItem2;
    //    var ioBuf = new IOBuffer();
    //    ProtocolCoder.instance.encode(ioBuf, r);
    //    console.log("得到大小：" + ioBuf.canReadLen());
    //    var r2 = ProtocolCoder.instance.decode(ioBuf);
    //    console.log("剩余大小：" + ioBuf.canReadLen());
    //    test.deepEqual(r, r2);
    //    console.dir(r2);
    //    test.done();
    //},
    //testSix: function (test) {
    //    var Message = require("../logic/netMessage/testMessage").Message;
    //    var Component = require("../logic/netMessage/testMessage").Component;
    //    var a = new Component();
    //    a.propString = "广州";
    //    a.propNumber = 123456;
    //    a.propArray = ["1","2","3"];
    //    a.propEnum = TestEnum.enItem2;
    //    var l = new ArrayList();
    //    l.push(a);
    //    l.push(a);
    //    l.push(a);
    //    var m = new Map();
    //    m.set("a", a);
    //    m.set("b", a);
    //    m.set("c", a);
    //    var r = new Message();
    //    r.propString     = "default";
    //    r.propString2    = "default";
    //    r.propNumber     = 123;
    //    r.propBoolean    = true;
    //    r.propNull       = null;
    //    r.propSubObj     = a;
    //    r.propSubObj2    = a;
    //    r.propArray      = [a,a,a,a];
    //    r.propArray2     = r.propArray;
    //    r.propList       = l;
    //    r.propList2      = r.propList;
    //    r.propMap        = m;
    //    r.propMap2       = r.propMap;
    //    r.propBuf        = new Buffer("123456");
    //    r.propBuf2       = r.propBuf;
    //    r.propDate       = new Date("2015-12-20 0:0:0");
    //    r.propEnum      = TestEnum.enItem2;
    //    var ioBuf = new IOBuffer();
    //    var r2 = r;
    //    var coder = ProtocolCoder.instance;
    //    console.time("profile");
    //    for (var i = 0; i < testForCount; ++i) {
    //        coder.encode(ioBuf, r2, true);
    //        r2 = coder.decode(ioBuf, true);
    //    }
    //    console.timeEnd("profile");
    //    test.deepEqual(r, r2);
    //    console.dir(r2);
    //    test.done();
    //},
    //testSix2: function (test) {
    //    var a = {propString:"广州", "propNumber":123456, "propArray":[1,2,3], propEnum:2};
    //    a.propString = "广州";
    //    a.propNumber = 123456;
    //    a.propArray = ["1","2","3"];
    //    a.propEnum = 2;
    //    var l = [];
    //    l.push(a);
    //    l.push(a);
    //    l.push(a);
    //    var m = {};
    //    m.a = a;
    //    m.b = a;
    //    m.c = a;
    //    var r = {};
    //    r.propString     = "default";
    //    r.propString2    = "default";
    //    r.propNumber     = 123;
    //    r.propBoolean    = true;
    //    r.propNull       = null;
    //    r.propSubObj     = a;
    //    r.propSubObj2    = a;
    //    r.propArray      = [a,a,a,a];
    //    r.propArray2     = r.propArray;
    //    r.propList       = l;
    //    r.propList2      = r.propList;
    //    r.propMap        = m;
    //    r.propMap2       = r.propMap;
    //    r.propBuf        = "123456";
    //    r.propBuf2       = r.propBuf;
    //    r.propDate       = Math.floor(new Date("2015-12-20 0:0:0").getTime() / 1000);
    //    r.propEnum      = 2;
    //    var ioBuf = new IOBuffer();
    //    var r2 = r;
    //    console.time("profile");
    //    for (var i = 0; i < testForCount; ++i) {
    //        var s = JSON.stringify(r);
    //        r2 = JSON.parse(s);
    //    }
    //    console.timeEnd("profile");
    //    test.deepEqual(r, r2);
    //    console.dir(r2);
    //    test.done();
    //},
    //testSeven: function (test) {
    //    var Component = require("../logic/netMessage/testMessage").Component;
    //    var a = new Component();
    //    a.propString = "广州";
    //    a.propNumber = 123456;
    //    a.propArray = ["1","2","3"];
    //    a.propEnum = TestEnum.enItem2;
    //    var ioBuf = new IOBuffer();
    //    var a2 = a;
    //    console.time("profile");
    //    var coder = ProtocolCoder.instance;
    //    for (var i = 0; i < testForCount; ++i) {
    //        coder.encode(ioBuf, a, true);
    //        a2 = coder.decode(ioBuf, true);
    //    }
    //    console.timeEnd("profile");
    //    test.deepEqual(a, a2);
    //    console.dir(a2);
    //    test.done();
    //},
    //testSeven2: function (test) {
    //    var a = {propString:"广州", "propNumber":123456, "propArray":["1","2","3"], propEnum:2};
    //    var ioBuf = new IOBuffer();
    //    var a2 = a;
    //    console.time("profile");
    //    for (var i = 0; i < testForCount; ++i) {
    //        var s = JSON.stringify(a);
    //        var buf = snappy.compress(s);
    //        s  = snappy.uncompress(buf);
    //        a2 = JSON.parse(s);
    //    }
    //    console.timeEnd("profile");
    //    test.deepEqual(a, a2);
    //    console.dir(a2);
    //    test.done();
    //},
    //testEight: function (test) {
    //    var a = new TestClass();
    //    a.prop1 = 123456;
    //    a.prop2 = 123456;
    //    a.prop3 = 123456;
    //    a.prop4 = 123456;
    //    a.prop5 = 123456;
    //    a.prop6 = 123456;
    //    a.prop7 = 123456;
    //    a.prop8 = 123456;
    //    a.prop9 = 123456;
    //    a.prop10 = 123456;
    //    a.prop11 = 123456;
    //    a.prop12 = 123456;
    //    a.prop13 = 123456;
    //    a.prop14 = 123456;
    //    a.prop15 = 123456;
    //    a.prop16 = 123456;
    //    var ioBuf = new IOBuffer();
    //    var a2 = a;
    //    console.time("profile");
    //    var coder = ProtocolCoder.instance;
    //    for (var i = 0; i < testForCount; ++i) {
    //        coder.encode(ioBuf, a, true);
    //        a2 = coder.decode(ioBuf, true);
    //    }
    //    console.timeEnd("profile");
    //    test.deepEqual(a, a2);
    //    console.dir(a2);
    //    test.done();
    //},
    //testEight2: function (test) {
    //    var a = {};
    //    a.prop1 = 123456;
    //    a.prop2 = 123456;
    //    a.prop3 = 123456;
    //    a.prop4 = 123456;
    //    a.prop5 = 123456;
    //    a.prop6 = 123456;
    //    a.prop7 = 123456;
    //    a.prop8 = 123456;
    //    a.prop9 = 123456;
    //    a.prop10 = 123456;
    //    a.prop11 = 123456;
    //    a.prop12 = 123456;
    //    a.prop13 = 123456;
    //    a.prop14 = 123456;
    //    a.prop15 = 123456;
    //    a.prop16 = 123456;
    //    var ioBuf = new IOBuffer();
    //    var a2 = a;
    //    console.time("profile");
    //    for (var i = 0; i < testForCount; ++i) {
    //        var s = JSON.stringify(a);
    //        var buf = snappy.compress(s);
    //        s  = snappy.uncompress(buf);
    //        a2 = JSON.parse(s);
    //    }
    //    console.timeEnd("profile");
    //    test.deepEqual(a, a2);
    //    console.dir(a2);
    //    test.done();
    //},
    testNine: function (test) {
        var coder = ProtocolCoder.instance;
        var a = new TestClass2();
        a.prop1 = 123456;
        a.prop2 = "123456";
        a.prop3 = true;
        a.prop4 = [1,2,3];
        a.prop5 = new TestClass3();
        a.prop5.prop1 = 123;
        a.prop5.prop2 = "123";
        a.prop5.prop3 = false;
        a.prop5.prop4 = [1,2];
        var a2 = a;
        var ioBuf = new IOBuffer();

        console.time("profile custom");
        for (let i = 0; i < testForCount; ++i)
        {
            coder.encode(ioBuf, a);
            a2 = coder.decode(ioBuf);
        }
        console.timeEnd("profile custom");
        test.deepEqual(a, a2);

        console.time("profile json");
        for (let i = 0; i < testForCount; ++i)
        {
            var s = JSON.stringify(a);
            if (s.length > JSON_COMPRESS_LEN)
            {
                var buf = snappy.compress(s);
                s  = snappy.uncompress(buf);
            }
            a2 = JSON.parse(s);
        }
        console.timeEnd("profile json");
        test.deepEqual(a, a2);

        var ok = true;
        console.time("profile validate");
        for (let i = 0; i < testForCount; ++i)
        {
            ok = appUtil.validateObjectFields(a, TestClass2);
        }
        console.timeEnd("profile validate");
        test.ok(ok);

        ok = true;
        console.time("profile validate 2");
        for (let i = 0; i < testForCount; ++i)
        {
            ok = appUtil.validateObjectFields(a, TestClass2, null, true);
        }
        console.timeEnd("profile validate 2");
        test.ok(ok);

        test.done();
    },
};

class TestClass {
    static fieldsDesc() {
        return {
            prop1: {type: Number},
            prop2: {type: Number},
            prop3: {type: Number},
            prop4: {type: Number},
            prop5: {type: Number},
            prop6: {type: Number},
            prop7: {type: Number},
            prop8: {type: Number},
            prop9: {type: Number},
            prop10: {type: Number},
            prop11: {type: Number},
            prop12: {type: Number},
            prop13: {type: Number},
            prop14: {type: Number},
            prop15: {type: Number},
            prop16: {type: Number}
        };
    }
}

class TestClass2 {
    static fieldsDesc() {
        return {
            prop1: {type: Number},
            prop2: {type: String},
            prop3: {type: Boolean},
            prop4: {type: Array, itemType:Number},
            prop5: {type: TestClass3}
        };
    }
}

class TestClass3 {
    static fieldsDesc() {
        return {
            prop1: {type: Number},
            prop2: {type: String},
            prop3: {type: Boolean},
            prop4: {type: Array, itemType:Number}
        };
    }
}