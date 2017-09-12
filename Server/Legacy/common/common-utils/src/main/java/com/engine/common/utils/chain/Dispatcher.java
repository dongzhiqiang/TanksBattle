package com.engine.common.utils.chain;

import java.util.HashMap;
import java.util.Map;

/**
 * 处理通知分发器
 * 
 */
public class Dispatcher {
	
	private Map<String, ProcessChain> chains = new HashMap<String, ProcessChain>();

	/**
	 * 发出并处理一个处理通知(该方法为同步方法，即调用完成时，处理也完成了)<br/>
	 * 发出处理通知时，会根据{@link Notice#getName()}获取已经注册的{@link ProcessChain}，并交给它进行处理
	 * @param notice 处理通知对象
	 */
	public void process(Notice<?> notice) {
		String name = notice.getName();
		ProcessChain chain = chains.get(name);
		if (chain != null) {
			if (!chain.process(notice)) {
				notice.setInterrupt(true);
			}
		}
	}

	/**
	 * 注册处理链
	 * @param name 处理事件名
	 * @param chain 事件处理链
	 * @return 如果指定的事件名已有存在的处理链，将在注册时返回旧的处理链
	 */
	public ProcessChain register(String name, ProcessChain chain) {
		return chains.put(name, chain);
	}
}
