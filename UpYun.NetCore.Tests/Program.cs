using Microsoft.Extensions.DependencyInjection;
using System;
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
            }                
            Console.ReadKey();
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
                Console.WriteLine(fullPath);

                if(item.filetype=="F")
                {
                    await ListAsync(upyun, fullPath);
                }
            }
        }
    }
}