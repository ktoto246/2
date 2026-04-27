using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.EntityFrameworkCore;
using WpfApp1.Models;
using System.Text.RegularExpressions;

namespace WpfApp1.Pages
{
    public partial class ClientsPage : Page
    {
        private readonly BankDbContext _context;
        private Клиент? _currentClient;
        private ICollectionView? _clientsView;

        public ClientsPage()
        {
            InitializeComponent();
            _context = new BankDbContext();
            LoadData();
        }

        private void LoadData()
        {
            _context.Клиенты.Load();
            ClientsGrid.ItemsSource = _context.Клиенты.Local.ToObservableCollection();

            if (ClientsGrid.ItemsSource != null)
            {
                _clientsView = CollectionViewSource.GetDefaultView(ClientsGrid.ItemsSource);
                _clientsView.Filter = (obj) =>
                {
                    if (obj is Клиент item)
                    {
                        string search = SearchTextBox.Text?.ToLower() ?? "";
                        return string.IsNullOrWhiteSpace(search) ||
                               item.ФИО.ToLower().Contains(search) ||
                               item.Паспортные_Данные.Contains(search);
                    }
                    return false;
                };
            }
            ClearForm();
        }

        private void ClientsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ClientsGrid.SelectedItem is Клиент selected)
            {
                _currentClient = selected;
                TxtFullName.Text = selected.ФИО;
                TxtPassport.Text = selected.Паспортные_Данные;
                TxtPhone.Text = selected.Телефон;
                DpBirthDate.SelectedDate = selected.Дата_Рождения;
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            string fio = TxtFullName.Text.Trim();
            string passport = TxtPassport.Text.Trim().Replace(" ", "");
            string phone = TxtPhone.Text.Trim();
            DateTime? birthDate = DpBirthDate.SelectedDate;

            if (!IsDataValid(fio, passport, phone, birthDate))
            {
                return; 
            }

            try
            {
                var duplicate = _context.Клиенты
                    .FirstOrDefault(c => c.Паспортные_Данные == passport && (_currentClient == null || c.ID_Клиента != _currentClient.ID_Клиента));

                if (duplicate != null)
                {
                    MessageBox.Show($"Клиент с таким паспортом уже есть в базе! (ID: {duplicate.ID_Клиента})",
                        "Ошибка уникальности", MessageBoxButton.OK, MessageBoxImage.Stop);
                    return;
                }

                if (_currentClient == null)
                {
                    var newClient = new Клиент
                    {
                        ФИО = fio,
                        Паспортные_Данные = passport,
                        Телефон = phone,
                        Дата_Рождения = birthDate.Value
                    };
                    _context.Клиенты.Add(newClient);
                    _currentClient = newClient;
                }
                else
                {
                    _currentClient.ФИО = fio;
                    _currentClient.Паспортные_Данные = passport;
                    _currentClient.Телефон = phone;
                    _currentClient.Дата_Рождения = birthDate.Value;
                }

                _context.SaveChanges();
                ClientsGrid.Items.Refresh();
                MessageBox.Show("Данные успешно сохранены!", "Ок", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"База данных дала сбой: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_currentClient != null && MessageBox.Show("Удалить?", "Вопрос", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _context.Клиенты.Remove(_currentClient);
                _context.SaveChanges();
                ClearForm();
            }
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e) => ClearForm();

        private void ClearForm()
        {
            ClientsGrid.SelectedItem = null;
            _currentClient = null;
            TxtFullName.Text = TxtPassport.Text = TxtPhone.Text = string.Empty;
            DpBirthDate.SelectedDate = null;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e) => _clientsView?.Refresh();
        private bool IsDataValid(string fio, string passport, string phone, DateTime? birthDate)
        {
            if (string.IsNullOrWhiteSpace(fio) || string.IsNullOrWhiteSpace(passport))
            {
                MessageBox.Show("ФИО и Паспорт должны быть заполнены обязательно!", "Внимание");
                return false;
            }

            if (fio.Split(' ').Length < 2)
            {
                MessageBox.Show("Введите полное ФИО (Фамилия и Имя минимум).", "Внимание");
                return false;
            }

            if (!Regex.IsMatch(passport, @"^\d{10}$"))
            {
                MessageBox.Show("Паспорт должен содержать ровно 10 цифр без пробелов!", "Ошибка формата");
                return false;
            }

            if (!string.IsNullOrWhiteSpace(phone) && !Regex.IsMatch(phone, @"^(\+7|8)?\d{10}$"))
            {
                MessageBox.Show("Некорректный формат телефона! Пример: +79991234567", "Ошибка формата");
                return false;
            }

            if (birthDate == null)
            {
                MessageBox.Show("Укажите дату рождения!", "Внимание");
                return false;
            }

            int age = DateTime.Now.Year - birthDate.Value.Year;
            if (birthDate.Value > DateTime.Now.AddYears(-age)) age--;

            if (age < 18 || age > 100)
            {
                MessageBox.Show($"Возраст клиента ({age} л.) не подходит для банковских операций! (Нужно 18-100 лет)", "Ошибка возраста");
                return false;
            }

            return true; 
        }
    }
}