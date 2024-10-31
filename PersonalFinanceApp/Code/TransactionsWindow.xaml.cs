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
        private void IncomeButton_Click(object sender, RoutedEventArgs e)
        {
            // Подсвечиваем кнопку Доход
            IncomeButton.Background = Brushes.LightGreen;
            // Возвращаем кнопку Расход к исходному виду
            ExpenseButton.Background = Brushes.LightGray;
        }

        private void ExpenseButton_Click(object sender, RoutedEventArgs e)
        {
            // Подсвечиваем кнопку Расход
            ExpenseButton.Background = Brushes.OrangeRed;
            // Возвращаем кнопку Доход к исходному виду
            IncomeButton.Background = Brushes.LightGray;
        }
        #endregion

        #region Get/Lost Focus + Input restrictions for Text box
        private void TextBox_GetFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string defaultText = textBox.Tag.ToString();
            dbHelper.HandleFocus(textBox, defaultText, true);
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string defaultText = textBox.Tag.ToString();
            dbHelper.HandleFocus(textBox, defaultText, false);
        }

        private void AmountTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем только цифры
            e.Handled = !int.TryParse(e.Text, out _);
        }
        #endregion

        #region Continue click, Save transaction to DataBase and Update user account balance method 
        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            transaction.Amount = Convert.ToInt32(AmountTextBox.Text);
            transaction.Category = (CategoryComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            var appWindow = Application.Current.Windows.OfType<AppWindow>().FirstOrDefault();

            if (IncomeButton.Background == Brushes.LightGreen) transaction.Type = "Доход";
            else if (ExpenseButton.Background == Brushes.OrangeRed) transaction.Type = "Расход";
            else
            {
                MessageBox.Show("Пожалуйста, Выберите тип: Доход/Расход.");
                return;
            }

            if (transaction.Category == null)
            {
                MessageBox.Show("Пожалуйста, выберите категорию.");
                return;
            }

            // Сохранение транзакции в базе данных
            SaveTransaction(user.UserID, transaction.Amount, transaction.Type, transaction.Category);

            UpdateAccountBalance(user.UserID);

            appWindow.LoadUserAccountBalance();


            // Закрыть окно транзакций или показать сообщение об успехе
            MessageBox.Show("Транзакция успешно добавлена.");
            Close();

        }

        private void SaveTransaction(int userId, int amount, string type, string category)
        {
            try
            {
                using (NpgsqlConnection connection = dbHelper.GetConnection())
                {
                    using (var command = new NpgsqlCommand())
                    {
                        command.Connection = connection;
                        // SQL запрос для добавления новой транзакции
                        command.CommandText = "INSERT INTO Transactions (userID, amount, type, category, registration_date) " +
                                          "VALUES (@UserID, @Amount, @Type, @Category, @Registration_date)";
                        command.Parameters.AddWithValue("UserID", user.UserID);
                        command.Parameters.AddWithValue("Amount", transaction.Amount);
                        command.Parameters.AddWithValue("Type", transaction.Type);
                        command.Parameters.AddWithValue("Category", transaction.Category);
                        command.Parameters.AddWithValue("Registration_date", DateTime.Now);

                        command.ExecuteNonQuery();
                    }
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении транзакции: " + ex.Message);
            }
        }

        private void UpdateAccountBalance(int userId)
        {
            try
            {
                using (NpgsqlConnection connection = dbHelper.GetConnection())
                {
                    using (var command = new NpgsqlCommand())
                    {
                        command.Connection = connection;
                        // Обновление баланса аккаунта
                        command.CommandText = @"
                            UPDATE users
                            SET account = (
                                SELECT COALESCE(SUM(CASE 
                                                        WHEN type = 'Доход' THEN amount 
                                                        WHEN type = 'Расход' THEN -amount 
                                                        ELSE 0 
                                                    END), 0)
                                FROM transactions 
                                WHERE userid = @UserId
                            )
                            WHERE id = @UserId";

                        command.Parameters.AddWithValue("UserId", user.UserID);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обновлении баланса: " + ex.Message);
            }
        }
        #endregion
    }
}