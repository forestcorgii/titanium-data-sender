Public Class Form1
    Private formName As String

    Private Sub Form_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        formName = Application.ProductName & " v" & Application.ProductVersion
        Me.Text = formName



    End Sub



    Private Sub tbPath_TextChanged(sender As Object, e As EventArgs) Handles tbPath.TextChanged

    End Sub

    Private Sub tbPath_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles tbPath.MouseDoubleClick

    End Sub




    Private Sub changeStatus(message As String)
        Me.Text = formName & " " & message
    End Sub


End Class
