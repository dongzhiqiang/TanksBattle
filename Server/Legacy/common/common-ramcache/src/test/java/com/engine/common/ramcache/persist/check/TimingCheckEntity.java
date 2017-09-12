package com.engine.common.ramcache.persist.check;

import javax.persistence.Entity;
import javax.persistence.Id;

import com.engine.common.ramcache.IEntity;
import com.engine.common.ramcache.anno.Cached;
import com.engine.common.ramcache.anno.Persister;
import com.engine.common.ramcache.enhance.Enhance;

@Entity
@Cached(size = "30000", persister = @Persister("pre_min"))
public class TimingCheckEntity implements IEntity<Integer> {
	
	@Id
	private Integer id;
	private String content;

	@Override
	public Integer getId() {
		return id;
	}

	void setId(Integer id) {
		this.id = id;
	}
	
	public String getContent() {
		return content;
	}

	@Enhance
	void setContent(String content) {
		this.content = content;
	}

}
