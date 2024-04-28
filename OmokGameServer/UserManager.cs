using System.Collections.Generic;
using System.Threading.Tasks;

namespace OmokGameServer
{
    public class User
    {
        string sessionId;
        string userId;

        public void SetUser(string sessionId, string userId)
        {
            this.sessionId = sessionId;
            this.userId = userId;
        }
    }

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
                return ERROR_CODE.Login_User_Count_Limit_Exceed;
            }

            if(userDictionary.ContainsKey(sessionId))
            {
                return ERROR_CODE.Already_Exist_Session;
            }

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
                return ERROR_CODE.Not_Exist_Session;
            }

            return ERROR_CODE.None;
        }
    }
}