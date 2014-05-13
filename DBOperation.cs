using System;
using System.Collections.Generic;
using System.Web;
using System.Data;
using System.Linq;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.Security;
using System.Xml.Linq;
using System.Web.Script.Serialization;
using System.Web.Script.Services;


namespace ElevatorWebServices
{
    public class DBOperation : IDisposable
    {
        
        SystemError systemError = new SystemError();
        public SqlConnection sqlCon;
        string connectionstring = ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString;
        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        public DBOperation()
        {
            sqlCon = new SqlConnection();
            sqlCon.ConnectionString = connectionstring;
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
                    list.Add("用户名:" + reader[1].ToString());
                    list.Add("密码:" + reader[2].ToString());
                    //DateTime date = reader[3].ToString();
                    DateTime dt = Convert.ToDateTime(reader[3].ToString());
                    list.Add("生日:" + dt.ToString("yyyy-MM-dd"));
                    list.Add("性别:" + reader[4].ToString());
                    list.Add("电话号码:" + reader[9].ToString());
                    list.Add("真实姓名:" + reader[10].ToString());
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
        public string NewTask(string username,string elevaterid, string unit)
        {
            int id=0;
            try
            {
                string sql = "SELECT * FROM UserInfo where UserName='" + username + "'";
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    id = reader.GetInt32(reader.GetOrdinal("UserId"));
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
                    sqlCon = new SqlConnection();
                    sqlCon.ConnectionString = connectionstring;
                    sqlCon.Open();
                    string sql1 = "INSERT INTO Task"
                        + "(UserId,ElevatorId,Unit,TaskStatus,CreateTime,SignPlace,imagePath,RepaireContent,RepaireTime)"
                        + "VALUES" +
                        "('" + id + "','" + elevaterid + "','" + unit + "','" + 1 + "','" + DateTime.Now.ToString() + "','" + "还没签到" + "','" + "还没上传" + "','" + "还没维修" + "','" + "2000-01-01" + "')";
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

        #region 员工签到
        /// <summary>
        /// 员工签到
        /// </summary>
        /// <param name="id"></param>
        /// <param name="place"></param>
        /// <returns></returns>

        public string signPlace(int id, string place)
        { 
            try
            {
                string sql = "UPDATE Task set SignPlace = '" + place + "' where Id='" + id + "'";
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
        #endregion 员工签到

        #region 上传图片
        public string UploadPicture(int id ,string picturename)
        {
            try
            {
                string sql = "UPDATE Task set imagePath = '" + picturename + "' where Id='" + id + "'";
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
        #endregion 上传图片

        #region 按员工名字查询任务
        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public Object findTask(string username)
        {
            int id = 0;
            String[] Id = null;
            String[] elevatorId = null;
            String[] unit = null;
            String[] taskStatus = null;
            String[] signPlace = null;
            //String[] imagePath = null;
            String[] CreateTime = null;
            String[] RepaireContent = null;
            String[] RepaireTime = null;
            try
            {
                string sql = "SELECT * FROM UserInfo where UserName='" + username + "'";
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    id = reader.GetInt32(reader.GetOrdinal("UserId"));
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
                    sqlCon = new SqlConnection();
                    sqlCon.ConnectionString = connectionstring;
                    sqlCon.Open();
                    String sql1 = "select * from Task where UserId='" + id + "'";
                    SqlCommand cmd = new SqlCommand(sql1, sqlCon);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataSet dataset = new DataSet();
                    adapter.Fill(dataset);
                    DataTable table = dataset.Tables[0];
                    DataRowCollection rows = table.Rows;
                    Id = new String[rows.Count];
                    elevatorId = new String[rows.Count];
                    unit = new String[rows.Count];
                    taskStatus = new String[rows.Count];
                    signPlace = new String[rows.Count];
                    CreateTime = new String[rows.Count];
                    RepaireContent = new String[rows.Count];
                    RepaireTime = new String[rows.Count];
                    for (int i = 0; i < rows.Count; i++)
                    {
                        DataRow row = rows[i];
                        Id[i] = Convert.ToString(row["Id"]);
                        elevatorId[i] = Convert.ToString(row["ElevatorId"]);
                        unit[i] = Convert.ToString(row["Unit"]);
                        taskStatus[i] = Convert.ToString(row["TaskStatus"]);
                        signPlace[i] = Convert.ToString(row["SignPlace"]);
                        CreateTime[i] = Convert.ToString(row["CreateTime"]);
                        RepaireContent[i] = Convert.ToString(row["RepaireContent"]);
                        RepaireTime[i] = Convert.ToString(row["RepaireTime"]);
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
                var result = new
                {
                    Id,
                    elevatorId,
                    unit,
                    taskStatus,
                    signPlace,
                    CreateTime,
                    RepaireContent,
                    RepaireTime
                };
                return new JavaScriptSerializer().Serialize(result); 
            }
        }

        #endregion 员工查询任务

        #region 查询所有员工信息
        public Object GetUserInfo()
        {
            String sql = "SELECT * FROM UserInfo";
           // Object[] arr = null;
            String[] username = null;
            String[] realname = null;
            String[] telephone = null;
            String[] birthday = null;
            String[] sex = null;
            using (SqlCommand cmd = new SqlCommand(sql, sqlCon))
            {
                try
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataSet dataset = new DataSet();
                    adapter.Fill(dataset);
                    DataTable table = dataset.Tables[0];
                    DataRowCollection rows = table.Rows;
                    username = new String[rows.Count];
                    realname = new String[rows.Count];
                    telephone = new String[rows.Count];
                    birthday = new String[rows.Count];
                    sex = new String[rows.Count];
                    for (int i = 0; i < rows.Count; i++)
                    {
                        DataRow row = rows[i];
                        username[i] = Convert.ToString(row["UserName"]);
                        realname[i] = Convert.ToString(row["UesrRealName"]);
                        telephone[i] = Convert.ToString(row["UserPhone"]);
                        birthday[i] = Convert.ToString(row["UserBirthday"]);
                        sex[i] = Convert.ToString(row["UserSex"]);
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
                var result = new
                {
                    username,
                    realname,
                    telephone,
                    birthday,
                    sex
                };
                return new JavaScriptSerializer().Serialize(result);
            }
        }
        #endregion 查询所有员工信息

        #region 获取任务信息
        /// <summary>
        /// 获取任务信息
        /// </summary>
        /// <returns>任务信息</returns>
        public List<string> selectTaskInfo(int id)
        {
            List<string> list = new List<string>();
            try
            {
                string sql = "SELECT * FROM Task where Id='" + id + "'";
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    //将结果集信息添加到返回向量中
                    list.Add("编号:" + reader[0].ToString());
                    list.Add("用户名:" + reader[1].ToString());
                    list.Add("电梯编号:" + reader[2].ToString());
                    list.Add("地点:" + reader[3].ToString());
                    list.Add("任务状态:" + reader[4].ToString());
                    list.Add("签到:" + reader[5].ToString());
                    DateTime dt = Convert.ToDateTime(reader[7].ToString());
                    list.Add("任务发布时间:" + dt.ToString("yyyy-MM-dd"));
                    list.Add("维修内容:" + reader[8].ToString());
                    DateTime dt1 = Convert.ToDateTime(reader[9].ToString());
                    list.Add("任务完成时间:" + dt1.ToString("yyyy-MM-dd"));
                    
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
        #endregion 获取任务信息

        #region 提交任务的内容和完成的时间
        public string CommitTask(int id,string content, DateTime time)
        {
            try
            {
                string sql = "UPDATE Task set RepaireContent = '" + content + "',RepaireTime = '" + time + "',TaskStatus= '" + 2 + "' where Id='" + id + "'";
                SqlCommand cmd = new SqlCommand(sql, sqlCon);
                int i= cmd.ExecuteNonQuery();
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
        #endregion 提交任务的内容和完成的时间



    }
}