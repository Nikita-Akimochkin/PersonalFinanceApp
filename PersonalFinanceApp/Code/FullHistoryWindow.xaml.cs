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
            GetRecentTransactionsLoaded();
        }

        #region Get and Load recent user transactions
        private void GetRecentTransactionsLoaded()
        {
            var RecentTransaction = GetRecentTransactions(user.UserID);
            int i = 1;

            foreach (var transaction in RecentTransaction)
            {
                if (transaction.type == "Доход")
                {
                    FullTransactionsHistoryList.Items.Add(new TextBlock
                    {
                        Text = $"{i++}. {transaction.category} - {transaction.amount}\n {transaction.date}",
                        Foreground = Brushes.Green,
                        Margin = new Thickness(0, 0, 0, 2)
                    });
                }
                else
                {
                    FullTransactionsHistoryList.Items.Add(new TextBlock
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
                    LIMIT 30", connection))
                {
                    command.Parameters.AddWithValue("UserId", user.UserID);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            transaction.Category = reader.GetString(0);
                            transaction.Type = reader.GetString(1);
                            transaction.Amount = reader.GetInt32(2);
                            transaction.Date = reader.GetDateTime(3);

                            transactions.Add((transaction.Category, transaction.Type, transaction.Amount, transaction.Date));
                        }
                    }
                    return transactions;
                }
            }
        }
        #endregion
    }
}
