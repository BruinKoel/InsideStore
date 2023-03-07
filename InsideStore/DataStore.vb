Imports System.ComponentModel
Imports System.IO
Imports System.IO.MemoryMappedFiles
Imports System.Net.Mime.MediaTypeNames
Imports System.Runtime.InteropServices


Public Module DataStore


    <DllImport("kernel32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Private Function OpenFileMapping(dwDesiredAccess As UInteger, bInheritHandle As Boolean, lpName As String) As IntPtr
    End Function

    Private Const FILE_MAP_READ As UInteger = &H4
    Private Const FILE_MAP_WRITE As UInteger = &H2


    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Function CloseHandle(hObject As IntPtr) As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function


    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Function UnmapViewOfFile(lpBaseAddress As IntPtr) As <MarshalAs(UnmanagedType.Bool)> Boolean
    End Function

    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Function MapViewOfFile(hFileMappingObject As IntPtr, dwDesiredAccess As UInteger, dwFileOffsetHigh As UInteger, dwFileOffsetLow As UInteger, dwNumberOfBytesToMap As IntPtr) As IntPtr
    End Function


    Private startIndex As Int64

    ' Get the handle of the executable file
    Private fileHandle As IntPtr


    Private mainFile As String
    Private tempFile As String

    Private buffer As List(Of Byte)
    Public ReadOnly Property Size As Int64

        Get
            Return Size = buffer.Count - 8 - startIndex
        End Get

    End Property


    Public Sub Initialize()

        mainFile = FileSystem.CurDir() & "\InsideStore.exe"
        tempFile = FileSystem.CurDir() & "\InsideStore.tmp"

        buffer = File.ReadAllBytes(mainFile).ToList()


        startIndex = BitConverter.ToInt64(buffer.TakeLast(8).ToArray(), 0)

        fileHandle = OpenFileMapping(FILE_MAP_READ, False, mainFile)

        If startIndex = 0 Then
            startIndex = buffer.Count - 8

            buffer.RemoveRange(startIndex, 8)
            buffer.AddRange(BitConverter.GetBytes(startIndex))


        End If

        AddHandler AppDomain.CurrentDomain.ProcessExit, AddressOf OnApplicationExit


        Console.WriteLine(mainFile)

    End Sub




    Function MapEntireFile(filename As String) As Byte()
        Dim fileStream As FileStream = Nothing
        Dim fileMapping As MemoryMappedFile = Nothing
        Dim fileMapView As MemoryMappedViewAccessor = Nothing
        Dim data As Byte()

        Try
            ' Open the file in read-only mode
            fileStream = New FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)


            fileMapping = MemoryMappedFile.CreateFromFile(fileStream, Nothing, 0, MemoryMappedFileAccess.Read, HandleInheritability.None, False)
            fileMapView = fileMapping.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read)

            data = New Byte(CInt(fileStream.Length) - 1) {}
            Dim ptr As IntPtr = fileMapView.SafeMemoryMappedViewHandle.DangerousGetHandle()
            Marshal.Copy(ptr, data, 0, data.Length)

            ' Unmap the memory-mapped view
            fileMapView.SafeMemoryMappedViewHandle.ReleasePointer()
            fileMapView.Dispose()
            fileMapping.Dispose()


            Return data
        Finally

            If fileStream IsNot Nothing Then fileStream.Close()
        End Try
    End Function

    Sub DetachFileFromProcess(filename As String)
        Dim fileStream As FileStream = Nothing
        Dim fileMapping As MemoryMappedFile = Nothing
        Dim fileMapView As MemoryMappedViewAccessor = Nothing

        Try

            fileStream = New FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
            fileMapping = MemoryMappedFile.CreateFromFile(fileStream, Nothing, 0, MemoryMappedFileAccess.Read, HandleInheritability.None, False)

            ' Map the entire file to memory
            fileMapView = fileMapping.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read)

            fileMapView.SafeMemoryMappedViewHandle.ReleasePointer()
            fileMapView.Dispose()
            fileMapping.Dispose()
        Finally
            ' Close the fie stream
            If fileStream IsNot Nothing Then fileStream.Close()
        End Try
    End Sub




    Private Sub OnApplicationExit(ByVal sender As Object, ByVal e As EventArgs)
        ' Copy the contents of the temporry file to the executable file
        DetachFileFromProcess(mainFile)
        File.WriteAllBytes(tempFile, buffer.ToArray())

        File.Copy(tempFile, mainFile, True)

    End Sub

End Module
