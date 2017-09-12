package com.engine.common.scheduler.impl;

import java.lang.reflect.Method;
import java.util.HashMap;
import java.util.Map;
import java.util.Map.Entry;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.aop.support.AopUtils;
import org.springframework.beans.BeansException;
import org.springframework.beans.factory.BeanFactory;
import org.springframework.beans.factory.BeanFactoryAware;
import org.springframework.beans.factory.NoSuchBeanDefinitionException;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.beans.factory.config.BeanPostProcessor;
import org.springframework.context.ApplicationListener;
import org.springframework.context.event.ContextRefreshedEvent;
import org.springframework.context.expression.BeanFactoryResolver;
import org.springframework.core.Ordered;
import org.springframework.core.annotation.AnnotationUtils;
import org.springframework.expression.ExpressionParser;
import org.springframework.expression.spel.standard.SpelExpressionParser;
import org.springframework.expression.spel.support.StandardEvaluationContext;
import org.springframework.scheduling.support.MethodInvokingRunnable;
import org.springframework.stereotype.Component;
import org.springframework.util.ReflectionUtils;
import org.springframework.util.ReflectionUtils.MethodCallback;

import com.engine.common.scheduler.Scheduled;
import com.engine.common.scheduler.ScheduledTask;
import com.engine.common.scheduler.Scheduler;

/**
 * 定时任务处理器<br/>
 * 由他负责检查定时任务声明，以及开启任务线程池
 * 
 */
@Component
public class ScheduledProcessor implements BeanPostProcessor, ApplicationListener<ContextRefreshedEvent>, Ordered, BeanFactoryAware {

	private static final Logger logger = LoggerFactory.getLogger(ScheduledProcessor.class);
	
	// 定时任务注册部分
	private final Map<ScheduledTask, String> tasks = new HashMap<ScheduledTask, String>();

	/** 获取定时任务信息 */
	public Object postProcessAfterInitialization(final Object bean, String beanName) {
		ReflectionUtils.doWithMethods(AopUtils.getTargetClass(bean), new MethodCallback() {
			public void doWith(Method method) throws IllegalArgumentException,
					IllegalAccessException {
				Scheduled annotation = AnnotationUtils.getAnnotation(method, Scheduled.class);
				if (annotation == null) {
					return;
				}

				ScheduledTask task = createTask(bean, method, annotation);
				String experssion = resolveExperssion(bean, annotation);
				tasks.put(task, experssion);
			}
		});
		return bean;
	}

	/** 创建定时任务 */
	private ScheduledTask createTask(Object bean, Method method, Scheduled annotation) {
		if (!void.class.equals(method.getReturnType())) {
			throw new IllegalArgumentException("定时方法的返回值必须为 void");
		}
		if (method.getParameterTypes().length != 0) {
			throw new IllegalArgumentException("定时方法不能有参数");
		}

		final MethodInvokingRunnable runnable = new MethodInvokingRunnable();
		runnable.setTargetObject(bean);
		runnable.setTargetMethod(method.getName());
		runnable.setArguments(new Object[0]);
		try {
			runnable.prepare();
		} catch (Exception e) {
			throw new IllegalStateException("无法创建定时任务", e);
		}

		final String name = annotation.name();
		return new ScheduledTask() {
			@Override
			public void run() {
				runnable.run();
			}

			@Override
			public String getName() {
				return name;
			}
		};
	}

	/** 获取Cron表达式 */
	private String resolveExperssion(Object bean, Scheduled annotation) {
		String result = null;
		switch (annotation.type()) {
		case EXPRESSION:
			result = annotation.value();
			break;
		case BEANNAME:
			String name = annotation.value();
			try {
				Object obj = beanFactory.getBean(name);
				if (obj instanceof String) {
					result = (String) obj;
				} else {
					result = annotation.defaultValue();
				}
			} catch (NoSuchBeanDefinitionException e) {
				logger.error("无法获取定时任务配置[{}],将使用默认值替代", annotation);
				result = annotation.defaultValue();
			}
			break;
		case SPEL:
			try {
				ExpressionParser parser = new SpelExpressionParser();
				StandardEvaluationContext context = new StandardEvaluationContext();
				context.setBeanResolver(new BeanFactoryResolver(beanFactory));
				result = parser.parseExpression(annotation.value()).getValue(context, String.class);
			} catch (Exception e) {
				logger.error("无法获取定时任务配置[{}],将使用默认值替代", annotation);
				result = annotation.defaultValue();
			}
			break;
		default:
			break;
		}
		return result;
	}

	/** 定时任务调度器 */
	@Autowired
	private Scheduler scheduler;

	@Override
	public void onApplicationEvent(ContextRefreshedEvent event) {
		for (Entry<ScheduledTask, String> entry : tasks.entrySet()) {
			scheduler.schedule(entry.getKey(), new CronTrigger(entry.getValue()));
		}
	}

	public int getOrder() {
		return LOWEST_PRECEDENCE;
	}

	public Object postProcessBeforeInitialization(Object bean, String beanName) {
		return bean;
	}

	private BeanFactory beanFactory;
	
	@Override
	public void setBeanFactory(BeanFactory beanFactory) throws BeansException {
		this.beanFactory = beanFactory;
	}

}
