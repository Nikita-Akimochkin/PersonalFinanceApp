using Npgsql;
using System;
using System.Windows;

namespace PersonalFinanceApp
{
    internal class ActiveUser
    {
        private static int userID;
        private static string email;
        private static string password;
        private static string userName;
        private static int userAccount;
        DataBaseHelper dbHelper = new DataBaseHelper();


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

        public int GetUserId(string email)
        {
            UserID = -1; // Значение по умолчанию, если пользователь не найден

            try
            {
                using (NpgsqlConnection connection = dbHelper.GetConnection())
                {
                    using (var command = new NpgsqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = "SELECT id FROM users WHERE email = @Email LIMIT 1";
                        command.Parameters.AddWithValue("Email", Email);

                        // Выполнение запроса и чтение результата
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                UserID = reader.GetInt32(0); // Получаем ID пользователя
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при получении ID пользователя: " + ex.Message);
            }

            return UserID; // Возвращаем ID пользователя или -1, если не найден
        }
    }
}
