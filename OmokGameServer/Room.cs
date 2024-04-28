namespace OmokGameServer
{
    public class Room
    {
        public int RoomNumber { get; set; }

        public int nowUserCount;
        int MaxUserNumber;

        public static Func<string, byte[], bool> sendFunc;

        List<User> userList = new List<User>();

        public void Init(int roomNum, int maxUserNum)
        {
            RoomNumber = roomNum;
            MaxUserNumber = maxUserNum;
            nowUserCount = 0;
        }

        public ERROR_CODE AddUser(User user)
        {
            if (nowUserCount >= MaxUserNumber)
            {
                return ERROR_CODE.Room_Enter_Fail_User_Count_Limit_Exceed;
            }

            if (CheckUserExist(user))
            {
                return ERROR_CODE.Room_Enter_Fail_Already_In_Room;
            }

            userList.Add(user);

            return ERROR_CODE.None;
        }

        public bool RemoveUser(User user)
        {
            if (!CheckUserExist(user))
            {
                return false;
            }

            userList.Remove(user);

            return true;
        }

        public bool CheckUserExist(User user)
        {
            return userList.Contains(user);
        }
        public void Broadcast(byte[] packet)
        {
            for (int i = 0; i < userList.Count; i++)
            {
                User user = userList[i];
                sendFunc(user.sessionId, packet);
            }
        }
    }
}