"use strict";

var EnumType = require("../../libs/enumType").EnumType;

/**
 * 测试枚举，为了智能提示，这个定义还是有必要的
 * @typedef {object} TestEnumDef
 * @property {function} getByLabel
 * @property {function} getByValue
 * @property {EnumType} enItem1
 * @property {EnumType} enItem2
 * @property {EnumType} enItem3
 */

/**
 * 为了防止跟内置属性同名，最好加上前缀
 * 枚举值要>=0
 * @type {TestEnumDef}
 */
exports.TestEnum = EnumType.define("TestEnum", {
    enItem1 : 1,
    enItem2 : 2,
    enItem3 : 3
});