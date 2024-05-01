namespace OmokGameServer
{
    public class OmokBoard
    {
        int[,] omokBoard = new int[19, 19];
        public string BlackUserId { get; set; }
        public string WhiteUserId { get; set; }

        int turnCount;



        const int blackStone = 10;
        const int whiteStone = 20;

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

        public void OmokStonePlace(string userId, int posX, int posY)
        {
            if (userId == BlackUserId)
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
    }
}