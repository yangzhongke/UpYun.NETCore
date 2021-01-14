# UpYun.NETCore
又拍云 SDK .net Core版  
[如鹏网](http://www.rupeng.com) 杨中科 开发  
Nuget安装指令：   
```C#
Install-Package UpYun.NETCore
```
示例代码：  
```C#
UpYunClient upyun = new UpYunClient(bucketName, username, password, httpClientFactory);
byte[] bytes = Encoding.UTF8.GetBytes("www.youzack.com");
var a = await upyun.WriteFileAsync("/test.txt", bytes, true);
```