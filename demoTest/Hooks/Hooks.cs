using AventStack.ExtentReports;
using AventStack.ExtentReports.Gherkin.Model;
using BoDi;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using demoTest.Utility;
using demoTest.Configuration;
using Microsoft.Extensions.Configuration;
using AventStack.ExtentReports.Configuration;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Edge;
using Allure.Net.Commons;
using TechTalk.SpecFlow;

namespace demoTest.Hooks
{
    [Binding]
    public sealed class Hooks : ExtentReport
    {
        private readonly IObjectContainer _container;
        public static ConfigSetting config;
        private static ThreadLocal<IWebDriver> _driver = new ThreadLocal<IWebDriver>(() => new ChromeDriver());
        static IWebDriver driver => _driver.Value;

        static string configsettingpath = System.IO.Directory.GetParent(@"../../../").FullName
            + Path.DirectorySeparatorChar + "Configuration/configsetting.json";

        static string clearResultsBatchFilePath = System.IO.Directory.GetParent(@"../../../").FullName
            + Path.DirectorySeparatorChar + "BatchFiles\\clear-allure-results.bat";

        static string generateResultsBatchFilePath = System.IO.Directory.GetParent(@"../../../").FullName
            + Path.DirectorySeparatorChar + "BatchFiles\\generate-allure-report.bat";
        public Hooks(IObjectContainer container)
        {
            _container = container;
        }

        [BeforeTestRun]
        public static void BeforeTestRun()
        {
            RunBatchFile(clearResultsBatchFilePath);
            config = new ConfigSetting();
            ConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddJsonFile(configsettingpath);
            IConfigurationRoot configuration = builder.Build();
            configuration.Bind(config);
            Console.WriteLine("Running before test run...");
            ExtentReportInit();
        }

        [AfterTestRun]
        public static void AfterTestRun()
        {
            Console.WriteLine("Running after test run...");
            ExtentReportTearDown();
            //RunBatchFile(generateResultsBatchFilePath);
        }

        [BeforeFeature]
        public static void BeforeFeature(FeatureContext featureContext)
        {
            Console.WriteLine("Running before feature...");
            _feature = _extentReports.CreateTest<Feature>(featureContext.FeatureInfo.Title);
        }

        [AfterFeature]
        public static void AfterFeature()
        {
            Console.WriteLine("Running after feature...");
        }

        [BeforeScenario("@Testers")]
        public void BeforeScenarioWithTag()
        {
            Console.WriteLine("Running inside tagged hooks in specflow");
        }

        [BeforeScenario(Order = 1)]
        public void FirstBeforeScenario(ScenarioContext scenarioContext)
        {
            Console.WriteLine("Running before scenario...");
            _driver.Value = GetWebDriver();
            
            _container.RegisterInstanceAs<IWebDriver>(_driver.Value);

            _scenario = _feature.CreateNode<Scenario>(scenarioContext.ScenarioInfo.Title);
            scenarioContext["StepLogs"] = new List<string>(); // Initialize a log list for the scenario

        }

        public IWebDriver GetWebDriver() {
            if (Hooks.config.BrowserType.ToLower() == "chrome")
            {
                _driver.Value = new ChromeDriver();
            }
            else if (Hooks.config.BrowserType.ToLower() == "firefox")
            {
                _driver.Value = new FirefoxDriver();
            }
            else if (Hooks.config.BrowserType.ToLower() == "edge")
            {
                _driver.Value = new EdgeDriver();
            }
            _driver.Value.Manage().Window.Maximize();

            return _driver.Value;
        }

        [AfterScenario]
        public void AfterScenario()
        {
            Console.WriteLine("Running after scenario...");
            var driver = _container.Resolve<IWebDriver>();
            if (driver != null) {
                driver.Quit();
                driver.Dispose();
            } 
        if (_driver.IsValueCreated)
            {
            _driver.Value.Quit();  // Quit the WebDriver instance
            _driver.Value.Dispose();  // Dispose the WebDriver resources
            _driver = new ThreadLocal<IWebDriver>(() => null);  // Clean up the WebDriver
             }
        }

        [AfterStep]
        public void AfterStep(ScenarioContext scenarioContext)
        {
            Console.WriteLine("Running after step....");
            string stepType = scenarioContext.StepContext.StepInfo.StepDefinitionType.ToString();
            string stepName = scenarioContext.StepContext.StepInfo.Text;
            var stepLogs = scenarioContext["StepLogs"] as List<string>;

            //var driver = _container.Resolve<IWebDriver>();

            //When scenario passed
            if (scenarioContext.TestError == null)
            {
                //if (stepType == "Given")
                //{
                //    _scenario.CreateNode<Given>(stepName);
                //}
                //else if (stepType == "When")
                //{
                //    _scenario.CreateNode<When>(stepName);
                //}
                //else if (stepType == "Then")
                //{
                //    _scenario.CreateNode<Then>(stepName);
                //}
                //else if (stepType == "And")
                //{
                //    _scenario.CreateNode<And>(stepName);
                //}
                ExtentTest stepNode = stepType switch
                {
                    "Given" => _scenario.CreateNode<Given>(stepName),
                    "When" => _scenario.CreateNode<When>(stepName),
                    "Then" => _scenario.CreateNode<Then>(stepName),
                    _ => _scenario.CreateNode<And>(stepName)
                };

                // Log each stored message to the step node
                if (stepLogs != null)
                {
                    foreach (var log in stepLogs)
                    {
                        stepNode.Info(log);
                    }
                }
            }

            //When scenario fails
            if (scenarioContext.TestError != null)
            {
                byte[] content = CaptureScreenshot();
                AllureApi.AddAttachment("Failed Screenshot", "image/png", content);
                if (stepType == "Given")
                {
                    _scenario.CreateNode<Given>(stepName).Fail(scenarioContext.TestError.Message,
                        MediaEntityBuilder.CreateScreenCaptureFromPath(addScreenshot(_driver.Value, scenarioContext)).Build()).Log(AventStack.ExtentReports.Status.Fail, scenarioContext.TestError.StackTrace);
                }
                else if (stepType == "When")
                {
                    _scenario.CreateNode<When>(stepName).Fail(scenarioContext.TestError.Message,
                        MediaEntityBuilder.CreateScreenCaptureFromPath(addScreenshot(_driver.Value, scenarioContext)).Build()).Log(AventStack.ExtentReports.Status.Fail, scenarioContext.TestError.StackTrace);
                }
                else if (stepType == "Then")
                {
                    _scenario.CreateNode<Then>(stepName).Fail(scenarioContext.TestError.Message,
                        MediaEntityBuilder.CreateScreenCaptureFromPath(addScreenshot(_driver.Value, scenarioContext)).Build()).Log(AventStack.ExtentReports.Status.Fail, scenarioContext.TestError.StackTrace);
                }
                else if (stepType == "And")
                {
                    _scenario.CreateNode<And>(stepName).Fail(scenarioContext.TestError.Message,
                        MediaEntityBuilder.CreateScreenCaptureFromPath(addScreenshot(_driver.Value, scenarioContext)).Build()).Log(AventStack.ExtentReports.Status.Fail, scenarioContext.TestError.StackTrace);
                }
            }
            scenarioContext["StepLogs"] = new List<string>();

        }

        public static byte[] CaptureScreenshot()
        {
            return ((ITakesScreenshot)_driver.Value).GetScreenshot().AsByteArray;
        }

        private static void RunBatchFile(string batchFilePath)
        {
            var processInfo = new System.Diagnostics.ProcessStartInfo("cmd.exe", "/C \"" + batchFilePath + "\"")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true, // Redirect error output
                UseShellExecute = false,
                CreateNoWindow = true // Set to true for background execution without a window
            };

            using (var process = System.Diagnostics.Process.Start(processInfo))
            {
                // Capture the output and errors
                string output = process.StandardOutput.ReadToEnd();
                string errorOutput = process.StandardError.ReadToEnd();

                // Wait for the process to exit
                process.WaitForExit();

                // Check the exit code
                if (process.ExitCode != 0)
                {
                    Console.WriteLine("Batch file execution failed with exit code: " + process.ExitCode);
                    Console.WriteLine("Error Output: " + errorOutput);
                    // Handle the failure accordingly, e.g., logging or throwing an exception
                    throw new Exception("Batch file execution failed: " + errorOutput);
                }

                // If needed, process the output
                Console.WriteLine("Batch output: " + output);
            }
        }
    }
}