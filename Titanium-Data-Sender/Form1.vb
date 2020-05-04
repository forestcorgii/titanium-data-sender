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




    Private Sub changeStatus(message As String)
        Me.Text = formName & " " & message
    End Sub

End Class
