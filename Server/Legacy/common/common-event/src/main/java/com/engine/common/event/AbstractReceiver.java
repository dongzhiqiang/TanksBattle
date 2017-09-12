package com.engine.common.event;

import javax.annotation.PostConstruct;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;

/**
 * 抽象事件订阅者，用于简化编码
 * 
 */
public abstract class AbstractReceiver<T> implements Receiver<T> {
	
	protected final Logger log = LoggerFactory.getLogger(this.getClass());
	
	@Autowired
	protected EventBus eventBus;

	@PostConstruct
	void init() {
		for (String name : getEventNames()) {
			eventBus.register(name, this);
		}
	}

	/**
	 * 获取该订阅者负责处理的事件名数组
	 * @return
	 */
	public abstract String[] getEventNames();
	
	@Override
	public void onEvent(Event<T> event) {
		T content = event.getBody();
		doEvent(content);
	}
	
	/**
	 * 事件处理方法
	 * @param event 事件消息体
	 */
	public abstract void doEvent(T event);
}
