package com.engine.common.ramcache.aop;

import java.util.List;
import java.util.Map;

import org.springframework.stereotype.Component;

import com.engine.common.ramcache.aop.AutoLocked;
import com.engine.common.ramcache.aop.IsLocked;

@Component
public class TestTarget {
	
	@AutoLocked
	public void methodObject(@IsLocked Object obj1, @IsLocked Object obj2) {
	}
	
	@AutoLocked
	public void methodList(@IsLocked(element = true) List<Object> list) {
	}

	@AutoLocked
	public void methodArrayOne(@IsLocked(element = true) Object...objs) {
	}
	
	@AutoLocked
	public void methodArrayTwo(@IsLocked(element = true) Object[] objs) {
	}
	
	@SuppressWarnings("rawtypes")
	@AutoLocked
	public void methodMap(@IsLocked(element = true) Map map) {
	}
	
}
