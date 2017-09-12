package test;

import static org.junit.Assert.*;

import java.io.IOException;
import java.util.ArrayList;
import java.util.Date;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.TreeMap;

import org.apache.mina.core.buffer.IoBuffer;

import com.engine.common.protocol.Transfer;

public class Test {

	@org.junit.Test
	public void test() throws Exception {
		IoBuffer buf = IoBuffer.allocate(100);
		List<Class<?>> list = new ArrayList<Class<?>>();
		list.add(Person.class);
		list.add(Status.class);
		Transfer enc = new Transfer(list, 1);
		Transfer dec = new Transfer(list, 1);
		
		//32位最大值
		{
			IoBuffer r1 = enc.encode(Integer.MAX_VALUE);
			Object r = dec.decode(r1);
			assertEquals(r, Integer.MAX_VALUE);
		}
		
		//数组
		{
			int[] array = new int[2];
			array[0] = 1;
			array[1]=2;
			buf = enc.encode(array);
			int[] r1 = dec.decode(buf, array.getClass());
			assertArrayEquals(array, r1);

		}
		
		
		//byte数组测试
		byte[] bytes={-1,1,127,-127,-128};
		buf=enc.encode(bytes);
		buf.rewind();
		byte[] bytes_dec=(byte[]) dec.decode(buf);
		assertEquals(bytes.length, bytes_dec.length);
		for(int i=0;i<bytes_dec.length;i++){
			assertEquals(bytes_dec[i], bytes[i]);
		}
		//NULL测试
		Object obj=null;
		buf=enc.encode(obj);
		buf.rewind();
		Object dobj=dec.decode(buf);
		 assertTrue(dobj==null);
		//32位数字
		for (int i = 0; i < Integer.MAX_VALUE; i = (i + 1) * 1000) {
			buf = enc.encode(i);
			Object r1 = dec.decode(buf);
			assertEquals(i, r1);
			if (i < 0) {
				break;
			}
		}
		//32位数字(负数)
		for (int i = 0; i < Integer.MAX_VALUE; i = (i + 1) * 1000) {
			buf = enc.encode(-i);
			buf.rewind();
			Object r1 = dec.decode(buf);
			assertEquals(-i, r1);
			if (i < 0) {
				break;
			}
		}
		//64位数字
		for (long i = 0; i < 0x00FFFFFFFFFFFFFFL; i = (i + 1) * 1000) {
			buf = enc.encode(i);
			buf.rewind();
			Object r1 = dec.decode(buf);
			assertEquals(i, r1);
			if (i < 0) {
				break;
			}
		}
		//64位数字(负数)
		for (long i = 0; i < 0x00FFFFFFFFFFFFFFL; i = (i + 1) * 1000) {
			buf = enc.encode(-i);
			buf.rewind();
			Object r1 = dec.decode(buf);
			assertEquals(-i, r1);
			if (i < 0) {
				break;
			}
		}
		//boolean测试
		boolean b=false;
		buf=enc.encode(b);
		buf.rewind();
	    boolean afb=(Boolean) dec.decode(buf);
		assertEquals(b,afb);
		//字符串测试
		String str="wenkin34";
		buf=enc.encode(str);
		buf.rewind();
	    String afstr=(String)dec.decode(buf);
	    assertTrue(str.equals(afstr));
        
	    //枚举测试
	    Status status=Status.NEW;
	    buf=enc.encode(status);
	    buf.rewind();
	    Status status_dec=(Status) dec.decode(buf);
	    assertEquals(status_dec,Status.NEW);
	    
	    //日期类型测试(精确到秒)
	    long mills=System.currentTimeMillis();
	    Date date=new Date(mills);
	    buf=enc.encode(date);
	    buf.rewind();
	    Date date_dec=(Date) dec.decode(buf);
	    System.out.println(date_dec.getTime());
	    assertEquals(date_dec.getYear(),date.getYear());
	    assertEquals(date_dec.getMonth(),date.getMonth());
	    assertEquals(date_dec.getDay(),date.getDay());
	    assertEquals(date_dec.getHours(),date.getHours());
	    assertEquals(date_dec.getMinutes(),date.getMinutes());
	    assertEquals(date_dec.getSeconds(),date.getSeconds());
	    
        
        
		//对象
		{
			Person person = Person.valueOf(1, "FAL");
			person.setOk("NO");
			buf = enc.encode(person);
			buf.rewind();
			Object r1 = dec.decode(buf);
			assertTrue(person.equals(r1));
		}
		
		//集合测试
		 List listData=new ArrayList();
		 listData.add("wenkin");
		 listData.add("test");
		 listData.add(true);
		 listData.add(100);
		 buf=enc.encode(listData);
		 buf.rewind();
		 List listData_dec=(List) dec.decode(buf);
		 assertEquals(listData_dec.size(), listData.size());
		 for(int i=0;i<listData.size();i++){
			  assertTrue(listData_dec.get(i).equals(listData.get(i)));
		 }
		 
		//对象数组
		{
			Person person = Person.valueOf(2, "N2");
			Object[] arr = new Object[] { person, person, person, person, person, person, person };
			buf = enc.encode(arr);
			buf.rewind();
			Object[] r1 = (Object[]) dec.decode(buf);
			assertArrayEquals(arr, r1);
			System.out.println(buf.position());
		}
		
		//对象集合
		{
			Person person = Person.valueOf(1, "OK");
			List<Person> ps = new ArrayList<Person>();
			ps.add(person);
			person.setList(ps);
			buf = enc.encode(person);
			buf.rewind();
			Object r1 = dec.decode(buf);
			assertEquals(person, r1);
		}
        
		//对象Map
		{
			Map<String, Object> person = new TreeMap<String, Object>();
			person.put("name", "HELLO");
			person.put("id", 1L);
			person.put("age", 20);
			buf = enc.encode(person);
			buf.rewind();
			Object r1 = dec.decode(buf);
			assertEquals(person, r1);
		}

		{
			Map<String, Object> m1 = new HashMap<String, Object>();
			Map<String, Object> m2 = new HashMap<String, Object>();
			Map<String, Object> map = new HashMap<String, Object>();
			map.put("1", m1);
			map.put("2", m2);

			buf = enc.encode(map);
			buf.rewind();
			@SuppressWarnings("unchecked")
			Map<String, Object> r1 = (Map<String, Object>) dec.decode(buf);
			assertNotSame(r1.get("1"), r1.get("2"));
		}

	}
	
//	@org.junit.Test
	public void testEncodeAndDecode(){
         Transfer transfer=new Transfer();
         
         try {
        	 
        	int intvalue=100;
			IoBuffer buffer=transfer.encode(intvalue);
			Object obj=transfer.decode(buffer);
			System.out.println("解码后的value"+obj);
			
			boolean b=false;
			IoBuffer buf=transfer.encode(b);
		    boolean afb=(Boolean) transfer.decode(buf);
			System.out.println("after afb:"+afb);
			
			String str="wenkin34";
			IoBuffer strbuf=transfer.encode(str);
		    String afstr=(String)transfer.decode(strbuf);
		    assertTrue(str.equals(afstr));
		    
		} catch (IOException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
	}
}
