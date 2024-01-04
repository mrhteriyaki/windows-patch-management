Imports System.IO
Imports System.Runtime.InteropServices
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Serialization
Imports WUApiLib

Public Class WindowsPatchManagement
    'Microsoft API Docs : https://docs.microsoft.com/en-us/windows/win32/api/wuapi/nn-wuapi-iupdatesearcher

    'Required Refs (add to project)
    'WUApiLib 

    'Additional ref:
    'https://learn.microsoft.com/en-us/windows/win32/wua_sdk/searching--downloading--and-installing-updates


    'Public Shared updatesToDownload As Object
    'Public Shared downloader As Object
    'Public Shared updatesToInstall As Object
    'Public Shared strInput As String
    'Public Shared installer As Object
    'Public Shared installationResult As Object

    Public Class WUpdateHistory
        <JsonProperty(PropertyName:="title")>
        Public Title As String
        <JsonProperty(PropertyName:="date")>
        Public InstallDate As String
        <JsonProperty(PropertyName:="description")>
        Public Description As String
        <JsonProperty(PropertyName:="rev")>
        Public Revision As String
        <JsonProperty(PropertyName:="category")>
        Public Category As String

    End Class

    Public Class WUpdate
        <JsonProperty(PropertyName:="title")>
        Public Title As String
        <JsonProperty(PropertyName:="type")>
        Public Type As Integer '1 = Software, 2 = Driver.
        <JsonProperty(PropertyName:="downloaded")>
        Public Downloaded As Integer
        <JsonProperty(PropertyName:="product")> 'update category name.
        Public Product As Integer
    End Class


    Public Shared Function GetUpdateHistory() As List(Of WUpdateHistory)
        'Returns list of installed windows updates.


        Dim updateSession As Object = CreateObject("Microsoft.Update.Session")
        updateSession.ClientApplicationID = "GI System Client Monitoring Service"

        Dim updateSearcher As Object = updateSession.CreateupdateSearcher()
        Dim UpdateCount As Integer = updateSession.CreateupdateSearcher().GetTotalHistoryCount
        Dim searchResult As Object = updateSearcher.QueryHistory(0, UpdateCount)


        Dim WHistory As New List(Of WUpdateHistory)
        For I = 0 To searchResult.Count - 1
            Dim update As Object = searchResult.Item(I)
            Dim tWUpdateHistory As New WUpdateHistory
            tWUpdateHistory.Title = update.Title
            tWUpdateHistory.InstallDate = DateTime.Parse(update.Date).ToString("yyyy-MM-dd HH:mm:ss") 'format for SQL.
            tWUpdateHistory.Description = update.Description

            Dim UpdateIdentity As Object = update.UpdateIdentity
            tWUpdateHistory.Revision = UpdateIdentity.RevisionNumber

            Dim Categories As Object = update.Categories
            Dim CatString As String = ""
            For Each Cat In Categories
                CatString = CatString & Cat.Name & ","
            Next
            If CatString.EndsWith(",") Then
                CatString = CatString.Substring(0, CatString.Length - 1)
            End If

            tWUpdateHistory.Category = CatString


            'Skip updates without title.
            If tWUpdateHistory.Title = "" Then
                Continue For
            End If

            WHistory.Add(tWUpdateHistory)
        Next

        Return WHistory
    End Function



    Public Shared Function GetUpdatePending() As List(Of WUpdate)

        'Dim updateSession As Object = CreateObject("Microsoft.Update.Session")

        Dim updateSession As New UpdateSession
        Dim updateSearcher As UpdateSearcher = updateSession.CreateUpdateSearcher()

        Dim WUpdateList As New List(Of WUpdate)

        'Alternate query = IsInstalled=0 and Type='Software'
        Dim searchResult As Object = updateSearcher.Search("IsInstalled = 0")
        Console.WriteLine("Checking for Updates:")
        For I = 0 To searchResult.Updates.Count - 1
            Dim tU As New WUpdate
            Dim update As IUpdate = searchResult.Updates.Item(I)
            tU.Title = update.Title
            tU.Type = update.Type
            Console.WriteLine("Update Found:" & tU.Title)
            If update.IsDownloaded = True Then
                tU.Downloaded = 1
            Else
                tU.Downloaded = 0
            End If
            WUpdateList.Add(tU)
        Next
        Return WUpdateList

    End Function



    Public Shared Function InstallUpdates(Optional IncludeDrivers As Boolean = False, Optional IncludeSoftware As Boolean = True) As Boolean 'Return if shutdown required.


        Dim updateSession As New UpdateSession
        Dim updateSearcher As UpdateSearcher = updateSession.CreateUpdateSearcher()
        updateSession.ClientApplicationID = "VBWindowsUpdateLibrary" 'appName

        'Fetch available updates.
        'Alternate query = IsInstalled=0 and Type='Software'
        'more info at https://learn.microsoft.com/en-us/windows/win32/api/wuapi/nf-wuapi-iupdatesearcher-search

        'IUpdate Type integer values are 1 = Software, 2 = Driver.
        'https://learn.microsoft.com/en-us/windows/win32/api/wuapi/ne-wuapi-updatetype

        Console.WriteLine("Searching for Updates.")
        Dim searchResult As ISearchResult
        Try
            searchResult = updateSearcher.Search("IsInstalled=0 And IsHidden=0")
        Catch ex As Exception
            Console.WriteLine("Error searching for updates. Exception:" & ex.ToString)
            Return False
        End Try

        If searchResult.Updates.Count = 0 Then
            Console.WriteLine("No Updates available to install.")
            Return False
        End If

        Console.WriteLine("Pending Updates:")
        Dim updateCol As New UpdateCollection
        For I = 0 To searchResult.Updates.Count - 1
            Dim update As IUpdate = searchResult.Updates.Item(I)
            If update.Type = UpdateType.utDriver And IncludeDrivers Then
                'Only install drivers if requested.
                Console.WriteLine("Driver: " & update.Title)
                updateCol.Add(update)
            ElseIf update.Type = UpdateType.utSoftware And IncludeSoftware Then
                'Install software updates unless excluded.
                Console.WriteLine("Software:" & update.Title)
                updateCol.Add(update)
            End If

        Next
        Console.WriteLine()

        If updateCol.Count = 0 Then
            'No Updates for installation, skip remaining.
            Console.WriteLine("No updates available.")
            Return False
        End If

        'Download Updates.
        Console.WriteLine(vbCrLf & "Downloading updates.")
        Dim downloader As UpdateDownloader = updateSession.CreateUpdateDownloader()
        downloader.Updates = updateCol
        downloader.Download()

        'Console.WriteLine(vbCrLf & "List of downloaded updates:")
        'For I = 0 To searchResult.Updates.Count - 1
        'Dim update As IUpdate = searchResult.Updates.Item(I)
        'If update.IsDownloaded Then
        'Console.WriteLine(I + 1 & "> " & update.Title)
        'End If
        'Next


        Console.WriteLine("Creating Update Installer")
        Dim installer As UpdateInstaller = updateSession.CreateUpdateInstaller()
        installer.Updates = updateCol

        Console.WriteLine("Installing " & updateCol.Count.ToString & " updates, please wait this may take a while.")
        Dim installationResult As IInstallationResult = installer.Install()

        'Output results of install
        Console.WriteLine("Installation Result: " & installationResult.ResultCode)
        Console.WriteLine("Reboot Required: " & installationResult.RebootRequired & vbCrLf)
        Console.WriteLine("Listing of updates installed and individual installation results:")

        For I = 0 To updateCol.Count - 1
            Console.WriteLine(I + 1 & "> " & updateCol.Item(I).Title & ": " & installationResult.GetUpdateResult(I).ResultCode)
        Next

        If installationResult.RebootRequired = True Then
            Return True
        End If
        Return False
    End Function



End Class
