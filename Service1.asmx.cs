using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Services;
using System.IO;
using System.ComponentModel;
using System.Web.Script.Serialization;
using System.Web.Script.Services;


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

        public class Result
        {
            public string iResult;
        }

        [WebMethod(Description = "测试例子输出helloworld")]
        public string HelloWorld()
        {
            return "Hello World";
        }

        [WebMethod(Description = "验证数据库中是否有此用户名或密码是否正确")]
        //[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public object checkUserPwd(string username, string password)
        {
            Result result = new Result();
            //System.Diagnostics.Debug.Write("1");
            try
            {
                switch (db.Login(username, password))
                {
                    case "LOGIN_SUCCESS":
                        //result.iResult = "1";
                        //string iResult = "1";
                        //return "{\"iResult\":\"" + 1 + "\"}";
                        //return new JavaScriptSerializer().Serialize(result);
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
        public string UploadFile(string fs1,string FileName,int id)  
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
               switch (db.UploadPicture(id, FileName))
               {
                   case "SUCCESS": return "文件上传成功";
                   case "FAIL": return "FAIL";
                   default: return null;
               }
               //f = null;  
               //m = null;
              // m.Dispose();
               //return "文件上传成功";  
           }  
           catch(Exception ex)  
           {
               SystemError.SystemLog("上传图片 Error。Error Message:" + ex.Message);
               return ex.Message.ToString(); 
           }  

       }

        [WebMethod(Description = "新建任务，返回是否成功")]
        public string NewTask(string username, string ElevatorId, string unit)
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
                SystemError.SystemLog("新建任务失败 Error。Error Message:" + ex.Message);
                return ex.Message.ToString();
            }

        }

        [WebMethod(Description = "根据员工姓名来查询任务")]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string FindTask(string username)
        {
            try
            {
                string str=(string)db.findTask(username);
                if (str.Equals("USER_ERROR"))
                {
                    return "USER_ERROR";
                }
                else if (str.Equals("have_user"))
                {
                    return "HAVE USER";
                }
                else
                {
                    return str;
                    //return new JavaScriptSerializer().Serialize(db.findTask(username));
                }
                //return db.findTask(username);
            }
            catch (Exception e)
            {
                SystemError.SystemLog("查询任务失败 Error。Error Message:" + e.Message);
                return e.Message.ToString();
            }
        }

        [WebMethod(Description = "获取全部员工的信息")]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetUserInfo()
        {
            string str = (string)db.GetUserInfo();
            return str;
            //return new JavaScriptSerializer().Serialize(db.GetUserInfo());
        }

        [WebMethod(Description = "获取单个任务信息")]
        public string[] selectTaskInfo(int id)
        {
            try
            {
                return db.selectTaskInfo(id).ToArray();
            }
            catch (Exception e)
            {
                SystemError.SystemLog("获取任务失败 Error。Error Message:" + e.Message);
                return null;
            }
        }

        [WebMethod(Description = "员工签到")]
        public string signPlace(int id, string place)
        {
            try
            {
                switch (db.signPlace(id, place))
                {
                    case "SUCCESS": return "SUCCESS";
                    case "FAIL": return "FAIL";
                    default: return null;
                }
            }
            catch (Exception e)
            {
                SystemError.SystemLog("签到 Error。Error Message:" + e.Message);
                return e.Message;
            }

        }

        [WebMethod(Description = "任务提交")]
        public string CommitTask(int id, string content,DateTime time)
        {
            try
            {
                switch (db.CommitTask(id, content,time))
                {
                    case "SUCCESS": return "SUCCESS";
                    case "FAIL": return "FAIL";
                    default: return null;
                }
            }
            catch (Exception e)
            {
                SystemError.SystemLog("任务提交失败 Error。Error Message:" + e.Message);
                return e.Message;
            }

        }
    }
}