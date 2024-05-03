namespace OmokGameServer
{
    public class UserManager
    {
        int maxUserNumber;
        
        Dictionary<string, User> userDictionary = new Dictionary<string, User>();
        List<User> nowConnectUserList = new List<User>();

        public void Init()
        {
            
        }

        public void SetMaxUserNumber(int maxUserNumber)
        {
            this.maxUserNumber = maxUserNumber;
        }
        
        public ERROR_CODE AddUser(string sessionId, string userId)
        {
            if(userDictionary.Count()>=maxUserNumber)
            {
                return ERROR_CODE.LoginFailUserCountLimitExceed;
            }

            User newUser = new User();
            newUser.SetUser(sessionId, userId);
            userDictionary.Add(sessionId, newUser);
            nowConnectUserList.Add(newUser);

            return ERROR_CODE.None;
        }

        public ERROR_CODE RemoveUser(string sessionId)
        {
            bool isRemoveSuccess = userDictionary.Remove(sessionId);

            if(!isRemoveSuccess)
            {
                return ERROR_CODE.RemoveFailNotExistSession;
            }

            nowConnectUserList.Remove(GetUser(sessionId));
            return ERROR_CODE.None;
        }

        public ERROR_CODE CheckUserExist(string sessionId)
        {
            bool isUserExist = userDictionary.ContainsKey(sessionId);
            
            if(isUserExist)
            {
                return ERROR_CODE.LoginFailAlreadyExistSession;
            }

            return ERROR_CODE.None;
        }
        public User GetUser(string sessionId)
        {
            return userDictionary[sessionId];
        }
        public List<User> GetNowConnectUsers()
        {
            return nowConnectUserList;
        }
    }
}