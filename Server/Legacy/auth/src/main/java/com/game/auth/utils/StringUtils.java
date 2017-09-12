package com.game.auth.utils;

import java.util.regex.Matcher;
import java.util.regex.Pattern;

/**
 * string过滤工具
 * 
 * @author LJJ
 */
public final class StringUtils extends org.apache.commons.lang.StringUtils
{

	private StringUtils()
	{
		throw new IllegalAccessError("该类不允许实例化");
	}

	/**
	 * 过滤掉字符串中特殊字符，只允许字母,数字和中文(繁体字)
	 * 
	 * @param content
	 *            :需要过滤掉的字符串
	 * @return
	 */
	public static String getContentAfterFilterSpecialChar(String content)
	{
		String regEx = "[^0-9a-zA-Z\u4e00-\u9fbf]+";
		return content.replaceAll(regEx, "");
	}
	
	public static boolean haveIllegalChar(String content){
		//\u0000-\u002F 从开始到0030之前，即数字0之前
		String regEx = "[\u0000-\u002F]";
		if(matcherByRegex(regEx,content)){
			System.out.println("段1");
			return true;
		}
		//\u003A-\u0040 从数字9之后，到字符串A之前
		regEx = "[\u003A-\u0040]";
		if(matcherByRegex(regEx,content)){
			System.out.println("段2");
			return true;
		}
		//\u005B-\u0060 从字符串Z之后，到字符串a之前
		//\u007B-\u00FF 从字符串z之后，到单字节所有 
		regEx = "[\\[\\]/^ _`]";
		if(matcherByRegex(regEx,content)){
			System.out.println("段3");
			return true;
		}
		 
        //为了兼容老版本 のぁ★※¤●  ，特意屏蔽了段5 和段7
		//\u007B-\u00FF 从字符串z之后，到单字节所有 
//		regEx = "[\u007B-\u00BF]";
//		if(matcherByRegex(regEx,content)){
//			System.out.println("段5");
//			return true;
//		}
		//\u02B0-\u036F：空白修饰字母 (Spacing Modifiers)
		regEx = "[\u02B0-\u036F]";
		if(matcherByRegex(regEx,content)){
			System.out.println("段6");
			return true;
		}
		//\u2000-\u2BFF：常用标点(General Punctuation)
//		regEx = "[\u2000-\u2BFF]";
//		if(matcherByRegex(regEx,content)){
//			System.out.println("段7");
//			return true;
//		}
		//\u2E00-\u303F：追加标点 (Supplemental Punctuation)
		regEx = "[\u2E00-\u303F]";
		if(matcherByRegex(regEx,content)){
			System.out.println("段8");
			return true;
		}
		//\u3100-\u312F：注音字母 (Bopomofo)
		regEx = "[\u3100-\u312F]";
		if(matcherByRegex(regEx,content)){
			System.out.println("段9");
			return true;
		}
		
		//\u3190-\u31EF：象形字注释标志 (Kanbun)
		regEx = "[\u3190-\u31EF]";
		if(matcherByRegex(regEx,content)){
			System.out.println("段10");
			return true;
		}
		//\u3200-\u4DFF：封闭式 CJK 文字和月份 (Enclosed CJK Letters and Months)
		regEx = "[\u3200-\u4DFF]";
		if(matcherByRegex(regEx,content)){
			System.out.println("段11");
			return true;
		}
		//\uA000-\uAADF
		regEx = "[\uA000-\uAADF]";
		if(matcherByRegex(regEx,content)){
			System.out.println("段12");
			return true;
		}
		//\uD800-\uF8FF 
		regEx = "[\uD800-\uF8FF]";
		if(matcherByRegex(regEx,content)){
			System.out.println("段13");
			return true;
		}
		//\uFB00-\uFFFF：字母表达形式 (Alphabetic Presentation Form)
		regEx = "[\uFB00-\uFFFF]";
		if(matcherByRegex(regEx,content)){
			System.out.println("段14");
			return true;
		}
		
		return false;
	}

//	public static boolean haveIllegalChar2(String content)
//	{
//		//过滤•
//		String regEx = "[\u2022]"; 
//		if(matcherByRegex(regEx,content)){
//			return true;
//		}
//		//判断是否有非法标点符号，是否有空格，有回车符，换号符号
//		regEx = "[`~!@#$%^&*()+=|{}':;',//[//].<>/?~！@#￥%……&*（）——+|{}【】‘；：”“’。《》，、？\\s*|\t|\r|\n\u9fa6-\u9fff]";
//		if(matcherByRegex(regEx,content)){
//			return true;
//		}
//		//过滤 
//		regEx = "[\ue78d\u3000]"; 
//		if(matcherByRegex(regEx,content)){
//			return true;
//		}
//		 
//		//全角１２３４５６７８９０·、。，；’＼】【＝－？“：｜”｛｝——＋）（～！＠＃￥％……＆×》《
//		regEx = "[１２３４５６７８９０·、。，；’＼】【＝－？“：｜”｛｝——＋）（～！＠＃￥％……＆×》《]"; 
//		if(matcherByRegex(regEx,content)){
//			return true;
//		}
//		
////		regEx = "[１２３４５６７８９０·、。，；’＼】【＝－？“：｜”｛｝——＋）（～！＠＃￥％……＆×》《]"; 
////		if(matcherByRegex(regEx,content)){
////			return true;
////		}
//		return false;
//	}
	 /**
	  * 是否有匹配的
	  * @param regEx
	  * @param content
	  * @return
	  */
	private static boolean matcherByRegex(String regEx,String content){
		Pattern p = Pattern.compile(regEx);
		Matcher m = p.matcher(content);
		return m.find();
	}

	public static void main(String[] args)
	{
		String contentString = "";
		System.out.println(contentString + "是否有非法字符：" + haveIllegalChar(contentString));
		contentString = "nihao";
		System.out.println(contentString + "是否有非法字符：" + haveIllegalChar(contentString));
		contentString = "dfe年后";
		System.out.println(contentString + "是否有非法字符：" + haveIllegalChar(contentString));
		contentString = "dfe年后./";
		System.out.println(contentString + "是否有非法字符：" + haveIllegalChar(contentString));
		contentString = ""
				+ "";
		System.out.println(contentString + "是否有非法字符：" + haveIllegalChar(contentString));
		contentString = "のメぁ★※¤●";
		System.out.println(contentString + "是否有非法字符：" + haveIllegalChar(contentString));
		
		contentString = "魵鱝鳻黂黺鼖龦";
		System.out.println(contentString + "是否有非法字符：" + haveIllegalChar(contentString));
		
		contentString = "•";
		System.out.println(contentString + "是否有非法字符：" + haveIllegalChar(contentString));
		
		contentString = "";
		System.out.println(contentString + "是否有非法字符：" + haveIllegalChar(contentString));
		
		contentString = "３";
		System.out.println(contentString + "是否有非法字符：" + haveIllegalChar(contentString));
		 
		contentString = "|{}";
		System.out.println(contentString + "是否有非法字符：" + haveIllegalChar(contentString));
		
		contentString = "^";
		System.out.println(contentString + "是否有非法字符：" + haveIllegalChar(contentString));
		
		contentString = "２３５ ";
		System.out.println(contentString + "是否有非法字符：" + haveIllegalChar(contentString));
		contentString = "▅㈱⋚";
		System.out.println(contentString + "是否有非法字符：" + haveIllegalChar(contentString));
		contentString = "ⅵⅶ";
		System.out.println(contentString + "是否有非法字符：" + haveIllegalChar(contentString));
		
		contentString = " ";
		System.out.println(contentString + "是否有非法字符：" + haveIllegalChar(contentString));
		contentString = "1234567890qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM易菲菲是否有非法字符";
		System.out.println(contentString + "是否有非法字符：" + haveIllegalChar(contentString));
		
		contentString = "안녕하세요";
		System.out.println(contentString + "是否有非法字符：" + haveIllegalChar(contentString));
		
		contentString = "Përshëndetje";
		System.out.println(contentString + "是否有非法字符：" + haveIllegalChar(contentString));
		contentString = "こんにちは";
		System.out.println(contentString + "是否有非法字符：" + haveIllegalChar(contentString));
		
		contentString = "Xinchào";
		System.out.println(contentString + "是否有非法字符：" + haveIllegalChar(contentString));
		
		contentString = "のぁ★※¤●";
		System.out.println(contentString + "是否有非法字符：" + haveIllegalChar(contentString));
		contentString = "枯禅ぁ阳羽";
		System.out.println(contentString + "是否有非法字符：" + haveIllegalChar(contentString));
	}
}
