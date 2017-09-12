"use strict";

/**
 * 无参回调
 * @callback CallbackWithNoParam
 */

/**
 * 只有boolean参数的回调
 * @callback CallbackOnlyBooleanParam
 * @param {boolean} ok
 */

/**
 * 只有error参数的回调
 * @callback CallbackOnlyErrorParam
 * @param {Error} err
 */

/**
 * 接收数据回调
 * @callback ReceiveDataCallback
 * @param {Message} data - 数据缓存
 */

/**
 * 连接关闭回调
 * @callback ConnCloseCallback
 * @param {Message[]} pendingMsgList - 还没发送成功给对方的数据包
 */

/**
 * 网络连接处理函数
 * @typedef {Object} NetConnectionHandler
 * @property {ReceiveDataCallback} onRecvData - 接收数据回调
 * @property {ConnCloseCallback} onConnClose - 连接关闭回调
 */

/**
 * 客户端网络连接处理函数
 * @typedef {Object} ClientConnectionHandler
 * @property {ReceiveDataCallback} onRecvData - 接收数据回调
 * @property {ConnCloseCallback} onConnClose - 连接关闭回调
 * @property {CallbackWithNoParam} onConnOK - 连接成功回调
 */

/**
 * 定时器回调
 * @callback OnTimerCallback
 * @param {number} timerID - 定时器ID
 */

/**
 * 可以响应定时器回调的类
 * @typedef {Object} OnTimerClass
 * @property {OnTimerCallback} onTimer - 接收数据回调
 */

/**
 * 事件回调
 * @callback OnEventCallback
 * @param {string} eventName - 事件名
 * @param {*} context - 上下文
 * @param {object} notifier - 事件发布者
 */

/**
 * 可以响应事件回调的类
 * @typedef {Object} OnEventClass
 * @property {OnEventCallback} onEvent - 接收事件回调
 */

/**
 * Http回调
 * @callback HttpCallback
 * @param {Error} err - 错误对象
 * @param {*} data - 返回的数据（可能转成对象了）
 */

/**
 * Http回调，参数为ok、msg、cxt
 * @callback HttpCallbackWithOkMsgCxt
 * @param {boolean} ok - 是否成功，如果false有可能是http问题，也可能是逻辑问题
 * @param {string} msg - 相关消息
 * @param {*} cxt - 相关数据
 */