using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.EntityFrameworkCore;
using WpfApp1.Models;

namespace WpfApp1.Pages
{
    public partial class PaymentsPage : Page
    {
        private readonly BankDbContext _context;
        private Платеж? _currentPayment;
        private ICollectionView? _paymentsView;

        public PaymentsPage()
        {
            InitializeComponent();
            _context = new BankDbContext();
            LoadData();
        }

        private void LoadData()
        {
            _context.Клиенты.Load();
            _context.Займы.Include(z => z.Клиент).Load();
            _context.Платежи.Load();

            var loans = _context.Займы.Local
                .Select(l => new { l.ID_Займа, Display = $"ID {l.ID_Займа} - {(l.Клиент?.ФИО ?? "<Нет клиента>")}" })
                .ToList();
            CmbLoan.ItemsSource = loans;
            CmbLoan.DisplayMemberPath = "Display";
            CmbLoan.SelectedValuePath = "ID_Займа";

            CmbPaymentType.ItemsSource = new[] { "Основной долг", "Проценты" };
            CmbPaymentType.SelectedIndex = 0;

            FilterTypeCombo.ItemsSource = new[] { "Все", "Основной долг", "Проценты" };
            FilterTypeCombo.SelectedIndex = 0;

            PaymentsGrid.ItemsSource = _context.Платежи.Local.ToObservableCollection();

            if (PaymentsGrid.ItemsSource != null)
            {
                _paymentsView = CollectionViewSource.GetDefaultView(PaymentsGrid.ItemsSource);
                _paymentsView.Filter = FilterPayments;
            }
            ClearForm();
        }

        private bool FilterPayments(object obj)
        {
            if (obj is not Платеж item)
                return false;

            string search = SearchTextBox.Text?.ToLower() ?? string.Empty;
            string filterType = FilterTypeCombo.SelectedItem as string ?? "Все";

            string type = (item.Тип_Платежа ?? string.Empty).ToLower();

            bool matchesSearch = string.IsNullOrWhiteSpace(search) ||
                                 item.ID_Займа.ToString().Contains(search) ||
                                 type.Contains(search);
            bool matchesType = filterType == "Все" || (item.Тип_Платежа ?? string.Empty) == filterType;

            return matchesSearch && matchesType;
        }

        private void PaymentsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PaymentsGrid.SelectedItem is Платеж selected)
            {
                _currentPayment = selected;
                CmbLoan.SelectedValue = selected.ID_Займа;
                DpPaymentDate.SelectedDate = selected.Дата_Платежа;
                TxtAmount.Text = selected.Сумма_Платежа.ToString();
                CmbPaymentType.SelectedItem = selected.Тип_Платежа;
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CmbLoan.SelectedValue == null)
            {
                MessageBox.Show("Выберите займ!", "Внимание");
                return;
            }

            string amountStr = TxtAmount.Text.Trim();
            DateTime? paymentDate = DpPaymentDate.SelectedDate;

            if (!IsDataValid(amountStr, paymentDate))
                return;

            int loanId = (int)CmbLoan.SelectedValue;
            decimal amount = decimal.Parse(amountStr);
            string paymentType = CmbPaymentType.SelectedItem as string ?? "Основной долг";

            try
            {
                if (_currentPayment == null)
                {
                    var newPayment = new Платеж
                    {
                        ID_Займа = loanId,
                        Дата_Платежа = paymentDate.Value,
                        Сумма_Платежа = amount,
                        Тип_Платежа = paymentType
                    };
                    _context.Платежи.Add(newPayment);
                    _currentPayment = newPayment;
                }
                else
                {
                    _currentPayment.ID_Займа = loanId;
                    _currentPayment.Дата_Платежа = paymentDate.Value;
                    _currentPayment.Сумма_Платежа = amount;
                    _currentPayment.Тип_Платежа = paymentType;
                }

                _context.SaveChanges();
                PaymentsGrid.Items.Refresh();
                MessageBox.Show("Данные успешно сохранены!", "Ок", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"База данных дала сбой: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPayment != null && MessageBox.Show("Удалить?", "Вопрос", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _context.Платежи.Remove(_currentPayment);
                _context.SaveChanges();
                ClearForm();
            }
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e) => ClearForm();

        private void ClearForm()
        {
            PaymentsGrid.SelectedItem = null;
            _currentPayment = null;
            CmbLoan.SelectedIndex = -1;
            DpPaymentDate.SelectedDate = null;
            TxtAmount.Text = string.Empty;
            CmbPaymentType.SelectedIndex = 0;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e) => _paymentsView?.Refresh();
        private void FilterTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e) => _paymentsView?.Refresh();

        private bool IsDataValid(string amountStr, DateTime? paymentDate)
        {
            if (!decimal.TryParse(amountStr, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Сумма платежа должна быть числом больше нуля!", "Ошибка");
                return false;
            }

            if (paymentDate == null)
            {
                MessageBox.Show("Укажите дату платежа!", "Внимание");
                return false;
            }

            return true;
        }
    }
}
