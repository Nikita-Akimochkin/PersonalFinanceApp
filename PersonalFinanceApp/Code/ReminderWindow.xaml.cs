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
            LoadReminders();
        }

        #region Get/Lost Focus

        // Event handler for getting focus for a text field
        private void TextBox_GetFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string defaultText = textBox.Tag.ToString();
            dbHelper.HandleFocus(textBox, defaultText, true);
        }

        // Focus loss event handler for text field
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string defaultText = textBox.Tag.ToString();
            dbHelper.HandleFocus(textBox, defaultText, false);
        }
        #endregion

        #region Add reminder/Click methods

        // Event handler for the event of pressing the button to add a reminder
        private void ReminderAdd_Click(object sender, RoutedEventArgs e)
        {
            reminder.Description = ReminderTextBox.Text;

            if (string.IsNullOrWhiteSpace(reminder.Description))
            {
                MessageBox.Show("Please post what you would like to be reminded of.");
                return;
            }

            ReminderAdd(user.UserID, reminder.Description);
        }

        // Method for adding a reminder to the database
        private void ReminderAdd(int userId, string description)
        {
            try
            {
                using (NpgsqlConnection connection = dbHelper.GetConnection())
                {
                    string insertQuery = @"
                        INSERT INTO reminders (userID, description, registration_date) 
                        VALUES (@UserID, @Description, @Registration_date)";

                    using (NpgsqlCommand command = new NpgsqlCommand(insertQuery, connection))
                    {
                        if (description != "What you need to reminded of?")
                        {
                            command.Parameters.AddWithValue("UserID", user.UserID);
                            command.Parameters.AddWithValue("Description", reminder.Description);
                            command.Parameters.AddWithValue("Registration_date", DateTime.Today);

                            command.ExecuteNonQuery();

                            MessageBox.Show("Reminder has been successfully added!");
                            return;
                        }
                        MessageBox.Show("Enter the data!");
                    }
                }
            }
            catch (Exception ex)
            {
                // Handles exceptions
                MessageBox.Show($"Error when entering a reminder: {ex.Message}");
            }
        }
        #endregion

        #region Get and Load all user reminders

        // Method for loading and displaying all user reminders
        private void LoadReminders()
        {
            var reminderList = GetReminders(user.UserID);
            int i = 1;

            foreach (var remind in reminderList)
            {

                YourReminderList.Items.Add(new TextBlock
                {
                    Text = $"{i++}. {remind.description}\n {remind.date.ToString("dd.MM.yyyy")}",
                    Margin = new Thickness(0, 0, 0, 2)
                });
            }
        }

        // Method to get the list of reminders from the database
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
                            reminderList.Add((reader.GetString(0), reader.GetDateTime(1)));
                    }

                    return reminderList;
                }
            }
        }
        #endregion
    }
}