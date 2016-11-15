using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LotteryData
{
   public class GlobalFunction
    {
        /// <summary>
        /// 数据表中的数据转成需要的类型
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="obj">对象</param>
        /// <returns>实例</returns>
        public static T ConvertToType<T>(object obj)
        {
            T t = default(T);

            if (null == obj || Convert.IsDBNull(obj))
                t = default(T);
            else
                t = (T)obj;

            return t;
        }

        /// <summary>
        /// xml验证
        /// </summary>
        /// <param name="xmlString">要验证的字符串</param>
        /// <returns>是否为xml字符串</returns>
        public static bool ValidXmlString(string xmlString)
        {
            try
            {
                XmlDocument xd = new XmlDocument();
                xd.LoadXml(xmlString);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 字符串转为任意枚举值
        /// </summary>
        /// <param name="enumString">枚举值对应的字符串</param>
        /// <returns>枚举值</returns>
        public static T ConvertToStatusEnum<T>(string enumString) where T : struct, IConvertible
        {
            try
            {
                if (!typeof(T).IsEnum)
                    throw new ArgumentException("T 必须是一个枚举类型");

                return (T)Enum.Parse(typeof(T), enumString, true);
            }
            catch
            {
                // 返回枚举值为0的内容
                return default(T);
            }
        }


        /// <summary>
        /// 枚举类型二进制与操作   
        /// 判断当前值是否包含指定值
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="currentValue">当前值</param>
        /// <param name="containValue">指定值</param>
        /// <returns>是否包含</returns>
        /// <exception cref="ArgumentException">类型不是枚举时报错</exception>
        public static bool ContainEnumValue<T>(T currentValue, T containValue) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("T 必须是一个枚举类型");

            return (currentValue.ToInt32(null) & containValue.ToInt32(null)) == containValue.ToInt32(null);
        }


        /// <summary>
        /// 根据DataTable中的数据生成实体类集合
        /// </summary>
        /// <typeparam name="T">实体层的类</typeparam>
        /// <param name="dt">表</param>
        /// <returns>集合</returns>
        public static List<T> GetEntityListByTable<T>(DataTable dt) where T : new()
        {
            List<T> entityList = new List<T>();

            foreach (DataRow dr in dt.Rows)
            {
                T t = (T)Activator.CreateInstance(typeof(T));
                foreach (DataColumn dc in dt.Columns)
                {
                    PropertyInfo field = t.GetType().GetProperty(dc.ColumnName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                    if (field == null)
                        continue;

                    if (null == dr[dc.ColumnName] || Convert.IsDBNull(dr[dc.ColumnName]))
                        field.SetValue(t, null, null);
                    else
                        field.SetValue(t, dr[dc.ColumnName], null);
                }
                entityList.Add(t);
            }


            if (entityList.Count == 0)
                return null;

            return entityList;
        }


        /// <summary>
        /// 根据IDataReader中的数据生成实体类集合
        /// </summary>
        /// <typeparam name="T">实体层的类</typeparam>
        /// <param name="dt">表</param>
        /// <returns>集合</returns>
        public static List<T> GetEntityList<T>(IDataReader dr) where T : new()
        {
            List<T> entityList = new List<T>();

            int fieldCount = -1;

            while (dr.Read())
            {
                if (-1 == fieldCount)
                    fieldCount = dr.FieldCount;

                T t = (T)Activator.CreateInstance(typeof(T));

                for (int i = 0; i < fieldCount; i++)
                {
                    PropertyInfo field = t.GetType().GetProperty(dr.GetName(i), BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                    if (field == null)
                        continue;

                    if (null == dr[i] || Convert.IsDBNull(dr[i]))
                        field.SetValue(t, null, null);
                    else if (dr[i] is decimal)
                        field.SetValue(t, Convert.ToDecimal(dr[i]), null);
                    else if (dr[i] is double || dr[i] is float)
                        field.SetValue(t, float.Parse(dr[i].ToString()), null);
                    else if (dr[i] is bool)
                        field.SetValue(t, Convert.ToInt32(dr[i]), null);
                    else
                        field.SetValue(t, dr[i], null);
                }
                entityList.Add(t);
            }

            dr.Close();

            if (entityList.Count == 0)
                return null;

            return entityList;
        }


        /// <summary>
        /// 序列化并压缩Ilist集合（用于提高网络传送的效率）
        /// </summary>
        /// <param name="datalist">必须是可序列化的对象或集合</param>
        /// <returns>返回压缩后的byte数组用于网间传送</returns>
        public static byte[] CompressList(object datalist)
        {
            try
            {
                if (datalist == null)
                    return null;

                byte[] result;
                using (MemoryStream ms = new MemoryStream())
                {
                    using (GZipStream zs = new GZipStream(ms, CompressionMode.Compress, true))
                    {
                        using (BufferedStream bs = new BufferedStream(zs))
                        {
                            IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                            formatter.Serialize(bs, datalist);
                        }
                    }
                    ms.Flush();
                    result = ms.ToArray();
                }
                return result;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 解压缩并反序列化byte[]
        /// </summary>
        /// <param name="data">需返序列化的数据</param>
        /// <returns>集合对象</returns>
        public static object DecompressList(byte[] data)
        {
            try
            {
                if (data == null)
                    return null;

                using (MemoryStream ms = new MemoryStream(data))
                {
                    using (GZipStream zs = new GZipStream(ms, CompressionMode.Decompress, true))
                    {
                        using (BufferedStream bs = new BufferedStream(zs))
                        {
                            IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                            return formatter.Deserialize(bs);
                        }
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 类型反射转换
        /// </summary>
        /// <typeparam name="TSource">源数据　</typeparam>
        /// <typeparam name="TResult">结果数据　</typeparam>
        /// <param name="source">起始数据　</param>
        /// <returns>result结果　</returns>
        public static TResult ConvertTo<TSource, TResult>(TSource source) where TResult : new()
        {
            TResult result = new TResult();

            Type sourceType = source.GetType();
            Type resultType = result.GetType();

            PropertyInfo[] pis = resultType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            if (null != pis)
            {
                foreach (PropertyInfo pi in pis)
                {
                    string propertyName = pi.Name;
                    PropertyInfo pit = sourceType.GetProperty(propertyName);
                    if (pit != null)
                    {
                        pi.SetValue(result, pit.GetValue(source, null), null);
                    }
                }
            }
            return result;
        }
    }
}
