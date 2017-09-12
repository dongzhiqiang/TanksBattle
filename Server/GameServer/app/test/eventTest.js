"use strict";

var eventMgr = require("../libs/eventMgr");

function showEventDebugInfo(tag)
{
    //return;
    console.log("=====" + tag　+ "=====");
    eventMgr.showDebugInfo();
    console.log("=====" + tag　+ "=====");
}

class TestNotifier
{
    fireNow(eventName, context)
    {
        eventMgr.fire(eventName, context, this);
    }
}

class TestObserver
{
    start(notifier)
    {
        eventMgr.addListener(this, "test1", notifier);
        showEventDebugInfo(1);
        eventMgr.addListener(this, "test2", notifier);
        showEventDebugInfo(2);
        eventMgr.addListener(this, "test3", notifier);
        showEventDebugInfo(3);
        eventMgr.addGlobalListener(this, "test3");
        showEventDebugInfo(4);
        eventMgr.addGlobalListener(this, "test4");
        showEventDebugInfo(4);
        console.log("===1====");
        console.log("hasListener(notifier):" + eventMgr.hasListener(this, "test3", notifier));
        console.log("hasListener:" + eventMgr.hasListener(this, "test3"));
        console.log("hasGlobalListener:" + eventMgr.hasGlobalListener(this, "test3"));
        console.log("===1====");
    }

    onEvent(eventName, context, notifier)
    {
        console.log("eventName=" + eventName + ", context=" + context + ", notifier=" + notifier.constructor.name);

        if (eventName === "test3")
        {
            eventMgr.removeListener(this, eventName);
            showEventDebugInfo(5.1);
            eventMgr.removeGlobalListener(this, eventName);
            showEventDebugInfo(5.2);
            if (context)
            {
                eventMgr.addListener(this, eventName, notifier);
                showEventDebugInfo(5.3);
                eventMgr.addGlobalListener(this, eventName);
                showEventDebugInfo(5.4);
            }
        }
    }

    stop()
    {
        eventMgr.removeListener(this);
        eventMgr.removeGlobalListener(this);
        showEventDebugInfo(7);
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
        var notifier = new TestNotifier();
        var observer = new TestObserver();
        observer.start(notifier);
        notifier.fireNow("test1", "test1_1");
        notifier.fireNow("test2", "test2_1");
        notifier.fireNow("test3", "test3_1");
        console.log("===2====");
        console.log("hasListener(notifier):" + eventMgr.hasListener(this, "test3", notifier));
        console.log("hasListener:" + eventMgr.hasListener(this, "test3"));
        console.log("hasGlobalListener:" + eventMgr.hasGlobalListener(this, "test3"));
        console.log("===2====");
        showEventDebugInfo(8);
        notifier.fireNow("test1", "test1_2");
        notifier.fireNow("test2", "test2_2");
        notifier.fireNow("test3", "");
        console.log("===2====");
        console.log("hasListener(notifier):" + eventMgr.hasListener(this, "test3", notifier));
        console.log("hasListener:" + eventMgr.hasListener(this, "test3"));
        console.log("hasGlobalListener:" + eventMgr.hasGlobalListener(this, "test3"));
        console.log("===2====");
        showEventDebugInfo(9);
        observer.stop();
        notifier.fireNow("test1", "test1_3");
        notifier.fireNow("test2", "test2_3");
        notifier.fireNow("test3", "test3_3");
        showEventDebugInfo(10);
        test.done();
    },
    testTwo: function (test) {
        var notifier = new TestNotifier();
        function onEvent(eventName, context, notifier)
        {
            console.log("eventName=" + eventName + ", context=" + context + ", notifier=" + notifier.constructor.name);
        }
        eventMgr.addListener(onEvent, "test1", notifier);
        eventMgr.addListener(onEvent, "test2", notifier);
        eventMgr.addListener(onEvent, "test3", notifier);
        eventMgr.addGlobalListener(onEvent, "test3");
        showEventDebugInfo(1);
        notifier.fireNow("test1", "test1_1");
        notifier.fireNow("test2", "test2_1");
        notifier.fireNow("test3", "test3_1");
        eventMgr.removeListener(onEvent, "test1", notifier);
        eventMgr.removeListener(onEvent, "test2", notifier);
        eventMgr.removeListener(onEvent, "test3", notifier);
        eventMgr.removeGlobalListener(onEvent, "test3");
        notifier.fireNow("test1", "test1_2");
        notifier.fireNow("test2", "test2_2");
        notifier.fireNow("test3", "test3_2");
        showEventDebugInfo(2);
        eventMgr.removeListener(onEvent);
        eventMgr.removeGlobalListener(onEvent);
        showEventDebugInfo(3);
        test.done();
    },
    testThree: function (test) {
        var notifier = new TestNotifier();
        function onEvent(eventName, context, notifier)
        {
            console.log("eventName=" + eventName + ", context=" + context + ", notifier=" + notifier.constructor.name);
        }
        eventMgr.addListener(onEvent, "test1", notifier);
        eventMgr.addListener(onEvent, "test2", notifier);
        eventMgr.addListener(onEvent, "test3", notifier);
        showEventDebugInfo(1);
        notifier.fireNow("test1", "test1_1");
        notifier.fireNow("test2", "test2_1");
        notifier.fireNow("test3", "test3_1");
        eventMgr.removeNotifier(notifier);
        notifier.fireNow("test1", "test1_2");
        notifier.fireNow("test2", "test2_2");
        notifier.fireNow("test3", "test3_2");
        showEventDebugInfo(2);
        test.done();
    },
    testFour:function(test) {
        var notifier = new TestNotifier();
        function onEvent(eventName, context, notifier)
        {
            //console.log("eventName=" + eventName + ", context=" + context + ", notifier=" + notifier.constructor.name);
        }
        var i;
        var cnt = 1000000;
        console.time("all");
        for (i = 0; i < cnt; ++i)
        {
            //if (i === 0)
                eventMgr.addListener(onEvent, "test1", notifier);
            eventMgr.fire("test1", "test1", notifier);
            //if (i === cnt - 1)
                eventMgr.removeListener(onEvent);
        }
        console.timeEnd("all");
        showEventDebugInfo(1);
        test.done();
    }
};