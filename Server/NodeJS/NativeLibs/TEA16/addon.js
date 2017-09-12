var tea16 = require('./build/Release/tea16');

const default_key = [
    0x3687C5E3,
    0xB7EF3327,
    0xE3791011,
    0x84E2D3BC
];

var resstr = '{"code":0,"serverList":[{"game":"测试区","serverName":"测试服","port":11111,"index":0,"ip":"127.0.0.1","serverId":0,"lanIp":"","lanPort":0,"recommendState":"hot","channel":"netease","onlineNums":0,"serverState":0}],"userRoles":{"userId":"test","preRolePacked":"null","rolesPacked":"[]","preRole":null,"roles":[]},"flag":false,"md5":null,"address":"192.168.0.205:80","userId":"test","key":"ac6cd816586a7bb546ea9d3fd702049f","timestamp":1450261693442}';
var resbuf = new Buffer(resstr);
var start = 0;
var oplen = resbuf.length;

console.log("------------------");
console.log("offset:" + start + ", length:" + oplen + ", total length:" + resbuf.length);

console.log("----正确性测试----");
tea16.encrypt(resbuf, default_key, start, oplen);
console.log("加密后内容");
console.log(resbuf.toString());
tea16.decrypt(resbuf, default_key, start, oplen);
console.log("解密后内容");
console.log(resbuf.toString());
var resstr2 = resbuf.toString();
console.log("解密后数据是否跟原数据一样：" + (resstr == resstr2));

console.log("----性能测试----");
console.time("性能测试");
for (var i = 0; i < 100000; ++i)
{
    tea16.encrypt(resbuf, default_key, start, oplen);
    tea16.decrypt(resbuf, default_key, start, oplen);
}
console.timeEnd("性能测试");
var resstr2 = resbuf.toString();
console.log("解密后数据是否跟原数据一样：" + (resstr == resstr2));

console.log("------------------");