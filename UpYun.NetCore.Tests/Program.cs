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
                byte[] bytes = Encoding.UTF8.GetBytes("www.youzack.com");
                var a = await upyun.WriteFileAsync("/test.txt", bytes, true);
                Console.WriteLine(a);
            }                
            Console.ReadKey();
        }
    }
}