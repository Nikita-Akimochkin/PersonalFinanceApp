using System;

namespace PersonalFinanceApp
{
    internal class ActiveUser
    {
        private static int userID;
        private static string email;
        private static string password;
        private static string userName;
        private static int userAccount;


        public int UserID
        {
            get { return userID; }
            set
            {
                if (value > 0) userID = value;
                else Console.WriteLine("Введите корректное значение");
            }
        }

        public string Email
        {
            get { return email; }
            set
            {
                if (!string.IsNullOrEmpty(value)) email = value;
                else Console.WriteLine("Введите корректное значение");
            }
        }

        public string Password
        {
            get { return password; }
            set
            {
                if (!string.IsNullOrEmpty(value)) password = value;
                else Console.WriteLine("Введите корректное значение");
            }
        }

        public string UserName
        {
            get { return userName; }
            set
            {
                if (!string.IsNullOrEmpty(value)) userName = value;
                else Console.WriteLine("Введите корректное значение");
            }
        }

        public int UserAccount
        {
            get { return userAccount; }
            set
            {
                if (value > 0) userAccount = value;
                else Console.WriteLine("Введите корректное значение");
            }
        }
    }
}
