package com.engine.common.socket.core;

import java.util.Collection;
import java.util.Map;
import java.util.Set;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.ConcurrentMap;

import org.apache.mina.core.session.IoSession;

/**
 * 通信会话抽象接口，用于屏蔽底层的真实会话对象
 * 
 */
public class Session implements ConcurrentMap<String, Object> {
	
	public static final String MAIN_KEY = "engine";

	@SuppressWarnings("unchecked")
	public static Session valueOf(IoSession session) {
		long id = session.getId();
		ConcurrentMap<String, Object> content = (ConcurrentMap<String, Object>) session.getAttribute(MAIN_KEY);
		if (content == null) {
			content = new ConcurrentHashMap<String, Object>();
			ConcurrentHashMap<String, Object> prev = (ConcurrentHashMap<String, Object>) session.setAttributeIfAbsent(MAIN_KEY, content);
			if (prev != null) {
				content = prev;
			}
		}
		return new Session(id, content);
	}
	
	private final long id;
	private final ConcurrentMap<String, Object> content;
	
	public Session(long id, ConcurrentMap<String, Object> content) {
		this.id = id;
		this.content = content;
	}
	
	public long getId() {
		return id;
	}

	// 代理 content 的方法
	
	public void clear() {
		content.clear();
	}

	public boolean containsKey(Object key) {
		return content.containsKey(key);
	}

	public boolean containsValue(Object value) {
		return content.containsValue(value);
	}

	public Set<java.util.Map.Entry<String, Object>> entrySet() {
		return content.entrySet();
	}

	public boolean equals(Object o) {
		return content.equals(o);
	}

	public Object get(Object key) {
		return content.get(key);
	}

	public int hashCode() {
		return content.hashCode();
	}

	public boolean isEmpty() {
		return content.isEmpty();
	}

	public Set<String> keySet() {
		return content.keySet();
	}

	public Object put(String key, Object value) {
		return content.put(key, value);
	}

	public void putAll(Map<? extends String, ? extends Object> m) {
		content.putAll(m);
	}

	public Object putIfAbsent(String key, Object value) {
		return content.putIfAbsent(key, value);
	}

	public boolean remove(Object key, Object value) {
		return content.remove(key, value);
	}

	public Object remove(Object key) {
		return content.remove(key);
	}

	public boolean replace(String key, Object oldValue, Object newValue) {
		return content.replace(key, oldValue, newValue);
	}

	public Object replace(String key, Object value) {
		return content.replace(key, value);
	}

	public int size() {
		return content.size();
	}

	public Collection<Object> values() {
		return content.values();
	}
}
