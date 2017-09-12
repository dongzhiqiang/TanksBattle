"use strict";

var IOBuffer = require("../libs/ioBuffer").IOBuffer;
var snappy = require("../libs/snappy");

module.exports = {
    setUp: function (callback) {
        console.log("===========");
        callback();
    },
    tearDown: function (callback) {
        callback();
    },
    //testFunction: function (test) {
    //    var ioBuf = new IOBuffer();
    //
    //    console.log("开始功能测试");
    //
    //    var testArr = [1,2,3,4,5,6,7,8,9,10];
    //    var testBuf = new Buffer(testArr);
    //    var testIOB = new IOBuffer(testArr);
    //
    //    var longArr = new Array(1024);
    //    for (var i = 0; i < longArr.length; ++i)
    //        longArr[i] = i % 128;
    //    var longBuf = new Buffer(longArr);
    //    var longIOB = new IOBuffer(longArr);
    //
    //    ioBuf.writeBool(true);
    //    ioBuf.writeBool(false);
    //    ioBuf.writeBool(true);
    //    ioBuf.writeBytes(testArr);
    //    ioBuf.writeBytes(testArr, 0, testArr.length);
    //    ioBuf.writeBytes(testBuf);
    //    ioBuf.writeBytes(testBuf, 0, testBuf.length);
    //    ioBuf.writeBytes(testIOB);
    //    ioBuf.writeBytes(testIOB, 0, testIOB.canReadLen());
    //    ioBuf.writeBytes(longArr);
    //    ioBuf.writeBytes(longArr, 0, longArr.length);
    //    ioBuf.writeBytes(longBuf);
    //    ioBuf.writeBytes(longBuf, 0, longBuf.length);
    //    ioBuf.writeBytes(longIOB);
    //    ioBuf.writeBytes(longIOB, 0, longIOB.canReadLen());
    //    ioBuf.writeInt8(-Math.pow(2, 7));
    //    ioBuf.writeUInt8(Math.pow(2, 8) - 1);
    //    ioBuf.writeInt16(-Math.pow(2, 15));
    //    ioBuf.writeUInt16(Math.pow(2, 16) - 1);
    //    ioBuf.writeInt24(-Math.pow(2, 23));
    //    ioBuf.writeUInt24(Math.pow(2, 24) - 1);
    //    var relativePos = ioBuf.getRelativeWritePos();
    //    ioBuf.writeInt32(-Math.pow(2, 31) + 10);
    //    ioBuf.writeInt32WithRelativePos(-Math.pow(2, 31), relativePos);
    //    relativePos = ioBuf.getRelativeWritePos();
    //    ioBuf.writeUInt32(Math.pow(2, 32) - 11);
    //    ioBuf.writeUInt32WithRelativePos(Math.pow(2, 32) - 1, relativePos);
    //    ioBuf.writeInt40(-Math.pow(2, 39));
    //    ioBuf.writeUInt40(Math.pow(2, 40) - 1);
    //    ioBuf.writeInt48(-Math.pow(2, 47));
    //    ioBuf.writeUInt48(Math.pow(2, 48) - 1);
    //    ioBuf.writeInt56(-Math.pow(2, 53));
    //    ioBuf.writeUInt56(Math.pow(2, 53));
    //    ioBuf.writeInt64(-Math.pow(2, 53));
    //    ioBuf.writeUInt64(Math.pow(2, 53));
    //    ioBuf.writeFloat(1.23);
    //    ioBuf.writeDouble(2.34);
    //    ioBuf.writeString("哈哈");
    //    ioBuf.writeOnlyString("哼哼");
    //
    //    console.log("得到的数据大小：" + ioBuf.canReadLen());
    //    console.log("得到的预留大小：" + ioBuf.capacity());
    //
    //    var ret  = ioBuf.peekBool();
    //    var ret0 = ioBuf.readBool();
    //    test.equal(ret, true);
    //    test.equal(ret, ret0);
    //    console.log(ret0);
    //
    //    ret  = ioBuf.peekBool();
    //    ret0 = ioBuf.readBool();
    //    test.equal(ret, false);
    //    test.equal(ret, ret0);
    //    console.log(ret0);
    //
    //    ret  = ioBuf.peekBool();
    //    ret0 = ioBuf.readBool();
    //    test.equal(ret, true);
    //    test.equal(ret, ret0);
    //    console.log(ret0);
    //
    //    ret  = ioBuf.peekArray(testArr.length);
    //    ret0 = ioBuf.readArray(testArr.length);
    //    test.deepEqual(ret, testArr);
    //    test.deepEqual(ret, ret0);
    //    console.log(ret0);
    //
    //    ret  = ioBuf.peekBuffer(testArr.length);
    //    ret0 = ioBuf.readBuffer(testArr.length);
    //    test.equal(ret.equals(testBuf), true);
    //    test.equal(ret.equals(ret0), true);
    //    console.log(ret0);
    //
    //    var tempArr = new Array(testBuf.length);
    //    ioBuf.readBytes(tempArr);
    //    test.deepEqual(tempArr, testArr);
    //    console.log(tempArr);
    //
    //    tempArr = new Array(testBuf.length);
    //    ioBuf.readBytes(tempArr, 0, 5);
    //    ioBuf.readBytes(tempArr, 5, testBuf.length - 5);
    //    test.deepEqual(tempArr, testArr);
    //    console.log(tempArr);
    //
    //    var tempBuf = new Buffer(testIOB.canReadLen());
    //    ioBuf.readBytes(tempBuf);
    //    test.equal(tempBuf.equals(testBuf), true);
    //    console.log(tempBuf);
    //
    //    tempBuf = new Buffer(testIOB.canReadLen());
    //    ioBuf.readBytes(tempBuf, 0, 5);
    //    ioBuf.readBytes(tempBuf, 5, testIOB.canReadLen() - 5);
    //    test.equal(tempBuf.equals(testBuf), true);
    //    console.log(tempBuf);
    //
    //    var tempIOB = new IOBuffer(longArr.length);
    //    ioBuf.readBytes(tempIOB);
    //    test.equal(tempIOB.equals(longIOB), true);
    //    console.log(tempIOB.buffer());
    //
    //    tempIOB = new IOBuffer(longArr.length);
    //    ioBuf.readBytes(tempIOB, 0, 5);
    //    ioBuf.readBytes(tempIOB, 0, longArr.length - 5);
    //    test.equal(tempIOB.equals(longIOB), true);
    //    console.log(tempIOB.buffer());
    //
    //    tempBuf = new Buffer(longBuf.length);
    //    ioBuf.readBytes(tempBuf);
    //    test.equal(tempBuf.equals(longBuf), true);
    //    console.log(tempBuf);
    //
    //    tempBuf = new Buffer(longBuf.length);
    //    ioBuf.readBytes(tempBuf);
    //    test.equal(tempBuf.equals(longBuf), true);
    //    console.log(tempBuf);
    //
    //    tempIOB = new IOBuffer(longIOB.canReadLen());
    //    ioBuf.readBytes(tempIOB);
    //    test.equal(tempIOB.equals(longIOB), true);
    //    console.log(tempIOB.buffer());
    //
    //    tempIOB = new IOBuffer(longIOB.canReadLen());
    //    ioBuf.readBytes(tempIOB);
    //    test.equal(tempIOB.equals(longIOB), true);
    //    console.log(tempIOB.buffer());
    //
    //    ioBuf.tidy(0);
    //    console.log("===========");
    //
    //    ret = ioBuf.peekInt8();
    //    ret0 = ioBuf.readInt8();
    //    test.equal(ret, -Math.pow(2, 7));
    //    test.equal(ret, ret0);
    //    console.log("int8, ref:" + (-Math.pow(2, 7)) + ",val:" + ret0);
    //
    //    ret = ioBuf.peekUInt8();
    //    ret0 = ioBuf.readUInt8();
    //    test.equal(ret, Math.pow(2, 8) - 1);
    //    test.equal(ret, ret0);
    //    console.log("uint8, ref:" + (Math.pow(2, 8)-1) + ",val:" + ret0);
    //
    //    ret = ioBuf.peekInt16();
    //    ret0 = ioBuf.readInt16();
    //    test.equal(ret, -Math.pow(2, 15));
    //    test.equal(ret, ret0);
    //    console.log("int16, ref:" + (-Math.pow(2, 15)) + ",val:" + ret0);
    //
    //    ret = ioBuf.peekUInt16();
    //    ret0 = ioBuf.readUInt16();
    //    test.equal(ret, Math.pow(2, 16) - 1);
    //    test.equal(ret, ret0);
    //    console.log("uint16, ref:" + (Math.pow(2, 16)-1) + ",val:" + ret0);
    //
    //    ret = ioBuf.peekInt24();
    //    ret0 = ioBuf.readInt24();
    //    test.equal(ret, -Math.pow(2, 23));
    //    test.equal(ret, ret0);
    //    console.log("int24, ref:" + (-Math.pow(2, 23)) + ",val:" + ret0);
    //
    //    ret = ioBuf.peekUInt24();
    //    ret0 = ioBuf.readUInt24();
    //    test.equal(ret, Math.pow(2, 24) - 1);
    //    test.equal(ret, ret0);
    //    console.log("uint24, ref:" + (Math.pow(2, 24)-1) + ",val:" + ret0);
    //
    //    ret = ioBuf.peekInt32();
    //    ret0 = ioBuf.readInt32();
    //    test.equal(ret, -Math.pow(2, 31));
    //    test.equal(ret, ret0);
    //    console.log("int32, ref:" + (-Math.pow(2, 31)) + ",val:" + ret0);
    //
    //    ret = ioBuf.peekUInt32();
    //    ret0 = ioBuf.readUInt32();
    //    test.equal(ret, Math.pow(2, 32) - 1);
    //    test.equal(ret, ret0);
    //    console.log("uint32, ref:" + (Math.pow(2, 32)-1) + ",val:" + ret0);
    //
    //    ret = ioBuf.peekInt40();
    //    ret0 = ioBuf.readInt40();
    //    test.equal(ret, -Math.pow(2, 39));
    //    test.equal(ret, ret0);
    //    console.log("int40, ref:" + (-Math.pow(2, 39)) + ",val:" + ret0);
    //
    //    ret = ioBuf.peekUInt40();
    //    ret0 = ioBuf.readUInt40();
    //    test.equal(ret, Math.pow(2, 40) - 1);
    //    test.equal(ret, ret0);
    //    console.log("uint40, ref:" + (Math.pow(2, 40) - 1) + ",val:" + ret0);
    //
    //    ret = ioBuf.peekInt48();
    //    ret0 = ioBuf.readInt48();
    //    test.equal(ret, -Math.pow(2, 47));
    //    test.equal(ret, ret0);
    //    console.log("int48, ref:" + (-Math.pow(2, 47)) + ",val:" + ret0);
    //
    //    ret = ioBuf.peekUInt48();
    //    ret0 = ioBuf.readUInt48();
    //    test.equal(ret, Math.pow(2, 48) - 1);
    //    test.equal(ret, ret0);
    //    console.log("uint48, ref:" + (Math.pow(2, 48) - 1) + ",val:" + ret0);
    //
    //    ret = ioBuf.peekInt56();
    //    ret0 = ioBuf.readInt56();
    //    test.equal(ret, -Math.pow(2, 53));
    //    test.equal(ret, ret0);
    //    console.log("int56, ref:" + (-Math.pow(2, 53)) + ",val:" + ret0);
    //
    //    ret = ioBuf.peekUInt56();
    //    ret0 = ioBuf.readUInt56();
    //    test.equal(ret, Math.pow(2, 53));
    //    test.equal(ret, ret0);
    //    console.log("uint56, ref:" + (Math.pow(2, 53)) + ",val:" + ret0);
    //
    //    ret = ioBuf.peekInt64();
    //    ret0 = ioBuf.readInt64();
    //    test.equal(ret, -Math.pow(2, 53));
    //    test.equal(ret, ret0);
    //    console.log("int64, ref:" + (-Math.pow(2, 53)) + ",val:" + ret0);
    //
    //    ret = ioBuf.peekUInt64();
    //    ret0 = ioBuf.readUInt64();
    //    test.equal(ret, Math.pow(2, 53));
    //    test.equal(ret, ret0);
    //    console.log("uint64, ref:" + (Math.pow(2, 53)) + ",val:" + ret0);
    //
    //    ret = ioBuf.peekFloat().toFixed(2);
    //    ret0 = ioBuf.readFloat().toFixed(2);
    //    test.equal(ret, 1.23);
    //    test.equal(ret, ret0);
    //    console.log(ret0);
    //
    //    ret = ioBuf.peekDouble();
    //    ret0 = ioBuf.readDouble();
    //    test.equal(ret, 2.34);
    //    test.equal(ret, ret0);
    //    console.log(ret0);
    //
    //    ret = ioBuf.readString();
    //    test.equal(ret, "哈哈");
    //    console.log(ret);
    //
    //    ret = ioBuf.readOnlyString(6);
    //    test.equal(ret, "哼哼");
    //    console.log(ret);
    //
    //    test.equal(ioBuf.canReadLen(), 0);
    //    console.log("剩余的数据大小：" + ioBuf.canReadLen());
    //    console.log("得到的预留大小：" + ioBuf.capacity());
    //
    //    ioBuf.clear();
    //
    //    test.equal(ioBuf.canReadLen(), 0);
    //    console.log("剩余的数据大小：" + ioBuf.canReadLen());
    //    console.log("得到的预留大小：" + ioBuf.capacity());
    //
    //    console.log("结束功能测试");
    //
    //    test.done();
    //},
    //testPerformance: function (test)
    //{
    //    var repeatCnt = 1000000;
    //    var useArr = new Array(256);
    //    for (let i = 0; i < useArr.length; ++i)
    //        useArr[i] = i;
    //    var recvRet = {a:null, b:null, c:null, d:null, e:null, f:null, g:null, h:null};
    //
    //    console.time("string");
    //    for (let i = 0; i < repeatCnt; ++i)
    //    {
    //        var str = "{";
    //        str += "\"a\":" + true + ",";
    //        str += "\"b\":" + 123 + ",";
    //        str += "\"c\":" + 1.23 + ",";
    //        str += "\"d\":\"" + "123" + "\",";
    //        str += "\"e\":" + true + ",";
    //        str += "\"f\":" + 123 + ",";
    //        str += "\"g\":" + 1.23 + ",";
    //        str += "\"h\":[" + useArr.join(",") + "]";
    //        str += "}";
    //        var objFromStr = JSON.parse(str);
    //        recvRet.a = objFromStr.a;
    //        recvRet.b = objFromStr.b;
    //        recvRet.c = objFromStr.c;
    //        recvRet.d = objFromStr.d;
    //        recvRet.e = objFromStr.e;
    //        recvRet.f = objFromStr.f;
    //        recvRet.g = objFromStr.g;
    //        recvRet.h = objFromStr.h;
    //    }
    //    console.timeEnd("string");
    //    console.log("最后接收值：" + JSON.stringify(recvRet));
    //    console.log("最后值：" + str);
    //    console.log("最后值长度：" + str.length);
    //
    //    console.time("json");
    //    for (let i = 0; i < repeatCnt; ++i)
    //    {
    //        var json = {a:true, b:123, c:1.23, d:"123", e:true, f:123, g:1.23, h:useArr};
    //        var jsonStr = JSON.stringify(json);
    //        var objFromJsonStr = JSON.parse(jsonStr);
    //        let ret = objFromJsonStr.a;
    //        recvRet.a = objFromStr.a;
    //        recvRet.b = objFromStr.b;
    //        recvRet.c = objFromStr.c;
    //        recvRet.d = objFromStr.d;
    //        recvRet.e = objFromStr.e;
    //        recvRet.f = objFromStr.f;
    //        recvRet.g = objFromStr.g;
    //        recvRet.h = objFromStr.h;
    //    }
    //    console.timeEnd("json");
    //    console.log("最后接收值：" + JSON.stringify(recvRet));
    //    console.log("最后值：" + jsonStr);
    //    console.log("最后值长度：" + jsonStr.length);
    //
    //    console.time("buffer");
    //    for (let i = 0; i < repeatCnt; ++i)
    //    {
    //        var buf = new Buffer(33 + useArr.length);
    //        buf.writeInt8(1, 0);
    //        buf.writeInt32BE(123, 1);
    //        buf.writeDoubleBE(1.23, 5);
    //        buf.writeInt32BE(4, 13);
    //        buf.write("123", 17);
    //        buf.writeInt8(1, 20);
    //        buf.writeInt32BE(123, 21);
    //        buf.writeDoubleBE(1.23, 25);
    //        new Buffer(useArr).copy(buf, 33);
    //        let ret = buf.readInt8(0);
    //        recvRet.a = buf.readInt32BE(1);
    //        recvRet.b = buf.readDoubleBE(5);
    //        recvRet.c = buf.readInt32BE(13);
    //        recvRet.d = buf.slice(17, 3).toString();
    //        recvRet.e = buf.readInt8(20);
    //        recvRet.f = buf.readInt32BE(21);
    //        recvRet.g = buf.readDoubleBE(25);
    //        recvRet.h = IOBuffer.bufferToArray(buf, 33, useArr.length);
    //    }
    //    console.timeEnd("buffer");
    //    console.log("最后接收值：" + JSON.stringify(recvRet));
    //    console.log("最后值：" + buf);
    //    console.log("最后值长度：" + buf.length);
    //
    //    console.time("bufferFor");
    //    for (let i = 0; i < repeatCnt; ++i)
    //    {
    //        var buf2 = new Buffer(33 + useArr.length);
    //        buf2.writeInt8(1, 0);
    //        buf2.writeInt32BE(123, 1);
    //        buf2.writeDoubleBE(1.23, 5);
    //        buf2.writeInt32BE(4, 13);
    //        buf2.write("123", 17);
    //        buf2.writeInt8(1, 20);
    //        buf2.writeInt32BE(123, 21);
    //        buf2.writeDoubleBE(1.23, 25);
    //        for (var k = 0; k < useArr.length; ++k)
    //            buf2[33 + k] = useArr[k];
    //        let ret = buf.readInt8(0);
    //        recvRet.a = buf.readInt32BE(1);
    //        recvRet.b = buf.readDoubleBE(5);
    //        recvRet.c = buf.readInt32BE(13);
    //        recvRet.d = buf.slice(17, 3).toString();
    //        recvRet.e = buf.readInt8(20);
    //        recvRet.f = buf.readInt32BE(21);
    //        recvRet.g = buf.readDoubleBE(25);
    //        recvRet.h = IOBuffer.bufferToArray(buf, 33, useArr.length);
    //    }
    //    console.timeEnd("bufferFor");
    //    console.log("最后接收值：" + JSON.stringify(recvRet));
    //    console.log("最后值：" + buf);
    //    console.log("最后值长度：" + buf.length);
    //
    //    console.time("iobuffer");
    //    for (let i = 0; i < repeatCnt; ++i)
    //    {
    //        var ioBuf = new IOBuffer(33 + useArr.length);
    //        ioBuf.writeBool(true);
    //        ioBuf.writeInt32(123);
    //        ioBuf.writeDouble(1.23);
    //        ioBuf.writeString("123");
    //        ioBuf.writeBool(true);
    //        ioBuf.writeInt32(123);
    //        ioBuf.writeDouble(1.23);
    //        ioBuf.writeBytes(useArr);
    //        recvRet.h = ioBuf.canReadLen();
    //        let ret = ioBuf.readBool();
    //        recvRet.a = ioBuf.readInt32();
    //        recvRet.b = ioBuf.readDouble();
    //        recvRet.c = ioBuf.readString();
    //        recvRet.d = ioBuf.readBool();
    //        recvRet.e = ioBuf.readInt32();
    //        recvRet.f = ioBuf.readDouble();
    //        recvRet.g = ioBuf.readArray(useArr.length);
    //    }
    //    console.timeEnd("iobuffer");
    //    console.log("最后接收值：" + JSON.stringify(recvRet));
    //    console.log("最后值：" + ioBuf.getReadableRef());
    //    console.log("最后值长度：" + ioBuf.getReadableRef().length);
    //
    //    test.done();
    //},
    //testPerformance2: function (test)
    //{
    //    var repeatCnt = 1000000;
    //    var useArr = new Array(1024);
    //    for (let i = 0; i < useArr.length; ++i)
    //        useArr[i] = i;
    //    var recvRet = null;
    //
    //    console.time("iobuffer");
    //    for (let i = 0; i < repeatCnt; ++i)
    //    {
    //        var ioBuf = new IOBuffer(useArr.length);
    //        ioBuf.writeBytes(useArr);
    //        recvRet = ioBuf.readArray(useArr.length);
    //    }
    //    console.timeEnd("iobuffer");
    //    console.log("最后接收值：" + JSON.stringify(recvRet));
    //    console.log("最后值：" + ioBuf.getReadableRef());
    //    console.log("最后值长度：" + ioBuf.getReadableRef().length);
    //
    //    useArr = new Buffer(useArr);
    //
    //    console.time("iobuffer2");
    //    for (let i = 0; i < repeatCnt; ++i)
    //    {
    //        var ioBuf2 = new IOBuffer(useArr.length);
    //        ioBuf2.writeBytes(useArr);
    //        recvRet = ioBuf2.readBuffer(useArr.length);
    //    }
    //    console.timeEnd("iobuffer2");
    //    console.log("最后接收值：" + JSON.stringify(recvRet));
    //    console.log("最后值：" + ioBuf2.getReadableRef());
    //    console.log("最后值长度：" + ioBuf2.getReadableRef().length);
    //
    //    test.done();
    //},
    //testPerformance3: function (test)
    //{
    //    var repeatCnt = 1000000;
    //    var useArr = new Array(256);
    //    for (let i = 0; i < useArr.length; ++i)
    //        useArr[i] = i;
    //    var recvRet = {a:null, b:null, c:null, d:null, e:null, f:null, g:null, h:null};
    //
    //    console.time("iobuffer");
    //    var ioBuf = new IOBuffer();
    //    for (let i = 0; i < repeatCnt; ++i)
    //    {
    //        ioBuf.writeBool(true);
    //        ioBuf.writeInt32(123);
    //        ioBuf.writeDouble(1.23);
    //        ioBuf.writeString("123");
    //        ioBuf.writeBool(true);
    //        ioBuf.writeInt32(123);
    //        ioBuf.writeDouble(1.23);
    //        ioBuf.writeBytes(useArr);
    //    }
    //    console.timeEnd("iobuffer");
    //    console.log("缓冲容量：" + ioBuf.capacity());
    //    console.log("数据长度：" + ioBuf.canReadLen());
    //
    //    test.done();
    //},
    testSnappy: function(test)
    {
        var obj = {propString:"广州", "propNumber":123456, "propArray":["1","2","3"], propEnum:2};
        var msg = JSON.stringify(obj);

        console.log("压缩功能测试1开始");
        var buf = snappy.compress(msg);
        var msg2 = snappy.uncompress(buf);
        if (snappy.isValid(buf))
            console.log("压缩内容有效");
        else
            console.log("压缩内容无效");
        if (msg === msg2)
            console.log("压缩解压正确");
        else
            console.log("压缩解压错误");
        console.log("压缩功能测试1结束\r\n");

        console.log("压缩功能测试2开始");
        var srcBuf = new Buffer(msg);
        buf = snappy.compress(srcBuf);
        var desBuf = snappy.uncompress(buf, true);
        if (snappy.isValid(buf))
            console.log("压缩内容有效");
        else
            console.log("压缩内容无效");
        if (srcBuf.equals(desBuf))
            console.log("压缩解压正确");
        else
            console.log("压缩解压错误");
        console.log("压缩功能测试2结束\r\n");

        console.log("内存占用：" + JSON.stringify(process.memoryUsage()) + "\r\n");

        console.log("压缩性能测试开始");
        var count = 1000000;
        console.time("profile");
        for (var i = 0; i < count; ++i)
        {
            buf = snappy.compress(msg);
            msg2 = snappy.uncompress(buf);
        }
        console.timeEnd("profile");
        console.log("压缩前内容：" + msg);
        console.log("压缩后内容：" + buf.toString());
        console.log("解压后内容：" + msg2);
        console.log("压缩前长度：" + Buffer.byteLength(msg));
        console.log("压缩后长度：" + buf.length);
        console.log("压缩性能测试结束\r\n");

        console.log("内存占用：" + JSON.stringify(process.memoryUsage()) + "\r\n");

        test.done();
    }
};