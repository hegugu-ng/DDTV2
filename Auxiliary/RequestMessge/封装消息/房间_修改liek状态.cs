﻿using Auxiliary.RequestMessage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using static Auxiliary.RequestMessage.MessageClass;
using static Auxiliary.RequestMessage.Room;
using static Auxiliary.RoomInit;

namespace Auxiliary.RequestMessage.封装消息
{
    public class 房间_修改liek状态
    {
        public static string 修改Like配置(string RoomId, bool RecStatus)
        {
            int roomId = 0;
            try
            {
                string roomDD = bilibili.根据房间号获取房间信息.获取真实房间号(RoomId);
                if (!string.IsNullOrEmpty(roomDD))
                {
                    roomId = int.Parse(roomDD);
                }
            }
            catch (Exception)
            {
                return ReturnInfoPackage.InfoPkak((int)ServerSendMessageCode.请求成功但出现了错误, new List<RoomLiekInfo>() {new RoomLiekInfo()
                    {
                        result=false
                    }}, "输入的直播间房间号不符合房间号规则(数字)");
            }
            var data = new List<RoomCadr>();
            foreach (var item in bilibili房间主表)
            {
                if (item.唯一码 == roomId.ToString())
                {
                    data.Add(new RoomCadr
                    {
                        LiveStatus = item.直播状态,
                        Name = item.名称,
                        OfficialName = item.原名,
                        RoomNumber = item.唯一码,
                        VideoStatus = item.是否录制,
                        Types = item.平台,
                        RemindStatus = item.是否提醒,
                        status = false,
                        Like = RecStatus
                    });
                }
                else
                {
                    data.Add(new RoomCadr
                    {
                        LiveStatus = item.直播状态,
                        Name = item.名称,
                        OfficialName = item.原名,
                        RoomNumber = item.唯一码,
                        VideoStatus = item.是否录制,
                        Types = item.平台,
                        RemindStatus = item.是否提醒,
                        status = false,
                        Like = item.Like
                    });
                }
            }
            //RoomInit.根据唯一码获取直播状态(房间表[i].唯一码)
            string JOO = JsonConvert.SerializeObject(new RoomBox() { data = data });
            MMPU.储存文本(JOO, RoomConfigFile, true);
            InitializeRoomList(0, false, false);
            return ReturnInfoPackage.InfoPkak((int)ServerSendMessageCode.请求成功, new List<RoomLiekInfo>() {new RoomLiekInfo()
                    {
                        result=true,
                        message="修改设置完成"
                    }});
        }
    }
}
