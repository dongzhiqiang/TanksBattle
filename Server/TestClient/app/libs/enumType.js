"use strict";

/**
 * @class
 * @param {string} lbl
 * @param {number} val
 */
function EnumType(lbl, val)
{
    this.lbl = lbl;
    this.val = val;
}

EnumType.getByLabel = function(lbl) {
};

EnumType.getByValue = function(val) {
};

EnumType.isEnumType = function(o) {
    return o instanceof EnumType;
};

/**
 * 为了防止跟内置属性同名，最好加上前缀
 * 枚举值要>=0
 * @param {string} name
 * @param {object.<string, number>} initObj
 * @returns {EnumType}
 */
EnumType.define = function(name, initObj)
{
    var subType = function(lbl, val) {
        EnumType.call(this, lbl, val);
    };

    subType.prototype = Object.create(EnumType.prototype);
    subType.prototype.constructor = subType;

    Object.defineProperty(subType, "name", {get:function () {return name;}});
    Object.defineProperty(subType, "_mapByLabel", {value:new Map()});
    Object.defineProperty(subType, "_mapByValue", {value:new Map()});
    Object.defineProperty(subType, "getByLabel", {value:function(lbl) { return subType._mapByLabel.get(lbl); }});
    Object.defineProperty(subType, "getByValue", {value:function(val) { return subType._mapByValue.get(val); }});
    for (var lbl in initObj)
    {
        if (initObj.hasOwnProperty(lbl))
        {
            var item = new subType(lbl, initObj[lbl]);
            Object.defineProperty(subType, lbl, {
                enumerable: true,
                configurable: false,
                writable: false,
                value: item
            });
            subType._mapByLabel.set(item.lbl, item);
            subType._mapByValue.set(item.val, item);
        }
    }
    return subType;
};

exports.EnumType = EnumType;