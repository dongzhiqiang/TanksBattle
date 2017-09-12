package com.engine.common.ramcache.persist;

import java.io.Serializable;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.engine.common.ramcache.IEntity;
import com.engine.common.ramcache.enhance.EnhancedEntity;

/**
 * 队列元素
 * 
 */
@SuppressWarnings("rawtypes")
public class Element {
	
	private static final Logger logger = LoggerFactory.getLogger(Element.class);

	/** 构造插入元素 */
	public static Element saveOf(IEntity entity) {
		if (entity instanceof EnhancedEntity) {
			entity = ((EnhancedEntity) entity).getEntity();
		}
		return new Element(EventType.SAVE, (Serializable) entity.getId(), entity, entity.getClass());
	}

	/** 构造删除元素 */
	public static Element removeOf(Serializable id, Class<? extends IEntity> entityClass) {
		return new Element(EventType.REMOVE, id, null, entityClass);
	}

	/** 构造更新元素 */
	public static Element updateOf(IEntity entity) {
		if (entity instanceof EnhancedEntity) {
			entity = ((EnhancedEntity) entity).getEntity();
		}
		return new Element(EventType.UPDATE, entity.getId(), entity, entity.getClass());
	}

	/** 事件类型(不会为null) */
	private EventType type;
	/** 实体主键(不会为null) */
	private final Serializable id;
	/** 实体实例(删除时会为null) */
	private IEntity entity;
	/** 实体类型(不会为null) */
	private final Class<? extends IEntity> entityClass;

	/** 构造方法 */
	private Element(EventType type, Serializable id, IEntity entity, Class<? extends IEntity> entityClass) {
		this.type = type;
		this.id = id;
		this.entity = entity;
		this.entityClass = entityClass;
	}
	
	/** 获取标识 */
	public String getIdentity() {
		return entityClass.getName() + ":" + id;
	}

	/**
	 * 更新队列元素的状态<br/>
	 * 该方法不假设更新元素是同一个元素，因此元素判断要在使用该方法前处理
	 * @param element 最新的元素状态
	 * @return true:需要保留,false:更新元素已经不需要保留
	 */
	public boolean update(Element element) {
		entity = element.getEntity();
		switch (type) {
		// 之前的状态为SAVE
		case SAVE:
			switch (element.getType()) {
			// 当前的状态
			case SAVE:
				logger.error("更新元素异常，实体[{}]原状态[{}]当前状态[{}]不进行修正",
						new Object[] { getIdentity(), type, element.getType()});
				break;
			case UPDATE:
				if (logger.isDebugEnabled()) {
					logger.debug("实体[{}]原状态[{}]当前状态[{}]修正后状态[{}]是否保留队列元素[{}]",
							new Object[] { getIdentity(), EventType.SAVE, element.getType(), type, true });
				}
				break;
			case REMOVE:
				if (logger.isDebugEnabled()) {
					logger.debug("实体[{}]原状态[{}]当前状态[{}]修正后状态[{}]是否保留队列元素[{}]",
							new Object[] { getIdentity(), EventType.SAVE, element.getType(), type, false });
				}
				return false;
			}
			break;
		// 之前的状态为UPDATE
		case UPDATE:
			switch (element.getType()) {
			case SAVE:
				logger.error("更新元素异常，实体[{}]原状态[{}]当前状态[{}]不进行修正",
						new Object[] { getIdentity(), type, element.getType()});
				break;
			case UPDATE:
				if (logger.isDebugEnabled()) {
					logger.debug("实体[{}]原状态[{}]当前状态[{}]修正后状态[{}]是否保留队列元素[{}]",
							new Object[] { getIdentity(), EventType.SAVE, element.getType(), type, true });
				}
				break;
			case REMOVE:
				type = EventType.REMOVE;
				if (logger.isDebugEnabled()) {
					logger.debug("实体[{}]原状态[{}]当前状态[{}]修正后状态[{}]是否保留队列元素[{}]",
							new Object[] { getIdentity(), EventType.SAVE, element.getType(), type, true });
				}
				break;
			}
			break;
		// 之前的状态为REMOVE
		case REMOVE:
			switch (element.getType()) {
			case SAVE:
				type = EventType.UPDATE;
				if (logger.isDebugEnabled()) {
					logger.debug("实体[{}]原状态[{}]当前状态[{}]修正后状态[{}]是否保留队列元素[{}]",
							new Object[] { getIdentity(), EventType.REMOVE, EventType.SAVE, type, true });
				}
				break;
			case UPDATE:
				logger.error("更新元素异常，实体[{}]原状态[{}]当前状态[{}]不进行修正",
						new Object[] { getIdentity(), type, element.getType()});
				break;
			case REMOVE:
				logger.error("更新元素异常，实体[{}]原状态[{}]当前状态[{}]不进行修正",
						new Object[] { getIdentity(), type, element.getType()});
				break;
			}
			break;
		}
		return true;
	}

	// Getter and Setter ...

	public EventType getType() {
		return type;
	}

	public Serializable getId() {
		return id;
	}

	public IEntity getEntity() {
		return entity;
	}

	public Class<? extends IEntity> getEntityClass() {
		return entityClass;
	}

	@Override
	public String toString() {
		StringBuilder builder = new StringBuilder();
		builder.append(type).append(" {").append(entityClass.getSimpleName()).append("} ID:").append(id);
		return builder.toString();
	}

}
