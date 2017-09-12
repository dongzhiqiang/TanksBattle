"use strict";

var gameConfig = require("./gameConfig");

var TestEnum = require("../enumType/testEnum").TestEnum;

class TestConfig
{
    constructor() {
        this.id = 0;
        this.rowA = 0;
        this.rowB = "";
        this.rowC = "";
        this.rowD = "";
        this.rowE = "";
        this.rowF = "";
        this.rowBool = false;
        this.rowArray = null;
        this.rowMultiDArray = null;
        this.rowMultiDArray2 = null;
        this.rowEnum = null;
        this.rowBuffer = null;
    }

    static fieldsDesc() {
        return {
            id: {type: Number},
            rowA: {type: Number},
            rowB: {type: String},
            rowC: {type: String},
            rowD: {type: String},
            rowE: {type: String},
            rowF: {type: String},
            rowBool: {type: Boolean},
            rowArray: {type: Array, elemType:Number},
            rowMultiDArray: {type: Array, elemType:Number, arrayLayer:3}, //多维数组，elemType是最内层的类型，arrayLayer是层数，不填，就是1层
            rowMultiDArray2: {type: Array, elemType:Number, arrayLayer:5}, //多维数组，elemType是最内层的类型，arrayLayer是层数，不填，就是1层
            rowEnum: {type: TestEnum},
            rowBuffer: {type: Buffer}
        };
    }

    /**
     * 自定义读取数据的过程，就是把原始的csv行数组提供过来，自己负责把行的数据转成本类的对象，那个默认的读取过程就不执行了
     * 这个函数可选，没有就不执行
     * @param {string[]} csvRow - csv的原始数据数组
     * @return {object}
     */
    //static customReadRow(csvRow)
    //{
    //    //key表示该行的主键值，如果配置没有主键，就填null吧
    //    //row表示该行的数据对象，一般不允许为null
    //    //注意要自己校验和转换类型
    //    var o = new TestConfig();
    //    o.id = csvRow[0];
    //    o.rowA = csvRow[1];
    //    o.rowB = csvRow[2];
    //    o.rowC = csvRow[3];
    //    o.rowD = csvRow[4];
    //    o.rowE = csvRow[5];
    //    o.rowF = csvRow[6];
    //    o.rowBool = csvRow[7];
    //    o.rowArray = csvRow[8];
    //    o.rowMultiDArray = csvRow[9];
    //    o.rowMultiDArray2 = csvRow[10];
    //    o.rowEnum = csvRow[11];
    //    o.rowBuffer = csvRow[12];
    //    return {key:null, row:o};
    //}

    /**
     * 使用默认读取方式读完一行数据后可以执行对行对象的再次处理
     * 如果使用自定义读取方式，直接在那个自定义读取方式里处理就行了，不用这个函数了
     * 这个函数可选，没有就不执行
     * @param {object} row - 行数据对象
     */
    static afterDefReadRow(row)
    {
        //console.log("afterDefReadRow, RowKey:" + row.id);
    }

    /**
     * 无论哪种方式读取，读取完全部数据后可以执行对全部行综合再次处理
     * 这个函数可选，没有就不执行
     * @param {object[]|object.<(string|number), object>} rows
     */
    static afterReadAll(rows)
    {
        //console.log("afterReadAll");
    }
}

/**
 * 这个是单个文件的
 * @param {(string|number)?} key - 主键或有效数据行号，不填的话，就返回全部行
 * @returns {TestConfig}
 */
function getTestConfig(key)
{
    return gameConfig.getCsvConfig("test", key);
}

/**
 * 这个是多个文件读到一个配置对象里
 * @param {(string|number)?} key - 主键或有效数据行号，不填的话，就返回全部行
 * @returns {TestConfig}
 */
function getTestConfig2(key)
{
    return gameConfig.getCsvConfig("test2", key);
}

exports.TestConfig = TestConfig;
exports.getTestConfig = getTestConfig;
exports.getTestConfig2 = getTestConfig2;