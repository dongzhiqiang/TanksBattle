package com.game.gow.console;

import java.io.IOException;
import com.engine.common.socket.client.Client;
import com.engine.common.socket.core.Command;
import com.engine.common.socket.core.Request;
import com.engine.common.socket.core.Response;

public class ProtocolDescribe {

	public static void main(String[] arguments) throws IOException, Exception {
//		ResourceBundle rb = ResourceBundle.getBundle("server");
//		String pstr = rb.getString("server.socket.address");
//		String addr = "localhost" + pstr.substring(pstr.indexOf(":"));
//		SocketSessionConfig sessionConfig = new DefaultSocketSessionConfig();
//		sessionConfig.setReadBufferSize(512);
//		sessionConfig.setWriteTimeout(512);
//		sessionConfig.setBothIdleTime(512);
//
//		Map<Byte, Coder> coders = new HashMap<Byte, Coder>();
//		final Transfer transfer = new Transfer();
//		coders.put((byte) 0, ProtocolCoder.valueOf(transfer));
//
//		CommandRegister register = new CommandRegister();
//		Command request_description = Command.valueOf(SystemModule.REQUEST_DESCRIPTION, SystemModule.MODULES);
//		Command md5_description = Command.valueOf(SystemModule.MD5_DESCRIPTION, SystemModule.MODULES);
//		register.register(request_description, TypeDefinition.valueOf((byte) 0, Void.class, byte[].class), null);
//		register.register(md5_description, TypeDefinition.valueOf((byte) 0, Void.class, String.class), null);
//
//		final ClientFactory factory = ClientFactory.valueOf(sessionConfig, register, coders);
//		Client client = factory.getClient(addr, false);
//
//		String md5 = getMd5(md5_description, client);
//		byte[] description = getDescription(request_description, client);
//		File f = new File("target/describe.dat");
//		if (!f.exists()) {
//			f.createNewFile();
//		}
//		FileOutputStream fos = new FileOutputStream(f);
//		fos.write(description);
//		fos.close();
//
//		System.out.println("生成协议定义文件 [" + f.getCanonicalPath() + "]");
//		System.out.println("文件大小  [" + f.length() + "] 字节");
//		System.out.println("返回 MD5 [" + md5 + "]");
//		System.out.println("文件 MD5 [" + CryptUtils.byte2hex(CryptUtils.md5(description)) + "]");
//		factory.destory();
	}

	@SuppressWarnings("unused")
	private static String getMd5(Command cmd, Client client) {
		Response<String> r = client.send(Request.valueOf(cmd, null), String.class);
		if (r.hasError()) {
			throw new RuntimeException("命令" + cmd + "状态异常" + r.getState());
		} else {
			return r.getBody();
		}
	}

	@SuppressWarnings("unused")
	private static byte[] getDescription(Command cmd, Client client) {
		Response<byte[]> r = client.send(Request.valueOf(cmd, null), byte[].class);
		if (r.hasError()) {
			throw new RuntimeException("命令" + cmd + "状态异常" + r.getState());
		} else {
			return r.getBody();
		}
	}

}
