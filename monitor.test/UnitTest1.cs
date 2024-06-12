using Microsoft.VisualStudio.TestPlatform.TestHost;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace monitor.Tests
{
    [TestFixture]
    public class ProgramTests
    {
        private string logFilePath;

        [SetUp]
        public void Setup()
        {
            logFilePath = "logs.txt";

            if (File.Exists(logFilePath))
            {
                File.Delete(logFilePath);
            }
        }

        [Test]
        public async Task CheckProcessTest()
        {
            string processName = "notepad";
            int lifeTime = 1;
            Process? process = null;
            var cts = new CancellationTokenSource();

            try
            {
                process = Process.Start("notepad.exe");
                Assert.IsNotNull(process, "Failed to start notepad.exe");

                //wait longer to simulate process running past its lifetime
                await Task.Delay(TimeSpan.FromMinutes(lifeTime + 0.5)); // Wait for 1.5 minutes

                //run monitor
                await Program.RunMonitor(processName, lifeTime, cts.Token);

                //allow some time for the process to exit
                int retryCount = 5;
                bool processExists;
                do
                {
                    await Task.Delay(500); // Wait 500 ms before rechecking
                    processExists = Process.GetProcessesByName(processName).Length > 0;
                    retryCount--;
                } while (processExists && retryCount > 0);

                Assert.IsFalse(processExists, "The process was not killed");
            }
            catch (Exception ex)
            {
                Assert.Fail($"Test failed with exception: {ex.Message}");
            }
            finally
            {
                cts.Cancel();
                if (process != null && !process.HasExited)
                {
                    process.Kill();
                }
                process?.Dispose();
            }
        }

        [Test]
        public void LogToFileTest()
        {
            string testLog = "This is a test log.";

            //log to file
            Program.LogToFile(testLog);

            //verify if the log file is created
            bool fileExists = File.Exists(logFilePath);
            Assert.That(fileExists, Is.True, "Log file was not created");

            //verify the content of the log file
            string loggedContent = File.ReadAllText(logFilePath);
            StringAssert.Contains(testLog, loggedContent, "Logged content does not match the expected content");

            //cleanup
            File.Delete(logFilePath);
        }

        [Test]
        public async Task KillProcessTest()
        {
            Process? process = null;
            try
            {
                //start notepad process
                process = Process.Start("notepad.exe");
                Assert.IsNotNull(process, "Failed to start notepad.exe");

                //allow the process to initialize
                await Task.Delay(2000);

                //capture the start time for log comparison
                DateTime startTime = process.StartTime;

                //kill the process and capture the log
                string log = Program.KillProcess(process, DateTime.Now - startTime);

                //verify if process is killed
                bool processExists = Process.GetProcessesByName("notepad").Length > 0;
                Assert.IsFalse(processExists, "The process was not killed");

                //verify the log contains expected information
                StringAssert.Contains("notepad", log, "Log does not contain process name");
                StringAssert.Contains("was killed", log, "Log does not contain kill confirmation");

                //additional checks to ensure the process has exited
                process.Refresh();
                Assert.IsTrue(process.HasExited, "The process has not exited");
            }
            catch (Exception ex)
            {
                Assert.Fail($"Test failed with exception: {ex.Message}");
            }
            finally
            {
                process?.CloseMainWindow();
                process?.Dispose();
            }
        }
    }
}
