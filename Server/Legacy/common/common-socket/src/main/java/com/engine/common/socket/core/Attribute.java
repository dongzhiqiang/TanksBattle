package com.engine.common.socket.core;

import java.io.Serializable;
import java.util.concurrent.ConcurrentHashMap;

import org.apache.mina.core.session.IoSession;

/**
 * 属性类，用于统一与简化对{@link IoSession}的属性操作<br/>
 * 所有属性都将放在以{@link Session#MAIN_KEY}作为KEY，对应的{@link ConcurrentHashMap}中
 * 
 */
@SuppressWarnings("unchecked")
public class Attribute<T> implements Serializable {

	private static final long serialVersionUID = -2820545255304851339L;
	
	/** 属性键 */
    private final String key;

    /**
     * 构建属性对象
     * @param key 属性键
     */
    public Attribute(String key) {
        this.key = key;
    }
    
	/**
	 * 获取会话中的全部属性
	 * @param session
	 * @return
	 */
	public ConcurrentHashMap<String, Object> getAttributes(IoSession session) {
		ConcurrentHashMap<String, Object> attributes = (ConcurrentHashMap<String, Object>) session.getAttribute(Session.MAIN_KEY);
		if (attributes != null) {
			return attributes;
		}
		
		attributes = new ConcurrentHashMap<String, Object>();
		ConcurrentHashMap<String, Object> prev = (ConcurrentHashMap<String, Object>) session.setAttributeIfAbsent(Session.MAIN_KEY, attributes);
		return prev == null ? attributes : prev;
	}
    
    /**
     * 获取属性值
     * @param session
     * @return
     */
	public T getValue(IoSession session) {
    	ConcurrentHashMap<String, Object> attributes = getAttributes(session);
    	return (T) attributes.get(key);
    }

	/**
	 * 获取属性值，不存在则以默认值返回
	 * @param session
	 * @param defaultValue 默认值
	 * @return
	 */
    public T getValue(IoSession session, T defaultValue) {
    	if (contain(session)) {
    		return getValue(session);
    	}
    	return defaultValue;
    }
    
    /**
     * 设置属性值
     * @param session
     * @param value
     * @return
     */
	public T setValue(IoSession session, T value) {
		ConcurrentHashMap<String, Object> attributes = getAttributes(session);
		return (T) attributes.putIfAbsent(key, value);
	}

    /**
     * 检查是否存在对应的属性值
     * @param session
     * @return
     */
    public boolean contain(IoSession session) {
    	ConcurrentHashMap<String, Object> attributes = getAttributes(session);
    	return attributes.containsKey(key);
    }

    /**
     * 获取属性键
     * @return
     */
    public String getKey() {
    	return key;
    }
    
    @Override
    public String toString() {
    	return key;
    }
}
