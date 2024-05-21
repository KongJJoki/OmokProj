using MemoryPack;
using PacketDefine;
using InPacketTypes;
using SockInternalPacket;

namespace OmokGameServer
{
    public class OmokBoard
    {
        int[,] omokBoard = new int[19, 19];
        public int BlackUserUid { get; set; }
        public int WhiteUserUid { get; set; }

        Action<ServerPacketData> pushPacketInProcessorFunc;

        const int blackStone = 10;
        const int whiteStone = 20;

        public void Init(int blackUserUid, int whiteUserUid, Action<ServerPacketData> pushPacketInProcessorFunc)
        {
            this.BlackUserUid = blackUserUid;
            this.WhiteUserUid = whiteUserUid;
            this.pushPacketInProcessorFunc = pushPacketInProcessorFunc;
        }

        public void ClearOmokBoard()
        {
            Array.Clear(omokBoard, 0, omokBoard.Length);
        }

        public bool CheckStoneExist(int posX, int posY)
        {
            if (omokBoard[posX, posY] == 10 || omokBoard[posX, posY] == 20)
            {
                return true;
            }

            return false;
        }

        public void OmokStonePlace(int uid, int posX, int posY)
        {
            if (uid == BlackUserUid)
            {
                omokBoard[posX, posY] = blackStone;
            }
            else
            {
                omokBoard[posX, posY] = whiteStone;
            }
        }

        public bool OmokWinCheck(int posX, int posY)
        {
            if(OmokCheckVertical(posX, posY))
            {
                return true;
            }
            else if(OmokCheckHorizontal(posX, posY))
            {
                return true;
            }
            else if(OmokCheckRightUpDiagonal(posX, posY))
            {
                return true;
            }
            else if(OmokCheckLeftUpDiagonal(posX, posY))
            {
                return true;
            }

            return false;
        }

        bool OmokCheckVertical(int posX, int posY)
        {
            int count = 1;

            for (int i = 1; i < 5; i++)
            {
                if (posX + i <= 18 && omokBoard[posX + i, posY] == omokBoard[posX, posY])
                {
                    count++;
                }
                else
                {
                    break;
                }
            }

            for (int i = 1; i <= 5; i++)
            {
                if (posX - i >= 0 && omokBoard[posX - i, posY] == omokBoard[posX, posY])
                {
                    count++;
                }
                else
                {
                    break;
                }
            }

            if (count >= 5)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        bool OmokCheckHorizontal(int posX, int posY)
        {
            int count = 1;

            for (int i = 1; i <= 5; i++)
            {
                if (posY + i <= 18 && omokBoard[posX, posY + i] == omokBoard[posX, posY])
                {
                    count++;
                }
                else
                {
                    break;
                }
            }

            for (int i = 1; i <= 5; i++)
            {
                if (posY - i >= 0 && omokBoard[posX, posY - i] == omokBoard[posX, posY])
                {
                    count++;
                }
                else
                {
                    break;
                }
            }

            if (count >= 5)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        bool OmokCheckRightUpDiagonal(int posX, int posY)
        {
            int count = 1;

            for (int i = 1; i <= 5; i++)
            {
                if (posX + i <= 18 && posY - i >= 0 && omokBoard[posX + i, posY - i] == omokBoard[posX, posY])
                {
                    count++;
                }
                else
                {
                    break;
                }
            }

            for (int i = 1; i <= 5; i++)
            {
                if (posX - i >= 0 && posY + i <= 18 && omokBoard[posX - i, posY + i] == omokBoard[posX, posY])
                {
                    count++;
                }
                else
                {
                    break;
                }
            }

            if (count >= 5)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        bool OmokCheckLeftUpDiagonal(int posX, int posY)
        {
            int count = 1;

            for (int i = 1; i <= 5; i++)
            {
                if (posX + i <= 18 && posY + i <= 18 && omokBoard[posX + i, posY + i] == omokBoard[posX, posY])
                {
                    count++;
                }
                else
                {
                    break;
                }
            }

            for (int i = 1; i <= 5; i++)
            {
                if (posX - i >= 0 && posY - i >= 0 && omokBoard[posX - i, posY - i] == omokBoard[posX, posY])
                {
                    count++;
                }
                else
                {
                    break;
                }
            }

            if (count >= 5)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void GameResultSave(int winUserUid, int loseUserUid)
        {
            InPKTGameResult gameResult = new InPKTGameResult();
            gameResult.WinUserUid = winUserUid;
            gameResult.LoseUseUid = loseUserUid;

            var bodyData = MemoryPackSerializer.Serialize(gameResult);

            ServerPacketData packet = new ServerPacketData();
            packet.SetPacket("", (int)InPacketID.InSaveGameResult, bodyData);

            pushPacketInProcessorFunc(packet);
        }
    }
}