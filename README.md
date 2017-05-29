# UpYun.NETCore
又拍云 SDK .net Core版  
[如鹏网](http://www.rupeng.com) 杨中科 开发  
Nuget安装指令：   
```C#
Install-Package UpYun.NETCore
```
示例代码：  
```C#
  UpYunClient upyun = new UpYunClient("bucketname", "username", "secret");  
  var a = upyun.WriteFileAsync("/test.txt", new byte[] { 3, 5, 6, 222, 33, 99, 21 }, true).Result;
```