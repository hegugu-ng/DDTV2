﻿using Auxiliary.LiveChatScript;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Auxiliary
{
    public class RoomInit:bilibili
    {
        public static string RoomConfigFile = MMPU.getFiles("RoomConfiguration", "./RoomListConfig.json");
       // public static List<BiliWebSocket> biliWebSocket = new List<BiliWebSocket>();

        public static List<RL> bilibili房间主表 = new List<RL>();
        public static List<RL> 之前的bilibili房间主表状态 = new List<RL>();
        public static int bilibili房间主表长度 = 0;
        public static int bilibili房间信息更新次数 = 0;

        public static List<RL> youtube房间主表 = new List<RL>();
        public static List<RL> 之前的youtube房间主表状态 = new List<RL>();
        public static int youtube房间主表长度 = 0;
        public static int youtube房间信息更新次数 = 0;

        public static bool 首次启动 = true;

        public static int B站更新刷新次数 = 0;
        public static int youtube更新刷新次数 = 0;

        public class RoomInfo
        {
            public bool 是否提醒 { set; get; }
            public string 名称 { set; get; }
            public string 房间号 { set; get; }
            public string 标题 { set; get; }
            public bool 直播状态 { set; get; }
            public bool 轮播状态 { set; get; }
            public string UID { set; get; }
            public string 直播开始时间 { set; get; }
            public bool 是否录制视频 { set; get; }
            public bool 是否录制弹幕 { set; get; }
            public string 原名 { set; get; }
            public string 平台 { set; get; }
            public string youtubeVideoId { set; get; }
            public LiveChatListener liveChatListener { set; get; }
            public bool Like { set; get; }

        }

        public class RL
        {
            public string 名称 { set; get; }
            public string 原名 { set; get; }
            public bool 直播状态 { set; get; }
            public string 平台 { set; get; }
            public bool 是否提醒 { set; get; }
            public bool 是否录制 { set; get; }
            public string 唯一码 { set; get; }
            public bool Like { set; get; }
            public long UID { set; get; }
        }
        public static bool 根据唯一码获取直播状态(string GUID)
        {
            foreach (RL item in bilibili房间主表)
            {
                try
                {
                    if (item.唯一码 == GUID)
                    {
                        if (item.直播状态)
                            return true;
                        else
                            return false;
                    }
                }
                catch (Exception)
                {
                }

            }
            return false;
        }
        new public static void start()
        {
            InitializeRoomConfigFile();
            InitializeRoomList(0,false,false);

            bilibili.start();
            //youtube.start();

            Task.Run(async () =>
            {
                InfoLog.InfoPrintf("开始周期轮询bilibili房间开播状态", InfoLog.InfoClass.Debug);
                while (true)
                {
                    try
                    {
                        刷新B站房间列表();
                        bilibili房间信息更新次数++;
                        await Task.Delay(5 * 1000).ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        InfoLog.InfoPrintf($"在常规主线程B2中发生严重错误，错误堆栈内容:\n{e}",InfoLog.InfoClass.系统错误信息);
                    }
                }
            });
            //Task.Run(async () =>
            //{
            //    InfoLog.InfoPrintf("开始周期轮询youtube频道开播状态", InfoLog.InfoClass.Debug);
            //    while (true)
            //    {
            //        刷新youtube站房间列表();
            //        youtube房间信息更新次数++;
            //        await Task.Delay(5 * 1000).ConfigureAwait(false);
            //    }
            //});
        }
        public static bool 首次房间列表刷新 = true;
        private static void 刷新B站房间列表()
        {
            if (!MMPU.是否能连接阿B)
                return;
            之前的bilibili房间主表状态.Clear();
            List<RL> 临时主表 = new List<RL>();
            foreach (var item in bilibili房间主表)
            {
                if (item.平台 == "bilibili")
                {
                    之前的bilibili房间主表状态.Add(item);
                    临时主表.Add(item);
                }
            }
            int A = 临时主表.Count();
            for (int i = 0; i < A; i++)
            {
                if (临时主表[i].平台 == "bilibili")
                {
                    临时主表.Remove(临时主表[i]);
                    i--;
                }
                A--;
            }
         
           
            foreach (var 最新的状态 in bilibili.RoomList)
            {
                foreach (var 之前的状态 in 之前的bilibili房间主表状态)
                {
                    
                    if (之前的状态.唯一码 == 最新的状态.房间号)
                    {
                        if (首次房间列表刷新)
                        {
                            之前的状态.直播状态 = false;
                        }                    
                        if (B站更新刷新次数 > 5)
                        {
                            if (之前的状态.直播状态 == false && 最新的状态.直播状态 == true && 之前的状态.是否提醒)
                            {
                                MMPU.弹窗.Add(3000, "直播提醒", 最新的状态.名称 + "/" + 最新的状态.原名 + "的直播状态发生了变化");
                            }
                        }
                        if (之前的状态.直播状态 == false && 最新的状态.直播状态 == true && 之前的状态.是否录制 == true)
                        {
                            if (B站更新刷新次数 > 5)
                            {
                                MMPU.弹窗.Add(3000, "自动录制", 最新的状态.名称 + "/" + 最新的状态.原名 + "直播间状态发生了变化，也许是开播了");

                            }
                            if (MMPU.初始化后启动下载提示)
                            {
                                MMPU.初始化后启动下载提示 = !MMPU.初始化后启动下载提示;
                                MMPU.弹窗.Add(3000, "自动录制", "有关注的正在直播,根据配置列表开始自动录制");
                            }

                            InfoLog.InfoPrintf(最新的状态.名称 + "/" + 最新的状态.原名 + "直播间状态发生了变化", InfoLog.InfoClass.下载系统信息);
                            bool 是否新建任务 = true;
                            foreach (var item in MMPU.DownList)
                            {
                                if (item.DownIofo.房间_频道号== 最新的状态.房间号)
                                {
                                    if(item.DownIofo.下载状态 == true && item.DownIofo.是否保存)
                                    {
                                        是否新建任务 = false;
                                        break;
                                    }
                                }
                            }
                            //Console.WriteLine(最新的状态.名称);
                            if (是否新建任务)
                            {
                                new Task(() =>
                                {
                                    string 下载地址 = bilibili.根据房间号获取房间信息.下载地址(之前的状态.唯一码);
                                    if (string.IsNullOrEmpty(下载地址))
                                    {
                                        InfoLog.InfoPrintf("解析下载地址失败，一般是该房间未开播或已加密", InfoLog.InfoClass.下载系统信息);
                                        return;
                                    }
                                    Downloader DLL = Downloader.新建下载对象(之前的状态.平台, 之前的状态.唯一码, bilibili.根据房间号获取房间信息.获取标题(之前的状态.唯一码), Guid.NewGuid().ToString(), 下载地址, "自动录制", true, 最新的状态.名称, false, null);
                                    if (DLL != null)
                                    {
                                        if (string.IsNullOrEmpty(DLL.DownIofo.备注))
                                        {
                                            DLL.DownIofo.备注 = "新建自动录制任务..等待数据..";
                                        }
                                    }

                                }).Start();
                            }
                        }
                        break;

                    }
                }
                int B = 之前的bilibili房间主表状态.Count();
                临时主表.Add(new RL { 名称 = 最新的状态.名称, 唯一码 = 最新的状态.房间号, 平台 = "bilibili", 是否录制 = 最新的状态.是否录制视频, 是否提醒 = 最新的状态.是否提醒, 直播状态 = 最新的状态.直播状态, 原名 = 最新的状态.原名,Like=最新的状态.Like,UID=long.Parse(最新的状态.UID) });
            }
            if(之前的bilibili房间主表状态.Count!=0)
            {
                首次房间列表刷新 = false;
            }
           
            bilibili房间主表.Clear();
            foreach (var item in 临时主表)
            {
                bilibili房间主表.Add(item);

            }
            bilibili房间主表长度 = bilibili房间主表.Count();
            B站更新刷新次数++;
        }
        private static void 刷新youtube站房间列表()
        {
          
            之前的youtube房间主表状态.Clear();
            List<RL> 临时主表 = new List<RL>();
            foreach (var item in youtube房间主表)
            {
                if (item.平台 == "youtube")
                {
                    之前的youtube房间主表状态.Add(item);
                    临时主表.Add(item);
                }
            }
            int A = 临时主表.Count();
            for (int i = 0; i < A; i++)
            {
                if (临时主表[i].平台 == "youtube")
                {
                    临时主表.Remove(临时主表[i]);
                    i--;
                }
                A--;
            }
            foreach (var 最新的状态 in youtube.RoomList)
            {
                foreach (var 之前的状态 in 之前的youtube房间主表状态)
                {

                    if (之前的状态.唯一码 == 最新的状态.房间号)
                    {
                        if (youtube更新刷新次数 > 5)
                        {
                            if (之前的状态.直播状态 == false && 最新的状态.直播状态 == true && 之前的状态.是否提醒)
                            {
                                MMPU.弹窗.Add(3000, "youtube直播提醒", 最新的状态.名称 + "/" + 最新的状态.原名 + "正在直播");
                            }
                        }
                        //if (之前的状态.直播状态 == true && 最新的状态.直播状态 == false && 之前的状态.是否录制 == true)
                        //{
                        //    new Task(() =>
                        //    {
                        //        youtube.使用youtubeDl下载("https://www.youtube.com/watch?v="+ 最新的状态.youtubeVideoId);
                        //        //Downloader.新建下载对象(之前的状态.平台, 最新的状态.youtubeVideoId, 最新的状态.标题, Guid.NewGuid().ToString(), 最新的状态.平台, "自动录制", true, 最新的状态.名称 + "-" + 最新的状态.原名, false, null).DownIofo.备注 = "自动录制下载中";
                        //    }).Start();

                        //}
                        //break;
                    }
                }
                
                临时主表.Add(new RL { 名称 = 最新的状态.名称, 唯一码 = 最新的状态.房间号, 平台 = "youtube", 是否录制 = 最新的状态.是否录制视频, 是否提醒 = 最新的状态.是否提醒, 直播状态 = 最新的状态.直播状态, 原名 = 最新的状态.原名 });
            }
            youtube房间主表.Clear();
            foreach (var item in 临时主表)
            {
                youtube房间主表.Add(item);

            }
            youtube房间主表长度 = youtube房间主表.Count();
            youtube更新刷新次数++;
        }

        /// <summary>
        /// 初始化房间配置文件
        /// </summary>
        public static void InitializeRoomConfigFile()
        {
            try
            {
                ReadConfigFile(RoomConfigFile);
                InfoLog.InfoPrintf("房间配置信息加载完成", InfoLog.InfoClass.Debug);
            }
            catch (Exception)
            {

                File.WriteAllText(RoomConfigFile, "{}");
                InfoLog.InfoPrintf("未检测到房间配置文件，生成一个新的缺省配置文件", InfoLog.InfoClass.Debug);
            }
        }
        /// <summary>
        /// 读取房间配置文件
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string ReadConfigFile(string file)
        {
            if (!File.Exists(file))
                File.Move("./RoomListConfig.ini", file);
            return File.ReadAllText(file);
        }

        public static bool 初始化储存房间储存一次 = true;
        /// <summary>
        /// 初始化房间列表
        /// </summary>
        /// <param name="roomId">有变动的房间号</param>
        /// <param name="R">是否是删除操作</param>
        public static void InitializeRoomList(int roomId, bool Rem,bool Rst)
        {
            InfoLog.InfoPrintf("开始刷新本地房间列表", InfoLog.InfoClass.Debug);
            var rlc = new RoomBox();
            try
            {
                rlc = JsonConvert.DeserializeObject<RoomBox>(ReadConfigFile(RoomConfigFile));
            }
            catch (Exception)
            {
                rlc = JsonConvert.DeserializeObject<RoomBox>("{}");
                InfoLog.InfoPrintf("房间json配置文件格式错误！请检测核对！", InfoLog.InfoClass.系统错误信息);
                InfoLog.InfoPrintf("房间json配置文件格式错误！请检测核对！", InfoLog.InfoClass.系统错误信息);
                InfoLog.InfoPrintf("房间json配置文件格式错误！请检测核对！", InfoLog.InfoClass.系统错误信息);
                return;
            }
            List<RoomCadr> RoomConfigList = new List<RoomCadr>();//房间信息1List
            RoomConfigList = rlc?.data;
            if (RoomConfigList == null)
                RoomConfigList = new List<RoomCadr>();
            bilibili.RoomList.Clear();
            youtube.RoomList.Clear();
            if (初始化储存房间储存一次)
            {
                string JOO = JsonConvert.SerializeObject(rlc);
                MMPU.储存文本(JOO, RoomConfigFile);
                初始化储存房间储存一次 = false;
            }

            foreach (var item in RoomConfigList)
            {
                if (item.Types == "bilibili")
                {
                    //if(首次启动)
                    //{
                       
                    //    BiliWebSocket BWS = new BiliWebSocket();
                    //    BWS.WebSocket(int.Parse(item.RoomNumber));
                    //    biliWebSocket.Add(BWS);
                    //}
                    bilibili.RoomList.Add(new RoomInfo
                    {
                        房间号 = item.RoomNumber,
                        标题 = "",
                        是否录制弹幕 = item.VideoStatus,
                        是否录制视频 = item.VideoStatus,
                        UID = item.UID.ToString(),
                        直播开始时间 = "",
                        名称 = item.Name,
                        直播状态 = item.LiveStatus,
                        原名 = item.OfficialName,
                        是否提醒 = item.RemindStatus,
                        平台="bilibili",
                        Like=item.Like
                    });
                    if (首次启动)
                    {
                        bilibili.RoomList[bilibili.RoomList.Count - 1].直播状态 = false;
                    }
                }
                else if (item.Types == "youtube")
                {

                    youtube.RoomList.Add(new RoomInfo
                    {
                        房间号 = item.RoomNumber,
                        标题 = "",
                        是否录制弹幕 = item.VideoStatus,
                        是否录制视频 = item.VideoStatus,
                        UID = item.UID.ToString(),
                        直播开始时间 = "",
                        名称 = item.Name,
                        直播状态 = item.LiveStatus,
                        原名 = item.OfficialName,
                        是否提醒 = item.RemindStatus,
                        平台="youtube",
                        Like = item.Like
                    });
                    if (首次启动)
                    {
                        youtube.RoomList[youtube.RoomList.Count - 1].直播状态 = false;
                    }
                }
            }
            if (首次启动)
            {
                InfoLog.InfoPrintf("监控列表中有" + (bilibili.RoomList.Count()+ youtube.RoomList.Count()) + "个单推对象，开始监控", InfoLog.InfoClass.下载系统信息);
            }
            首次启动 = false;
            InfoLog.InfoPrintf("刷新本地房间列表完成", InfoLog.InfoClass.Debug);
        }
        public class RoomBox
        {
            public List<RoomCadr> data { get; set; }
        }

        public class RoomCadr
        {
            public string Name { get; set; }//中文名
            public string OfficialName { set; get; } = "";//官方频道名称
            public string RoomNumber { get; set; }//直播间房间号
            public long UID { set; get; } = 0;
            public string Types { get; set; } = "bilibili";//平台类型,现在默认bilibili，不要改成其他的
            public bool status { get; set; } = false;//直播状态缓存，默认为false
            public bool VideoStatus { get; set; } = false;//是否开播自动录制，默认为false
            public bool RemindStatus { get; set; } = false;//是否开播气泡提示，默认false
            public bool LiveStatus { get; set; } = false;//当前的直播状态，默认为false
            public bool Like { set; get; } = false;//用于标记在列表中优先显示、特殊关照的房间

        }
    }
}
