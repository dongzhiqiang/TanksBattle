package com.engine.common.resource.support;

import java.util.Collections;
import java.util.Set;

import org.springframework.core.convert.TypeDescriptor;
import org.springframework.core.convert.converter.ConditionalGenericConverter;

import com.engine.common.utils.json.JsonUtils;

/**
 * 将json格式的array字符串转换成对应的数组实例
 * 
 */
public class JsonToArrayConverter implements ConditionalGenericConverter {

	@Override
	public boolean matches(TypeDescriptor sourceType, TypeDescriptor targetType) {
		if (sourceType.getType() != String.class) {
			return false;
		}
		if (!targetType.getType().isArray()) {
			return false;
		}
		return true;
	}

	@Override
	public Set<ConvertiblePair> getConvertibleTypes() {
		return Collections.singleton(new ConvertiblePair(String.class, Object[].class));
	}

	@Override
	public Object convert(Object source, TypeDescriptor sourceType, TypeDescriptor targetType) {
		String content = (String) source;
		if (targetType.getElementTypeDescriptor().isPrimitive()) {
			return JsonUtils.string2Object(content, targetType.getObjectType());
		} else {
			Class<?> elementType = targetType.getElementTypeDescriptor().getType();
			return JsonUtils.string2Array(content, elementType);
		}
	}

}
