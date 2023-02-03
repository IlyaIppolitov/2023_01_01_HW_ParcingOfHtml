using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
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
using System.Windows.Threading;

namespace HW_ParcingOfHtml
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void buttonGetVac_Click(object sender, RoutedEventArgs e)
        {
            // Очистка существующего списка
            listBoxOfVac.Items.Clear();

            // Главная страница
            var uriMainPage = "https://proglib.io/vacancies/all";

            // Определение количества страниц
            int numberOfPages = await getNumberOfPages(uriMainPage);

            // Инициализация массива с необходимым количеством элементов
            var arrOfPages = Enumerable.Range(0, numberOfPages).ToArray();

            // ConcurrentBag использовал для проерки
            // Результаты совпали, поэтому закоментировал
            // var bag = new ConcurrentBag<string>();

            // Общая часть названия страницы
            var uri = "https://proglib.io/vacancies/all?workType=all&workPlace=all&experience=&salaryFrom=&page=";

            // Определение макимального количество параллельных потоков
            ParallelOptions parallelOptions = new()
            {
                MaxDegreeOfParallelism = 5
            };

            //Считывание необходимых данных с сайта параллельно
            await Parallel.ForEachAsync(arrOfPages, parallelOptions, async (i, token) =>
            {
                using var client = new HttpClient();
                string response = await client.GetStringAsync(uri + (i + 1).ToString());

                string pattern = @"<div itemprop=""description"">(.+?)</div>";
                Regex regex = new Regex(pattern);
                MatchCollection matches = regex.Matches(response);
                foreach (Match match in matches)
                {
                    //bag.Add(match.Groups[1].Value);
                    Dispatcher.Invoke(() => listBoxOfVac.Items.Add(match.Groups[1].Value));
                }
            });

            //foreach (string s in bag)
            //{
            //    listBoxOfVac.Items.Add(s);
            //}
        }

        async Task<int> getNumberOfPages (string mainPage)
        {
            int numberOfPages;

            using var client = new HttpClient();
            {
                string response = await client.GetStringAsync(mainPage);

                string patternPageCount = @"data-current=""(.+)"" data-total=""(.+?)"">";
                Regex regex = new Regex(patternPageCount);
                MatchCollection matches = regex.Matches(response);
                numberOfPages = int.Parse(matches.ElementAt(0).Groups[2].Value);
            }
            return numberOfPages;
        }
    }
}
