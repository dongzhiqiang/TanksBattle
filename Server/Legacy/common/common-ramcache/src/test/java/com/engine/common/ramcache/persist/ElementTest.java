package com.engine.common.ramcache.persist;

import static org.hamcrest.CoreMatchers.*;
import static org.junit.Assert.*;

import org.junit.Test;

import com.engine.common.ramcache.IEntity;
import com.engine.common.ramcache.persist.Element;
import com.engine.common.ramcache.persist.EventType;

public class ElementTest {
	
	@SuppressWarnings("rawtypes")
	@Test
	public void update_test() {
		{	
			// SAVE,SAVE:非法
			Person prevEntity = new Person(1, "frank");
			Element element = Element.saveOf(prevEntity);
			Person currentEntity = new Person(1, "frank");
			assertThat(element.update(Element.saveOf(currentEntity)), is(true));
			assertThat(element.getType(), is(EventType.SAVE));
			assertThat(element.getEntity(), sameInstance((IEntity) currentEntity));
			// SAVE,REMOVE
			element = Element.saveOf(prevEntity);
			assertThat(element.update(Element.removeOf(1, Person.class)), is(false));
			assertThat(element.getEntity(), nullValue());
			// SAVE,UPDATE
			element = Element.saveOf(prevEntity);
			assertThat(element.update(Element.updateOf(currentEntity)), is(true));
			assertThat(element.getType(), is(EventType.SAVE));
			assertThat(element.getEntity(), sameInstance((IEntity) currentEntity));
		}
		{
			// REMOVE,SAVE
			Element element = Element.removeOf(1, Person.class);
			Person currentEntity = new Person(1, "frank");
			assertThat(element.update(Element.saveOf(currentEntity)), is(true));
			assertThat(element.getType(), is(EventType.UPDATE));
			assertThat(element.getEntity(), sameInstance((IEntity) currentEntity));
			// REMOVE,REMOVE:非法
			element = Element.removeOf(1, Person.class);
			assertThat(element.update(Element.removeOf(1, Person.class)), is(true));
			assertThat(element.getType(), is(EventType.REMOVE));
			assertThat(element.getEntity(), nullValue());
			// REMOVE,UPDATE:非法
			element = Element.removeOf(1, Person.class);
			assertThat(element.update(Element.updateOf(currentEntity)), is(true));
			assertThat(element.getType(), is(EventType.REMOVE));
			assertThat(element.getEntity(), sameInstance((IEntity) currentEntity));
		}
		{
			// UPDATE,SAVE:非法
			Person prevEntity = new Person(1, "frank");
			Element element = Element.updateOf(prevEntity);
			Person currentEntity = new Person(1, "frank");
			assertThat(element.update(Element.saveOf(currentEntity)), is(true));
			assertThat(element.getType(), is(EventType.UPDATE));
			assertThat(element.getEntity(), sameInstance((IEntity) currentEntity));
			// UPDATE,REMOVE
			element = Element.updateOf(prevEntity);
			assertThat(element.update(Element.removeOf(1, Person.class)), is(true));
			assertThat(element.getType(), is(EventType.REMOVE));
			assertThat(element.getEntity(), nullValue());
			// UPDATE,UPDATE
			element = Element.updateOf(prevEntity);
			assertThat(element.update(Element.updateOf(currentEntity)), is(true));
			assertThat(element.getType(), is(EventType.UPDATE));
			assertThat(element.getEntity(), sameInstance((IEntity) currentEntity));
		}
	}

}
