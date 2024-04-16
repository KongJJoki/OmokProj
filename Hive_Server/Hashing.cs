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

            using (SHA256 sha256Hash = SHA256.Create()) // using ���� �ȿ� SHA256 ��ü �����ؼ� �ڵ����� dispose ȣ��
            {
                // ��Ʈ���� ���� �н����带 ����Ʈ �迭�� ��ȯ�Ͽ� �ؽ� ���
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));

                // ����Ʈ �迭�� ���ڿ��� ��ȯ
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2")); // 16������ ��ȯ
                }

                return builder.ToString(); // return�� ����Ǳ� ���� using���� dispose��
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