package com.engine.common.ramcache.schema;

import java.io.IOException;
import java.util.HashMap;
import java.util.HashSet;
import java.util.Map;
import java.util.Set;

import org.apache.commons.lang3.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.slf4j.helpers.FormattingTuple;
import org.slf4j.helpers.MessageFormatter;
import org.springframework.beans.factory.support.AbstractBeanDefinition;
import org.springframework.beans.factory.support.BeanDefinitionBuilder;
import org.springframework.beans.factory.support.BeanDefinitionRegistry;
import org.springframework.beans.factory.support.ManagedMap;
import org.springframework.beans.factory.xml.AbstractBeanDefinitionParser;
import org.springframework.beans.factory.xml.ParserContext;
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
import org.springframework.util.xml.DomUtils;
import org.w3c.dom.Element;
import org.w3c.dom.Node;
import org.w3c.dom.NodeList;

import com.engine.common.ramcache.IEntity;
import com.engine.common.ramcache.anno.Cached;
import com.engine.common.ramcache.anno.Persister;
import com.engine.common.ramcache.aop.LockAspect;
import com.engine.common.ramcache.exception.ConfigurationException;
import com.engine.common.ramcache.persist.PersisterConfig;
import com.engine.common.ramcache.persist.PersisterType;

/**
 * 服务器定义处理器
 * 
 */
public class RamCacheParser extends AbstractBeanDefinitionParser {

	private static final Logger logger = LoggerFactory.getLogger(RamCacheParser.class);
	
	/** 默认资源匹配符 */
	protected static final String DEFAULT_RESOURCE_PATTERN = "**/*.class";
	/** 资源搜索分析器，由它来负责检索EAO接口 */
	private ResourcePatternResolver resourcePatternResolver = new PathMatchingResourcePatternResolver();
	/** 类的元数据读取器，由它来负责读取类上的注释信息 */
	private MetadataReaderFactory metadataReaderFactory = new CachingMetadataReaderFactory(this.resourcePatternResolver);

	@SuppressWarnings({ "unchecked", "rawtypes" })
	@Override
	protected AbstractBeanDefinition parseInternal(Element element, ParserContext parserContext) {
		// 注册注入处理器
		registerInjectProcessor(parserContext);
		
		// 注册锁拦截切面
		if (Boolean.valueOf(element.getAttribute(AttributeNames.LOCK_ASPECT))) {
			registerLockAspect(parserContext);
		}

		// 创建工厂类定义
		BeanDefinitionBuilder builder = BeanDefinitionBuilder.rootBeanDefinition(ServiceManagerFactory.class);

		// 设置存储器
		builder.addPropertyReference(ElementNames.ACCESSOR, getAccessorBeanName(element));
		// 设置查询器
		builder.addPropertyReference(ElementNames.QUERIER, getQuerierBeanName(element));
		// 设置常量配置信息
		parseConstants2Bean(builder, DomUtils.getChildElementByTagName(element, ElementNames.CONSTANTS), parserContext);
		
		// 设置持久化处理器配置
		Map<String, PersisterConfig> persisterConfigs = new HashMap<String, PersisterConfig>();
		Element persisterElement = DomUtils.getChildElementByTagName(element, ElementNames.PERSIST);
		PersisterType type = PersisterType.valueOf(persisterElement.getAttribute(AttributeNames.TYPE));
		String value = persisterElement.getAttribute(AttributeNames.CONFIG);
		persisterConfigs.put(Persister.DEFAULT, new PersisterConfig(type, value));
		for (Element e : DomUtils.getChildElementsByTagName(persisterElement, ElementNames.PERSISTER)) {
			String name = e.getAttribute(AttributeNames.NAME);
			type = PersisterType.valueOf(e.getAttribute(AttributeNames.TYPE));
			value = e.getAttribute(AttributeNames.CONFIG);
			persisterConfigs.put(name, new PersisterConfig(type, value));
		}
		builder.addPropertyValue(ServiceManagerFactory.PERSISTER_CONFIG_NAME, persisterConfigs);
		
		// 设置实体集合
		Set<Class<? extends IEntity>> classes = new HashSet<Class<? extends IEntity>>();
		NodeList child = DomUtils.getChildElementByTagName(element, ElementNames.ENTITY).getChildNodes();
		for (int i = 0; i < child.getLength(); i++) {
			Node node = child.item(i);
			if (node.getNodeType() != Node.ELEMENT_NODE) {
				continue;
			}
			String name = node.getLocalName();
			
			if (name.equals(ElementNames.PACKAGE)) {
				// 自动包扫描处理
				String packageName = ((Element) node).getAttribute(AttributeNames.NAME);
				String[] names = getResources(packageName);
				for (String resource : names) {
					Class<? extends IEntity> clz = null;
					try {
						clz = (Class<? extends IEntity>) Class.forName(resource);
					} catch (ClassNotFoundException e) {
						FormattingTuple message = MessageFormatter.format("无法获取的资源类[{}]", resource);
						logger.error(message.getMessage());
						throw new ConfigurationException(message.getMessage(), e);
					}
					classes.add(clz);
				}
			}
			
			if (name.equals(ElementNames.CLASS)) {
				// 自动类加载处理
				String className = ((Element) node).getAttribute(AttributeNames.NAME);
				Class<? extends IEntity> clz = null;
				try {
					clz = (Class<? extends IEntity>) Class.forName(className);
				} catch (ClassNotFoundException e) {
					FormattingTuple message = MessageFormatter.format("无法获取的资源类[{}]", className);
					logger.error(message.getMessage());
					throw new ConfigurationException(message.getMessage(), e);
				}
				classes.add(clz);
			}
		}
		builder.addPropertyValue(ServiceManagerFactory.ENTITY_CLASSES_NAME, classes);
		
		return builder.getBeanDefinition();
	}
	
	private void parseConstants2Bean(BeanDefinitionBuilder builder, Element element, ParserContext parserContext) {
		String ref = element.getAttribute(AttributeNames.REF);
		// 引用设置
		if (StringUtils.isNotBlank(ref)) {
			builder.addPropertyReference(ElementNames.CONSTANTS, ref);
			return;
		}
		
		// 指定设置
		ManagedMap<String, Integer> constants = new ManagedMap<String, Integer>();
		for (Element e : DomUtils.getChildElementsByTagName(element, ElementNames.CONSTANT)) {
			String name = e.getAttribute(AttributeNames.NAME);
			Integer value = Integer.parseInt(e.getAttribute(AttributeNames.SIZE));
			constants.put(name, value);
		}
		builder.addPropertyValue(ElementNames.CONSTANTS, constants);
	}

	/** 注册{@link LockAspect} */
	private void registerLockAspect(ParserContext parserContext) {
		BeanDefinitionRegistry registry = parserContext.getRegistry();
		String name = StringUtils.uncapitalize(LockAspect.class.getSimpleName());
		BeanDefinitionBuilder factory = BeanDefinitionBuilder.rootBeanDefinition(LockAspect.class);
		registry.registerBeanDefinition(name, factory.getBeanDefinition());
	}

	/**
	 * 注册 {@link InjectProcessor}
	 * @param parserContext
	 */
	private void registerInjectProcessor(ParserContext parserContext) {
		BeanDefinitionRegistry registry = parserContext.getRegistry();
		String name = StringUtils.uncapitalize(InjectProcessor.class.getSimpleName());
		BeanDefinitionBuilder factory = BeanDefinitionBuilder.rootBeanDefinition(InjectProcessor.class);
		registry.registerBeanDefinition(name, factory.getBeanDefinition());
	}

	/** 获取查询器配置的 Bean 引用 */
	private String getQuerierBeanName(Element element) {
		element = ParserHelper.getUniqueChildElementByTagName(element, ElementNames.QUERIER);
		// 引用处理
		if (element.hasAttribute(AttributeNames.REF)) {
			return element.getAttribute(AttributeNames.REF);
		}
		throw new ConfigurationException("查询器配置声明缺失");
	}

	/** 获取存储器配置的 Bean 引用 */
	private String getAccessorBeanName(Element element) {
		element = ParserHelper.getUniqueChildElementByTagName(element, ElementNames.ACCESSOR);
		// 引用处理
		if (element.hasAttribute(AttributeNames.REF)) {
			return element.getAttribute(AttributeNames.REF);
		}
		throw new ConfigurationException("存储器配置声明缺失");
	}

	/**
	 * 获取指定包下的静态资源对象
	 * @param packageName 包名
	 * @return
	 * @throws IOException 
	 */
	private String[] getResources(String packageName) {
		try {
			// 搜索资源
			String packageSearchPath = ResourcePatternResolver.CLASSPATH_ALL_URL_PREFIX
					+ resolveBasePackage(packageName) + "/" + DEFAULT_RESOURCE_PATTERN;
			Resource[] resources = this.resourcePatternResolver.getResources(packageSearchPath);
			// 提取资源
			Set<String> result = new HashSet<String>();
			String name = Cached.class.getName();
			for (Resource resource : resources) {
				if (!resource.isReadable()) {
					continue;
				}
				// 判断是否静态资源
				MetadataReader metaReader = this.metadataReaderFactory.getMetadataReader(resource);
				AnnotationMetadata annoMeta = metaReader.getAnnotationMetadata();
				if (!annoMeta.hasAnnotation(name)) {
					continue;
				}
				ClassMetadata clzMeta = metaReader.getClassMetadata();
				result.add(clzMeta.getClassName());
			}
			
			return result.toArray(new String[0]);
		} catch (IOException e) {
			String message = "无法读取资源信息";
			logger.error(message, e);
			throw new ConfigurationException(message, e);
		}
	}

	protected String resolveBasePackage(String basePackage) {
		return ClassUtils.convertClassNameToResourcePath(SystemPropertyUtils.resolvePlaceholders(basePackage));
	}

}
