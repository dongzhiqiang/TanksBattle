package com.engine.common.resource.excel;

import com.engine.common.resource.anno.Id;
import com.engine.common.resource.anno.Resource;

@Resource
public class Pet {

	@Id
	private Integer id;
	private String name;

	public Integer getId() {
		return id;
	}

	public void setId(Integer id) {
		this.id = id;
	}

	public String getName() {
		return name;
	}

	public void setName(String name) {
		this.name = name;
	}

}
