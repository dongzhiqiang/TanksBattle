package com.engine.common.socket.schema;

import org.springframework.beans.factory.xml.NamespaceHandlerSupport;

/**
 * 模块的 XML 命名空间注册器
 * 
 */
public class NamespaceHandler extends NamespaceHandlerSupport {

	@Override
	public void init() {
		// 注册服务器解析器
		registerBeanDefinitionParser(ElementNames.SERVER, new ServerParser());
		// 注册客户端工厂解析器
		registerBeanDefinitionParser(ElementNames.CLIENT_FACTORY, new ClientFactoryParser());
	}

}
