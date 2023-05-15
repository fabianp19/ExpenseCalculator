using ExpenseCalculator.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Drawing;
using System.Data;
using FusionCharts.DataEngine;
using FusionCharts.Visualization;

namespace ExpenseCalculator.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        
        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            DateTime startDate = DateTime.Today.AddMonths(-1);
            DateTime endDate = DateTime.Today;

            List<Transaction> selectedTransactions = await _context.Transactions
                .Include(x => x.Category)
                .Where(x => x.Date >= startDate && x.Date <= endDate)
                .ToListAsync();

            int totalIncome = selectedTransactions
                .Where(x => x.Category!.Type == "Income")
                .Sum(x => x.Amount);
            ViewBag.TotalIncome = totalIncome.ToString("C0");

            int totalExpense = selectedTransactions
                .Where(x => x.Category!.Type == "Expense")
                .Sum(x => x.Amount);
            ViewBag.TotalExpense = totalExpense.ToString("C0");

            int Balance = totalIncome - totalExpense;
            ViewBag.Balance = Balance.ToString("C0");

            DoughnutChart(selectedTransactions);
            ExpenseSplineChart(selectedTransactions);
            IncomeSplineChart(selectedTransactions);

            return View();
        }

        private void DoughnutChart(List<Transaction> selectedTransactions)
        {
            DataTable ChartData = new DataTable();

            var title = selectedTransactions
                .Where(x => x.Category!.Type == "Expense")
                .GroupBy(x => x.Category!.CategoryId)
                .Select(x => x.First().Category!.Title)
                .ToArray();

            var amount = selectedTransactions
                .Where(x => x.Category!.Type == "Expense")
                .GroupBy(x => x.Category!.CategoryId)
                .Select(x => x.Sum(x => x.Amount))
                .ToArray();

            ChartData.Columns.Add("Title", typeof(System.String));
            ChartData.Columns.Add("Amount", typeof(System.Double));

            for (int i = 0; i < title.Length; i++)
            {
                ChartData.Rows.Add(title[i], amount[i]);
            }

            StaticSource source = new StaticSource(ChartData);
            DataModel model = new DataModel();
            model.DataSources.Add(source);
            Charts.DoughnutChart donut = new Charts.DoughnutChart("doughnut_chart");

            donut.Width.Pixel(1120);
            donut.Height.Pixel(500);
            donut.Data.Source = model;
            donut.Caption.Text = "Expense by category";
            donut.ThemeName = FusionChartsTheme.ThemeName.FUSION;

            ViewData["DoughnutChart"] = donut.Render();
        }

        private void ExpenseSplineChart(List<Transaction> selectedTransactions)
        {
            DataTable ExpenseChartData = new DataTable();

            ExpenseChartData.Columns.Add("Day", typeof(System.String));
            ExpenseChartData.Columns.Add("Amount", typeof(System.Double));

            var expenseSummaryDate = selectedTransactions
                .Where(x => x.Category!.Type == "Expense")
                .OrderBy(x => x.Date)
                .GroupBy(x => x.Date)
                .Select(x => x.First().Date.ToString("dd-MMM"))
                .ToArray();

            var expenseSummaryAmount = selectedTransactions
                .Where(x => x.Category!.Type == "Expense")
                .OrderBy(x => x.Date)
                .GroupBy(x => x.Date)
                .Select(x => x.Sum(x => x.Amount))
                .ToArray();

            for (int i = 0; i < expenseSummaryDate.Length; i++)
            {
                ExpenseChartData.Rows.Add(expenseSummaryDate[i], expenseSummaryAmount[i]);
            }

            StaticSource expenseSource = new StaticSource(ExpenseChartData);
            
            DataModel expenseModel = new DataModel();
            expenseModel.DataSources.Add(expenseSource);

            Charts.SplineChart expenseSpline = new Charts.SplineChart("expense_spline_chart");

            expenseSpline.ThemeName = FusionChartsTheme.ThemeName.FUSION;
            expenseSpline.Width.Pixel(1120);
            expenseSpline.Height.Pixel(600);

            expenseSpline.Data.Source = expenseModel;
            expenseSpline.Caption.Text = "Expense";
            expenseSpline.Caption.Bold = true;
            expenseSpline.Legend.Show = true;

            ViewData["ExpenseSplineChart"] = expenseSpline.Render();
        }

        private void IncomeSplineChart(List<Transaction> selectedTransactions)
        {
            DataTable IncomeChartData = new DataTable();

            IncomeChartData.Columns.Add("Day", typeof(System.String));
            IncomeChartData.Columns.Add("Amount", typeof(System.Double));

            var incomeSummaryDate = selectedTransactions
                .Where(x => x.Category!.Type == "Income")
                .OrderBy(x => x.Date)
                .GroupBy(x => x.Date)
                .Select(x => x.First().Date.ToString("dd-MMM"))
                .ToArray();

            var incomeSummaryAmount = selectedTransactions
                .Where(x => x.Category!.Type == "Income")
                .OrderBy(x => x.Date)
                .GroupBy(x => x.Date)
                .Select(x => x.Sum(x => x.Amount))
                .ToArray();

            for (int i = 0; i < incomeSummaryDate.Length; i++)
            {
                IncomeChartData.Rows.Add(incomeSummaryDate[i], incomeSummaryAmount[i]);
            }

            StaticSource incomeSource = new StaticSource(IncomeChartData);

            DataModel incomeModel = new DataModel();
            incomeModel.DataSources.Add(incomeSource);

            Charts.SplineChart incomeSpline = new Charts.SplineChart("income_spline_chart");

            incomeSpline.ThemeName = FusionChartsTheme.ThemeName.FUSION;
            incomeSpline.Width.Pixel(1120);
            incomeSpline.Height.Pixel(600);

            incomeSpline.Data.Source = incomeModel;
            incomeSpline.Caption.Text = "Income";
            incomeSpline.Caption.Bold = true;
            incomeSpline.Legend.Show = true;

            ViewData["IncomeSplineChart"] = incomeSpline.Render();
        }
    }
}