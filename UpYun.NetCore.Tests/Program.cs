using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UpYun.NETCore;

namespace UpYun.NetCore.Tests
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string bucketName = args[0];
            string username = args[1];
            string password = args[2];

            ServiceCollection services = new ServiceCollection();            
            services.AddHttpClient();
            using (var sp = services.BuildServiceProvider())
            {
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                UpYunClient upyun = new UpYunClient(bucketName, username, password, httpClientFactory);
                /*
                byte[] bytes = Encoding.UTF8.GetBytes("www.youzack.com");
                var a = await upyun.WriteFileAsync("/test.txt", bytes, true);
                Console.WriteLine(a);*/
                /*
                var r = await upyun.RenameFileAsync("/02b2b5f0-3484-11e6-81f3-ccb34c23190a%20%5Blow%5D.mp3", "/02b2b5f0-3484-11e6-81f3-ccb34c23190a.mp3");
                Console.WriteLine(r);*/
                 await ListAsync(upyun, "/");
                //var r = await upyun.RenameFileAsync("/电脑02b2b5f0-3484-11e6-81f3-ccb34c23190a [low].mp3", "/电脑02b2b5f0-3484-11e6-81f3-ccb34c23190a.mp3");
                //Console.WriteLine(r);
            }
            Console.ReadLine(); Console.ReadLine();
        }

        static void WriteLog(string s)
        {
            File.AppendAllText("d:/1.txt",s+"\r\n");
        }

        static async Task ListAsync(UpYunClient upyun, string folder)
        {
            var items = await upyun.ReadDirAsync(folder);
            foreach (var item in items.Value)
            {
                string fullPath;
                if(!folder.EndsWith('/'))
                {
                    folder = folder + "/";
                }
                fullPath=folder + item.filename;
                //Console.WriteLine(fullPath);
                if(fullPath.Contains(" [low]"))
                {
                    string newPath = fullPath.Replace(" [low]", "");
                    try
                    {
                        var r = await upyun.RenameFileAsync(fullPath, newPath);
                        if (r.IsOK)
                        {
                            Console.WriteLine("成功：" + fullPath + "," + newPath);
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.BackgroundColor = ConsoleColor.White;
                            Console.WriteLine("失败：" + fullPath + "," + newPath + "," + r);
                            WriteLog("失败：" + fullPath + "," + newPath + "," + r);
                            Console.ResetColor();
                        }
                    }
                    catch(Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.WriteLine("失败：" + fullPath + "," + newPath + "," + ex);
                        WriteLog("失败：" + fullPath + "," + newPath + "," + ex);
                        Console.ResetColor();
                    }
                }
                if(item.filetype=="F")
                {
                    await ListAsync(upyun, fullPath);
                }
            }
        }
    }
}