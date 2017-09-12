#region Header
/**
 * 名称：ProtocolCoder
 
 * 日期：2015.11.18
 * 描述：用于将服务端发来的字节流转为对应的类型
 *  1.类和枚举的反射信息都先计算好了，以提升效率,见EnumDef和ObjectDef
 **/
#endregion
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
namespace NetCore
{
    public class ProtocolCoder : Singleton<ProtocolCoder>
    {
        
        
        #region Fields
        private Dictionary<byte, Proxy> m_proxys = new Dictionary<byte, Proxy>();

        private Dictionary<int, EnumDef> m_enumIdxs = new Dictionary<int, EnumDef>();
        private Dictionary<System.Type, EnumDef> m_enumDefs = new Dictionary<System.Type, EnumDef>();
        private Dictionary<int, TypeDef> m_typeIdxs = new Dictionary<int, TypeDef>();
        private Dictionary<System.Type, TypeDef> m_typeDefs = new Dictionary<System.Type, TypeDef>();
        private Dictionary<int, MapDef> m_mapIdxs = new Dictionary<int, MapDef>();
        private Dictionary<System.Type, MapDef> m_mapDefs = new Dictionary<System.Type, MapDef>();
        private Dictionary<int, CollectionDef> m_collectionIdxs = new Dictionary<int, CollectionDef>();
        private Dictionary<System.Type, CollectionDef> m_collectionDefs = new Dictionary<System.Type, CollectionDef>();

        int m_indent = 0;
        StringBuilder m_log= new StringBuilder();
        #endregion


        #region Properties
        public StringBuilder Log { get{return m_log;}}
        public int Indent { get{return m_indent;}set{m_indent=value;}}
        #endregion


        #region Constructors
        public ProtocolCoder()
        {
            m_proxys.Add(Types.ARRAY, new ArrayProxy());
            m_proxys.Add(Types.BOOLEAN, new BooleanProxy());
            //m_proxys.Add(Types.BYTE_ARRAY, new BytesProxy());//暂时不支持，要支持的话要注意c#的byte是无符号的，java的byte是有符号的
            m_proxys.Add(Types.DATE_TIME, new DateProxy());
            m_proxys.Add(Types.ENUM, new EnumProxy());
            m_proxys.Add(Types.MAP, new MapProxy());
            m_proxys.Add(Types.NULL, new NullProxy());
            m_proxys.Add(Types.NUMBER, new NumberProxy());
            m_proxys.Add(Types.OBJECT, new ObjectProxy());
            m_proxys.Add(Types.STRING, new StringProxy());
            m_proxys.Add(Types.COLLECTION, new CollectionProxy());

            
        }
        #endregion


        #region Static Methods
        public static bool CanRegister(System.Type t)
        {
            return t!=null && (t.IsClass || t.IsEnum || t.IsArray) && !t.IsPrimitive && t != typeof(string);            
        }
        public static bool IsObjectType(System.Type t)
        {
            //这里不能序列化模板类型、c#原生类型、继承自其他类的类型，不然可能会序列化一些不想要的东西
            if(t == null || !t.IsClass || t.IsGenericType||t.IsPrimitive)
                return false;

            //字符串排除在外
            if(t == typeof(string))
                return false;

            System.Type baseType = t.BaseType;
            if(baseType != null && baseType != typeof(System.Object))
                return false;
            
            return true;
        }
        #endregion


        #region Private Methods
        
        #endregion

        public void Register(System.Type clz)
        {
        
		    if (clz.IsEnum) {
                if (m_enumDefs.Get(clz) == null)
                {
                    EnumDef def = EnumDef.valueOf(clz);
                    if (def == null) return;
                    m_enumIdxs.Add(def.code, def);
                    m_enumDefs.Add(clz, def);
                    m_log.AppendFormat("Protocol注册了枚举:{0} code:{1} \n", clz.Name, def.code);
			    }
		    }
            else if(!clz.IsClass){
                Debuger.LogError("不能注册的类型:{0}", clz.Name);
            }
            else if (clz.IsArray)
            {
                System.Type elemType = clz.GetElementType();
                if (ProtocolCoder.CanRegister(elemType))
                    ProtocolCoder.instance.Register(elemType);

            }
            else if (clz.GetInterface("System.Collections.IList") != null)
            {
                if (m_collectionDefs.Get(clz) == null)
                {
                    CollectionDef def = CollectionDef.valueOf(clz);
                    if (def == null) return;
                    m_collectionIdxs.Add(def.code, def);
                    m_collectionDefs.Add(clz, def);
                    m_log.AppendFormat("Protocol注册了list:{0} code:{1} element:{2} \n", clz.Name, def.code, def.elementType.Name);
                }
            }
            else if (clz.GetInterface("System.Collections.IDictionary") != null)
            {
                if (m_mapDefs.Get(clz) == null)
                {
                    MapDef def = MapDef.valueOf(clz);
                    if (def == null)return;
                    m_mapIdxs.Add(def.code, def);
                    m_mapDefs.Add(clz, def);
                    m_log.AppendFormat("Protocol注册了map:{0} code:{1} key:{2} value:{3} \n", clz.Name, def.code, def.keyType.Name, def.valueType.Name);
                }
            }
            else
            {
                if (m_typeDefs.Get(clz) == null)
                {
                    if (!IsObjectType(clz))
                    {
                        Debuger.LogError("不能序列化的类型，不能序列化模板类型、c#原生类型、继承自其他类的类型:{0}", clz.Name);
                        return;
                    }
                    TimeCheck check = new TimeCheck();
                    TypeDef def = TypeDef.valueOf(clz, m_typeDefs);
                    if (def == null) return;
                    m_typeIdxs.Add(def.code, def);
                    
                    m_log.AppendFormat("Protocol注册了类:{0} code:{1} 耗时:{2} \n", clz.Name, def.code, check.delayMS);
                }
		    }
	    }

        public bool Encode(IoBuffer buffer, object obj)
        {
            //类型
            byte type;
            
            //System.Type t = obj != null ? obj.GetType():null; 
            if (obj == null )
                type = Types.NULL;
            else if (obj is byte || obj is short || obj is int || obj is long || obj is float || obj is double)
                type = Types.NUMBER;
            else if(obj is string)
                type = Types.STRING;
            else if(obj is bool)
                type = Types.BOOLEAN;
            else if (obj is System.Enum)//t.IsEnum
                type = Types.ENUM;
            else if(obj is System.DateTime)
                type = Types.DATE_TIME;
            else if (obj is IDictionary)
                type = Types.MAP;
            else if (obj is System.Array)//t.IsArray
                type = Types.ARRAY;
            else if (obj is IList)
                type = Types.COLLECTION;
            else {
                System.Type t = obj.GetType();
                if (IsObjectType(t))//这里不能序列化模板类型、c#原生类型、继承自其他类的类型，不然可能会序列化一些不想要的东西
                {
                    type = Types.OBJECT;
                }
                else
                {
                    Debuger.LogError("不能序列化的类型，不能序列化模板类型、c#原生类型、继承自其他类的类型：" + obj.GetType().Name);
                    return false;
                }
            }
                

            Proxy proxy = m_proxys.Get(type);
            if (proxy == null)
            {
                Debuger.LogError("找不到序列化类。{0}", obj.GetType().ToString());
                return false;
            }

            if(!proxy.setValue(buffer, obj))
                return false;
            return true;
        }

        public bool Decode(IoBuffer buffer, System.Type type, out object value)
        {
            byte flag = buffer.ReadByte();

            byte t = Proxy.getFlagTypes(flag);
            Proxy proxy = m_proxys.Get(t);
            if (proxy == null)
            {
                Debuger.LogError("找不到序列化类。flag:{0:x}", t);
                value = null;
                return false;
            }


            return proxy.getValue(buffer, type, flag, out value) ;
        }


	    public EnumDef getEnumDef(int def) {
		    return m_enumIdxs.Get(def);
	    }

	   
	    public EnumDef getEnumDef(System.Type def) {
            return m_enumDefs.Get(def);
	    }
     
        public TypeDef getTypeDef(int def)
        {
            return m_typeIdxs.Get(def);
        }

     
        public TypeDef getTypeDef(System.Type def)
        {
            return m_typeDefs.Get(def);
        }

        public MapDef getMapDef(int def)
        {
            return m_mapIdxs.Get(def);
        }


        public MapDef getMapDef(System.Type def)
        {
            return m_mapDefs.Get(def);
        }
        public CollectionDef getCollectionDef(int def)
        {
            return m_collectionIdxs.Get(def);
        }


        public CollectionDef getCollectionDef(System.Type def)
        {
            return m_collectionDefs.Get(def);
        }
    }
}