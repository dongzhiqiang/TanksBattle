package com.engine.common.resource.excel;

import static org.hamcrest.CoreMatchers.is;
import static org.junit.Assert.assertThat;

import java.util.BitSet;
import java.util.Collection;
import java.util.List;
import java.util.Random;

import org.hamcrest.core.Is;
import org.junit.Test;
import org.junit.runner.RunWith;
import org.springframework.stereotype.Component;
import org.springframework.test.context.ContextConfiguration;
import org.springframework.test.context.junit4.SpringJUnit4ClassRunner;

import com.engine.common.resource.Storage;
import com.engine.common.resource.StorageManager;
import com.engine.common.resource.anno.InjectBean;
import com.engine.common.resource.anno.Static;

@RunWith(SpringJUnit4ClassRunner.class)
@ContextConfiguration
@Component
public class ResTest {
	@Static
	private Storage<Integer, Res> storage;
	@Static
	private Storage<Integer, Human> huStorage;
	@Static("1")
	private Res allRes;

	@Test
	public void resTest() {

		Res res = storage.get(1, true);
		assertThat(res.getId(), is(1));
		System.out.println("id：" + res.getId());
		System.out.println("name:" + res.getName());
		Res res2 = storage.get(2, true);
		System.out.println("id:" + res2.getId() + " name:" + res2.getName());
		System.out.println("------------------------------");
		Collection<Res> values = storage.getAll();
		for (Res eRes : values) {
			// System.out.println("*******id:"+eRes.getsId()+" name:"+eRes.getName()+" resId:"+eRes.getResId()+" IntArray["+eRes.getIntArray()[0]+","+eRes.getIntArray()[1]+","+eRes.getIntArray()[2]+"]");
			Grade[] grades = eRes.getGrades();
			if(grades==null){
				continue;
			}
			for (Grade e : grades) {
				System.out.println("******* id:" + eRes.getId()
						+ " singleGrade:" + eRes.getGrade() + "---" + e);

			}
		}
		// System.out.println("直接应用---------");
		// System.out.println("allRes:"+allRes.getId()+":"+allRes.getName()+" "+allRes.getSerNumber());
		// System.out.println("索引测试--------");
		// List<Res> resList=storage.getIndex(Res.name_idx, "wenkin");
		// for(Res eRes:resList){
		// System.out.println("id:"+eRes.getId()+" name:"+eRes.getName()+" resId:"+eRes.getResId()+" IntArray["+eRes.getIntArray()[0]+","+eRes.getIntArray()[1]+","+eRes.getIntArray()[2]+"]");
		// }
		// Res temRes=storage.getUnique(Res.name_idx, "wenkind");
		// System.out.println("id: "+temRes.getId()+" name:"+temRes.getName());
		// List<Res> otherResList= storage.getIndex(Res.type_idx,
		// Byte.valueOf((byte) 10));
		// for(Res eRes:otherResList){
		// System.out.println("id:"+eRes.getId()+" type:"+eRes.getType());
		// }
	}

}
