"use strict";

var Promise = require("bluebird");
var fs = require("fs");
var csvParse = require("csv-parse");
var csvStringify = require("csv-stringify");
var gameCfg = require("../logic/gameConfig/gameConfig");
var TestConfig = require("../logic/gameConfig/testConfig").TestConfig;

module.exports = {
    setUp: function (callback) {
        console.log("===========\r\n");
        callback();
    },
    tearDown: function (callback) {
        callback();
    },
    testCsvParseStringify: function (test) {
        fs.readFile("data/test.csv", function(err, data){
            if (err)
            {
                console.error(err);
                test.done();
                return;
            }

            csvParse(data, {delimiter: ","}, function (err, data){
                if (err)
                {
                    console.error(err);
                    test.done();
                    return;
                }

                console.log(data);
                csvStringify(data, function (err, data){
                    if (err)
                    {
                        console.error(err);
                        test.done();
                        return;
                    }

                    console.log(data);
                    test.done();
                });
            });
        });
    },
    testGameConfig: function (test) {
        //这个用法效率很低，正确的用法是var coFun = Promise.coroutine()一次，多次coFun()，但这里不频繁调用，所以暂时可以这样用
        Promise.coroutine(function * (){
            var cfgObjs1 = {
                "test" : {file:"test.csv", rowType:TestConfig, rowKey:"id"},
                "test2" : {file:["test.csv","test2.csv"], rowType:TestConfig, rowKey:"id"}
            };
            var cfgObjs2 = {
                "test" : {file:"test.csv", rowType:TestConfig}
            };
            var cfgObjs3 = {
                "test" : {file:"test.csv"}
            };
            var cfgObjs4 = {
                "test" : "test.json"
            };
            var error = null;
            try {
                yield gameCfg.loadConfig(cfgObjs1);
                console.log(gameCfg.getCsvConfig("test"));
                console.log(gameCfg.getCsvConfig("test", 1));
                console.log(gameCfg.getCsvConfig("test2"));
                console.log(gameCfg.getCsvConfig("test2", 5));

                yield gameCfg.loadConfig(cfgObjs2);
                console.log(gameCfg.getCsvConfig("test"));
                console.log(gameCfg.getCsvConfig("test", 1));

                yield gameCfg.loadConfig(cfgObjs3);
                console.log(gameCfg.getCsvConfig("test"));
                console.log(gameCfg.getCsvConfig("test", 1));

                yield gameCfg.loadConfig(cfgObjs4);
                console.log(gameCfg.getJsonConfig("test"));
            }
            catch (err) {
                error = err;
            }
            finally {
                test.ifError(error);
                test.done();
            }
        })();
    },
};