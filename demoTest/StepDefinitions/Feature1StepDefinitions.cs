using OpenQA.Selenium;

namespace demoTest.StepDefinitions
{
    
    
    [Binding]
    public sealed class Feature1StepDefinitions
    {
        private IWebDriver driver;

        public Feature1StepDefinitions(IWebDriver driver)
        {
            this.driver = driver;
        }


        [Given(@"Open the browser")]
        public void GivenOpenTheBrowser()
        {
            //   driver = new ChromeDriver();
            //   driver.Manage().Window.Maximize();
        }

        [When(@"Enter the URL")]
        public void WhenEnterTheURL()
        {
            driver.Url = "https://www.youtube.com/";
            Thread.Sleep(3000);

        }

        [Then(@"Search for the BBC Earth")]
        public void ThenSearchForTheBBCEarth()
        {
            driver.FindElement(By.XPath("//*[@name='search_query']")).SendKeys("BBC Earth");
            driver.FindElement(By.XPath("//*[@name='search_query']")).SendKeys(Keys.Enter);
            Thread.Sleep(5000);
            //  driver.Quit();
        }

    }
}
