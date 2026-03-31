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
using System.Windows.Shapes;

namespace SportsStoreApp1
{
    public partial class ProductsListWindow : Window
    {
        private Model1Container db;
        private Users currentUser;

        public ProductsListWindow()
        {
            InitializeComponent();
            db = new Model1Container();
            LoadProducts();
        }

        public ProductsListWindow(Users user) : this()
        {
            currentUser = user;
            if (user != null)
            {
                txtUserInfo.Text = $"{user.LastName} {user.FirstName} ({user.Role})";
            }
        }

        private void LoadProducts()
        {
            try
            {
                var products = db.Products
                    .Select(p => new
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Category = p.Categories.Name,
                        Price = p.Price,
                        Quantity = p.Quantity,
                        Status = p.Quantity > 10 ? "В наличии" : (p.Quantity > 0 ? "Мало" : "Нет в наличии"),
                        AddedDate = p.AddedDate
                    })
                    .ToList();

                dgProducts.ItemsSource = products;
                txtTotalItems.Text = products.Count.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки товаров: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ProductEditWindow productWindow = new ProductEditWindow(db);
                productWindow.Owner = this;

                if (productWindow.ShowDialog() == true)
                {
                    LoadProducts();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgProducts.SelectedItem == null)
                {
                    MessageBox.Show("Выберите товар для редактирования",
                        "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                dynamic selected = dgProducts.SelectedItem;
                int productId = selected.Id;
                var product = db.Products.Find(productId);

                if (product != null)
                {
                    ProductEditWindow productWindow = new ProductEditWindow(db, product);
                    productWindow.Owner = this;

                    if (productWindow.ShowDialog() == true)
                    {
                        LoadProducts();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка редактирования: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgProducts.SelectedItem == null)
                {
                    MessageBox.Show("Выберите товар для удаления",
                        "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show("Вы действительно хотите удалить выбранный товар?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    dynamic selected = dgProducts.SelectedItem;
                    int productId = selected.Id;
                    var product = db.Products.Find(productId);

                    if (product != null)
                    {
                        db.Products.Remove(product);
                        db.SaveChanges();
                        LoadProducts();
                        MessageBox.Show("Товар успешно удален", "Информация",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void txtSearch_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            try
            {
                string searchText = txtSearch.Text.ToLower();

                if (string.IsNullOrWhiteSpace(searchText))
                {
                    LoadProducts();
                    return;
                }

                var filteredProducts = db.Products
                    .Where(p => p.Name.ToLower().Contains(searchText) ||
                               p.Categories.Name.ToLower().Contains(searchText))
                    .Select(p => new
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Category = p.Categories.Name,
                        Price = p.Price,
                        Quantity = p.Quantity,
                        Status = p.Quantity > 10 ? "В наличии" : (p.Quantity > 0 ? "Мало" : "Нет в наличии"),
                        AddedDate = p.AddedDate
                    })
                    .ToList();

                dgProducts.ItemsSource = filteredProducts;
                txtTotalItems.Text = filteredProducts.Count.ToString();
            }
            catch
            {
                LoadProducts();
            }
        }

        private void dgProducts_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            btnEdit_Click(sender, null);
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы действительно хотите выйти из системы?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                MainWindow loginWindow = new MainWindow();
                loginWindow.Show();
                this.Close();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            db?.Dispose();
            base.OnClosed(e);
        }
    }
}