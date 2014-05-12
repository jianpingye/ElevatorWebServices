using System;
using System.Collections.Generic;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.Security;

namespace ElevatorWebServices
{
    public class DBOperation : IDisposable
    {
        SystemError systemError = new SystemError();
        public SqlConnection sqlCon;
        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        public DBOperation()
        {
            sqlCon = new SqlConnection();
            sqlCon.ConnectionString = ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString;
            try
            {
                sqlCon.Open();
            }
            catch (Exception e)
            {
                SystemError.SystemLog("数据库连接失败。Error Message:" + e.Message);
            }
        }
        #endregion

        #region 析构函数
        /// <summary>
        /// 析构函数，释放非托管资源
        /// </summary>
        ~DBOperation()
        {
            try
            {
                if (sqlCon != null)
                {
                    sqlCon.Close();
                }
            }
            catch (Exception e)
            {
                SystemError.SystemLog("释放Error。Error Message:" + e.Message);
                Console.WriteLine("数据库释放失败" + e.Message);
            }
            finally
            {
                Dispose();
            }
        }
        #endregion

        #region 释放资源
        /// <summary>
        /// 公有方法，释放资源。
        /// </summary>
        public void Dispose()
        {
            // 确保连接被关闭
            if (sqlCon != null)
            {
                sqlCon.Dispose();
                sqlCon = null;
            }
        }
        #endregion

        #region 验证用户名密码
        /// <summary>
        /// 验证用户名密码
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public string Login(string username, string password)
        {
            try
            {
                string pass = FormsAuthentication.HashPasswordForStoringInConfigFile(password, "MD5");
                //创建SQL语句，该语句用来查询用户输入的用户名和密码是否正确
                string sqlSel = "select count(*) from UserInfo where UserName=@username and UserPassword=@userpass";
                //创建SqlCommand对象
                SqlCommand com = new SqlCommand(sqlSel, sqlCon);
                //使用Parameters的add方法添加参数类型
                com.Parameters.Add(new SqlParameter("username", SqlDbType.VarChar, 20));
                //设置Parameters的参数值
                com.Parameters["username"].Value = username;
                com.Parameters.Add(new SqlParameter("userpass", SqlDbType.VarChar, 50));
                com.Parameters["userpass"].Value = pass;
                //判断ExecuteScalar方法返回的参数是否大于0大于表示登录成功并给出提示
                if (Convert.ToInt32(com.ExecuteScalar()) > 0)
                {
                    return "LOGIN_SUCCESS";
                }
                else
                {
                    return "PWD_ERROR";
                }
            }
            //try
            //{
            //    string sql = "SELECT * FROM UserInfo where UserName='" + username + "'";
            //    SqlCommand cmd = new SqlCommand(sql, sqlCon);
            //    SqlDataReader reader = cmd.ExecuteReader();
            //    while (reader.Read())
            //    {                   
            //        string pwd = reader.GetString(reader.GetOrdinal("UserPassword"));
            //        //SystemError.SystemLog("Login Error。Error Message:" + pwd);
            //        string pass = FormsAuthentication.HashPasswordForStoringInConfigFile(password, "MD5");//MD5加密
            //        if (pass == pwd)
            //        {
            //            reader.Close();
            //            return "LOGIN_SUCCESS";
            //        }
            //        else
            //        {
            //            reader.Close();
            //            return "PWD_ERROR";
            //        }
            //    }
            //    reader.Close();
            //    return "USER_ERROR";
            //}
            catch (Exception e)
            {
                //SystemError.SystemLog("Login Error。Error Message:" + e.Message);
                throw e;
            }
            finally
            {
                Dispose();
            }
        }
        #endregion

        #region 注册
        /// <summary>
        /// 注册
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="birthday"></param>
        /// <param name="usersex"></param>
        /// <returns></returns>
        public string Register(string username, string password,DateTime birthday,char usersex,string telephone,string userrealname)
        {
            try
            {
                string sql = "SELECT * FROM UserInfo where UserName='" + username + "'";
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    reader.Close();
                    return "USER_ERROR";//这个用户名已经被注册了
                }
                reader.Close();
                //再次调用数据库要先关闭在开启
                if (sqlCon.State == ConnectionState.Open)
                {
                    sqlCon.Close();
                }
                if (sqlCon.State == ConnectionState.Closed)
                {
                    sqlCon.Open();
                }
                string pass = FormsAuthentication.HashPasswordForStoringInConfigFile(password, "MD5");//MD5加密
                string sql1 = "INSERT INTO UserInfo"
                    + "(UserName,UserPassword,UserBirthday,UserSex,UserPhone,UesrRealName,UserLevel,IsDelete,CreateTime)"
                    + "VALUES" +
                    "('" + username + "','" + pass + "','" + birthday + "','" + usersex + "','" + telephone + "','" + userrealname + "','" + 1 + "','" + 0 + "','" + DateTime.Now.ToString() + "')";
                SqlCommand cmd1 = new SqlCommand(sql1, sqlCon);
                int i = cmd1.ExecuteNonQuery();
                if (i != 0 && i != -1)
                {
                    return "SUCCESS";
                }
                else
                {
                    return "FAIL";
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                Dispose();
            }
        }
        #endregion 注册

        #region 获取用户的个人信息
        /// <summary>
        /// 获取用户的个人信息
        /// </summary>
        /// <returns>用户的个人信息</returns>
        public List<string> selectUserInfo(string username)
        {
            List<string> list = new List<string>();
            try
            {
                string sql = "SELECT * FROM UserInfo where UserName='" + username + "'";
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    //将结果集信息添加到返回向量中
                    list.Add("用户名" + reader[1].ToString());
                    list.Add("密码" + reader[2].ToString());
                    //DateTime date = reader[3].ToString();
                    DateTime dt = Convert.ToDateTime(reader[3].ToString());
                    list.Add("生日" + dt.ToString("yyyy-MM-dd"));
                    list.Add("性别" + reader[4].ToString());
                    list.Add("电话号码" + reader[9].ToString());
                    list.Add("真实姓名" + reader[10].ToString());
                }
                reader.Close();
                cmd.Dispose();

            }
            catch (Exception)
            {

            }
            finally
            {
                Dispose();
            }
            return list;
        }
        #endregion 获取用户的个人信息

        #region 修改密码
        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="username"></param>
        /// <param name="oldpassword"></param>
        /// <param name="userphone"></param>
        /// <returns></returns>
        public string changePassword(string username, string userpassword)
        {
            try
            {
                string pass = FormsAuthentication.HashPasswordForStoringInConfigFile(userpassword, "MD5");//MD5加密
                string sql = "UPDATE UserInfo set UserPassword = '" + pass + "' where UserName='" + username + "'";
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                int i = cmd.ExecuteNonQuery();
                if (i != 0 && i != -1)
                {
                    return "SUCCESS";
                }
                else
                {
                    return "FAIL";
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                Dispose();
            }
        }
        #endregion 修改密码

        #region 修改用户电话号码
        /// <summary>
        /// 修改用户电话号码
        /// </summary>
        /// <param name="username"></param>
        /// <param name="userphone"></param>
        /// <returns></returns>
        public string changeTelephone(string username, string userphone)
        {
            try
            {
                string sql = "UPDATE UserInfo set UserPhone = '" + userphone + "' where UserName='" + username + "'";
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                int i = cmd.ExecuteNonQuery();
                if (i != 0 && i != -1)
                {
                    return "SUCCESS";
                }
                else
                {
                    return "FAIL";
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                Dispose();
            }
        }
        #endregion 修改用户电话号码

        #region 新建任务
        /// <summary>
        /// 新建任务
        /// </summary>
        /// <param name="username"></param>
        /// <param name="elevaterid"></param>
        /// <param name="unit"></param>
        /// <param name="?"></param>
        /// <returns></returns>
        public string NewTask(string username,char elevaterid, string unit)
        {
            int id=0;
            try
            {
                string sql = "SELECT * FROM UserInfo where UserName='" + username + "'";
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    id = reader.GetInt32(reader.GetOrdinal("Password"));
                }
                reader.Close();
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                Dispose();
            }
            if (id == 0)//查找不到该员工
            {
                return "USER_ERROR";
            }
            else
            {
                try
                {
                    string sql1 = "INSERT INTO UserInfo"
                        + "(UserId,ElevatorId,Unit,TaskStatus,CreateTime)"
                        + "VALUES" +
                        "('" + id + "','" + elevaterid + "','" + unit + "','" + 1 + "','" + DateTime.Now.ToString() + "')";
                    SqlCommand cmd1 = new SqlCommand(sql1, sqlCon);
                    int i = cmd1.ExecuteNonQuery();
                    if (i != 0 && i != -1)
                    {
                        return "SUCCESS";
                    }
                    else
                    {
                        return "FAIL";
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
                finally
                {
                    Dispose();
                }
            }
        }
        #endregion 新建任务

        #region 按员工名字查询任务
        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public string findTask(string username) 
        {
            return null;
        }
        #endregion 员工查询任务

    }
}