package com.engine.common.utils.chain;

import java.util.List;

/**
 * 处理链
 * 
 */
@SuppressWarnings("rawtypes")
public class ProcessChain {
	
	private NodeProcessor current;
	private ProcessChain next;
	
	/**
	 * 根据一个有序的{@link NodeProcessor}列表构建对应的处理链对象
	 * @param nodes 有序的{@link NodeProcessor}列表
	 * @throws IllegalArgumentException 处理链的节点数量为0时抛出
	 */
	public ProcessChain(List<NodeProcessor> nodes) {
		if (nodes == null || nodes.size() == 0) {
			throw new IllegalArgumentException("处理链的节点数量不能为0");
		}
		this.current = nodes.remove(0);
		if (nodes.size() > 0) {
			this.next = new ProcessChain(nodes);
		} else {
			this.next = null;
		}
	}
	
	/**
	 * 处理事件通知<br/>
	 * 被打断的处理，{@link Notice#getStep()}和{@link Notice#getWay()}会停留在被打断的状态
	 * @param notice 通知对象
	 * @return 处理过程是否完全处理通过，true表示没被打断，false表示被打断过
	 */
	public boolean process(Notice notice) {
		int step = notice.getStep();
		step++;
		notice.setStep(step);
		notice.setWay(Way.IN);
		if (!current.in(notice)) {
			return false;
		}
		if (next != null) {
			if (!next.process(notice)) {
				return false;
			}
		}
		if (!current.out(notice)) {
			return false;
		}
		step--;
		notice.setStep(step);
		notice.setWay(Way.OUT);
		return true;
	}

}
