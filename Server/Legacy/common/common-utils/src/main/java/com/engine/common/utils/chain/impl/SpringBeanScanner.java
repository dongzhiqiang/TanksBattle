package com.engine.common.utils.chain.impl;

import java.lang.reflect.Method;
import java.util.HashMap;
import java.util.Map;
import java.util.Map.Entry;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.aop.support.AopUtils;
import org.springframework.beans.BeansException;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.beans.factory.config.BeanPostProcessor;
import org.springframework.context.ApplicationListener;
import org.springframework.context.event.ContextRefreshedEvent;
import org.springframework.util.ReflectionUtils;
import org.springframework.util.ReflectionUtils.MethodCallback;

import com.engine.common.utils.chain.ChainBuilder;
import com.engine.common.utils.chain.Dispatcher;
import com.engine.common.utils.chain.NodeProcessor;
import com.engine.common.utils.chain.ProcessChain;
import com.engine.common.utils.chain.anno.Processing;

/**
 * {@link Processing} 注解处理器<br/>
 * 负责将{@link Processing}声明的方法，构建为合法的{@link NodeProcessor}对象并向{@link Dispatcher}注册
 * 
 * 
 */
public class SpringBeanScanner implements BeanPostProcessor, ApplicationListener<ContextRefreshedEvent> {

	private static final Logger log = LoggerFactory.getLogger(SpringBeanScanner.class);

	/** 处理链创建者 */
	private final Map<String, ChainBuilder> builders = new HashMap<String, ChainBuilder>();
	@Autowired
	private Dispatcher processor;

	/**
	 * Bean 扫描
	 */
	@Override
	public Object postProcessAfterInitialization(final Object bean, String beanName) throws BeansException {
		ReflectionUtils.doWithMethods(AopUtils.getTargetClass(bean), new MethodCallback() {
			public void doWith(Method method) throws IllegalArgumentException, IllegalAccessException {
				if (!AnnotationMethodNodeBuilder.isValid(method)) {
					return;
				}
				addNodeProcessor(bean, method);
			}
		});
		return bean;
	}

	/**
	 * 添加处理方法
	 * 
	 * @param bean
	 * @param method
	 */
	private void addNodeProcessor(Object bean, Method method) {
		NodeProcessor processor = AnnotationMethodNodeBuilder.build(bean, method);
		String name = processor.getName();
		if (!builders.containsKey(name)) {
			builders.put(name, new ChainBuilder());
		}
		ChainBuilder builder = builders.get(name);
		builder.addNode(processor);
	}

	/** 构建{@link ProcessChain}并向{@link Dispatcher}注册 */
	@Override
	public void onApplicationEvent(ContextRefreshedEvent event) {
		for (Entry<String, ChainBuilder> entry : builders.entrySet()) {
			String name = entry.getKey();
			ChainBuilder builder = entry.getValue();
			ProcessChain chain = builder.build();
			chain = processor.register(name, chain);
			if (chain != null) {
				log.error("处理链[{}]重复", name);
			}
		}
	}

	// 非业务逻辑方法

	@Override
	public Object postProcessBeforeInitialization(final Object bean, String beanName) throws BeansException {
		return bean;
	}

}
