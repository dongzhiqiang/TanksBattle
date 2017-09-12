package com.engine.common.socket.schema;

import static com.engine.common.socket.schema.AttributeNames.*;
import static com.engine.common.socket.schema.ElementNames.*;
import static com.engine.common.socket.schema.Names.*;

import java.util.List;

import org.apache.mina.core.filterchain.IoFilter;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.slf4j.helpers.FormattingTuple;
import org.slf4j.helpers.MessageFormatter;
import org.springframework.beans.factory.config.RuntimeBeanReference;
import org.springframework.beans.factory.support.AbstractBeanDefinition;
import org.springframework.beans.factory.support.BeanDefinitionBuilder;
import org.springframework.beans.factory.support.BeanDefinitionRegistry;
import org.springframework.beans.factory.support.ManagedMap;
import org.springframework.beans.factory.support.ManagedSet;
import org.springframework.beans.factory.xml.ParserContext;
import org.springframework.util.xml.DomUtils;
import org.w3c.dom.Element;

import com.engine.common.socket.codec.Coder;
import com.engine.common.socket.handler.CommandRegister;

/**
 * 配置文件解析帮助类
 * 
 */
class ParserHelper {

	private static final Logger logger = LoggerFactory.getLogger(ParserHelper.class);

	/** 获取过滤器集合 */
	public static ManagedMap<String, Object> getFilters(Element element, ParserContext parserContext) {
		if (!hasChildElementsByTagName(element, FILTERS)) {
			return null;
		}
		element = getUniqueChildElementByTagName(element, FILTERS);
		ManagedMap<String, Object> result = new ManagedMap<String, Object>();
		result.setValueTypeName(IoFilter.class.getName());
		result.setSource(parserContext.extractSource(element));
		// 设置每个过滤器的配置
		List<Element> elements = DomUtils.getChildElementsByTagName(element, FILTER);
		for (Element e : elements) {
			String name = e.getAttribute(NAME);
			if (e.hasAttribute(REF)) {
				String beanName = e.getAttribute(REF);
				result.put(name, new RuntimeBeanReference(beanName));
			} else {
				String clzName = e.getAttribute(CLASS);
				try {
					@SuppressWarnings("unchecked")
					Class<IoFilter> clz = (Class<IoFilter>) Class.forName(clzName);
					result.put(name, clz.newInstance());
				} catch (Exception ex) {
					FormattingTuple message = MessageFormatter.format("无法创建过滤器[{}]实例", clzName);
					logger.error(message.getMessage(), ex);
					throw new RuntimeException(message.getMessage(), ex);
				}
			}
		}
		return result;
	}

	/** 获取是否克隆客户端编码器 */
	public static boolean isClone(Element element, ParserContext parserContext) {
		if (element.hasAttribute(CLONE)) {
			String attName = element.getAttribute(CLONE);
			boolean clone = Boolean.getBoolean(attName);
			return clone;
		}
		return false;
	}

	/** 获取编码器集合 */
	public static ManagedMap<Byte, Object> getCoders(Element element, ParserContext parserContext) {
		element = getUniqueChildElementByTagName(element, CODERS);
		ManagedMap<Byte, Object> result = new ManagedMap<Byte, Object>();
		result.setValueTypeName(Coder.class.getName());
		result.setSource(parserContext.extractSource(element));
		// 设置每个过滤器的配置
		List<Element> elements = DomUtils.getChildElementsByTagName(element, CODER);
		for (Element e : elements) {
			String name = e.getAttribute(FORMAT);
			Byte format = Byte.parseByte(name);
			if (e.hasAttribute(REF)) {
				String beanName = e.getAttribute(REF);
				result.put(format, new RuntimeBeanReference(beanName));
			} else {
				String clzName = e.getAttribute(CLASS);
				try {
					@SuppressWarnings("unchecked")
					Class<Coder> clz = (Class<Coder>) Class.forName(clzName);
					result.put(format, clz.newInstance());
				} catch (Exception ex) {
					FormattingTuple message = MessageFormatter.format("无法创建编码器[{}]实例", clzName);
					logger.error(message.getMessage(), ex);
					throw new RuntimeException(message.getMessage(), ex);
				}
			}
		}
		return result;
	}

	/** 注册{@link CommandRegister} */
	public static RuntimeBeanReference regCommandRegister(Element element, ParserContext parserContext,
			RuntimeBeanReference convertor) {
		BeanDefinitionRegistry registry = parserContext.getRegistry();

		String id = element.getAttribute(ID);
		element = ParserHelper.getUniqueChildElementByTagName(element, COMMANDS);
		BeanDefinitionBuilder builder = BeanDefinitionBuilder.rootBeanDefinition(CommandRegisterFactory.class);

		builder.addPropertyValue(CONVERTOR_NAME, convertor);
		// 是否扫描容器中的Bean
		builder.addPropertyValue(SCAN_BEANS_NAME, Boolean.valueOf(element.getAttribute(SCAN_BEANS)));
		// 扫描包注册
		if (ParserHelper.hasChildElementsByTagName(element, PACKAGE)) {
			ManagedSet<String> packages = new ManagedSet<String>();
			for (Element e : DomUtils.getChildElementsByTagName(element, PACKAGE)) {
				String name = e.getAttribute(NAME);
				packages.add(name);
			}
			builder.addPropertyValue(PACKAGES_NAME, packages);
			builder.addPropertyValue(SCAN_INTERFACES_NAME, true);
		}
		// 扫描接口注册
		if (ParserHelper.hasChildElementsByTagName(element, INTERFACE)) {
			ManagedSet<String> packages = new ManagedSet<String>();
			for (Element e : DomUtils.getChildElementsByTagName(element, INTERFACE)) {
				String name = e.getAttribute(NAME);
				packages.add(name);
			}
			builder.addPropertyValue(INTERFACES_NAME, packages);
			builder.addPropertyValue(SCAN_INTERFACES_NAME, true);
		}
		// 包含表达式注册
		if (ParserHelper.hasChildElementsByTagName(element, INCLUDE)) {
			ManagedSet<String> packages = new ManagedSet<String>();
			for (Element e : DomUtils.getChildElementsByTagName(element, INCLUDE)) {
				String name = e.getAttribute(NAME);
				packages.add(name);
			}
			builder.addPropertyValue(INCLUDES_NAME, packages);
			builder.addPropertyValue(SCAN_INTERFACES_NAME, true);
		}
		// 排除表达式注册
		if (ParserHelper.hasChildElementsByTagName(element, EXCLUDE)) {
			ManagedSet<String> packages = new ManagedSet<String>();
			for (Element e : DomUtils.getChildElementsByTagName(element, EXCLUDE)) {
				String name = e.getAttribute(NAME);
				packages.add(name);
			}
			builder.addPropertyValue(EXCLUDES_NAME, packages);
			builder.addPropertyValue(SCAN_INTERFACES_NAME, true);
		}

		String name = id + SPLIT + REGISTER_NAME;
		AbstractBeanDefinition factoryDef = builder.getBeanDefinition();
		registry.registerBeanDefinition(name, factoryDef);
		return new RuntimeBeanReference(name);
	}

	/**
	 * 检查是否有指定标签的子元素
	 * @param parent 父元素节点
	 * @param tagName 标签名
	 * @return
	 */
	public static boolean hasChildElementsByTagName(Element parent, String tagName) {
		List<Element> elements = DomUtils.getChildElementsByTagName(parent, tagName);
		if (elements.size() > 0) {
			return true;
		}
		return false;
	}

	/**
	 * 获取唯一的子元素
	 * @param parent 父元素节点
	 * @param tagName 标签名
	 * @return
	 */
	public static Element getUniqueChildElementByTagName(Element parent, String tagName) {
		List<Element> elements = DomUtils.getChildElementsByTagName(parent, tagName);
		if (elements.size() != 1) {
			FormattingTuple message = MessageFormatter.format("Tag Name[{}]的元素数量[{}]不唯一", tagName, elements.size());
			logger.error(message.getMessage());
			throw new RuntimeException(message.getMessage());
		}
		return elements.get(0);
	}

}
