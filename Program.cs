using System;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using NLog;

namespace InstaService
{
    public class ServiceLogger
    {
        private Logger _logger;

        public Logger Logger
        {
            get
            {
                if (_logger == null)
                {
                    var config = new NLog.Config.LoggingConfiguration();
                    var logfile = new NLog.Targets.FileTarget("logfile") { FileName = $"{DateTime.Now:yyyy-MM-dd HH-mm-ss}.txt" };

                    config.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Fatal, logfile);
                    LogManager.Configuration = config;

                    _logger = LogManager.GetLogger(GetType().Name);
                }

                return _logger;
            }
        }
    }

    public class Program
    {
        public static IConfiguration Configuration { get; set; }
        private static IWebDriver Driver { get; set; }
        private static ServiceLogger Logger { get; set; }
        private static int LikeCount { get; set; }

        static void Main(string[] args)
        {
            try
            {
                Logger = new ServiceLogger();

                var builder = new ConfigurationBuilder()
                    .SetBasePath(Path.Combine(AppContext.BaseDirectory))
                    .AddJsonFile("appsettings.json", optional: true);

                Configuration = builder.Build();


                Console.WriteLine("Введите хештег (без символа #)");
                var tag = Console.ReadLine();
                Console.WriteLine("Введите количество лайков (не рекомендуется больше 900)");
                var likeCountStr = Console.ReadLine();
                var likes = 0;
                int.TryParse(likeCountStr, out likes);

                var chromePath = AppDomain.CurrentDomain.BaseDirectory;
                Driver = new ChromeDriver(chromePath, new ChromeOptions(), TimeSpan.FromSeconds(180));
                Driver.Navigate().GoToUrl("https://www.instagram.com/accounts/login/?hl=ru&source=auth_switcher");
                Console.WriteLine(Driver.Title);
                Driver.Manage().Window.Maximize();

                Login();

                Go(tag, likes);

                Driver.Quit();
            }
            catch (Exception ex)
            {
                Logger.Logger.Error(ex.ToString());
            }
            finally
            {
                Driver.Quit();
            }
        }

        static void Login()
        {
            try
            {
                var login = $"{Configuration["insta:Login"]}";
                var password = $"{Configuration["insta:Password"]}";

                var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
                WebDriverExt.UntilForElementExists(wait, By.XPath(".//input[@name='username']"), "не найдено поле ввода username");

                Driver.FindElement(By.XPath(".//input[@name='username']")).SendKeys(login);
                Driver.FindElement(By.XPath(".//input[@name='password']")).SendKeys(password);
                Driver.FindElement(By.XPath(".//button[@type='submit']")).Click();

                wait.UntilForElementExists(By.XPath(".//div[contains(@class, 'mt3GC')]"), "вероятно, окно не загрузилось");
                var noteForm = Driver.FindElement(By.XPath(".//div[contains(@class, 'mt3GC')]"));
                noteForm.FindElement(By.XPath(".//*[text()='Не сейчас']/..")).Click();
            }
            catch (Exception ex)
            {
                Logger.Logger.Error(ex.ToString());
            }
        }

        static void Go(string tag, int likes)
        {
            try
            {
                Logger.Logger.Info($"Хештег: {tag}");

                Driver.Navigate().GoToUrl($"https://www.instagram.com/explore/tags/{tag}/");

                var resultPane = Driver.FindElement(By.XPath(".//article"));
                resultPane.FindElement(By.XPath(".//a")).Click();

                var session = 1;
                for (var i = 0; i < likes; i++)
                {
                    Like();
                    Thread.Sleep(5000);
                    if (i > 50 * session)
                    {
                        Random random = new Random();
                        Console.WriteLine($"Отдыхаем 5 минут");

                        Thread.Sleep(300000);

                        session++;
                    }
                    Thread.Sleep(5000);
                    Next();
                    Thread.Sleep(5000);
                }

                Logger.Logger.Info($"Поставлено лайков: {LikeCount}");
            }

            catch (Exception ex)
            {
                Logger.Logger.Error(ex.ToString());
                Logger.Logger.Info($"Поставлено лайков: {LikeCount}");

                throw;
            }
        }

        static void Like()
        {
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(300));
            WebDriverExt.UntilForElementExists(wait, By.XPath(".//span/button[contains(@class, 'wpO6b')]"), "не найдена кнопка для лайка");

            bool canLike = Driver.FindElements(By.XPath(".//span/button[contains(@class, 'wpO6b')]")).Count > 0;
            if (canLike)
            {
                var button = Driver.FindElement(By.XPath(".//span/button[contains(@class, 'wpO6b')]"));

                var nowLike = button.FindElements(By.XPath(".//span[contains(@class, 'FY9nT')]")).Count;
                if (nowLike > 0)
                {
                    Console.WriteLine($"Лайк уже поставлен");
                }
                else
                {
                    Driver.FindElement(By.XPath(".//span/button[contains(@class, 'wpO6b ')]")).Click();
                    LikeCount++;
                    Console.WriteLine($"Поставлено лайков: {LikeCount}");
                }

                //if (likeCount % 100 == 0)
                //{
                Logger.Logger.Info($"Поставлено лайков: {LikeCount}");
                //}
            }
        }

        static void Next()
        {
            Driver.FindElement(By.XPath(".//a[contains(@class, 'coreSpriteRightPaginationArrow')]")).Click();
        }
    }
}
