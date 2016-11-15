using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LotteryData;
using System.Data.SqlClient;
using System.Data;
using System.Data.SQLite;
using Newtonsoft.Json;
namespace LotteryTest
{
    public class Program
    {
        static void Main(string[] args)
        {
            Program p = new Program();
        }
        public Program()
        {
            fillTable();
            QueueUserlist();
            // QueueUserlistpage();
            UpdateUserbyID();
            DeleteUserByID();
            Console.ReadKey();
        }
        //插入用户
        void fillTable()
        {
            var objOrderinfo = new Uses
            {
                phone = "15695197097",
                UserNo = "Coach001",
                UserName = "测试",
                Department = "技术部",
                CreateTime = DateTime.Now,
                Islottery = 0
            };
            string sql = @"insert into Users (phone,UserNo,UserName,Department,CreateTime,Islottery) values (
            @phone,@UserNo,@UserName,@Department,@CreateTime,@Islottery
            )";
            try
            {
                var paras = new SQLiteParameter[]
           {

                    new SQLiteParameter() { ParameterName = "@phone", DbType = DbType.String, Value =objOrderinfo.phone },
                              new SQLiteParameter() { ParameterName = "@UserNo", DbType = DbType.String, Value = objOrderinfo.UserNo },
                              new SQLiteParameter() { ParameterName = "@UserName", DbType = DbType.String, Value = objOrderinfo.UserName },
                              new SQLiteParameter() { ParameterName = "@Department", DbType = DbType.String, Value = objOrderinfo.Department },
                              new SQLiteParameter() { ParameterName = "@CreateTime", DbType = DbType.DateTime, Value = DateTime.Now },
                              new SQLiteParameter() { ParameterName = "@Islottery", DbType = DbType.Int32, Value =objOrderinfo.Islottery },
           };
                string connectstr = "Data Source=D:\\SqlLites.db;Version=3;UseUTF16Encoding=True";
                var data = SQLiteHelper.ExecuteNonQuery(connectstr, sql, paras);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        //获取列表
        void QueueUserlist()
        {
            var sql = @"select* from Users where Islottery=@Islottery";
            var paras = new SQLiteParameter[]
          {
                              new SQLiteParameter() { ParameterName = "@Islottery", DbType = DbType.Boolean, Value = 1 },
          };
            using (IDataReader dReader = SQLiteHelper.ExecuteReader(sql, paras))
            {
                List<Uses> userlist = GlobalFunction.GetEntityList<Uses>(dReader);

                Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(userlist));

            }
           

        }
        //页面列表
        void QueueUserlistpage()
        {
            var pageIndex = 1;
            var pageSize = 10;
            var strsql = string.Format("select * from Users order by ID limit {0} offset {0}*{1}", pageSize, pageIndex - 1);
            using (IDataReader dReader = SQLiteHelper.ExecuteReader(strsql, null))
            {
                List<Uses> userlist = GlobalFunction.GetEntityList<Uses>(dReader);

                Console.WriteLine(userlist);
            }
        }
        //更新
        void UpdateUserbyID()
        {
            var objOrderinfo = new Uses
            {
                ID = 7,
                Islottery = 1
            };
            var strsql = @"update Users set Islottery=@Islottery where ID=@ID";
            try
            {
                var paras = new SQLiteParameter[]
           {

                    new SQLiteParameter() { ParameterName = "@ID", DbType = DbType.Int64, Value =objOrderinfo.ID },
                              new SQLiteParameter() { ParameterName = "@Islottery", DbType = DbType.Boolean, Value = objOrderinfo.Islottery },
           };
                string connectstr = "Data Source=D:\\SqlLites.db;Version=3;UseUTF16Encoding=True";
                var data = SQLiteHelper.ExecuteNonQuery(connectstr, strsql, paras);
                Console.WriteLine(data);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        //删除
        void DeleteUserByID()
        {
            var objOrderinfo = new Uses
            {
                ID = 6
            };
            var strsql = @"Delete from Users where ID=@ID";
            try
            {
                var paras = new SQLiteParameter[]
           {

                    new SQLiteParameter() { ParameterName = "@ID", DbType = DbType.Int64, Value =objOrderinfo.ID },
           };
                string connectstr = "Data Source=D:\\SqlLites.db;Version=3;UseUTF16Encoding=True";
                var data = SQLiteHelper.ExecuteNonQuery(connectstr, strsql, paras);
                Console.WriteLine("删除返回结果："+data);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        ////创建一个连接到指定数据库
        //void connectToDatabase()
        //{
        //    m_dbConnection = new SQLiteConnection("Data Source=MyDatabase.sqlite;Version=3;");
        //    m_dbConnection.Open();
        //}
    }

    public class Uses
    {
        public long ID { get; set; }
        public string phone { get; set; }

        public string UserNo { get; set; }

        public string UserName { get; set; }

        public string Department { get; set; }

        public DateTime? CreateTime { get; set; }

        public int Islottery { get; set; }
    }
}
