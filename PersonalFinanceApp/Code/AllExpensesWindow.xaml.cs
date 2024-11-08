﻿using Npgsql;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace PersonalFinanceApp
{
    public partial class AllExpensesWindow : Window
    {
        ActiveUser user = new ActiveUser();
        Transaction transaction = new Transaction();
        DataBaseHelper dbHelper = new DataBaseHelper();

        public AllExpensesWindow()
        {
            InitializeComponent();
            LoadAllExpenses();
        }

        #region Get and Load all user expenses

        // Loads all user costs and displays them in the interface
        private void LoadAllExpenses()
        {
            var allCategories = GetAllExpenses(user.UserID);
            int i = 1;

            foreach (var category in allCategories)
            {

                AllExpensesList.Items.Add(new TextBlock
                {
                    Text = $"{i++}. {category.name} - {category.amount}",
                    Margin = new Thickness(0, 0, 0, 4)
                });

            }
        }

        // Gets all user costs from the database
        private List<(string name, int amount)> GetAllExpenses(int userId)
        {
            var categories = new List<(string name, int amount)>();

            try
            {
                using (NpgsqlConnection connection = dbHelper.GetConnection())
                {
                    using (var command = new NpgsqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = @"
                            SELECT category, SUM(amount) AS total_amount
                            FROM transactions
                            WHERE userId = @UserId AND type = 'Expense'
                            GROUP BY category
                            ORDER BY total_amount DESC
                            LIMIT 8";

                        command.Parameters.AddWithValue("UserId", user.UserID);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                                categories.Add((reader.GetString(0), reader.GetInt32(1)));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handles exceptions by displaying an error message
                MessageBox.Show("Error in getting the top costs: " + ex.Message);
            }

            return categories;
        }
        #endregion
    }
}
