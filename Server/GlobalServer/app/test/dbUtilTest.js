"use strict";

var Promise = require("bluebird");
var appCfg = require("../../config");
var dbUtil = require("../libs/dbUtil");

const dbUrl = "mongodb://127.0.0.1:27017/test?maxPoolSize=1&autoReconnect=true&w=1&wtimeoutMS=5000";
const colName = "user";
const dbPoolSize = 5;

module.exports = {
    setUp: function (callback) {
        //为了安全，测试用单独的数据库
        appCfg.dbUrl = dbUrl;
        appCfg.colName = colName;
        appCfg.dbPoolSize = dbPoolSize;

        //用协程初始化
        //这个用法效率很低，正确的用法是var coFun = Promise.coroutine()一次，多次coFun()，但这里不频繁调用，所以暂时可以这样用
        Promise.coroutine(function * () {
            //初始化数据库
            yield dbUtil.initDB();
            //先把集合删除，为了清空数据
            var col = dbUtil.getDB().collection(colName);
            yield col.drop();
            //可以开测了
            callback();
        })();
    },
    tearDown: function (callback) {
        //用协程初始化
        //这个用法效率很低，正确的用法是var coFun = Promise.coroutine()一次，多次coFun()，但这里不频繁调用，所以暂时可以这样用
        Promise.coroutine(function * () {
            //关闭数据库
            yield dbUtil.closeDB();
            //可以结束了
            callback();
        })();
    },
    testConnect: function (test) {
        test.ok(dbUtil.getDB().isConnect(), "DB未成功连接");

        console.log("先获取一个连接和集合对象，引用它");
        var db = dbUtil.getDB(0);
        var col = db.collection(colName);

        console.log("添加一个延时计数");
        dbUtil.incPendingOp();
        setTimeout(function () {
            console.log("减少一个延时计数");
            dbUtil.decPendingOp();
        }, 1000);

        //这个用法效率很低，正确的用法是var coFun = Promise.coroutine()一次，多次coFun()，但这里不频繁调用，所以暂时可以这样用
        Promise.coroutine(function * () {
            console.log("试试关闭数据库");
            yield dbUtil.closeDB();
            console.log("试试关闭后继续操作");
            var errObj = null;
            try {
                var cnt = yield col.count();
                console.log("cnt=" + cnt);
            }
            catch (err) {
                console.log("是的，出错了", err);
                errObj = err;
            }
            test.notEqual(errObj, null);
            test.done();
        })();
    },
    testReconnect: function (test) {
        test.ok(dbUtil.getDB().isConnect(), "DB未成功连接");

        console.log("先获取一个连接和集合对象，引用它");
        var db = dbUtil.getDB(0);
        var col = db.collection(colName);
        var docCnt = 1000;
        var delay = 50;

        //这个用法效率很低，正确的用法是var coFun = Promise.coroutine()一次，多次coFun()，但这里不频繁调用，所以暂时可以这样用
        Promise.coroutine(function * () {
            //先插入N条数据
            var docs = [];
            for (let i = 0; i < docCnt; ++i)
            {
                docs.push({userid: i, username: "xiao" + i, birthday: new Date(), money: i});
            }
            yield col.insertMany(docs);

            //关数据库
            console.log("试试关闭数据库");
            db.close();

            //修改它的连接字符串，让它长时间不能连接成功
            db._connUrl = "";
            setTimeout(function(){
                //恢复连接字符串
                db._connUrl = dbUrl;
            }, 3000);

            console.log("试试关闭后重连");
            var errObj = null;
            try {
                for (let i = 0; i < docCnt; ++i) {
                    col.findArray({userid:i}, undefined, undefined, true);
                }
                while (dbUtil.getPendingOp() > 0)
                    yield Promise.delay(delay);
            }
            catch (err) {
                console.log("出错了？", err);
                errObj = err;
            }
            test.equal(errObj, null);
            test.done();
        })();
    },
    testFind: function (test) {
        //这个用法效率很低，正确的用法是var coFun = Promise.coroutine()一次，多次coFun()，但这里不频繁调用，所以暂时可以这样用
        Promise.coroutine(function * () {
            var col = dbUtil.getDB().collection(colName);
            try {
                var insertArr = [{userid: 1, username: "xiao1", birthday: new Date(), money: 18}, {
                    userid: 2,
                    username: "xiao2",
                    birthday: new Date(),
                    money: 16
                }, {userid: 3, username: "xiao3", birthday: new Date(), money: 20}, {
                    userid: 4,
                    username: "xiao4",
                    birthday: new Date(),
                    money: 14
                }];
                var insertCnt = yield col.insertMany(insertArr);
                test.equal(insertCnt, insertArr.length, "插入数据行数量不对");

                var arr = yield col.findArray({userid: {$gte: 0}}, {
                    _id: 0,
                    userid: 1,
                    username: 1,
                    birthday: 1,
                    money: 1
                }, {skip: 1, limit: 2, sort: {money: 1}});
                console.log("_____________________");
                console.log(arr);
                arr = yield col.findArray();
                console.log("_____________________");
                console.log(arr);
                arr = yield col.findArray({userid: {$gte: 2}});
                console.log("_____________________");
                console.log(arr);
                arr = yield col.findArray({userid: {$gte: 0}}, {_id: 0, userid: 1, username: 1, birthday: 1, money: 1});
                console.log("_____________________");
                console.log(arr);
                arr = yield col.findArray({userid: {$gte: 0}}, null, {skip: 1, limit: 2, sort: {money: 1}});
                console.log("_____________________");
                console.log(arr);

                var cursor = col.findCursor({userid: {$gte: 0}}, {
                    _id: 0,
                    userid: 1,
                    username: 1,
                    birthday: 1,
                    money: 1
                }, {skip: 1, limit: 2, sort: {money: 1}});
                test.notEqual(cursor, null, "得到cursor为null");
                while (yield cursor.hasNext()) {
                    var doc = yield cursor.next();
                    console.log(doc);
                }
                yield cursor.close();

                arr = yield col.findOne({userid: {$gte: 0}}, {_id: 0, userid: 1, username: 1, birthday: 1, money: 1});
                console.log(arr);
            }
            catch (err) {
                test.ifError(err);
            }
            test.done();
        })();
    },
    testAggregate: function (test) {
        //这个用法效率很低，正确的用法是var coFun = Promise.coroutine()一次，多次coFun()，但这里不频繁调用，所以暂时可以这样用
        Promise.coroutine(function * () {
            var col = dbUtil.getDB().collection(colName);
            try {
                var insertArr = [{userid: 1, username: "xiao1", birthday: new Date(), money: 18}, {
                    userid: 2,
                    username: "xiao2",
                    birthday: new Date(),
                    money: 16
                }, {userid: 3, username: "xiao3", birthday: new Date(), money: 20}, {
                    userid: 4,
                    username: "xiao4",
                    birthday: new Date(),
                    money: 14
                }];
                var insertCnt = yield col.insertMany(insertArr);
                test.equal(insertCnt, insertArr.length, "插入数据行数量不对");

                var arr = yield col.aggregateArray([{$match: {userid: {$gt: 1}}}], {allowDiskUse: true});
                console.log("_____________________");
                console.log(arr);
                arr = yield col.aggregateArray();
                console.log("_____________________");
                console.log(arr);
                arr = yield col.aggregateArray([{$out: "user_out"}]);
                console.log("_____________________");
                console.log(arr);

                var cursor = col.aggregateCursor([{$match: {userid: {$gt: 1}}}], {allowDiskUse: true});
                test.notEqual(cursor, null, "得到cursor为null");
                while (yield cursor.hasNext()) {
                    var doc = yield cursor.next();
                    console.log(doc);
                }
                yield cursor.close();
            }
            catch (err) {
                test.ifError(err);
            }
            test.done();
        })();
    },
    testCount: function (test) {
        //这个用法效率很低，正确的用法是var coFun = Promise.coroutine()一次，多次coFun()，但这里不频繁调用，所以暂时可以这样用
        Promise.coroutine(function * () {
            var col = dbUtil.getDB().collection(colName);
            try {
                var insertArr = [{userid: 1, username: "xiao1", birthday: new Date(), money: 18}, {
                    userid: 2,
                    username: "xiao2",
                    birthday: new Date(),
                    money: 16
                }, {userid: 3, username: "xiao3", birthday: new Date(), money: 20}, {
                    userid: 4,
                    username: "xiao4",
                    birthday: new Date(),
                    money: 14
                }];
                var insertCnt = yield col.insertMany(insertArr);
                test.equal(insertCnt, insertArr.length, "插入数据行数量不对");

                var cnt = yield col.count({userid: {$gt: 1}});
                console.log(cnt);
                test.equal(cnt, 3);
                cnt = yield col.count({userid: {$gt: 1}}, {skip: 1, limit: 2});
                console.log(cnt);
                test.equal(cnt, 2);
            }
            catch (err) {
                test.ifError(err);
            }
            test.done();
        })();
    },
    testDistinct: function (test) {
        //这个用法效率很低，正确的用法是var coFun = Promise.coroutine()一次，多次coFun()，但这里不频繁调用，所以暂时可以这样用
        Promise.coroutine(function * () {
            var col = dbUtil.getDB().collection(colName);
            try {
                var insertArr = [{userid: 1, username: "xiao1", birthday: new Date(), money: 18}, {
                    userid: 2,
                    username: "xiao2",
                    birthday: new Date(),
                    money: 16
                }, {userid: 3, username: "xiao3", birthday: new Date(), money: 20}, {
                    userid: 4,
                    username: "xiao4",
                    birthday: new Date(),
                    money: 14
                }];
                var insertCnt = yield col.insertMany(insertArr);
                test.equal(insertCnt, insertArr.length, "插入数据行数量不对");

                var arr = yield col.distinct("userid");
                arr.sort();
                console.log(arr);
                test.deepEqual(arr, [1, 2, 3, 4]);
                arr = yield col.distinct("userid", {userid: {$gt: 1}});
                arr.sort();
                console.log(arr);
                test.deepEqual(arr, [2, 3, 4]);
            }
            catch (err) {
                test.ifError(err);
            }
            test.done();
        })();
    },
    testIndex: function (test) {
        //这个用法效率很低，正确的用法是var coFun = Promise.coroutine()一次，多次coFun()，但这里不频繁调用，所以暂时可以这样用
        Promise.coroutine(function * () {
            var col = dbUtil.getDB().collection(colName);
            try {
                var insertArr = [{userid: 1, username: "xiao1", birthday: new Date(), money: 18}, {
                    userid: 2,
                    username: "xiao2",
                    birthday: new Date(),
                    money: 16
                }, {userid: 3, username: "xiao3", birthday: new Date(), money: 20}, {
                    userid: 4,
                    username: "xiao4",
                    birthday: new Date(),
                    money: 14
                }];
                var insertCnt = yield col.insertMany(insertArr);
                test.equal(insertCnt, insertArr.length, "插入数据行数量不对");

                var setIndexName = "userid_1_username_1";
                var result = yield col.createIndex({userid: 1, username: 1}, {
                    unique: true,
                    dropDups: 1,
                    background: 1,
                    name: setIndexName
                });
                console.log(result);
                test.equal(result, setIndexName);

                result = yield col.indexes();
                console.log(result);
                test.equal(Array.isArray(result), true);
                test.equal(result.length, 2);

                result = yield col.indexExists(setIndexName);
                console.log(result);
                test.equal(result, true);

                result = yield col.dropIndex(setIndexName);
                console.log(result);
                test.equal(result, true);

                result = yield col.indexExists(setIndexName);
                console.log(result);
                test.equal(result, false);

                col = dbUtil.getDB().collection(colName + "_not_exists");
                result = yield col.indexes();
                console.log(result);
                test.equal(Array.isArray(result), true);
                test.equal(result.length, 0);
            }
            catch (err) {
                test.ifError(err);
            }
            test.done();
        })();
    },
    testDelete: function (test) {
        //这个用法效率很低，正确的用法是var coFun = Promise.coroutine()一次，多次coFun()，但这里不频繁调用，所以暂时可以这样用
        Promise.coroutine(function * () {
            var col = dbUtil.getDB().collection(colName);
            try {
                var insertArr = [{userid: 1, username: "xiao1", birthday: new Date(), money: 18}, {
                    userid: 2,
                    username: "xiao2",
                    birthday: new Date(),
                    money: 16
                }, {userid: 3, username: "xiao3", birthday: new Date(), money: 20}, {
                    userid: 4,
                    username: "xiao4",
                    birthday: new Date(),
                    money: 14
                }];
                var insertCnt = yield col.insertMany(insertArr);
                test.equal(insertCnt, insertArr.length, "插入数据行数量不对");

                var result = yield col.deleteMany({userid: {$lte: 2}});
                console.log(result);
                test.equal(result, 2);

                result = yield col.deleteOne({userid: 3});
                console.log(result);
                test.equal(result, 1);

                result = yield col.deleteMany();
                console.log(result);
                test.equal(result, 1);
            }
            catch (err) {
                test.ifError(err);
            }
            test.done();
        })();
    },
    testFindAndModify: function (test) {
        //这个用法效率很低，正确的用法是var coFun = Promise.coroutine()一次，多次coFun()，但这里不频繁调用，所以暂时可以这样用
        Promise.coroutine(function * () {
            var col = dbUtil.getDB().collection(colName);
            try {
                var insertArr = [{userid: 1, username: "xiao1", birthday: new Date(), money: 18}, {
                    userid: 2,
                    username: "xiao2",
                    birthday: new Date(),
                    money: 16
                }, {userid: 3, username: "xiao3", birthday: new Date(), money: 20}, {
                    userid: 4,
                    username: "xiao4",
                    birthday: new Date(),
                    money: 14
                }];
                var insertCnt = yield col.insertMany(insertArr);
                test.equal(insertCnt, insertArr.length, "插入数据行数量不对");

                var result = yield col.findOneAndDelete({}, {
                    projection: {
                        _id: 0,
                        userid: 1,
                        username: 1,
                        birthday: 1,
                        money: 1
                    }, sort: {money: 1}
                });
                console.log(result);
                test.equal(result.userid, 4);

                result = yield col.count(result);
                console.log(result);
                test.equal(result, 0);

                result = yield col.findOneAndUpdate({userid: 5}, {
                    userid: 5,
                    username: "xiao5",
                    birthday: new Date(),
                    money: 26
                }, {projection: {_id: 0, userid: 1, username: 1, birthday: 1, money: 1}, upsert: true});
                console.log(result);
                test.equal(result, null);

                result = yield col.count({userid: 5});
                console.log(result);
                test.equal(result, 1);

                result = yield col.findOneAndUpdate({userid: 6}, {
                    $set: {
                        username: "xiao6",
                        birthday: new Date(),
                        money: 26
                    }
                }, {
                    projection: {_id: 0, userid: 1, username: 1, birthday: 1, money: 1},
                    upsert: true,
                    returnOriginal: false
                });
                console.log(result);
                test.equal(result.userid, 6);

                result = yield col.count({userid: 6});
                console.log(result);
                test.equal(result, 1);
            }
            catch (err) {
                test.ifError(err);
            }
            test.done();
        })();
    },
    testInsert: function (test) {
        //这个用法效率很低，正确的用法是var coFun = Promise.coroutine()一次，多次coFun()，但这里不频繁调用，所以暂时可以这样用
        Promise.coroutine(function * () {
            var col = dbUtil.getDB().collection(colName);
            try {
                var insertArr = [{userid: 1, username: "xiao1", birthday: new Date(), money: 18}, {
                    userid: 2,
                    username: "xiao2",
                    birthday: new Date(),
                    money: 16
                }, {userid: 3, username: "xiao3", birthday: new Date(), money: 20}, {
                    userid: 4,
                    username: "xiao4",
                    birthday: new Date(),
                    money: 14
                }];
                var insertCnt = yield col.insertMany(insertArr);
                test.equal(insertCnt, insertArr.length, "插入数据行数量不对");

                var result = yield col.insertOne({userid: 5, username: "xiao5", birthday: new Date(), money: 26});
                console.log(result);
                test.equal(result, 1);
            }
            catch (err) {
                test.ifError(err);
            }
            test.done();
        })();
    },
    testUpsert: function (test) {
        //这个用法效率很低，正确的用法是var coFun = Promise.coroutine()一次，多次coFun()，但这里不频繁调用，所以暂时可以这样用
        Promise.coroutine(function * () {
            var col = dbUtil.getDB().collection(colName);
            try {
                var insertArr = [{userid: 1, username: "xiao1", birthday: new Date(), money: 18}, {
                    userid: 2,
                    username: "xiao2",
                    birthday: new Date(),
                    money: 16
                }, {userid: 3, username: "xiao3", birthday: new Date(), money: 20}, {
                    userid: 4,
                    username: "xiao4",
                    birthday: new Date(),
                    money: 14
                }];
                var insertCnt = yield col.insertMany(insertArr);
                test.equal(insertCnt, insertArr.length, "插入数据行数量不对");

                var result = yield col.updateMany({userid: 1}, {$inc: {money: 10}});
                console.log(result);
                test.equal(result, 1);

                result = yield col.findOne({userid: {$lte: 2}});
                console.log(result);
                test.equal(result.money, insertArr[0].money + 10);

                result = yield col.updateOne({userid: 1}, {$set: {money: 10}});
                console.log(result);
                test.equal(result, 1);

                result = yield col.findOne({userid: 1});
                console.log(result);
                test.equal(result.money, 10);

                result = yield col.updateOne({userid: 1}, {
                    userid: 5,
                    username: "xiao5",
                    birthday: new Date(),
                    money: 26
                });
                console.log(result);
                test.equal(result, 1);

                result = yield col.findOne({userid: 5});
                console.log(result);
                test.notEqual(result, null);
            }
            catch (err) {
                test.ifError(err);
            }
            test.done();
        })();
    },
    testPerformance0: function (test) {
        //这个用法效率很低，正确的用法是var coFun = Promise.coroutine()一次，多次coFun()，但这里不频繁调用，所以暂时可以这样用
        Promise.coroutine(function * () {
            try {
                let col = dbUtil.getDB().getMongoDB().collection(colName);
                let insertObj = {userid: 1, username: "xiao1", birthday: new Date(), money: 18};
                yield col.insertOne({userid: 1, username: "xiao1", birthday: new Date(), money: 18});

                let repeatCount = 10000;
                let doneCount = 0;
                let delayTime = 50;

                console.time("单连接FindOne");
                for (let i = 0; i < repeatCount; ++i) {
                    col.find({userid: 1}).toArray(function(err, result){
                        ++doneCount;
                    });
                }
                while (doneCount < repeatCount)
                    yield Promise.delay(delayTime);
                console.timeEnd("单连接FindOne");
            }
            catch (err) {
                test.ifError(err);
            }

            try {
                let col = dbUtil.getDB().getMongoDB().collection(colName);
                let insertObj = {userid: 1, username: "xiao1", birthday: new Date(), money: 18};
                yield col.insertOne({userid: 1, username: "xiao1", birthday: new Date(), money: 18});

                let repeatCount = 10000;
                let doneCount = 0;
                let delayTime = 50;

                console.time("多连接FindOne");
                for (let i = 0; i < repeatCount; ++i) {
                    col = dbUtil.getDB(i).getMongoDB().collection(colName);
                    col.find({userid: 1}).toArray(function(err, result){
                        ++doneCount;
                    });
                }
                while (doneCount < repeatCount)
                    yield Promise.delay(delayTime);
                console.timeEnd("多连接FindOne");
            }
            catch (err) {
                test.ifError(err);
            }

            try {
                let col = dbUtil.getDB().getMongoDB().collection(colName);
                let insertObj = {userid: 1, username: "xiao1", birthday: new Date(), money: 18};
                yield col.insertOne({userid: 1, username: "xiao1", birthday: new Date(), money: 18});

                let repeatCount = 10000;
                let doneCount = 0;
                let delayTime = 50;

                console.time("单连接FindOne");
                var findCoroutine = Promise.coroutine(function * () {
                    yield col.find({userid: 1}).toArray();
                    ++doneCount;
                });
                for (let i = 0; i < repeatCount; ++i) {
                    findCoroutine();
                }
                while (doneCount < repeatCount)
                    yield Promise.delay(delayTime);
                console.timeEnd("单连接FindOne");
            }
            catch (err) {
                test.ifError(err);
            }

            try {
                let col = dbUtil.getDB().getMongoDB().collection(colName);
                let insertObj = {userid: 1, username: "xiao1", birthday: new Date(), money: 18};
                yield col.insertOne({userid: 1, username: "xiao1", birthday: new Date(), money: 18});

                let repeatCount = 10000;
                let doneCount = 0;
                let delayTime = 50;

                console.time("多连接FindOne");
                var findCoroutine2 = Promise.coroutine(function * (i) {
                    let col = dbUtil.getDB(i).getMongoDB().collection(colName);
                    yield col.find({userid: 1}).toArray();
                    ++doneCount;
                });
                for (let i = 0; i < repeatCount; ++i) {
                    findCoroutine2(i);
                }
                while (doneCount < repeatCount)
                    yield Promise.delay(delayTime);
                console.timeEnd("多连接FindOne");
            }
            catch (err) {
                test.ifError(err);
            }

            try {
                let col = dbUtil.getDB().collection(colName);
                let insertObj = {userid: 1, username: "xiao1", birthday: new Date(), money: 18};
                let insertCnt = yield col.insertOne(insertObj);

                let repeatCount = 10000;
                let delayTime = 50;

                console.time("单连接FindOne");
                for (let i = 0; i < repeatCount; ++i) {
                    col.findOne({userid: 1});
                }
                while (dbUtil.getPendingOp() > 0)
                    yield Promise.delay(delayTime);
                console.timeEnd("单连接FindOne");
            }
            catch (err) {
                test.ifError(err);
            }

            try {
                let col = dbUtil.getDB().collection(colName);
                let insertObj = {userid: 1, username: "xiao1", birthday: new Date(), money: 18};
                let insertCnt = yield col.insertOne(insertObj);

                let repeatCount = 10000;
                let delayTime = 50;

                console.time("多连接FindOne");
                for (let i = 0; i < repeatCount; ++i) {
                    col = dbUtil.getDB(i).collection(colName);
                    col.findOne({userid: 1});
                }
                while (dbUtil.getPendingOp() > 0)
                    yield Promise.delay(delayTime);
                console.timeEnd("多连接FindOne");
            }
            catch (err) {
                test.ifError(err);
            }

            test.done();
        })();
    },
    testPerformance1: function (test) {
        //这个用法效率很低，正确的用法是var coFun = Promise.coroutine()一次，多次coFun()，但这里不频繁调用，所以暂时可以这样用
        Promise.coroutine(function * () {
            try {
                let col = dbUtil.getDB().getMongoDB().collection(colName);
                let insertObj = {userid: 1, username: "xiao1", birthday: new Date(), money: 18};
                yield col.insertOne({userid: 1, username: "xiao1", birthday: new Date(), money: 18});

                let repeatCount = 10000;
                let doneCount = 0;
                let delayTime = 50;

                console.time("单连接UpdateOne");
                for (let i = 0; i < repeatCount; ++i) {
                    col.updateOne({userid:1}, {$set:{userid:1, username:"xiao1", birthday:new Date(), money:18}}, function(err, result){
                        ++doneCount;
                    });
                }
                while (doneCount < repeatCount)
                    yield Promise.delay(delayTime);
                console.timeEnd("单连接UpdateOne");
            }
            catch (err) {
                test.ifError(err);
            }

            try {
                let col = dbUtil.getDB().getMongoDB().collection(colName);
                let insertObj = {userid: 1, username: "xiao1", birthday: new Date(), money: 18};
                yield col.insertOne({userid: 1, username: "xiao1", birthday: new Date(), money: 18});

                let repeatCount = 10000;
                let doneCount = 0;
                let delayTime = 50;

                console.time("多连接UpdateOne");
                for (let i = 0; i < repeatCount; ++i) {
                    col = dbUtil.getDB(i).getMongoDB().collection(colName);
                    col.updateOne({userid:1}, {$set:{userid:1, username:"xiao1", birthday:new Date(), money:18}}, function(err, result){
                        ++doneCount;
                    });
                }
                while (doneCount < repeatCount)
                    yield Promise.delay(delayTime);
                console.timeEnd("多连接UpdateOne");
            }
            catch (err) {
                test.ifError(err);
            }

            try {
                let col = dbUtil.getDB().getMongoDB().collection(colName);
                let insertObj = {userid: 1, username: "xiao1", birthday: new Date(), money: 18};
                yield col.insertOne({userid: 1, username: "xiao1", birthday: new Date(), money: 18});

                let repeatCount = 10000;
                let doneCount = 0;
                let delayTime = 50;

                console.time("单连接UpdateOne");
                var updateOneCoroutine = Promise.coroutine(function * () {
                    yield col.updateOne({userid:1}, {$set:{userid:1, username:"xiao1", birthday:new Date(), money:18}});
                    ++doneCount;
                });
                for (let i = 0; i < repeatCount; ++i) {
                    updateOneCoroutine();
                }
                while (doneCount < repeatCount)
                    yield Promise.delay(delayTime);
                console.timeEnd("单连接UpdateOne");
            }
            catch (err) {
                test.ifError(err);
            }

            try {
                let col = dbUtil.getDB().getMongoDB().collection(colName);
                let insertObj = {userid: 1, username: "xiao1", birthday: new Date(), money: 18};
                yield col.insertOne({userid: 1, username: "xiao1", birthday: new Date(), money: 18});

                let repeatCount = 10000;
                let doneCount = 0;
                let delayTime = 50;

                console.time("多连接UpdateOne");
                var updateOneCoroutine2 = Promise.coroutine(function * (i) {
                    let col = dbUtil.getDB(i).getMongoDB().collection(colName);
                    yield col.updateOne({userid:1}, {$set:{userid:1, username:"xiao1", birthday:new Date(), money:18}});
                    ++doneCount;
                });
                for (let i = 0; i < repeatCount; ++i) {
                    updateOneCoroutine2(i);
                }
                while (doneCount < repeatCount)
                    yield Promise.delay(delayTime);
                console.timeEnd("多连接UpdateOne");
            }
            catch (err) {
                test.ifError(err);
            }

            try {
                let col = dbUtil.getDB().collection(colName);
                let insertObj = {userid: 1, username: "xiao1", birthday: new Date(), money: 18};
                let insertCnt = yield col.insertOne(insertObj);

                let repeatCount = 10000;
                let delayTime = 50;

                console.time("单连接UpdateOne");
                for (let i = 0; i < repeatCount; ++i) {
                    col.updateOne({userid:1}, {$set:{userid:1, username:"xiao1", birthday:new Date(), money:18}});
                }
                while (dbUtil.getPendingOp() > 0)
                    yield Promise.delay(delayTime);
                console.timeEnd("单连接UpdateOne");
            }
            catch (err) {
                test.ifError(err);
            }

            try {
                let col = dbUtil.getDB().collection(colName);
                let insertObj = {userid: 1, username: "xiao1", birthday: new Date(), money: 18};
                let insertCnt = yield col.insertOne(insertObj);

                let repeatCount = 10000;
                let delayTime = 50;

                console.time("多连接UpdateOne");
                for (let i = 0; i < repeatCount; ++i) {
                    col = dbUtil.getDB(i).collection(colName);
                    col.updateOne({userid:1}, {$set:{userid:1, username:"xiao1", birthday:new Date(), money:18}});
                }
                while (dbUtil.getPendingOp() > 0)
                    yield Promise.delay(delayTime);
                console.timeEnd("多连接UpdateOne");
            }
            catch (err) {
                test.ifError(err);
            }

            test.done();
        })();
    }
};