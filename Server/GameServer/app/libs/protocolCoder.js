"use strict";

var ArrayList = require("./arrayList").ArrayList;

var logUtil = require("./logUtil");
var EnumType = require("./enumType").EnumType;
var BaseProxy = require("./protocolProxy/baseProxy").BaseProxy;
var ObjectProxy = require("./protocolProxy/objectProxy").ObjectProxy;
var StringProxy = require("./protocolProxy/stringProxy").StringProxy;
var ArrayProxy = require("./protocolProxy/arrayProxy").ArrayProxy;
var MapProxy = require("./protocolProxy/mapProxy").MapProxy;
var BytesProxy = require("./protocolProxy/bytesProxy").BytesProxy;
var DateProxy = require("./protocolProxy/dateProxy").DateProxy;
var ListProxy = require("./protocolProxy/listProxy").ListProxy;
var BooleanProxy = require("./protocolProxy/booleanProxy").BooleanProxy;
var NumberProxy = require("./protocolProxy/numberProxy").NumberProxy;
var NullProxy = require("./protocolProxy/nullProxy").NullProxy;
var EnumProxy = require("./protocolProxy/enumProxy").EnumProxy;

class FieldDef
{
    constructor(name, type, notNull)
    {
        /**
         * 字段名
         */
        this.name = name;
        this.type = type;
        this.notNull = !!notNull;
    }
}

class TypeDef
{
    constructor(name, code, type, fields)
    {
        /**
         * 类型名
         * @type {string}
         */
        this.name = name;
        /**
         * @type {number}
         */
        this.code = code;
        /**
         * @type {function}
         */
        this.type = type;
        /**
         * @type {FieldDef[]}
         */
        this.fields = fields || [];
    }
}

class EnumDef
{
    constructor(name, code, type)
    {
        /**
         * @type {string}
         */
        this.name = name;
        /**
         * @type {number}
         */
        this.code = code;
        /**
         * @type {EnumType}
         */
        this.type = type;
    }
}

class ProtocolCoder
{
    constructor()
    {
        var proxiesMap = {};
        proxiesMap[BaseProxy.TYPE_FLAG_OBJECT]    = new ObjectProxy;
        proxiesMap[BaseProxy.TYPE_FLAG_STRING]    = new StringProxy;
        proxiesMap[BaseProxy.TYPE_FLAG_ARRAY]     = new ArrayProxy;
        proxiesMap[BaseProxy.TYPE_FLAG_MAP]       = new MapProxy;
        proxiesMap[BaseProxy.TYPE_FLAG_BYTEARRAY] = new BytesProxy;
        proxiesMap[BaseProxy.TYPE_FLAG_DATETIME]  = new DateProxy;
        proxiesMap[BaseProxy.TYPE_FLAG_LIST]      = new ListProxy;
        proxiesMap[BaseProxy.TYPE_FLAG_ENUM]      = new EnumProxy;
        proxiesMap[BaseProxy.TYPE_FLAG_BOOLEAN]   = new BooleanProxy;
        proxiesMap[BaseProxy.TYPE_FLAG_NUMBER]    = new NumberProxy;
        proxiesMap[BaseProxy.TYPE_FLAG_NULL]      = new NullProxy;
        this._proxiesMap = proxiesMap;

        /**
         * 类型名HashCode与类型描述对象的映射
         * @type {Map.<number, TypeDef>}
         */
        this._typeCodeMap = new Map();
        /**
         * 类型与类型描述对象的映射
         * @type {Map.<function, TypeDef>}
         */
        this._typeDefsMap = new Map();
        /**
         * 枚举类型名HashCode与类型描述对象的映射
         * @type {Map.<number, EnumDef>}
         */
        this._enumCodeMap = new Map();
        /**
         * 枚举类型与类型描述对象的映射
         * @type {Map.<function, EnumDef>}
         */
        this._enumDefsMap = new Map();
    }

    /**
     *
     * @param {IOBuffer} ioBuf
     * @param {*} obj
     */
    encode(ioBuf, obj) {
        var typeFlag = BaseProxy.TYPE_FLAG_UNKOWN;
        if (obj === null || obj === undefined)
            typeFlag = BaseProxy.TYPE_FLAG_NULL;
        else if (Object.isNumber(obj))
            typeFlag = BaseProxy.TYPE_FLAG_NUMBER;
        else if (Object.isString(obj))
            typeFlag = BaseProxy.TYPE_FLAG_STRING;
        else if (Object.isBoolean(obj))
            typeFlag = BaseProxy.TYPE_FLAG_BOOLEAN;
        else if (ArrayList.isArrayList(obj))    //ArrayList要放在Array前面，因为有继承关系
            typeFlag = BaseProxy.TYPE_FLAG_LIST;
        else if (Object.isArray(obj))
            typeFlag = BaseProxy.TYPE_FLAG_ARRAY;
        else if (Object.isMap(obj))
            typeFlag = BaseProxy.TYPE_FLAG_MAP;
        else if (Object.isDate(obj))
            typeFlag = BaseProxy.TYPE_FLAG_DATETIME;
        else if (EnumType.isEnumType(obj))
            typeFlag = BaseProxy.TYPE_FLAG_ENUM;
        else if (Buffer.isBuffer(obj))
            typeFlag = BaseProxy.TYPE_FLAG_BYTEARRAY;
        else if (Object.isObject(obj))
            typeFlag = BaseProxy.TYPE_FLAG_OBJECT;
        else
            throw new Error("不能序列化的类型：" + typeof(obj));
        var proxy = this._proxiesMap[typeFlag];
        if (!proxy)
            throw new Error("找不到序列化类，flag：" + typeFlag);
        proxy.setValue(ioBuf, obj);
    }

    /**
     *
     * @param {IOBuffer} ioBuf
     * @returns {*}
     */
    decode(ioBuf) {
        var flag = ioBuf.readUInt8();
        if (flag === null)
        {
            throw new Error("ProtocolCoder~decode 数据字节不足");
        }

        var typeFlag = BaseProxy.getFlagType(flag);
        var proxy = this._proxiesMap[typeFlag];
        if (!proxy)
        {
            throw new Error("找不到序列化类，flag：" + typeFlag);
        }
        return proxy.getValue(ioBuf, flag);
    }

    /**
     * 判断是否能（要）注册
     * @param {function} type
     * @returns {boolean}
     */
    canRegisterType(type)
    {
        return Object.isFunction(type) && !!type.fieldsDesc;
    }

    /**
     *
     * @param {function} type
     * @return {TypeDef|null} 注册失败返回null
     */
    registerType(type)
    {
        if (!this.canRegisterType(type))
        {
            logUtil.warn("registerType 不需要注册的类型：" + type);
            return null;
        }

        //Function.name暂时是非ES标准
        var code = type.name.hashCode();

        //检查是否注册或不同类型但HashCode一样
        var typeDef = this._typeCodeMap.get(code);
        if (typeDef)
        {
            if (typeDef.type !== type)
                //Function.name暂时是非ES标准
                logUtil.error("registerType 发现不同类型但HashCode一样，分别是：" + type.name + "，" + typeDef.type.name);
            return null;
        }

        //先注册
        //Function.name暂时是非ES标准
        typeDef = new TypeDef(type.name, code, type, []);
        this._typeCodeMap.set(code, typeDef);
        this._typeDefsMap.set(type, typeDef);

        //注册属性成员
        var fieldsDesc = type.fieldsDesc();
        for (var name in fieldsDesc)
        {
            if (fieldsDesc.hasOwnProperty(name))
            {
                var desc = fieldsDesc[name];
                var fieldType = new FieldDef(name, desc.type, desc.notNull);
                typeDef.fields.push(fieldType);

                if (this.canRegisterType(desc.type)) {
                    this.registerType(desc.type);
                    if (desc.itemType && this.canRegisterType(desc.itemType))
                        this.registerType(desc.itemType);
                    if (desc.keyType && this.canRegisterType(desc.keyType))
                        this.registerType(desc.keyType);
                    if (desc.valType && this.canRegisterType(desc.valType))
                        this.registerType(desc.valType);
                }
                else if (this.canRegisterEnum(desc.type))
                {
                    this.registerEnum(desc.type);
                }
            }
        }
        return typeDef;
    }

    /**
     * 获取类型定义，找不到就返回undefined
     * @param {number|function} key
     * @returns {TypeDef|undefined}
     */
    getTypeDef(key)
    {
        if (Object.isNumber(key))
            return this._typeCodeMap.get(key);
        else
            return this._typeDefsMap.get(key);
    }

    /**
     * 判断是否能（要）注册
     * @param {EnumType} type
     * @returns {boolean}
     */
    canRegisterEnum(type)
    {
        return type && Object.isSubClass(type, EnumType);
    }

    /**
     *
     * @param {EnumType} type
     * @return {EnumDef|null} 注册失败返回null
     */
    registerEnum(type)
    {
        if (!this.canRegisterEnum(type))
        {
            logUtil.warn("registerEnum 不是枚举类型：" + type);
            return null;
        }

        //Function.name暂时是非ES标准
        var code = type.name.hashCode();

        //检查是否注册或不同类型但HashCode一样
        var enumDef = this._enumCodeMap.get(code);
        if (enumDef)
        {
            if (enumDef.type !== type)
                //Function.name暂时是非ES标准
                logUtil.error("registerEnum 发现不同类型但HashCode一样，分别是：" + type.name + "，" + enumDef.type.name);
            return null;
        }

        //先注册
        //Function.name暂时是非ES标准
        enumDef = new EnumDef(type.name, code, type);
        this._enumCodeMap.set(code, enumDef);
        this._enumDefsMap.set(type, enumDef);
        return enumDef;
    }

    /**
     * 获取枚举类型定义，找不到就返回undefined
     * @param {number|EnumType} key
     * @returns {EnumDef|undefined}
     */
    getEnumDef(key)
    {
        if (Object.isNumber(key))
            return this._enumCodeMap.get(key);
        else
            return this._enumDefsMap.get(key);
    }
}

/**
 *
 * @type {ProtocolCoder}
 */
exports.instance = new ProtocolCoder();