﻿using Auxiliary.RequestMessage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using static Auxiliary.RequestMessage.MessageClass;
using static Auxiliary.RequestMessage.Room;
using static Auxiliary.WSServer.WSServer;

namespace Auxiliary.WSServer.CommandParsing
{
    internal class 删除配置中的房间
    {
        internal static string 删除房间配置(string mess)
        {
            RoomInfo Rec = new RoomInfo();
            try
            {
                JObject JO = (JObject)JsonConvert.DeserializeObject(mess);
                Rec.RoomId = JO["RoomId"].ToString();
            }
            catch (Exception)
            {
                return ReturnInfoPackage.InfoPkak<Message<RoomDeleteInfo>>((int)ServerSendMessageCode.请求成功但出现了错误, null, "服务器收到的数据不符合消息解析的必要条件，请检查数据格式");
            }
            return RequestMessage.封装消息.房间_删除房间.删除(Rec.RoomId);
        }
        internal class RoomInfo
        {
            internal string RoomId { set; get; }
        }

    }
}
