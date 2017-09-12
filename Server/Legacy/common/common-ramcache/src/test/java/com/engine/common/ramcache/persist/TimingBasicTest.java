package com.engine.common.ramcache.persist;

import static org.hamcrest.CoreMatchers.*;
import static org.junit.Assert.*;

import org.junit.Test;
import org.junit.runner.RunWith;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.test.context.ContextConfiguration;
import org.springframework.test.context.junit4.SpringJUnit4ClassRunner;

import com.engine.common.ramcache.orm.Accessor;
import com.engine.common.ramcache.persist.Element;
import com.engine.common.ramcache.persist.TimingPersister;

@RunWith(SpringJUnit4ClassRunner.class)
@ContextConfiguration
public class TimingBasicTest {

	@Autowired
	private Accessor accessor;
	private TimingPersister persister = new TimingPersister();
	
	@Test
	public void test_merge() {
		persister.initialize("test", accessor, "0 0 0 1 1 *");

		// SAVE, SAVE
		persister.put(Element.saveOf(new Person(1, "frank")));
		persister.put(Element.saveOf(new Person(1, "frank")));
		assertThat(persister.size(), is(1));
		persister.flush();
		
		// SAVE, REMOVE
		persister.put(Element.saveOf(new Person(2, "frank")));
		persister.put(Element.removeOf(2, Person.class));
		assertThat(persister.size(), is(0));
		persister.flush();
		
		// SAVE, UPDATE
		persister.put(Element.saveOf(new Person(3, "frank")));
		persister.put(Element.updateOf(new Person(3, "ramon")));
		assertThat(persister.size(), is(1));
		persister.flush();

		// REMOVE, SAVE
		persister.put(Element.removeOf(1, Person.class));
		persister.put(Element.saveOf(new Person(1, "ramon")));
		assertThat(persister.size(), is(1));
		persister.flush();
		
		persister.shutdown();
	}

}
