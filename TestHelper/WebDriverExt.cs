using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace InstaService
{
    public static class WebDriverExt
    {
        public static SelectElement FindSelectList(this IWebDriver driver, By selector)
        {
            var findElement = driver.FindElement(selector);
            var positionSelect = new SelectElement(findElement);

            return positionSelect;
        }

        public static IWebElement UntilForElementExists(this WebDriverWait wait, By selector, string message)
        {
            try
            {
                return wait.Until(ExpectedConditions.ElementExists(selector));
            }
            catch (WebDriverTimeoutException)
            {
                throw new Exception(
                    string.Format(
                        (string)"Не найден элемент {0}. {1}",
                        (object)selector, message));
            }
        }

        public static IWebElement UntilForElementVisible(this WebDriverWait wait, By selector, string message)
        {
            try
            {
                return wait.Until(ExpectedConditions.ElementIsVisible(selector));
            }
            catch (WebDriverTimeoutException)
            {
                throw new Exception(
                    string.Format(
                        (string)"Не найден элемент {0}. {1}",
                        (object)selector, message));
            }
        }

        public static IWebElement UntilForElementVisivle(this WebDriverWait wait, By selector, string message)
        {
            try
            {
                return wait.Until(ExpectedConditions.ElementIsVisible(selector));
            }
            catch (WebDriverTimeoutException)
            {
                throw new Exception(
                    string.Format(
                        (string)"Не виден элемент {0}. {1}",
                        (object)selector, message));
            }
        }

        public static object ExecuteScript(this IWebDriver driver, string script, params object[] args)
        {
            return (driver as IJavaScriptExecutor).ExecuteScript(script, args);
        }
    }
}
