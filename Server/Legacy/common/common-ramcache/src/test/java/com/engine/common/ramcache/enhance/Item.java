package com.engine.common.ramcache.enhance;

import javax.persistence.Embeddable;
import javax.persistence.Embedded;
import javax.persistence.Entity;
import javax.persistence.Id;
import javax.persistence.NamedQueries;
import javax.persistence.NamedQuery;

import com.engine.common.ramcache.IEntity;
import com.engine.common.ramcache.anno.CacheUnit;
import com.engine.common.ramcache.anno.Cached;
import com.engine.common.ramcache.anno.ChangeIndex;
import com.engine.common.ramcache.anno.Idx;
import com.engine.common.ramcache.enhance.Enhance;

@Entity
@Cached(size = "default", unit = CacheUnit.REGION)
@NamedQueries({ @NamedQuery(name = "Item.owner", query = "from Item i where i.owner = ?") })
public class Item implements IEntity<Integer> {

	@Id
	private Integer id;
	@Idx(query = "Item.owner")
	private int owner;

	@Embedded
	private Content content = new Content();

	@Enhance
	public void setOwner(@ChangeIndex("owner") int owner) {
		this.owner = owner;
	}

	// Getter and Setter ...

	public Integer getId() {
		return id;
	}

	protected void setId(Integer id) {
		this.id = id;
	}

	public int getOwner() {
		return owner;
	}

	//

	public Content getContent() {
		return content;
	}

	public void setContent(Content content) {
		this.content = content;
	}

	@Embeddable
	public static class Content {
		private String name;
		private String title;

		public String getName() {
			return name;
		}

		public String getTitle() {
			return title;
		}

		public void setName(String name) {
			this.name = name;
		}

		public void setTitle(String title) {
			this.title = title;
		}

	}

}
