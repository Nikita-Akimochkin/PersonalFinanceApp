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
            GetTopExpensesLoaded();
            GetRecentTransactionsLoaded();
        }

        #region Load USer Acc
        public void LoadUserAccountBalance()
        {
            user.UserAccount = GetUserAccountBalance(user.UserID);
            TotalTextBlock.Text = Convert.ToString(user.UserAccount);
        }
        #endregion

        #region GET USER ACC
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

        #region Trans Click
        private void Transaction_Click(object sender, RoutedEventArgs e)
        {
            TransactionsWindow transactionsWindow = new TransactionsWindow();
            transactionsWindow.Show();
        }
        #endregion

        #region Get Top expenses load
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
        #endregion

        #region Get top expenses
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
                            {
                                transaction.Category = reader.GetString(0);
                                transaction.Amount = reader.GetInt32(1);

                                categories.Add((transaction.Category, transaction.Amount));
                            }
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
        #endregion

        #region Show full history click
        private void ShowFullHistory_Click(object sender, RoutedEventArgs e)
        {
            FullHistoryWindow fullHistoryWindow = new FullHistoryWindow();
            fullHistoryWindow.Show();
        }
        #endregion

        #region Get recent trans load
        private void GetRecentTransactionsLoaded()
        {
            var RecentTransaction = GetRecentTransactions(user.UserID);
            int i = 1;

            foreach (var transaction in RecentTransaction)
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
        #endregion

        #region Get Recent trans
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

        #region Expand top expenses click
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
