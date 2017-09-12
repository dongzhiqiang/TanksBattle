"use strict";

var Promise = require("bluebird");

var doInitCoroutine = Promise.coroutine(function * () {
});

function doInit()
{
    return doInitCoroutine();
}

var doDestroyCoroutine = Promise.coroutine(function * () {
});

function doDestroy()
{
    return doDestroyCoroutine();
}

////////////导出元素////////////
exports.doInit = doInit;
exports.doDestroy = doDestroy;