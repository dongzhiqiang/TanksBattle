package com.engine.common.resource.excel;

import static org.hamcrest.CoreMatchers.*;
import static org.junit.Assert.*;

import java.io.File;
import java.io.IOException;
import java.net.URISyntaxException;
import java.util.Observable;
import java.util.Observer;

import org.apache.commons.io.FileUtils;
import org.junit.After;
import org.junit.Test;
import org.junit.runner.RunWith;
import org.springframework.stereotype.Component;
import org.springframework.test.context.ContextConfiguration;
import org.springframework.test.context.junit4.SpringJUnit4ClassRunner;

import com.engine.common.resource.Storage;
import com.engine.common.resource.anno.Static;

/**
 * 重新加载静态资源的测试用例
 * 
 */
@RunWith(SpringJUnit4ClassRunner.class)
@ContextConfiguration
@Component
public class ReloadTest {

	@Static(value = "1")
	private Human human;

	@Static
	private Storage<Integer, Human> storage;
	
	@Test
	public void test_reload() throws URISyntaxException, IOException {
		// 重新加载前的测试
		assertThat(human.getId(), is(1));
		assertThat(human.getName(), is("Frank"));
		assertThat(human.getAge(), is(32));
		assertThat(human.isSex(), is(true));
		Human target = storage.get(1, true);
		assertThat(target, sameInstance(human));

		// 覆盖文件
		override();
		Observer observer = new Observer() {
			@SuppressWarnings({ "unchecked", "rawtypes" })
			@Override
			public void update(Observable o, Object arg) {
				Storage<Integer, Human> target = (Storage<Integer, Human>) o;
				assertThat((Storage) o, sameInstance(storage));
				Human human = target.get(1, true);
				assertThat(human.getName(), is("May"));
				assertThat(human.getAge(), is(31));
				assertThat(human.isSex(), is(false));
			}
		};
		storage.addObserver(observer);
		storage.reload();
		
		// 重新加载后的测试
		assertThat(human.getId(), is(1));
		assertThat(human.getName(), is("May"));
		assertThat(human.getAge(), is(31));
		assertThat(human.isSex(), is(false));
		target = storage.get(1, true);
		assertThat(target, sameInstance(human));
		
	}
	
	@After
	public void recover() throws IOException, URISyntaxException {
		if (!override) {
			return;
		}
		File src = new File(this.getClass().getResource(BACKUP).toURI());
		File dest = new File(this.getClass().getResource(CURRENT).toURI());
		FileUtils.copyFile(src, dest);
	}

	private static final String BACKUP = "backup.xlsx";
	private static final String CURRENT = "Human.xlsx";
	private static final String RELOAD = "reload.xlsx";
	private boolean override;
	
	private void override() throws URISyntaxException, IOException {
		File src = new File(this.getClass().getResource(CURRENT).toURI());
		File dest = new File(this.getClass().getResource(BACKUP).toURI());
		FileUtils.copyFile(src, dest);
		
		src = new File(this.getClass().getResource(RELOAD).toURI());
		dest = new File(this.getClass().getResource(CURRENT).toURI());
		FileUtils.copyFile(src, dest);
		
		override = true;
	}
}
