"use strict";

////////////我的模块////////////
var appUtil = require("../libs/appUtil");

////////////我的本地模块////////////
var snappyNative = appUtil.requireNativeLib("snappy", true);

////////////导出函数////////////
/**
 * 压缩数据
 * @param {string|Buffer} buffer - 可传入字符串或Buffer
 * @returns {Buffer} 返回压缩后的Buffer
 */
function compress(buffer)
{
    return snappyNative.compress(buffer);
}

/**
 * 解压数据
 * @param {Buffer} buffer - 压缩后的Buffer
 * @param {boolean?} [asBuffer=false] - 是否把解压后的数据转成字符串
 * @returns {string|Buffer} 如果asBuffer为true，则返回Buffer，否则返回String
 */
function uncompress(buffer, asBuffer)
{
    return snappyNative.uncompress(buffer, asBuffer);
}

/**
 *
 * @param {Buffer} buffer
 * @returns {boolean}
 */
function isValid(buffer)
{
    return snappyNative.isValid(buffer);
}

////////////导出元素////////////
exports.compress = compress;
exports.uncompress = uncompress;
exports.isValid = isValid;