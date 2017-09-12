package com.engine.common.resource.schema;

import java.lang.reflect.Field;
import java.util.Observable;
import java.util.Observer;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.slf4j.helpers.FormattingTuple;
import org.slf4j.helpers.MessageFormatter;

import com.engine.common.resource.Storage;
import com.engine.common.resource.anno.Static;

/**
 * 静态资源变更观察者
 * 
 * 
 */
@SuppressWarnings("rawtypes")
public class StaticObserver implements Observer {

	private final static Logger logger = LoggerFactory.getLogger(StaticObserver.class);

	/** 接收更新通知 */
	@Override
	public void update(Observable o, Object arg) {
		if (!(o instanceof Storage)) {
			if (logger.isDebugEnabled()) {
				FormattingTuple message = MessageFormatter.format("被观察对象[{}]不是指定类型", o.getClass());
				logger.debug(message.getMessage());
			}
			return;
		}

		inject((Storage) o);
	}

	/** 注入资源实例 */
	private void inject(Storage o) {
		@SuppressWarnings("unchecked")
		Object value = o.get(key, false);
		if (anno.required() && value == null) {
			FormattingTuple message = MessageFormatter.format("被注入属性[{}]不存在[key:{}]", field, key);
			logger.error(message.getMessage());
			throw new RuntimeException(message.getMessage());
		}

		try {
			field.set(target, value);
		} catch (Exception e) {
			FormattingTuple message = MessageFormatter.format("无法设置被注入属性[{}]", field);
			logger.error(message.getMessage());
			throw new RuntimeException(message.getMessage());
		}
	}

	/** 注入目标 */
	private final Object target;
	/** 注入属性 */
	private final Field field;
	/** 注入属性 */
	private final Static anno;
	/** 资源键值 */
	private final Object key;
	
	public StaticObserver(Object target, Field field, Static anno, Object key) {
		this.target = target;
		this.field = field;
		this.anno = anno;
		this.key = key;
	}
	
	// Getter and Setter ...

	public Object getTarget() {
		return target;
	}

	public Field getField() {
		return field;
	}

	public Static getAnno() {
		return anno;
	}

	public Object getKey() {
		return key;
	}

}
