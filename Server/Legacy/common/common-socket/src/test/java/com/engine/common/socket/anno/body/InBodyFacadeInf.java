package com.engine.common.socket.anno.body;

import java.util.Map;

import com.engine.common.socket.anno.InBody;
import com.engine.common.socket.anno.SocketCommand;
import com.engine.common.socket.anno.SocketModule;
import com.engine.common.socket.model.Person;

/**
 * {@link InBody}测试接口
 * 
 */
@SocketModule(2)
public interface InBodyFacadeInf {

	@SocketCommand(1)
	Integer personId(@InBody("id") int id);
	
	@SocketCommand(2)
	String personName(@InBody("name") String name);
	
	@SocketCommand(3)
	Double arrayDouble(@InBody("0") double number);

	@SocketCommand(4)
	String arrayString(@InBody("1") String string);
	
	@SocketCommand(5)
	Person arrayObject(@InBody("0") Person p1, @InBody("1") String string);
	
	@SocketCommand(6)
	Map<String, Object> mapPrimitive(@InBody("one") Long one, @InBody("two") String two);
	
	@SocketCommand(7)
	Person mapPerson(@InBody("person") Person person, @InBody("id") String id);
	
	@SocketCommand(8)
	int required(@InBody(value = "person", required = false) Person person,
			@InBody(value = "id", required = false) String id);
	
}
