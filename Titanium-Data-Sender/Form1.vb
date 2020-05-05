﻿Imports System.Net
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
        workers.SetWorker(3)

        conf = New LogInf
        Dim confPath As String = Path.Combine(Application.StartupPath, Application.ProductName & ".config.xml")
        If File.Exists(confPath) Then
            conf = ConfigurationStoring.XmlSerialization.ReadFromFile(confPath, New LogInf)
            conf.Validate()

            conf.Path = confPath
            workers.AddRangetoQueue(conf.Queues)

            LastRecord = conf.LastLog
            tbPath.Text = conf.BioMDBPath
            start(conf.BioMDBPath)
        End If

        If File.Exists(conf.Path) Then
            OpenFile(True)
        End If

        cbSite.SelectedIndex = conf.Site
    End Sub

    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        Connection.Close()
        ConfigurationStoring.XmlSerialization.WriteToFile(conf.Path, conf)
    End Sub

    Private Sub tbPath_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles tbPath.MouseDoubleClick
        workers.StopWorking()

        OpenFile()
    End Sub

    Private Sub tbPath_TextChanged(sender As Object, e As EventArgs) Handles tbPath.TextChanged
        If File.Exists(tbPath.Text) AndAlso Path.GetExtension(tbPath.Text).ToUpper = ".MDB" Then
            start(tbPath.Text)

            conf.BioMDBPath = tbPath.Text
            ConfigurationStoring.XmlSerialization.WriteToFile(conf.Path, conf)
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
        Dim id As String = e.Argument(0)
        Dim site As String = e.Argument(2)
        Dim logDatetime As String = e.Argument(1)

        SendDataLog(id, site, Date.Parse(logDatetime))

        lstLogs.Invoke(Sub()
                           Dim item As New ListViewItem({id, site, logDatetime})
                           lstLogs.Items.Add(item)
                       End Sub)
    End Sub

    Private Sub workers_Worker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles workers.Worker_RunWorkerCompleted
        conf.Queues = workers.QueueArgs
        ConfigurationStoring.XmlSerialization.WriteToFile(conf.Path, conf)
    End Sub



    Private Sub start(path)
        OpenMDBConnection(path)
        tmBuffer.Enabled = True
        tmTracker.Enabled = True
        Status.Running = True
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
                Dim logDatetime As String = String.Format("20{0}-{1}-{2} {3}:{4}", _date.Substring(0, 2),
                                                      _date.Substring(2, 2), _date.Substring(4, 2),
                                                      _time.Substring(0, 2), _time.Substring(2, 2))

                Dim id As String = rdr.Item("id")
                Dim record As String = String.Format("{0} {1}", id, logDatetime)

                If isFirst Then newestRecord = record

                If LastRecord = record Then
                    Exit While
                Else
                    workers.AddtoQueue({id, logDatetime, cbSite.Text})
                    newestRecord = record
                End If
            End While

            If Not LastRecord = newestRecord Then
                LastRecord = newestRecord
                conf.LastLog = LastRecord
                ConfigurationStoring.XmlSerialization.WriteToFile(conf.Path, conf)
            End If
        End Using
    End Sub


    Private Function SendDataLog(id As String, site As String, date_added As Date) As Boolean
        Dim responseFromServer As String = ""
        Dim failcnt As Short = 0

        Dim l As New TitaniumData With {.id = id, .site = site, .date_added = date_added}
        Dim postData As String = "postData= " & JsonConvert.SerializeObject(l, Formatting.Indented)

        While True
            Try
                responseFromServer = SendAPIMessage(postData, "")
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
    Public id As String
    Public site As String
    Public date_added As Date
End Class

Public Class LogInf
    <System.Xml.Serialization.XmlIgnore>
    Public Path As String = ""
    Public BioMDBPath As String = ""
    Public LastLog As String = ""
    Public Site As Integer = 0
    Public Queues As New List(Of Object)

    Public Sub Validate()
        If Not File.Exists(BioMDBPath) Then
            BioMDBPath = ""
        End If
    End Sub
End Class

