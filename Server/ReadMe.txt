【搭建开发环境】

1、安装WebStorm 11
    激活服务器：http://idea.lanyus.com/
    WebStorm的Languages & Frameworks
        JavaScript
            language version 改为 ECMAScript 6
            Prefer Strict Mode 打开
        Node.js and NPM
            Code Assistant 打开

2、安装最新版MongoChef，安装时使用Non-Commercial方式
3、安装Node.js 64bit
    目前版本是5.2.0，请安装这个
    LINUX下如果自动安装版本达不到这个版本号，那就官网的途径安装
    具体文档：https://nodejs.org/en/download/package-manager/
    Ubuntu下执行过程：
        sudo apt-get install -y curl
        sudo apt-get install -y build-essential
        curl -sL https://deb.nodesource.com/setup_5.x | sudo -E bash -
        sudo apt-get install -y nodejs
4、安装Node.js全局插件
    用于编译C++插件
    npm install node-gyp -g
    用于单元测试
    npm install nodeunit -g	
    如果安装比较慢或下载不了依赖包，那就搞个VPN来下载
    如果是LINUX，建议安装桌面版的LINUX，配置VPN更简单
5、由于node-gyp要用python和C++编译器，所以要安装python 2.7和编译工具（Windows下就是VS系列，比如VS2013）
6、运行MongoDB需要VC运行时，请安装vcredist_2013_x64.exe和vcredist_2013_x86.exe
    下载地址：https://www.microsoft.com/zh-CN/download/details.aspx?id=40784

【运行方法】

1、每个项目下的有个config.js，里面简单配置一下
2、启动数据库，双击Win_LaunchDatabase.bat
3、启动全局服，双击Win_LaunchGlobalServer.bat
4、启动游戏服，双击Win_LaunchGameServer.bat

【开发注意】

1、每个JS文件的第一行一定要加上以下以表示用严格模式写JS：
    "use strict";
2、对象成员如果想私有，就加下划线前缀，比如_ok。
3、学习JS，请学习以下东西:
   https://nodejs.org/api/
   http://segmentfault.com/a/1190000000515151
   http://es6.ruanyifeng.com/
   https://developer.mozilla.org/zh-CN/docs/Web/JavaScript
   http://usejsdoc.org/
   http://bluebirdjs.com/docs/api-reference.html
   https://github.com/caolan/nodeunit
4、学习MongoDB，请学习以下东西，不过内容太多，只能找国内快速学习的PPT，然后以下内容当参考：
   https://docs.mongodb.org/manual/
   http://mongodb.github.io/node-mongodb-native/2.0/api/
5、WebStorm添加JS Docs的方法：
   在目标上方，输入/**回车
6、如果处理网络消息的代码不涉及异步或不关心异步结果，也就是没有yield（比如NoThrow后缀的存盘API，不关心存盘结果，读盘就一般要yield等结果），就不要写成coroutine，因为这个效率不高