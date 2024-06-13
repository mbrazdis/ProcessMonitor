# ProcessMonitor

## Overview

The Monitor Utility is a C# command-line application designed to monitor Windows processes and terminate those that exceed a specified lifetime. The utility accepts three arguments: the process name, maximum lifetime (in minutes), and monitoring frequency (in minutes). The application checks the specified process at the given frequency and terminates any instances running longer than the allowed duration, logging the action. The utility continues monitoring until a special exit key (`q`) is pressed.

## Features

- Monitor specified Windows processes.
- Terminate processes exceeding a specified lifetime.
- Log terminated processes with details.
- Continuous monitoring until the `q` key is pressed.
- NUnit tests covering key functionalities.

## Usage

### Command-Line Arguments

The utility expects three arguments:
1. **Process Name**: The name of the process to monitor (e.g., `notepad`).
2. **Maximum Lifetime**: The maximum allowed runtime for the process in minutes.
3. **Monitoring Frequency**: The frequency at which the utility checks the process in minutes.

### Example

monitor.exe notepad 5 1

This example monitors the `notepad` process every minute and terminates any instances running longer than 5 minutes.


### Stopping the Utility

Press the `q` key to stop the utility.

## Logging

Terminated processes are logged in a file named `logs.txt` in the application's directory. Each log entry includes the timestamp, process name, process ID, and runtime duration.

## Development

### NUnit Tests

- **CheckProcess_ShouldStartNotepad**: Tests if Notepad starts correctly.
-**CheckProcess_ShouldKillNotepadAfterLifetime**: Tests if Notepad is killed after exceeding the specified lifetime.
-**LogToFile_ShouldCreateLogFile**: Tests if the log file is created correctly.
-**LogToFile_ShouldContainLoggedContent**: Tests if the log file contains the expected content.
-**KillProcess_ShouldTerminateNotepad**: Tests if Notepad is terminated correctly.
-**KillProcess_ShouldContainProcessNameInLog**: Tests if the log contains the process name.
-**KillProcess_ShouldContainKillConfirmationInLog**: Tests if the log contains kill confirmation.
-**KillProcess_ShouldEnsureProcessHasExited**: Tests if the process has exited after termination.

## Source Code

### Main Program

The main program logic is contained in `Program.cs`. It includes methods for:

- Parsing command-line arguments.
- Monitoring processes.
- Checking process runtimes.
- Terminating processes and logging the action.
- Listening for the exit key (`q`).

### Tests

NUnit tests are located in the `ProgramTests.cs` file. These tests validate the functionality of key methods in the program.
