/*
**功能：记录错误！
**描述：当我们把程序封装好之后，别人就看不到调试的信息，我们只能生成到本地的文件里面。
*/
using System;
using System.Collections.Generic;
using System.Web;
using System.IO;
using System.Web.UI;

namespace ElevatorWebServices
{
    public class SystemError : Page
    {
        //记录错误日志位置
    private static string _fileName = HttpContext.Current.Server.MapPath(@"~/LOG/Systemlog.txt");

		public static String  FileName
		{
			get
			{
				return(_fileName);
			}
			set
			{
				if(value != null || value != "")
				{
					_fileName = value;
				}
			}
		}

		/// <summary>
		/// 记录日志至文本文件
		/// </summary>
		/// <param name="message">记录的内容</param>
		public static void SystemLog(string message) 
		{
            if (File.Exists(FileName))
            {
                ///如果日志文件已经存在，则直接写入日志文件
                StreamWriter sr = File.AppendText(FileName);
                sr.WriteLine("\n");
                sr.WriteLine(DateTime.Now.ToString() + message);
                sr.Close();
            }
            else
            {
                ///创建日志文件
                StreamWriter sr = File.CreateText(FileName);
                sr.Close();
            }	
		}
	}
    
}