using System;
using Npgsql;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PersonalFinanceApp
{
    public partial class TransactionsWindow : Window
    {
        ActiveUser user = new ActiveUser();
        Transaction transaction = new Transaction();
        DataBaseHelper dbHelper = new DataBaseHelper();

        public TransactionsWindow()
        {
            InitializeComponent();
        }

        #region Income/Expense click

        // Processes clicks on the “Income” button, changing their color to indicate the selection.
        private void IncomeButton_Click(object sender, RoutedEventArgs e)
        {
            IncomeButton.Background = Brushes.LightGreen;
            ExpenseButton.Background = Brushes.LightGray;
            CategoryText.Text = "Income";
            CategoryComboBox.Visibility = Visibility.Collapsed;
        }

        // Processes clicks on the “Expense” button, changing their color to indicate the selection.
        private void ExpenseButton_Click(object sender, RoutedEventArgs e)
        {
            ExpenseButton.Background = Brushes.OrangeRed;
            IncomeButton.Background = Brushes.LightGray;
            CategoryText.Text = "Select a category:";
            CategoryComboBox.Visibility = Visibility.Visible;
        }
        #endregion

        #region Get/Lost Focus + Input restrictions for Text box

        // Processes getting TextBox focus
        private void TextBox_GetFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string defaultText = textBox.Tag.ToString();
            dbHelper.HandleFocus(textBox, defaultText, true);
        }

        // Handles loss of focus TextBox
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string defaultText = textBox.Tag.ToString();
            dbHelper.HandleFocus(textBox, defaultText, false);
        }

        // We only allow numbers
        private void AmountTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }
        #endregion

        #region Continue click, Save transaction to DataBase and Update user account balance method 

        // Processes a click on the “Continue” button, saves the transaction to the database and updates the user's account balance.
        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            transaction.Amount = Convert.ToInt32(AmountTextBox.Text);
            transaction.Category = (CategoryComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            var appWindow = Application.Current.Windows.OfType<AppWindow>().FirstOrDefault();

            if (IncomeButton.Background == Brushes.LightGreen)
            {
                transaction.Category = "Income";
                transaction.Type = "Income";
            }
            else if (ExpenseButton.Background == Brushes.OrangeRed) transaction.Type = "Expense";
            else
            {
                MessageBox.Show("Please select type: Income/Expense.");
                return;
            }

            if (transaction.Category == null && transaction.Type == "Expense")
            {
                MessageBox.Show("Please select a category.");
                return;
            }

            // Saving the transaction in the database
            SaveTransaction(user.UserID, transaction.Amount, transaction.Type, transaction.Category);

            //Updates the user's balance
            UpdateAccountBalance(user.UserID);

            appWindow.LoadUserAccountBalance();

            MessageBox.Show("Transaction successfully added.");
            Close();

        }

        // Saves the transaction to the database
        private void SaveTransaction(int userId, int amount, string type, string category)
        {
            try
            {
                using (NpgsqlConnection connection = dbHelper.GetConnection())
                {
                    using (var command = new NpgsqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = @"
                            INSERT INTO Transactions (userID, amount, type, category, registration_date)
                            VALUES (@UserID, @Amount, @Type, @Category, @Registration_date)";

                        command.Parameters.AddWithValue("UserID", user.UserID);
                        command.Parameters.AddWithValue("Amount", transaction.Amount);
                        command.Parameters.AddWithValue("Type", transaction.Type);
                        command.Parameters.AddWithValue("Category", transaction.Category);
                        command.Parameters.AddWithValue("Registration_date", DateTime.Today);

                        command.ExecuteNonQuery();
                    }
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show("Error when saving a transaction: " + ex.Message);
            }
        }

        // Updates the user's account balance.
        private void UpdateAccountBalance(int userId)
        {
            try
            {
                using (NpgsqlConnection connection = dbHelper.GetConnection())
                {
                    using (var command = new NpgsqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = @"
                            UPDATE users
                            SET account = (
                            SELECT COALESCE(SUM(CASE 
                            WHEN type = 'Income' THEN amount 
                            WHEN type = 'Expense' THEN -amount 
                            ELSE 0 
                            END), 0)
                            FROM transactions 
                            WHERE userid = @UserId)
                            WHERE id = @UserId";

                        command.Parameters.AddWithValue("UserId", user.UserID);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error when updating balance: " + ex.Message);
            }
        }
        #endregion
    }
}