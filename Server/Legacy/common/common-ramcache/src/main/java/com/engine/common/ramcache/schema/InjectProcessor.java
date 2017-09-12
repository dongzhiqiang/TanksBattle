package com.engine.common.ramcache.schema;

import java.lang.reflect.Field;
import java.lang.reflect.ParameterizedType;
import java.lang.reflect.Type;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.slf4j.helpers.FormattingTuple;
import org.slf4j.helpers.MessageFormatter;
import org.springframework.beans.BeansException;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.beans.factory.config.InstantiationAwareBeanPostProcessorAdapter;
import org.springframework.util.ReflectionUtils;
import org.springframework.util.ReflectionUtils.FieldCallback;

import com.engine.common.ramcache.IEntity;
import com.engine.common.ramcache.ServiceManager;
import com.engine.common.ramcache.anno.Inject;
import com.engine.common.ramcache.exception.ConfigurationException;
import com.engine.common.ramcache.service.EntityCacheService;
import com.engine.common.ramcache.service.RegionCacheService;

/**
 * 缓存服务注入处理器，负责完成 {@link Inject} 声明的资源的注入工作
 * 
 */
@SuppressWarnings({ "rawtypes", "unchecked" })
public class InjectProcessor extends InstantiationAwareBeanPostProcessorAdapter {

	private static final Logger logger = LoggerFactory.getLogger(InjectProcessor.class);
	
	@Autowired
	private ServiceManager manager;

	@Override
	public boolean postProcessAfterInstantiation(final Object bean, final String beanName) throws BeansException {
		ReflectionUtils.doWithFields(bean.getClass(), new FieldCallback() {
			public void doWith(Field field) throws IllegalArgumentException, IllegalAccessException {
				Inject anno = field.getAnnotation(Inject.class);
				if (anno == null) {
					return;
				}
				if (field.getType().equals(EntityCacheService.class)) {
					// 注入实体单位缓存服务
					injectEntityCacheService(bean, beanName, field);
				} else if (field.getType().equals(RegionCacheService.class)) {
					// 注入区域单位缓存服务
					injectRegionCacheService(bean, beanName, field);
				} else {
					FormattingTuple message = MessageFormatter.format("Bean[]的注入属性[]类型声明错误", beanName, field.getName());
					logger.error(message.getMessage());
					throw new ConfigurationException(message.getMessage());
				}
			}
		});
		return super.postProcessAfterInstantiation(bean, beanName);
	}

	/** 注入实体单位缓存服务 */
	private void injectEntityCacheService(Object bean, String beanName, Field field) {
		Class<? extends IEntity> clz = null;
		EntityCacheService service = null;
		try {
			Type type = field.getGenericType();
			Type[] types = ((ParameterizedType) type).getActualTypeArguments();
			clz = (Class<? extends IEntity>) types[1];
			service = manager.getEntityService(clz);
		} catch (Exception e) {
			FormattingTuple message = MessageFormatter.format("Bean[{}]的注入属性[{}]类型声明错误", beanName, field.getName());
			logger.error(message.getMessage());
			throw new ConfigurationException(message.getMessage());
		}
		if (service == null) {
			FormattingTuple message = MessageFormatter.format("实体[{}]缓存服务对象不存在", clz.getName());
			logger.debug(message.getMessage());
			throw new ConfigurationException(message.getMessage());
		}
		inject(bean, field, service);
	}

	/** 注入区域单位缓存服务 */
	private void injectRegionCacheService(Object bean, String beanName, Field field) {
		Class<? extends IEntity> clz = null;
		RegionCacheService service = null;
		try {
			Type type = field.getGenericType();
			Type[] types = ((ParameterizedType) type).getActualTypeArguments();
			clz = (Class<? extends IEntity>) types[1];
			service = manager.getRegionService(clz);
		} catch (Exception e) {
			FormattingTuple message = MessageFormatter.format("Bean[{}]的注入属性[{}]类型声明错误", beanName, field.getName());
			logger.error(message.getMessage());
			throw new ConfigurationException(message.getMessage());
		}
		if (service == null) {
			FormattingTuple message = MessageFormatter.format("实体[{}]缓存服务对象不存在", clz.getName());
			logger.debug(message.getMessage());
			throw new ConfigurationException(message.getMessage());
		}
		inject(bean, field, service);
	}

	/**
	 * 注入属性值
	 * @param bean
	 * @param field
	 * @param value
	 */
	private void inject(Object bean, Field field, Object value) {
		ReflectionUtils.makeAccessible(field);
		try {
			field.set(bean, value);
		} catch (Exception e) {
			FormattingTuple message = MessageFormatter.format("属性[{}]注入失败", field);
			logger.debug(message.getMessage());
			throw new ConfigurationException(message.getMessage());
		}
	}
}
