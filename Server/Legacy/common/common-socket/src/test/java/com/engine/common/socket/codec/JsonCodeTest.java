package com.engine.common.socket.codec;

import static org.hamcrest.CoreMatchers.*;
import static org.junit.Assert.*;

import java.io.IOException;
import java.nio.charset.Charset;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import org.codehaus.jackson.JsonFactory;
import org.codehaus.jackson.JsonGenerationException;
import org.codehaus.jackson.JsonNode;
import org.codehaus.jackson.JsonParser;
import org.codehaus.jackson.map.JsonMappingException;
import org.codehaus.jackson.map.ObjectMapper;
import org.codehaus.jackson.map.type.TypeFactory;
import org.junit.Test;

import com.engine.common.socket.codec.JsonCoder;
import com.engine.common.socket.model.Generic;
import com.engine.common.socket.model.Person;

public class JsonCodeTest {
	
	private JsonCoder coder = new JsonCoder();
	
	@Test
	public void test_string_encode() {
		String content = "12345";
		byte[] bytes = coder.encode(content, null);
		String target = new String(bytes, Charset.forName("utf-8"));
		assertThat(target, is("\"" + content + "\""));

		content = "中文测试";
		bytes = coder.encode(content, null);
		target = new String(bytes, Charset.forName("utf-8"));
		assertThat(target, is("\"" + content + "\""));
	}

	@Test
	public void test_string_decode() {
		byte[] bytes = "\"12345\"".getBytes();
		String target = (String) coder.decode(bytes, String.class);
		assertThat(target, is("12345"));
		
		bytes = "\"中文测试\"".getBytes();
		target = (String) coder.decode(bytes, String.class);
		assertThat(target, is("中文测试"));
	}
	
	@Test
	public void test_number() {
		Byte n1 = 100;
		byte[] bytes = coder.encode(n1, Byte.class);
		Byte t1 = (Byte) coder.decode(bytes, Byte.class);
		assertThat(n1, is(t1));
		
		Short n2 = 200;
		bytes = coder.encode(n2, Short.class);
		Short t2 = (Short) coder.decode(bytes, Short.class);
		assertThat(n2, is(t2));

		Integer n3 = 300;
		bytes = coder.encode(n3, Integer.class);
		Integer t3 = (Integer) coder.decode(bytes, Integer.class);
		assertThat(n3, is(t3));

		Long n4 = 400L;
		bytes = coder.encode(n4, Long.class);
		Long t4 = (Long) coder.decode(bytes, Long.class);
		assertThat(n4, is(t4));

		Float n5 = 500.5F;
		bytes = coder.encode(n5, Float.class);
		Float t5 = (Float) coder.decode(bytes, Float.class);
		assertThat(n5, is(t5));
		
		Double n6 = 400.5;
		bytes = coder.encode(n6, Double.class);
		Double t6 = (Double) coder.decode(bytes, Double.class);
		assertThat(n6, is(t6));
	}
	
	@Test
	public void test_array() {
		int[] a1 = {1, 2, 3, 4};
		byte[] bytes = coder.encode(a1, int[].class);
		int[] t1 = (int[]) coder.decode(bytes, int[].class);
		assertThat(a1, is(t1));
		
		Integer[] a2 = {5, 6, 7, 8};
		bytes = coder.encode(a2, Integer[].class);
		Integer[] t2 = (Integer[]) coder.decode(bytes, Integer[].class);
		assertThat(a2, is(t2));
		
		String[] a3 = {"a", "b", "c", "d"};
		bytes = coder.encode(a3, String[].class);
		String[] t3 = (String[]) coder.decode(bytes, String[].class);
		assertThat(a3, is(t3));
	}
	
	@Test
	public void test_object() {
		Person p1 = Person.valueOf(1, "frank");
		byte[] bytes = coder.encode(p1, Person.class);
		Person t1 = (Person) coder.decode(bytes, Person.class);
		assertThat(p1, is(t1));
	}
	
	@Test
	public void test_array_object() {
		Person[] a1 = {Person.valueOf(1, "frank"), Person.valueOf(2, "ramon")};
		byte[] bytes = coder.encode(a1, Person[].class);
		Person[] t1 = (Person[]) coder.decode(bytes, Person[].class);
		assertThat(a1, is(t1));
	}
	
	private static final TypeFactory typeFactory = TypeFactory.defaultInstance();
	
	@SuppressWarnings("unchecked")
	@Test
	public void test_collection() {
		List<Person> list = new ArrayList<Person>();
		list.add(Person.valueOf(1, "frank"));
		list.add(Person.valueOf(2, "ramon"));
		byte[] bytes = coder.encode(list, null);
		list = (List<Person>) coder.decode(bytes, typeFactory.constructCollectionType(ArrayList.class, Person.class));
		assertThat(list.get(0).getId(), is(1));
		assertThat(list.get(1).getId(), is(2));
	}

	@SuppressWarnings("unchecked")
	@Test
	public void test_map() {
		Map<String, Integer> m1 = new HashMap<String, Integer>();
		m1.put("a", 1);
		m1.put("b", 2);
		m1.put("c", 3);
		byte[] bytes = coder.encode(m1, Map.class);
		Map<String, Integer> t1 = (Map<String, Integer>) coder.decode(bytes, Map.class);
		assertThat(m1, is(t1));

		Map<String, Long> m2 = new HashMap<String, Long>();
		m2.put("a", 1L);
		m2.put("b", 2L);
		m2.put("c", 3L);
		bytes = coder.encode(m2, Map.class);
		Map<String, Long> t2 = (Map<String, Long>) coder.decode(bytes, typeFactory.constructMapType(Map.class, String.class, Long.class));
		assertThat(m2, is(t2));
		
		Map<Double, Person> m3 = new HashMap<Double, Person>();
		m3.put(0.1, Person.valueOf(1, "frank"));
		m3.put(0.2, Person.valueOf(2, "ramon"));
		bytes = coder.encode(m3, Map.class);
		Map<Double, Person> t3 = (Map<Double, Person>) coder.decode(bytes, typeFactory.constructMapType(Map.class, Double.class, Person.class));
		assertThat(m3, is(t3));
	}
	
	@SuppressWarnings("unchecked")
	@Test
	public void test_generic() {
		Generic<Long, Double> g1 = Generic.valueOf(1L, 0.5);
		byte[] bytes = coder.encode(g1, Generic.class);
		Generic<Long, Double> t1 = (Generic<Long, Double>) coder.decode(bytes, typeFactory.constructParametricType(Generic.class, Long.class, Double.class));
		assertThat(g1, is(t1));
	}
	
	@SuppressWarnings("unchecked")
	@Test
	public void test_node_map() throws JsonGenerationException, JsonMappingException, IOException {
		JsonFactory jsonFactory = new JsonFactory();
		ObjectMapper mapper = new ObjectMapper(jsonFactory);
		
		Map<String, Object> map = new HashMap<String, Object>();
		map.put("long", 5L);
		map.put("person", Person.valueOf(1, "frank"));
		
		String s = mapper.writeValueAsString(map);
		byte[] bytes = mapper.writeValueAsBytes(map);
		System.out.println(s);
		
		Map<String, Object> target = (Map<String, Object>) mapper.readValue(s, Object.class); 
		System.out.println(target.get("long").getClass());
		System.out.println(target.get("person").getClass());
		
		JsonNode node = mapper.readValue(bytes, 0, bytes.length, JsonNode.class);
//		JsonNode node = mapper.readTree(s);
		JsonParser parser = node.get("person").traverse();
		parser.setCodec(new ObjectMapper());
		Person p = parser.readValueAs(Person.class);
		System.out.println(p);
		
	}

	@SuppressWarnings("rawtypes")
	@Test
	public void test_node_array() throws JsonGenerationException, JsonMappingException, IOException {
		ObjectMapper mapper = new ObjectMapper();
		
		Object[] array = {Person.valueOf(1, "frank"), Generic.valueOf(2, "ramon")};
		String s = mapper.writeValueAsString(array);
		System.out.println(s);
		
		Object[] target = (Object[]) mapper.readValue(s, Object[].class); 
		System.out.println(Arrays.toString(target));
		
		JsonNode node = mapper.readTree(s);
		System.out.println(node.isArray());
		JsonParser parser = node.get(0).traverse();
		parser.setCodec(new ObjectMapper());
		Person p = parser.readValueAs(Person.class);
		System.out.println(p);
		
		parser = node.get(1).traverse();
		parser.setCodec(new ObjectMapper());
		Generic g = parser.readValueAs(Generic.class);
		System.out.println(g);
	}
	
}
