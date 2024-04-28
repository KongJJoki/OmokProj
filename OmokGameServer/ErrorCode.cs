namespace OmokGameServer
{
    public enum ERROR_CODE
    {
        None = 0,

        // 전송
        Send_Fail_Session_None = 1000,


        // 로그인
        Login_User_Count_Limit_Exceed = 2000,
        Already_Exist_Session = 2001,

        Not_Exist_Session = 2002,
    }
}