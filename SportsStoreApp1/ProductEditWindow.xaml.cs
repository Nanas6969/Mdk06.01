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
using System.Data.Entity.Validation;

namespace SportsStoreApp1
{
    public partial class ProductEditWindow : Window
    {
        private Model1Container db;
        private Products currentProduct;
        private bool isEditMode = false;

        public ProductEditWindow(Model1Container database)
        {
            InitializeComponent();
            db = database;
            LoadCategories();
            txtWindowTitle.Text = "Добавление товара";
        }

        public ProductEditWindow(Model1Container database, Products product) : this(database)
        {
            currentProduct = product;
            isEditMode = true;
            txtWindowTitle.Text = "Редактирование товара";
            LoadProductData();
        }

        private void LoadCategories()
        {
            try
            {
                var categories = db.Categories.ToList();
                cmbCategory.ItemsSource = categories;
                cmbCategory.DisplayMemberPath = "Name";
                cmbCategory.SelectedValuePath = "Id";

                if (categories.Any())
                    cmbCategory.SelectedIndex = 0;
                else
                    MessageBox.Show("В базе данных нет категорий! Добавьте категории через SQL.",
                        "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}");
            }
        }

        private void LoadProductData()
        {
            if (currentProduct != null)
            {
                txtName.Text = currentProduct.Name;
                cmbCategory.SelectedValue = currentProduct.CategoryId;
                txtPrice.Text = currentProduct.Price.ToString();
                txtQuantity.Text = currentProduct.Quantity.ToString();
                txtManufacturer.Text = currentProduct.Manufacturer;
                txtArticle.Text = currentProduct.Article;
                txtDescription.Text = currentProduct.Description;

                // Устанавливаем статус в ComboBox
                if (!string.IsNullOrEmpty(currentProduct.Status))
                {
                    switch (currentProduct.Status)
                    {
                        case "В наличии":
                            cmbStatus.SelectedIndex = 0;
                            break;
                        case "Мало":
                            cmbStatus.SelectedIndex = 1;
                            break;
                        case "Нет в наличии":
                            cmbStatus.SelectedIndex = 2;
                            break;
                        default:
                            cmbStatus.SelectedIndex = 0;
                            break;
                    }
                }

                if (currentProduct.Weight.HasValue)
                    txtWeight.Text = currentProduct.Weight.ToString();

                txtSize.Text = currentProduct.Size;
                txtColor.Text = currentProduct.Color;
                txtMaterial.Text = currentProduct.Material;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // ПРОВЕРКА 1: Наименование
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Введите наименование товара", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // ПРОВЕРКА 2: Категория
                if (cmbCategory.SelectedValue == null)
                {
                    MessageBox.Show("Выберите категорию из списка", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // ПРОВЕРКА 3: Цена
                if (!decimal.TryParse(txtPrice.Text, out decimal price))
                {
                    MessageBox.Show("Введите корректную цену (например: 1000,50)", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // ПРОВЕРКА 4: Количество
                if (!int.TryParse(txtQuantity.Text, out int quantity))
                {
                    MessageBox.Show("Введите корректное количество", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // ПРОВЕРКА 5: Статус
                if (cmbStatus.SelectedItem == null)
                {
                    MessageBox.Show("Выберите статус товара", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Получаем статус из ComboBox
                string status = (cmbStatus.SelectedItem as ComboBoxItem).Content.ToString();
                int categoryId = (int)cmbCategory.SelectedValue;

                // ПРОВЕРКА 6: Существует ли категория в БД
                var categoryExists = db.Categories.Any(c => c.Id == categoryId);
                if (!categoryExists)
                {
                    MessageBox.Show($"Категория с ID {categoryId} не существует в базе данных!",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (isEditMode)
                {
                    // Редактирование существующего товара
                    currentProduct.Name = txtName.Text;
                    currentProduct.CategoryId = categoryId;
                    currentProduct.Price = price;
                    currentProduct.Quantity = quantity;
                    currentProduct.Status = status;

                    // Необязательные поля
                    currentProduct.Manufacturer = string.IsNullOrWhiteSpace(txtManufacturer.Text) ? null : txtManufacturer.Text;
                    currentProduct.Article = string.IsNullOrWhiteSpace(txtArticle.Text) ? null : txtArticle.Text;
                    currentProduct.Description = string.IsNullOrWhiteSpace(txtDescription.Text) ? null : txtDescription.Text;

                    if (decimal.TryParse(txtWeight.Text, out decimal weight))
                        currentProduct.Weight = weight;
                    else
                        currentProduct.Weight = null;

                    currentProduct.Size = string.IsNullOrWhiteSpace(txtSize.Text) ? null : txtSize.Text;
                    currentProduct.Color = string.IsNullOrWhiteSpace(txtColor.Text) ? null : txtColor.Text;
                    currentProduct.Material = string.IsNullOrWhiteSpace(txtMaterial.Text) ? null : txtMaterial.Text;
                }
                else
                {
                    // Добавление нового товара
                    var newProduct = new Products
                    {
                        Name = txtName.Text,
                        CategoryId = categoryId,
                        Price = price,
                        Quantity = quantity,
                        Status = status,
                        AddedDate = DateTime.Now,
                        Manufacturer = string.IsNullOrWhiteSpace(txtManufacturer.Text) ? null : txtManufacturer.Text,
                        Article = string.IsNullOrWhiteSpace(txtArticle.Text) ? null : txtArticle.Text,
                        Description = string.IsNullOrWhiteSpace(txtDescription.Text) ? null : txtDescription.Text,
                        Size = string.IsNullOrWhiteSpace(txtSize.Text) ? null : txtSize.Text,
                        Color = string.IsNullOrWhiteSpace(txtColor.Text) ? null : txtColor.Text,
                        Material = string.IsNullOrWhiteSpace(txtMaterial.Text) ? null : txtMaterial.Text
                    };

                    if (decimal.TryParse(txtWeight.Text, out decimal weight))
                        newProduct.Weight = weight;

                    db.Products.Add(newProduct);
                }

                // Сохраняем изменения
                db.SaveChanges();

                DialogResult = true;
                Close();
            }
            catch (DbEntityValidationException ex)
            {
                // ОШИБКА ВАЛИДАЦИИ ENTITY FRAMEWORK
                string errorMessage = "ОШИБКА ВАЛИДАЦИИ:\n\n";
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        errorMessage += $"📌 Поле: {validationError.PropertyName}\n";
                        errorMessage += $"❌ Ошибка: {validationError.ErrorMessage}\n\n";
                    }
                }
                MessageBox.Show(errorMessage, "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
            {
                // ОШИБКА ОБНОВЛЕНИЯ БАЗЫ ДАННЫХ
                string errorMessage = $"ОШИБКА БАЗЫ ДАННЫХ:\n\n{ex.Message}\n\n";

                if (ex.InnerException != null)
                {
                    errorMessage += $"ВНУТРЕННЯЯ ОШИБКА:\n{ex.InnerException.Message}\n\n";

                    if (ex.InnerException.InnerException != null)
                    {
                        errorMessage += $"ДЕТАЛИ SQL:\n{ex.InnerException.InnerException.Message}";

                        // Проверяем на конкретные ошибки SQL
                        string sqlError = ex.InnerException.InnerException.Message.ToLower();

                        if (sqlError.Contains("unique") || sqlError.Contains("duplicate"))
                        {
                            errorMessage += "\n\n🔴 Возможно, артикул должен быть уникальным!";
                        }
                        if (sqlError.Contains("foreign key"))
                        {
                            errorMessage += "\n\n🔴 Ошибка внешнего ключа - проверьте категорию!";
                        }
                        if (sqlError.Contains("null"))
                        {
                            errorMessage += "\n\n🔴 Какое-то обязательное поле не заполнено!";
                        }
                        if (sqlError.Contains("length"))
                        {
                            errorMessage += "\n\n🔴 Превышена максимальная длина поля!";
                        }
                    }
                }

                MessageBox.Show(errorMessage, "Ошибка базы данных",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                // ВСЕ ОСТАЛЬНЫЕ ОШИБКИ
                string errorMessage = $"НЕИЗВЕСТНАЯ ОШИБКА:\n\n{ex.Message}\n\n";

                if (ex.InnerException != null)
                {
                    errorMessage += $"Внутренняя ошибка: {ex.InnerException.Message}";
                }

                MessageBox.Show(errorMessage, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}