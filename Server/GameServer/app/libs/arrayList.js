"use strict";

/**
 * 用处不大，主要用于在数据串行化到客户端时区分是Array还是List
 * @extends Array
 */
class ArrayList extends Array
{
    constructor()
    {
        var len = arguments.length;
        if (len === 1)
            super(arguments[0]);
        else if (len < 1)
            super();
        else
            super(...arguments);
    }
}

ArrayList.isArrayList = function(o)
{
    return o instanceof ArrayList;
};


exports.ArrayList = ArrayList;
