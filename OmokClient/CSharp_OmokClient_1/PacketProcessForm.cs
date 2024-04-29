using CSCommon;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PacketDefine;
using MemoryPack;

#pragma warning disable CA1416

namespace csharp_test_client
{
    public partial class mainForm
    {
        Dictionary<int, Action<byte[]>> PacketFuncDic = new Dictionary<int, Action<byte[]>>();

        void SetPacketHandler()
        {
            //PacketFuncDic.Add(PACKET_ID.PACKET_ID_ERROR_NTF, PacketProcess_ErrorNotify);
            PacketFuncDic.Add((int)PACKET_ID.LoginRespond, PacketProcess_Loginin);

            PacketFuncDic.Add((int)PACKET_ID.RoomEnterRespond, PacketProcess_RoomEnterResponse);
            PacketFuncDic.Add((int)PACKET_ID.RoomEnterNotify, PacketProcess_RoomNewUserNotify);
            PacketFuncDic.Add((int)PACKET_ID.RoomMemberNotify, PacketProcess_RoomUserListNotify);
            PacketFuncDic.Add((int)PACKET_ID.RoomLeaveRespond, PacketProcess_RoomLeaveResponse);
            PacketFuncDic.Add((int)PACKET_ID.RoomLeaveNotify, PacketProcess_RoomLeaveUserNotify);
            PacketFuncDic.Add((int)PACKET_ID.RoomChatRespond, PacketProcess_RoomChatResponse);
            PacketFuncDic.Add((int)PACKET_ID.RoomChatNotify, PacketProcess_RoomChatNotify);
            //PacketFuncDic.Add(PacketID.ResReadyOmok, PacketProcess_ReadyOmokResponse);
            //PacketFuncDic.Add(PacketID.NtfReadyOmok, PacketProcess_ReadyOmokNotify);
            //PacketFuncDic.Add(PacketID.NtfStartOmok, PacketProcess_StartOmokNotify);
            //PacketFuncDic.Add(PacketID.ResPutMok, PacketProcess_PutMokResponse);
            //PacketFuncDic.Add(PacketID.NTFPutMok, PacketProcess_PutMokNotify);
            //PacketFuncDic.Add(PacketID.NTFEndOmok, PacketProcess_EndOmokNotify);
        }

        void PacketProcess(byte[] packet)
        {
            var header = new MsgPackPacketHeaderInfo();
            header.Read(packet);

            int headerSize = 4;

            var packetID = header.ID;

            ushort packetSize = BitConverter.ToUInt16(packet, 0);

            int bodySize = packetSize - headerSize;

            byte[] bodyData = new byte[bodySize];

            Buffer.BlockCopy(packet, 4, bodyData, 0, bodySize);

            if (PacketFuncDic.ContainsKey(packetID))
            {
                PacketFuncDic[packetID](bodyData);
            }
            else
            {
                DevLog.Write("Unknown Packet Id: " + packetID);
            }
        }

        void PacketProcess_PutStoneInfoNotifyResponse(byte[] bodyData)
        {
            /*var responsePkt = new PutStoneNtfPacket();
            responsePkt.FromBytes(bodyData);

            DevLog.Write($"'{responsePkt.userID}' Put Stone  : [{responsePkt.xPos}] , [{responsePkt.yPos}] ");*/

        }

        void PacketProcess_GameStartResultResponse(byte[] bodyData)
        {
            /*var responsePkt = new GameStartResPacket();
            responsePkt.FromBytes(bodyData);

            if ((ERROR_CODE)responsePkt.Result == ERROR_CODE.NOT_READY_EXIST)
            {
                DevLog.Write($"모두 레디상태여야 시작합니다.");
            }
            else
            {
                DevLog.Write($"게임시작 !!!! '{responsePkt.UserID}' turn  ");
            }*/
        }

        void PacketProcess_GameEndResultResponse(byte[] bodyData)
        {
            /*var responsePkt = new GameResultResPacket();
            responsePkt.FromBytes(bodyData);
            
            DevLog.Write($"'{responsePkt.UserID}' WIN , END GAME ");*/

        }

        void PacketProcess_PutStoneResponse(byte[] bodyData)
        {
            /*var responsePkt = new MatchUserResPacket();
            responsePkt.FromBytes(bodyData);

            if((ERROR_CODE)responsePkt.Result != ERROR_CODE.ERROR_NONE)
            {
                DevLog.Write($"Put Stone Error : {(ERROR_CODE)responsePkt.Result}");
            }

            DevLog.Write($"다음 턴 :  {(ERROR_CODE)responsePkt.Result}");*/

        }




        void PacketProcess_ErrorNotify(byte[] packetData)
        {
            /*var notifyPkt = new ErrorNtfPacket();
            notifyPkt.FromBytes(bodyData);

            DevLog.Write($"에러 통보 받음:  {notifyPkt.Error}");*/
        }


        void PacketProcess_Loginin(byte[] packetData)
        {
            var responsePkt = MemoryPackSerializer.Deserialize<PKTResLogin>(packetData);
            DevLog.Write($"로그인 결과: {(ErrorCode)responsePkt.Result}");
        }

        void PacketProcess_RoomEnterResponse(byte[] packetData)
        {
            var responsePkt = MemoryPackSerializer.Deserialize<PKTResRoomEnter>(packetData);
            DevLog.Write($"방 입장 결과:  {(ErrorCode)responsePkt.Result}");
        }

        void PacketProcess_RoomUserListNotify(byte[] packetData)
        {
            var notifyPkt = MemoryPackSerializer.Deserialize<PKTNTFRoomMember>(packetData);

            for (int i = 0; i < notifyPkt.UserIdList.Count; ++i)
            {
                AddRoomUserList(notifyPkt.UserIdList[i]);
            }

            DevLog.Write($"방의 기존 유저 리스트 받음");
        }

        void PacketProcess_RoomNewUserNotify(byte[] packetData)
        {
            var notifyPkt = MemoryPackSerializer.Deserialize<PKTNTFRoomEnter>(packetData);

            AddRoomUserList(notifyPkt.UserId);

            DevLog.Write($"방에 {notifyPkt.UserId}가 들어왔습니다.");
        }


        void PacketProcess_RoomLeaveResponse(byte[] packetData)
        {
            var responsePkt = MemoryPackSerializer.Deserialize<PKTResRoomLeave>(packetData);

            listBoxRoomUserList.Items.Clear();
            listBoxRoomChatMsg.Items.Clear();

            DevLog.Write($"방 나가기 결과:  {(ErrorCode)responsePkt.Result}");
        }

        void PacketProcess_RoomLeaveUserNotify(byte[] packetData)
        {
            var notifyPkt = MemoryPackSerializer.Deserialize<PKTNTFRoomLeave>(packetData);

            RemoveRoomUserList(notifyPkt.UserId);

            DevLog.Write($"방에서 {notifyPkt.UserId}가 나갔습니다.");
        }


       void PacketProcess_RoomChatResponse(byte[] packetData)
        {
            var responsePkt = MemoryPackSerializer.Deserialize<PKTResRoomChat>(packetData);

            DevLog.Write($"방 채팅 결과:  {(ErrorCode)responsePkt.Result}");
        }


        void PacketProcess_RoomChatNotify(byte[] packetData)
        {
            var notifyPkt = MemoryPackSerializer.Deserialize<PKTNTFRoomChat>(packetData);

            AddRoomChatMessageList(notifyPkt.UserId, notifyPkt.Message);
        }

        void AddRoomChatMessageList(string userID, string message)
        {
            if (listBoxRoomChatMsg.Items.Count > 512)
            {
                listBoxRoomChatMsg.Items.Clear();
            }

            listBoxRoomChatMsg.Items.Add($"[{userID}]: {message}");
            listBoxRoomChatMsg.SelectedIndex = listBoxRoomChatMsg.Items.Count - 1;
        }

        /*void PacketProcess_ReadyOmokResponse(byte[] packetData)
        {
            var responsePkt = MessagePackSerializer.Deserialize<PKTResReadyOmok>(packetData);

            DevLog.Write($"게임 준비 완료 요청 결과:  {(ErrorCode)responsePkt.Result}");
        }

        void PacketProcess_ReadyOmokNotify(byte[] packetData)
        {
            var notifyPkt = MessagePackSerializer.Deserialize<PKTNtfReadyOmok>(packetData);

            if (notifyPkt.IsReady)
            {
                DevLog.Write($"[{notifyPkt.UserID}]님은 게임 준비 완료");
            }
            else
            {
                DevLog.Write($"[{notifyPkt.UserID}]님이 게임 준비 완료 취소");
            }

        }

        void PacketProcess_StartOmokNotify(byte[] packetData)
        {
            var isMyTurn = false;

            var notifyPkt = MessagePackSerializer.Deserialize<PKTNtfStartOmok>(packetData);
            
            if(notifyPkt.FirstUserID == textBoxUserID.Text)
            {
                isMyTurn = true;
            }

            StartGame(isMyTurn, textBoxUserID.Text, GetOtherPlayer(textBoxUserID.Text));

            DevLog.Write($"게임 시작. 흑돌 플레이어: {notifyPkt.FirstUserID}");
        }
        

        void PacketProcess_PutMokResponse(byte[] packetData)
        {
            var responsePkt = MessagePackSerializer.Deserialize<PKTResPutMok>(packetData);

            DevLog.Write($"오목 놓기 실패: {(ErrorCode)responsePkt.Result}");

            //TODO 방금 놓은 오목 정보를 취소 시켜야 한다
        }
        

        void PacketProcess_PutMokNotify(byte[] packetData)
        {
            var notifyPkt = MessagePackSerializer.Deserialize<PKTNtfPutMok>(packetData);

            플레이어_돌두기(true, notifyPkt.PosX, notifyPkt.PosY);

            DevLog.Write($"오목 정보: X: {notifyPkt.PosX},  Y: {notifyPkt.PosY},   알:{notifyPkt.Mok}");
        }
        

        void PacketProcess_EndOmokNotify(byte[] packetData)
        {
            var notifyPkt = MessagePackSerializer.Deserialize<PKTNtfEndOmok>(packetData);

            EndGame();

            DevLog.Write($"오목 GameOver: Win: {notifyPkt.WinUserID}");
        }*/
    }
}
