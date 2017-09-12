using System;
using System.Collections.Generic;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Linq;

public class Util{
     static string GetCopyFile(string sExcelFile)
    {
        int index = sExcelFile.LastIndexOf('.');
        if (index <= -1)
            return sExcelFile + "__copy__";

        return sExcelFile.Insert(index, "__copy__");
    }

    public static byte[] ReadFileByte(string file)
    {
        if (!File.Exists(file))
        {
            Console.WriteLine(file + " not find!");
            return null;
        }

        try
        {
            FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
            byte[] bytes = new byte[fs.Length];
            fs.Read(bytes, 0, (int)fs.Length);
            fs.Close();
            return bytes;
        }
        catch (Exception /*ex*/)
        {

        }

        string copy_file = GetCopyFile(file);
        while (File.Exists(copy_file))
            copy_file = GetCopyFile(copy_file);

        File.Copy(file, copy_file);
        byte[] data = ReadFileByte(copy_file);
        File.Delete(copy_file);
        return data;
    }

    
}
public class ExportCfg{
    public string clientPath;
    public string serverPath;
    public bool endPause;
}

public class XlsTableCfg{
    public ExportCfg cfg;
    public XlsCfg parent;
    public string name;
    public string clientPath;
    public string serverPath;

    // 从excel表取出来的原始数据
    public List<string> descs = new List<string>();//第一列描述
    public List<string> fields = new List<string>();//第二列字段
    public List<List<string>> data = new List<List<string>>();
    public bool isRead = false;

    public XlsTableCfg(ExportCfg cfg,XlsCfg parent){
        this.parent = parent;
        this.cfg =cfg;
    }

    public void Read(ISheet sheet)
    {
        int fieldsCnt = 0;  //统一的列数
        int emptyRowCnt = 0;//连续空白行数
        for (int j = 0; j <= sheet.LastRowNum; j++)
        {
            IRow row = sheet.GetRow(j);  //读取当前行数据
            if (row != null)
            {
                List<string> rowData;
                if (j == 0)
                    continue;
                else if (j == 1)
                    rowData = descs;//描述
                else if (j == 2)
                    rowData = fields;//字段名
                else
                {
                    rowData = new List<string>();
                    data.Add(rowData);
                }

                int lastColNum = fieldsCnt > 0 ? fieldsCnt + 1 : row.LastCellNum;
                bool allCellEmpty = true;
                for (int k = 0; k < lastColNum; k++)
                {
                    if(k == 0)//第一列忽略
                        continue;
                    ICell cell = row.LastCellNum < k ? null : row.GetCell(k);  //当前表格
                    string cellText = cell == null ? "" : cell.ToString();
                    if (string.IsNullOrEmpty(cellText))
                    {
                        rowData.Add("");                        
                    }
                    else
                    {
                        allCellEmpty = false;
                        rowData.Add(cellText);
                    }

                    //读取第二行（描述行）、第三行（字段名行）时没确定列数，判断连续三个全空白就不再读取了
                    if (j == 1 || j == 2)
                    {
                        int curColCnt = rowData.Count;
                        if (curColCnt >= 3)
                        {
                            if (rowData[curColCnt - 1] == "" && rowData[curColCnt - 2] == "" && rowData[curColCnt - 3] == "")
                            {
                                //列数，只参考字段名行
                                if (j == 2)
                                    fieldsCnt = rowData.Count - 3;
                                break;
                            }
                        }
                    }
                }

                //如果读取的是第三行（字段名行），如果没遇上连续三列空白，那就直接用这个字段名行的列数
                if (j == 2 && fieldsCnt <= 0)
                    fieldsCnt = rowData.Count;

                //如果连续三行是空行，那就直接不读后续行了
                if (j > 2 && allCellEmpty)
                {
                    ++emptyRowCnt;
                    if (emptyRowCnt >= 3)
                    {
                        data.RemoveRange(data.Count - 3, 3);
                        break;
                    }                        
                }
                else
                {
                    emptyRowCnt = 0;
                }
            }
        }
        //调整描述行、字段名行的列数为统一的列数
        if (descs.Count > fieldsCnt)
            descs.RemoveRange(fieldsCnt, descs.Count - fieldsCnt);
        else if (descs.Count < fieldsCnt)
            descs.AddRange(Enumerable.Repeat("", fieldsCnt - descs.Count));
        if (fields.Count > fieldsCnt)
            fields.RemoveRange(fieldsCnt, fields.Count - fieldsCnt);
        else if (fields.Count < fieldsCnt)
            fields.AddRange(Enumerable.Repeat("", fieldsCnt - fields.Count));
        isRead = true;
    }

    public void WriteServer()
    {
        if (string.IsNullOrEmpty(serverPath) || !isRead)
            return;

        //计算路径
        string path ;
        if (!cfg.serverPath.Contains(":"))//相对路径
        {
            path = Directory.GetCurrentDirectory() + "/" + cfg.serverPath+"/" + serverPath;
        }
        else
        {
            path = cfg.serverPath + "/" + serverPath;
        }

        //创建文件
        if (File.Exists(path))
            File.Delete(path);
        else
            Directory.CreateDirectory(path.Substring(0, path.LastIndexOf('/')));//确保有这个目录

        //保存
        File.WriteAllText(path, new CsvWriter().WriteAll(descs, fields, data));
    }
    public void WriteClient()
    {
        if (string.IsNullOrEmpty(clientPath) || !isRead)
            return;

        //计算路径
        string path;
        if (!cfg.clientPath.Contains(":"))//相对路径
        {
            path = Directory.GetCurrentDirectory() + "/" + cfg.clientPath + "/" + clientPath;
        }
        else
        {
            path = cfg.clientPath + "/" + clientPath;
        }

        //创建文件
        if (File.Exists(path))
            File.Delete(path);
        else
            Directory.CreateDirectory(path.Substring(0, path.LastIndexOf('/')));//确保有这个目录

        //保存
        File.WriteAllText(path, new CsvWriter().WriteAll(descs, fields, data));
    }
}

public class XlsCfg{
    public ExportCfg cfg;
    public string file;
    public Dictionary<string,XlsTableCfg> tables = new Dictionary<string,XlsTableCfg>();

    public XlsCfg(ExportCfg cfg)
    {
        this.cfg = cfg;
    }
    public void Read()
    {
        string currentDirectory = Directory.GetCurrentDirectory();
        IWorkbook wk = null;
        string filePath = currentDirectory + "/配置表/" + this.file;
        string fileExt = Path.GetExtension(filePath).ToLower();
        MemoryStream stream = new MemoryStream(Util.ReadFileByte(filePath));
        if (fileExt == ".xls")
        {
            wk = new HSSFWorkbook(stream);
        }
        else if (fileExt == ".xlsx")
        {
            wk = new XSSFWorkbook(stream);
        }
        else
        {
            Console.WriteLine(filePath + " Error!");
            return;
        }

        for (int i = 0; i < wk.NumberOfSheets; i++)  //NumberOfSheets是myxls.xls中总共的表数
        {
            ISheet sheet = wk.GetSheetAt(i);   //读取当前表数据

            XlsTableCfg tableCfg;
            if (!this.tables.TryGetValue(sheet.SheetName, out tableCfg))
                continue;
            tableCfg.Read(sheet);
        }
    }

    public void Write()
    {
        foreach (XlsTableCfg t in tables.Values)
        {
            if (!t.isRead)
            {
                Console.WriteLine("{0}的{1}没有被读取", file,t.name);
                continue;
            }
            t.WriteServer();
            t.WriteClient();
        }

    }
}

public class ExportHandler: XMLHandlerReg
{
    public ExportCfg cfg = new ExportCfg();
    public List<XlsCfg> xlss = new List<XlsCfg>();

    XlsCfg Current;

    public ExportHandler()
    {
        RegElementStart("Config", OnElementStart_Config);
        RegElementEnd("Config", OnElementEnd_Config);

        RegElementStart("XlsFile", OnElementStart_XlsFile);
        RegElementEnd("XlsFile", OnElementEnd_XlsFile);

        RegElementStart("Table", OnElementStart_Table);
        RegElementEnd("Table", OnElementEnd_Table);        
    }

    public void OnElementStart_Config(string element, XMLAttributes attributes)
    {
        cfg.clientPath = attributes.getValue("clientPath");
        cfg.serverPath = attributes.getValue("serverPath");
        cfg.endPause = attributes.getValueAsBool("endPause", false);
    }

    public void OnElementEnd_Config(string element)
    {

    }

    public void OnElementStart_XlsFile(string element, XMLAttributes attributes)
    {
        Current = new XlsCfg(cfg);
        Current.file = attributes.getValue("file");
        xlss.Add(Current);
    }

    public void OnElementEnd_XlsFile(string element)
    {
        
    }

    public void OnElementStart_Table(string element, XMLAttributes attributes)
    {
        XlsTableCfg d = new XlsTableCfg(cfg,Current);
        d.name = attributes.getValue("name");
        d.clientPath = attributes.getValue("client");
        d.serverPath = attributes.getValue("server");

        Current.tables.Add(d.name, d);
    }

    public void OnElementEnd_Table(string element)
    {

    }
    
}