using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SportsStoreApp1
{
    public partial class MainWindow : Window
    {
        private Model1Container db;

        public MainWindow()
        {
            InitializeComponent();
            db = new Model1Container();
        }

        // Переключение на вкладку Вход
        private void btnLoginTab_Click(object sender, RoutedEventArgs e)
        {
            panelLogin.Visibility = Visibility.Visible;
            panelRegister.Visibility = Visibility.Collapsed;

            // ИСПРАВЛЕНО: используем правильный синтаксис C# для цветов
            btnLoginTab.Background = new SolidColorBrush(Color.FromRgb(30, 60, 114)); // #1E3C72
            btnLoginTab.Foreground = Brushes.White;
            btnRegisterTab.Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)); // #F5F5F5
            btnRegisterTab.Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51)); // #333333

            errorBorder.Visibility = Visibility.Collapsed;
        }

        // Переключение на вкладку Регистрация
        private void btnRegisterTab_Click(object sender, RoutedEventArgs e)
        {
            panelLogin.Visibility = Visibility.Collapsed;
            panelRegister.Visibility = Visibility.Visible;

            // ИСПРАВЛЕНО: используем правильный синтаксис C# для цветов
            btnRegisterTab.Background = new SolidColorBrush(Color.FromRgb(30, 60, 114)); // #1E3C72
            btnRegisterTab.Foreground = Brushes.White;
            btnLoginTab.Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)); // #F5F5F5
            btnLoginTab.Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51)); // #333333

            errorBorder.Visibility = Visibility.Collapsed;
        }

        // Вход в систему
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            errorBorder.Visibility = Visibility.Collapsed;

            // Проверка полей
            if (string.IsNullOrWhiteSpace(txtLoginEmail.Text))
            {
                ShowError("Введите email");
                return;
            }

            if (txtLoginPassword.Password == "")
            {
                ShowError("Введите пароль");
                return;
            }

            try
            {
                // Ищем пользователя в базе
                var user = db.Users.FirstOrDefault(u => u.Email == txtLoginEmail.Text);

                if (user == null)
                {
                    ShowError("Пользователь не найден");
                    return;
                }

                // Проверяем пароль
                if (user.PasswordHash == txtLoginPassword.Password)
                {
                    // Обновляем дату последнего входа
                    user.LastLogin = DateTime.Now;
                    db.SaveChanges();

                    MessageBox.Show($"Добро пожаловать, {user.FirstName} {user.LastName}!",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    ProductsListWindow productsWindow = new ProductsListWindow(user);
                    productsWindow.Show();
                    this.Close();
                }
                else
                {
                    ShowError("Неверный пароль");
                }
            }
            catch (Exception ex)
            {
                ShowError("Ошибка подключения к БД: " + ex.Message);
            }
        }

        // Регистрация нового пользователя
        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            errorBorder.Visibility = Visibility.Collapsed;

            // Проверка полей
            if (string.IsNullOrWhiteSpace(txtFirstName.Text))
            {
                ShowError("Введите имя");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                ShowError("Введите фамилию");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtRegisterEmail.Text))
            {
                ShowError("Введите email");
                return;
            }

            if (txtRegisterPassword.Password == "")
            {
                ShowError("Введите пароль");
                return;
            }

            if (txtConfirmPassword.Password == "")
            {
                ShowError("Подтвердите пароль");
                return;
            }

            if (txtRegisterPassword.Password != txtConfirmPassword.Password)
            {
                ShowError("Пароли не совпадают");
                return;
            }

            if (txtRegisterPassword.Password.Length < 3)
            {
                ShowError("Пароль должен быть не менее 3 символов");
                return;
            }

            try
            {
                // Проверяем, существует ли уже такой email
                var existingUser = db.Users.FirstOrDefault(u => u.Email == txtRegisterEmail.Text);
                if (existingUser != null)
                {
                    ShowError("Пользователь с таким email уже существует");
                    return;
                }

                // Создаем нового пользователя
                var newUser = new Users
                {
                    Email = txtRegisterEmail.Text,
                    PasswordHash = txtRegisterPassword.Password,
                    FirstName = txtFirstName.Text,
                    LastName = txtLastName.Text,
                    Role = "User",
                    CreatedAt = DateTime.Now
                };

                db.Users.Add(newUser);
                db.SaveChanges();

                MessageBox.Show("Регистрация прошла успешно! Теперь вы можете войти в систему.",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                // Переключаемся на вкладку входа
                btnLoginTab_Click(sender, e);

                // Очищаем поля
                txtFirstName.Text = "";
                txtLastName.Text = "";
                txtRegisterEmail.Text = "";
                txtRegisterPassword.Password = "";
                txtConfirmPassword.Password = "";
            }
            catch (Exception ex)
            {
                ShowError("Ошибка регистрации: " + ex.Message);
            }
        }

        private void ShowError(string message)
        {
            txtError.Text = message;
            errorBorder.Visibility = Visibility.Visible;
        }

        protected override void OnClosed(EventArgs e)
        {
            db?.Dispose();
            base.OnClosed(e);
        }
    }
}
