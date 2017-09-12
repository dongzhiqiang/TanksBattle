package com.game.module.migration;

import java.util.Collection;
import java.util.HashSet;
import java.util.Iterator;
import java.util.Map;
import java.util.regex.Pattern;

import org.apache.commons.configuration.ConfigurationException;
import org.apache.commons.configuration.PropertiesConfiguration;
import org.apache.commons.lang.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.context.support.ClassPathXmlApplicationContext;

public class Merger {
	
	private static final Logger logger = LoggerFactory.getLogger(Merger.class);
	
	public static final PropertiesConfiguration JDBC_PROPERTIES;
	
	public static final PropertiesConfiguration MERGER_PROPERTIES;
	
	public static final PropertiesConfiguration SERVERS_PROPERTIES;
	
	public static final String SERVER_SEPARATOR = ".s";
	
	public static final Pattern MERGER_PATTERN = Pattern.compile("^([\\s\\S]+)(\\"+SERVER_SEPARATOR+"\\d+)$");
	
	public static String CURRENT_KEY;
	
	static {
		PropertiesConfiguration jdbcConfiguration;
		PropertiesConfiguration mergerConfiguration;
		PropertiesConfiguration serversConfiguration;
		try {
			jdbcConfiguration  = new PropertiesConfiguration("jdbc.properties");
			mergerConfiguration = new PropertiesConfiguration("merger.properties");
			serversConfiguration = new PropertiesConfiguration("database.properties");
		} catch (ConfigurationException exception) {
			throw new RuntimeException(exception);
		}
		JDBC_PROPERTIES = jdbcConfiguration;
		
		MERGER_PROPERTIES = mergerConfiguration;
		
		SERVERS_PROPERTIES = serversConfiguration;
	}

	/** 合并器的上下文配置名 */
	private static final String MERGER_CONFIG_CONTEXT = "mergerConfig.xml";
	
	/** 实体名称与转换器名称的映射 */
	private final Map<String, String> entityConverterMap;
	
	private final Map<String, Collection<String>> entityDependencyMap;
	
	public static void main(String[] arguments) throws Exception {
		final ClassPathXmlApplicationContext mergerContext = new ClassPathXmlApplicationContext(MERGER_CONFIG_CONTEXT);
		Merger merger = mergerContext.getBean(Merger.class);
		// TODO 合服需要考虑迁移序列合并的问题
		merger.merger();
	}
	
	public Merger(Map<String, String> entityConverterMap, Map<String, Collection<String>> entityDependencyMap) {
		this.entityDependencyMap = entityDependencyMap;
		this.entityConverterMap = entityConverterMap;
	}
	
	public void merger() throws Exception {
		// 让环境自动构建迁移的实体数据表
		final Iterator<String> keyIterator = SERVERS_PROPERTIES.getKeys();
		
		final Collection<String> databaseDirectories = new HashSet<String>();
		while(keyIterator.hasNext()) {
			CURRENT_KEY = keyIterator.next();
			final String oldDatabaseUrl = SERVERS_PROPERTIES.getString(CURRENT_KEY);
			if(StringUtils.isBlank(oldDatabaseUrl)) {
				logger.debug("[{}]没有对应数据库目录,无需合并", CURRENT_KEY);
				continue;
			}
			if(databaseDirectories.contains(oldDatabaseUrl)) {
				continue;
			} else {
				databaseDirectories.add(oldDatabaseUrl);
			}
			MERGER_PROPERTIES.setProperty("jdbc.url", oldDatabaseUrl);
			
			final Migrator migrator = new Migrator(MERGER_PROPERTIES, JDBC_PROPERTIES, entityConverterMap, entityDependencyMap);
			migrator.migrate();
			logger.debug("[{}]数据库合并完毕", CURRENT_KEY);
			CURRENT_KEY = null;
		}
		
	}
	
}
