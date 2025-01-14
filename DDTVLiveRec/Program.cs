﻿using Auxiliary;
using Auxiliary.Upload;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DDTVLiveRec
{
    class Program
    {
        static void Main(string[] args)
        {
            Auxiliary.VTBS.API.VTBS服务器CDN.根据CDN更新VTBS_Url();       
            new Task(() =>
            {
                //启动DDTV.CORE核心
                MMPU.配置文件初始化(1);
            }).Start();
           

            new Task(() =>
            {
                //启动WEB服务
                DDTVLiveRecWebServer.Program.Main(new string[] { });
            }).Start();

            bool 是否已检查更新 = false;
            bool 是否已发送过webhook = false;
            #region 检查更新
            new Task(() =>
            {
                while (true)
                {
                    try
                    {
                        update.检查升级程序是否需要升级("rec");
                        string 服务器版本号 = MMPU.TcpSend(Server.RequestCode.GET_GET_DDTVLiveRec_LATEST_VERSION_NUMBER, "{}", true, 50);
                        if (!string.IsNullOrEmpty(服务器版本号))
                        {
                            bool 检测状态 = true;
                            foreach (var item in MMPU.不检测的版本号)
                            {
                                if (服务器版本号 == item)
                                {
                                    检测状态 = false;
                                }
                            }
                            if (MMPU.DDTVLiveRec版本号 != 服务器版本号 && 检测状态)
                            {
                                MMPU.检测到的新版本号 = 服务器版本号;
                                MMPU.更新公告 = MMPU.TcpSend(Server.RequestCode.GET_DDTVLiveRec_UPDATE_ANNOUNCEMENT, "{}", true, 100);
                                MMPU.是否有新版本 = true;
                                InfoLog.InfoPrintf("检测到版本更新，更新内容:\n" + MMPU.更新公告 + "\n\n", InfoLog.InfoClass.下载系统信息);
                                if (!是否已发送过webhook)
                                {
                                    Auxiliary.Webhook.更新推送.更新提示(new Auxiliary.Webhook.更新推送.更新Info()
                                    {
                                        Ver = 服务器版本号,
                                        UpdateMsg = MMPU.更新公告
                                    });
                                    是否已发送过webhook = true;
                                }
                            }
                        }
                        是否已检查更新 = true;
                    }
                    catch (Exception) { }
                    Thread.Sleep(3600 * 1000);
                }
            }).Start();
            #endregion
            int i = 0;
            #region 提示更新
            new Task(() =>
            {
                while (true)
                {                   
                    try
                    {                  
                        if(MMPU.是否有新版本)
                        {
                            if(i<1)
                            {
                                i++;
                            }
                            else
                            {
                                InfoLog.InfoPrintf("检测到版本更新，请运行DDTVLiveRec的update子目录中的update程序进行升级，更新内容:\n" + MMPU.更新公告 + "\n\n", InfoLog.InfoClass.系统强制信息);
                            }
                        }
                    }
                    catch (Exception)
                    {
                     
                    }
                    if(是否已检查更新)
                    {
                        Thread.Sleep(300 * 1000);
                    }
                    else
                    {
                        Thread.Sleep(10 * 1000);
                    }
                    
                }
            }).Start();
            #endregion

            InfoLog.InfoPrintf(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": " + "DDTVLiveRec启动完成", InfoLog.InfoClass.下载系统信息);
            while (true)
            {
                if(Console.ReadKey().Key.Equals(ConsoleKey.I))
                {
                    Console.WriteLine($"请按对应的按键查看或修改配置：\n" +
                        $"a：修改调试模式(控制台会输出API接口调用日志)\n" +
                        $"b：查看WebUserName和WebPassword\n" +
                        $"c：查看WebToken\n" +
                        $"d：查看ApiToken\n" +
                        $"");
                    switch (Console.ReadKey().Key)
                    {
                        case ConsoleKey.A:
                            MMPU.调试模式 = !MMPU.调试模式;
                            Console.WriteLine($"修改调试模式\n修改调试模式为:{ MMPU.调试模式}");
                            break;
                        case ConsoleKey.B:
                            Console.WriteLine($"查看WebUserName和WebPassword\nWebUserName:{ MMPU.WebUserName}\nWebPassword:{MMPU.WebPassword}");
                            break;
                        case ConsoleKey.C:
                            Console.WriteLine($"查看WebToken\nWebToken:{MMPU.WebToken}");
                            break;
                        case ConsoleKey.D:
                            Console.WriteLine($"查看ApiToken\nApiToken:{MMPU.ApiToken}");
                            break;
                        default:
                            Console.WriteLine("没有该操作命令！");
                            break;
                    }  
                }
            }
           
        }
    }
}
