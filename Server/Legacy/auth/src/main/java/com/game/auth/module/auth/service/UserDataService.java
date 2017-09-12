package com.game.auth.module.auth.service;

import java.io.InputStream;
import java.net.URL;
import java.net.URLConnection;
import java.nio.charset.Charset;
import java.util.Date;
import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.ConcurrentMap;
import java.util.concurrent.DelayQueue;

import javax.annotation.PostConstruct;

import org.apache.commons.httpclient.HttpClient;
import org.apache.commons.httpclient.methods.GetMethod;
import org.apache.commons.lang.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Service;

import com.engine.common.scheduler.Scheduled;
import com.engine.common.utils.concurrent.DelayedElement;
import com.engine.common.utils.json.JsonUtils;
import com.engine.common.utils.time.DateUtils;
import com.game.auth.module.auth.constant.ResultCode;
import com.game.auth.module.auth.exception.AuthException;
import com.game.auth.module.auth.model.BasePassportData;
import com.game.auth.module.auth.model.UserData;


@Service
public class UserDataService
{
	private static final Logger logger = LoggerFactory.getLogger(UserDataService.class);

	@Value("${server.gm.player.url}")
	private String gmPlayersUrl;

	private String gmPlayers;
	
	private String[] gms;

	private static final String SDK_CHECK_URL = "http://123.58.175.237:5400/sauth?";

	private static DelayQueue<DelayedElement<UserData>> cache = new DelayQueue<>();

	private static ConcurrentMap<String, UserData> cacheObjMap = new ConcurrentHashMap<String, UserData>();

	@Scheduled(name = "清理过期数据", value = "40 0/5 * * * * ")
	private void clearDelayCache()
	{
		try{
			for (;;)
			{
				try{
					DelayedElement<UserData> e = cache.poll();
					if (e == null)
					{
						break;
					}
					UserData ud = e.getContent();
					String userId = ud.getUserId();
					UserData curUd = cacheObjMap.get(userId);
					if(curUd == null){
						cacheObjMap.remove(userId);
					}else{
						if(curUd == ud){ //这里必须是==,不能用equals
							cacheObjMap.remove(userId);
						}
					}
				}catch(Exception e1){
					logger.error("定时清理过期数据错误1:" + e1.getMessage());
				}
			}
			logger.info("延迟队列大小:" + cache.size() + "; 数据缓存大小:" + cacheObjMap.size());
		}catch(Exception e2){
			
		}
	}


	public UserData getUserData(String uid)
	{

		UserData ud = cacheObjMap.get(uid);

		return ud;

	}
	


	public String checkSdk(BasePassportData passportData)
	{

		String platform1 = passportData.getPlatform();
		if (platform1 != null && !platform1.equals("other"))
		{

			StringBuffer url = new StringBuffer();

			url.append(SDK_CHECK_URL);
			// =====================所有的参数=============================
			String gameid = "ma11";
			// String hostid = passportData.getServerId() == null ?
			// "":passportData.getServerId();
			String token = passportData.getToken();
			String login_channel = passportData.getChannel();
			String app_channel = passportData.getApp_channel();
			String platform = platform1;
			String ip = passportData.getIp();
			String username = passportData.getUserId() + "@" + passportData.getPlatform() + "."
					+ login_channel + ".win.163.com";
			passportData.setAccount_id(username);
			String udid = passportData.getUdid();
			String sessionid = passportData.getToken(); // edg的sessionid为sign值，sign
														// = MD5(userId + "&" +
														// timestamp + "&" +
														// AppKey)

			String sdk_version = passportData.getSdk_version();
			String deviceid = "0"; // 可选 对应手机硬件id 目前以下渠道需要添加：netease（网易sdk）
									// jinliu_sdk
			String channel_gameid = ""; // 可选 渠道gamdid 目前以下渠道需要添加：ledou
			String userid = ""; // 可选 目前以下渠道需要添加：xunlei
			String anonymous = ""; // 可选 匿名登录,字符串类型不为空表示匿名登录
									// 目前以下渠道需要添加：jinliu_sdk
			long timestamp = 0; // 可选 时间戳 登录时间戳 目前以下渠道需要添加：efun_sdk edg
								// feiliu_sdk
			int cpid = 0; // 开发商id（游戏开发者在渠道对应的id），目前meizu_sdk渠道version=2时，增加参数cpid=1
			// ======================所有的参数============================
			// ======================参数修改===============================
			// 不同登陆渠道 参数可能不一样
			switch (login_channel)
			{
				case "netease":
					deviceid = passportData.getDevId();
					break;
				default:
					break;
			}
			// ======================参数修改===============================
			url.append("gameid=").append(gameid);
			url.append("&hostid=").append("0");
			url.append("&channel=").append(login_channel);
			url.append("&app_channel=").append(app_channel);
			url.append("&platform=").append(platform);
			url.append("&ip=").append(ip);
			url.append("&username=").append(username);
			url.append("&udid=").append(udid);
			url.append("&sessionid=").append(sessionid);
			url.append("&sdk_version=").append(sdk_version);
			url.append("&deviceid=").append(deviceid);
			url.append("&channel_gameid=").append(channel_gameid);
			url.append("&userid=").append(userid);
			url.append("&anonymous=").append(anonymous);
			url.append("&timestamp=").append(timestamp);
			url.append("&cpid=").append(cpid);
			try
			{
				HttpClient httpClient = new HttpClient();
				httpClient.getHttpConnectionManager().getParams().setConnectionTimeout(3000);
				GetMethod method = new GetMethod(url.toString());

				String result = "";
				try
				{
					int statusCode = httpClient.executeMethod(method);
					logger.debug("URL = " + url.toString());
					if (statusCode != 200)
					{

						logger.error("Method failed code=" + statusCode + ": " + method.getStatusLine());
						throw new AuthException("账号验证失败", ResultCode.USER_CHECK_ERROR);
					}
					else
					{

						result = new String(method.getResponseBody(), "utf-8");
						Map<String, Object> resultMap = JsonUtils.string2Map(result);
						int code = (int) resultMap.get("code");
						if (code != 200)
						{
							logger.error("验证账号失败错误码=" + result);
							throw new AuthException("账号验证失败1", ResultCode.USER_CHECK_ERROR);
						}
						else
						{
							int aid = (int) resultMap.get("aid");
							UserData userdata = new UserData();
							userdata.setAid(aid + "");
							String userNameResult = (String) resultMap.get("username");
							String usernameSplit[] = userNameResult.split("\\@");
							String userId = usernameSplit[0];
							userdata.setUserId(userId);
							username = userId + "@" + passportData.getPlatform() + "." + login_channel
									+ ".win.163.com";
							userdata.setAccount_id(username);
							userdata.setToken(token);
							DelayedElement<UserData> dp = DelayedElement.valueOf(userdata,
									DateUtils.addTime(new Date(), 0, 10, 0));
							cache.add(dp);
							cacheObjMap.put(dp.getContent().getUserId(), dp.getContent());
							logger.debug("验证成功 userId = " + userId);
							// LoginResult role = playService.login(
							// loginCache.getSession(),
							// loginCache.getServerId(),
							// passportData.getPassport(), passportData);
							// resultCont = Result.SUCCESS(role);
							return userId;
						}

					}
				}
				finally
				{
					method.releaseConnection();
				}
			}
			catch (Exception e)
			{
				logger.error("向SDK server验证玩家身份出错。\n" + e.getMessage());
				throw new AuthException("账号验证失败2", ResultCode.USER_CHECK_ERROR);
			}

		}
		return "";
	}

	@PostConstruct
	public void init()
	{
		try
		{
			URL url = new URL(gmPlayersUrl);
			URLConnection uconn = url.openConnection();
			InputStream in = uconn.getInputStream();
			byte[] bytes = new byte[20240];
			int readNums = in.read(bytes);
			if (readNums > 0)
			{
				gmPlayers = new String(bytes, Charset.forName("UTF-8"));
			}
			in.close();
			
			if (StringUtils.isNotBlank(gmPlayers))
			{
				gms = gmPlayers.split(",");
			}
			else
			{
				gms = null;
			}
		}
		catch (Exception e)
		{
			throw new RuntimeException(e);
		}
		// final String tmpUrl = url;
		Thread thread = new Thread(new Runnable()
		{

			@Override
			public void run()
			{
				// upSystemInfo(tmpUrl);
			}
		});
		thread.start();
	}
	

	public boolean isGm(String userId)
	{
		if( gms == null )
		{
			return false;
		}
		for (String strg : gms)
		{

			if (strg.equals(userId))
			{
				return true;
			}
		}
		return false;
	}






}
