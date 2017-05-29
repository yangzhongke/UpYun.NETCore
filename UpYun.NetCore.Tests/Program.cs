using System;
using UpYun.NETCore;

namespace UpYun.NetCore.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            string bucketName = Console.ReadLine();
            string username = Console.ReadLine();
            string password = Console.ReadLine();
            UpYunClient upyun = new UpYunClient(bucketName, username, password);
            var a = upyun.WriteFileAsync("/test.txt", new byte[] { 3, 5, 6, 222, 33, 99, 21 }, true).Result;
            Console.WriteLine(a);
            Console.ReadKey();
        }
    }
}