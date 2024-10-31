using Npgsql;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PersonalFinanceApp
{
    public partial class ReminderWindow : Window
    {
        ActiveUser user = new ActiveUser();
        Reminder reminder = new Reminder();
        DataBaseHelper dbHelper = new DataBaseHelper();

        public ReminderWindow()
        {
            InitializeComponent();
            GetRemindersLoaded();
        }

        #region GotFocus
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string defaultText = textBox.Tag.ToString();
            dbHelper.HandleFocus(textBox, defaultText, true);
        }
        #endregion

        #region LostFocus
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string defaultText = textBox.Tag.ToString();
            dbHelper.HandleFocus(textBox, defaultText, false);
        }
        #endregion

        #region rem click
        private void ReminderAdd_Click(object sender, RoutedEventArgs e)
        {
            reminder.Description = ReminderTextBox.Text;

            // Проверка на пустые поля
            if (string.IsNullOrWhiteSpace(reminder.Description))
            {
                MessageBox.Show("Пожалуйста, напишите, что вам напомнить.");
                return;
            }

            // Вызов метода регистрации пользователя
            ReminderAdd(user.UserID, reminder.Description);
        }
        #endregion 

        #region Reminder Add
        private void ReminderAdd(int userId, string description)
        {
            try
            {
                // Строка подключения к PostgreSQL
                using (NpgsqlConnection connection = dbHelper.GetConnection())
                {
                    // Если Email уникальный, добавляем пользователя
                    string insertQuery = "INSERT INTO reminders (userID, description, registration_date) VALUES (@UserID, @Description, @Registration_date)";
                    using (NpgsqlCommand command = new NpgsqlCommand(insertQuery, connection))
                    {
                        if (description != "Введите что вам напомнить:")
                        {
                            command.Parameters.AddWithValue("UserID", user.UserID);
                            command.Parameters.AddWithValue("Description", reminder.Description);
                            command.Parameters.AddWithValue("Registration_date", DateTime.Now);

                            command.ExecuteNonQuery();

                            MessageBox.Show("Напоминание успешно добавлено!");
                            return;
                        }
                        MessageBox.Show("Введите данные!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при вводе напоминания: {ex.Message}");
            }
        }
        #endregion

        #region Ger load
        private void GetRemindersLoaded()
        {
            var reminderList = GetReminders(user.UserID);
            int i = 1;

            foreach (var r in reminderList)
            {

                YourReminderList.Items.Add(new TextBlock
                {
                    Text = $"{i++}. {r.description}\n {r.date}",
                    Margin = new Thickness(0, 0, 0, 2)
                });
            }
        }
        #endregion

        #region Get remind
        private List<(string description, DateTime date)> GetReminders(int userID)
        {
            var reminderList = new List<(string description, DateTime date)>();

            using (var connection = dbHelper.GetConnection())
            {
                using (var command = new NpgsqlCommand(@"
                    SELECT description, registration_date
                    FROM reminders WHERE userid=@UserId
                    ORDER BY reminderid DESC
                    LIMIT 30", connection))
                {
                    command.Parameters.AddWithValue("UserId", user.UserID);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            reminder.Description = reader.GetString(0);
                            reminder.Date = reader.GetDateTime(1);

                            reminderList.Add((reminder.Description, reminder.Date));
                        }
                    }
                    return reminderList;
                }
            }
        }
        #endregion
    }
}