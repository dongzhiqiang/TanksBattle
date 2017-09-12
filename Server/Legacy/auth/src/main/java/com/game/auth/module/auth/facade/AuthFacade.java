package com.game.auth.module.auth.facade;

import java.nio.charset.Charset;

import javax.servlet.http.HttpServletRequest;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.BeansException;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.context.ApplicationContext;
import org.springframework.context.ApplicationContextAware;
import org.springframework.http.HttpHeaders;
import org.springframework.http.HttpStatus;
import org.springframework.http.MediaType;
import org.springframework.http.ResponseEntity;
import org.springframework.stereotype.Controller;
import org.springframework.web.bind.annotation.RequestMapping;

import com.alibaba.fastjson.JSON;
import com.engine.common.socket.core.Tea;
import com.game.auth.module.auth.constant.ResultCode;
import com.game.auth.module.auth.exception.AuthException;
import com.game.auth.module.auth.model.ResultBase;
import com.game.auth.module.auth.service.AuthService;



@Controller
@RequestMapping("/server")
public class AuthFacade implements ApplicationContextAware
{
	@SuppressWarnings("unused")
	private ApplicationContext applicationContext;
	private static final Logger logger = LoggerFactory.getLogger(AuthFacade.class);
	@Autowired
	private AuthService authService;
	
	private ResponseEntity<String> resultError( HttpHeaders headers, int errCode )
	{
		StringBuilder str = new StringBuilder("");
		ResultBase result = new ResultBase();
		result.setCode(errCode);
		str.append(JSON.toJSONString(result));
		ResponseEntity<String> responseEntity = new ResponseEntity<String>(str.toString(), headers,
				HttpStatus.OK);
		return responseEntity;
	}
	
	private ResponseEntity<byte[]> resultErrorEncrypt( HttpHeaders headers, int errCode )
	{
		StringBuilder str = new StringBuilder("");
		ResultBase result = new ResultBase();
		result.setCode(errCode);
		str.append(JSON.toJSONString(result));
		byte[] retBytes = str.toString().getBytes();
		Tea.encrypt(retBytes);
		ResponseEntity<byte[]> responseEntity = new ResponseEntity<byte[]>(retBytes, headers,
				HttpStatus.OK);
		return responseEntity;
	}

	@RequestMapping("/obtainUserData")
	public ResponseEntity<String> getUserData(HttpServletRequest request)
	{
		HttpHeaders headers = new HttpHeaders();
		MediaType mediaType = new MediaType("text", "html", Charset.forName("UTF-8"));
		headers.setContentType(mediaType);

		try
		{
			return authService.getUserData(headers, request);
		}
		catch (AuthException e)
		{
			logger.error("登陆获取错误", e);
			return resultError(headers, e.getCode());
		}
		catch (Exception e)
		{
			logger.error("未知错误", e);
			return resultError(headers, ResultCode.UNKOWN_ERROR);
		}
	}
	
	/**
	 * 登陆获取服务器列表http://192.168.1.38:8086/ht_serverlist/server/login.html?uId=
	 * 22222&version=3.2.1&channel=ios
	 * 
	 * @param request
	 * @return {@link LoginResult}
	 */
	@RequestMapping("/login")
	public ResponseEntity<byte[]> login(HttpServletRequest request)
	{
		HttpHeaders headers = new HttpHeaders();
		MediaType mediaType = new MediaType("text", "html", Charset.forName("UTF-8"));
		headers.setContentType(mediaType);
		
		try
		{
			return authService.login(headers, request);
		}
		catch (AuthException e)
		{
			logger.error("登陆获取错误", e);
			return resultErrorEncrypt(headers, e.getCode());
		}
		catch (Exception e)
		{
			logger.error("未知错误", e);
			return resultErrorEncrypt(headers, ResultCode.UNKOWN_ERROR);
		}

	}


	/***
	 * 重新刷新服务器列表
	 * 
	 * @return
	 */
	@RequestMapping("/reset")
	public ResponseEntity<String> resetSeverList()
	{
		HttpHeaders headers = new HttpHeaders();
		MediaType mediaType = new MediaType("text", "html", Charset.forName("UTF-8"));
		headers.setContentType(mediaType);
		try
		{
			return authService.resetSeverList(headers);
		}
		catch (AuthException e)
		{
			logger.error("重置服务器列表出错", e);
			return resultError(headers, e.getCode());
		}
		catch (Exception e)
		{
			logger.error("未知错误", e);
			return resultError(headers, ResultCode.UNKOWN_ERROR);
		}
	}

	@RequestMapping("/setLevel")
	public ResponseEntity<String> setLevel(HttpServletRequest request)
	{
		HttpHeaders headers = new HttpHeaders();
		MediaType mediaType = new MediaType("text", "html", Charset.forName("UTF-8"));
		headers.setContentType(mediaType);

		try
		{
			return authService.setLevel(headers, request);
		}
		catch (AuthException e)
		{
			logger.error("设置新角色错误", e);
			return resultError(headers, e.getCode());
		}
		catch (Exception e)
		{

			logger.error("未知错误", e);
			return resultError(headers, ResultCode.UNKOWN_ERROR);
		}
	}

	@RequestMapping("/setRole")
	public ResponseEntity<String> setNewRole(HttpServletRequest request)
	{
		HttpHeaders headers = new HttpHeaders();
		MediaType mediaType = new MediaType("text", "html", Charset.forName("UTF-8"));
		headers.setContentType(mediaType);

		try
		{
			return authService.setNewRole(headers, request);
		}
		catch (AuthException e)
		{
			logger.error("设置新角色错误", e);
			return resultError(headers, e.getCode());
		}
		catch (Exception e)
		{
			logger.error("未知错误", e);
			return resultError(headers, ResultCode.UNKOWN_ERROR);
		}
	}

	/**
	 * 更新服务器在线人数
	 * 
	 * @param request
	 * @return
	 */
	@RequestMapping("/onlineNums")
	public ResponseEntity<String> updateSeverOnlineNums(HttpServletRequest request)
	{
		HttpHeaders headers = new HttpHeaders();
		MediaType mediaType = new MediaType("text", "html", Charset.forName("UTF-8"));
		headers.setContentType(mediaType);

		try
		{
			return authService.updateSeverOnlineNums(headers, request);
		}
		catch (AuthException e)
		{
			logger.error("更新服务器在线人数错误", e);
			return resultError(headers, e.getCode());
		}
		catch (Exception e)
		{
			logger.error("未知错误", e);
			return resultError(headers, ResultCode.UNKOWN_ERROR);
		}
	}

	@Override
	public void setApplicationContext(ApplicationContext arg0) throws BeansException
	{
		applicationContext = arg0;

	}


}
