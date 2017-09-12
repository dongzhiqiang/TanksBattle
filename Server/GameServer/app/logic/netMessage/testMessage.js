"use strict";

var ArrayList = require("../../libs/arrayList").ArrayList;
var TestEnum = require("../enumType/testEnum").TestEnum;

class Component {
    constructor() {
        this.propString = "";
        this.propNumber = 0;
        this.propArray = null;
        this.propEnum = null;
    }

    static fieldsDesc() {
        return {
            propString: {type: String},
            propNumber: {type: Number},
            propArray:  {type: Array, itemType:String},
            propEnum:   {type: TestEnum}
        };
    }
}

class Message
{
    constructor()
    {
        this.propString     = "";
        this.propString2    = "";
        this.propNumber     = 0;
        this.propBoolean    = false;
        this.propNull       = null;
        this.propSubObj     = null;
        this.propSubObj2    = null;
        this.propObjNull    = null;
        this.propArray      = null;
        this.propArray2     = null;
        this.propList       = null;
        this.propList2      = null;
        this.propMap        = null;
        this.propMap2       = null;
        this.propBuf        = null;
        this.propBuf2       = null;
        this.propDate       = null;
        this.propEnum       = null;
    }

    static fieldsDesc()
    {
        return {
            propString: {type:String},
            propString2:{type:String},
            propNumber: {type:Number},
            propBoolean:{type:Boolean},
            propNull:   {type:Object},
            propSubObj: {type:Component, notNull:true},
            propSubObj2:{type:Component},
            propObjNull:{type:Component},
            propArray:  {type:Array, itemType:Component},
            propArray2: {type:Array, itemType:Component},
            propList:   {type:ArrayList, itemType:Component},
            propList2:  {type:ArrayList, itemType:Component},
            propMap:    {type:Map, keyType:String, valType:Component},
            propMap2:   {type:Map, keyType:String, valType:Component},
            propBuf:    {type:Buffer},
            propBuf2:   {type:Buffer},
            propDate:   {type:Date},
            propEnum:   {type:TestEnum}
        };
    }
}
/**
 *
 * @type {Message}
 */
exports.Message = Message;
/**
 *
 * @type {Component}
 */
exports.Component = Component;