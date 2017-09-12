"use strict";

var Promise = require("bluebird");
var appCfg  = require("../../config");
var account = require("../logic/player/account");

module.exports = {
    setUp: function (callback) {
        callback();
    },
    tearDown: function (callback) {
        callback();
    },
    testOne: function (test) {
        //这个用法效率很低，正确的用法是var coFun = Promise.coroutine()一次，多次coFun()，但这里不频繁调用，所以暂时可以这样用
        Promise.coroutine(function * (){
            yield account.defaultPage();
            yield account.register("xiaolizhi", "123456");
            var retMsg = yield account.login("xiaolizhi", "123456");
            yield account.logout(retMsg.cxt.userId, retMsg.cxt.token);
            yield account.modifyPassword("xiaolizhi", "123456", "123456", true);
            yield account.getServers(appCfg.channelId, retMsg.cxt.userId);
            test.done();
        })();
    }
};