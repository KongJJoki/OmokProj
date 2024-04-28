namespace OmokGameServer
{
    public enum ERROR_CODE
    {
        None = 0,

        // 전송
        Send_Fail_Session_None = 1000,


        // 로그인
        Login_Fail_User_Count_Limit_Exceed = 2000,
        Login_Fail_Already_Exist_Session = 2001,

        Remove_Fail_Not_Exist_Session = 2002,

        // 방 입장, 퇴장
        Room_Enter_Fail_Invalid_User = 3000,
        Room_Enter_Fail_Already_In_Room = 3001,
        Room_Enter_Fail_Not_Exist_Room = 3002,
        Room_Enter_Fail_User_Count_Limit_Exceed = 3003,

    }
}