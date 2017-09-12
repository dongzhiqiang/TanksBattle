package com.engine.common.protocol;

import java.io.EOFException;
import java.io.IOException;
import java.lang.reflect.Array;
import java.lang.reflect.ParameterizedType;
import java.lang.reflect.Type;
import java.nio.ByteBuffer;
import java.util.ArrayList;
import java.util.Collection;
import java.util.Collections;
import java.util.HashSet;
import java.util.List;
import java.util.Map;
import java.util.Set;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.ConcurrentMap;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.atomic.AtomicInteger;

import org.apache.commons.beanutils.ConvertUtils;
import org.apache.commons.lang3.reflect.TypeUtils;
import org.apache.mina.core.buffer.IoBuffer;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.slf4j.helpers.FormattingTuple;
import org.slf4j.helpers.MessageFormatter;

import com.engine.common.protocol.def.EnumDef;
import com.engine.common.protocol.def.FieldDef;
import com.engine.common.protocol.def.TypeDef;
import com.engine.common.protocol.exception.ObjectConvertException;
import com.engine.common.protocol.proxy.ArrayProxy;
import com.engine.common.protocol.proxy.BooleanProxy;
import com.engine.common.protocol.proxy.BytesProxy;
import com.engine.common.protocol.proxy.CollectionProxy;
import com.engine.common.protocol.proxy.DateProxy;
import com.engine.common.protocol.proxy.EnumProxy;
import com.engine.common.protocol.proxy.MapProxy;
import com.engine.common.protocol.proxy.NullProxy;
import com.engine.common.protocol.proxy.NumberProxy;
import com.engine.common.protocol.proxy.ObjectProxy;
import com.engine.common.protocol.proxy.Proxy;
import com.engine.common.protocol.proxy.StringProxy;
import com.engine.common.protocol.utils.QuickLZUtils;
import com.engine.common.utils.codec.CryptUtils;
import com.engine.common.utils.codec.ZlibUtils;

/**
 * 编解码上下文
 * 
 */
public class Transfer {
	protected Logger log = LoggerFactory.getLogger(getClass());

	// 默认缓冲区初始大小
	private static final int DEFAULT_SIZE = 2048;

	// 枚举定义
	private AtomicInteger enumCurrent = new AtomicInteger();
	private ConcurrentMap<Integer, EnumDef> enumIdxs = new ConcurrentHashMap<Integer, EnumDef>();
	private ConcurrentMap<Class<?>, EnumDef> enumDefs = new ConcurrentHashMap<Class<?>, EnumDef>();
	// 类型定义
	private AtomicInteger typeCurrent = new AtomicInteger();
	private ConcurrentMap<Integer, TypeDef> typeIdxs = new ConcurrentHashMap<Integer, TypeDef>();
	private ConcurrentMap<Class<?>, TypeDef> typeDefs = new ConcurrentHashMap<Class<?>, TypeDef>();
	// 类型映射
	private ConcurrentMap<Class<?>, TypeDef> aliasDefs = new ConcurrentHashMap<Class<?>, TypeDef>();
	// MAP对象
	private ConcurrentMap<Class<?>, TypeDef> mappedDefs = new ConcurrentHashMap<Class<?>, TypeDef>();

	private ConcurrentMap<Byte, Proxy<?>> proxys = new ConcurrentHashMap<Byte, Proxy<?>>();
	private byte[] description;
	private String md5Description;

	public Transfer() {
		// 初始化类型代理
		proxys.put(Types.ARRAY, new ArrayProxy());
		proxys.put(Types.BOOLEAN, new BooleanProxy());
		proxys.put(Types.BYTE_ARRAY, new BytesProxy());
		proxys.put(Types.DATE_TIME, new DateProxy());
		proxys.put(Types.ENUM, new EnumProxy());
		proxys.put(Types.MAP, new MapProxy());
		proxys.put(Types.NULL, new NullProxy());
		proxys.put(Types.NUMBER, new NumberProxy());
		proxys.put(Types.OBJECT, new ObjectProxy());
		proxys.put(Types.STRING, new StringProxy());
		proxys.put(Types.COLLECTION, new CollectionProxy());
	}

	public Transfer(byte[] description) throws IOException {
		this();
		// 类型注册
		setDescribe(description);
	}

	public Transfer(Collection<Class<?>> clzs, int startIndex) {
		this();
		for (Class<?> clz : clzs) {
			register(clz, startIndex);
//			if (startIndex > 0) {
				startIndex++;
//			}
		}
	}

	public void register(Class<?> clz, int index) {
		if (log.isDebugEnabled()) {
			log.debug("注册传输对象类型 [{}]", clz);
		}
		if (clz.isEnum()) {
			if (enumDefs.get(clz) == null) {
//				int code = (index > 0 ? index : enumCurrent.incrementAndGet());
				int code=index;
				EnumDef def = EnumDef.valueOf(code, clz);
				enumIdxs.put(code, def);
				enumDefs.put(clz, def);
			}
		} else {
			if (typeDefs.get(clz) == null) {
//				int code = (index > 0 ? index : typeCurrent.incrementAndGet());
				int code=index;
				TypeDef def = TypeDef.valueOf(code, clz);
				typeIdxs.put(code, def);
				typeDefs.put(clz, def);
			}
		}
		description = null;
	}

	private byte[] describe(Collection<Class<?>> clzs) {
		// 类型描述
		IoBuffer buf = IoBuffer.allocate(DEFAULT_SIZE * 10);
		buf.setAutoExpand(true);
		for (Class<?> clz : clzs) {
			if (clz.isEnum()) {
				EnumDef typeDef = enumDefs.get(clz);
				if (typeDef != null) {
					typeDef.describe(buf);
				}
			} else {
				TypeDef typeDef = typeDefs.get(clz);
				if (typeDef != null) {
					typeDef.describe(buf);
				}
			}
		}
		int pos = buf.position();
		byte[] tmp = new byte[pos];
		buf.rewind();
		buf.get(tmp);
		buf.clear();
		buf = null;
		return tmp;
	}

	public void setDescribe(byte[] bytes) throws IOException {
		// 重置枚举定义
		enumIdxs.clear();
		enumDefs.clear();
		enumCurrent.set(0);
		// 重置类型定义
		typeIdxs.clear();
		typeDefs.clear();
		typeCurrent.set(0);

		// 解析格式定义
		byte[] unziplz = QuickLZUtils.unzip(bytes, 30, TimeUnit.SECONDS);
		byte[] unzip = ZlibUtils.unzip(unziplz, 30, TimeUnit.SECONDS);
		unziplz = null;
		ByteBuffer buf = ByteBuffer.wrap(unzip);
		// 类型描述
		while (buf.hasRemaining()) {
			byte flag = buf.get();
			if (flag == 0x00) {
				// 枚举
				EnumDef def = EnumDef.valueOf(buf);
				int code = def.getCode();
				Class<?> clz = def.getType();
				enumIdxs.put(code, def);
				if (clz != null) {
					enumDefs.put(clz, def);
				}
			} else if (flag == 0x01) {
				// 对象
				TypeDef def = TypeDef.valueOf(buf);
				int code = def.getCode();
				Class<?> clz = def.getType();
				typeIdxs.put(code, def);
				if (clz != null) {
					typeDefs.put(clz, def);
				}
			}
		}
		description = bytes;
	}

	private Context build(IoBuffer buffer) {
		Context ctx = new Context(buffer, this);
		return ctx;
	}

	/**
	 * 获取类型定义
	 * @param def
	 * @return
	 */
	TypeDef getTypeDef(int def) {
		return typeIdxs.get(def);
	}

	/**
	 * 获取类型定义
	 * @param type
	 * @return
	 */
	TypeDef getTypeDef(final Class<?> type) {
		TypeDef typeDef = typeDefs.get(type);
		Class<?> superType = type;
		while (typeDef == null) {
			typeDef = aliasDefs.get(superType);
			if (typeDef != null) {
				// 存在类型映射
				break;
			}
			superType = superType.getSuperclass();
			if (superType == Object.class) {
				typeDef = TypeDef.NULL;
				aliasDefs.put(type, typeDef);
				break;
			}
			typeDef = typeDefs.get(superType);
			if (typeDef != null) {
				aliasDefs.put(type, typeDef);
				break;
			}
		}
		return typeDef;
	}

	/**
	 * 获取枚举定义
	 * @param def
	 * @return
	 */
	EnumDef getEnumDef(int def) {
		return enumIdxs.get(def);
	}

	/**
	 * 获取枚举定义
	 * @param def
	 * @return
	 */
	EnumDef getEnumDef(Class<?> def) {
		return enumDefs.get(def);
	}

	// TypeDef getMappedDef(int def) {
	// return mappedIdxs.get(def);
	// }

	/**
	 * 获取MAPPED类型定义
	 * @param type
	 * @param createNew
	 * @return
	 */
	TypeDef getMappedDef(Class<?> type, boolean createNew) {
		TypeDef typeDef = mappedDefs.get(type);
		if (createNew && typeDef == null) {
			typeDef = TypeDef.valueOf(-1, type);
			mappedDefs.put(type, typeDef);
		}
		return typeDef;
	}

	/**
	 * 获取类型代理
	 * @param type
	 * @return
	 */
	Proxy<?> getProxy(byte type) {
		return proxys.get(type);
	}

	/**
	 * 获取消息定义
	 */
	public byte[] getDescription() {
		if (description == null) {
			List<Class<?>> all = new ArrayList<Class<?>>(typeDefs.size() + enumDefs.size());

			// 类型
			List<TypeDef> defs = new ArrayList<TypeDef>(typeDefs.values());
			Collections.sort(defs);
			for (TypeDef t : defs) {
				all.add(t.getType());
			}

			// 枚举
			List<EnumDef> enmus = new ArrayList<EnumDef>(enumDefs.values());
			Collections.sort(enmus);
			for (EnumDef t : enmus) {
				all.add(t.getType());
			}

			byte[] bytes = this.describe(all);
			// Zlib压缩
			description = ZlibUtils.zip(bytes);
			// QuickLZ压缩
			description = QuickLZUtils.zip(description);
			try {
				md5Description = CryptUtils.byte2hex(CryptUtils.md5(description));
				if (log.isInfoEnabled()) {
					log.info("协议定义MD5[{}], [{}]字节", md5Description, description.length);
				}
			} catch (Exception ex) {
				log.error("MD5", ex);
				md5Description = "";
			}
		}
		return description;
	}

	/**
	 * 获取消息定义MD5串
	 */
	public String getMD5Description() {
		if (description == null) {
			// 生成
			getDescription();
		}
		return md5Description;
	}

	/**
	 * 对象解码
	 * @param buf
	 * @return
	 * @throws IOException
	 */
	public Object decode(IoBuffer buf) throws IOException {
		Context ctx = build(buf);
		if (!buf.hasRemaining()) {
			// return null;
			throw new EOFException("Empty ByteBuffer...");
		}
		byte flag = buf.get();
		return ctx.getValue(flag);
	}

	/**
	 * 对象解码
	 * @param buf
	 * @param type
	 * @return
	 * @throws IOException
	 */
	public <T> T decode(IoBuffer buf, Type type) throws IOException {
		Context ctx = build(buf);
		if (!buf.hasRemaining()) {
			// return null;
			throw new EOFException("Empty ByteBuffer...");
		}
		byte flag = buf.get();
		return ctx.getValue(flag, type);
	}

	/**
	 * 对象类型转换
	 * @param value
	 * @param type
	 * @return
	 * @throws IOException
	 */
	@SuppressWarnings({ "unchecked", "rawtypes" })
	public <T> T convert(Object value, Type type) throws IOException {
		if (value == null) {
			return (T) value;
		}
		if (type == null) {
			return (T) value;
		}

		if (TypeUtils.isInstance(value, type)) {
			return (T) value;
		}

		// 枚举
		if (TypeUtils.isAssignable(type, Enum.class)) {
			Class enumType = (Class<T>) type;
			if (value instanceof Number) {
				T[] enums = (T[]) enumType.getEnumConstants();
				int ordinal = ((Number) value).intValue();
				return enums[ordinal];
			} else if (value instanceof String) {
				return (T) Enum.valueOf(enumType, (String) value);
			}
		}

		// 泛型数组
		if (TypeUtils.isArrayType(type)) {
			int len = Array.getLength(value);
			Type componentType = TypeUtils.getArrayComponentType(type);
			if (componentType instanceof Class<?>) {
				Object arr = Array.newInstance((Class<?>) componentType, len);
				for (int i = 0; i < len; i++) {
					Object o = Array.get(value, i);
					Object v = convert(o, componentType);
					Array.set(arr, i, v);
				}
				return (T) arr;
			}
		}

		// 泛型集合
		if (TypeUtils.isAssignable(type, Collection.class)) {
			ParameterizedType parameterizedType = (ParameterizedType) type;
			Class<?> rawType = (Class<?>) parameterizedType.getRawType();
			Type[] typeArguments = parameterizedType.getActualTypeArguments();
			Type componentType = typeArguments[0];
			Collection result;
			int len = 0;
			if (value.getClass().isArray()) {
				len = Array.getLength(value);
			} else if (value instanceof Collection) {
				len = ((Collection) value).size();
			} else {
				FormattingTuple message = MessageFormatter.format("参数类型[{}]无法匹配数值类型[{}]", type, value.getClass());
				log.error(message.getMessage());
				throw new IllegalArgumentException(message.getMessage());
			}

			if (TypeUtils.isAssignable(rawType, Set.class)) {
				result = new HashSet(len);
			} else {
				result = new ArrayList(len);
			}
			for (int i = 0; i < len; i++) {
				Object v = Array.get(value, i);
				Object e = convert(v, componentType);
				result.add(e);
			}
			return (T) result;
		}

		// TODO 参数对象

		// 映射对象
		if (TypeUtils.isInstance(value, Map.class) && !TypeUtils.isAssignable(type, Map.class)) {
			TypeDef mappedDef = this.getMappedDef((Class<Map>) type, true);
			if (mappedDef != null) {
				Map<Object, Object> map = (Map<Object, Object>) value;
				Object obj;
				try {
					obj = mappedDef.newInstance();
				} catch (Exception e) {
					throw new ObjectConvertException(e);
				}
				List<FieldDef> fields = mappedDef.getFields();
				int len = fields.size();
				for (int i = 0; i < len; i++) {
					FieldDef fieldDef = fields.get(i);
					Class<?> clz = fieldDef.getType();
					String name = fieldDef.getName();
					Object mval = map.get(name);
					if (mval == null) {
						continue;
					}
					Object val = convert(mval, clz);
					if (val == null) {
						continue;
					}
					// 字段赋值
					try {
						mappedDef.setValue(obj, i, val);
					} catch (Exception e) {
						throw new ObjectConvertException(e);
					}
				}
				return (T) obj;
			}
		}

		if (!(type instanceof Class<?>)) {
			FormattingTuple message = MessageFormatter.format("不支持的类型参数[{}]", type);
			log.error(message.getMessage());
			throw new IllegalArgumentException(message.getMessage());
		}
		return (T) ConvertUtils.convert(value, (Class<?>) type);
	}

	/**
	 * 对象编码
	 * @param value
	 * @return
	 * @throws IOException
	 */
	public IoBuffer encode(Object value) throws IOException {
		IoBuffer buf = IoBuffer.allocate(DEFAULT_SIZE);
		buf.setAutoExpand(true);
		Context ctx = build(buf);
		ctx.setValue(value);
		buf.flip();
		return buf;
	}

}
