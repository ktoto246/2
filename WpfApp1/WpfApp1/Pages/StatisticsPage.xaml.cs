using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using WpfApp1.Models;

namespace WpfApp1.Pages
{
    public partial class StatisticsPage : Page
    {
        public Func<double, string> Formatter { get; set; }
        public List<string> ProductLabels { get; set; }

        public StatisticsPage()
        {
            InitializeComponent();
            LoadCharts();
        }

        private void LoadCharts()
        {
            using (var db = new BankDbContext())
            {
                var loans = db.Займы.Include(z => z.Продукт).ToList();

                if (!loans.Any()) return;

                var productGroups = loans.GroupBy(z => z.Продукт.Название)
                                         .Select(g => new { Name = g.Key, Count = g.Count() });

                SeriesCollection pieSeries = new SeriesCollection();
                foreach (var item in productGroups)
                {
                    pieSeries.Add(new PieSeries
                    {
                        Title = item.Name,
                        Values = new ChartValues<int> { item.Count },
                        DataLabels = true
                    });
                }
                ChartProducts.Series = pieSeries;

                var amountGroups = loans.GroupBy(z => z.Продукт.Название)
                                        .Select(g => new { Name = g.Key, Total = g.Sum(z => z.Сумма_Займа) })
                                        .ToList();

                ProductLabels = amountGroups.Select(a => a.Name).ToList();
                Formatter = value => value.ToString("N0") + " ₽";

                ChartAmounts.Series = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "Выдано всего",
                        Values = new ChartValues<decimal>(amountGroups.Select(a => (decimal)a.Total)),
                        DataLabels = true
                    }
                };

                DataContext = this;
            }
        }
    }
}