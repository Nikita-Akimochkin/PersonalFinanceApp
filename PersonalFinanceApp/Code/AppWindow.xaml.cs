using Npgsql;
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
            LoadTopExpenses();
            LoadRecentTransactions();
        }

        #region Get user account balance and than loading it

        // Loads the user's balance from the database and displays it in the interface
        public void LoadUserAccountBalance()
        {
            user.UserAccount = GetUserAccountBalance(user.UserID);
            TotalTextBlock.Text = Convert.ToString(user.UserAccount);
        }

        // Gets the user's balance from the database by user ID
        private int GetUserAccountBalance(int userId)
        {
            try
            {
                using (NpgsqlConnection connection = dbHelper.GetConnection())
                {
                    using (var command = new NpgsqlCommand())
                    {
                        command.Connection = connection;

                        command.CommandText = @"
                            SELECT account
                            FROM users 
                            WHERE id = @UserId";

                        command.Parameters.AddWithValue("UserId", user.UserID);

                        user.UserAccount = Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error when receiving balance: " + ex.Message);
            }

            return user.UserAccount;
        }
        #endregion

        #region Transaction Click

        // Opens the transaction window
        private void Transaction_Click(object sender, RoutedEventArgs e)
        {
            TransactionsWindow transactionsWindow = new TransactionsWindow();
            transactionsWindow.Show();
        }
        #endregion

        #region Get top user expenses, loading them + Full history click

        // Loads and displays the top 3 user expense categories in the interface
        private void LoadTopExpenses()
        {
            var topExpenses = GetTopExpenses(user.UserID);
            int i = 1;

            foreach (var transaction in topExpenses)
            {
                TopExpensesList.Items.Add(new TextBlock
                {
                    Text = $"{i++}. {transaction.name} - {transaction.amount}",
                    Margin = new Thickness(0, 0, 0, 4)
                });
            }
        }

        // Gets user's top 3 spending categories from the database
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
                            WHERE userId = @UserId AND type = 'Expense'
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
                MessageBox.Show("Error in getting the top costs: " + ex.Message);
            }

            return categories;
        }

        // Opens a window with the full history of user's expenses
        private void ShowFullHistory_Click(object sender, RoutedEventArgs e)
        {
            FullHistoryWindow fullHistoryWindow = new FullHistoryWindow();
            fullHistoryWindow.Show();
        }
        #endregion

        #region Get recent user transactions, loading them + Full history click

        // Loads the last 5 user transactions and displays them in the interface
        private void LoadRecentTransactions()
        {
            var recentTransaction = GetRecentTransactions(user.UserID);
            int i = 1;

            foreach (var transaction in recentTransaction)
            {
                // Defines the text color depending on the transaction type (Income/Expense)
                RecentTransactionsList.Items.Add(new TextBlock
                {
                    Text = $"{i++}. {transaction.category} - {transaction.amount}\n {transaction.date.ToString("dd.MM.yyyy")}",
                    Foreground = transaction.type == "Income" ? Brushes.Green : Brushes.Red,
                    Margin = new Thickness(0, 0, 0, 2)
                });
            }
        }

        // Gets the last 5 user transactions from the database
        private List<(string category, string type, int amount, DateTime date)> GetRecentTransactions(int userID)
        {
            var transactions = new List<(string category, string type, int amount, DateTime date)>();

            using (var connection = dbHelper.GetConnection())
            {
                using (var command = new NpgsqlCommand(@"
                    SELECT category, type, amount, registration_date
                    FROM transactions 
                    WHERE userid=@UserId
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

        // Opens a window with all expenses
        private void ExpandTopExpenses_Click(object sender, RoutedEventArgs e)
        {
            var allExpensesWindow = new AllExpensesWindow();
            allExpensesWindow.Show();
        }
        #endregion

        #region Reminder Open CLick

        // Opens the reminder window
        private void ReminderOpen_Click(object sender, RoutedEventArgs e)
        {
            ReminderWindow reminderWindow = new ReminderWindow();
            reminderWindow.Show();
        }
        #endregion
    }
}
