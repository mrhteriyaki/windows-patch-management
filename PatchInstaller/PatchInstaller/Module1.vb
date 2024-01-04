Imports System.Runtime.CompilerServices
Imports Patch_Management.WindowsPatchManagement

Module Module1

    Sub Main(ByVal args() As String)
        Try
            Dim DriverInstall As Boolean = False
            Dim RebootApproved As Boolean = False
            Dim SoftwareUpdates As Boolean = True


            For Each arg In args
                If arg = "?" Or arg.ToLower = "help" Then
                    Console.WriteLine("Installs windows updates, use argument 'drivers' to include drivers or 'nosoftware' to exclude regular software updates.")
                    Exit Sub
                ElseIf arg = "drivers" Then
                    DriverInstall = True
                ElseIf arg = "reboot" Then
                    RebootApproved = True
                ElseIf arg = "nosoftware" Then
                    SoftwareUpdates = False
                End If
            Next

            Dim rebootrequired As Boolean = InstallUpdates(DriverInstall, SoftwareUpdates)
            If rebootrequired Then
                Console.WriteLine("Reboot required for updates.")
                If RebootApproved Then
                    'Reboot system.
                    Dim rebproc As New Process
                    rebproc.StartInfo.FileName = "C:\Windows\System32\shutdown.exe"
                    rebproc.StartInfo.Arguments = "/r /t 1"
                    rebproc.Start()
                End If
            End If

        Catch ex As Exception
            Console.WriteLine("Error installing updates: " & ex.ToString)
            Environment.ExitCode = 1
        End Try



    End Sub

End Module
