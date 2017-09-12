package com.engine.common.resource.excel;

import static org.hamcrest.CoreMatchers.*;
import static org.junit.Assert.*;

import javax.annotation.PostConstruct;

import org.springframework.beans.factory.InitializingBean;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;

import com.engine.common.resource.Storage;
import com.engine.common.resource.StorageManager;
import com.engine.common.resource.anno.Static;

@Component
public class StaticTestTarget implements InitializingBean {

	@Autowired
	private StorageManager resourceManager;
	@Static
	private Storage<Integer, Human> storage;
	@Static(value = "1")
	private Human human;
	
	@PostConstruct
	protected void initilize() {
		assertThat(resourceManager, notNullValue());
		assertThat(storage, notNullValue());
		assertThat(human, notNullValue());
	}

	@Override
	public void afterPropertiesSet() throws Exception {
		assertThat(resourceManager, notNullValue());
		assertThat(storage, notNullValue());
		assertThat(human, notNullValue());
	}

}
