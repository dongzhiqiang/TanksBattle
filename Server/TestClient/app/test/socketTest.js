"use strict";

var Connection = require("../logic/network/connection").Connection;
var Message = require("../libs/message").Message;

const HOST = "127.0.0.1";
const PORT = 20168;

module.exports = {
    setUp: function (callback) {
        callback();
    },
    tearDown: function (callback) {
        callback();
    },
    testOne: function (test) {
        var conn = new Connection();

        var MessageClass = require("../logic/netMessage/testMessage").Message;
        var Component = require("../logic/netMessage/testMessage").Component;
        var ArrayList = require("../libs/arrayList").ArrayList;
        var TestEnum = require("../logic/enumType/testEnum").TestEnum;

        var a = new Component();
        a.propString = "";
        a.propNumber = 123456;
        a.propArray = [1,2];
        a.propEnum = TestEnum.enItem2;

        var l = new ArrayList();
        l.push(a);
        l.push(a);
        l.push(a);
        var m = new Map();
        m.set("a", a);
        m.set("b", a);
        m.set("c", a);
        var r = new MessageClass();
        r.propString     = "default";
        r.propString2    = "default";
        r.propNumber     = 123;
        r.propBoolean    = true;
        r.propNull       = null;
        r.propSubObj     = a;
        r.propSubObj2    = a;
        r.propArray      = [a,a,a,a];
        r.propArray2     = r.propArray;
        r.propList       = l;
        r.propList2      = r.propList;
        r.propMap        = m;
        r.propMap2       = r.propMap;
        r.propBuf        = new Buffer("123456");
        r.propBuf2       = r.propBuf;
        r.propDate       = new Date("2015-12-20 0:0:0");
        r.propEnum      = TestEnum.enItem1;

        var msgList = [];
        var msgCount = 1000;
        for (var i = 1; i <= msgCount; ++i)
        {
            var cmd = i == msgCount ? -1 : i;
            let msg = Message.newRequest(i, cmd, r);
            msgList.push(msg);
        }

        var recvCount = 0;
        conn.setHandler({
            /**
             *
             * @param {Message} msgObj
             */
            onRecvData: function(msgObj) {
                recvCount++;
                if (msgObj.command == -1)
                {
                    if (recvCount == msgCount)
                    {
                        console.timeEnd("send_profile");
                        console.log("收到包数：" + recvCount + "，包编号：" + msgObj.getModule());
                    }
                    else
                    {
                        console.log("包数不对，收到包数：" + recvCount + "，包编号：" + msgObj.getCommand());
                    }
                    console.log(JSON.stringify(msgObj.getBodyObj()));
                }
            },
            onConnClose: function(pendingMsgList) {
                test.done();
            },
            onConnOK: function(){
                recvCount = 0;
                console.time("send_profile");
                for (var i = 0; i < msgCount; ++i)
                {
                    var msgObj = msgList[i];
                    conn.send(msgObj);
                }
            }
        });
        conn.connect(HOST, PORT);
    }
};