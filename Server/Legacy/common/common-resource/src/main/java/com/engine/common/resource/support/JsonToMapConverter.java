package com.engine.common.resource.support;

import java.util.Collections;
import java.util.Map;
import java.util.Set;

import org.springframework.core.convert.TypeDescriptor;
import org.springframework.core.convert.converter.ConditionalGenericConverter;

import com.engine.common.utils.json.JsonUtils;

/**
 * 将json格式的map字符串转换成对应的Map实例
 * 
 */
public class JsonToMapConverter implements ConditionalGenericConverter {

	@Override
	public boolean matches(TypeDescriptor sourceType, TypeDescriptor targetType) {
		if (sourceType.getType() != String.class) {
			return false;
		}
		if (!Map.class.isAssignableFrom(targetType.getType())) {
			return false;
		}
		return true;
	}

	@Override
	public Set<ConvertiblePair> getConvertibleTypes() {
		return Collections.singleton(new ConvertiblePair(String.class, Map.class));
	}

	@Override
	public Object convert(Object source, TypeDescriptor sourceType, TypeDescriptor targetType) {
		String string = (String) source;
		return JsonUtils.string2Map(string, targetType.getMapKeyTypeDescriptor().getType(), targetType
				.getMapValueTypeDescriptor().getType());
	}

}
