Imports System.Net
Imports System.IO
Imports System.Text
Imports System.Data.OleDb

Imports Newtonsoft.Json

Public Class Form1
    Private formName As String
    Private dots As String = ""

    Private Sub Form_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        formName = Application.ProductName & " v" & Application.ProductVersion
        Me.Text = formName



    End Sub




    Private Sub tbPath_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles tbPath.MouseDoubleClick

    End Sub

    Private Sub tm_Tick(sender As Object, e As EventArgs) Handles tm.Tick
        If dots.Length = 3 Then
            dots = ""
        Else : dots &= "."
        End If
        changeStatus("Running" & dots)
    End Sub




    Private lastRecord As String
    Private Sub readData()
        Dim con As New OleDbConnection(tbPath.Text)
        Dim com As New OleDb.OleDbCommand("select * from Emp_InOut order by `date`, `time` desc", con)
        Dim rdr As OleDb.OleDbDataReader = com.ExecuteReader()

        Dim previousRecord As String = lastRecord
        While rdr.Read
            Dim _date As String = rdr.Item("date")
            Dim _time As String = rdr.Item("time")
            Dim logDatetime As String = String.Format("20{0}-{1}-{2} {3}:{4}", _date.Substring(0, 2),
                                                      _date.Substring(2, 2), _date.Substring(4, 2),
                                                       _time.Substring(0, 2), _time.Substring(2, 2))

            Dim id As String = rdr.Item("id")
            Dim record As String = String.Format("{0} {1}", id, logDatetime)

            If Not lastRecord = record Then
                'send
                previousRecord = record
            Else   'end
            End If
        End While

        lastRecord = previousRecord
    End Sub


    Private Function sendTimeLog(id As String, site As String, date_added As Date) As Boolean
        Dim responseFromServer As String = ""
        Dim failcnt As Short = 0
        While True
            Try
                Dim l As New tiData With {.id = id, .site = site, .date_added = date_added}
                Dim postData As String = "postData= " & JsonConvert.SerializeObject(l, Formatting.Indented)
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


    Private Sub changeStatus(message As String)
        Me.Text = formName & " " & message
    End Sub

End Class


Public Class tiData
    Public id As String
    Public site As String
    Public date_added As Date
End Class