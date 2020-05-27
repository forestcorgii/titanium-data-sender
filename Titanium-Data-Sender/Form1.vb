Imports System.Net
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Data.OleDb

Imports Newtonsoft.Json
Imports System.ComponentModel

Imports confsaver_xml
Imports Titanium_Data_Sender.Classes

Public Class Form1
    Private formName As String
    Private dots As String = ""
    Private WithEvents workers As sean_workers.Workers

    Private conf As LogInf
    Private confPath As String

    Private Status As Statuses

    Private WithEvents _backup As BackupLogs
    Private Property Backup As BackupLogs
        Get
            If _backup.day <> Now.Day Then
                If Not BackupFileInfo.Exists Then
                    XmlSerialization.WriteToFile(BackupFileInfo.FullName, New BackupLogs)
                End If
                _backup = XmlSerialization.ReadFromFile(BackupFileInfo.FullName, New BackupLogs)
                _backup.day = Now.Day
            End If

            Return _backup
        End Get
        Set(value As BackupLogs)
            _backup = value
        End Set
    End Property


    Private ReadOnly Property BackupFileInfo As FileInfo
        Get
            Return New FileInfo(String.Format("{0}\logs\{1}.config.xml", Application.StartupPath, Now.ToString("yyyy-MM-dd")))
        End Get
    End Property


    Private Sub Form_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        formName = Application.ProductName & " v" & Application.ProductVersion
        Me.Text = formName

        Status = New Statuses
        ChangeStatus(Status.ToString)

        workers = New sean_workers.Workers
        workers.SetWorker(3)

        conf = New LogInf
        confPath = Path.Combine(Application.StartupPath, Application.ProductName & ".config.xml")
        If File.Exists(confPath) Then
            conf = XmlSerialization.ReadFromFile(confPath, New LogInf)

            For i As Integer = 0 To conf.Queues.Count - 1
                workers.AddtoQueue({conf.Queues(i), -1})
            Next

            LastRecord = conf.LastLog
        End If

        conf.Path = confPath

        If Not setup() Then Me.Close() : Exit Sub

        _backup = New BackupLogs
        If Not BackupFileInfo.Directory.Exists Then
            BackupFileInfo.Directory.Create()
        End If


        start()
        cbSite.SelectedIndex = conf.Site
        Status.OnQueues = workers.QueueArgs.Count
    End Sub

    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        TimekeeperConnection.Close()
        MasterfileConnection.Close()
        'saveConf()
        If _backup IsNot Nothing Then
            XmlSerialization.WriteToFile(BackupFileInfo.FullName, Backup)
            conf.AddQueues(workers.QueueArgs)
        End If
    End Sub

    Private Sub btnFilter_Click(sender As Object, e As EventArgs) Handles btnFilter.Click
        workers.StopWorking()
        Using filter As New frmSettings(conf)
            If filter.ShowDialog() = DialogResult.Yes Then
                conf = filter.Conf
                saveConf()
            End If
        End Using
        workers.StartWorking()
    End Sub

    Private Sub cbSite_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbSite.SelectedIndexChanged
        conf.Site = cbSite.SelectedIndex
    End Sub

    Private Sub tm_Tick(sender As Object, e As EventArgs) Handles tmBuffer.Tick
        If dots.Length = 3 Then
            dots = ""
        Else : dots &= "."
        End If
        ChangeStatus(Status.ToString & dots)
    End Sub

    Private recentModifiedDate As Date
    Private Sub tmTracker_Tick(sender As Object, e As EventArgs) Handles tmTracker.Tick
        tmTracker.Enabled = False
        If Not timekeeper.LastWriteTime = recentModifiedDate Then
            recentModifiedDate = timekeeper.LastWriteTime
            ReadData()
            'If workers.QueueArgs.Count > 0 Then
            '    workers.StartWorking()
            'End If
        End If
        'If Not workers.IsBusy Then workers.StopWorking()
        tmTracker.Enabled = True
    End Sub

    Private Sub tmResender_Tick(sender As Object, e As EventArgs) Handles tmResender.Tick
        tmResender.Enabled = False
        If workers.QueueArgs.Count = 0 Then
            Resend()
        End If
        tmResender.Enabled = True
    End Sub

    Private Sub workers_Worker_DoWork(sender As Object, e As DoWorkEventArgs) Handles workers.Worker_DoWork
        Try
            Dim tiData As TitaniumData = DirectCast(e.Argument(0), TitaniumData)
            Dim isNew As Boolean = e.Argument(1) < 0

            Dim item As New ListViewItem({tiData.bio_id, tiData.site, tiData.added_ts})
            item.ForeColor = Nothing
            Dim statusSub As New ListViewItem.ListViewSubItem
            statusSub.Text = SendDataLog(tiData.Clone)

            item.SubItems.Add(statusSub)

            lstLogs.Invoke(Sub()
                               If isNew Then
                                   lstLogs.Items.Insert(0, item)
                                   Backup.Add(New BackupItem With {.Data = tiData.Clone, .Message = statusSub.Text, .TimeSent = Now})
                               Else
                                   If statusSub.Text.ToUpper.Contains("SUCCESS") Then lstLogs.Items.Insert(0, item)
                                   Backup.Item(e.Argument(1)) = (New BackupItem With {.Data = tiData.Clone, .Message = statusSub.Text, .TimeSent = Now})
                               End If
                           End Sub)
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Sub workers_Worker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles workers.Worker_RunWorkerCompleted
        Status.OnQueues = workers.QueueArgs.Count
    End Sub

    Private Sub _backup_ItemChanged() Handles _backup.ItemChanged
        XmlSerialization.WriteToFile(BackupFileInfo.FullName, Backup)
    End Sub





    Dim masterfiles As FileInfo
    Dim timekeeper As FileInfo
    Private Function setup() As Boolean

        For Each defaulttimekeeperdir As String In {Path.Combine(Environment.ExpandEnvironmentVariables("%ProgramFiles(x86)%"), "Titanium", "Timekeeper"), "D:\Titanium\Timekeeper"}
            masterfiles = New FileInfo(defaulttimekeeperdir & "\masterfiles.mdb")
            timekeeper = New FileInfo(defaulttimekeeperdir & "\timekeeper.mdb")
            If (masterfiles.Exists And timekeeper.Exists) Then
                manager_mdb.manager.Open(masterfiles.FullName, MasterfileConnection)
                manager_mdb.manager.Open(timekeeper.FullName, TimekeeperConnection)
                Status.Connected = True
                Return True
            End If
        Next


        Dim timekeeperdir As String = ""
pop:    If Not (masterfiles.Exists And timekeeper.Exists) Then
            MsgBox("Default titanium path not found, please select a valid titanium path", MsgBoxStyle.Critical)
            If FBD.ShowDialog = DialogResult.OK Then
                timekeeperdir = Path.Combine(FBD.SelectedPath, "Timekeeper")
                masterfiles = New FileInfo(timekeeperdir & "\masterfiles.mdb")
                timekeeper = New FileInfo(timekeeperdir & "\timekeeper.mdb")
                GoTo pop
            Else
                Return False
            End If
        End If

        manager_mdb.manager.Open(masterfiles.FullName, MasterfileConnection)
        manager_mdb.manager.Open(timekeeper.FullName, TimekeeperConnection)
        Status.Connected = True
        Return True
    End Function

    Private Sub start()
        tmResender.Interval = 1000 * 15
        tmBuffer.Interval = 1000 * 2
        tmTracker.Interval = 1000 * 3

        tmResender.Enabled = True
        tmBuffer.Enabled = True
        tmTracker.Enabled = True
        Status.Running = True
    End Sub


    Private Sub Test()
        'DatabaseManagement.MDBConfiguration.Open("D:\timekeeper.mdb", TimekeeperConnection)
        'Dim da = DatabaseManagement.MDBConfiguration.getTables(TimekeeperConnection)
        'Console.Write("Asda")

        Dim logDatetime As String = Now.ToString("yyyy-MM-dd HH:mm:ss")
        MsgBox(SendDataLog(New TitaniumData With {.bio_id = Lookup("13", "idno"), .added_ts = logDatetime, .site = "MANILA"}))
    End Sub

    Private LastRecord As String

    Private Function SendDataLog(tiData As TitaniumData) As String
        Dim responseFromServer As String = ""
        Dim failcnt As Short = 0

        With tiData
            .token = "bf3d9e0a7bfc4ffa9765641530c49242"
            .action = "insert"
        End With

        Dim postData As String = tiData.ToString 'JsonConvert.SerializeObject(tiData)

        'MsgBox(postData)
        While True
            Try
                responseFromServer = SendAPIMessage(postData, "http://idcsi-officesuites.com:8082/upsg/bio_api/API_Receiver")
                If responseFromServer.ToUpper.Contains("!DOCTYPE") Then
                    Return Regex.Match(responseFromServer, "(\<body\>)[\w\d\s \<\>\`\=\""\'\/\:\-\(\)\`\,\.\?\;\[\]\!\~\@\#\$\%\^\&\*]+(\<\/body\>)").Value
                ElseIf responseFromServer.ToUpper.Contains("SUCCESS") _
                    Or responseFromServer.ToUpper.Contains("FAILED") Then

                    Return responseFromServer
                Else
                    If failcnt >= 1 Then
                        Return responseFromServer
                    End If
                    failcnt += 1
                End If
                Return True
            Catch ex As Exception
                Return (ex.Message)
                'Exit While
            End Try
        End While
        Return False
    End Function

    Public Function SendAPIMessage(PostData As String, Address As String) As String
        Dim w As WebRequest = WebRequest.Create(Address)
        w.Method = "POST"
        Dim byteArray As Byte() = Encoding.ASCII.GetBytes(PostData)
        w.ContentType = "application/x-www-form-urlencoded"
        w.ContentLength = byteArray.Length

        Dim dataStream As Stream = w.GetRequestStream()
        dataStream.Write(byteArray, 0, byteArray.Length)

        Dim response As WebResponse = w.GetResponse()
        dataStream = response.GetResponseStream()

        Dim reader As New StreamReader(dataStream)
        Dim responseFromServer As String = reader.ReadToEnd()

        reader.Close()
        dataStream.Close()
        response.Close()

        Return responseFromServer
    End Function

    Private Sub Resend()
        For i As Integer = 0 To Backup.Count - 1
            Dim item As BackupItem = Backup(i)
            If item.Message.ToLower.Contains("remote name could not be resolved") Then
                workers.AddtoQueue({item.Data, i})
                'Else
                '    With item.Data
                '        Dim lstItem As New ListViewItem({ .bio_id, .site, .added_ts, item.Message})
                '        lstLogs.Items.Insert(0, lstItem)
                '    End With
            End If
        Next
    End Sub

    Private Sub ChangeStatus(message As String)
        Me.Text = formName & " - " & message
    End Sub


    Private Sub SaveConf()
        If conf IsNot Nothing AndAlso Not conf.Path = "" Then
            XmlSerialization.WriteToFile(conf.Path, conf)
        End If
    End Sub


#Region "database"
    Private TimekeeperConnection As New OleDbConnection()
    Private MasterfileConnection As New OleDbConnection()

    Private Sub ReadData()
        '   Dim com As New OleDb.OleDbCommand("SELECT * FROM `timecard` ORDER BY `timecardid` DESC", TimekeeperConnection)
        Dim com As New OleDb.OleDbCommand(String.Format("SELECT * FROM `timecard` WHERE `trandate` BETWEEN '{0}' AND '{1}' ORDER BY `timecardid` DESC",
       Now.ToString("yyyyMMdd"),
                                                       Now.AddDays(-1).ToString("yyyyMMdd")),
                                                       TimekeeperConnection)

        ' Dim d As New List(Of String)

        Using rdr As OleDb.OleDbDataReader = com.ExecuteReader()

            Dim newestRecord As String = LastRecord
            Dim isFirst As Boolean = True
            While rdr.Read
                Dim _date As String = rdr.Item("trandate") 'yyyyMMdd
                Dim _time As String = rdr.Item("trantime") 'HHmmss
                Dim logDatetime As String = String.Format("{0}-{1}-{2} {3}:{4}:{5}", _date.Substring(0, 4),
                                                      _date.Substring(4, 2), _date.Substring(6, 2),
                                                      _time.Substring(0, 2), _time.Substring(2, 2), _time.Substring(4, 2))

                Dim id As String = rdr.Item("employeeid") 'used in getting info from masterfiles
                Dim record As String = String.Format("{0} {1}", id, logDatetime)

                If isFirst Then
                    newestRecord = record
                    isFirst = False
                End If

                '      d.Add(record)
                If LastRecord = record Then
                    Exit While
                Else
                    If conf.DepartmentIds.Count > 0 Then
                        Dim departmentID As String = Lookup(id, "deptid")
                        If departmentID IsNot Nothing AndAlso conf.DepartmentIds.Contains(departmentID) Then
                            Continue While
                        End If
                    End If
                    Dim dt As New TitaniumData With {.bio_id = Lookup(id, "idno"), .added_ts = logDatetime, .site = cbSite.Text}
                    workers.AddtoQueue({dt, -1})
                End If
            End While

            If Not LastRecord = newestRecord Then
                LastRecord = newestRecord
                conf.LastLog = LastRecord
                saveConf()
                Status.OnQueues = workers.QueueArgs.Count
            End If
        End Using
    End Sub

    Private Function Lookup(id As String, targetField As String) As String
        Dim com As New OleDb.OleDbCommand(String.Format("select top 1 `{0}` from `employee` where employeeid={1}", targetField, id), MasterfileConnection)
        Using rdr As OleDb.OleDbDataReader = com.ExecuteReader()
            While rdr.Read
                Return rdr.Item(targetField)
            End While
        End Using
        Return Nothing
    End Function

    Private Sub Form1_KeyDown(sender As Object, e As KeyEventArgs) Handles Me.KeyDown
        If e.KeyCode = Keys.T Then
            Test()
        End If
    End Sub


#End Region

End Class

