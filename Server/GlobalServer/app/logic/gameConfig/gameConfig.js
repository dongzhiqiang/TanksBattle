"use strict";

////////////外部模块////////////
var Promise = require("bluebird");

////////////内部模块////////////
var fsUtil = require("fs");
var pathUtil = require("path");

////////////外部模块////////////
var csvParse = Promise.promisify(require("csv-parse"));
var csvStringify = Promise.promisify(require("csv-stringify"));

////////////我的模块////////////
var appUtil = require("../../libs/appUtil");
var logUtil = require("../../libs/logUtil");

////////////模块内数据////////////
/**
 * 结构：配置名 键 行对象
 * @type {object.<string, object.<(string|number), object>>}
 */
var csvConfigMap = {};
/**
 * 结构：配置名 配置对象
 * @type {object.<string, object>}
 */
var jsonConfigMap = {};

////////////内部函数////////////
function readFileCoroutine(filePath) {
    return new Promise(function (resolve, reject) {
        fsUtil.readFile(filePath, {encoding: "utf8"}, function (err, data) {
            if (err)
                reject(err);
            else
                resolve(data);
        });
    });
}

////////////导出函数////////////
var loadConfigCoroutine = Promise.coroutine(
    /**
     * @type {(string|object)[]}
     */
    function * (cfgObjs) {
        logUtil.info("开始加载配置");
        let tempCsvConfigMap = {};
        let tempJsonConfigMap = {};

        for (let cfgName in cfgObjs) {
            let cfgItem = cfgObjs[cfgName];
            /**
             * @type {string|string[]|object[]}
             */
            let fileNameOrArr = Object.isObject(cfgItem) ? cfgItem.file : cfgItem;
            let fileNameIsArr = Object.isArray(fileNameOrArr);

            try {
                logUtil.info("加载配置：" + cfgName);
                //如果是数组，要每个文件都是csv
                if (fileNameIsArr)
                {
                    if (fileNameOrArr.length <= 0)
                        throw new Error("文件路径名数组不能为空，配置名：" + cfgName);

                    for (let i = 0, leni = fileNameOrArr.length; i < leni; ++i)
                    {
                        let fileName = fileNameOrArr[i];
                        let fileExt = appUtil.getFileExtName(fileName);
                        if (fileExt !== "csv")
                            throw new Error("文件路径名数组的方式只能是csv文件，配置名：" + cfgName + "，数组内容：" + fileNameOrArr.join(","));
                    }
                }

                let fileExt = fileNameIsArr ? "" : appUtil.getFileExtName(fileNameOrArr);
                if (fileExt == "csv" || fileNameIsArr) {
                    let csvRows;  //数组情况下是多个文件的合并后数组，后续表的前两行不追加进去
                    let titleRow; //数组情况下是第一个文件的对应行
                    let fieldRow; //数组情况下是第一个文件的对应行
                    if (!fileNameIsArr) {
                        let fileName = fileNameOrArr;
                        let filePath = pathUtil.join("data", fileName);
                        let fileData = yield readFileCoroutine(filePath);
                        csvRows = yield csvParse(fileData, {delimiter: ","});
                        if (csvRows.length < 2)
                            throw new Error("CSV文件至少要有两行：标题行、字段名行，文件名：" + fileName);
                        titleRow = csvRows[0];
                        fieldRow = csvRows[1];
                    }
                    else {
                        csvRows = [];
                        for (let i = 0, leni = fileNameOrArr.length; i < leni; ++i)
                        {
                            let fileName = fileNameOrArr[i];
                            let filePath = pathUtil.join("data", fileName);
                            let fileData = yield readFileCoroutine(filePath);
                            let csvRowsTmp = yield csvParse(fileData, {delimiter: ","});
                            if (csvRowsTmp.length < 2)
                                throw new Error("CSV文件至少要有两行：标题行、字段名行，文件名：" + fileName);
                            if (i === 0)
                            {
                                titleRow = csvRowsTmp[0];
                                fieldRow = csvRowsTmp[1];
                                csvRows = csvRowsTmp;
                            }
                            else
                            {
                                for (let j = 2, lenj = csvRowsTmp.length; j < lenj; ++j)
                                    csvRows.push(csvRowsTmp[j]);
                            }
                        }
                    }

                    let rowType = Object.isObject(cfgItem) && !Object.isUndefined(cfgItem.rowType) ? cfgItem.rowType : Object;
                    if (!Object.isFunction(rowType))
                        throw new Error("CSV文件设置了rowType，但不是Function类型，配置名：" + cfgName + "，类型：" + rowType);
                    //看看有没有字段描述
                    let fieldDescMap;
                    if (rowType.fieldsDesc)
                    {
                        fieldDescMap = rowType.fieldsDesc();
                        for (let i = 0, leni = fieldRow.length; i < leni; ++i)
                        {
                            let fieldName = fieldRow[i];
                            //【注意】没有字段名？那就放弃这列
                            if (!fieldName)
                                continue;
                            if (!Object.isObject(fieldDescMap[fieldName]))
                                continue;
                                //throw new Error("CSV发现字段没有对应字段描述，配置名：" + cfgName + "，字段名：" + fieldName);
                        }
                    }

                    let rowKey = Object.isObject(cfgItem) ? cfgItem.rowKey : undefined;
                    //看看主键在第几列
                    if (rowKey != undefined && fieldRow.indexOf(rowKey) < 0)
                        throw new Error("CSV文件设置了rowKey，发现不存在对应列，配置名：" + cfgName + "，主键列名：" + rowKey);

                    let cfgObj = rowKey == undefined ? [] : {};

                    for (let i = 2, leni = csvRows.length; i < leni; ++i)
                    {
                        let csvRow = csvRows[i];
                        let keyVal;
                        let rowObj;

                        //有自定义读取方法就使用自定义的
                        if (rowType.customReadRow) {
                            var ret = rowType.customReadRow(csvRow);
                            keyVal = ret.key; //这个值可能为null
                            rowObj = ret.row;
                        }
                        else {
                            //【注意】列数不一样？不管了，就按fieldRow来读，实际多出来的列，抛弃，少掉的列，当空字符串，再转成其它类型
                            //if (csvRow.length != fieldRow.length)
                            //    throw new Error("CSV文件数据行列数不等于字段行列数，配置名:" + cfgName + "，行号(0开始)：" + i);
                            rowObj = new rowType();
                            for (let k = 0, lenk = fieldRow.length; k < lenk; ++k) {
                                let csvCellVal = csvRow[k] == undefined ? "" : csvRow[k];
                                let fieldName = fieldRow[k];
                                //【注意】没有字段名？那就放弃这列
                                if (!fieldName)
                                    continue;
                                if (fieldDescMap != undefined) {
                                    let fieldDesc = fieldDescMap[fieldName];
                                    if(fieldDesc == undefined)
                                        continue;
                                    csvCellVal = appUtil.tryParseToDataType(csvCellVal, fieldDesc.type, fieldDesc.elemType, fieldDesc.arrayLayer);
                                }
                                if (fieldName == rowKey)
                                    keyVal = csvCellVal;
                                rowObj[fieldName] = csvCellVal;
                            }

                            //有再次处理函数就再次处理
                            if (rowType.afterDefReadRow)
                                rowType.afterDefReadRow(rowObj);
                        }

                        if (rowKey == undefined)
                            cfgObj.push(rowObj);
                        else
                            cfgObj[keyVal] = rowObj;
                    }

                    //有整体再次处理函数就再次处理
                    if (rowType.afterReadAll)
                        rowType.afterReadAll(cfgObj);

                    tempCsvConfigMap[cfgName] = cfgObj;
                }
                else if (fileExt == "json") {
                    let filePath = pathUtil.join("data", fileNameOrArr);
                    let fileData = yield readFileCoroutine(filePath);
                    //这里如果有报错那就让它报，把错误抛给catch，不隐藏错误
                    let jsonObj = JSON.parse(fileData);
                    tempJsonConfigMap[cfgName] = jsonObj;
                }
                else {
                    logUtil.warn("发现未知配置文件格式：" + cfgName);
                    continue;
                }

                logUtil.info("完成加载：" + cfgName);
            }
            catch (err) {
                logUtil.error("加载配置错误", err);
                throw err;
            }
        }
        //把临时配置合并到正式
        for (let cfgName in tempCsvConfigMap) {
            csvConfigMap[cfgName] = tempCsvConfigMap[cfgName];
        }
        for (let cfgName in tempJsonConfigMap) {
            jsonConfigMap[cfgName] = tempJsonConfigMap[cfgName];
        }
        logUtil.info("配置加载完成");
    }
);

/**
 * 格式说明：
 * 配置对象：{配置名:文件路径名|文件路径名数组|子配置对象}，如果是文件路径名或文件路径名数组，相当于是{file:文件路径名|文件路径名数组}
 * 配置对象：{file:文件路径名|文件路径名数组,rowType:行数据类（可选，默认就是Object类型）,rowKey:主键列名（可选，没主键就读取为数组）}
 * 文件路径名：*.csv|*.json，就这两种格式，如果是json，rowType和rowKey无效
 * 文件路径名数组：只能是*.csv的数组
 */
/*
示例：
 var cfgObjs1 = {
 "test"  : {file:"test.csv", rowType:TestConfig, rowKey:"id"},
 "test2"  : {file:["test.csv","test2.csv"], rowType:TestConfig, rowKey:"id"},
 };
 var cfgObjs2 = {
 "test" : "test.json"
 };
 */
function loadConfig(cfgObjs) {
    return loadConfigCoroutine(cfgObjs);
}

/**
 *
 * @param {string} configName
 * @param {(string|number)?} key - 主键或有效数据行号，不填的话，就返回全部行
 * @returns {Object}
 */
function getCsvConfig(configName, key) {
    var configs = csvConfigMap[configName];
    if (configs === undefined)
        return null;
    else if (Object.isString(key) || Object.isNumber(key))
        return configs[key];
    //{
        //if (Object.isArray(configs))
        //    return configs[key];
        //else
        //    return configs[key];
    //}
    else
        return configs;
}

/**
 *
 * @param {string} configName
 * @returns {*}
 */
function getCsv(configName) {
    var configs = csvConfigMap[configName];
    if (configs === undefined)
        return null;
    else
        return configs;
}

/**
 *
 * @param {string} configName
 * @returns {Object}
 */
function getJsonConfig(configName) {
    return jsonConfigMap[configName];
}

////////////导出元素////////////
exports.loadConfig = loadConfig;
exports.getCsvConfig = getCsvConfig;
exports.getCsv = getCsv;
exports.getJsonConfig = getJsonConfig;