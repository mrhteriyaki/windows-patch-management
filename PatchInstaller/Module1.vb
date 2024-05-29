Imports System.Runtime.CompilerServices
Imports Patch_Management
Imports Patch_Management.WindowsPatchManagement

Module Module1

    Sub Main(ByVal args() As String)
        Try
            Dim DriverInstall As Boolean = False
            Dim RebootApproved As Boolean = False
            Dim SoftwareUpdates As Boolean = False

            If args.Count = 0 Then
                Help()
                Exit Sub
            End If

            For Each arg In args
                If arg = "?" Or arg.ToLower = "help" Then
                    Help()
                    Exit Sub
                ElseIf arg = "-drivers" Then
                    DriverInstall = True
                ElseIf arg = "-reboot" Then
                    RebootApproved = True
                ElseIf arg = "-updates" Then
                    SoftwareUpdates = True
                ElseIf arg.Equals("history") Then
                    Console.WriteLine("Install Date,HResult Code,HResult Description,Revision,Category,Title,Description")
                    For Each WUpdate In GetUpdateHistory()
                        Console.WriteLine(WUpdate.InstallDate & "," & WUpdate.InstallResult.ToString("X") & "," & WindowsPatchManagement.GetHCODEDescription(WUpdate.InstallResult) & "," & WUpdate.Revision & "," & WUpdate.Category & "," & WUpdate.Title & "," & WUpdate.Description)
                    Next
                    Exit Sub
                Else
                    Console.WriteLine("Invalid Argument: " & arg)
                    Exit Sub
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

        'Potential speed improvement using offline database of updates:  wsusscn2.cab
        'https://stackoverflow.com/questions/27337433/slow-wua-windows-update-api

        'https://support.microsoft.com/en-au/topic/a-new-version-of-the-windows-update-offline-scan-file-wsusscn2-cab-is-available-for-advanced-users-fe433f4d-44f4-28e3-88c5-5b22329c0a08


    End Sub

    Sub Help()
        Console.WriteLine("Patch Installer - Mitchell Hayden")
        Console.WriteLine("Installs windows updates.")
        Console.WriteLine("Arguments available:")
        Console.WriteLine("-updates     Include Windows Software Updates.")
        Console.WriteLine("-drivers     Include Hardware driver updates.")
        Console.WriteLine("-reboot      Reboot if required when updates completed.")
        Console.WriteLine("history      Show Windows update history for device.")
    End Sub

End Module
