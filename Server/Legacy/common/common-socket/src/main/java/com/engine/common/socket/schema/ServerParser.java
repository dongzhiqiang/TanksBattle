package com.engine.common.socket.schema;

import static com.engine.common.socket.schema.AttributeNames.*;
import static com.engine.common.socket.schema.ElementNames.*;
import static com.engine.common.socket.schema.Names.*;

import org.apache.commons.lang3.StringUtils;
import org.apache.mina.transport.socket.SocketAcceptor;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.slf4j.helpers.FormattingTuple;
import org.slf4j.helpers.MessageFormatter;
import org.springframework.beans.factory.config.BeanDefinition;
import org.springframework.beans.factory.config.RuntimeBeanReference;
import org.springframework.beans.factory.support.AbstractBeanDefinition;
import org.springframework.beans.factory.support.BeanDefinitionBuilder;
import org.springframework.beans.factory.support.BeanDefinitionRegistry;
import org.springframework.beans.factory.xml.AbstractBeanDefinitionParser;
import org.springframework.beans.factory.xml.ParserContext;
import org.w3c.dom.Element;

import com.engine.common.socket.core.Convertor;
import com.engine.common.socket.server.ServerConfig;
import com.engine.common.socket.server.ServerHandler;
import com.engine.common.socket.server.SocketServer;

/**
 * 服务器定义处理器
 * 
 */
public class ServerParser extends AbstractBeanDefinitionParser {

	private static final Logger logger = LoggerFactory.getLogger(ServerParser.class);

	@Override
	protected AbstractBeanDefinition parseInternal(Element element, ParserContext parserContext) {
		// 创建服务器定义
		BeanDefinitionBuilder builder = BeanDefinitionBuilder.rootBeanDefinition(SocketServer.class);
		String name = element.getAttribute(DEPENDS_ON);
		if (StringUtils.isNotEmpty(name)) {
			builder.addDependsOn(name);
		}

		// 注册编码转换器
		RuntimeBeanReference convertor = regConvertor(element, parserContext);

		// 设置配置属性
		builder.addPropertyValue(CONFIG_NAME, registerConfig(element, parserContext));
		// 设置接收器属性
		builder.addPropertyValue(ACCEPTOR_NAME, getAcceptor(element, parserContext));
		// 设置控制器属性
//		builder.addPropertyValue(HANDLER_NAME, registerHandlerFactory(element, parserContext));
		builder.addPropertyValue(HANDLER_NAME, regHandler(element, parserContext, convertor));
		// 设置过滤器集合属性
		builder.addPropertyValue(FILTERS_NAME, ParserHelper.getFilters(element, parserContext));
		return builder.getBeanDefinition();
	}
	
	/** 注册{@link ServerHandler} */
	private RuntimeBeanReference regHandler(Element element, ParserContext parserContext, RuntimeBeanReference convertor) {
		// 注册指令注册器工厂
		RuntimeBeanReference register = ParserHelper.regCommandRegister(element, parserContext, convertor);
		
		// 注册控制器
		String serverId = element.getAttribute(ID);
		BeanDefinitionRegistry registry = parserContext.getRegistry();
		BeanDefinitionBuilder builder = BeanDefinitionBuilder.rootBeanDefinition(ServerHandler.class);
		builder.addConstructorArgValue(register);
		builder.addPropertyValue(CONVERTOR_NAME, convertor);
		String beanName = serverId + SPLIT + HANDLER_NAME;
		registry.registerBeanDefinition(beanName, builder.getBeanDefinition());
		return new RuntimeBeanReference(beanName);
	}

	/** 注册{@link Convertor} */
	private RuntimeBeanReference regConvertor(Element element, ParserContext parserContext) {
		String serverId = element.getAttribute(ID);
		BeanDefinitionRegistry registry = parserContext.getRegistry();
		BeanDefinitionBuilder builder = BeanDefinitionBuilder.rootBeanDefinition(Convertor.class);
		builder.addPropertyValue(CODERS_NAME, ParserHelper.getCoders(element, parserContext));
		String convertorName = serverId + SPLIT + CONVERTOR_NAME;
		registry.registerBeanDefinition(convertorName, builder.getBeanDefinition());
		return new RuntimeBeanReference(convertorName);
	}

//	/** 注册{@link BeanScanHolder} */
//	public RuntimeBeanReference registerHandlerFactory(Element element, ParserContext parserContext) {
//		String serverId = element.getAttribute(ID);
//		BeanDefinitionRegistry registry = parserContext.getRegistry();
//		
//		BeanDefinitionBuilder builder;
//		registerConvertor(element, parserContext, serverId, registry);
//		
//		// 注册服务端控制器工厂
//		builder = BeanDefinitionBuilder.rootBeanDefinition(HandlerFactory.class);
//		builder.addPropertyValue(HANDLER_NAME, registerHandler(element, parserContext, convertor));
//		builder.addPropertyValue(SCAN_BEANS_NAME, true);
//		// 扫描包注册
//		if (ParserHelper.hasChildElementsByTagName(element, SCAN_DEFINITION)) {
//			Element scanElement = ParserHelper.getUniqueChildElementByTagName(element, SCAN_DEFINITION);
//			ManagedSet<String> packages = new ManagedSet<String>();
//			for (Element e : DomUtils.getChildElementsByTagName(scanElement, PACKAGE)) {
//				String name = e.getAttribute(NAME);
//				packages.add(name);
//			}
//			builder.addPropertyValue(PACKAGES_NAME, packages);
//			builder.addPropertyValue(SCAN_INTERFACES_NAME, true);
//		}
//		// 扫描接口注册
//		if (ParserHelper.hasChildElementsByTagName(element, SCAN_INTERFACES)) {
//			Element scanElement = ParserHelper.getUniqueChildElementByTagName(element, SCAN_INTERFACES);
//			ManagedSet<String> packages = new ManagedSet<String>();
//			for (Element e : DomUtils.getChildElementsByTagName(scanElement, INTERFACE)) {
//				String name = e.getAttribute(NAME);
//				packages.add(name);
//			}
//			builder.addPropertyValue(INTERFACES_NAME, packages);
//			builder.addPropertyValue(SCAN_INTERFACES_NAME, true);
//		}
//		
//		String name = serverId + SPLIT + HANDLER_FACTORY_NAME;
//		AbstractBeanDefinition factoryDef = builder.getBeanDefinition();
//		registry.registerBeanDefinition(name, factoryDef);
//		return new RuntimeBeanReference(name);
//	}
//
//	/** 注册{@link Handler} */
//	public static RuntimeBeanReference registerHandler(Element element, ParserContext parserContext, RuntimeBeanReference convertor) {
//		String id = element.getAttribute(ID);
//		BeanDefinitionRegistry registry = parserContext.getRegistry();
//		
//		BeanDefinitionBuilder builder = null;
//		if (ParserHelper.hasChildElementsByTagName(element, HANDLER)) {
//			// 声明处理
//			Element handlerElement = ParserHelper.getUniqueChildElementByTagName(element, HANDLER);
//			if (handlerElement.hasAttribute(CLASS)) {
//				String clzName = element.getAttribute(CLASS);
//				try {
//					@SuppressWarnings("unchecked")
//					Class<Handler> clz = (Class<Handler>) Class.forName(clzName);
//					BeanDefinitionBuilder handlerBuilder = BeanDefinitionBuilder.rootBeanDefinition(clz);
//					handlerBuilder.addPropertyValue(CONVERTOR_NAME, convertor);
//					AbstractBeanDefinition handlerDef = handlerBuilder.getBeanDefinition();
//					String name = id + SPLIT + HANDLER_NAME;
//					registry.registerBeanDefinition(name, handlerDef);
//					return new RuntimeBeanReference(name);
//				} catch (Exception e) {
//					FormattingTuple message = MessageFormatter.format("无法创建控制器[{}]实例", clzName);
//					logger.error(message.getMessage(), e);
//					throw new RuntimeException(message.getMessage(), e);
//				}
//			} else {
//				String name = element.getAttribute(REF);
//				return new RuntimeBeanReference(name);
//			}
//		} else {
//			// 默认处理方式
//			builder = BeanDefinitionBuilder.rootBeanDefinition(ServerHandler.class);
//			builder.addPropertyValue(CONVERTOR_NAME, convertor);
//			AbstractBeanDefinition handlerDef = builder.getBeanDefinition();
//			String name = id + SPLIT + HANDLER_NAME;
//			registry.registerBeanDefinition(name, handlerDef);
//			return new RuntimeBeanReference(name);
//		}
//	}

	/** 获取{@link SocketAcceptor}属性值 */
	private Object getAcceptor(Element element, ParserContext parserContext) {
		element = ParserHelper.getUniqueChildElementByTagName(element, ACCEPTOR);
		// 引用处理
		if (element.hasAttribute(REF)) {
			String beanName = element.getAttribute(REF);
			return new RuntimeBeanReference(beanName);
		}
		// 创建处理
		String clzName = element.getAttribute(CLASS);
		try {
			@SuppressWarnings("unchecked")
			Class<SocketAcceptor> clz = (Class<SocketAcceptor>) Class.forName(clzName);
			return clz.newInstance();
		} catch (Exception e) {
			FormattingTuple message = MessageFormatter.format("无法创建接收器[{}]实例", clzName);
			logger.error(message.getMessage(), e);
			throw new RuntimeException(message.getMessage(), e);
		}
	}

	/** 创建{@link ServerConfig}的{@link BeanDefinition} */
	private RuntimeBeanReference registerConfig(Element element, ParserContext parserContext) {
		String id = element.getAttribute(ID);
		String location = element.getAttribute(AttributeNames.CONFIG);
		BeanDefinitionBuilder builder = BeanDefinitionBuilder.rootBeanDefinition(ServerConfig.class);
		builder.addPropertyValue(ServerConfig.PROP_NAME_LOCATION, location);
		BeanDefinition result = builder.getBeanDefinition();
		BeanDefinitionRegistry registry = parserContext.getRegistry();
		
		String name = id + ServerConfig.BEAN_NAME_SUFFIX;
		registry.registerBeanDefinition(name, result);
		return new RuntimeBeanReference(name);
	}

}
