using System;
using Npgsql;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Markup;

namespace PersonalFinanceApp
{
    public partial class MainWindow : Window
    {
        ActiveUser user = new ActiveUser();
        DataBaseHelper dbHelper = new DataBaseHelper();

        public MainWindow()
        {
            InitializeComponent();
        }

        #region Get/Lost Focus

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
        #endregion

        #region Login click + Check user exist

        // Processes the login button press, checking the user's existence
        private void Login_Click(object sender, RoutedEventArgs e)
        {
            user.Email = EmailLoginTextBox.Text;
            user.Password = PasswordLoginTextBox.Text;
            user.UserID = user.GetUserId(user.Email);

            if (CheckUserCredentials(user.Email, user.Password))
            {
                AppWindow appWindow = new AppWindow();
                Close();
                appWindow.Show();
            }
            else
            {
                MessageBox.Show("Invalid data! Try again.");
            }
        }

        // Checks if a user with the specified email and password exists
        private bool CheckUserCredentials(string email, string password)
        {
            using (NpgsqlConnection connection = dbHelper.GetConnection())
            {
                string query = @"
                    SELECT COUNT(*) 
                    FROM users 
                    WHERE email = @Email AND password = @Password";

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("Email", user.Email);
                    cmd.Parameters.AddWithValue("Password", user.Password);

                    int userExist = Convert.ToInt32(cmd.ExecuteScalar());
                    return userExist > 0;
                }
            }
        }
        #endregion

        #region Switch to Registration/Login

        // Go to the registration window
        private void SwitchToRegistration_Click(object sender, RoutedEventArgs e)
        {
            RegistrationGrid.Visibility = Visibility.Visible;
            LoginGrid.Visibility = Visibility.Collapsed;
        }

        // Go to the login window
        private void SwitchToLogin_Click(object sender, RoutedEventArgs e)
        {
            RegistrationGrid.Visibility = Visibility.Collapsed;
            LoginGrid.Visibility = Visibility.Visible;
        }
        #endregion

        #region Registration click + User registration

        // Processes pressing the registration button, checks if the fields are filled in and calls user registration
        private void Registration_Click(object sender, RoutedEventArgs e)
        {
            user.UserName = NameTextBox.Text;
            user.Email = EmailTextBox.Text;
            user.Password = PasswordTextBox.Text;

            // Check for empty fields
            if (string.IsNullOrWhiteSpace(user.UserName) || string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(user.Password))
            {
                MessageBox.Show("Please fill in all fields!");
                return;
            }

            // Call method "RegisterUser"
            RegisterUser(user.UserName, user.Email, user.Password);
        }

        // Registers a new user in the database, checking if the email is unique
        private void RegisterUser(string name, string email, string password)
        {
            try
            {
                using (NpgsqlConnection connection = dbHelper.GetConnection())
                {
                    // Check if such a user already exists
                    string checkEmailQuery = @"
                        SELECT COUNT(*) 
                        FROM users 
                        WHERE email = @Email";

                    using (NpgsqlCommand checkCommand = new NpgsqlCommand(checkEmailQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("Email", user.Email);
                        int userCount = Convert.ToInt32(checkCommand.ExecuteScalar());

                        if (userCount > 0)
                        {
                            MessageBox.Show("A user with this Email already exists!");
                            return;
                        }
                    }

                    // If Email unique, add this user
                    string insertQuery = @"
                        INSERT INTO users (name, email, password, account, registration_date) 
                        VALUES (@Name, @Email, @Password, 0, @Registration_date)";

                    using (NpgsqlCommand insertCommand = new NpgsqlCommand(insertQuery, connection))
                    {
                        if (user.UserName != "Username:" && user.Email != "Email:" && user.Password != "Password:")
                        {
                            insertCommand.Parameters.AddWithValue("Name", user.UserName);
                            insertCommand.Parameters.AddWithValue("Email", user.Email);
                            insertCommand.Parameters.AddWithValue("Password", user.Password);  // Ideally, you should hash
                            insertCommand.Parameters.AddWithValue("Registration_date", DateTime.Now);

                            insertCommand.ExecuteNonQuery();

                            MessageBox.Show("Registration was successful!");
                            return;
                        }
                        MessageBox.Show("Enter the data!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Registration error: {ex.Message}");
            }
        }
        #endregion
    }
}
