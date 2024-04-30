namespace OmokGameServer
{
    public class UserManager
    {
        int maxUserNumber;
        
        Dictionary<string, User> userDictionary = new Dictionary<string, User>();

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

            /*if(userDictionary.ContainsKey(sessionId))
            {
                return ERROR_CODE.Already_Exist_Session;
            }*/

            User newUser = new User();
            newUser.SetUser(sessionId, userId);
            userDictionary.Add(sessionId, newUser);

            return ERROR_CODE.None;
        }

        public ERROR_CODE RemoveUser(string sessionId)
        {
            bool isRemoveSuccess = userDictionary.Remove(sessionId);

            if(!isRemoveSuccess)
            {
                return ERROR_CODE.RemoveFailNotExistSession;
            }

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
    }
}