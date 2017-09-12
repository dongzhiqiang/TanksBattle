"use strict";

var timerMgr = require("../libs/timerMgr");

class TestClass
{
    start()
    {
        timerMgr.addTimer(this, 1, 1000, true);
        timerMgr.addTimer(this, 2, 1000, false);

        this.counter = 0;
    }

    onTimer(timerID)
    {
        ++this.counter;
        console.log("timerID=" + timerID + ", counter=" + this.counter);

        if (this.counter === 3)
        {
            console.log("是否存在2号定时器：" + timerMgr.hasTimer(this, 2));
            timerMgr.addTimer(this, 2, 500, false);
        }
    }

    stop()
    {
        timerMgr.removeTimer(this);
    }
}

module.exports = {
    setUp: function (callback) {
        console.log("===========\r\n");
        callback();
    },
    tearDown: function (callback) {
        callback();
    },
    testOne: function (test) {
        var o = new TestClass();
        o.start();
        setTimeout(function(){
            o.stop();
            test.done();
        }, 10000);
    },
    testTwo: function (test) {
        var counter = 0;
        function onTimer(timerID) {
            ++counter;
            console.log("timerID=" + timerID + ", counter=" + counter);
            if (counter > 5)
            {
                timerMgr.removeTimer(onTimer);
                test.done();
            }
        }
        timerMgr.addTimer(onTimer, 1, 1000);
        timerMgr.addTimer(onTimer, 2, 100, true);
    }
};