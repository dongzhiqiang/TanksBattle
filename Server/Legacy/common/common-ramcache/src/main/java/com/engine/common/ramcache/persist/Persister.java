package com.engine.common.ramcache.persist;

import java.util.Map;

import com.engine.common.ramcache.IEntity;
import com.engine.common.ramcache.orm.Accessor;

/**
 * 持久化处理器接口
 * 
 */
@SuppressWarnings("rawtypes")
public interface Persister {

	/** 初始化方法 */
	void initialize(String name, Accessor accessor, String config);

	/**
	 * 将指定元素插入此队列中。
	 * @param element 被添加元素(元素为null时直接返回)
	 */
	void put(Element element);

	/** 添加监听器 */
	void addListener(Class<? extends IEntity> clz, Listener listener);
	
	/** 获取监听器 */
	Listener getListener(Class<? extends IEntity> clz);

	/** 停止更新队列并等待全部入库完成 */
	void shutdown();

	/**
	 * 获取当前的状态信息
	 * @return
	 */
	Map<String, String> getInfo();

}
