Imports System.Net
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Data.OleDb

Imports Newtonsoft.Json
Imports System.ComponentModel

Imports SEAN

Public Class Form1
    Private formName As String
    Private dots As String = ""
    Private WithEvents workers As Workers

    Private conf As LogInf
    Private Status As Statuses

    Private WithEvents _backup As BackupLogs
    Private Property Backup As BackupLogs
        Get
            If _backup.day <> Now.Day Then
                If Not BackupFileInfo.Exists Then
                    ConfigurationStoring.XmlSerialization.WriteToFile(BackupFileInfo.FullName, New BackupLogs)
                End If
                _backup = ConfigurationStoring.XmlSerialization.ReadFromFile(BackupFileInfo.FullName, New BackupLogs)
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

        workers = New Workers
        workers.SetWorker(1)

        _backup = New BackupLogs
        If Not BackupFileInfo.Directory.Exists Then
            BackupFileInfo.Directory.Create()
        End If

        For i As Integer = 0 To Backup.Count - 1
            Dim item As BackupItem = Backup(i)
            If Not item.Message.ToUpper.Contains("SUCCESS") Then
                workers.AddtoQueue({item.Data, i})
            Else
                With item.Data
                    Dim lstItem As New ListViewItem({ .bio_id, .site, .added_ts, item.Message})
                    lstLogs.Items.Insert(0, lstItem)
                End With
            End If
        Next

        conf = New LogInf
        Dim confPath As String = Path.Combine(Application.StartupPath, Application.ProductName & ".config.xml")
        If File.Exists(confPath) Then
            conf = ConfigurationStoring.XmlSerialization.ReadFromFile(confPath, New LogInf)
            conf.Validate()

            For i As Integer = 0 To conf.Queues.Count - 1
                workers.AddtoQueue({conf.Queues(i), -1})
            Next

            LastRecord = conf.LastLog
            tbPath.Text = conf.BioMDBPath
        End If

        conf.Path = confPath

        If File.Exists(conf.Path) Then
            OpenFile(True)
        End If

        start(conf.BioMDBPath)
        cbSite.SelectedIndex = conf.Site
        Status.OnQueues = workers.QueueArgs.Count
    End Sub

    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        Connection.Close()
        saveConf()
    End Sub

    Private Sub tbPath_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles tbPath.MouseDoubleClick
        workers.StopWorking()

        OpenFile()
    End Sub

    Private Sub tbPath_TextChanged(sender As Object, e As EventArgs) Handles tbPath.TextChanged
        If File.Exists(tbPath.Text) AndAlso Path.GetExtension(tbPath.Text).ToUpper = ".MDB" Then
            start(tbPath.Text)

            conf.BioMDBPath = tbPath.Text
            saveConf()
        Else : MsgBox("Invalid BIO MDB Path.")
        End If
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

    Private Sub tmTracker_Tick(sender As Object, e As EventArgs) Handles tmTracker.Tick
        tmTracker.Enabled = False
        ReadData()
        If workers.QueueArgs.Count > 0 Then
            workers.StartWorking()
        End If
        tmTracker.Enabled = True
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
                               lstLogs.Items.Insert(0, item)
                               If isNew Then
                                   Backup.Add(New BackupItem With {.Data = tiData.Clone, .Message = statusSub.Text, .TimeSent = Now})
                               Else
                                   Backup.Item(e.Argument(1)) = (New BackupItem With {.Data = tiData.Clone, .Message = statusSub.Text, .TimeSent = Now})
                               End If
                           End Sub)
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Sub workers_Worker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles workers.Worker_RunWorkerCompleted
        conf.AddQueues(workers.QueueArgs)
        Status.OnQueues = workers.QueueArgs.Count
    End Sub

    Private Sub _backup_ItemChanged() Handles _backup.ItemChanged
        ConfigurationStoring.XmlSerialization.WriteToFile(BackupFileInfo.FullName, Backup)
    End Sub









    Private Sub start(path)
        If File.Exists(path) Then
            OpenMDBConnection(path)
            tmBuffer.Enabled = True
            tmTracker.Enabled = True
            Status.Running = True
        End If
    End Sub

    Private LastRecord As String
    Private Connection As New OleDbConnection()
    Private Sub ReadData()
        Dim com As New OleDb.OleDbCommand("select * from Emp_InOut order by `Date`, `Time_In` desc", Connection)
        Using rdr As OleDb.OleDbDataReader = com.ExecuteReader()
            Dim newestRecord As String = LastRecord
            Dim isFirst As Boolean = True
            While rdr.Read
                Dim _date As String = rdr.Item("Date")
                Dim _time As String = rdr.Item("Time_In")
                Dim logDatetime As String = String.Format("20{0}-{1}-{2} {3}:{4}:00", _date.Substring(0, 2),
                                                      _date.Substring(2, 2), _date.Substring(4, 2),
                                                      _time.Substring(0, 2), _time.Substring(2, 2))

                Dim id As String = rdr.Item("id")
                Dim record As String = String.Format("{0} {1}", id, logDatetime)

                If isFirst Then
                    newestRecord = record
                    isFirst = False
                End If

                If LastRecord = record Then
                    Exit While
                Else
                    workers.AddtoQueue({New TitaniumData With {.bio_id = id, .added_ts = logDatetime, .site = cbSite.Text}, -1})
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


    Private Function SendDataLog(tiData As TitaniumData) As String
        Dim responseFromServer As String = ""
        Dim failcnt As Short = 0

        With tiData
            .token = "bf3d9e0a7bfc4ffa9765641530c49242"
            .action = "insert"
        End With

        Dim postData As String = "postData= " & JsonConvert.SerializeObject(tiData, Formatting.Indented)

        While True
            Try
                responseFromServer = SendAPIMessage(postData, "http://idcsi-officesuites.com:8082/upsg/bio_api/API_Receiver")
                If responseFromServer.ToUpper.Contains("!DOCTYPE") Then
                    Return Regex.Match(responseFromServer, "(\<body\>)[\w\d\s \<\>\`\=\""\'\/\:\-\(\)\`\,\.\?\;\[\]\!\~\@\#\$\%\^\&\*]+(\<\/body\>)").Value
                ElseIf responseFromServer.ToUpper.Contains("SUCCESS") _
                    Or responseFromServer.ToUpper.Contains("FAILED") Then

                    Return responseFromServer
                Else
                    If failcnt >= 10 Then
                        Return responseFromServer
                    End If
                    failcnt += 1
                End If
                Return True
            Catch ex As Exception
                Exit While
            End Try
        End While
        Return False
    End Function

    Public Function SendAPIMessage(PostData As String, Address As String) As String
        Dim w As WebRequest = WebRequest.Create(Address)
        w.Method = "POST"
        Dim byteArray As Byte() = Encoding.UTF8.GetBytes(PostData)
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

    Private Sub OpenMDBConnection(path As String)
        Try
            Dim providerFormat As String = "Provider=Microsoft.JET.OLEDB.4.0;Data Source={0};User Id={1};Password={2};"
            Dim connectionString As String = String.Format(providerFormat, path, "", "")

            Connection = New OleDbConnection(connectionString)
            Connection.Open()
            Status.Connected = True
        Catch ex As Exception
            Status.Connected = False
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Sub ChangeStatus(message As String)
        Me.Text = formName & " - " & message
    End Sub

    Private Sub OpenFile(Optional persist = False)
        While Not Status.Connected
            If OPFD.ShowDialog = DialogResult.OK Then
                tbPath.Text = OPFD.FileName
            End If
            If Not persist Then Exit While
        End While
    End Sub

    Private Sub saveConf()
        If Not conf.Path = "" Then
            ConfigurationStoring.XmlSerialization.WriteToFile(conf.Path, conf)
        End If
    End Sub


End Class

Public Class Statuses
    Public Running As Boolean
    Public Connected As Boolean
    Public OnQueues As Integer

    Public Overrides Function ToString() As String
        Return String.Format("{0} On Queue[{1}] {2}",
                             IIf(Connected, "Connected", "Not Connected"),
                             OnQueues,
                              IIf(Running, "Running", "Stand By"))
    End Function
End Class

Public Class TitaniumData
    Implements ICloneable

    Public token As String
    Public action As String
    Public site As String
    Public bio_id As String
    Public added_ts As String

    Public Function Clone() As Object Implements ICloneable.Clone
        Return Me.MemberwiseClone
    End Function
End Class

Public Class LogInf
    <System.Xml.Serialization.XmlIgnore>
    Public Path As String = ""
    Public BioMDBPath As String = ""
    Public LastLog As String = ""
    Public Site As Integer = 0
    Public Queues As New List(Of TitaniumData)


    Public Sub AddQueues(queueArgs As List(Of Object))
        Queues = New List(Of TitaniumData)
        For Each args In queueArgs
            If args(1) = -1 Then
                Queues.Add(DirectCast(args(0), TitaniumData))
            End If
        Next
    End Sub

    Public Sub Validate()
        If Not File.Exists(BioMDBPath) Then
            BioMDBPath = ""
        End If
    End Sub
End Class

Public Class BackupItem
    Public Data As TitaniumData
    Public Message As String
    Public TimeSent As Date
End Class

Public Class BackupLogs
    Inherits List(Of BackupItem)
    Public Event ItemChanged()

    <System.Xml.Serialization.XmlIgnore> Public day As Integer

    Public Shadows Sub Add(Item As BackupItem)
        MyBase.Add(Item)
        RaiseEvent ItemChanged()
    End Sub

End Class
