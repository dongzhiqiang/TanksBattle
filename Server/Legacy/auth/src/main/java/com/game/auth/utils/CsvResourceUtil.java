package com.game.auth.utils;

import java.io.IOException;
import java.io.InputStream;
import java.lang.reflect.Constructor;
import java.lang.reflect.Field;
import java.net.URL;
import java.net.URLConnection;
import java.nio.charset.Charset;
import java.util.ArrayList;
import java.util.List;

import org.apache.commons.lang.StringUtils;

import com.alibaba.fastjson.JSON;
import com.csvreader.CsvReader;

public class CsvResourceUtil
{

	//"http://update.lm.163.com/serverHostList.csv"
	public static <T> List<T> getJsonResource(Class<T> clzz, String urlStr) throws Exception
	{

		List<T> list = new ArrayList<T>();
		
		
	
	

		

		
		CsvReader read = null;
		try
		{

			URL url = new URL(urlStr);
			URLConnection uconn = url.openConnection();
			uconn.setConnectTimeout(1000*30);
			InputStream is = uconn.getInputStream();
			read = new CsvReader(is, ',', Charset.forName("UTF-8"));
			read.readHeaders();
			// 逐条读取记录，直至读完
			while (read.readRecord())
			{
				Constructor<T> defualtConstruct = clzz.getConstructor();
				T obj = defualtConstruct.newInstance();

				Field[] fields = clzz.getDeclaredFields();
				read.getRawRecord();
				for (Field field : fields)
				{
					field.setAccessible(true);

					String value = read.get(field.getName());

					if (StringUtils.isNotBlank(value))
					{
						if (!field.getType().isAssignableFrom(String.class))
						{
							field.set(obj, JSON.parseObject(value, field.getType()));
						}
						else
						{
							field.set(obj, value);
						}

					}

				}
				list.add(obj);

				// // 读取一条记录
				// );
				// // 按列名读取这条记录的值
				// System.out.println(read.get("Name"));
				// System.out.println(read.get("class"));
				// System.out.println(read.get("number"));
				// System.out.println(read.get("sex"));
			}
			read.close();
		}
		catch (IOException e)
		{
			e.printStackTrace();
		}
		finally
		{
			if (read != null)
			{
				read.close();
			}
		}

		return list;

	}
	
	public static <T> List<T> getJsonResourceFromFile(Class<T> clzz, String fileName) throws Exception
	{


		List<T> list = new ArrayList<T>();
		CsvReader read = null;
		try
		{
			read = new CsvReader(clzz.getResource("/").getPath() + fileName, ',', Charset.forName("UTF-8"));
			read.readHeaders();
			// 逐条读取记录，直至读完
			while (read.readRecord())
			{

				Constructor<T> defualtConstruct = clzz.getConstructor();
				T obj = defualtConstruct.newInstance();

				Field[] fields = clzz.getDeclaredFields();
				read.getRawRecord();
				for (Field field : fields)
				{
					field.setAccessible(true);

					String value = read.get(field.getName());

					if (StringUtils.isNotBlank(value))
					{
						if (!field.getType().isAssignableFrom(String.class))
						{
							field.set(obj, JSON.parseObject(value, field.getType()));
						}
						else
						{
							field.set(obj, value);
						}

					}

				}
				list.add(obj);

				// // 读取一条记录
				// );
				// // 按列名读取这条记录的值
				// System.out.println(read.get("Name"));
				// System.out.println(read.get("class"));
				// System.out.println(read.get("number"));
				// System.out.println(read.get("sex"));
			}
			read.close();
		}
		catch (IOException e)
		{
			e.printStackTrace();
		}
		finally
		{
			if (read != null)
			{
				read.close();
			}
		}

		return list;

	}
}
