package com.game.gow.module.system.manager;

import java.util.HashMap;
import java.util.Map;
import java.util.Timer;
import java.util.TimerTask;

import javax.annotation.PostConstruct;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Component;

import com.engine.common.utils.json.JsonUtils;
import com.game.gow.utils.HttpUtil;

@Component
public class GlobalServerManager {

	private static final Logger logger = LoggerFactory.getLogger(GlobalServerManager.class);
	private static final int TIMER_INV = 30 * 1000; 
	
	@Value("${server.globalserver.url}")
	private String globalServerUrl;
	@Value("${server.globalserver.key}")
	private String globalServerKey;
	@Value("${server.config.area}")
	private String serverConfigArea;
	@Value("${server.config.name}")
	private String serverConfigName;
	@Value("${server.config.index}")
	private int serverConfigIndex;
	@Value("${server.config.host}")
	private String serverConfigHost;
	@Value("${server.socket.address}")
	private String serverConfigPort;
	@Value("${server.config.servers}")
	private String serverConfigIds;
	@Value("${server.config.showState}")
	private String serverConfigShowState;
	
	private Timer timer = null;
	
	protected class OnTimerTask extends TimerTask
	{
		@Override
		public void run() {
			notifyServerAlive();
		}
	}
	
	public GlobalServerManager()
	{
	}
	
	@PostConstruct
	protected void init() {
		//启动定时器，定时登记服务器存活
	    timer = new Timer(true);
        timer.schedule(new OnTimerTask(), 0, TIMER_INV);
	}
	
	protected void notifyServerAlive()
	{
		try
		{
			Map<String, Object> reqMap = new HashMap<String, Object>();
			reqMap.put("area", serverConfigArea);
			reqMap.put("name", serverConfigName);
			reqMap.put("index", serverConfigIndex);
			reqMap.put("host", serverConfigHost);
			reqMap.put("port", Integer.valueOf(serverConfigPort.split(":")[1]));
			reqMap.put("serverId", Integer.valueOf(serverConfigIds.split(",")[0]));
			reqMap.put("onlineNum", 0);
			reqMap.put("showState", serverConfigShowState);
			String req = JsonUtils.map2String(reqMap);
			String res = HttpUtil.doPost(HttpUtil.joinPath(globalServerUrl, "notifySvrAlive?key=" + HttpUtil.urlEncode(globalServerKey)), req);
			//http失败？那不让登录
			if (res == null)
				return;
			Map<String, Object> resMap = JsonUtils.string2Map(res);
		 	int code = (Integer)resMap.get("code");
		 	String msg = (String)resMap.get("msg");
		 	if (code != 0)
		 	{
		 		logger.debug("通知服务器存活得到失败的结果，消息：" + msg);
		 	}
		}
		catch (Exception e)
		{
			logger.error("通知服务器存活时异常", e);		
		}
	}
	
	public boolean updateRoleInfo(String channelId, String userId, String name, int level, long playerId, int serverId)
	{
		try
		{
			Map<String, Object> reqMap = new HashMap<String, Object>();
			reqMap.put("channelId", channelId);
			reqMap.put("userId", userId);
			reqMap.put("name", name);
			reqMap.put("level", level);
			reqMap.put("roleOId", playerId);
			reqMap.put("serverId", serverId);
			String req = JsonUtils.map2String(reqMap);
			String res = HttpUtil.doPost(HttpUtil.joinPath(globalServerUrl, "updateRole?key=" + HttpUtil.urlEncode(globalServerKey)), req);
			//http失败？那不让登录
			if (res == null)
				return false;
			Map<String, Object> resMap = JsonUtils.string2Map(res);
		 	int code = (Integer)resMap.get("code");
		 	String msg = (String)resMap.get("msg");
		 	if (code != 0)
		 	{
		 		logger.debug("通知角色信息得到失败的结果，消息：" + msg);
		 		return false; 
		 	}
		}
		catch (Exception e)
		{
			logger.error("通知角色信息时发生异常", e);
			return false;
		}
		return true;
	}
	
	public boolean checkLogin(String channelId, String userId, String token)
	{
		try
		{
			Map<String, Object> reqMap = new HashMap<String, Object>();
			reqMap.put("channelId", channelId);
			reqMap.put("userId", userId);
			reqMap.put("token", token);
			String req = JsonUtils.map2String(reqMap);
			String res = HttpUtil.doPost(HttpUtil.joinPath(globalServerUrl, "checkLogin?key=" + HttpUtil.urlEncode(globalServerKey)), req);
			//http失败？那不让登录
			if (res == null)
				return false;
			Map<String, Object> resMap = JsonUtils.string2Map(res);
		 	int code = (Integer)resMap.get("code");
		 	String msg = (String)resMap.get("msg");
		 	if (code != 0)
		 	{
		 		logger.debug("检查登录得到失败的结果，消息：" + msg);
		 		return false; 
		 	}
		}
		catch (Exception e)
		{
			logger.error("检查登录时发生异常", e);
			return false;
		}
		return true;
	}
}
