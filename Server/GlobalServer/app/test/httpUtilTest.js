"use strict";

var http = require("http");
var https = require("https");
var fs = require("fs");
var Promise = require("bluebird");
var appUtil  = require("../libs/appUtil");
var httpUtil  = require("../libs/httpUtil");

module.exports = {
    setUp: function (callback) {
        var portHttp = 4321;
        var portHttps = 4322;
        var emptyAlt = "没数据来啊";
        var handler = function (req, res) {
            var data = "";
            req.on("data", function (chunk) {
                data += chunk;
            });
            req.on("end", function () {
                res.writeHead(200, {"Content-Type": "text/plain"});
                res.end(data || emptyAlt);
            });
        };
        var options = {
            key:  fs.readFileSync("data/http_certs/key.pem"),
            cert: fs.readFileSync("data/http_certs/cert.pem")
        };
        var self = this;

        process.env.NODE_TLS_REJECT_UNAUTHORIZED = "0";

        self.emptyAlt = emptyAlt;
        self.httpSrv = http.createServer(handler);
        self.httpSrv.listen(portHttp, function () {
            self.portHttp = portHttp;
            console.log("Http server listen on " + portHttp);

            if (self.portHttp && self.portHttps)
                callback();
        });
        self.httpsSrv = https.createServer(options, handler);
        self.httpsSrv.listen(portHttps, function () {
            self.portHttps = portHttps;
            console.log("Https server listen on " + portHttps);

            if (self.portHttp && self.portHttps)
                callback();
        });
    },
    tearDown: function (callback) {
        var self = this;

        self.httpSrv.close(function () {
            console.log("Http server close");
            self.portHttp = undefined;

            if (!self.portHttp && !self.portHttps)
                callback();
        });
        self.httpsSrv.close(function () {
            console.log("Https server close");
            self.portHttps = undefined;

            if (!self.portHttp && !self.portHttps)
                callback();
        });
    },
    testGet: function (test) {
        var self = this;

        httpUtil.doGet("http://127.0.0.1:" + self.portHttp, function (err, res) {
            console.log(res);
            test.ifError(err);
            test.equal(res, self.emptyAlt);
            test.done();
        }, "text");
    },
    testGetHttps: function (test) {
        var self = this;

        httpUtil.doGet("https://127.0.0.1:" + self.portHttps, function (err, res) {
            console.log(res);
            test.ifError(err);
            test.equal(res, self.emptyAlt);
            test.done();
        }, "text");
    },
    testPost: function (test) {
        var self = this;

        var post = "我是提交的数据";
        httpUtil.doPost("http://127.0.0.1:" + self.portHttp, post, function (err, res) {
            console.log(res);
            test.ifError(err);
            test.equal(res, post);
            test.done();
        }, "text");
    },
    testPostHttps: function (test) {
        var self = this;

        var post = "我是提交的数据";
        httpUtil.doPost("https://127.0.0.1:" + self.portHttps, post, function (err, res) {
            console.log(res);
            test.ifError(err);
            test.equal(res, post);
            test.done();
        }, "text");
    },
    testPostJson: function (test) {
        var self = this;

        var post = {a:1,b:2,c:3};
        httpUtil.doPost("https://127.0.0.1:" + self.portHttps, post, function (err, res) {
            console.log(res);
            test.ifError(err);
            test.deepEqual(res, post);
            test.done();
        }, "json");
    },
    testPostQuery: function (test) {
        var self = this;

        var post = {a:1,b:2,c:3};
        httpUtil.doPost("https://127.0.0.1:" + self.portHttps, httpUtil.queryObjToString(post), function (err, res) {
            console.log(res);
            test.ifError(err);
            test.deepEqual(res, post);
            test.done();
        }, "query");
    },
    testPostBuffer: function (test) {
        var self = this;

        var post = {a:1,b:2,c:3};
        httpUtil.doPost("https://127.0.0.1:" + self.portHttps, httpUtil.queryObjToString(post), function (err, res) {
            test.ifError(err);
            res = res.toString();
            console.log(res);
            res = httpUtil.getQueryObj(res);
            test.deepEqual(res, post);
            test.done();
        }, "buffer");
    },
    testGetCoroutine: function (test) {
        var self = this;

        var testGetWithPromise = Promise.coroutine(function * (url, resultType, headers) {
            console.log("begin");
            try
            {
                var res = yield httpUtil.doGetCoroutine(url, resultType, headers);
                console.log("res:" + res);
                test.equal(res, self.emptyAlt);
            }
            catch (err)
            {
                test.ifError(err);
            }
            test.done();
            console.log("end");
        });

        testGetWithPromise("http://127.0.0.1:" + self.portHttp, "text");
    },
    testPostCoroutine: function (test) {
        var self = this;
        var post = "我是提交的数据";

        var testPostWithPromise = Promise.coroutine(function * (url, resultType, headers) {
            console.log("begin");
            try
            {
                var res = yield httpUtil.doPostCoroutine(url, post, resultType, headers);
                console.log("res:" + res);
                test.equal(res, post);
            }
            catch (err)
            {
                test.ifError(err);
            }
            test.done();
            console.log("end");
        });

        testPostWithPromise("http://127.0.0.1:" + self.portHttp, "text");
    },
    testPostCoroutineWithError: function (test) {
        var self = this;
        var post = "我是提交的数据";

        var testPostWithPromise = Promise.coroutine(function * (url, resultType, headers) {
            console.log("begin");
            var errObj = null;
            try
            {
                var res = yield httpUtil.doPostCoroutine(url, post, resultType, headers);
                console.log("res:" + res);
                test.equal(res, post);
            }
            catch (err)
            {
                errObj = err;
            }
            test.notEqual(errObj, null, "居然没有HTTP报错？");
            test.done();
            console.log("end");
        });

        testPostWithPromise("http://127.0.0.1:" + (self.portHttp + 1000), "text");
    }
};