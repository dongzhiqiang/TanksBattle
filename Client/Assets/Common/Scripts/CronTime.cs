using UnityEngine;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

public class CronTime
{
    int m_sec;
    int m_min;
    int m_hour;
    int m_day;
    int m_month;
    int m_dayOfWeek;
    int m_year;
    bool m_hasSec;
    bool m_hasMin;
    bool m_hasHour;
    bool m_hasDay;
    bool m_hasMonth;
    bool m_hasDayOfWeek;
    bool m_hasYear;
    public CronTime()
    {
        m_hasSec = false;
        m_hasMin = false;
        m_hasHour = false;
        m_hasDay = false;
        m_hasMonth = false;
        m_hasDayOfWeek = false;
        m_hasYear = false;
    }
    public CronTime(string cronStr)
    {
        LoadStr(cronStr);
    }
    void ParseOneNum(string cronNum, out int num, out bool hasNum)
    {
        num = 0;
        if (cronNum == "*")
        {
            hasNum = false;
        }
        else
        {
            if (int.TryParse(cronNum, out num))
            {
                hasNum = true;
            }
            else
            {
                hasNum = false;
                Debuger.LogError("cron表达式不正确，未支持的子表达式:" + num);
            }
        }
    }
    public void LoadStr(string cronStr)
    {
        string[] tempStr = cronStr.Split(' ');
        if (tempStr.Length < 6)
        {
            Debuger.LogError("cron表达式不正确:" + cronStr);
            return;
        }
        ParseOneNum(tempStr[0], out m_sec, out m_hasSec);
        ParseOneNum(tempStr[1], out m_min, out m_hasMin);
        ParseOneNum(tempStr[2], out m_hour, out m_hasHour);
        ParseOneNum(tempStr[3], out m_day, out m_hasDay);
        ParseOneNum(tempStr[4], out m_month, out m_hasMonth);
        ParseOneNum(tempStr[5], out m_dayOfWeek, out m_hasDayOfWeek);
        if(tempStr.Length>6)
        {
            ParseOneNum(tempStr[6], out m_year, out m_hasYear);
        }
        else
        {
            m_year = 0;
            m_hasYear = false;
        }
    }
    public int Sec
    {
        get
        {
            if(!m_hasSec)
            {
                Debuger.LogError("没有设置秒");
                return 0;
            }
            return m_sec;
        }
    }
    public int Min
    {
        get
        {
            if (!m_hasMin)
            {
                Debuger.LogError("没有设置分");
                return 0;
            }
            return m_min;
        }
    }
    public int Hour
    {
        get
        {
            if (!m_hasHour)
            {
                Debuger.LogError("没有设置时");
                return 0;
            }
            return m_hour;
        }
    }
    public int Day
    {
        get
        {
            if (!m_hasDay)
            {
                Debuger.LogError("没有设置日");
                return 0;
            }
            return m_day;
        }
    }
    public int Month
    {
        get
        {
            if (!m_hasMonth)
            {
                Debuger.LogError("没有设置月");
                return 0;
            }
            return m_month;
        }
    }
    public int DayOfWeek
    {
        get
        {
            if (!m_hasDayOfWeek)
            {
                Debuger.LogError("没有设置星期");
                return 0;
            }
            return m_dayOfWeek;
        }
    }
    public int Year
    {
        get
        {
            if (!m_hasYear)
            {
                Debuger.LogError("没有设置年");
                return 0;
            }
            return m_year;
        }
    }
    public bool HasSec { get { return m_hasSec; } }
    public bool HasMin { get { return m_hasMin; } }
    public bool HasHour { get { return m_hasHour; } }
    public bool HasDay { get { return m_hasDay; } }
    public bool HasMonth { get { return m_hasMonth; } }
    public bool HasDayOfWeek { get { return m_hasDayOfWeek; } }
    public bool HasYear { get { return m_hasYear; } }

    int DayOfWeekToNum(System.DayOfWeek dayOfWeek)
    {
        switch(dayOfWeek)
        {
            case System.DayOfWeek.Monday:
                return 1;
            case System.DayOfWeek.Tuesday:
                return 2;
            case System.DayOfWeek.Wednesday:
                return 3;
            case System.DayOfWeek.Thursday:
                return 4;
            case System.DayOfWeek.Friday:
                return 5;
            case System.DayOfWeek.Saturday:
                return 6;
            case System.DayOfWeek.Sunday:
                return 7;
        }
        return 0;
    }

    bool CompareWith(DateTime time, int compareSign, bool orEqual)
    {
        bool compared = false;
        int sign = 0;
        if(m_hasYear)
        {
            compared = true;
            sign = Math.Sign(m_year - time.Year);
            if (sign == compareSign && compareSign != 0)
            {
                return true;
            }
            else if(sign != 0)
            {
                return false;
            }
        }
        if (m_hasMonth)
        {
            compared = true;
            sign = Math.Sign(m_month - time.Month);
            if (sign == compareSign && compareSign != 0)
            {
                return true;
            }
            else if (sign != 0)
            {
                return false;
            }
        }
        if (m_hasDay)
        {
            compared = true;
            sign = Math.Sign(m_day - time.Day);
            if (sign == compareSign && compareSign != 0)
            {
                return true;
            }
            else if (sign != 0)
            {
                return false;
            }
        }
        if (m_hasDayOfWeek) //..
        {
            compared = true;
            sign = Math.Sign(m_dayOfWeek - DayOfWeekToNum(time.DayOfWeek));
            if (sign == compareSign && compareSign != 0)
            {
                return true;
            }
            else if (sign != 0)
            {
                return false;
            }
        }
        if (m_hasHour)
        {
            compared = true;
            sign = Math.Sign(m_hour - time.Hour);
            if (sign == compareSign && compareSign != 0)
            {
                return true;
            }
            else if (sign != 0)
            {
                return false;
            }
        }
        if (m_hasMin)
        {
            compared = true;
            sign = Math.Sign(m_min - time.Minute);
            if (sign == compareSign && compareSign != 0)
            {
                return true;
            }
            else if (sign != 0)
            {
                return false;
            }
        }
        if (m_hasSec)
        {
            compared = true;
            sign = Math.Sign(m_sec - time.Second);
            if (sign == compareSign && compareSign != 0)
            {
                return true;
            }
            else if (sign != 0)
            {
                return false;
            }
        }
        if(!compared || orEqual)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool GreaterThan(DateTime time)
    {
        return CompareWith(time, 1, false);
    }

    public bool GreaterThanOrEqual(DateTime time)
    {
        return CompareWith(time, 1, true);
    }

    public bool LessThan(DateTime time)
    {
        return CompareWith(time, -1, false);
    }

    public bool LessThanOrEqual(DateTime time)
    {
        return CompareWith(time, -1, true);
    }

    public bool EqualTo(DateTime time)
    {
        return CompareWith(time, 0, true);
    }

    static string[] m_dayOfWeekStr = { "", "星期一", "星期二","星期三","星期四","星期五","星期六","星期日"};

    public string GetString(bool first=false)
    {
        StringBuilder result = new StringBuilder();

        //如果前后端时区不一样，要强调以服务器时间为准
        if (first && TimeMgr.instance.GetClientTimeZoneOffset() != TimeMgr.instance.GetServerTimeZoneOffset())
        {
            result.Append("以服务器时间为准的");
        }
            
        if(m_hasYear)
        {
            result.AppendFormat("{0}年", m_year);
        }
        if (m_hasMonth)
        {
            result.AppendFormat("{0}月", m_month);
        }
        if (m_hasDay)
        {
            if(first && !m_hasMonth)
            {
                result.Append("每月");
            }
            result.AppendFormat("{0}日", m_day);
        }
        if( m_hasDayOfWeek)
        {
            if(first)
            {
                result.Append("每");
            }
            result.Append(m_dayOfWeekStr[m_dayOfWeek]);
        }
        if( m_hasHour)
        {
            if (first && !m_hasDay && !m_hasDayOfWeek)
            {
                result.Append("每日");
            }
            if (m_min == 0 && m_sec == 0)
            {
                result.AppendFormat("{0}时", m_hour);
            }
            else if (m_sec == 0)
            {
                result.AppendFormat("{0}时{1}分", m_hour, m_min);
            }
            else
            {
                result.AppendFormat("{0}时{1}分{2}秒", m_hour, m_min, m_sec);
            }
        }
        else if(m_hasMin)
        {
            if (first)
            {
                result.Append("每小时");
            }
            result.AppendFormat("{0}分{1}秒", m_min, m_sec);
        }
        else if(m_hasSec)
        {
            if (first)
            {
                result.Append("每分钟");
            }
            result.AppendFormat("{0}秒", m_sec);
        }

        if(result.Length <= 0)
            return "任何时间";
        else
            return result.ToString();
    }

    static void TestWith(bool result, string msg)
    {
        if(result)
        {
            Debuger.Log(msg + " pass");
        }
        else
        {
            Debuger.LogError(msg + " failed");
        }
    }
    public static void Test()
    {
        TestWith(new CronTime("* * * * * 1").EqualTo(new DateTime(2016, 4, 18, 15, 43, 0)), "case 1");
        TestWith(!new CronTime("1 43 15 * * *").EqualTo(new DateTime(2016, 4, 18, 15, 43, 0)), "case 2");
        TestWith(new CronTime("0 43 15 * 4 *").EqualTo(new DateTime(2016, 4, 18, 15, 43, 0)), "case 3");
        TestWith(new CronTime("* * * * * 1").GreaterThanOrEqual(new DateTime(2016, 4, 18, 15, 43, 0)), "case 4");
        TestWith(new CronTime("0 43 15 * 4 *").GreaterThanOrEqual(new DateTime(2016, 4, 18, 15, 43, 0)), "case 5");
        TestWith(new CronTime("* * * * * 1").LessThanOrEqual(new DateTime(2016, 4, 18, 15, 43, 0)), "case 6");
        TestWith(new CronTime("0 43 15 * 4 *").LessThanOrEqual(new DateTime(2016, 4, 18, 15, 43, 0)), "case 7");
        TestWith(!new CronTime("1 43 15 * * *").LessThan(new DateTime(2016, 4, 18, 15, 43, 0)), "case 8");
        TestWith(new CronTime("1 43 15 * * *").GreaterThan(new DateTime(2016, 4, 18, 15, 43, 0)), "case 9");
        TestWith(!new CronTime("1 43 15 * * *").LessThanOrEqual(new DateTime(2016, 4, 18, 15, 43, 0)), "case 10");
        TestWith(new CronTime("1 43 15 * * *").GreaterThanOrEqual(new DateTime(2016, 4, 18, 15, 43, 0)), "case 11");
        TestWith(new CronTime("1 43 15 * * * 2015").LessThan(new DateTime(2016, 4, 18, 15, 43, 0)), "case 12");
        TestWith(new CronTime("1 43 15 * 5 * 2016").GreaterThan(new DateTime(2016, 4, 18, 15, 43, 0)), "case 13");
        TestWith(new CronTime("1 43 15 * * * 2015").LessThanOrEqual(new DateTime(2016, 4, 18, 15, 43, 0)), "case 14");
        TestWith(new CronTime("1 43 15 * 5 * 2016").GreaterThanOrEqual(new DateTime(2016, 4, 18, 15, 43, 0)), "case 15");
        TestWith(new CronTime("1 43 15 17 4 * 2016").LessThan(new DateTime(2016, 4, 18, 15, 43, 0)), "case 16");
    }
}
