using System.Security.Cryptography;
using System;
using System.Text;

namespace Hive_Server
{
    public class Hashing
    {
        const string saltValue = "Omok";

        public static string HashingPassword(string originPassword)
        {
            string saltedPassword = originPassword + saltValue;

            using (SHA256 sha256Hash = SHA256.Create()) // using 구문 안에 SHA256 객체 선언해서 자동으로 dispose 호출
            {
                // 솔트값을 붙인 패스워드를 바이트 배열로 변환하여 해시 계산
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));

                // 바이트 배열을 문자열로 변환
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2")); // 16진수로 변환
                }

                return builder.ToString(); // return이 실행되기 전에 using문이 dispose함
            }
        }
        public static string MakeAuthToken(Int32 userId)
        {
            string saltedToken = userId.ToString() + saltValue;

            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(saltedToken));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }
    }
}