﻿using Npgsql;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PersonalFinanceApp
{
    public partial class AppWindow : Window
    {
        ActiveUser user = new ActiveUser();
        Transaction transaction = new Transaction();
        DataBaseHelper dbHelper = new DataBaseHelper();

        public AppWindow()
        {
            InitializeComponent();
            LoadUserAccountBalance();
            GetTopExpensesLoaded();
            GetRecentTransactionsLoaded();
        }

        #region Get user account balance and than loading it
        public void LoadUserAccountBalance()
        {
            user.UserAccount = GetUserAccountBalance(user.UserID);
            TotalTextBlock.Text = Convert.ToString(user.UserAccount);
        }

        private int GetUserAccountBalance(int userId)
        {
            try
            {
                using (NpgsqlConnection connection = dbHelper.GetConnection())
                {
                    using (var command = new NpgsqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = "SELECT account FROM users WHERE id = @UserId";
                        command.Parameters.AddWithValue("UserId", user.UserID);

                        // Выполняем запрос и получаем баланс
                        user.UserAccount = Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при получении баланса: " + ex.Message);
            }

            return user.UserAccount;
        }
        #endregion

        #region Transaction Click
        private void Transaction_Click(object sender, RoutedEventArgs e)
        {
            TransactionsWindow transactionsWindow = new TransactionsWindow();
            transactionsWindow.Show();
        }
        #endregion

        #region Get top user expenses, loading them + Full history click
        private void GetTopExpensesLoaded()
        {
            var topExpenses = GetTopExpenses(user.UserID);
            int i = 1;

            foreach (var category in topExpenses)
            {
                TopExpensesList.Items.Add(new TextBlock
                {
                    Text = $"{i++}. {category.name} - {category.amount}",
                    Margin = new Thickness(0, 0, 0, 4)
                });
            }
        }

        private List<(string name, int amount)> GetTopExpenses(int userId)
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
                            WHERE userId = @UserId AND type = 'Расход'
                            GROUP BY category
                            ORDER BY total_amount DESC
                            LIMIT 3";

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
                MessageBox.Show("Ошибка при получении топ расходов: " + ex.Message);
            }

            return categories;
        }

        private void ShowFullHistory_Click(object sender, RoutedEventArgs e)
        {
            FullHistoryWindow fullHistoryWindow = new FullHistoryWindow();
            fullHistoryWindow.Show();
        }
        #endregion

        #region Get recent user transactions, loading them + Full history click
        private void GetRecentTransactionsLoaded()
        {
            var recentTransaction = GetRecentTransactions(user.UserID);
            int i = 1;

            foreach (var transaction in recentTransaction)
            {
                if (transaction.type == "Доход")
                {
                    RecentTransactionsList.Items.Add(new TextBlock
                    {
                        Text = $"{i++}. {transaction.category} - {transaction.amount}\n {transaction.date}",
                        Foreground = Brushes.Green,
                        Margin = new Thickness(0, 0, 0, 2)
                    });
                }
                else
                {
                    RecentTransactionsList.Items.Add(new TextBlock
                    {
                        Text = $"{i++}. {transaction.category} - {transaction.amount}\n {transaction.date}",
                        Foreground = Brushes.Red,
                        Margin = new Thickness(0, 0, 0, 2)
                    });
                }
            }
        }

        private List<(string category, string type, int amount, DateTime date)> GetRecentTransactions(int userID)
        {
            var transactions = new List<(string category, string type, int amount, DateTime date)>();

            using (var connection = dbHelper.GetConnection())
            {
                using (var command = new NpgsqlCommand(@"
                    SELECT category, type, amount, registration_date
                    FROM transactions WHERE userid=@UserId
                    ORDER BY transactionid DESC
                    LIMIT 5", connection))
                {
                    command.Parameters.AddWithValue("UserId", user.UserID);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                            transactions.Add((reader.GetString(0), reader.GetString(1), reader.GetInt32(2), reader.GetDateTime(3)));
                    }

                    return transactions;
                }
            }
        }

        private void ExpandTopExpenses_Click(object sender, RoutedEventArgs e)
        {
            var allExpensesWindow = new AllExpensesWindow(); // Создай новое окно для всех расходов
            allExpensesWindow.Show(); // Открой его
        }
        #endregion

        #region Reminder Open CLick
        private void ReminderOpen_Click(object sender, RoutedEventArgs e)
        {
            ReminderWindow reminderWindow = new ReminderWindow();
            reminderWindow.Show();
        }
        #endregion
    }
}
