"use strict";

////////////我的模块////////////
var appUtil = require("../libs/appUtil");

////////////我的本地模块////////////
var tea16Native = appUtil.requireNativeLib("tea16", true);

////////////模块内变量////////////
//是否开启加密
const ENABLE_ENCRYPT = false;   //TODO 正式发布开启加密

//默认密码
const default_key = [
    0x3687C5E3,
    0xB7EF3327,
    0xE3791011,
    0x84E2D3BC
];

////////////私有函数////////////
/**
 *
 * @param {Buffer} buffer
 * @param {number} offset
 * @returns {number}
 */
function readUInt32(buffer, offset)
{
    return buffer.readUInt32BE(offset);
}

/**
 *
 * @param {number} value
 * @param {Buffer} buffer
 * @param {number} offset
 */
function writeUInt32(value, buffer, offset)
{
    buffer.writeUInt32BE(value, offset);
}

function _encrypt(v, k)
{
    var v0 = v[0] >>> 0, v1 = v[1] >>> 0, sum = 0 >>> 0;
    var delta = 0x9e3779b9 >>> 0;
    var k0 = k[0] >>> 0, k1 = k[1] >>> 0, k2 = k[2] >>> 0, k3 = k[3] >>> 0;
    for (var i = 0; i < 16; i++)
    {
        sum += delta;
        v0 += ((v1 << 4) + k0) ^ (v1 + sum) ^ ((v1 >>> 5) + k1);
        v1 += ((v0 << 4) + k2) ^ (v0 + sum) ^ ((v0 >>> 5) + k3);
    }

    v[0] = v0 >>> 0;
    v[1] = v1 >>> 0;
}

function _decrypt(v, k)
{
    var v0 = v[0] >>> 0, v1 = v[1] >>> 0, sum = 0xE3779B90 >>> 0;
    var delta = 0x9e3779b9 >>> 0;
    var k0 = k[0] >>> 0, k1 = k[1] >>> 0, k2 = k[2] >>> 0, k3 = k[3] >>> 0;
    for (var i = 0; i < 16; i++)
    {
        v1 -= ((v0 << 4) + k2) ^ (v0 + sum) ^ ((v0 >>> 5) + k3);
        v0 -= ((v1 << 4) + k0) ^ (v1 + sum) ^ ((v1 >>> 5) + k1);
        sum -= delta;
    }

    v[0] = v0 >>> 0;
    v[1] = v1 >>> 0;
}

function _processLeftBytes(bytes, start, len, keys)
{
    for (var i = 0; i < len; ++i)
    {
        var index = start + i;
        var b = bytes.readUInt8(index);
        var k = (keys[i % 4] >>> 0) & 0xFF;
        b = b ^ k;
        bytes.writeUInt8(b, index);
    }
}

/**
 * 加解密统一函数
 * @param {boolean} doEncrypt
 * @param {Buffer} buffer
 * @param {number[]?} [keys=default_key]
 * @param {number?} [start=0]
 * @param {number?} [length=buffer.length-start]
 */
function doEncryptOrDecrypt(doEncrypt, buffer, keys, start, length)
{
    var bufLen = buffer.length;
    start  = Math.min(bufLen, Math.max(0, start || 0));
    length = Math.min(bufLen - start, Math.max(0,  length || bufLen));
    var offset = start;
    var count = Math.floor(length / 8);

    var v = [0, 0];
    for (var i = 0; i < count; ++i)
    {
        v[0] = readUInt32(buffer, offset);
        v[1] = readUInt32(buffer, offset + 4);

        if (doEncrypt)
            _encrypt(v, keys);
        else
            _decrypt(v, keys);

        writeUInt32(v[0], buffer, offset);
        writeUInt32(v[1], buffer, offset + 4);

        offset += 8;
    }
    _processLeftBytes(buffer, start + count * 8, length % 8, keys);
}

////////////导出函数////////////
/**
 * 加密
 * @param {Buffer} buffer - 要加密的数据
 * @param {number?} start - 开始位置
 * @param {number?} length - 处理长度
 * @param {number[]?} [keys=default_key] - 加解密密钥
 */
function encrypt(buffer, start, length, keys)
{
    if (!ENABLE_ENCRYPT)
        return;

    keys = keys || default_key;
    if (tea16Native)
        tea16Native.encrypt(buffer, keys, start, length);
    else
        doEncryptOrDecrypt(true, buffer, keys, start, length);
}

/**
 * 解密
  * @param {Buffer} buffer 要解密的数据
  * @param {number?} start - 开始位置
  * @param {number?} length - 处理长度
  * @param {number[]?} [keys=default_key] - 加解密密钥
  */
function decrypt(buffer, start, length, keys)
{
    if (!ENABLE_ENCRYPT)
        return;

    keys = keys || default_key;
    if (tea16Native)
        tea16Native.decrypt(buffer, keys, start, length);
    else
        doEncryptOrDecrypt(false, buffer, keys, start, length);
}

/**
 * 加密字符串，得到Buffer
 * @param {string} source
 * @return {Buffer} result
  */
function encryptString(source)
{
    var buf = new Buffer(source);
    encrypt(buf);
    return buf;
}

/**
 * Buffer解密成字符串
 * @param {Buffer} source
 * @param {boolean?} cloneSource - source not changed if true
 * @return {string} result
 */
function decryptString(source, cloneSource)
{
    if (cloneSource)
        source = new Buffer(source);
    decrypt(source);
    return source.toString();
}

////////////导出元素////////////
exports.encrypt = encrypt;
exports.decrypt = decrypt;
exports.encryptString = encryptString;
exports.decryptString = decryptString;

////////////说明////////////
/*
 JS里有符号数转无符号数是 N >>> 0，但仅对int32范围内的数据有效
 */