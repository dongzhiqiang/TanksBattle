"use strict";

/**
* 这里使用了Set，但Set遍历比较慢，Set的优势在于数据量多时add、delete、has速度更稳定，但数据量少时可以用Array
*/

var logUtil = require("../libs/logUtil");

/**
 * 格式：发布者 事件名 订阅者
 * @type {Map.<object, Map.<string, Set.<(OnEventClass|OnEventCallback)>>>}
 */
var notifierMap = new Map();

/**
 * 格式：订阅者 事件名 发布者
 * @type {Map.<(OnEventClass|OnEventCallback), Map.<string, Set.<object>>>}
 */
var observerMap = new Map();

/**
 * 格式：事件名 订阅者
 * @type {Map.<string, Set.<(OnEventClass|OnEventCallback)>>}
 */
var globalEventObserversMap = new Map();

/**
 * 格式：订阅者 事件名
 * @type {Map.<Set.<(OnEventClass|OnEventCallback)>, string>}
 */
var globalObserverEventsMap = new Map();

/**
 * 发布事件过程中不要删除、添加事件
 * @type {number}
 */
var fireRefCount = 0;

/**
 * 延迟操作，发布事件过程中
 * @type {object[]}
 */
var pendingOpList = [];

const PENDING_OP_REMOVE_NOTIFIER       = 1;
const PENDING_OP_ADD_LISTENER          = 2;
const PENDING_OP_REMOVE_LISTENER       = 3;
const PENDING_OP_ADD_GLOBAL_LISTENER   = 4;
const PENDING_OP_REMOVE_GLOBAL_LISTENER= 5;

/**
 * 删除某事件发布者的所有监听者
 * @param {object} notifier
 */
function removeNotifier(notifier)
{
    if (!notifier)
    {
        logUtil.error("EventMgr~removeNotifier，请提供有效notifier");
        return;
    }

    if (fireRefCount > 0)
    {
        pendingOpList.push({opType:PENDING_OP_REMOVE_NOTIFIER, notifier:notifier});
        return;
    }

    var nameObsMap = notifierMap.get(notifier);
    if (!nameObsMap)
        return;

    //从订阅者的数据结构里删除自己
    for (var nameObs of nameObsMap)
    {
        var eventName   = nameObs[0];
        var observerSet = nameObs[1];
        for (var observer of observerSet)
        {
            var nameNtsMap = observerMap.get(observer);
            if (!nameNtsMap)
                continue;
            var notifierSet = nameNtsMap.get(eventName);
            if (!notifierSet)
                continue;
            notifierSet.delete(notifier);
            if (notifierSet.size <= 0)
                nameNtsMap.delete(eventName);
            if (nameNtsMap.size <= 0)
                observerMap.delete(observer);
        }
    }

    //删除自己
    notifierMap.delete(notifier);
}

/**
 * 添加监听，这个必须指定监听哪个发布者
 * @param {(OnEventClass|OnEventCallback)} observer
 * @param {string} eventName
 * @param {object} notifier
 */
function addListener(observer, eventName, notifier)
{
    if (!(Object.isFunction(observer) || (Object.isObject(observer) && Object.isFunction(observer.onEvent))))
    {
        logUtil.error("EventMgr~addListener observer必须是函数或带有onEvent函数成员的对象");
        return;
    }

    if (!eventName)
    {
        logUtil.error("EventMgr~addListener 必须提供eventName");
        return;
    }

    if (!notifier)
    {
        logUtil.error("EventMgr~addListener 必须提供notifier");
        return;
    }

    if (fireRefCount > 0)
    {
        pendingOpList.push({opType:PENDING_OP_ADD_LISTENER, observer:observer, eventName:eventName, notifier:notifier});
        return;
    }

    //加入通知者的数据结构
    var nameObsMap = notifierMap.get(notifier);
    if (!nameObsMap)
    {
        nameObsMap = new Map();
        notifierMap.set(notifier, nameObsMap);
    }
    var observerSet = nameObsMap.get(eventName);
    if (!observerSet)
    {
        observerSet = new Set();
        nameObsMap.set(eventName, observerSet);
    }
    observerSet.add(observer);

    //加入观察者的数据结构
    var nameNtsMap = observerMap.get(observer);
    if (!nameNtsMap)
    {
        nameNtsMap = new Map();
        observerMap.set(observer, nameNtsMap);
    }
    var notifierSet = nameNtsMap.get(eventName);
    if (!notifierSet)
    {
        notifierSet = new Set();
        nameNtsMap.set(eventName, notifierSet);
    }
    notifierSet.add(notifier);
}

/**
 * 添加全局监听，就是不设定监听谁的事件，监听所有发布者的某个事件
 * @param {(OnEventClass|OnEventCallback)} observer
 * @param {string} eventName
 */
function addGlobalListener(observer, eventName)
{
    if (!(Object.isFunction(observer) || (Object.isObject(observer) && Object.isFunction(observer.onEvent))))
    {
        logUtil.error("EventMgr~addGlobalListener observer必须是函数或带有onEvent函数成员的对象");
        return;
    }

    if (!eventName)
    {
        logUtil.error("EventMgr~addGlobalListener 必须提供eventName");
        return;
    }

    if (fireRefCount > 0)
    {
        pendingOpList.push({opType:PENDING_OP_ADD_GLOBAL_LISTENER, observer:observer, eventName:eventName});
        return;
    }

    var observerSet = globalEventObserversMap.get(eventName);
    if (!observerSet)
    {
        observerSet = new Set();
        globalEventObserversMap.set(eventName, observerSet);
    }
    observerSet.add(observer);

    var eventNameSet = globalObserverEventsMap.get(observer);
    if (!eventNameSet)
    {
        eventNameSet = new Set();
        globalObserverEventsMap.set(observer, eventNameSet);
    }
    eventNameSet.add(eventName);
}

function _removeListener(notifierSet, observer, eventName, notifier)
{
    for (var curNotifier of notifierSet)
    {
        if (!notifier || curNotifier === notifier)
        {
            var nameObsMap = notifierMap.get(curNotifier);
            if (!nameObsMap)
                continue;
            var observerSet = nameObsMap.get(eventName);
            if (!observerSet)
                continue;
            observerSet.delete(observer);
            if (observerSet.size <= 0)
                nameObsMap.delete(eventName);
            if (nameObsMap.size <= 0)
                notifierMap.delete(curNotifier);

            notifierSet.delete(curNotifier);
        }
    }
}

/**
 * 删除监听，这里可以批量删除
 * @param {(OnEventClass|OnEventCallback)} observer
 * @param {string?} eventName - 如果notifier没填，则可不填，不填则删除这个监听者的全部监听
 * @param {object?} notifier - 可不填，不填则删除这个监听者对所有事件发布者的某事件的全部监听
 */
function removeListener(observer, eventName, notifier)
{
    if (!observer)
    {
        logUtil.error("EventMgr~removeListener 请提供observer");
        return;
    }

    if (!eventName && notifier)
    {
        logUtil.error("EventMgr~removeListener 不能只提供notifier，而不提供eventName");
        return;
    }

    if (fireRefCount > 0)
    {
        pendingOpList.push({opType:PENDING_OP_REMOVE_LISTENER, observer:observer, eventName:eventName, notifier:notifier});
        return;
    }

    var nameNtsMap = observerMap.get(observer);
    if (!nameNtsMap)
        return;

    if (eventName)
    {
        var notifierSet = nameNtsMap.get(eventName);
        if (notifierSet)
        {
            _removeListener(notifierSet, observer, eventName, notifier);
            if (notifierSet.size <= 0)
                nameNtsMap.delete(eventName);
        }
    }
    else
    {
        for (var nameNtsEntry of nameNtsMap)
        {
            var curEventName = nameNtsEntry[0];
            var curNotifierSet = nameNtsEntry[1];
            _removeListener(curNotifierSet, observer, curEventName, notifier);
            if (curNotifierSet.size <= 0)
                nameNtsMap.delete(curEventName);
        }
    }

    if (nameNtsMap.size <= 0)
        observerMap.delete(observer);
}

/**
 * 删除全局监听，这里可以批量删除
 * @param {(OnEventClass|OnEventCallback)} observer
 * @param {string?} eventName - 可不填，不填则删除全部全局监听的事件
 */
function removeGlobalListener(observer, eventName)
{
    if (!observer)
    {
        logUtil.error("EventMgr~removeGlobalListener 请提供observer");
        return;
    }

    if (fireRefCount > 0)
    {
        pendingOpList.push({opType:PENDING_OP_REMOVE_GLOBAL_LISTENER, observer:observer, eventName:eventName});
        return;
    }

    var observerSet;
    var eventNameSet;

    if (eventName)
    {
        observerSet = globalEventObserversMap.get(eventName);
        if (observerSet)
        {
            observerSet.delete(observer);
            if (observerSet.size <= 0)
                globalEventObserversMap.delete(eventName);
        }
        eventNameSet = globalObserverEventsMap.get(observer);
        if (eventNameSet)
        {
            eventNameSet.delete(eventName);
            if (eventNameSet.size <= 0)
                globalObserverEventsMap.delete(observer);
        }
    }
    else
    {
        eventNameSet = globalObserverEventsMap.get(observer);
        if (eventNameSet)
        {
            for (var curEventName of eventNameSet)
            {
                observerSet = globalEventObserversMap.get(curEventName);
                if (observerSet)
                {
                    observerSet.delete(observer);
                    if (observerSet.size <= 0)
                        globalEventObserversMap.delete(curEventName);
                }
            }
        }
        globalObserverEventsMap.delete(observer);
    }
}

/**
 * 判断是否存在监听，可以不关心是监听谁的事件，只判断是否监听这个事件
 * @param {(OnEventClass|OnEventCallback)} observer
 * @param {string} eventName
 * @param {object?} notifier - 可不填，不填则不管订阅的是谁的事件
 * @return {boolean}
 */
function hasListener(observer, eventName, notifier)
{
    if (!observer)
    {
        logUtil.error("EventMgr~hasListener 请提供observer");
        return false;
    }

    if (!eventName && notifier)
    {
        logUtil.error("EventMgr~hasListener 不能只提供notifier，而不提供eventName");
        return false;
    }

    var nameNtsMap = observerMap.get(observer);
    if (!nameNtsMap)
        return false;

    if (!eventName)
        return true;

    var notifierSet = nameNtsMap.get(eventName);
    if (!notifierSet)
        return false;

    if (!notifier)
        return true;

    return notifierSet.has(notifier);
}

/**
 * 是否存在全局监听
 * @param {(OnEventClass|OnEventCallback)} observer
 * @param {string} eventName
 */
function hasGlobalListener(observer, eventName)
{
    if (!observer)
    {
        logUtil.error("EventMgr~hasGlobalListener 请提供observer");
        return false;
    }

    var observerSet = globalObserverEventsMap.get(observer);
    if (!observerSet)
        return false;

    return observerSet.has(eventName);
}

function _fire(observerSet, eventName, context, notifier)
{
    for (var observer of observerSet)
    {
        try {
            if (Object.isFunction(observer))
                observer(eventName, context, notifier);
            else
                observer.onEvent(eventName, context, notifier);
        }
        catch (err) {
            logUtil.error("EventMgr~_fire发生错误，事件：" + eventName, err);
        }
    }
}

/**
 * 发起事件
 * @param {string} eventName
 * @param {*?} context - 上下文
 * @param {object?} notifier - 不填也行，不过只能全局监听才能收到这个事件了
 */
function fire(eventName, context, notifier)
{
    if (!eventName)
    {
        logUtil.error("EventMgr~fire 请提供eventName");
        return;
    }

    ++fireRefCount;

    var observerSet = globalEventObserversMap.get(eventName);
    if (observerSet)
        _fire(observerSet, eventName, context, notifier);

    if (notifier)
    {
        var nameObsMap = notifierMap.get(notifier);
        if (nameObsMap)
        {
            observerSet = nameObsMap.get(eventName);
            if (observerSet)
                _fire(observerSet, eventName, context, notifier);
        }
    }

    --fireRefCount;

    if (fireRefCount <= 0)
    {
        var len = pendingOpList.length;
        if (len > 0)
        {
            for (var i = 0; i < len; ++i)
            {
                var item = pendingOpList[i];
                switch (item.opType)
                {
                    case PENDING_OP_REMOVE_NOTIFIER:
                        removeNotifier(item.notifier);
                        break;
                    case PENDING_OP_ADD_LISTENER:
                        addListener(item.observer, item.eventName, item.notifier);
                        break;
                    case PENDING_OP_REMOVE_LISTENER:
                        removeListener(item.observer, item.eventName, item.notifier);
                        break;
                    case PENDING_OP_ADD_GLOBAL_LISTENER:
                        addGlobalListener(item.observer, item.eventName);
                        break;
                    case PENDING_OP_REMOVE_GLOBAL_LISTENER:
                        removeGlobalListener(item.observer, item.eventName);
                        break;
                }
            }
            pendingOpList = [];
        }
    }
}

function showDebugInfo()
{
    console.log("notifierMap：");
    console.log(notifierMap);
    console.log("observerMap：");
    console.log(observerMap);
    console.log("globalEventObserversMap：");
    console.log(globalEventObserversMap);
    console.log("globalObserverEventsMap：");
    console.log(globalObserverEventsMap);
    console.log("pendingOpList：");
    console.log(pendingOpList);
    console.log("fireRefCount：");
    console.log(fireRefCount);
}

exports.removeNotifier = removeNotifier;
exports.addListener = addListener;
exports.addGlobalListener = addGlobalListener;
exports.removeListener = removeListener;
exports.removeGlobalListener = removeGlobalListener;
exports.hasListener = hasListener;
exports.hasGlobalListener = hasGlobalListener;
exports.fire = fire;
exports.showDebugInfo = showDebugInfo;