package com.game.auth.module.auth.service;

import java.util.ArrayList;
import java.util.List;
import javax.annotation.PostConstruct;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Component;
import com.game.auth.module.auth.constant.ResultCode;
import com.game.auth.module.auth.exception.AuthException;
import com.game.auth.module.auth.model.ServerHost;
import com.game.auth.utils.CsvResourceUtil;


@Component
public class ServerListService
{
	@SuppressWarnings("unused")
	private static final Logger logger = LoggerFactory.getLogger(ServerListService.class);

	//
	private List<ServerHost> serverHosrList = null;

	@Value("${server.config.url}")
	private String configUrl;


	public void resetSeverList()
	{
		try
		{
			serverHosrList = CsvResourceUtil.getJsonResource(ServerHost.class, configUrl);
			maskLanInfo();
		}
		catch (Exception e)
		{
			throw new AuthException("获取服务器列表错误", ResultCode.UNKOWN_ERROR);
		}
	}

	
	/**
	 * 掩盖服务器列表中与内网IP端口
	 */
	private void maskLanInfo(){
		if(serverHosrList != null){
			for (ServerHost serverHost : serverHosrList)
			{
				serverHost.setLanIp("");
				serverHost.setLanPort(0);
			}
		}
	}

	@PostConstruct
	public void init()
	{
		try
		{
			resetSeverList();
		}
		catch (Exception e)
		{
			throw new RuntimeException(e);
		}
	}
	

	public List<ServerHost> getServerList(String version, boolean flag, String channel)
	{
		List<ServerHost> list = new ArrayList<ServerHost>();

		if (version.equals("0.0.0"))
		{
			list = serverHosrList;
		}
		else if (flag)
		{
			list = serverHosrList;
		}
		else
		{
			for (ServerHost serverHost : serverHosrList)
			{
				if (serverHost.getChannel().equals(channel))
				{
					list.add(serverHost);
				}

			}
		}

		return list;
	}


	public void updateSeverOnlineNums(String serverId, String nums)
	{
		List<ServerHost> list = serverHosrList;

		for (ServerHost serverHost : list)
		{
			if (serverHost.getServerId() == Integer.valueOf(serverId).intValue())
			{
				serverHost.setOnlineNums(Integer.valueOf(nums));
				break;
			}
		}
	}


}
