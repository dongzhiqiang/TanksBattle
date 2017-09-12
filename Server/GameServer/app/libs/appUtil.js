"use strict";

/*
 这个模块会优先被加载，所以不能加载其它自己写的模块
 */

////////////内部模块////////////
var crypto = require("crypto");
var EventEmitter = require("events");
var pathUtil = require("path");

////////////外部模块////////////
var cluster = require("cluster");

////////////我的模块////////////
var EnumType = require("./enumType").EnumType;

////////////模块内数据////////////
var processEvent = new EventEmitter();
var isInited = false;   //是否初始化完成
var isFired = false;    //退出只能触发一次

////////////原型扩展////////////
/**
 * 值可以是：darwin，freebsd，linux，sunos，win32
 * @type {String|*}
 */
var systemType = process.platform;

/**
 * 值可以是：x64，arm，ia32
 * @type {String|*}
 */
var cpuArchType = process.arch;

//最大32位整数
const INT32_MAX_POSITIVE = 2147483647;

//最小32位整数
const INT32_MAX_NEGTIVE  = -2147483648;

//Double最大可精确表示的正整数
const INT_MAX_POSITIVE = 9007199254740992;

//Double最大可精确表示的负整数
const INT_MAX_NEGTIVE = -9007199254740992;

//最大无符号32位整数
const UINT32_MAX = 4294967296;

//最大无符号16位整数
const UINT16_MAX = 65536;

//单精度最大值（负数就直接负号）
const FLOAT_MAX = 3.40282347e+38;

//双精度最大值（负数就直接负号）
const DOUBLE_MAX = 1.7976931348623157e+308;

////////////内建对象扩展////////////
var regExpTrim = /(^\s*)|(\s*$)/g;
var regExpLTrim = /(^\s*)/g;
var regExpRTrim = /(\s*$)/g;
var toString = Object.prototype.toString;

Math.clamp = function(num, min, max) {
    return Math.min(max, Math.max(min, num));
};

String.prototype.trim = function () {
    return this.replace(regExpTrim, "");
};

String.prototype.ltrim = function () {
    return this.replace(regExpLTrim, "");
};

String.prototype.rtrim = function () {
    return this.replace(regExpRTrim, "");
};

String.prototype.endWith = function (str) {
    if (str === null || str === undefined || str === "" || this.length === 0 || str.length > this.length)
        return false;
    return this.substring(this.length - str.length) === str;
};

String.prototype.startWith = function (str) {
    if (str === null || str === undefined || str === "" || this.length === 0 || str.length > this.length)
        return false;
    return this.substr(0, str.length) === str;
};

String.prototype.format = function (args) {
    var result = this;
    if (arguments.length > 0) {
        if (arguments.length === 1 && Object.isObject(args)) {
            for (var key in args) {
                let val = args[key];
                if (val !== null && val !== undefined) {
                    let reg = new RegExp("({" + key + "})", "g");
                    result = result.replace(reg, val);
                }
            }
        }
        else {
            for (var i = 0; i < arguments.length; i++) {
                let val = arguments[i];
                if (val !== null && val !== undefined) {
                    let reg = new RegExp("({[" + i + "]})", "g");
                    result = result.replace(reg, val);
                }
            }
        }
    }
    return result;
};

String.prototype.hashCode = function() {
    var hash = 0;
    for (var i = 0; i < this.length; i++) {
        var char = this.charCodeAt(i);
        hash = ((hash << 5) - hash) + char;
        hash = hash >> 0;
    }
    return hash;
};

/**
 * 是否存在某值的元素
 * @param v
 * @returns {boolean}
 */
Array.prototype.existsValue = function(v)
{
    return this.indexOf(v) >= 0;
};

/**
 * 只删除一次
 * @param v
 * @return {boolean} 发生删除操作就返回true
 */
Array.prototype.removeValue = function(v)
{
    var index = this.indexOf(v);
    if (index >= 0)
    {
        this.splice(index, 1);
        return true;
    }
    else
    {
        return false;
    }
};

/**
 *
 * @param v
 * @return {boolean} 发生push操作就返回true
 */
Array.prototype.pushIfNotExist = function(v)
{
    if (this.indexOf(v) < 0)
    {
        this.push(v);
        return true;
    }
    else
    {
        return false;
    }
};
/**
 * 数组随机洗牌排序
 */
Array.prototype.shuffle = function()
{
    for(var i = 0,len = this.length; i<len; ++i)
    {
        var idx = getRandom(0, len-i-1);
        var temp = this[idx];
        this[idx] = this[len-1-i];
        this[len-1-i] = temp;
    }
}


//类型判断
Object.isArray = function (o) {
    //注意instanceof Array在浏览器多frame情况下是无效的，浏览器情况下使用toString.call(o) === "[object Array]"
    //return toString.call(o) === "[object Array]";
    //return Array.isArray(o);
    //用instanceof效率最高了
    return o instanceof Array;
};

Object.isRegExp = function (o) {
    //注意instanceof RegExp在浏览器多frame情况下是无效的，浏览器情况下使用toString.call(o) === "[object RegExp]"
    //return toString.call(o) === "[object RegExp]";
    //用instanceof效率最高了
    return o instanceof RegExp;
};

Object.isDate = function (o) {
    //注意instanceof Date在浏览器多frame情况下是无效的，浏览器情况下使用toString.call(o) === "[object Date]"
    //return toString.call(o) === "[object Date]";
    //用instanceof效率最高了
    return o instanceof Date;
};

Object.isMap = function (o) {
    return o instanceof Map;
};

Object.isSet = function (o) {
    return o instanceof Set;
};

Object.isObject = function (o) {
    return typeof(o) === "object";
};

Object.isString = function (o) {
    return typeof(o) === "string";
};

Object.isNumber = function (o) {
    return typeof(o) === "number";
};

Object.isFunction = function (o) {
    return typeof(o) === "function";
};

Object.isUndefined = function (o) {
    return typeof(o) === "undefined";
};

Object.isBoolean = function (o) {
    return typeof(o) === "boolean";
};

/**
 *
 * @param {function} sub 可能子类
 * @param {function} sup 可能父类
 * @param {boolean?} [notSelf=false] 不包括相等
 */
Object.isSubClass = function(sub, sup, notSelf)
{
    if (notSelf)
        return sub.prototype instanceof sup;
    else
        return sub.prototype instanceof sup || sub === sup;
};

/**
 * 对象浅复制
 * @param {object} o
 * @returns {object}
 */
Object.simpleCopy = function(o)
{
    var o2 = {};
    for (var k in o)
        o2[k] = o[k];
    return o2;
};

////////////导出函数////////////
/**
 *
 * @param {string} str
 * @param {number} maxLen
 * @param {string} [ellipsisStr=""]
 */
function limitString(str, maxLen, ellipsisStr)
{
    if (str.length > maxLen)
        str = str.slice(0, maxLen) + (ellipsisStr || "");
    return str;
}

/**
 * 
 * @type {string[]} 字符串分割成（多维）数组的分割符
 */
var stringSplitSeqs = [ ',', '|', ':', ';', '^' ];

/**
 * 尝试把字符串转为某种类型，注意一定要是字符串
 * @param {string} str
 * @param {function} type - 主类型
 * @param {function?} [elemType=Number] - 如果是数组，还有元素类型，默认是数字
 * @param {number?} [maxLayer=1] - 数组最大层数，默认值是1
 * @param {number?} [curLayer=1] - 当前处于第几层（1-maxLayer），默认值是1
 */
function tryParseToDataType(str, type, elemType, maxLayer, curLayer)
{
    //保证是string，为了性能使用===
    if (str === null || str === undefined)
        str = "";
    else if (!Object.isString(str))
        str = str.toString();

    if (type === Number)
        return parseFloat(str) || 0; //防止得到NaN
    else if (type === String)
        return str;
    else if (type === Boolean)
        return !(str.length <= 0 || str === "0" || str === "false");
    else if (type === Array)
    {
        if (str.length <= 0)
            return [];

        if (elemType === Array)
            throw new Error("数组元素不能为Array");

        if (curLayer > stringSplitSeqs.length)
            throw new Error("数组层次太多");

        maxLayer = maxLayer || 1;
        curLayer = curLayer || 1;
        type = curLayer < maxLayer ?  Array : elemType || Number;
        elemType = curLayer < maxLayer ?  elemType : undefined;
        let arr = str.split(stringSplitSeqs[curLayer - 1]);
        for (let i = 0; i < arr.length; ++i)
            arr[i] = tryParseToDataType(arr[i], type, elemType, maxLayer, curLayer + 1);
        return arr;
    }
    else if (Object.isSubClass(type, EnumType))
        return type.getByValue(parseInt(str) || 0);
    else
    //假设这个类型有以string为参数的构造函数
        return new type(str);
}

/**
 * 使用类描述结构来验证json对象，注意是json对象，如果是自定义二进制转换过来的，不要用这个验证，因为反串行化过程中已验证了
 * @param {object} obj
 * @param {function} type
 * @param {(string[])?} [fields=null]
 * @param {boolean?} [noCheckSub=false] - 不检测子对象
 * @returns {boolean}
 */
function validateObjectFields(obj, type, fields, noCheckSub)
{
    if (!Object.isObject(obj))
        return false;
    //没有类型描述就当它验证通过了吧
    if (!type.fieldsDesc)
        return true;
    var fieldsDesc = type.fieldsDesc();
    if (!fields)
    {
        //缓存到类静态变量，因为for in速度比较慢
        if (!type.fieldNames)
            fields = type.fieldNames = Object.keys(fieldsDesc);
        else
            fields = type.fieldNames;
    }
    for (var i = 0, lenI = fields.length; i < lenI; ++i)
    {
        var name = fields[i];
        var desc = fieldsDesc[name];
        var fieldType = desc.type;
        var val = obj[name];

        //如果是null，得看看允不允许null
        //为了性能，这里不是用val == null（如果是双等号，null和undefined都行）
        if (val === null || val === undefined) {
            if (desc.notNull)
                return false;
        }
        //如果不是null，得用对类型
        else {
            //这些类型类型相等
            if (fieldType === Number || fieldType === String || fieldType === Boolean) {
                //这里为什么不用instanceof？因为数字、字符串、布尔的instanceof会为false
                if (val.constructor !== fieldType)
                    return false;
            }
            else if (fieldType === Array) {
                if (!Object.isArray(val))
                    return false;

                //用于数组元素类型验证
                var itemType = desc.itemType;
                //如果是要验证子对象、是数组、有元素类型、且元素类型是简单类型，就验证数组元素
                if (!noCheckSub && (itemType === Number || itemType === String || itemType === Boolean))
                {
                    for (var j = 0, lenJ = val.length; j < lenJ; ++j)
                    {
                        var elemVal = val[j];
                        //数组元素不允许null
                        if (elemVal === null || elemVal === undefined || elemVal.constructor !== itemType)
                            return false;
                    }
                }
            }
            //其它就假定是Object，由于json里对象的类型就是Object，不能使用val.constructor !== desc.type来验证
            else {
                if (!noCheckSub)
                    return validateObjectFields(val, fieldType);
                //如果不检查子对象的字段，那这个值至少要是个object
                else if (!Object.isObject(val))
                    return false;
            }
        }
    }
    return true;
}

/**
 *
 * @param {number} a
 * @param {number} b
 */
function getRandom(a, b)
{
    if (b < a)
    {
        var temp = b;
        b = a;
        a = temp;
    }
    return Math.floor(Math.random() * (b - a + 1)) + a;
}

function getRandomStr(len) {
    var s = [];
    for (var i = 0; i < len; i++) {
        s[i] = Math.random() > 0.5 ? getRandom(0, 9) : String.fromCharCode(getRandom(0, 25) + (Math.random() > 0.5 ? 65 : 97));
    }
    return s.join("");
}

/**
 * 根据权重获取N个随机项，这个N个项，不重复
 * @param {object[]} itemList - 数据列表
 * @param {number} wantNum - 想随机的数据个数
 * @param {string} weightField - itemList里的数据项的权重字段名
 * @param {string?} idField - 可选，itemList里的数据项的ID字段名，如果有填这个参数，返回的结果是ID列表，否则是数据项列表
 */
function getUnrepeatableRandItems(itemList, wantNum, weightField, idField)
{
    var result = [];

    //要全部？直接全部加入
    if (wantNum >= itemList.length)
    {
        for (let i = 0; i < itemList.length; ++i)
        {
            let temp = itemList[i];
            result.push(idField ? temp[idField] : temp);
        }
    }
    //要部分？那就用权重来随机吧
    else if (wantNum > 0)
    {
        //复制数据，用于删除item
        let itemList2 = itemList.slice();

        //计算全部权重
        let weightSum = 0;
        for (let i = 0; i < itemList2.length; ++i)
        {
            let temp = itemList2[i];
            weightSum += temp[weightField];
        }

        //随机wantNum次，每次根据权重取出一个数据，总权重减去这个数据的权重，下次随机时，排除这个数据
        for (let i = 0; i < wantNum; ++i)
        {
            let tempWeightSum = weightSum;
            let randVal = getRandom(0, tempWeightSum - 1);
            for (let j = 0; j < itemList2.length; ++j)
            {
                let temp = itemList2[j];
                tempWeightSum -= temp[weightField];
                if (tempWeightSum <= randVal)
                {
                    result.push(idField ? temp[idField] : temp);
                    weightSum -= temp[weightField];
                    itemList2.splice(j, 1);
                    break;
                }
            }
        }
    }

    return result;
}

/**
 * 根据权重获取N个随机项，这个N个项，可重复
 * @param {object[]} itemList - 数据列表
 * @param {number} wantNum - 想随机的数据个数
 * @param {string} weightField - itemList里的数据项的权重字段名
 * @param {string?} idField - 可选，itemList里的数据项的ID字段名，如果有填这个参数，返回的结果是ID列表，否则是数据项列表
 */
function getRepeatableRandItems(itemList, wantNum, weightField, idField)
{
    var result = [];

    if (wantNum > 0)
    {
        //计算全部权重
        let weightSum = 0;
        for (let i = 0; i < itemList.length; ++i)
        {
            let temp = itemList[i];
            weightSum += temp[weightField];
        }

        //随机wantNum次，每次根据权重取出一个数据，总权重减去这个数据的权重，下次随机时，排除这个数据
        for (let i = 0; i < wantNum; ++i)
        {
            let tempWeightSum = weightSum;
            let randVal = getRandom(0, tempWeightSum - 1);
            for (let j = 0; j < itemList.length; ++j)
            {
                let temp = itemList[j];
                tempWeightSum -= temp[weightField];
                if (tempWeightSum <= randVal)
                {
                    result.push(idField ? temp[idField] : temp);
                    break;
                }
            }
        }
    }

    return result;
}

function getWorkerID() {
    return cluster.isMaster ? 0 : cluster.worker.id;
}

function getMD5(str) {
    var md5 = crypto.createHash("md5");
    md5.update(str, "utf8");
    return md5.digest("hex");
}

function formatCapacity(bytes) {
    if (bytes >= 1024 * 1024 * 1024) {
        return (bytes / (1024 * 1024 * 1024)).toFixed(2) + "GB";
    }
    else if (bytes >= 1024 * 1024) {
        return (bytes / (1024 * 1024)).toFixed(2) + "MB";
    }
    else if (bytes >= 1024) {
        return (bytes / 1024).toFixed(2) + "KB";
    }
    else {
        return bytes + "字节";
    }
}

function isWindows()
{
    return systemType === "win32";
}

function isLinux()
{
    return systemType === "linux";
}

function isSystem64Bit()
{
    return cpuArchType === "x64";
}

/**
 * 加载本地库
 * @param {string} libName - 库名，不带路径
 * @param {boolean?} [failReturnNull=false] - 加载失败就返回null，而不是抛出异常
 * @returns {*}
 */
function requireNativeLib(libName, failReturnNull)
{
    var err  = null;
    try
    {
        if (isSystem64Bit())
        {
            if (isWindows())
            {
                return require("../native/win64/" + libName);
            }
            else if (isLinux())
            {
                return require("../native/linux64/" + libName);
            }
            else
            {
                err = new Error("目前只支持Windows 64位和Linux 64位系统的本地库");
            }
        }
        else
        {
            err = new Error("目前只支持Windows 64位和Linux 64位系统的本地库");
        }
    }
    catch (e)
    {
        err = e;
    }

    if (failReturnNull)
        return null;
    else
        throw err;
}

function parseJsonObj(str)
{
    try
    {
        return JSON.parse(str);
    }
    catch (e)
    {
        return null;
    }
}

/**
 * 取文件扩展名
 * 得到的是小写扩展名，前面没有点
 * @param {string} path
 * @returns {string}
 */
function getFileExtName(path)
{
    return pathUtil.extname(path).slice(1).toLowerCase();
}

/**
 * 取文件基本名
 * 不包括扩展名，不转为小写
 * @param path
 * @returns {string}
 */
function getFileBaseName(path)
{
    return pathUtil.parse(path).name;
}

/**
 * 获取
 * @param {function} l
 */
function addProcessExitingListener(l)
{
    //已触发了，不能添加监听了
    if (isFired)
        return;
    processEvent.on("exit", l);
}

function addProcessListener(e, l)
{
    if (e === "exit")
        return;
    processEvent.on(e, l);
}

function removeProcessExitingListener(l)
{
    processEvent.removeListener("exit", l);
}

function removeProcessListener(e, l)
{
    if (e === "exit")
        return;
    processEvent.removeListener(e, l);
}

/**
 *
 * @param {boolean?} noBroad - 不再转发给父进程而产生广播，主要用于子进程收到父进程的消息后使用
 */
function fireProcessExiting(noBroad)
{
    //已触发了，不能再触发了
    if (isFired)
        return;

    //标记已触发进程退出
    isFired = true;

    //这里不得已用一下logUtil
    var logUtil = require("./logUtil");
    logUtil.info("注意：进程即将退出");
    //如果有主进程，让主进程通知其它进程也退出
    if (!noBroad && process.send)
        process.send({code:"exit"});
    //本进程发出进程退出消息
    processEvent.emit("exit");
}

/**
 *
 * @param {string} e
 * @param {*?} c
 * @param {boolean?} noBroad - 不再转发给父进程而产生广播，主要用于子进程收到父进程的消息后使用
 */
function fireProcessEvent(e, c, noBroad)
{
    if (e === "exit")
        return;

    //如果有主进程，让主进程通知其它进程也处理消息
    if (!noBroad && process.send)
        process.send({code:e, cxt:c});
    //本进程发出消息
    processEvent.emit(e, c);
}

function getExitingListenerCount()
{
    return processEvent.listenerCount("exit");
}

function getListenerCount(e)
{
    return processEvent.listenerCount(e);
}

function isProcessExiting()
{
    return isFired;
}

function setProcessInited()
{
    isInited = true;
}

function isProcessInited()
{
    return isInited;
}

////////////导出元素////////////
exports.limitString = limitString;
exports.tryParseToDataType = tryParseToDataType;
exports.validateObjectFields = validateObjectFields;
exports.getRandom = getRandom;
exports.getRandomStr = getRandomStr;
exports.getUnrepeatableRandItems = getUnrepeatableRandItems;
exports.getRepeatableRandItems = getRepeatableRandItems;
exports.getWorkerID = getWorkerID;
exports.getMD5 = getMD5;
exports.formatCapacity = formatCapacity;
exports.requireNativeLib = requireNativeLib;
exports.parseJsonObj = parseJsonObj;
exports.getFileExtName = getFileExtName;
exports.getFileBaseName = getFileBaseName;
exports.addProcessExitingListener = addProcessExitingListener;
exports.addProcessListener = addProcessListener;
exports.removeProcessExitingListener = removeProcessExitingListener;
exports.removeProcessListener = removeProcessListener;
exports.fireProcessExiting = fireProcessExiting;
exports.fireProcessEvent = fireProcessEvent;
exports.getExitingListenerCount = getExitingListenerCount;
exports.getListenerCount = getListenerCount;
exports.isProcessExiting = isProcessExiting;
exports.setProcessInited = setProcessInited;
exports.isProcessInited = isProcessInited;
exports.INT32_MAX_POSITIVE = INT32_MAX_POSITIVE;
exports.INT32_MAX_NEGTIVE = INT32_MAX_NEGTIVE;
exports.INT_MAX_POSITIVE = INT_MAX_POSITIVE;
exports.INT_MAX_NEGTIVE = INT_MAX_NEGTIVE;
exports.UINT32_MAX = UINT32_MAX;
exports.UINT16_MAX = UINT16_MAX;
exports.FLOAT_MAX = FLOAT_MAX;
exports.DOUBLE_MAX = DOUBLE_MAX;