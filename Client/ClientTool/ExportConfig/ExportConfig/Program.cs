using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using LitJson;

namespace ExportConfig
{
    class Program
    {
        static void DoTask(XlsCfg xls)
        {
            TimeCheck check = new TimeCheck();
            try
            {                
                xls.Read();
                Console.WriteLine("解析EXCEL数据{0}耗时:{1}", xls.file, check.renew);
            }
            catch (Exception e)
            {
                Console.WriteLine("解析EXCEL数据{0}出错:{1}", xls.file, e.Message);
                Console.WriteLine(e.StackTrace);
                return;
            }

            try
            {
                xls.Write();
                Console.WriteLine("输出表格{0}耗时:{1}", xls.file, check.renew);
            }
            catch (Exception e)
            {
                Console.WriteLine("输出表格{0}出错:{1}", xls.file, e.Message);
                Console.WriteLine(e.StackTrace);
                return;
            }
        }

        static void Main(string[] args)
        {
            TimeCheck check = new TimeCheck();
            string currentDirectory = Directory.GetCurrentDirectory();
            Console.WriteLine("当前目录:{0}", currentDirectory);

            //1 解析导表配置
            StreamReader reader = File.OpenText(currentDirectory + "/config.xml" );
            ExportHandler handle = new ExportHandler();
            XMLParser.parseXMLFile(handle, reader.ReadToEnd());
            Console.WriteLine("解析导表配置耗时:{0}", check.renew);

            //2 执行多线程任务
            Parallel.ForEach(handle.xlss, DoTask);
            Console.WriteLine("处理所有数据耗时:{0}", check.renew);

            if (handle.cfg.endPause)
            {
                Console.WriteLine("输入任意键返回");
                Console.Read(); 
            }            
        }
    }
}
