using CSCommon;
using MemoryPack;
using MessagePack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using System.Windows.Forms;
using PacketDefine;
using System.Net.Http;
using OmokClient.DTO;
using System.Text.Json;
using System.Text;
using OmokClient.CSCommon;
using Windows.Services.Maps;
using static System.Net.Mime.MediaTypeNames;

#pragma warning disable CA1416

namespace csharp_test_client
{
    [SupportedOSPlatform("windows10.0.177630")]
    public partial class mainForm : Form
    {
        ClientSimpleTcp Network = new ClientSimpleTcp();

        bool IsNetworkThreadRunning = false;
        bool IsBackGroundProcessRunning = false;

        bool isNowMatchingInProgress = false;
        System.Threading.Timer matchingCheckTimer;

        System.Threading.Thread NetworkReadThread = null;
        System.Threading.Thread NetworkSendThread = null;
        System.Threading.Timer heartBeatTimer;

        PacketBufferManager PacketBuffer = new PacketBufferManager();
        ConcurrentQueue<byte[]> RecvPacketQueue = new ConcurrentQueue<byte[]>();
        ConcurrentQueue<byte[]> SendPacketQueue = new ConcurrentQueue<byte[]>();

        System.Windows.Forms.Timer dispatcherUITimer = new();

        HttpClient httpClient;

        public mainForm()
        {
            InitializeComponent();
        }

        private void mainForm_Load(object sender, EventArgs e)
        {
            PacketBuffer.Init((8096 * 10), MsgPackPacketHeaderInfo.HeadSize, 2048);

            IsNetworkThreadRunning = true;
            NetworkReadThread = new System.Threading.Thread(this.NetworkReadProcess);
            NetworkReadThread.Start();
            NetworkSendThread = new System.Threading.Thread(this.NetworkSendProcess);
            NetworkSendThread.Start();

            IsBackGroundProcessRunning = true;
            dispatcherUITimer.Tick += new EventHandler(BackGroundProcess);
            dispatcherUITimer.Interval = 100;
            dispatcherUITimer.Start();

            httpClient = new HttpClient();

            //btnDisconnect.Enabled = false;

            SetPacketHandler();


            Omok_Init();
            DevLog.Write("프로그램 시작 !!!", LOG_LEVEL.INFO);
        }

        private void mainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            IsNetworkThreadRunning = false;
            IsBackGroundProcessRunning = false;

            Network.Close();
        }

        private void btnSocketConnect_Click(object sender, EventArgs e)
        {
            string address = textBoxSocketIP.Text;

            int port = Convert.ToInt32(textBoxSocketPort.Text);

            if (Network.Connect(address, port))
            {
                labelStatus.Text = string.Format("{0}. 서버에 접속 중", DateTime.Now);

                DevLog.Write($"서버에 접속 중", LOG_LEVEL.INFO);

                heartBeatTimer = new System.Threading.Timer(HeartBeatToServer, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));

                // 로그인 요청
                SockLoginReq();
            }
            else
            {
                labelStatus.Text = string.Format("{0}. 서버에 접속 실패", DateTime.Now);
            }

            PacketBuffer.Clear();
        }

        private void SocketConnect()
        {
            string address = textBoxSocketIP.Text;

            int port = Convert.ToInt32(textBoxSocketPort.Text);

            if (Network.Connect(address, port))
            {
                //labelStatus.Text = string.Format("{0}. 서버에 접속 중", DateTime.Now);

                DevLog.Write($"서버에 접속 중", LOG_LEVEL.INFO);

                heartBeatTimer = new System.Threading.Timer(HeartBeatToServer, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));

                // 로그인 요청
                SockLoginReq();
            }
            else
            {
                //labelStatus.Text = string.Format("{0}. 서버에 접속 실패", DateTime.Now);
            }

            PacketBuffer.Clear();
        }

        void HeartBeatToServer(object state)
        {
            var heartbeatToServer = new PKTHeartBeatFromClient();
            var packet = MemoryPackSerializer.Serialize(heartbeatToServer);

            PostSendPacket((short)PACKET_ID.HeartBeatResponseFromClient, packet);
        }

        /*private void btnDisconnect_Click(object sender, EventArgs e)
        {
            SetDisconnectd();
            Network.Close();
            heartBeatTimer.Dispose();
        }*/



        void NetworkReadProcess()
        {
            while (IsNetworkThreadRunning)
            {
                if (Network.IsConnected() == false)
                {
                    System.Threading.Thread.Sleep(1);
                    continue;
                }

                var recvData = Network.Receive();

                if (recvData != null)
                {
                    PacketBuffer.Write(recvData.Item2, 0, recvData.Item1);

                    while (true)
                    {
                        var data = PacketBuffer.Read();
                        if (data == null)
                        {
                            break;
                        }

                        RecvPacketQueue.Enqueue(data);
                    }
                    //DevLog.Write($"받은 데이터: {recvData.Item2}", LOG_LEVEL.INFO);
                }
                else
                {
                    Network.Close();
                    //SetDisconnectd();
                    DevLog.Write("서버와 접속 종료 !!!", LOG_LEVEL.INFO);
                }
            }
        }

        void NetworkSendProcess()
        {
            while (IsNetworkThreadRunning)
            {
                System.Threading.Thread.Sleep(1);

                if (Network.IsConnected() == false)
                {
                    continue;
                }


                if (SendPacketQueue.TryDequeue(out var packet))
                {
                    Network.Send(packet);
                }
            }
        }


        void BackGroundProcess(object sender, EventArgs e)
        {
            ProcessLog();

            try
            {
                byte[] packet = null;

                if (RecvPacketQueue.TryDequeue(out packet))
                {
                    PacketProcess(packet);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("BackGroundProcess. error:{0}", ex.Message));
            }
        }

        private void ProcessLog()
        {
            // 너무 이 작업만 할 수 없으므로 일정 작업 이상을 하면 일단 패스한다.
            int logWorkCount = 0;

            while (IsBackGroundProcessRunning)
            {
                System.Threading.Thread.Sleep(1);

                string msg;

                if (DevLog.GetLog(out msg))
                {
                    ++logWorkCount;

                    if (listBoxLog.Items.Count > 512)
                    {
                        listBoxLog.Items.Clear();
                    }

                    listBoxLog.Items.Add(msg);
                    listBoxLog.SelectedIndex = listBoxLog.Items.Count - 1;
                }
                else
                {
                    break;
                }

                if (logWorkCount > 8)
                {
                    break;
                }
            }
        }


        /*public void SetDisconnectd()
        {
            if (btnConnect.Enabled == false)
            {
                btnConnect.Enabled = true;
                btnDisconnect.Enabled = false;
            }

            while (true)
            {
                if (SendPacketQueue.TryDequeue(out var temp) == false)
                {
                    break;
                }
            }

            listBoxRoomChatMsg.Items.Clear();
            listBoxRoomUserList.Items.Clear();

            EndGame();

            labelStatus.Text = "서버 접속이 끊어짐";
        }*/

        void PostSendPacket(short packetID, byte[] packetData)
        {
            if (Network.IsConnected() == false)
            {
                DevLog.Write("서버 연결이 되어 있지 않습니다", LOG_LEVEL.ERROR);
                return;
            }

            const int headerSize = 4;
            byte[] bodyBytes = packetData;
            short packetSize = (short)(headerSize + packetData.Length);

            byte[] packet = new byte[packetSize];
            WriteUInt16(packet, 0, packetSize);
            WriteUInt16(packet, 2, packetID);
            Array.Copy(bodyBytes, 0, packet, headerSize, bodyBytes.Length);

            SendPacketQueue.Enqueue(packet);



            /*var header = new MsgPackPacketHeaderInfo();
            header.ID = packetID;

            if (packetData != null)
            {
                header.TotalSize = (UInt16)packetData.Length;
                
                header.Write(packetData);
            }
            else
            {
                packetData = header.Write();
            }

            SendPacketQueue.Enqueue(packetData);*/
        }

        private void WriteUInt16(byte[] bytes, int offset, int value)
        {
            bytes[offset] = (byte)(value & 0xFF);
            bytes[offset + 1] = (byte)((value >> 8) & 0xFF);
        }


        void AddRoomUserList(Int32 userID)
        {
            listBoxRoomUserList.Items.Add(userID);
        }

        void RemoveRoomUserList(Int32 uid)
        {
            object removeItem = null;

            foreach (int user in listBoxRoomUserList.Items)
            {
                if (user == uid)
                {
                    removeItem = user;
                    break;
                }
            }

            if (removeItem != null)
            {
                listBoxRoomUserList.Items.Remove(removeItem);
            }
        }

        int GetOtherPlayer(int myName)
        {
            if (listBoxRoomUserList.Items.Count != 2)
            {
                return 0;
            }

            var firstName = (int)listBoxRoomUserList.Items[0];
            if (firstName == myName)
            {
                return firstName;
            }
            else
            {
                return (int)listBoxRoomUserList.Items[1];
            }
        }

        public async Task<HiveErrorCode> HiveAccountCreateHttpRequest(string httpUrl, string requestBody)
        {
            HttpResponseMessage response = await httpClient.PostAsync(httpUrl,
                new StringContent(requestBody, Encoding.UTF8, "application/json")); // 요청 본문의 문자 인코딩 + 미디어 타입 지정

            if (response.IsSuccessStatusCode) // 성공해서 응답 받은 경우
            {
                string responseBody = await response.Content.ReadAsStringAsync();

                // JsonDoument : JSON 데이터 읽고 파싱 JsonElemnt : JSON 데이터 접근 및 값 가져오기
                JsonDocument jsonDocument = JsonDocument.Parse(responseBody);
                JsonElement jsonResult = jsonDocument.RootElement;

                int resultValue = jsonResult.GetProperty("result").GetInt32();

                if (resultValue != 0) // 인증토큰 유효성 검사 결과가 성공이 아닌 경우
                {
                    return (HiveErrorCode)resultValue;
                }
                else
                {
                    return HiveErrorCode.None;
                }
            }
            else
            {
                return HiveErrorCode.HiveHttpReqFail;
            }
        }

        public async Task<HiveLoginRes> HiveLoginHttpRequest(string httpUrl, string requestBody)
        {
            HttpResponseMessage response = await httpClient.PostAsync(httpUrl,
                new StringContent(requestBody, Encoding.UTF8, "application/json"));

            HiveLoginRes res = new HiveLoginRes();

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();

                JsonDocument jsonDocument = JsonDocument.Parse(responseBody);
                JsonElement jsonResult = jsonDocument.RootElement;

                int resultValue = jsonResult.GetProperty("result").GetInt32();

                if(resultValue != 0)
                {
                    res.Result = (HiveErrorCode)resultValue;
                    res.Uid = 0;
                    res.AuthToken = "";
                }
                else
                {
                    res.Result = HiveErrorCode.None;
                    res.Uid = jsonResult.GetProperty("uid").GetInt32();
                    res.AuthToken = jsonResult.GetProperty("authToken").GetString();
                }
            }
            else
            {
                res.Result = HiveErrorCode.HiveHttpReqFail;
                res.Uid = 0;
                res.AuthToken = "";
            }

            return res;
        }

        // Hive 서버에 계정 생성
        private async void btnRegister_Click(object sender, EventArgs e)
        {
            string accountCreateURL = textBoxHiveIP.Text + "/accountcreate";

            CreateAccountReq req = new CreateAccountReq
            {
                Id = textBoxHiveID.Text,
                password = textBoxHivePW.Text
            };

            string requestBody = JsonSerializer.Serialize(req);

            CreateAccountRes res = new CreateAccountRes();
            res.Result = await HiveAccountCreateHttpRequest(accountCreateURL, requestBody);

            DevLog.Write($"계정 생성 요청 결과 : {res.Result}");
        }

        // Hive 서버에 로그인 요청
        private async void btnHiveLogin_Click(object sender, EventArgs e)
        {
            string hiveLoginURL = textBoxHiveIP.Text + "/HiveLogin";

            HiveLoginReq req = new HiveLoginReq
            {
                Id = textBoxHiveID.Text,
                password = textBoxHivePW.Text
            };

            string requestBody = JsonSerializer.Serialize(req);

            HiveLoginRes res = await HiveLoginHttpRequest(hiveLoginURL, requestBody);
            if(res.Result == HiveErrorCode.None)
            {
                textBoxApiLoginUid.Text = res.Uid.ToString();
                textBoxApiLoginAuthToken.Text = res.AuthToken;
            }

            DevLog.Write($"Hive 로그인 요청 결과 : {res.Result}");
        }

        public async Task<ApiLoginRes> ApiLoginHttpRequest(string httpUrl, string requestBody)
        {
            HttpResponseMessage response = await httpClient.PostAsync(httpUrl,
                new StringContent(requestBody, Encoding.UTF8, "application/json"));

            ApiLoginRes res = new ApiLoginRes();

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();

                // JsonDoument : JSON 데이터 읽고 파싱 JsonElemnt : JSON 데이터 접근 및 값 가져오기
                JsonDocument jsonDocument = JsonDocument.Parse(responseBody);
                JsonElement jsonResult = jsonDocument.RootElement;

                int resultValue = jsonResult.GetProperty("result").GetInt32();

                if (resultValue != 0)
                {
                    res.Result = (ApiErrorCode)resultValue;
                    res.SockIP = "";
                    res.SockPort = "";
                }
                else
                {
                    res.Result = ApiErrorCode.None;
                    res.SockIP = jsonResult.GetProperty("sockIP").GetString();
                    res.SockPort = jsonResult.GetProperty("sockPort").GetString();
                }
            }
            else
            {
                res.Result = ApiErrorCode.GameApiHttpReqFail;
            }

            return res;
        }

        public async Task<ApiMatchReqRes> ApiMatchReqHttpRequest(string httpUrl, string requestBody)
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, httpUrl);

            requestMessage.Headers.Add("Uid", textBoxApiLoginUid.Text);
            requestMessage.Headers.Add("AuthToken", textBoxApiLoginAuthToken.Text);

            requestMessage.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.SendAsync(requestMessage);

            ApiMatchReqRes res = new ApiMatchReqRes();

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();

                // JsonDoument : JSON 데이터 읽고 파싱 JsonElemnt : JSON 데이터 접근 및 값 가져오기
                JsonDocument jsonDocument = JsonDocument.Parse(responseBody);
                JsonElement jsonResult = jsonDocument.RootElement;

                int resultValue = jsonResult.GetProperty("result").GetInt32();

                if (resultValue != 0)
                {
                    res.Result = (ApiErrorCode)resultValue;
                }
                else
                {
                    res.Result = ApiErrorCode.None;
                }
            }
            else
            {
                res.Result = ApiErrorCode.GameApiHttpReqFail;
            }

            return res;
        }


        // GameApi 서버에 로그인 요청
        private async void btnApiLogin_Click(object sender, EventArgs e)
        {
            string apiLoginURL = textBoxApiIP.Text + "/GameLogin";

            ApiLoginReq req = new ApiLoginReq
            {
                Uid = textBoxApiLoginUid.Text.ToInt32(),
                AuthToken = textBoxApiLoginAuthToken.Text
            };

            string requestBody = JsonSerializer.Serialize(req);

            ApiLoginRes res = await ApiLoginHttpRequest(apiLoginURL, requestBody);
            if (res.Result == ApiErrorCode.None)
            {
                //textBoxSocketIP.Text = res.SockIP;
                //textBoxSocketPort.Text = res.SockPort;
                textBoxSocketID.Text = textBoxApiLoginUid.Text;
                textBoxSocketToken.Text = textBoxApiLoginAuthToken.Text;
            }

            DevLog.Write($"Game API 로그인 요청 결과 : {res.Result}");
        }



        // 소켓 로그인 요청
        private void SockLoginReq()
        {
            var loginReq = new PKTReqLogin();
            loginReq.Uid = textBoxSocketID.Text.ToInt32();
            loginReq.AuthToken = textBoxSocketToken.Text;
            var packet = MemoryPackSerializer.Serialize(loginReq);

            PostSendPacket((short)PACKET_ID.LoginRequest, packet);
            DevLog.Write($"로그인 요청:  {textBoxSocketID.Text}, {textBoxSocketToken.Text}");
        }

        // 매칭 요청
        private async void btnMatchRequest_Click(object sender, EventArgs e)
        {
            if (!isNowMatchingInProgress)
            {
                string apiMatchReqURL = textBoxApiIP.Text + "/MatchRequest";

                ApiMatchReq req = new ApiMatchReq();

                string requestBody = JsonSerializer.Serialize(req);

                ApiMatchReqRes res = await ApiMatchReqHttpRequest(apiMatchReqURL, requestBody);

                isNowMatchingInProgress = true;
                btnMatching.Text = "Matching...";
                btnMatching.Enabled = false;
                matchingCheckTimer = new System.Threading.Timer(MatchCheckReq, null, TimeSpan.Zero, TimeSpan.FromSeconds(1)); // 매칭 체크 타이머 시작
                DevLog.Write($"매칭 요청 결과 : {res.Result}");
            }
            else
            {
                DevLog.Write($"이미 매칭 중입니다.");
            }
        }

        // 게임 api에 매칭 체크 Http요청
        public async Task<ApiMatchCheckRes> ApiMatchCheckHttpRequest(string httpUrl, string requestBody)
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, httpUrl);

            requestMessage.Headers.Add("Uid", textBoxApiLoginUid.Text);
            requestMessage.Headers.Add("AuthToken", textBoxApiLoginAuthToken.Text);

            requestMessage.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.SendAsync(requestMessage);

            ApiMatchCheckRes res = new ApiMatchCheckRes();

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();

                JsonDocument jsonDocument = JsonDocument.Parse(responseBody);
                JsonElement jsonResult = jsonDocument.RootElement;

                int resultValue = jsonResult.GetProperty("result").GetInt32();

                if (resultValue == 0)
                {
                    matchingCheckTimer.Dispose();
                    res.Result = ApiErrorCode.None;
                    res.SockIP = jsonResult.GetProperty("sockIP").GetString();
                    res.SockPort = jsonResult.GetProperty("sockPort").GetString();
                    res.RoomNum = jsonResult.GetProperty("roomNum").GetInt32();
                }
                else
                {
                    res.Result = (ApiErrorCode)resultValue;
                    res.SockIP = "";
                    res.SockPort = "";
                    res.RoomNum = 0;
                }
            }
            else
            {
                res.Result = ApiErrorCode.GameApiHttpReqFail;
                res.SockIP = "";
                res.SockPort = "";
                res.RoomNum = 0;
            }

            return res;
        }

        // 매칭 체크 요청
        async void MatchCheckReq(object state)
        {
            string apiMatchCheckURL = textBoxApiIP.Text + "/MatchCheck";

            ApiMatchCheckReq req = new ApiMatchCheckReq();

            string requestBody = JsonSerializer.Serialize(req);

            ApiMatchCheckRes res = await ApiMatchCheckHttpRequest(apiMatchCheckURL, requestBody);
            if (res.Result == ApiErrorCode.None)
            {
                DevLog.Write("매칭 성공");
                UpdateInvokeTextBox(textBoxSocketIP, res.SockIP);
                UpdateInvokeTextBox(textBoxSocketPort, res.SockPort);
                UpdateInvokeTextBox(textBoxRoomNumber, res.RoomNum.ToString());
                isNowMatchingInProgress = false;

                // 소켓 연결
                SocketConnect();

                // 방 입장 요청하기
                RoomEnter();
            }
            else
            {
                return;
            }
        }

        void UpdateInvokeTextBox(Control control, string updateInfo)
        {
            // UI 스레드에게 대리자 실행
            // MethodInvoker : 매개변수, 반환값 없는 대리자

            void UpdateTextBox()
            {
                control.Text = updateInfo;
            }

            control.Invoke(new MethodInvoker(UpdateTextBox));
        }

        // 방 입장 요청
        private void btn_RoomEnter_Click(object sender, EventArgs e)
        {
            var roomEnterReq = new PKTReqRoomEnter();
            roomEnterReq.RoomNumber = textBoxRoomNumber.Text.ToInt16();
            var packet = MemoryPackSerializer.Serialize(roomEnterReq);

            PostSendPacket((short)PACKET_ID.RoomEnterRequest, packet);
            DevLog.Write($"방 입장 요청:  {textBoxRoomNumber.Text} 번");
        }

        private void RoomEnter()
        {
            var roomEnterReq = new PKTReqRoomEnter();
            roomEnterReq.RoomNumber = textBoxRoomNumber.Text.ToInt16();
            var packet = MemoryPackSerializer.Serialize(roomEnterReq);

            PostSendPacket((short)PACKET_ID.RoomEnterRequest, packet);
            DevLog.Write($"방 입장 요청:  {textBoxRoomNumber.Text} 번");
        }

        private void btn_RoomLeave_Click(object sender, EventArgs e)
        {
            var roomLeaveReq = new PKTReqRoomLeave();
            roomLeaveReq.RoomNumber = textBoxRoomNumber.Text.ToInt16();
            var packet = MemoryPackSerializer.Serialize(roomLeaveReq);

            PostSendPacket((short)PACKET_ID.RoomLeaveRequest, packet);
            DevLog.Write($"방 퇴장 요청:  {textBoxRoomNumber.Text} 번");
        }

        private void btnRoomChat_Click(object sender, EventArgs e)
        {
            if (textBoxRoomSendMsg.Text.IsEmpty())
            {
                MessageBox.Show("채팅 메시지를 입력하세요");
                return;
            }

            var requestPkt = new PKTReqRoomChat();
            requestPkt.Message = textBoxRoomSendMsg.Text;
            var packet = MemoryPackSerializer.Serialize(requestPkt);

            PostSendPacket((short)PACKET_ID.RoomChatRequest, packet);
            DevLog.Write($"방 채팅 요청");
        }

        // 게임 준비 요청
        private void button3_Click(object sender, EventArgs e)
        {
            var requestPkt = new PKTReqRoomChat();
            var packet = MemoryPackSerializer.Serialize(requestPkt);

            PostSendPacket((short)PACKET_ID.GameReadyRequest, packet);

            DevLog.Write($"게임 준비 완료 요청");
        }

        // 게임 시작 요청
        private void btn_GameStartClick(object sender, EventArgs e)
        {
            var requestPkt = new PKTReqGameStart();
            var packet = MemoryPackSerializer.Serialize(requestPkt);

            PostSendPacket((short)PACKET_ID.GameStartRequest, packet);

            DevLog.Write($"게임 시작 요청");
        }

        // 돌 두기 요청
        void SendPacketOmokPut(object sender, MouseEventArgs e)
        {
            if (OmokLogic.게임종료 || IsMyTurn == false)
            {
                return;
            }

            int x, y;

            // 왼쪽클릭만 허용
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            x = (e.X - 시작위치 + 10) / 눈금크기;
            y = (e.Y - 시작위치 + 10) / 눈금크기;

            // 바둑판 크기를 벗어나는지 확인
            if (x < 0 || x >= 바둑판크기 || y < 0 || y >= 바둑판크기)
            {
                return;
            }

            var requestPkt = new PKTReqOmokStonePlace();
            requestPkt.PosX = x;
            requestPkt.PosY = y;
            var packet = MemoryPackSerializer.Serialize(requestPkt);

            PostSendPacket((short)PACKET_ID.OmokStonePlaceRequest, packet);

            DevLog.Write($"put stone 요청 : x  [ {x} ], y: [ {y} ] ");
        }

        /*private void btnMatching_Click(object sender, EventArgs e)
        {
            //PostSendPacket(PACKET_ID.MATCH_USER_REQ, null);
            DevLog.Write($"매칭 요청");
        }
*/

        private void listBoxRoomChatMsg_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void textBoxRelay_TextChanged(object sender, EventArgs e)
        {

        }


        private void button1_Click(object sender, EventArgs e)
        {
            AddUser("test1");
            AddUser("test2");
        }

        void AddUser(string userID)
        {
            var value = new PvPMatchingResult
            {
                IP = "127.0.0.1",
                Port = 32451,
                RoomNumber = 0,
                Index = 1,
                Token = "123qwe"
            };
            var saveValue = MessagePackSerializer.Serialize(value);

            var key = "ret_matching_" + userID;

            var redisConfig = new CloudStructures.RedisConfig("omok", "127.0.0.1");
            var RedisConnection = new CloudStructures.RedisConnection(redisConfig);

            var v = new CloudStructures.Structures.RedisString<byte[]>(RedisConnection, key, null);
            var ret = v.SetAsync(saveValue).Result;
        }

        [MessagePackObject]
        public class PvPMatchingResult
        {
            [Key(0)]
            public string IP;
            [Key(1)]
            public UInt16 Port;
            [Key(2)]
            public Int32 RoomNumber;
            [Key(3)]
            public Int32 Index;
            [Key(4)]
            public string Token;
        }
    }
}
