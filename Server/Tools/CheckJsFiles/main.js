var fs = require("fs");
var path = require("path");
var os = require("os");

var rootDir = "../../GameServer/app";
var mainJs = "main/main.js";

var logFile = null;

function startLog()
{
    if (logFile)
        endLog();
    logFile = fs.openSync("result.txt", "w");
}

function writeLog(str)
{
    if (logFile)
        fs.appendFileSync(logFile, str + os.EOL);
    console.log(str);
}

function endLog()
{
    if (logFile)
        fs.closeSync(logFile);
    logFile = null;
}

function processFile(filePath, indent, func)
{
    filePath = path.normalize(filePath);

    if (path.extname(filePath) !== ".js")
        return;
    try {
        var content = fs.readFileSync(filePath, {encoding:"utf8"});
        func(filePath, indent + 1, content);
    }
    catch(e) {
        console.error(e.stack || e);        
    }
}

function travelDir(parentDir, indent, func)
{
    parentDir = path.normalize(parentDir);

    var space = "   ".repeat(indent);
    var objNames = fs.readdirSync(parentDir);
    var dirNames = [];
    var fileNames = [];
    for (var i = 0; i < objNames.length; ++i)
    {
        var objName = objNames[i];
        var subFile = path.join(parentDir, objName);
        var stat = fs.statSync(subFile);
        if (stat.isDirectory())
            dirNames.push(objName);
        else
            fileNames.push(objName);            
    }
    for (var i = 0; i < dirNames.length; ++i)
    {
        var dirName = dirNames[i];
        //writeLog(space + "[" + dirName + "]");
        var subFile = path.join(parentDir, dirName);
        travelDir(subFile, indent + 1, func);
    }
    for (var i = 0; i < fileNames.length; ++i)
    {
        var fileName = fileNames[i];
        //writeLog(space + fileName);
        var subFile = path.join(parentDir, fileName);
        processFile(subFile, indent + 1, func)
    }
}

function checkFileNameCase(filePath)
{
    try {
        var dirPath = path.dirname(filePath);
        if (dirPath === '/' || dirPath === '\\' || dirPath === '.')
            return true;
    
        var fileNames = fs.readdirSync(dirPath);
        var baseName = path.basename(filePath);
        if (baseName !== '..' && baseName !== '.' && fileNames.indexOf(baseName) < 0)
            return false;
        return checkFileNameCase(dirPath);
    }
    catch (e) {
        return false;
    }    
}

var pathWrongCount   = 0;
var varNotUsedCount  = 0;
var circularRefCount = 0;
var requireFileMap   = {}; //格式{JS文件路径:Require文件数组}
var tempPathNodes    = [];
var circularPathStrs = {}; //为了避免重复显示循环路径

function checkRequireFiles(filePath, indent, content)
{
    var reqFileNames = [];
    requireFileMap[filePath] = reqFileNames;
    var rootDirN = path.normalize(rootDir);
    var dirPath = path.dirname(filePath);
    var regExp = /var\s+(\w+)\s*=\s*require\s*\(\s*['"](.+)['"]\s*\)/g;
    var regExp2 = /var\s+(\w+)\s*=\s*require\s*\(\s*['"](.+)['"]\s*\)/;    //为了不干扰regExp遍历，还有一个同样匹配规则，但不全局匹配的
    var result;
    while ((result = regExp.exec(content)) !== null)
    {
        var varName = result[1];
        var reqPath = result[2];
        //如果有.，说明是自己的模块，要检查路径
        if (reqPath[0] === ".")
        {
            var modulePath = path.join(dirPath, reqPath);
            modulePath = path.normalize(modulePath) + ".js";
            //检查文件大小写是否一致和文件是否存在
            if (!checkFileNameCase(modulePath))
            {
                ++pathWrongCount;
                writeLog("路径有问题，JS文件：" + filePath + "，变量名：" + varName + "，引用路径：" + reqPath);
            }
            reqFileNames.push(modulePath);
        }
        //检查变量是否使用
        var regExpTemp = new RegExp(".*\\b" + varName + "\\b.*", "g");
        var resultTemp;
        var isFind = false;
        //找出单词
        while ((resultTemp = regExpTemp.exec(content)) !== null)
        {
            var line = resultTemp[0];
            //看看这行是不是require行本身，如果不是，说明找到了，为什么不直接使用预查排除掉require查？因为js不支持反向预查
            if (!regExp2.test(line))
            {
                isFind = true;
                break;
            }                
        }
        if (!isFind)
        {
            ++varNotUsedCount;
            writeLog("变量没使用，JS文件：" + filePath + "，变量名：" + varName + "，引用路径：" + reqPath);
        }
    }
}

function checkCirCularReference(filePath)
{
    filePath = path.normalize(filePath);

    //把本节点加入
    tempPathNodes.push(filePath);
    for (var len = tempPathNodes.length, i = len - 2; i >= 0; --i)
    {
        var s = tempPathNodes[i];
        if (s === filePath)
        {
            var pathStr = tempPathNodes.slice(i, len).join(os.EOL);
            //没加入统计？加入
            if (!circularPathStrs[pathStr])
            {
                ++circularRefCount;
                circularPathStrs[pathStr] = true;                
                writeLog("发现循环引用，路径：" + os.EOL + pathStr);
            }
            //发现循环了，直接退出，不然一直递归            
            //先弹出本节点
            tempPathNodes.pop();
            return;
        }
    }

    var reqFiles = requireFileMap[filePath];
    if (reqFiles)
    {
        for (var i = 0; i < reqFiles.length; ++i)
        {
            var reqPath = reqFiles[i];
            checkCirCularReference(reqPath);
        }
    }
    
    //弹出本节点
    tempPathNodes.pop();
}

function doTask1()
{
    pathWrongCount = 0;
    varNotUsedCount= 0;
    requireFileMap = {};
    travelDir(rootDir, 0, checkRequireFiles);
    writeLog(os.EOL + "路径有问题数：" + pathWrongCount + "，变量没使用数：" + varNotUsedCount + os.EOL);
}

function doTask2()
{
    circularRefCount = 0;
    tempPathNodes = [];
    circularPathStrs = {};
    var startFile = path.join(rootDir, mainJs);
    checkCirCularReference(startFile);
    writeLog(os.EOL + "循环引用数：" + circularRefCount + os.EOL);
}

startLog();
doTask1();
doTask2();
endLog();