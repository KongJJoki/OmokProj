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
            nowUserCount++;

            return ERROR_CODE.None;
        }

        public ERROR_CODE RemoveUser(User user)
        {
            if (!CheckUserExist(user))
            {
                return ERROR_CODE.Room_Leave_Fail_Not_In_Room;
            }

            userList.Remove(user);

            if (nowUserCount > 0)
            {
                nowUserCount--;
            }

            return ERROR_CODE.None;
        }

        public bool CheckUserExist(User user)
        {
            return userList.Contains(user);
        }
        public void Broadcast(string exceptSessionId, byte[] packet)
        {
            for (int i = 0; i < userList.Count; i++)
            {
                User user = userList[i];
                if(user.sessionId == exceptSessionId)
                {
                    continue;
                }
                sendFunc(user.sessionId, packet);
            }
        }
        public List<string> GetUserIds()
        {
            List<string> UserIds = new List<string>();

            for(int i = 0; i < nowUserCount; i++)
            {
                UserIds.Add(userList[i].userId);
            }

            return UserIds;
        }
    }
}