﻿using System;
using System.Collections.Generic;
using System.Text;
using static Auxiliary.RequestMessage.MessageClass;

namespace Auxiliary.RequestMessage.封装消息
{
    public class 下载_获取录制任务详情信息
    {
        public static string 录制任务详情信息(string GUID)
        {
            List<Downloader.DownIofoData> Package = new List<Downloader.DownIofoData>();
            foreach (var item in MMPU.DownList)
            {
                if (GUID == item.DownIofo.事件GUID)
                {
                    Package.Add(item.DownIofo);
                    Package[Package.Count - 1].WC = null;
                }
            }
            return ReturnInfoPackage.InfoPkak((int)ServerSendMessageCode.请求成功, Package);
        }
    }
}
