using Npgsql;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PersonalFinanceApp
{
    public partial class FullHistoryWindow : Window
    {
        ActiveUser user = new ActiveUser();
        Transaction transaction = new Transaction();
        DataBaseHelper dbHelper = new DataBaseHelper();

        public FullHistoryWindow()
        {
            InitializeComponent();
            LoadRecentTransactions();
        }

        #region Get and Load recent user transactions

        // Loads the latest user transactions and displays them in the interface
        private void LoadRecentTransactions()
        {
            var recentTransactions = GetRecentTransactions(user.UserID);
            int i = 1;

            foreach (var transaction in recentTransactions)
            {
                // Checks the transaction type to determine the text color
                FullTransactionsHistoryList.Items.Add(new TextBlock
                {
                    Text = $"{i++}. {transaction.category} - {transaction.amount}\n {transaction.date}",
                    Foreground = transaction.type == "Доход" ? Brushes.Green : Brushes.Red,
                    Margin = new Thickness(0, 0, 0, 2)
                });
            }
        }

        // Gets the latest user transactions from the database
        private List<(string category, string type, int amount, DateTime date)> GetRecentTransactions(int userID)
        {
            var transactions = new List<(string category, string type, int amount, DateTime date)>();

            using (var connection = dbHelper.GetConnection())
            {
                using (var command = new NpgsqlCommand(@"
                    SELECT category, type, amount, registration_date
                    FROM transactions WHERE userid=@UserId
                    ORDER BY transactionid DESC
                    LIMIT 30", connection)) // Limits the number of transactions to 30
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
        #endregion
    }
}
