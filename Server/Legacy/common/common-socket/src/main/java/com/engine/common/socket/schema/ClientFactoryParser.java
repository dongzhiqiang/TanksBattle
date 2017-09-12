package com.engine.common.socket.schema;

import static com.engine.common.socket.schema.AttributeNames.*;
import static com.engine.common.socket.schema.Names.*;

import org.apache.commons.lang3.StringUtils;
import org.springframework.beans.factory.config.RuntimeBeanReference;
import org.springframework.beans.factory.support.AbstractBeanDefinition;
import org.springframework.beans.factory.support.BeanDefinitionBuilder;
import org.springframework.beans.factory.support.BeanDefinitionRegistry;
import org.springframework.beans.factory.xml.AbstractBeanDefinitionParser;
import org.springframework.beans.factory.xml.ParserContext;
import org.w3c.dom.Element;

import com.engine.common.socket.client.ClientFactory;
import com.engine.common.socket.core.Convertor;

/**
 * 客户端工厂定义处理器
 * 
 */
public class ClientFactoryParser extends AbstractBeanDefinitionParser {

	@Override
	protected AbstractBeanDefinition parseInternal(Element element, ParserContext parserContext) {
		// 创建客户端工厂定义
		BeanDefinitionBuilder builder = BeanDefinitionBuilder.rootBeanDefinition(ClientFactory.class);
		String name = element.getAttribute(DEPENDS_ON);
		if (StringUtils.isNotEmpty(name)) {
			builder.addDependsOn(name);
		}
		
		// 设置编码转换器
		RuntimeBeanReference convertor = regConvertor(element, parserContext);
		builder.addPropertyValue(ClientFactory.PROP_CONVERTOR, convertor);
		// 设置指令注册器
		RuntimeBeanReference register = ParserHelper.regCommandRegister(element, parserContext, convertor);
		builder.addPropertyValue(ClientFactory.PROP_COMMAND_REGISTER, register);
		// 设置配置文件位置
		String location = element.getAttribute(AttributeNames.CONFIG);
		builder.addPropertyValue(ClientFactory.PROP_LOCATION, location);
		// 设置过滤器集合属性
		builder.addPropertyValue(ClientFactory.PROP_FILTERS, ParserHelper.getFilters(element, parserContext));
		// 设置是否克隆编码器
		builder.addPropertyValue(ClientFactory.PROP_CLONE, ParserHelper.isClone(element, parserContext));
		return builder.getBeanDefinition();
	}
	
	// 内部方法

	/** 注册{@link Convertor} */
	private RuntimeBeanReference regConvertor(Element element, ParserContext parserContext) {
		String facotryId = element.getAttribute(ID);
		BeanDefinitionRegistry registry = parserContext.getRegistry();
		BeanDefinitionBuilder builder = BeanDefinitionBuilder.rootBeanDefinition(Convertor.class);
		builder.addPropertyValue(CODERS_NAME, ParserHelper.getCoders(element, parserContext));
		String convertorName = facotryId + SPLIT + CONVERTOR_NAME;
		registry.registerBeanDefinition(convertorName, builder.getBeanDefinition());
		return new RuntimeBeanReference(convertorName);
	}
}
