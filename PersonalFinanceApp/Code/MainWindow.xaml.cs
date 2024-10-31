﻿using System;
using Npgsql;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PersonalFinanceApp
{
    public partial class MainWindow : Window
    {
        ActiveUser user = new ActiveUser();
        DataBaseHelper dbHelper = new DataBaseHelper();

        public MainWindow()
        {
            InitializeComponent();
        }

        #region GotFocus
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox.Text == "Введите Name:" || textBox.Text == "Введите Email:" || textBox.Text == "Введите Password:")
            {
                textBox.Text = "";
                textBox.Foreground = Brushes.Black;
            }
        }
        #endregion

        #region LostFocus
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (string.IsNullOrEmpty(textBox.Text))
            {
                if (textBox.Name == "EmailTextBox" || textBox.Name == "EmailLoginTextBox")
                    textBox.Text = "Введите Email:";
                else if (textBox.Name == "NameTextBox")
                    textBox.Text = "Введите Name:";
                else
                    textBox.Text = "Введите Password:";
                textBox.Foreground = Brushes.Gray;
            }
        }
        #endregion

        #region Registration_Click
        private void Registration_Click(object sender, RoutedEventArgs e)
        {
            user.UserName = NameTextBox.Text;
            user.Email = EmailTextBox.Text;
            user.Password = PasswordTextBox.Text;

            // Проверка на пустые поля
            if (string.IsNullOrWhiteSpace(user.UserName) || string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(user.Password))
            {
                MessageBox.Show("Пожалуйста, заполните все поля!");
                return;
            }

            // Вызов метода регистрации пользователя
            RegisterUser(user.UserName, user.Email, user.Password);
        }
        #endregion

        #region Login_Click
        private void Login_Click(object sender, RoutedEventArgs e)
        {
            user.Email = EmailLoginTextBox.Text;
            user.Password = PasswordLoginTextBox.Text;
            user.UserID = GetUserId(user.Email);

            if (CheckUserCredentials(user.Email, user.Password))
            {
                AppWindow appWindow = new AppWindow();
                Close();
                appWindow.Show();
            }
            else
            {
                MessageBox.Show("Неверные данные! Попробуйте снова.");
            }
        }
        #endregion

        #region SwitchToLogin_Click
        private void SwitchToLogin_Click(object sender, RoutedEventArgs e)
        {
            RegistrationGrid.Visibility = Visibility.Collapsed;
            LoginGrid.Visibility = Visibility.Visible;
        }
        #endregion

        #region SwitchToRegistration_Click
        private void SwitchToRegistration_Click(object sender, RoutedEventArgs e)
        {
            RegistrationGrid.Visibility = Visibility.Visible;
            LoginGrid.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region Register User
        private void RegisterUser(string name, string email, string password)
        {
            try
            {
                // Строка подключения к PostgreSQL
                using (NpgsqlConnection connection = dbHelper.GetConnection())
                {
                    // Проверка, существует ли уже пользователь с таким email
                    string checkEmailQuery = "SELECT COUNT(*) FROM users WHERE email = @Email";
                    using (NpgsqlCommand checkCommand = new NpgsqlCommand(checkEmailQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("Email", user.Email);
                        int userCount = Convert.ToInt32(checkCommand.ExecuteScalar());

                        if (userCount > 0)
                        {
                            MessageBox.Show("Пользователь с таким Email уже существует!");
                            return;
                        }
                    }

                    // Если Email уникальный, добавляем пользователя
                    string insertQuery = "INSERT INTO users (name, email, password, account, registration_date) VALUES (@Name, @Email, @Password, 0, @Registration_date)";
                    using (NpgsqlCommand insertCommand = new NpgsqlCommand(insertQuery, connection))
                    {
                        if (user.UserName != "Введите Name:" && user.Email != "Введите Email:" && user.Password != "Введите Password:")
                        {
                            insertCommand.Parameters.AddWithValue("Name", user.UserName);
                            insertCommand.Parameters.AddWithValue("Email", user.Email);
                            insertCommand.Parameters.AddWithValue("Password", user.Password);  // В идеале нужно хэшировать пароль
                            insertCommand.Parameters.AddWithValue("Registration_date", DateTime.Now);

                            insertCommand.ExecuteNonQuery();

                            MessageBox.Show("Регистрация прошла успешно!");
                            return;
                        }
                        MessageBox.Show("Введите данные!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при регистрации: {ex.Message}");
            }
        }
        #endregion

        #region Check User Credentials
        private bool CheckUserCredentials(string email, string password)
        {
            using (NpgsqlConnection connection = dbHelper.GetConnection())
            {
                string query = "SELECT COUNT(*) FROM users WHERE email = @Email AND password = @Password";
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("Email", user.Email);
                    cmd.Parameters.AddWithValue("Password", user.Password);

                    int userExist = Convert.ToInt32(cmd.ExecuteScalar());
                    return userExist > 0;
                }
            }
        }
        #endregion

        private int GetUserId(string email)
        {
            user.UserID = -1; // Значение по умолчанию, если пользователь не найден

            try
            {
                using (NpgsqlConnection connection = dbHelper.GetConnection())
                {
                    using (var command = new NpgsqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = "SELECT id FROM users WHERE email = @Email LIMIT 1";
                        command.Parameters.AddWithValue("Email", user.Email);

                        // Выполнение запроса и чтение результата
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                user.UserID = reader.GetInt32(0); // Получаем ID пользователя
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при получении ID пользователя: " + ex.Message);
            }

            return user.UserID; // Возвращаем ID пользователя или -1, если не найден
        }
    }
}
