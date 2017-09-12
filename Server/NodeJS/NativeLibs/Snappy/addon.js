var snappy = require('./build/Release/snappy');

var obj = {propString:"广州", "propNumber":123456, "propArray":["1","2","3"], propEnum:2};
var msg = JSON.stringify(obj);

console.log("压缩功能测试1开始");
var buf = snappy.compress(msg)
var msg2 = snappy.uncompress(buf);
if (snappy.isValid(buf))
    console.log("压缩内容有效");
else
    console.log("压缩内容无效");
if (msg == msg2)
    console.log("压缩解压正确");
else
    console.log("压缩解压错误");
console.log("压缩功能测试1结束\r\n");

console.log("压缩功能测试2开始");
var srcBuf = new Buffer(msg);
var buf = snappy.compress(srcBuf)
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
    var buf = snappy.compress(msg)
    var msg2 = snappy.uncompress(buf);
}
console.timeEnd("profile");
console.log("压缩前内容：" + msg);
console.log("压缩后内容：" + buf.toString());
console.log("解压后内容：" + msg2);
console.log("压缩前长度：" + Buffer.byteLength(msg));
console.log("压缩后长度：" + buf.length);
console.log("压缩性能测试结束\r\n");

console.log("内存占用：" + JSON.stringify(process.memoryUsage()) + "\r\n");