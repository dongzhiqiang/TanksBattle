package com.engine.common.socket.protocol;

import static org.hamcrest.CoreMatchers.*;
import static org.junit.Assert.*;

import org.apache.mina.core.session.IoSession;
import org.junit.Before;
import org.junit.Test;
import org.junit.runner.RunWith;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.test.context.ContextConfiguration;
import org.springframework.test.context.junit4.SpringJUnit4ClassRunner;

import com.engine.common.socket.client.Client;
import com.engine.common.socket.client.ClientFactory;
import com.engine.common.socket.core.Command;
import com.engine.common.socket.core.MessageConstant;
import com.engine.common.socket.core.Request;
import com.engine.common.socket.core.Response;

@RunWith(SpringJUnit4ClassRunner.class)
@ContextConfiguration
public class BasicTest {

	@Autowired
	private BasicFacade facade;
	@Autowired
	private ClientFactory clientFactory;

	private Client client;
	
	@Before
	public void before() {
		client = clientFactory.getClient(false);
	}
	
	@Test
	public void test_count() {
		Command command = Command.valueOf(1, 1);
		Request<?> request = Request.valueOf(command, null);
		Response<?> response = client.send(request, void.class);
		
		assertThat(response.hasError(), is(false));
		assertThat(response.getBody(), nullValue());
		assertThat(response.getSn(), is(request.getSn()));
		assertThat(response.getCommand(), is(command));
		assertThat(facade.getCount(), is(1));
	}
	
	@Test
	public void test_session() throws SecurityException, NoSuchFieldException, IllegalArgumentException, IllegalAccessException {
		Command command = Command.valueOf(2, 1);
		Request<?> request = Request.valueOf(command, null);
		Response<?> response = client.send(request, void.class);
		
		assertThat(response.hasError(), is(false));
		assertThat(response.getBody(), nullValue());
		assertThat(response.getSn(), is(request.getSn()));
		assertThat(response.getCommand(), is(command));
		IoSession serverSession = facade.getSession();
		assertThat(serverSession, notNullValue());
	}
	
	@Test
	public void test_request() throws Exception {
		Command command = Command.valueOf(3, 1);
		Request<String> request = Request.valueOf(command, "TEST");
		Response<?> response = client.send(request, void.class);
		
		assertThat(response.hasError(), is(false));
		assertThat(response.getBody(), nullValue());
		assertThat(response.getSn(), is(request.getSn()));
		assertThat(response.getCommand(), is(command));
		assertThat(facade.getRequest(), is("TEST"));
	}

	@Test
	public void test_body() {
		Command command = Command.valueOf(4, 1);
		Person p = Person.valueOf(1, "frank");
		Request<Person> request = Request.valueOf(command, p);
		Response<?> response = client.send(request, void.class);

		assertThat(response.hasError(), is(false));
		assertThat(response.getBody(), nullValue());
		assertThat(response.getSn(), is(request.getSn()));
		assertThat(response.getCommand(), is(command));
		Person body = facade.getBody();
		assertThat(body, is(p));
	}
	
	@Test
	public void test_compress() throws InterruptedException {
		Command command = Command.valueOf(5, 1);
		String string = "{\"result\":\"defender\",\"attacker\":[{\"name\":null,\"identifier\":\"攻击方\",\"attacks\":{\"strategy\":20,\"business\":20,\"technology\":20},\"defenses\":{\"strategy\":10,\"business\":10,\"technology\":10},\"maxs\":{\"dp\":100},\"energy\":0,\"dp\":100,\"speed\":13,\"position\":\"MANAGEMENT\",\"sex\":true,\"status\":0}],\"round\":[{\"content\":{\"action\":[{\"ACTION_EXECUTE\":{\"content\":[{\"value\":{\"dp\":-10},\"target\":\"防守方1\"},{\"value\":{\"dp\":-10},\"target\":\"防守方2\"},{\"value\":{\"dp\":-10},\"target\":\"防守方3\"}],\"name\":\"NormalAttack\",\"owner\":\"攻击方\"},\"name\":\"NormalAttack\",\"owner\":\"攻击方\"},{\"ACTION_EXECUTE\":{\"content\":[{\"value\":{\"dp\":-10},\"target\":\"攻击方\"}],\"name\":\"NormalAttack\",\"owner\":\"防守方1\"},\"name\":\"NormalAttack\",\"owner\":\"防守方1\",\"ACTION_START\":[{\"content\":[{\"target\":\"防守方2\"}],\"name\":\"JointAttack\",\"owner\":\"防守方1\"}]},{\"ACTION_EXECUTE\":{\"content\":[{\"value\":{\"dp\":-10},\"target\":\"攻击方\"}],\"name\":\"NormalAttack\",\"owner\":\"防守方2\"},\"name\":\"NormalAttack\",\"owner\":\"防守方2\",\"ACTION_START\":[{\"content\":[{\"target\":\"防守方3\"}],\"name\":\"JointAttack\",\"owner\":\"防守方2\"}]},{\"ACTION_EXECUTE\":{\"content\":[{\"value\":{\"dp\":-10},\"target\":\"攻击方\"}],\"name\":\"NormalAttack\",\"owner\":\"防守方3\"},\"name\":\"NormalAttack\",\"owner\":\"防守方3\"},{\"ACTION_EXECUTE\":{\"content\":[{\"value\":{\"dp\":-10},\"target\":\"攻击方\"}],\"name\":\"NormalAttack\",\"owner\":\"防守方2\"},\"name\":\"NormalAttack\",\"owner\":\"防守方2\",\"ACTION_START\":[{\"content\":[{\"target\":\"防守方3\"}],\"name\":\"JointAttack\",\"owner\":\"防守方2\"}]},{\"ACTION_EXECUTE\":{\"content\":[{\"value\":{\"dp\":-10},\"target\":\"攻击方\"}],\"name\":\"NormalAttack\",\"owner\":\"防守方3\"},\"name\":\"NormalAttack\",\"owner\":\"防守方3\"},{\"ACTION_EXECUTE\":{\"content\":[{\"value\":{\"dp\":-10},\"target\":\"攻击方\"}],\"name\":\"NormalAttack\",\"owner\":\"防守方3\"},\"name\":\"NormalAttack\",\"owner\":\"防守方3\"}]}},{\"content\":{\"action\":[{\"ACTION_EXECUTE\":{\"content\":[{\"value\":{\"dp\":-10},\"target\":\"防守方1\"},{\"value\":{\"dp\":-10},\"target\":\"防守方2\"},{\"value\":{\"dp\":-10},\"target\":\"防守方3\"}],\"name\":\"NormalAttack\",\"owner\":\"攻击方\"},\"name\":\"NormalAttack\",\"owner\":\"攻击方\"},{\"ACTION_EXECUTE\":{\"content\":[{\"value\":{\"dp\":-10},\"target\":\"攻击方\"}],\"name\":\"NormalAttack\",\"owner\":\"防守方1\"},\"name\":\"NormalAttack\",\"owner\":\"防守方1\",\"ACTION_START\":[{\"content\":[{\"target\":\"防守方2\"}],\"name\":\"JointAttack\",\"owner\":\"防守方1\"}]},{\"ACTION_EXECUTE\":{\"content\":[{\"value\":{\"dp\":-10},\"target\":\"攻击方\"}],\"name\":\"NormalAttack\",\"owner\":\"防守方2\"},\"name\":\"NormalAttack\",\"owner\":\"防守方2\",\"ACTION_START\":[{\"content\":[{\"target\":\"防守方3\"}],\"name\":\"JointAttack\",\"owner\":\"防守方2\"}]},{\"ACTION_EXECUTE\":{\"content\":[{\"value\":{\"dp\":-10},\"target\":\"攻击方\"}],\"name\":\"NormalAttack\",\"owner\":\"防守方3\"},\"name\":\"NormalAttack\",\"owner\":\"防守方3\"},{\"ACTION_EXECUTE\":{\"content\":[{\"value\":{\"dp\":-10},\"target\":\"攻击方\"}],\"name\":\"NormalAttack\",\"owner\":\"防守方2\"},\"name\":\"NormalAttack\",\"owner\":\"防守方2\",\"ACTION_START\":[{\"content\":[{\"target\":\"防守方3\"}],\"name\":\"JointAttack\",\"owner\":\"防守方2\"}]},{\"ACTION_EXECUTE\":{\"content\":[{\"cancel\":true}],\"name\":\"NormalAttack\",\"owner\":\"防守方3\"},\"name\":\"NormalAttack\",\"owner\":\"防守方3\"},{\"ACTION_EXECUTE\":{\"content\":[{\"cancel\":true}],\"name\":\"NormalAttack\",\"owner\":\"防守方3\"},\"name\":\"NormalAttack\",\"owner\":\"防守方3\"}]}}],\"defender\":[{\"name\":null,\"identifier\":\"防守方1\",\"attacks\":{\"strategy\":20,\"business\":20,\"technology\":20},\"defenses\":{\"strategy\":10,\"business\":10,\"technology\":10},\"maxs\":{\"dp\":100},\"energy\":0,\"dp\":100,\"speed\":13,\"position\":\"MANAGEMENT\",\"sex\":true,\"status\":0},{\"name\":null,\"identifier\":\"防守方2\",\"attacks\":{\"strategy\":20,\"business\":20,\"technology\":20},\"defenses\":{\"strategy\":10,\"business\":10,\"technology\":10},\"maxs\":{\"dp\":100},\"energy\":0,\"dp\":100,\"speed\":13,\"position\":\"MANAGEMENT\",\"sex\":true,\"status\":0},{\"name\":null,\"identifier\":\"防守方3\",\"attacks\":{\"strategy\":20,\"business\":20,\"technology\":20},\"defenses\":{\"strategy\":10,\"business\":10,\"technology\":10},\"maxs\":{\"dp\":100},\"energy\":0,\"dp\":100,\"speed\":13,\"position\":\"MANAGEMENT\",\"sex\":true,\"status\":0}]}";
		Request<String> request = Request.valueOf(command, string);
		Response<String> response = client.send(request, String.class);
		
		assertThat(response.hasError(), is(false));
		assertThat(response.hasState(MessageConstant.STATE_COMPRESS), is(true));
		assertThat(response.getSn(), is(request.getSn()));
		assertThat(response.getCommand(), is(command));
		assertThat(response.getBody(), is(string));
	}
}
