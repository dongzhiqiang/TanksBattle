package test.perf;

import java.lang.reflect.Method;
import java.util.ArrayList;
import java.util.List;

import test.Person;
import test.Pet;
import test.Status;

import com.engine.common.protocol.Transfer;
import com.engine.common.protocol.utils.HashUtils;

public class CheckSum {

	@org.junit.Test
	public void test() throws Exception {

		List<Class<?>> list = new ArrayList<Class<?>>();
		list.add(Person.class);
		list.add(Pet.class);
		list.add(Status.class);
		Transfer enc = new Transfer(list, 0);
		byte[] bytes = enc.getDescription();
		int len = bytes.length;

		int total = 1000000;

		Method[] ms = HashUtils.class.getDeclaredMethods();
		for (Method m : ms) {
			String name = m.getName();
			Class<?>[] ps = m.getParameterTypes();
			if (ps.length == 2) {
				long s = System.currentTimeMillis();
				for (int i = 0; i < total; i++) {
					m.invoke(HashUtils.class, bytes, len);
				}
				System.out.println(name + ",\t 长度 -" + len + ",\t HASH" + total
						+ "次总时间" + (System.currentTimeMillis() - s));
			}
		}
	}

}
