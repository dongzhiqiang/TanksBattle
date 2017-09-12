package com.game.gow.utils;

import java.io.ByteArrayOutputStream;
import java.io.InputStream;
import java.io.OutputStream;
import java.io.UnsupportedEncodingException;
import java.net.HttpURLConnection;
import java.net.URL;
import java.net.URLEncoder;
import java.security.SecureRandom;
import java.security.cert.CertificateException;
import java.security.cert.X509Certificate;

import javax.net.ssl.HostnameVerifier;
import javax.net.ssl.HttpsURLConnection;
import javax.net.ssl.SSLContext;
import javax.net.ssl.SSLSession;
import javax.net.ssl.TrustManager;
import javax.net.ssl.X509TrustManager;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import com.game.gow.module.account.facade.AccountFacadeImpl;

public class HttpUtil
{
    private static final Logger logger = LoggerFactory.getLogger(HttpUtil.class);
    
    ////////////////////////////////////
    private static SSLContext msSSLCxt = null;
    
    private static TrustManager[] msTrustAllCerts = new TrustManager[]
    {
        new X509TrustManager()
        {
            @Override
            public X509Certificate[] getAcceptedIssuers() {return null;}
            @Override
            public void checkClientTrusted(X509Certificate[] chain, String authType) throws CertificateException {}
            @Override
            public void checkServerTrusted(X509Certificate[] chain, String authType) throws CertificateException {}
        }
    };
    
    private static HostnameVerifier msHostVerifyier = new HostnameVerifier()            
    {
        @Override
        public boolean verify(String hostname, SSLSession session) {return true;}
    };    
    ////////////////////////////////////

    private static void tryEnableSSL(HttpURLConnection conn)
    {
        if (conn != null && conn instanceof HttpsURLConnection)
        {
            if (null == msSSLCxt)
                init();
            HttpsURLConnection conn2 = (HttpsURLConnection)conn;
            conn2.setSSLSocketFactory(msSSLCxt.getSocketFactory());
            conn2.setHostnameVerifier(msHostVerifyier);
        }
    }

    ////////////////////////////////////
    public static synchronized void init()
    {
        if (null != msSSLCxt)
            return;

        try
        {
            SSLContext cxt = SSLContext.getInstance("TLS");
            cxt.init(null, msTrustAllCerts, new SecureRandom());
            msSSLCxt = cxt;
        }
        catch (Exception e)
        {
        	logger.error(e.getMessage());
        }
    }
    
    public static String urlEncode(String str)
    {
        try
        {
            return URLEncoder.encode(str, "utf8");
        }
        catch (UnsupportedEncodingException e)
        {
        	logger.error(e.getMessage());
            return "";
        }
    }

    public static String doGet(String urlstr)
    {
        return doGet(urlstr, "utf8");
    }
    
    public static String doGet(String urlstr, String respEncode)
    {
    	HttpURLConnection conn = null;
        try
        {
            URL url = new URL(urlstr);
            conn = (HttpURLConnection)url.openConnection();

            tryEnableSSL(conn);

            conn.setUseCaches(false);
            conn.setRequestMethod("GET");
            conn.setInstanceFollowRedirects(true);
            conn.connect();

            InputStream is = conn.getInputStream();
            ByteArrayOutputStream bytebuff = new ByteArrayOutputStream();            
            byte[] buf = new byte[1024];
            int cnt = 0;
            while ((cnt = is.read(buf)) >= 0)
            {
                bytebuff.write(buf, 0, cnt);
            }

            bytebuff.close();
            is.close();
            conn.disconnect();
            
            String retstr = bytebuff.toString(respEncode);
            
            logger.debug(retstr);

            return retstr;
        }
        catch (Exception e)
        {
        	try
        	{
        		if (conn != null)
        			conn.disconnect();
        	}
        	catch (Exception e1)
        	{
        	}
        	
            logger.error(e.getMessage());
            return null;
        }
    }
    
    public static String doPost(String urlstr, String param)
    {
        return doPost(urlstr, param, "utf8");
    }

    public static String doPost(String urlstr, String param, String respEncode)
    {
    	HttpURLConnection conn = null;
        try
        {
            byte[] bytes = param.getBytes();
            
            URL url = new URL(urlstr);
            conn = (HttpURLConnection)url.openConnection();

            tryEnableSSL(conn);

            conn.setDoOutput(true);
            conn.setDoInput(true);
            conn.setUseCaches(false);
            conn.setRequestMethod("POST");
            conn.setInstanceFollowRedirects(true);
            conn.setRequestProperty("Content-Length", String.valueOf(bytes.length));
            conn.connect();

            OutputStream os = conn.getOutputStream();
            os.write(bytes);
            os.flush();
            os.close();
            
            InputStream is = conn.getInputStream();
            ByteArrayOutputStream bytebuff = new ByteArrayOutputStream();            
            byte[] buf = new byte[1024];
            int cnt = 0;
            while ((cnt = is.read(buf)) >= 0)
            {
                bytebuff.write(buf, 0, cnt);             
            }

            bytebuff.close();
            is.close();
            conn.disconnect();
            
            String retstr = bytebuff.toString(respEncode);

            logger.debug(retstr);

            return retstr;
        }
        catch (Exception e)
        {
        	try
        	{
        		if (conn != null)
        			conn.disconnect();
        	}
        	catch (Exception e1)
        	{
        	}
        	
        	logger.error(e.getMessage());
            return null;
        }
    }
    
    public static String joinPath(String a, String b)
    {
        boolean b1 = a.endsWith("/");
        boolean b2 = b.endsWith("/");
        if (b1 && b2)
            return a.substring(0, a.length() - 1) + b;
        else if (!b1 && !b2)
            return a + '/' + b;
        else
            return a + b;
    }
}