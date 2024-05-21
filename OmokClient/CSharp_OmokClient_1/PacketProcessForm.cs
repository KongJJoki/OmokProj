using CSCommon;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MemoryPack;
using GameServerClientShare;
using System.Threading;

#pragma warning disable CA1416

namespace csharp_test_client
{
    public partial class mainForm
    {
        Dictionary<int, Action<byte[]>> PacketFuncDic = new Dictionary<int, Action<byte[]>>();

        void SetPacketHandler()
        {
            
            //PacketFuncDic.Add(PACKET_ID.PACKET_ID_ERROR_NTF, PacketProcess_ErrorNotify);
            PacketFuncDic.Add((int)PacketID.LoginResponse, PacketProcess_Login);

            PacketFuncDic.Add((int)PacketID.RoomEnterResponse, PacketProcess_RoomEnterResponse);
            PacketFuncDic.Add((int)PacketID.RoomEnterNotify, PacketProcess_RoomNewUserNotify);
            PacketFuncDic.Add((int)PacketID.RoomMemberNotify, PacketProcess_RoomUserListNotify);
            PacketFuncDic.Add((int)PacketID.RoomLeaveResponse, PacketProcess_RoomLeaveResponse);
            PacketFuncDic.Add((int)PacketID.RoomLeaveNotify, PacketProcess_RoomLeaveUserNotify);
            PacketFuncDic.Add((int)PacketID.RoomChatResponse, PacketProcess_RoomChatResponse);
            PacketFuncDic.Add((int)PacketID.RoomChatNotify, PacketProcess_RoomChatNotify);
            PacketFuncDic.Add((int)PacketID.GameReadyResponse, PacketProcess_GameReadyResponse);
            PacketFuncDic.Add((int)PacketID.GameStartResponse, PacketProcess_GameStartResultResponse);
            PacketFuncDic.Add((int)PacketID.GameStartNotify, PacketProcess_GameStartNotify);
            PacketFuncDic.Add((int)PacketID.OmokStonePlaceResponse, PacketProcess_PutMokResponse);
            PacketFuncDic.Add((int)PacketID.OmokStonePlaceNotify, PacketProcess_PutMokNotify);
            PacketFuncDic.Add((int)PacketID.OmokWinNotify, PacketProcess_WinOmokNotify);
            PacketFuncDic.Add((int)PacketID.OmokLoseNotify, PacketProcess_LoseOmokNotify);
            PacketFuncDic.Add((int)PacketID.TurnChangeNotify, PacketProcess_TurnChangeNotify);
            //PacketFuncDic.Add((int)PACKET_ID.HeartBeatRequestToClient, PacketProcess_HeartbeatReqFromServer);
            PacketFuncDic.Add((int)PacketID.ForceDisconnect, PacketProcess_ForceDisconnected);
            PacketFuncDic.Add((int)PacketID.OmokForceFinish, PacketProcess_ForceGameFinish);
            //PacketFuncDic.Add(PacketID.NtfReadyOmok, PacketProcess_ReadyOmokNotify);
            //PacketFuncDic.Add(PacketID.NtfStartOmok, PacketProcess_StartOmokNotify);
            //PacketFuncDic.Add(PacketID.ResPutMok, PacketProcess_PutMokResponse);
            //PacketFuncDic.Add(PacketID.NTFPutMok, PacketProcess_PutMokNotify);
            //PacketFuncDic.Add(PacketID.NTFEndOmok, PacketProcess_EndOmokNotify);
        }

        void PacketProcess(byte[] packet)
        {
            var header = new PacketHeaderInfo();
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



        void PacketProcess_ErrorNotify(byte[] packetData)
        {
            /*var notifyPkt = new ErrorNtfPacket();
            notifyPkt.FromBytes(bodyData);

            DevLog.Write($"에러 통보 받음:  {notifyPkt.Error}");*/
        }

        void PacketProcess_ForceDisconnected(byte[] packetData)
        {
            heartBeatTimer.Dispose();
            btnMatching.Enabled = true;
            btnMatching.Text = "Matching";
            DevLog.Write("Force Disconnected");
        }

        void PacketProcess_Login(byte[] packetData)
        {
            var responsePkt = MemoryPackSerializer.Deserialize<PKTResLogin>(packetData);
            DevLog.Write($"로그인 결과: {responsePkt.Result}");
        }

        void PacketProcess_RoomEnterResponse(byte[] packetData)
        {
            var responsePkt = MemoryPackSerializer.Deserialize<PKTResRoomEnter>(packetData);
            DevLog.Write($"방 입장 결과:  {responsePkt.Result}");
        }

        void PacketProcess_RoomUserListNotify(byte[] packetData)
        {
            var notifyPkt = MemoryPackSerializer.Deserialize<PKTNTFRoomMember>(packetData);

            for (int i = 0; i < notifyPkt.UidList.Count; ++i)
            {
                AddRoomUserList(notifyPkt.UidList[i]);
            }

            DevLog.Write($"방의 기존 유저 리스트 받음");
        }

        void PacketProcess_RoomNewUserNotify(byte[] packetData)
        {
            var notifyPkt = MemoryPackSerializer.Deserialize<PKTNTFRoomEnter>(packetData);

            AddRoomUserList(notifyPkt.Uid);

            DevLog.Write($"방에 {notifyPkt.Uid}가 들어왔습니다.");
        }


        void PacketProcess_RoomLeaveResponse(byte[] packetData)
        {
            var responsePkt = MemoryPackSerializer.Deserialize<PKTResRoomLeave>(packetData);

            listBoxRoomUserList.Items.Clear();
            listBoxRoomChatMsg.Items.Clear();

            DevLog.Write($"방 나가기 결과:  {responsePkt.Result}");
        }

        void PacketProcess_RoomLeaveUserNotify(byte[] packetData)
        {
            var notifyPkt = MemoryPackSerializer.Deserialize<PKTNTFRoomLeave>(packetData);

            RemoveRoomUserList(notifyPkt.Uid);

            DevLog.Write($"방에서 {notifyPkt.Uid}가 나갔습니다.");
        }


       void PacketProcess_RoomChatResponse(byte[] packetData)
        {
            var responsePkt = MemoryPackSerializer.Deserialize<PKTResRoomChat>(packetData);

            DevLog.Write($"방 채팅 결과:  {responsePkt.Result}");
        }


        void PacketProcess_RoomChatNotify(byte[] packetData)
        {
            var notifyPkt = MemoryPackSerializer.Deserialize<PKTNTFRoomChat>(packetData);

            AddRoomChatMessageList(notifyPkt.Uid, notifyPkt.Message);
        }

        void AddRoomChatMessageList(Int32 uid, string message)
        {
            if (listBoxRoomChatMsg.Items.Count > 512)
            {
                listBoxRoomChatMsg.Items.Clear();
            }

            listBoxRoomChatMsg.Items.Add($"[{uid}]: {message}");
            listBoxRoomChatMsg.SelectedIndex = listBoxRoomChatMsg.Items.Count - 1;
        }

        void PacketProcess_GameReadyResponse(byte[] packetData)
        {
            var responsePkt = MemoryPackSerializer.Deserialize<PKTResGameReady>(packetData);

            DevLog.Write($"게임 준비 완료 요청 결과:  {responsePkt.Result}");
        }

        void PacketProcess_GameStartResultResponse(byte[] packetData)
        {
            var responsePkt = MemoryPackSerializer.Deserialize<PKTResGameStart>(packetData);

            DevLog.Write($"게임 준비 완료 요청 결과:  {responsePkt.Result}");
        }

        void PacketProcess_GameStartNotify(byte[] packetData)
        {
            var isMyTurn = false;

            var notifyPkt = MemoryPackSerializer.Deserialize<PKTNTFGameStart>(packetData);

            if (notifyPkt.StartUserUid.ToString() == textBoxSocketID.Text)
            {
                isMyTurn = true;
            }

            StartGame(isMyTurn, textBoxSocketID.Text.ToInt32(), GetOtherPlayer(textBoxSocketID.Text.ToInt32()));

            DevLog.Write($"게임 시작. 흑돌 플레이어: {notifyPkt.StartUserUid}");
        }

        void PacketProcess_PutMokResponse(byte[] packetData)
        {
            var responsePkt = MemoryPackSerializer.Deserialize<PKTResOmokStonePlace>(packetData);
            
            DevLog.Write($"오목 돌 두기 요청 결과:  {responsePkt.Result}");
        }


        void PacketProcess_PutMokNotify(byte[] packetData)
        {
            var notifyPkt = MemoryPackSerializer.Deserialize<PKTNTFOmokStonePlace>(packetData);

            플레이어_돌두기(notifyPkt.NextTurnUserUid, notifyPkt.PosX, notifyPkt.PosY);

            if (notifyPkt.NextTurnUserUid.ToString() == textBoxSocketID.Text)
            {
                IsMyTurn = true;
                DevLog.Write($"오목 정보: {EnemyStoneColor}돌 X: {notifyPkt.PosX},  Y: {notifyPkt.PosY},   다음 턴:{notifyPkt.NextTurnUserUid}");
            }
            if(notifyPkt.NextTurnUserUid.ToString() != textBoxSocketID.Text)
            {
                IsMyTurn = false;
                DevLog.Write($"오목 정보: {MyStoneColor}돌 X: {notifyPkt.PosX},  Y: {notifyPkt.PosY},   다음 턴:{notifyPkt.NextTurnUserUid}");
            }

        }

        void PacketProcess_WinOmokNotify(byte[] packetData)
        {
            var notifyPkt = MemoryPackSerializer.Deserialize<PKTNTFOmokWin>(packetData);

            EndGame();

            DevLog.Write($"오목 GameOver: Win: {notifyPkt.WinUserUid}");
        }

        void PacketProcess_LoseOmokNotify(byte[] packetData)
        {
            var notifyPkt = MemoryPackSerializer.Deserialize<PKTNTFOmokLose>(packetData);

            EndGame();

            DevLog.Write($"오목 GameOver: Lose: {notifyPkt.LoseUserUid}");
        }

        void PacketProcess_TurnChangeNotify(byte[] packetData)
        {
            var notifyPkt = MemoryPackSerializer.Deserialize<PKTNTFTurnChange>(packetData);

            if(notifyPkt.TurnGetUserUid.ToString() == textBoxSocketID.Text)
            {
                IsMyTurn = true;
            }
            else
            {
                IsMyTurn = false;
            }

            DevLog.Write($"{notifyPkt.TurnGetUserUid}의 턴으로 바뀌었습니다.");
        }

/*        void PacketProcess_HeartbeatReqFromServer(byte[] packetData)
        {
            var heartbeatToServer = new PKTHeartBeatFromClient();
            var packet = MemoryPackSerializer.Serialize(heartbeatToServer);

            PostSendPacket((short)PACKET_ID.HeartBeatResponseFromClient, packet);
        }*/

        void PacketProcess_ForceGameFinish(byte[] packetData)
        {
            var notifyPkt = MemoryPackSerializer.Deserialize<PKTNTFForceGameFinish>(packetData);

            EndGame();

            DevLog.Write("서버에 의해 게임이 강제 종료되었습니다.");
        }


        /*void PacketProcess_ReadyOmokNotify(byte[] packetData)
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
