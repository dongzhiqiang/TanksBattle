package com.engine.common.utils.chain;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

/**
 * 处理链创建者
 * 
 */
public class ChainBuilder {
	
	private List<NodeProcessor> processors = new ArrayList<NodeProcessor>();
	
	/**
	 * 添加处理节点
	 * @param processor 处理节点
	 */
	public void addNode(NodeProcessor processor) {
		processors.add(processor);
	}
	
	/**
	 * 将加入的处理节点，创建为处理链对象返回
	 * @return
	 */
	public ProcessChain build() {
		Collections.sort(processors);
		return new ProcessChain(processors);
	}

}
