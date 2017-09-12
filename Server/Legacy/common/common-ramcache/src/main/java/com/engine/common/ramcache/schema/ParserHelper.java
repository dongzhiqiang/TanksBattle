package com.engine.common.ramcache.schema;

import java.util.List;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.slf4j.helpers.FormattingTuple;
import org.slf4j.helpers.MessageFormatter;
import org.springframework.util.xml.DomUtils;
import org.w3c.dom.Element;

import com.engine.common.ramcache.exception.ConfigurationException;

/**
 * 配置文件解析帮助类
 * 
 */
public abstract class ParserHelper {
	
	private static final Logger logger = LoggerFactory.getLogger(ParserHelper.class);
	
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
			throw new ConfigurationException(message.getMessage());
		}
		return elements.get(0);
	}

}
