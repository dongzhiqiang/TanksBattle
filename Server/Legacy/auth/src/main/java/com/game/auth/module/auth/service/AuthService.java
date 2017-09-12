package com.game.auth.module.auth.service;

import java.io.InputStream;
import java.io.UnsupportedEncodingException;
import java.net.URL;
import java.net.URLConnection;
import java.net.URLDecoder;
import java.nio.charset.Charset;
import java.util.List;

import javax.annotation.PostConstruct;
import javax.servlet.http.HttpServletRequest;

import org.apache.commons.lang.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.http.HttpHeaders;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.stereotype.Service;

import com.alibaba.fastjson.JSON;
import com.engine.common.socket.core.Tea;
import com.engine.common.utils.codec.CryptUtils;
import com.engine.common.utils.json.JsonUtils;
import com.game.auth.module.auth.constant.ResultCode;
import com.game.auth.module.auth.exception.AuthException;
import com.game.auth.module.auth.manager.LoginUser;
import com.game.auth.module.auth.manager.LoginUserManager;
import com.game.auth.module.auth.manager.Role;
import com.game.auth.module.auth.model.BasePassportData;
import com.game.auth.module.auth.model.LoginResult;
import com.game.auth.module.auth.model.PassportData;
import com.game.auth.module.auth.model.ResultBase;
import com.game.auth.module.auth.model.ServerHost;
import com.game.auth.module.auth.model.UserData;
import com.game.auth.module.auth.model.UserDataResult;


@Service
public class AuthService
{
	private static final Logger logger = LoggerFactory.getLogger(AuthService.class);

	@Autowired
	private UserDataService userDataService;
	@Autowired
	private ServerListService serverListService;
	@Autowired
	private LoginUserManager loginUserManger;

	@Value("${server.version.url}")
	private String serverVersionUrl;
	private String serverVersion;

	@Value("${server.ip}")
	private String serverIp;
	
	@Value("${server.config.key}")
	private String key;

	public ResponseEntity<String> getUserData(HttpHeaders headers, HttpServletRequest request)
	{

		String uid = request.getParameter("uid");

		StringBuilder str = new StringBuilder();
		UserDataResult result = new UserDataResult();
		UserData ud = userDataService.getUserData(uid);

		if (ud == null)
		{
			logger.debug("缓存数据为空，可能过时或者前面验证出错uId=" + uid);
			result.setCode(ResultCode.CHECK_DATA_TIMEOUT);
			str.append(JSON.toJSONString(result));
			ResponseEntity<String> responseEntity = new ResponseEntity<String>(str.toString(), headers,
					HttpStatus.OK);
			return responseEntity;
		}

		result.setUd(ud);
		str.append(JsonUtils.object2String(result));
		logger.debug("获取缓存数据success");
		ResponseEntity<String> responseEntity = new ResponseEntity<String>(str.toString(), headers,
				HttpStatus.OK);

		return responseEntity;

	}
	
	@PostConstruct
	public void init()
	{

		try
		{

			URL url1 = new URL(serverVersionUrl);
			URLConnection uconn1 = url1.openConnection();
			InputStream in1 = uconn1.getInputStream();
			byte[] bytes1 = new byte[128];
			int readNums1 = in1.read(bytes1);
			if (readNums1 > 0)
			{
				serverVersion = new String(bytes1, Charset.forName("UTF-8"));
			}
			in1.close();

		}
		catch (Exception e)
		{
			throw new RuntimeException(e);
		}

	}
	
	/**
	 * 登陆获取服务器列表http://192.168.1.38:8086/ht_serverlist/server/login.html?uId=
	 * 22222&version=3.2.1&channel=ios
	 * 
	 * @param request
	 * @return {@link LoginResult}
	 */
	public ResponseEntity<byte[]> login(HttpHeaders headers, HttpServletRequest request)
	{
		String userStr = request.getParameter("user");
		String version = request.getParameter("version");
		String channel = request.getParameter("channel");

		BasePassportData userData = null;
		String userId = "";
		String oldUserId = "";
		PassportData pd = null;
		try
		{
			userData = JsonUtils.string2Object(userStr, PassportData.class);

			pd = ((PassportData) userData);

			logger.debug("request params = " + userStr);
			userId = pd.getUserId();
			if (StringUtils.isNotBlank(pd.getOldUid()))
			{
				oldUserId = pd.getOldUid();
			}
			else
			{
				oldUserId = pd.getOldUid();
			}
		}
		catch (Exception e)
		{
			userData = JsonUtils.string2Object(userStr, BasePassportData.class);
		}

		String userIdStr = userDataService.checkSdk(userData);

		if (StringUtils.isNotBlank(userIdStr))
		{
			userId = userIdStr;
		}

		String[] vers = version.split("\\.");
		String[] serverVers = serverVersion.split("\\.");

		if (version.equals("0.0.0"))
		{

		}
		else if (Integer.valueOf(vers[0]) > Integer.valueOf(serverVers[0]))
		{
			if (channel.equals("ios"))
			{
				channel = "iosreview";
			}
			else
			{
				channel = "neteasereview";
			}
		}
		else if (Integer.valueOf(vers[0]) == Integer.valueOf(serverVers[0]))
		{
			if (Integer.valueOf(vers[1]) > Integer.valueOf(serverVers[1]))
			{
				if (channel.equals("ios"))
				{
					channel = "iosreview";
				}
				else
				{
					channel = "neteasereview";
				}
			}

		}
		else
		{
			throw new AuthException("版本错误", ResultCode.VERSION_ERROR);
		}

		LoginUser user = null;
		LoginUser oldUser = null;
		/*
		 * if (StringUtils.isNotBlank(userId)) { user =
		 * queryByUserId(userId); // LoginUser user = null; if (user ==
		 * null) { user = insert(userId); } }
		 */
		
		if (StringUtils.isNotBlank(userId)){
			user = loginUserManger.queryByUserId(userId);
			if (user == null)
			{
				user = loginUserManger.insert(userId);
				//玩家绑定新的账号首次时，需要检查并复制olduid的记录信息到新账号
				if (StringUtils.isNotBlank(oldUserId) && !userId.equals(oldUserId)){
					oldUser = loginUserManger.queryByUserId(oldUserId);
				}
				if(oldUser != null){
					user.setPreRole(oldUser.getPreRole());
					user.setRoles(oldUser.getRoles());
					loginUserManger.update(user);
				}
			}
		}

		boolean flag = userDataService.isGm(userId);

		List<ServerHost> list = serverListService.getServerList(version, flag, channel);

		StringBuilder str = new StringBuilder("");
		long timestamp = System.currentTimeMillis();
		ResultBase result = LoginResult.valueOf(list, user, flag, serverIp, userId,timestamp, makeLoginKey(userId,timestamp));

		str.append(JsonUtils.object2String(result));
		byte[] retBytes = null;
		try {
			retBytes = str.toString().getBytes("UTF-8");
		} catch (UnsupportedEncodingException e) {
			throw new AuthException("未知错误", ResultCode.UNKOWN_ERROR);
		}
		Tea.encrypt(retBytes);
		ResponseEntity<byte[]> responseEntity = new ResponseEntity<byte[]>(retBytes, headers,
				HttpStatus.OK);

		return responseEntity;
	}
	
	private String makeLoginKey(String accountName,long timestamp) {
		
		String baseString = accountName + timestamp + key;
		try {
			String md5 = CryptUtils.md5(baseString).toLowerCase();
			return md5;
		} catch (Exception e) {
			logger.debug("生成登录密匙时发生异常", e);
			return null;
		}

	}

	/***
	 * 重新刷新服务器列表
	 * 
	 * @return
	 */
	public ResponseEntity<String> resetSeverList(HttpHeaders headers)
	{
		serverListService.resetSeverList();
		
		ResultBase result = new ResultBase();
		StringBuilder str = new StringBuilder();
		str.append(JSON.toJSONString(result));
		ResponseEntity<String> responseEntity = new ResponseEntity<String>(str.toString(), headers,
				HttpStatus.OK);
		return responseEntity;
	}

	public ResponseEntity<String> setLevel(HttpHeaders headers, HttpServletRequest request)
	{

		String uid = request.getParameter("uid");
		String serverId = request.getParameter("serverId");
		String level = request.getParameter("level");

		// dueNums.incrementAndGet();
		ResultBase result = new ResultBase();
		LoginUser user = loginUserManger.queryByUserId(uid);
		StringBuilder str = new StringBuilder();
		if (user == null)
		{

			result.setCode(ResultCode.USER_NOT_EXIST);
			str.append(JSON.toJSONString(result));

		}
		else
		{

			Role role = new Role();
			role.setServerId(Integer.valueOf(serverId));

			List<Role> roles = user.getRoles();

			if (roles.contains(role))
			{

				role = roles.get(roles.indexOf(role));
				role.setLevel(Integer.valueOf(level));
				loginUserManger.update(user);
			}
			str.append(JSON.toJSONString(result));
		}

		ResponseEntity<String> responseEntity = new ResponseEntity<String>(str.toString(), headers,
				HttpStatus.OK);

		return responseEntity;
	}

	public ResponseEntity<String> setNewRole(HttpHeaders headers, HttpServletRequest request)
	{

		String uid = request.getParameter("uId");
		String serverId = request.getParameter("serverId");
		String name = request.getParameter("name");
		String level = request.getParameter("level");

		if (name != null)
		{
			try {
				name = URLDecoder.decode(name, "utf-8");
			} catch (UnsupportedEncodingException e) {
				throw new AuthException("未知错误", ResultCode.UNKOWN_ERROR);
			}
		}
		ResultBase result = new ResultBase();
		LoginUser user = loginUserManger.queryByUserId(uid);
		StringBuilder str = new StringBuilder();
		if (user == null)
		{

			result.setCode(ResultCode.USER_NOT_EXIST);
			str.append(JSON.toJSONString(result));
			logger.debug("玩家不存在uid=" + uid);
		}
		else
		{

			Role role = new Role();
			role.setServerId(Integer.valueOf(serverId));
			role.setLevel(Integer.valueOf(level));
			role.setName(name);
			List<Role> roles = user.getRoles();
			if (roles.contains(role))
			{
				roles.remove(roles.indexOf(role));
			}
			user.setPreRole(role);
			roles.add(role);
			loginUserManger.update(user);
			str.append(JSON.toJSONString(result));
		}

		ResponseEntity<String> responseEntity = new ResponseEntity<String>(str.toString(), headers,
				HttpStatus.OK);

		return responseEntity;

	}


	/**
	 * 更新服务器在线人数
	 * 
	 * @param request
	 * @return
	 */
	public ResponseEntity<String> updateSeverOnlineNums(HttpHeaders headers, HttpServletRequest request)
	{
		String serverId = request.getParameter("serverId");
		String nums = request.getParameter("nums");

		serverListService.updateSeverOnlineNums(serverId, nums);

		StringBuilder str = new StringBuilder("success");
		ResponseEntity<String> responseEntity = new ResponseEntity<String>(str.toString(), headers,
				HttpStatus.OK);
		return responseEntity;
	}


}
