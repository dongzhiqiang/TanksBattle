"use strict";

////////////内置模块////////////

////////////外部模块////////////
var Promise = require("bluebird");

////////////我的模块////////////
var globalServerAgent = require("../logic/http/globalServerAgent");
var peerServerAgent = require("../logic/http/peerServerAgent");
var adminServerAgent = require("../logic/http/adminServerAgent");

////////////导出函数////////////
var doInitCoroutine = Promise.coroutine(function * (){
    yield globalServerAgent.doInit();
    yield peerServerAgent.doInit();
    yield adminServerAgent.doInit();
});

function doInit() {
    return doInitCoroutine();
}

var doDestroyCoroutine = Promise.coroutine(function * (){
    yield adminServerAgent.doDestroy();
    yield peerServerAgent.doDestroy();
    yield globalServerAgent.doDestroy();
});

function doDestroy() {
    return doDestroyCoroutine();
}

////////////导出元素////////////
exports.doInit = doInit;
exports.doDestroy = doDestroy;