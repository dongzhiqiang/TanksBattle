package com.engine.common.ramcache.aop;

import java.lang.annotation.ElementType;
import java.lang.annotation.Retention;
import java.lang.annotation.RetentionPolicy;
import java.lang.annotation.Target;

/**
 * 锁定的参数声明
 * 
 */
@Retention(RetentionPolicy.RUNTIME)
@Target(ElementType.PARAMETER)
public @interface IsLocked {

	/**
	 * 是否锁定对象中的元素<br/>
	 * 支持<code>Collection</code>,<code>Array</code>,<code>Map</code>(只锁定value)
	 * @return
	 */
	boolean element() default false;
}
