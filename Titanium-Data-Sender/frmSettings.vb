Imports Titanium_Data_Sender.Classes
Imports SEAN

Public Class frmSettings
    Public Conf As LogInf
    Sub New(_conf As LogInf)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Conf = _conf
        cbSite.SelectedIndex = _conf.Site
        For Each department As String In _conf.Departments
            tbDepartments.Text += department & vbNewLine
        Next
        tbDepartments.Text = tbDepartments.Text.TrimEnd
        Me.DialogResult = DialogResult.None
    End Sub

    Private Sub frmSettings_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        Conf.Departments.AddRange(tbDepartments.Lines)

        Dim ask As DialogResult = MsgBox("Save?", MsgBoxStyle.YesNoCancel)
        If ask = DialogResult.Yes Then
            Me.DialogResult = DialogResult.Yes
        ElseIf ask = DialogResult.Cancel Then
            e.Cancel = True
        End If
    End Sub
End Class