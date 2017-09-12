package com.engine.common.utils.chain;

/**
 * 节点的处理接口
 * 
 */
@SuppressWarnings("rawtypes") 
public interface NodeProcessor extends Comparable<NodeProcessor> {
	
	/**
	 * 获取处理任务名
	 * @return
	 */
	String getName();
	
	/**
	 * 获取位置序号
	 * @return
	 */
	int getIndex();
	
	/**
	 * 处理节点所处理的事件方向
	 * @return
	 */
	Way getWay();

	/**
	 * 处理链进入通知
	 * @param notice 通知对象
	 * @return 返回 false 将终止处理链处理
	 */
	boolean in(Notice notice);
	
	/**
	 * 处理链返回通知
	 * @param notice 通知对象
	 * @return 返回 false 将终止处理链处理
	 */
	boolean out(Notice notice);
}
