Imports System.Net
Imports System.IO
Imports System.Text
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


    Private Sub Form_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        formName = Application.ProductName & " v" & Application.ProductVersion
        Me.Text = formName

        Status = New Statuses
        ChangeStatus(Status.ToString)

        workers = New Workers
        workers.SetWorker(1)

        conf = New LogInf
        Dim confPath As String = Path.Combine(Application.StartupPath, Application.ProductName & ".config.xml")
        If File.Exists(confPath) Then
            conf = ConfigurationStoring.XmlSerialization.ReadFromFile(confPath, New LogInf)
            conf.Validate()

            workers.AddRangetoQueue(conf.Queues)

            LastRecord = conf.LastLog
            tbPath.Text = conf.BioMDBPath
        End If

        conf.Path = confPath

        If File.Exists(conf.Path) Then
            OpenFile(True)
        End If

        start(conf.BioMDBPath)
        cbSite.SelectedIndex = conf.Site
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
            Threading.Thread.Sleep(3000)
            Dim tiData As TitaniumData = DirectCast(e.Argument, TitaniumData)
            SendDataLog(tiData)

            lstLogs.Invoke(Sub()
                               Dim item As New ListViewItem({tiData.bio_id, tiData.site, tiData.added_ts})
                               lstLogs.Items.Add(item)
                           End Sub)
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Sub workers_Worker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles workers.Worker_RunWorkerCompleted
        conf.AddQueues(workers.QueueArgs)
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
                    workers.AddtoQueue(New TitaniumData With {.bio_id = id, .added_ts = logDatetime, .site = cbSite.Text})
                End If
            End While

            If Not LastRecord = newestRecord Then
                LastRecord = newestRecord
                conf.LastLog = LastRecord
                saveConf()
            End If
        End Using
    End Sub


    Private Function SendDataLog(tiData As TitaniumData) As Boolean
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
                If responseFromServer.Contains("Message sent") = False Then
                    If failcnt >= 10 Then
                        Return False
                    End If
                    failcnt += 1
                Else
                    Return True
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
    Public token As String
    Public action As String
    Public site As String
    Public bio_id As String
    Public added_ts As String
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
            Queues.Add(DirectCast(args, TitaniumData))
        Next
    End Sub

    Public Sub Validate()
        If Not File.Exists(BioMDBPath) Then
            BioMDBPath = ""
        End If
    End Sub
End Class

