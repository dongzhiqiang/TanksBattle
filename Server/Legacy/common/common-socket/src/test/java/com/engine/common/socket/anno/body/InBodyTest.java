package com.engine.common.socket.anno.body;

import static org.hamcrest.CoreMatchers.*;
import static org.junit.Assert.*;

import java.util.HashMap;
import java.util.Map;

import org.junit.Before;
import org.junit.Test;
import org.junit.runner.RunWith;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.test.context.ContextConfiguration;
import org.springframework.test.context.junit4.SpringJUnit4ClassRunner;

import com.engine.common.socket.client.Client;
import com.engine.common.socket.client.ClientFactory;
import com.engine.common.socket.core.Command;
import com.engine.common.socket.core.Request;
import com.engine.common.socket.core.Response;
import com.engine.common.socket.model.Person;

@RunWith(SpringJUnit4ClassRunner.class)
@ContextConfiguration
@SuppressWarnings("unchecked")
public class InBodyTest {

	@Autowired
	private ClientFactory clientFactory;
	private Client client;
	
	@Before
	public void before() {
		client = clientFactory.getClient(true);
	}
	
	@Test
	public void test_object_person_id() {
		Command command = Command.valueOf(1, 2);
		Person p = Person.valueOf(1, "frank");
		Request<Person> request = Request.valueOf(command, p);
		Response<Integer> response = client.send(request, Integer.class);
		
		assertThat(response.hasError(), is(false));
		assertThat(response.getSn(), is(request.getSn()));
		assertThat(response.getCommand(), is(command));
		assertThat(response.getBody(), is(1));
	}

	@Test
	public void test_object_person_name() {
		Command command = Command.valueOf(2, 2);
		Person p = Person.valueOf(1, "frank");
		Request<Person> request = Request.valueOf(command, p);
		Response<String> response = client.send(request, String.class);
		
		assertThat(response.hasError(), is(false));
		assertThat(response.getSn(), is(request.getSn()));
		assertThat(response.getCommand(), is(command));
		assertThat(response.getBody(), is("frank"));
	}

	@Test
	public void test_array_double() {
		Command command = Command.valueOf(3, 2);
		Request<Double[]> request = Request.valueOf(command, new Double[]{1.0, 2.0});
		Response<Double> response = client.send(request, Double.class);
		
		assertThat(response.hasError(), is(false));
		assertThat(response.getSn(), is(request.getSn()));
		assertThat(response.getCommand(), is(command));
		assertThat(response.getBody(), is(1.0));
	}
	
	@Test
	public void test_array_string() {
		Command command = Command.valueOf(4, 2);
		Request<String[]> request = Request.valueOf(command, new String[]{"a", "b"});
		Response<String> response = client.send(request, String.class);
		
		assertThat(response.hasError(), is(false));
		assertThat(response.getSn(), is(request.getSn()));
		assertThat(response.getCommand(), is(command));
		assertThat(response.getBody(), is("b"));
	}

	@Test
	public void test_array_object() {
		Command command = Command.valueOf(5, 2);
		Object[] body = {Person.valueOf(1, "frank"), "fuck"};
		Request<Object[]> request = Request.valueOf(command, body);
		Response<Person> response = client.send(request, Person.class);
		
		assertThat(response.hasError(), is(false));
		assertThat(response.getSn(), is(request.getSn()));
		assertThat(response.getCommand(), is(command));
		//assertThat(response.getBody(), is(new Person[]{Person.valueOf(2, "ramon"), Person.valueOf(1, "frank")}));
	}

	@Test
	public void test_map_primitive() {
		Command command = Command.valueOf(6, 2);
		Map<String, Object> map = new HashMap<String, Object>();
		map.put("one", 10);
		map.put("two", "frank");
		Request<Map<String, Object>> request = Request.valueOf(command, map);
		Response<Map<String, Object>> response = (Response<Map<String, Object>>) client.send(request);
		
		assertThat(response.hasError(), is(false));
		assertThat(response.getSn(), is(request.getSn()));
		assertThat(response.getCommand(), is(command));
		Map<String, Object> body = response.getBody();
		assertThat((Integer) body.get("two"), is(10));
		assertThat((String) body.get("one"), is("frank"));
	}

	@Test
	public void test_map_person() {
		Command command = Command.valueOf(7, 2);
		Map<String, Object> map = new HashMap<String, Object>();
		Person person = Person.valueOf(1, "frank");
		map.put("person", person);
		map.put("id", "uuid");
		Request<Map<String, Object>> request = Request.valueOf(command, map);
		Response<Person> response = (Response<Person>) client.send(request);
		
		assertThat(response.hasError(), is(false));
		assertThat(response.getSn(), is(request.getSn()));
		assertThat(response.getCommand(), is(command));
		assertThat(response.getBody(), is(person));
	}

	@Test
	public void test_required() {
		Command command = Command.valueOf(8, 2);
		Map<String, Object> map = new HashMap<String, Object>();

		Request<Map<String, Object>> request = Request.valueOf(command, map);
		Response<Integer> response = (Response<Integer>) client.send(request);
		assertThat(response.hasError(), is(false));
		assertThat(response.getSn(), is(request.getSn()));
		assertThat(response.getCommand(), is(command));
		assertThat(response.getBody(), is(3));

		map.put("person", Person.valueOf(1, "frank"));
		Request.valueOf(command, map);
		response = (Response<Integer>) client.send(request);
		assertThat(response.hasError(), is(false));
		assertThat(response.getSn(), is(request.getSn()));
		assertThat(response.getCommand(), is(command));
		assertThat(response.getBody(), is(2));
		
		map.clear();
		map.put("id", "uuid");
		Request.valueOf(command, map);
		response = (Response<Integer>) client.send(request);
		assertThat(response.hasError(), is(false));
		assertThat(response.getSn(), is(request.getSn()));
		assertThat(response.getCommand(), is(command));
		assertThat(response.getBody(), is(1));

		map.put("person", Person.valueOf(1, "frank"));
		Request.valueOf(command, map);
		response = (Response<Integer>) client.send(request);
		assertThat(response.hasError(), is(false));
		assertThat(response.getSn(), is(request.getSn()));
		assertThat(response.getCommand(), is(command));
		assertThat(response.getBody(), is(0));
	}

}
