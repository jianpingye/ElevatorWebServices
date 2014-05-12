using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Services;
using System.IO;

namespace ElevatorWebServices
{
    /// <summary>
    /// Service1 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    public class Service1 : System.Web.Services.WebService
    {
        DBOperation db = new DBOperation();
        
        [WebMethod(Description = "测试例子输出helloworld")]
        public string HelloWorld()
        {
            return "Hello World";
        }

        [WebMethod(Description = "验证数据库中是否有此用户名或密码是否正确")]
        public string checkUserPwd(string username, string password)
        {
            //System.Diagnostics.Debug.Write("1");
            try
            {
                switch (db.Login(username, password))
                {
                    case "LOGIN_SUCCESS":
                        return "SUCCSEE";
                    case "PWD_ERROR":
                        return "PWD_ERROR";
                    case "USER_ERROR":
                        return "USER_ERROR";
                    default: return null;
                }
            }
            catch (Exception e)
            {
                SystemError.SystemLog("Login Error。Error Message:" + e.Message);
                return e.Message;
            }
        }

        [WebMethod(Description = "注册新用户")]
        public string Register(string username, string password, DateTime birthday, char usersex,string telephone,string userrealname)
        {
            try
            {
                switch (db.Register(username, password, birthday, usersex, telephone, userrealname))
                {
                    case "SUCCESS":
                        return "REGISTER_SUCCSEE";
                    case "FAIL":
                        return "REGISTER_FAIL";
                    case "USER_ERROR":
                        return "REGISTER_USER_ERROR";
                    default: return null;
                }
            }
            catch (Exception e)
            {
                SystemError.SystemLog("Register Error。Error Message:" + e.Message);
                return e.Message;
            }

        }

        [WebMethod(Description = "获取用户的个人信息")]
        public string[] selectUserInfo(string uesrname)
        {
            try
            {
                return db.selectUserInfo(uesrname).ToArray();
            }
            catch (Exception e)
            {
                SystemError.SystemLog("DateBase Error。Error Message:" + e.Message);
                return null;
            }
        }

        [WebMethod(Description = "修改用户密码")]
        public string changePassword(string uesrname,string newpassword)
        {
            try
            {
                switch (db.changePassword(uesrname, newpassword))
                {
                    case "SUCCESS": return "SUCCESS";
                    case "FAIL": return "FAIL";
                    default: return null;
                }
            }
            catch (Exception e)
            {
                SystemError.SystemLog("changePassword Error。Error Message:" + e.Message);
                return e.Message;
            }
            
        }

        [WebMethod(Description = "修改用户电话号码")]
        public string changeTelephone(string uesrname, string phone)
        {
            try
            {
                switch (db.changeTelephone(uesrname, phone))
                {
                    case "SUCCESS": return "SUCCESS";
                    case "FAIL": return "FAIL";
                    default: return null;
                }
            }
            catch (Exception e)
            {
                SystemError.SystemLog("changeTelephone Error。Error Message:" + e.Message);
                return e.Message;
            }
        }
        
        [WebMethod(Description = "上传服务器图片信息，返回是否成功")]  
        public string UploadFile(string fs1,string FileName)  
       {  
           try 
           {
               byte[] fs = Convert.FromBase64String(fs1);
              // byte[] buffer = new BASE64Decoder().decodeBuffer(image); 
               MemoryStream m = new MemoryStream(fs);//定义并实例化一个内存流，来存放上传的图片二进制流  
               //FileStream f = new FileStream(Server.MapPath(".") + "\\images" + FileName, FileMode.Create);
               FileStream f = new FileStream(Server.MapPath(".\\images"+"\\"+ FileName), FileMode.Create);//把内存里的文件写入文件流  
               m.WriteTo(f);
               m.Flush();
               f.Flush();
               m.Close();
               f.Close();
               //f = null;  
               //m = null;
              // m.Dispose();
               return "文件上传成功";  
           }  
           catch(Exception ex)  
           {
               SystemError.SystemLog("changeTelephone Error。Error Message:" + ex.Message);
               return ex.Message.ToString(); 
           }  

       }

        [WebMethod(Description = "新建任务，返回是否成功")]
        public string NewTask(string username, char ElevatorId, string unit)
        {
            try
            {
                switch(db.NewTask(username, ElevatorId, unit))
                {
                    case "SUCCESS":
                        return "SUCCSEE";
                    case "FAIL":
                        return "FAIL";
                    case "USER_ERROR":
                        return "USER_ERROR";
                    default: return null;
                }
            }
            catch (Exception ex)
            {
                SystemError.SystemLog("changeTelephone Error。Error Message:" + ex.Message);
                return ex.Message.ToString();
            }

        }

    }
}