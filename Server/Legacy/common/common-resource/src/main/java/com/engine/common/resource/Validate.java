package com.engine.common.resource;

/**
 * 静态资源的内容验证接口
 * 
 */
public interface Validate {

	/**
	 * 检查静态资源的内容是否有效
	 * @return true:有效,false:无效
	 */
	boolean isValid();
}
