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
        public void CheckProcess_ShouldStartNotepad()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            Process? process = null;

            try
            {
                // Act
                process = Process.Start("notepad.exe");

                // Assert
                Assert.IsNotNull(process, "Failed to start notepad.exe");
            }
            finally
            {
                cts.Cancel();
                process?.Kill();
                process?.Dispose();
            }
        }

        [Test]
        public async Task CheckProcess_ShouldKillNotepadAfterLifetime()
        {
            // Arrange
            string processName = "notepad";
            int lifeTime = 1;
            var cts = new CancellationTokenSource();
            Process? process = null;

            try
            {
                process = Process.Start("notepad.exe");
                await Task.Delay(TimeSpan.FromMinutes(lifeTime + 0.5)); // Wait for 1.5 minutes

                // Act
                await Program.RunMonitor(processName, lifeTime, cts.Token);

                // Allow some time for the process to exit
                int retryCount = 5;
                bool processExists;
                do
                {
                    await Task.Delay(1000);
                    processExists = Process.GetProcessesByName(processName).Length > 0;
                    retryCount--;
                } while (processExists && retryCount > 0);

                // Assert
                Assert.IsFalse(processExists, "The process was not killed");
            }
            finally
            {
                cts.Cancel();
                process?.Kill();
                process?.Dispose();
            }
        }


        [Test]
        public void LogToFile_ShouldCreateLogFile()
        {
            // Arrange
            string testLog = "This is a test log.";

            // Act
            Program.LogToFile(testLog);

            // Assert
            bool fileExists = File.Exists(logFilePath);
            Assert.That(fileExists, Is.True, "Log file was not created");

            // Cleanup
            File.Delete(logFilePath);
        }

        [Test]
        public void LogToFile_ShouldContainLoggedContent()
        {
            // Arrange
            string testLog = "This is a test log.";
            Program.LogToFile(testLog);

            // Act
            string loggedContent = File.ReadAllText(logFilePath);

            // Assert
            StringAssert.Contains(testLog, loggedContent, "Logged content does not match the expected content");

            // Cleanup
            File.Delete(logFilePath);
        }

        [Test]
        public async Task KillProcess_ShouldTerminateNotepad()
        {
            // Arrange
            Process? process = null;
            try
            {
                process = Process.Start("notepad.exe");
                Assert.IsNotNull(process, "Failed to start notepad.exe");
                await Task.Delay(2000);
                DateTime startTime = process.StartTime;

                // Act
                string log = Program.KillProcess(process, DateTime.Now - startTime);

                // Allow some time for the process to exit
                int retryCount = 5;
                bool processExists;
                do
                {
                    await Task.Delay(1000);
                    processExists = Process.GetProcessesByName("notepad").Length > 0;
                    retryCount--;
                } while (processExists && retryCount > 0);

                // Assert
                Assert.IsFalse(processExists, "The process was not killed");

            }
            finally
            {
                process?.Kill();
                process?.Dispose();
            }
        }


        [Test]
        public async Task KillProcess_ShouldContainProcessNameInLog()
        {
            // Arrange
            Process? process = null;
            try
            {
                process = Process.Start("notepad.exe");
                await Task.Delay(2000);
                DateTime startTime = process.StartTime;

                // Act
                string log = Program.KillProcess(process, DateTime.Now - startTime);

                // Assert
                StringAssert.Contains("notepad", log, "Log does not contain process name");
            }
            finally
            {
                process?.Kill();
                process?.Dispose();
            }
        }

        [Test]
        public async Task KillProcess_ShouldContainKillConfirmationInLog()
        {
            // Arrange
            Process? process = null;
            try
            {
                process = Process.Start("notepad.exe");
                await Task.Delay(2000);
                DateTime startTime = process.StartTime;

                // Act
                string log = Program.KillProcess(process, DateTime.Now - startTime);

                // Assert
                StringAssert.Contains("was killed", log, "Log does not contain kill confirmation");
            }
            finally
            {
                process?.Kill();
                process?.Dispose();
            }
        }

        [Test]
        public async Task KillProcess_ShouldEnsureProcessHasExited()
        {
            // Arrange
            Process? process = null;
            try
            {
                process = Process.Start("notepad.exe");
                await Task.Delay(2000);
                DateTime startTime = process.StartTime;

                // Act
                Program.KillProcess(process, DateTime.Now - startTime);

                // Assert
                process.Refresh();
                Assert.IsTrue(process.HasExited, "The process has not exited");
            }
            finally
            {
                process?.Kill();
                process?.Dispose();
            }
        }
        [Test]
        public void Main_ShouldHandleInvalidArguments()
        {
            // Arrange
            string[] args = { "notepad", "invalid_lifetime", "1" };
            var sw = new StringWriter();
            Console.SetOut(sw);

            // Act
            Program.Main(args).Wait();

            // Assert
            var result = sw.ToString().Trim();
            Assert.That(result, Is.EqualTo("Invalid lifetime value."));
        }
    }
}
