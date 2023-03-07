Imports System
Imports System.IO
Imports System.IO.MemoryMappedFiles
Imports System.Reflection
Imports System.Diagnostics

Module Program



    Sub Main()
        ' Get the expected installation directory
        Dim expectedDirectory As String = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) & "\Temp"


        If Not Directory.Exists(expectedDirectory) Then
            Directory.CreateDirectory(expectedDirectory)
        End If

        ' Check if the program is in the correct directory
        If Not Directory.GetCurrentDirectory().Equals(expectedDirectory, StringComparison.OrdinalIgnoreCase) Then
            ' Copy the executable to the correct directory using a separate process
            Dim executablePath As String = Process.GetCurrentProcess().MainModule.FileName
            Dim newExecutablePath As String = Path.Combine(expectedDirectory, Path.GetFileName(executablePath))

            ' Start the copy process
            Dim copyProcess As New Process()
            copyProcess.StartInfo.FileName = "cmd.exe"
            copyProcess.StartInfo.Arguments = String.Format("/c copy ""{0}"" ""{1}"" & {4} & cd ""{2}"" & echo ""{0}"" >> {3} & timeout 20 & start """" ""{3}""", executablePath, newExecutablePath, expectedDirectory, Path.GetFileName(executablePath), expectedDirectory.Substring(0, 2))
            Console.WriteLine(Directory.GetCurrentDirectory())


            copyProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal
            copyProcess.Start()
            Directory.SetCurrentDirectory(expectedDirectory)
            ' Terminate the current process
            Environment.Exit(0)
        End If

        ' Continue running the program normally
        Console.WriteLine("Program is running in the correct directory.")
        Threading.Thread.Sleep(100000)
        Console.ReadLine()
    End Sub


End Module
