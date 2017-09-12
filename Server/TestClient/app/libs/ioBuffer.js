"use strict";

/**
 * IO缓冲类，就是可以一边写一边读
 *
 * 经测试，以下写法太慢，不要用
 * return Array.prototype.slice.call(buf, off, len);
 *
 * 经测试，复制到Buffer以下原则
 * 如果Array复制到Buffer的中间一段，用for更好，而不是var buf = new Buffer(arr)再buf.copy
 * 如果Array直接转成Buffer，用var buf = new Buffer(arr)更好
 * 如果Buffer复制到Buffer的中间一段，用buf.copy更好
 * 如果Buffer复制到一个完整的Buffer，用var buf = new Buffer(otherBuf)更好
 */

////////////模块内变量////////////
const DEFAULT_INITAL_SIZE = 1024;
const DEFAULT_MAX_EXCESS  = 1024 * 256;
const BUFFER_GROW_FACTOR  = 0.5;    //容量增长因子，经测试1比0.5效率高那么一点点，但最终内存占用更多一点

////////////导出类////////////
class IOBuffer
{
    /**
     *
     * @param {(Buffer|number[]|string|number)?} initialBuf - 初始缓冲区，当这个值是number时，跳过初始缓冲区，initialBuf表示initialLen，则initialLen表示maxExcess
     * @param {number?} initialLen - 初始缓冲区大小，字节，在未指定初始Buf或调用clear时用到
     * @param {number?} maxExcess - 如果空闲空间太多，就考虑新建buffer，回收空间
     */
    constructor(initialBuf, initialLen, maxExcess)
    {
        this._readPos  = 0;
        this._autoTidy = true;

        var buf = IOBuffer.ensureBufferType(initialBuf);
        if (buf)
        {
            this._initialLen = initialLen || DEFAULT_INITAL_SIZE;
            this._maxExcess = maxExcess || DEFAULT_MAX_EXCESS;
            this._buffer =  buf;
            this._writePos = buf.length;
        }
        else
        {
            //初始化缓冲区被跳过了
            this._initialLen = initialBuf || DEFAULT_INITAL_SIZE;
            this._maxExcess = initialLen || DEFAULT_MAX_EXCESS;
            this._buffer = new Buffer(this._initialLen);
            this._writePos = 0;
        }
    }

    /**
     * 把Array、String转为Buffer
     * @param {Buffer|number[]|string} buf
     * @returns {Buffer}
     */
    static ensureBufferType(buf)
    {
        if (Buffer.isBuffer(buf))
            return buf;
        else if (Array.isArray(buf) || typeof buf === "string")
            return new Buffer(buf);
        else
            return null;
    }

    /**
     * 把Buffer转为整数Array
     * @param {Buffer} buf
     * @param {number?} [off=0]
     * @param {number?} [len=buf.length-off]
     * @returns {number[]}
     */
    static bufferToArray(buf, off, len)
    {
        if (off === null || off === undefined)
            off = 0;
        if (len === null || len === undefined)
            len = buf.length - off;
        var ret = new Array(len);
        for (var i = 0; i < len; ++i)
            ret[i] = buf[off++];
        return ret;
    }

    /**
     * 计算检验和
     * 本来想把这个运算放到C++插件，发现更慢，因为C++和JS之间传输要性能消耗
     * @param {IOBuffer|Buffer|number[]} buf
     * @param {number?} [off=0] - 源缓冲对象上的偏移，对于IOBuffer，也是整个内部缓存的位置
     * @param {number?} [len=buf.length-off]
     * @returns {number} 返回的是int32
     */
    static calcCheckSum(buf, off, len)
    {
        if (buf instanceof IOBuffer)
            buf = buf._buffer;

        off = off || 0;
        len = len || buf.length - off;
        //js的移位只支持int32，所以这里只会在int32范围
        var hash = 0;
        for (var i = off, end = off + len; i < end; ++i) {
            hash = (hash << 7) ^ buf[i];
        }
        return hash;
    }

    /**
     * 跟另一个IOBuffer是否相等
     * @param {IOBuffer} other
     */
    equals(other)
    {
        return this._buffer.equals(other._buffer);
    }

    toString()
    {
        return "{buffer:0x" + this._buffer.toString("hex") + ", readPos:" + this._readPos + ", writePos:" + this._writePos + "}";
    }

    /**
     * 重置缓冲区大小为默认，重置读写指针
     */
    clear()
    {
        if (this._buffer.length !== this._initialLen)
            this._buffer = new Buffer(this._initialLen);
        this._readPos = 0;
        this._writePos = 0;
    }

    /**
     * 由于发送网络数据会引用Buffer，不能覆盖写过的数据，不然发送出去可能是被修改的数据，所以要指定是不是允许自动整理数据
     * @param {boolean} b
     */
    setAutoTidy(b) {
        this._autoTidy = !!b;
    }

    /**
     * 整理空间，如果空闲大小没超过阈值，那就只移动数据，如果超过了，那就重新申请一个小的空间
     * readMark会重置为-1
     * 注意：如果使用了mark或外部获取了readPos/writePos，就不要使用这个函数
     * @param {number?} [wantWriteLen=0] - 整理空间时考虑还要写入，所以除了保留原来未读的内容，还要预留空间用来接下来的写入
     */
    tidy(wantWriteLen)
    {
        wantWriteLen = Math.max(0, wantWriteLen || 0);

        var oldReadPos = this.getReadPos();
        if (oldReadPos > 0)
        {
            var oldWritePos = this.getWritePos();
            var canReadLen = this.canReadLen();
            var oldCapacity = this.capacity();

            //空闲太多？重新申请空间吧
            var excess = oldCapacity - canReadLen - wantWriteLen;
            if (excess > this._maxExcess)
            {
                var newCapacity = Math.ceil((Math.max(this._initialLen, wantWriteLen + canReadLen)) / 1024) * 1024; //保证是1KB倍数，且保证不小于初始大小
                var newBuffer = new Buffer(newCapacity);
                this._buffer.copy(newBuffer, 0, oldReadPos, oldWritePos);
                this._buffer = newBuffer;
            }
            else
            {
                this._buffer.copy(this._buffer, 0, oldReadPos, oldWritePos);
            }

            this._readPos = 0;
            this._writePos = canReadLen;
        }
    }

    /**
     * 保证有多少可写空间
     * @param {number} wantWriteLen
     */
    ensure(wantWriteLen)
    {
        var canWriteLen = this.canWriteLen();
        if (wantWriteLen <= canWriteLen)
            return;

        var oldReadPos = this.getReadPos();
        //移动空间后可以腾出空间来用，就用整理
        //oldReadPos + canWriteLen就是空闲的空间
        //如果设置不了自动整理数据，就不要自动整理
        if (this._autoTidy && oldReadPos + canWriteLen >= wantWriteLen)
        {
            this.tidy(wantWriteLen);
            return;
        }

        var oldCapacity = this.capacity();
        var newCapacity = Math.ceil((Math.max(wantWriteLen, oldCapacity * BUFFER_GROW_FACTOR) + oldCapacity) / 1024) * 1024; //每次增长0.5倍，并保证是1KB倍数
        var newBuffer = new Buffer(newCapacity);
        var oldWritePos = this.getWritePos();
        //这里只复制写入且未读取数据
        this._buffer.copy(newBuffer, 0, oldReadPos, oldWritePos);
        this._buffer = newBuffer;
        this._readPos = 0;
        this._writePos = oldWritePos - oldReadPos;
    }

    /**
     *
     * @return {number}
     */
    capacity()
    {
        return this._buffer.length;
    }

    /**
     *
     * @return {Buffer}
     */
    buffer()
    {
        return this._buffer;
    }

    /**
     *
     * @return {number}
     */
    canReadLen()
    {
        return this._writePos - this._readPos;
    }

    /**
     *
     * @return {number}
     */
    canWriteLen()
    {
        return this._buffer.length - this._writePos;
    }

    /**
     *
     * @return {number}
     */
    getReadPos()
    {
        return this._readPos;
    }

    /**
     *
     * @return {number}
     */
    getWritePos()
    {
        return this._writePos;
    }

    /**
     * 获取相对于读位置的写位置
     * 主要用于修改已写未读的数据
     * 注意：这个值仅当一直写而不读有效，如果读了数据，再写就会破坏相对位置
     */
    getRelativeWritePos()
    {
        return this._writePos - this._readPos;
    }

    /**
     * 获取可读的数据缓存段引用，这里是引用，也就是说，改变其中一个，会影响另一个
     * @return {Buffer}
     */
    getReadableRef()
    {
        return this._buffer.slice(this._readPos, this._writePos);
    }

    /**
     * 检查可读字节够不够n
     * @param {number} n - 字节数
     * @return {boolean}
     */
    checkReadable(n)
    {
        return this.canReadLen() >= n;
    }

    /**
     * 读取一字节的bool
     * @return {boolean|null} 如果字节不够，那就返回null
     */
    readBool()
    {
        if (!this.checkReadable(1))
            return null;

        var ret = !!this._buffer.readInt8(this._readPos);
        ++this._readPos;
        return ret;
    }

    /**
     * 读取一字节的bool，读取指针不移动
     * @return {boolean|null} 如果字节不够，那就返回null
     */
    peekBool()
    {
        if (!this.checkReadable(1))
            return null;

        return !!this._buffer.readInt8(this._readPos);
    }

    /**
     * 读取len字节，写入到目标IOBuffer、Buffer或Array
     * @param {IOBuffer|Buffer|Array} buf - 目标，可以是IOBuffer、Buffer或Array
     * @param {number?} [off=0] - 在目标上的偏移，对于IOBuffer，这个值无效
     * @param {number?} [len=buf.length-off] - 读取的字节数，不填的话，就是尽量把目标填满
     * @return {boolean} 如果字节不够，那就返回false，否则返回true
     */
    readBytes(buf, off, len)
    {
        if (buf instanceof IOBuffer) {
            let bufLen = buf.canWriteLen();
            len = len || bufLen;
            let start = this._readPos;
            let end = start + len;
            if (!this.checkReadable(len))
                return false;
            if (bufLen < len)
                return false;

            this._buffer.copy(buf._buffer, buf._writePos, start, end);
            buf._writePos += len;

            this._readPos = end;
            return true;
        }
        else {
            let bufLen = buf.length;
            off = off || 0;
            len = len || bufLen - off;
            let start = this._readPos;
            let end = start + len;
            if (!this.checkReadable(len))
                return false;
            if (bufLen < off + len)
                return false;

            if (Buffer.isBuffer(buf))
            {
                this._buffer.copy(buf, off, start, end);
            }
            else
            {
                var buffer = this._buffer;
                for (var i = 0; i < len; ++i)
                    buf[off + i] = buffer[start + i];
            }

            this._readPos = end;
            return true;
        }
    }

    /**
     * 读取len字节，返回Buffer
     * @param {number} len
     * @return {Buffer|null} 如果字节不够，那就返回null
     */
    readBuffer(len)
    {
        if (!this.checkReadable(len))
            return null;

        var ret = new Buffer(len);
        var start = this._readPos;
        var end = start + len;
        this._buffer.copy(ret, 0, start, end);
        this._readPos = end;
        return ret;
    }

    /**
     * 读取len字节，返回Buffer，读取指针不移动
     * @param {number} len
     * @return {Buffer|null} 如果字节不够，那就返回null
     */
    peekBuffer(len)
    {
        if (!this.checkReadable(len))
            return null;

        var ret = new Buffer(len);
        var start = this._readPos;
        var end = start + len;
        this._buffer.copy(ret, 0, start, end);
        return ret;
    }

    /**
     * 读取len字节，返回Array
     * @param {number} len
     * @return {number[]|null} 如果字节不够，那就返回null
     */
    readArray(len)
    {
        if (!this.checkReadable(len))
            return null;

        var buffer = this._buffer;
        var ret = new Array(len);
        for (var i = 0, j = this._readPos; i < len; ++i, ++j)
            ret[i] = buffer[j];
        this._readPos += len;
        return ret;
    }

    /**
     * 读取len字节，返回Array，读取指针不移动
     * @param {number} len
     * @return {number[]|null} 如果字节不够，那就返回null
     */
    peekArray(len)
    {
        if (!this.checkReadable(len))
            return null;

        var buffer = this._buffer;
        var ret = new Array(len);
        for (var i = 0, j = this._readPos; i < len; ++i, ++j)
            ret[i] = buffer[j];
        return ret;
    }

    /**
     * 读取int8
     * @return {number|null} 如果字节不够，那就返回null
     */
    readInt8()
    {
        if (!this.checkReadable(1))
            return null;

        var ret = this._buffer.readInt8(this._readPos);
        ++this._readPos;
        return ret;
    }

    /**
     * 读取int8，读取指针不移动
     * @return {number|null} 如果字节不够，那就返回null
     */
    peekInt8()
    {
        if (!this.checkReadable(1))
            return null;

        return this._buffer.readInt8(this._readPos);
    }

    /**
     * 读取uint8
     * @return {number|null} 如果字节不够，那就返回null
     */
    readUInt8()
    {
        if (!this.checkReadable(1))
            return null;

        var ret = this._buffer.readUInt8(this._readPos);
        ++this._readPos;
        return ret;
    }

    /**
     * 读取uint8，读取指针不移动
     * @return {number|null} 如果字节不够，那就返回null
     */
    peekUInt8()
    {
        if (!this.checkReadable(1))
            return null;

        return this._buffer.readUInt8(this._readPos);
    }

    /**
     * 读取int16
     * @returns {number|null} 如果字节不够，返回null
     */
    readInt16()
    {
        if (!this.checkReadable(2))
            return null;

        var ret = this._buffer.readInt16BE(this._readPos);
        this._readPos += 2;
        return ret;
    }

    /**
     * 读取int16，读取指针不移动
     * @returns {number|null} 如果字节不够，返回null
     */
    peekInt16()
    {
        if (!this.checkReadable(2))
            return null;

        return this._buffer.readInt16BE(this._readPos);
    }

    /**
     * 读取uint16
     * @returns {number|null} 如果字节不够，返回null
     */
    readUInt16()
    {
        if (!this.checkReadable(2))
            return null;

        var ret = this._buffer.readUInt16BE(this._readPos);
        this._readPos += 2;
        return ret;
    }

    /**
     * 读取uint16，读取指针不移动
     * @returns {number|null} 如果字节不够，返回null
     */
    peekUInt16()
    {
        if (!this.checkReadable(2))
            return null;

        return this._buffer.readUInt16BE(this._readPos);
    }

    /**
     * 读取int24
     * @returns {number|null} 如果字节不够，返回null
     */
    readInt24()
    {
        if (!this.checkReadable(3))
            return null;

        var high = this._buffer.readInt8(this._readPos);
        var low = this._buffer.readUInt16BE(this._readPos + 1);
        this._readPos += 3;
        return high * 0x10000 + low;
    }

    /**
     * 读取int24，读取指针不移动
     * @returns {number|null} 如果字节不够，返回null
     */
    peekInt24()
    {
        if (!this.checkReadable(3))
            return null;

        var high = this._buffer.readInt8(this._readPos);
        var low = this._buffer.readUInt16BE(this._readPos + 1);
        return high * 0x10000 + low;
    }

    /**
     * 读取uint24
     * @returns {number|null} 如果字节不够，返回null
     */
    readUInt24()
    {
        if (!this.checkReadable(3))
            return null;

        var high = this._buffer.readUInt8(this._readPos);
        var low = this._buffer.readUInt16BE(this._readPos + 1);
        this._readPos += 3;
        return high * 0x10000 + low;
    }

    /**
     * 读取uint24，读取指针不移动
     * @returns {number|null} 如果字节不够，返回null
     */
    peekUInt24()
    {
        if (!this.checkReadable(3))
            return null;

        var high = this._buffer.readUInt8(this._readPos);
        var low = this._buffer.readUInt16BE(this._readPos + 1);
        return high * 0x10000 + low;
    }

    /**
     * 读取int32
     * @returns {number|null} 如果字节不够，返回null
     */
    readInt32()
    {
        if (!this.checkReadable(4))
            return null;

        var ret = this._buffer.readInt32BE(this._readPos);
        this._readPos += 4;
        return ret;
    }

    /**
     * 读取int32，读取指针不移动
     * @returns {number|null} 如果字节不够，返回null
     */
    peekInt32()
    {
        if (!this.checkReadable(4))
            return null;

        return this._buffer.readInt32BE(this._readPos);
    }

    /**
     * 读取uint32
     * @returns {number|null} 如果字节不够，返回null
     */
    readUInt32()
    {
        if (!this.checkReadable(4))
            return null;

        var ret = this._buffer.readUInt32BE(this._readPos);
        this._readPos += 4;
        return ret;
    }

    /**
     * 读取uint32，读取指针不移动
     * @returns {number|null} 如果字节不够，返回null
     */
    peekUInt32()
    {
        if (!this.checkReadable(4))
            return null;

        return this._buffer.readUInt32BE(this._readPos);
    }

    /**
     * 读取int40
     * @returns {number|null} 如果字节不够，返回null
     */
    readInt40()
    {
        if (!this.checkReadable(5))
            return null;

        var high = this._buffer.readInt8(this._readPos);
        var low = this._buffer.readUInt32BE(this._readPos + 1);
        this._readPos += 5;
        return high * 0x100000000 + low;
    }

    /**
     * 读取int40，读取指针不移动
     * @returns {number|null} 如果字节不够，返回null
     */
    peekInt40()
    {
        if (!this.checkReadable(5))
            return null;

        var high = this._buffer.readInt8(this._readPos);
        var low = this._buffer.readUInt32BE(this._readPos + 1);
        return high * 0x100000000 + low;
    }

    /**
     * 读取uint40
     * @returns {number|null} 如果字节不够，返回null
     */
    readUInt40()
    {
        if (!this.checkReadable(5))
            return null;

        var high = this._buffer.readUInt8(this._readPos);
        var low = this._buffer.readUInt32BE(this._readPos + 1);
        this._readPos += 5;
        return high * 0x100000000 + low;
    }

    /**
     * 读取uint40，读取指针不移动
     * @returns {number|null} 如果字节不够，返回null
     */
    peekUInt40()
    {
        if (!this.checkReadable(5))
            return null;

        var high = this._buffer.readUInt8(this._readPos);
        var low = this._buffer.readUInt32BE(this._readPos + 1);
        return high * 0x100000000 + low;
    }

    /**
     * 读取int48
     * @returns {number|null} 如果字节不够，返回null
     */
    readInt48()
    {
        if (!this.checkReadable(6))
            return null;

        var high = this._buffer.readInt16BE(this._readPos);
        var low = this._buffer.readUInt32BE(this._readPos + 2);
        this._readPos += 6;
        return high * 0x100000000 + low;
    }

    /**
     * 读取int48，读取指针不移动
     * @returns {number|null} 如果字节不够，返回null
     */
    peekInt48()
    {
        if (!this.checkReadable(6))
            return null;

        var high = this._buffer.readInt16BE(this._readPos);
        var low = this._buffer.readUInt32BE(this._readPos + 2);
        return high * 0x100000000 + low;
    }

    /**
     * 读取uint48
     * @returns {number|null} 如果字节不够，返回null
     */
    readUInt48()
    {
        if (!this.checkReadable(6))
            return null;

        var high = this._buffer.readUInt16BE(this._readPos);
        var low = this._buffer.readUInt32BE(this._readPos + 2);
        this._readPos += 6;
        return high * 0x100000000 + low;
    }

    /**
     * 读取uint48，读取指针不移动
     * @returns {number|null} 如果字节不够，返回null
     */
    peekUInt48()
    {
        if (!this.checkReadable(6))
            return null;

        var high = this._buffer.readUInt16BE(this._readPos);
        var low = this._buffer.readUInt32BE(this._readPos + 2);
        return high * 0x100000000 + low;
    }

    /**
     * 读取int56
     * @returns {number|null} 如果字节不够，返回null
     */
    readInt56()
    {
        if (!this.checkReadable(7))
            return null;

        var high = this._buffer.readInt8(this._readPos);
        var mid = this._buffer.readUInt16BE(this._readPos + 1);
        var low = this._buffer.readUInt32BE(this._readPos + 3);
        this._readPos += 7;
        return (high * 0x10000 + mid) * 0x100000000 + low;
    }

    /**
     * 读取int56，读取指针不移动
     * @returns {number|null} 如果字节不够，返回null
     */
    peekInt56()
    {
        if (!this.checkReadable(7))
            return null;

        var high = this._buffer.readInt8(this._readPos);
        var mid = this._buffer.readUInt16BE(this._readPos + 1);
        var low = this._buffer.readUInt32BE(this._readPos + 3);
        return (high * 0x10000 + mid) * 0x100000000 + low;
    }

    /**
     * 读取uint56
     * @returns {number|null} 如果字节不够，返回null
     */
    readUInt56()
    {
        if (!this.checkReadable(7))
            return null;

        var high = this._buffer.readUInt8(this._readPos);
        var mid = this._buffer.readUInt16BE(this._readPos + 1);
        var low = this._buffer.readUInt32BE(this._readPos + 3);
        this._readPos += 7;
        return (high * 0x10000 + mid) * 0x100000000 + low;
    }

    /**
     * 读取uint56，读取指针不移动
     * @returns {number|null} 如果字节不够，返回null
     */
    peekUInt56()
    {
        if (!this.checkReadable(7))
            return null;

        var high = this._buffer.readUInt8(this._readPos);
        var mid = this._buffer.readUInt16BE(this._readPos + 1);
        var low = this._buffer.readUInt32BE(this._readPos + 3);
        return (high * 0x10000 + mid) * 0x100000000 + low;
    }

    /**
     * 读取int64，不过js的number其实是double，只能精确表示到2^53，所以如果可能超过这个值，那就不要用int64
     * @returns {number|null} 如果字节不够，返回null
     */
    readInt64()
    {
        if (!this.checkReadable(8))
            return null;

        var high = this._buffer.readInt32BE(this._readPos);
        var low = this._buffer.readUInt32BE(this._readPos + 4);
        this._readPos += 8;
        return high * 0x100000000 + low;
    }

    /**
     * 读取int64，读取指针不移动
     * @returns {number|null} 如果字节不够，返回null
     */
    peekInt64()
    {
        if (!this.checkReadable(8))
            return null;

        var high = this._buffer.readInt32BE(this._readPos);
        var low = this._buffer.readUInt32BE(this._readPos + 4);
        return high * 0x100000000 + low;
    }

    /**
     * 读取uint64，不过js的number其实是double，只能精确表示到2^53，所以如果可能超过这个值，那就不要用int64
     * @returns {number|null} 如果字节不够，返回null
     */
    readUInt64()
    {
        if (!this.checkReadable(8))
            return null;

        var high = this._buffer.readUInt32BE(this._readPos);
        var low = this._buffer.readUInt32BE(this._readPos + 4);
        this._readPos += 8;
        return high * 0x100000000 + low;
    }

    /**
     * 读取uint64，读取指针不移动
     * @returns {number|null} 如果字节不够，返回null
     */
    peekUInt64()
    {
        if (!this.checkReadable(8))
            return null;

        var high = this._buffer.readUInt32BE(this._readPos);
        var low = this._buffer.readUInt32BE(this._readPos + 4);
        return high * 0x100000000 + low;
    }

    /**
     * 读取单精度浮点
     * @returns {number|null} 如果字节不够，返回null
     */
    readFloat()
    {
        if (!this.checkReadable(4))
            return null;

        var ret = this._buffer.readFloatBE(this._readPos);
        this._readPos += 4;
        return ret;
    }

    /**
     * 读取单精度浮点，读取指针不移动
     * @returns {number|null} 如果字节不够，返回null
     */
    peekFloat()
    {
        if (!this.checkReadable(4))
            return null;

        return this._buffer.readFloatBE(this._readPos);
    }

    /**
     * 读取双精度浮点
     * @returns {number|null} 如果字节不够，返回null
     */
    readDouble()
    {
        if (!this.checkReadable(8))
            return null;

        var ret = this._buffer.readDoubleBE(this._readPos);
        this._readPos += 8;
        return ret;
    }

    /**
     * 读取双精度浮点，读取指针不移动
     * @returns {number|null} 如果字节不够，返回null
     */
    peekDouble()
    {
        if (!this.checkReadable(8))
            return null;

        return this._buffer.readDoubleBE(this._readPos);
    }

    /**
     * 读取字符串，会读取长度
     * @returns {string|null} 如果字节不够，返回null
     */
    readString()
    {
        var len = this.readInt32();
        if (len === null)
            return null;

        if (!this.checkReadable(len))
            return null;

        var start = this._readPos;
        var end = start + len;
        var str = this._buffer.slice(start, end).toString();
        this._readPos = end;
        return str;
    }

    /**
     * 读取字符串，不读取长度，要提供长度
     * @param {number} len
     * @returns {string|null} 如果字节不够，返回null
     */
    readOnlyString(len)
    {
        if (!this.checkReadable(len))
            return null;

        var start = this._readPos;
        var end = start + len;
        var str = this._buffer.slice(start, end).toString();
        this._readPos = end;
        return str;
    }

    /**
     * 读取时，跳过多少字节
     * @param n
     * @return {number} 实际跳过的字节数
     */
    skip(n)
    {
        n = Math.min(Math.max(0, n), this.canReadLen());
        this._readPos += n;
        return n;
    }

    /**
     * 全部可读数据跳过
     * @return {number} 实际跳过的字节数
     */
    skipAll()
    {
        var n = this.canReadLen();
        this._readPos += n;
        return n;
    }

    /**
     * 读指针重置为0
     */
    resetRead()
    {
        this._readPos = 0;
    }

    writeBool(v)
    {
        this.ensure(1);

        this._buffer.writeInt8(v ? 1 : 0, this._writePos);
        ++this._writePos;
    }

    /**
     * 把缓冲对象写入
     * 注意：考虑到多数应用场景，写入IOBuffer时，不修改源IOBuffer读取指针，如果写入完后源Buffer要移动指针，那就手动skip吧
     * @param {IOBuffer|Buffer|number[]} buf
     * @param {number?} [off=0] - 在源缓冲上的偏移，对于IOBuffer，这个值无效
     * @param {number?} [len=buf.length-off] - 读入的长度，如果是IOBuffer，这个值默认是可读内容的长度
     */
    writeBytes(buf, off, len) {
        if (buf instanceof IOBuffer)
        {
            len = len || buf.canReadLen();
            this.ensure(len);
            let start = buf._readPos;
            let end = start + len;
            buf._buffer.copy(this._buffer, this._writePos, start, end);
            //不移动源的读取指针了
            //buf._readPos = end;
        }
        else
        {
            off = off || 0;
            len = len || buf.length - off;
            this.ensure(len);
            if (Buffer.isBuffer(buf))
            {
                buf.copy(this._buffer, this._writePos, off, off + len);
            }
            else
            {
                let buffer = this._buffer;
                let writePos = this._writePos;
                for (var i = 0; i < len; ++i)
                    buffer[writePos + i] = buf[off + i];
            }
        }

        this._writePos += len;
    }

    writeInt8(v)
    {
        this.ensure(1);

        this._buffer.writeInt8(v, this._writePos);
        ++this._writePos;
    }

    writeUInt8(v)
    {
        this.ensure(1);

        this._buffer.writeUInt8(v, this._writePos);
        ++this._writePos;
    }

    writeInt16(v)
    {
        this.ensure(2);

        this._buffer.writeInt16BE(v, this._writePos);
        this._writePos += 2;
    }

    writeUInt16(v)
    {
        this.ensure(2);

        this._buffer.writeUInt16BE(v, this._writePos);
        this._writePos += 2;
    }

    writeInt24(v)
    {
        this.ensure(3);

        var high = Math.floor(v / 0x10000);
        var low = v - high * 0x10000;
        this._buffer.writeInt8(high, this._writePos);
        this._buffer.writeUInt16BE(low, this._writePos + 1);
        this._writePos += 3;
    }

    writeUInt24(v)
    {
        this.ensure(3);

        var high = Math.floor(v / 0x10000);
        var low = v - high * 0x10000;
        this._buffer.writeUInt8(high, this._writePos);
        this._buffer.writeUInt16BE(low, this._writePos + 1);
        this._writePos += 3;
    }

    writeInt32(v)
    {
        this.ensure(4);

        this._buffer.writeInt32BE(v, this._writePos);
        this._writePos += 4;
    }

    writeUInt32(v)
    {
        this.ensure(4);

        this._buffer.writeUInt32BE(v, this._writePos);
        this._writePos += 4;
    }

    /**
     * 使用相对于读指针的位置索引来写int32
     * 正常范围是已写未读的数据，也就是readPos到writePos - 4，相对位置数值范围就是0到canReadLen - 4
     * @param {number} v - 要写入的数据
     * @param {number} pos - 相对于读指针的位置
     * @returns {boolean} 如果位置在正常范围内，返回true，否则返回false
     */
    writeInt32WithRelativePos(v, pos)
    {
        if (pos < 0 || pos > this.canReadLen() - 4)
            return false;

        this._buffer.writeInt32BE(v, this._readPos + pos);
        return true;
    }

    /**
     * 使用相对于读指针的位置索引来写uint32
     * 正常范围是已写未读的数据，也就是readPos到writePos - 4，相对位置数值范围就是0到canReadLen - 4
     * @param {number} v - 要写入的数据
     * @param {number} pos - 相对于读指针的位置
     * @returns {boolean} 如果位置在正常范围内，返回true，否则返回false
     */
    writeUInt32WithRelativePos(v, pos)
    {
        if (pos < 0 || pos > this.canReadLen() - 4)
            return false;

        this._buffer.writeUInt32BE(v, this._readPos + pos);
        return true;
    }

    writeInt40(v)
    {
        this.ensure(5);

        var high = Math.floor(v / 0x100000000);
        var low = v - high * 0x100000000;
        this._buffer.writeInt8(high, this._writePos);
        this._buffer.writeUInt32BE(low, this._writePos + 1);
        this._writePos += 5;
    }

    writeUInt40(v)
    {
        this.ensure(5);

        var high = Math.floor(v / 0x100000000);
        var low = v - high * 0x100000000;
        this._buffer.writeUInt8(high, this._writePos);
        this._buffer.writeUInt32BE(low, this._writePos + 1);
        this._writePos += 5;
    }

    writeInt48(v)
    {
        this.ensure(6);

        var high = Math.floor(v / 0x100000000);
        var low = v - high * 0x100000000;
        this._buffer.writeInt16BE(high, this._writePos);
        this._buffer.writeUInt32BE(low, this._writePos + 2);
        this._writePos += 6;
    }

    writeUInt48(v)
    {
        this.ensure(6);

        var high = Math.floor(v / 0x100000000);
        var low = v - high * 0x100000000;
        this._buffer.writeUInt16BE(high, this._writePos);
        this._buffer.writeUInt32BE(low, this._writePos + 2);
        this._writePos += 6;
    }

    writeInt56(v)
    {
        this.ensure(7);

        var temp = Math.floor(v / 0x100000000);
        var high = Math.floor(temp / 0x10000);
        var mid = temp - high * 0x10000;
        var low = v - temp * 0x100000000;
        this._buffer.writeInt8(high, this._writePos);
        this._buffer.writeUInt16BE(mid, this._writePos + 1);
        this._buffer.writeUInt32BE(low, this._writePos + 3);
        this._writePos += 7;
    }

    writeUInt56(v)
    {
        this.ensure(7);

        var temp = Math.floor(v / 0x100000000);
        var high = Math.floor(temp / 0x10000);
        var mid = temp - high * 0x10000;
        var low = v - temp * 0x100000000;
        this._buffer.writeUInt8(high, this._writePos);
        this._buffer.writeUInt16BE(mid, this._writePos + 1);
        this._buffer.writeUInt32BE(low, this._writePos + 3);
        this._writePos += 7;
    }

    /**
     * 写入int64，不过js的number其实是double，只能精确表示到2^53，所以如果可能超过这个值，那就不要用int64
     * @param {number} v - 整数
     * @return {number}
     */
    writeInt64(v)
    {
        this.ensure(8);

        var high = Math.floor(v / 0x100000000);
        var low = v - high * 0x100000000;
        this._buffer.writeInt32BE(high, this._writePos);
        this._buffer.writeUInt32BE(low, this._writePos + 4);
        this._writePos += 8;
    }

    /**
     * 写入uint64，不过js的number其实是double，只能精确表示到2^53，所以如果可能超过这个值，那就不要用uint64
     * @param {number} v - 整数
     * @return {number}
     */
    writeUInt64(v)
    {
        this.ensure(8);

        var high = Math.floor(v / 0x100000000);
        var low = v - high * 0x100000000;
        this._buffer.writeUInt32BE(high, this._writePos);
        this._buffer.writeUInt32BE(low, this._writePos + 4);
        this._writePos += 8;
    }

    writeFloat(v)
    {
        this.ensure(4);

        this._buffer.writeFloatBE(v, this._writePos);
        this._writePos += 4;
    }

    writeDouble(v)
    {
        this.ensure(8);

        this._buffer.writeDoubleBE(v, this._writePos);
        this._writePos += 8;
    }

    /**
     * 写入字符串，写入长度
     * @param {string} v
     */
    writeString(v)
    {
        var len = Buffer.byteLength(v);

        this.writeInt32(len);

        this.ensure(len);

        this._buffer.write(v, this._writePos, len);
        this._writePos += len;
    }

    /**
     * 写入字符串，不写入长度
     * @param {string} v
     * @param {number} len - 如果不指定长度，就自己取
     */
    writeOnlyString(v, len)
    {
        len = len || Buffer.byteLength(v);

        this.ensure(len);

        this._buffer.write(v, this._writePos, len);
        this._writePos += len;
    }

    /**
     * 重置写指针为0，同时读指针也会重置
     */
    resetWrite()
    {
        this._readPos = 0;
        this._writePos = 0;
    }
}

////////////导出元素////////////
exports.IOBuffer = IOBuffer;