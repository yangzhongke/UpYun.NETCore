using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace UpYun.NETCore
{
    public class UpYunClient
    {
       

        private string bucketname;
        private string username;
        private string password;
        private bool upAuth = false;
        private string api_domain = "v0.api.upyun.com";
        private string DL = "/";
        private Dictionary<string,object> tmp_infos = new Dictionary<string, object>();
        private string file_secret;
        private string content_md5;
        private bool auto_mkdir = false;
        public string version() { return "1.0.1"; }

        /**
        * 初始化 UpYun 存储接口
        * @param $bucketname 空间名称
        * @param $username 操作员名称
        * @param $password 密码
        * return UpYun object
        */
        public UpYunClient(string bucketname, string username, string password)
        {
            this.bucketname = bucketname;
            this.username = username;
            this.password = password;
        }

        /**
        * 切换 API 接口的域名
        * @param $domain {默认 v0.api.upyun.com 自动识别, v1.api.upyun.com 电信, v2.api.upyun.com 联通, v3.api.upyun.com 移动}
        * return null;
        */
        public void setApiDomain(string domain)
        {
            this.api_domain = domain;
        }

        /**
        * 是否启用 又拍签名认证
        * @param upAuth {默认 false 不启用(直接使用basic auth)，true 启用又拍签名认证}
        * return null;
        */
        public void setAuthType(bool upAuth)
        {
            this.upAuth = upAuth;
        }

        private void upyunAuth(ByteArrayContent requestContent,string method,string uri)
        {
            DateTime dt = DateTime.UtcNow;
            string date = dt.ToString("ddd, dd MMM yyyy HH':'mm':'ss 'GMT'", new CultureInfo("en-US"));

            requestContent.Headers.Add("Date", date);
            string body = requestContent.ReadAsStringAsync().Result;
            string auth;
            if (!string.IsNullOrEmpty(body))
                auth = md5(method + '&' + uri + '&' + date + '&' + requestContent.ReadAsByteArrayAsync().Result.Length + '&' + md5(this.password));
            else
                auth = md5(method + '&' + uri + '&' + date + '&' + 0 + '&' + md5(this.password));
            requestContent.Headers.Add("Authorization", "UpYun " + this.username + ':' + auth);
        }

        private string md5(string str)
        {
            using (MD5 m = MD5.Create())
            {
                byte[] s = m.ComputeHash(UnicodeEncoding.UTF8.GetBytes(str));
                string resule = BitConverter.ToString(s);
                resule = resule.Replace("-", "");
                return resule.ToLower();
            }                
        }
        private async Task<bool> DeleteAsync(string path, Dictionary<string,object> headers)
        {
            var resp = await newWorker("DELETE", DL + this.bucketname + path, null, headers);
            if (resp.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        
        private async Task<HttpResponseMessage> newWorker(string method, string Url, byte[] postData, Dictionary<string,object> headers)
        {
            using (HttpClientHandler handler = new HttpClientHandler { UseProxy = false })
            using (HttpClient httpClient = new HttpClient(handler))
            using (ByteArrayContent byteContent = new ByteArrayContent(postData))
            {
                httpClient.BaseAddress = new Uri("http://" + api_domain);
                ;

                if (this.auto_mkdir == true)
                {
                    byteContent.Headers.Add("mkdir", "true");
                    this.auto_mkdir = false;
                }

                if (postData != null)
                {
                    if (this.content_md5 != null)
                    {
                        byteContent.Headers.Add("Content-MD5", this.content_md5);
                        this.content_md5 = null;
                    }
                    if (this.file_secret != null)
                    {
                        byteContent.Headers.Add("Content-Secret", this.file_secret);
                        this.file_secret = null;
                    }
                }

                if (this.upAuth)
                {
                    upyunAuth(byteContent, method, Url);
                }
                else
                {
                    //byteContent.Headers.Add("Authorization", "Basic " +
                    //Convert.ToBase64String(new System.Text.ASCIIEncoding().GetBytes(this.username + ":" + this.password)));
                    var value = Convert.ToBase64String(new System.Text.ASCIIEncoding().GetBytes(this.username + ":" + this.password));
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", value);
                }
                foreach (var kv in headers)
                {
                    byteContent.Headers.Add(kv.Key, kv.Value.ToString());
                }

                HttpResponseMessage responseMsg;
                if ("Get".Equals(method, StringComparison.OrdinalIgnoreCase))
                {
                    responseMsg = await httpClient.GetAsync(Url);
                }
                else if ("Post".Equals(method, StringComparison.OrdinalIgnoreCase))
                {
                    responseMsg = await httpClient.PostAsync(Url, byteContent);
                }
                else if ("PUT".Equals(method, StringComparison.OrdinalIgnoreCase))
                {
                    responseMsg = await httpClient.PutAsync(Url, byteContent);
                }
                else if ("Delete".Equals(method, StringComparison.OrdinalIgnoreCase))
                {
                    responseMsg = await httpClient.DeleteAsync(Url);
                }
                else
                {
                    throw new Exception("未知method：" + method);
                }

                this.tmp_infos = new Dictionary<string, object>();
                foreach (var header in responseMsg.Headers)
                {
                    if (header.Key.Length > 7 && header.Key.Substring(0, 7) == "x-upyun")
                    {
                        this.tmp_infos.Add(header.Key, header.Value);
                    }
                }

                return responseMsg;
            }               
        }

        /**
        * 获取总体空间的占用信息
        * return 空间占用量，失败返回 null
        */

        public async Task<long> GetFolderUsageAsync(string url)
        {
            Dictionary<string,object> headers = new Dictionary<string,object>();
            long size;
            using (var resp = await newWorker("GET", DL + this.bucketname + url + "?usage", null, headers))
            {
                try
                {
                    string strhtml = await resp.Content.ReadAsStringAsync();
                    size = long.Parse(strhtml);
                }
                catch (Exception)
                {
                    size = 0;
                }
            }
            return size;
        }

        /**
           * 获取某个子目录的占用信息
           * @param $path 目标路径
           * return 空间占用量，失败返回 null
           */
        public async Task<long> GetBucketUsageAsync()
        {
            return await GetFolderUsageAsync("/");
        }
        /**
        * 创建目录
        * @param $path 目录路径
        * return true or false
        */
        public async Task<bool> MkDirAsync(string path, bool auto_mkdir)
        {
            this.auto_mkdir = auto_mkdir;
            Dictionary<string,object> headers = new Dictionary<string,object>();
            headers.Add("folder", "create");

            using (var resp = await newWorker("POST", DL + this.bucketname + path, null, headers))
            {
                if (resp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }                
        }

        /**
        * 删除目录
        * @param $path 目录路径
        * return true or false
        */
        public async Task<bool> RmDirAsync(string path)
        {
            Dictionary<string,object> headers = new Dictionary<string,object>();
            return await DeleteAsync(path, headers);
        }

        /**
        * 读取目录列表
        * @param $path 目录路径
        * return array 数组 或 null
        */
        public async Task<List<FolderItem>> ReadDirAsync(string url)
        {
            Dictionary<string,object> headers = new Dictionary<string,object>();
            byte[] a = null;
            using (var resp = await newWorker("GET", DL + this.bucketname + url, a, headers))
            {
                string strhtml = await resp.Content.ReadAsStringAsync();
                strhtml = strhtml.Replace("\t", "\\");
                strhtml = strhtml.Replace("\n", "\\");
                string[] ss = strhtml.Split('\\');
                int i = 0;
                List<FolderItem> list = new List<FolderItem>();
                while (i < ss.Length)
                {
                    FolderItem fi = new FolderItem(ss[i], ss[i + 1], int.Parse(ss[i + 2]), int.Parse(ss[i + 3]));
                    list.Add(fi);
                    i += 4;
                }
                return list;
            }

        }


        /**
        * 上传文件
        * @param $file 文件路径（包含文件名）
        * @param $datas 文件内容 或 文件IO数据流
        * return true or false
        */
        public async Task<bool> WriteFileAsync(string path, byte[] data, bool auto_mkdir)
        {
            Dictionary<string,object> headers = new Dictionary<string,object>();
            this.auto_mkdir = auto_mkdir;
            using (var resp = await newWorker("POST", DL + this.bucketname + path, data, headers))
            {
                if (resp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            
        }
        /**
        * 删除文件
        * @param $file 文件路径（包含文件名）
        * return true or false
        */
        public async Task<bool> DeleteFileAsync(string path)
        {
            Dictionary<string,object> headers = new Dictionary<string,object>();
            return await DeleteAsync(path, headers);
        }

        /**
        * 读取文件
        * @param $file 文件路径（包含文件名）
        * @param $output_file 可传递文件IO数据流（默认为 null，结果返回文件内容，如设置文件数据流，将返回 true or false）
        * return 文件内容 或 null
        */
        public async Task<byte[]> ReadFileAsync(string path)
        {
            Dictionary<string,object> headers = new Dictionary<string,object>();
            byte[] a = null;

            using (var resp = await newWorker("GET", DL + this.bucketname + path, a, headers))
            {
                return await resp.Content.ReadAsByteArrayAsync();
            }                
        }
        /**
        * 设置待上传文件的 Content-MD5 值（如又拍云服务端收到的文件MD5值与用户设置的不一致，将回报 406 Not Acceptable 错误）
        * @param $str （文件 MD5 校验码）
        * return null;
        */
        public void SetContentMD5(string str)
        {
            this.content_md5 = str;
        }
        /**
        * 设置待上传文件的 访问密钥（注意：仅支持图片空！，设置密钥后，无法根据原文件URL直接访问，需带 URL 后面加上 （缩略图间隔标志符+密钥） 进行访问）
        * 如缩略图间隔标志符为 ! ，密钥为 bac，上传文件路径为 /folder/test.jpg ，那么该图片的对外访问地址为： http://空间域名/folder/test.jpg!bac
        * @param $str （文件 MD5 校验码）
        * return null;
        */
        public void SetFileSecret(string str)
        {
            this.file_secret = str;
        }
        /**
        * 获取文件信息
        * @param $file 文件路径（包含文件名）
        * return array('type'=> file | folder, 'size'=> file size, 'date'=> unix time) 或 null
        */
        public async Task<Dictionary<string,object>> GetFileInfoAsync(string file)
        {
            Dictionary<string,object> headers = new Dictionary<string,object>();
            byte[] a = null;
            using (var resp = await newWorker("HEAD", DL + this.bucketname + file, a, headers))
            {
                Dictionary<string, object> ht;
                try
                {
                    ht = new Dictionary<string, object>();
                    ht.Add("type", this.tmp_infos["x-upyun-file-type"]);
                    ht.Add("size", this.tmp_infos["x-upyun-file-size"]);
                    ht.Add("date", this.tmp_infos["x-upyun-file-date"]);
                }
                catch (Exception)
                {
                    ht = new Dictionary<string, object>();
                }
                return ht;
            }            
        }
        //获取上传后的图片信息（仅图片空间有返回数据）
        public object GetWritedFileInfo(string key)
        {
            if (this.tmp_infos == new Dictionary<string,object>()) return "";
            return this.tmp_infos[key];
        }
        //计算文件的MD5码
        public static string md5_file(string pathName)
        {
            string strResult = "";
            string strHashData = "";

            byte[] arrbytHashValue;
            System.IO.FileStream oFileStream = null;

            using (var md5 = MD5.Create())
            {
                oFileStream = new System.IO.FileStream(pathName, System.IO.FileMode.Open,
                         System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);
                arrbytHashValue = md5.ComputeHash(oFileStream);//计算指定Stream 对象的哈希值
                //由以连字符分隔的十六进制对构成的String，其中每一对表示value 中对应的元素；例如“F-2C-4A”
                strHashData = System.BitConverter.ToString(arrbytHashValue);
                //替换-
                strHashData = strHashData.Replace("-", "");
                strResult = strHashData;
                return strResult.ToLower();
            }
        }
    }

}
