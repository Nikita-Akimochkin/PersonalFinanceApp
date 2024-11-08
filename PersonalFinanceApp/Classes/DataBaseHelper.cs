﻿using Npgsql;
using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace PersonalFinanceApp
{
    internal class DataBaseHelper
    {
        string password = Environment.GetEnvironmentVariable("DB_PASSWORD");

        #region All about connection (Get string, Get/Close connection)
        private string GetConnectionString()
        {
            return $"Host=localhost;Username=postgres;Password={password};Database=PersonalFinanceDB";
        }

        public NpgsqlConnection GetConnection()
        {
            var connection = new NpgsqlConnection(GetConnectionString());
            connection.Open();
            return connection;
        }

        public void CloseConnection(NpgsqlConnection connection)
        {
            if (connection != null && connection.State == System.Data.ConnectionState.Open)
                connection.Close();
        }
        #endregion

        #region All about Focus (Get/Lost)
        public void HandleFocus(TextBox textBox, string defaultText, bool isFocused)
        {
            if (isFocused)
            {
                if (textBox.Text == defaultText)
                {
                    textBox.Text = string.Empty;
                    textBox.Foreground = Brushes.Black; // Меняем цвет текста, если нужно
                }
            }
            else
            {
                if (string.IsNullOrEmpty(textBox.Text))
                {
                    textBox.Text = defaultText;
                    textBox.Foreground = Brushes.Gray; // Меняем цвет текста обратно
                }
            }
        }
        #endregion
    }
}
