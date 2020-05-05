Imports System.Net
Imports System.IO
Imports System.Text
Imports System.Data.OleDb

Imports Newtonsoft.Json
Imports System.ComponentModel

Public Class Form1
    Private formName As String
    Private dots As String = ""
    Private WithEvents workers As SEAN.Workers

    Private Sub Form_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        formName = Application.ProductName & " v" & Application.ProductVersion
        Me.Text = formName

        workers.SetWorker(3)

        ChangeStatus("Stand By")
    End Sub

    Private Sub tbPath_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles tbPath.MouseDoubleClick
        workers.StopWorking()
        If OPFD.ShowDialog = DialogResult.OK Then
            OpenMDBConnection(OPFD.FileName)
            tmBuffer.Enabled = True
            tmTracker.Enabled = True
        End If
    End Sub

    Private Sub tm_Tick(sender As Object, e As EventArgs) Handles tmBuffer.Tick
        If dots.Length = 3 Then
            dots = ""
        Else : dots &= "."
        End If
        ChangeStatus("Running" & dots)
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
        Dim site As String = "LEYTE" ' e.Argument(2)
        Dim logDatetime As String = e.Argument(1)

        sendDataLog(id, site, Date.Parse(logDatetime))
    End Sub


    Private LastRecord As String
    Private Connection As New OleDbConnection(tbPath.Text)
    Private Sub ReadData()
        Dim com As New OleDb.OleDbCommand("select * from Emp_InOut order by `date`, `time` desc", Connection)
        Dim rdr As OleDb.OleDbDataReader = com.ExecuteReader()

        Dim previousRecord As String = LastRecord
        While rdr.Read
            Dim _date As String = rdr.Item("date")
            Dim _time As String = rdr.Item("time")
            Dim logDatetime As String = String.Format("20{0}-{1}-{2} {3}:{4}", _date.Substring(0, 2),
                                                      _date.Substring(2, 2), _date.Substring(4, 2),
                                                       _time.Substring(0, 2), _time.Substring(2, 2))

            Dim id As String = rdr.Item("id")
            Dim record As String = String.Format("{0} {1}", id, logDatetime)

            If Not LastRecord = record Then
                'send
                workers.AddtoQueue({id, logDatetime})
                previousRecord = record
            Else   'end
            End If
        End While

        LastRecord = previousRecord
    End Sub


    Private Function SendDataLog(id As String, site As String, date_added As Date) As Boolean
        Dim responseFromServer As String = ""
        Dim failcnt As Short = 0

        Dim l As New tiData With {.id = id, .site = site, .date_added = date_added}
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
        Dim providerFormat As String = "Provider=Microsoft.JET.OLEDB.4.0;Data Source={0};User Id={1};Password={2};"
        Dim connectionString As String = String.Format(providerFormat, path, "", "")

        Connection = New OleDbConnection(connectionString)
    End Sub

    Private Sub ChangeStatus(message As String)
        Me.Text = formName & " - " & message
    End Sub

End Class


Public Class tiData
    Public id As String
    Public site As String
    Public date_added As Date
End Class