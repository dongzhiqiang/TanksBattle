package com.engine.common.resource.spring;

import org.springframework.core.convert.ConversionService;

import com.engine.common.resource.anno.Id;
import com.engine.common.resource.anno.InjectBean;
import com.engine.common.resource.anno.Resource;

@Resource
public class Config {

	@InjectBean
	private static ConversionService conversionService;

	@Id
	private String id;
	private String value;

	@InjectBean("testBean")
	private InjectObject injectObject;

	public ConversionService getConversionService() {
		return conversionService;
	}

	public InjectObject getInjectObject() {
		return injectObject;
	}

	// Getter and Setter ...

	public String getId() {
		return id;
	}

	public void setId(String id) {
		this.id = id;
	}

	public String getValue() {
		return value;
	}

	public void setValue(String value) {
		this.value = value;
	}

}
