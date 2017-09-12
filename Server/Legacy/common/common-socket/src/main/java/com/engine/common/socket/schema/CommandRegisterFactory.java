package com.engine.common.socket.schema;

import java.io.IOException;
import java.lang.reflect.Method;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.Set;

import javax.annotation.PostConstruct;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.slf4j.helpers.FormattingTuple;
import org.slf4j.helpers.MessageFormatter;
import org.springframework.beans.BeansException;
import org.springframework.beans.FatalBeanException;
import org.springframework.beans.factory.FactoryBean;
import org.springframework.beans.factory.config.BeanPostProcessor;
import org.springframework.core.io.Resource;
import org.springframework.core.io.support.PathMatchingResourcePatternResolver;
import org.springframework.core.io.support.ResourcePatternResolver;
import org.springframework.core.type.AnnotationMetadata;
import org.springframework.core.type.ClassMetadata;
import org.springframework.core.type.classreading.CachingMetadataReaderFactory;
import org.springframework.core.type.classreading.MetadataReader;
import org.springframework.core.type.classreading.MetadataReaderFactory;
import org.springframework.util.ClassUtils;
import org.springframework.util.SystemPropertyUtils;

import com.engine.common.socket.anno.ParameterBuilder;
import com.engine.common.socket.anno.SocketCommand;
import com.engine.common.socket.anno.SocketModule;
import com.engine.common.socket.anno.SocketPush;
import com.engine.common.socket.core.Command;
import com.engine.common.socket.core.Convertor;
import com.engine.common.socket.handler.CommandRegister;
import com.engine.common.socket.handler.MethodDefinition;
import com.engine.common.socket.handler.MethodProcessor;
import com.engine.common.socket.other.AnnotationUtility;

/**
 * 指令注册器工厂
 * 
 */
public class CommandRegisterFactory implements FactoryBean<CommandRegister>, BeanPostProcessor {

	private static final Logger logger = LoggerFactory.getLogger(CommandRegisterFactory.class);

	private ParameterBuilder builder;

	@PostConstruct
	protected void initialize() throws Exception {
		builder = new ParameterBuilder();
		builder.setConvertor(convertor);
		if (isScanInterfaces()) {
			scanInterfaces();
		}
	}

	// 扫描接口相关的方法

	/** 默认资源匹配符 */
	protected static final String DEFAULT_RESOURCE_PATTERN = "**/*.class";

	/** 资源搜索分析器，由它来负责检索{@link SocketModule}声明的接口 */
	private ResourcePatternResolver resourcePatternResolver = new PathMatchingResourcePatternResolver();
	/** 类的元数据读取器，由它来负责读取类上的注释信息 */
	private MetadataReaderFactory metadataReaderFactory = new CachingMetadataReaderFactory(this.resourcePatternResolver);

	private boolean scanInterfaces = false;

	private Set<String> packages;
	private Set<String> interfaces;
	private Set<String> includes;
	private Set<String> excludes;

	private void scanInterfaces() throws IOException, ClassNotFoundException, Exception {
		// 扫描包注册
		List<Resource> resources = findResources();
		for (Resource resource : resources) {
			if (!resource.isReadable()) {
				continue; // 不可读资源，忽略
			}
			// 判断是否有 SocketHandler 注释
			MetadataReader metadataReader = this.metadataReaderFactory.getMetadataReader(resource);
			AnnotationMetadata annoData = metadataReader.getAnnotationMetadata();
			if (!annoData.hasAnnotation(SocketModule.class.getName())) {
				continue; // 不是要处理的资源，忽略
			}
			if (!annoData.hasAnnotation(SocketPush.class.getName())) {
				continue; // 不是要处理的资源，忽略
			}
			
			ClassMetadata clzMeta = metadataReader.getClassMetadata();
			if (clzMeta.isInterface()) { // 只处理接口
				Class<?> clz = Class.forName(clzMeta.getClassName());
				registerFromClass(clz);
			}
		}
		
		// 直接接口注册
		if (interfaces != null) {
			for (String inf : interfaces) {
				Class<?> clz = Class.forName(inf);
				registerFromClass(clz);
			}
		}
	}

	/**
	 * 查找出全部的类资源
	 * @return
	 * @throws IOException
	 */
	private List<Resource> findResources() throws IOException {
		List<Resource> resources = new ArrayList<Resource>();
		if (packages != null) {
			for (String name : packages) {
				String path = ResourcePatternResolver.CLASSPATH_ALL_URL_PREFIX + resolveBasePackage(name) + "/"
						+ DEFAULT_RESOURCE_PATTERN;
				Resource[] tmp = resourcePatternResolver.getResources(path);
				Collections.addAll(resources, tmp);
			}
		}

		if (includes != null) {
			for (String name : includes) {
				String path = ResourcePatternResolver.CLASSPATH_ALL_URL_PREFIX + resolveBasePackage(name) + ".class";
				Resource[] tmp = resourcePatternResolver.getResources(path);
				Collections.addAll(resources, tmp);
			}
		}

		if (excludes != null) {
			for (String name : excludes) {
				String path = ResourcePatternResolver.CLASSPATH_ALL_URL_PREFIX + resolveBasePackage(name) + ".class";
				Resource[] tmp = resourcePatternResolver.getResources(path);
				for (Resource e : tmp) {
					resources.remove(e);
				}
			}
		}

		return resources;
	}

	protected String resolveBasePackage(String basePackage) {
		return ClassUtils.convertClassNameToResourcePath(SystemPropertyUtils.resolvePlaceholders(basePackage));
	}

	// 扫描运行时的 Bean 完成注册的相关方法

	private boolean scanBeans = false;

	@Override
	public Object postProcessAfterInitialization(Object bean, String beanName) throws BeansException {
		if (!isScanBeans()) {
			return bean;
		}

		Class<?> clazz = AnnotationUtility
				.findAnnotationDeclaringClassAndInterface(SocketModule.class, bean.getClass());
		if (clazz == null) {
			return bean;
		}

		SocketModule handler = clazz.getAnnotation(SocketModule.class);
		if (handler != null) {
			byte[] modules = handler.value();
			for (Method method : clazz.getMethods()) {
				SocketCommand socketCommand = AnnotationUtility.findAnnotation(method, SocketCommand.class);
				if (socketCommand == null) {
					// 不是指令方法，跳过不处理
					continue;
				}

				// 创建指令对象
				Command command = null;
				if (socketCommand.modules().length != 0) {
					command = Command.valueOf(socketCommand.value(), socketCommand.modules());
				} else {
					command = Command.valueOf(socketCommand.value(), modules);
				}
				// 进行注册
				try {
					register(command, handler.format(), clazz, bean, method);
				} catch (Exception e) {
					FormattingTuple message = MessageFormatter.format("注册指令[{}]到对应方法[{}]时失败", command, method);
					logger.error(message.getMessage());
					throw new FatalBeanException(message.getMessage(), e);
				}
			}
		}
		return bean;
	}

	// 指令注册方法

	public void register(Command command, byte format, Class<?> clz, Object bean, Method method) throws Exception {
		MethodProcessor handler = MethodProcessor.valueOf(format, clz, bean, method, builder);
		getObject().register(command, handler.getDefinition(), handler);
	}

	private void registerFromClass(Class<?> clz) throws Exception {
		SocketModule handler = clz.getAnnotation(SocketModule.class);
		for (Method method : clz.getMethods()) {
			if (method.getAnnotation(SocketCommand.class) == null) {
				continue; // 忽略无效的方法
			}

			SocketCommand socketCommand = method.getAnnotation(SocketCommand.class);
			// 创建指令对象
			Command command = null;
			if (socketCommand.modules().length != 0) {
				command = Command.valueOf(socketCommand.value(), socketCommand.modules());
			} else {
				command = Command.valueOf(socketCommand.value(), handler.value());
			}
			// 创建类型定义
			MethodDefinition definition = MethodDefinition.valueOf(handler.format(), clz, method, builder);
			// 进行注册
			this.getObject().register(command, definition, null);
		}
	}

	// 实现 Spring 接口的方法

	private CommandRegister register = new CommandRegister();

	@Override
	public CommandRegister getObject() throws Exception {
		return register;
	}

	@Override
	public Class<?> getObjectType() {
		return CommandRegister.class;
	}

	@Override
	public boolean isSingleton() {
		return true;
	}

	@Override
	public Object postProcessBeforeInitialization(Object bean, String beanName) throws BeansException {
		return bean;
	}

	// Getter and Setter ...

	private Convertor convertor;

	public void setConvertor(Convertor convertor) {
		this.convertor = convertor;
	}

	public boolean isScanBeans() {
		return scanBeans;
	}

	public void setScanBeans(boolean scanBeans) {
		this.scanBeans = scanBeans;
	}

	public boolean isScanInterfaces() {
		return scanInterfaces;
	}

	public void setScanInterfaces(boolean scanInterfaces) {
		this.scanInterfaces = scanInterfaces;
	}

	public Set<String> getPackages() {
		return packages;
	}

	public void setPackages(Set<String> packages) {
		this.packages = packages;
	}

	public Set<String> getInterfaces() {
		return interfaces;
	}

	public void setInterfaces(Set<String> interfaces) {
		this.interfaces = interfaces;
	}

	public Set<String> getIncludes() {
		return includes;
	}

	public void setIncludes(Set<String> includes) {
		this.includes = includes;
	}

	public Set<String> getExcludes() {
		return excludes;
	}

	public void setExcludes(Set<String> excludes) {
		this.excludes = excludes;
	}

}
