﻿<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.lstLogs = New System.Windows.Forms.ListView()
        Me.ColumnHeader1 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader2 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader3 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.tbPath = New System.Windows.Forms.TextBox()
        Me.SuspendLayout()
        '
        'lstLogs
        '
        Me.lstLogs.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lstLogs.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader1, Me.ColumnHeader2, Me.ColumnHeader3})
        Me.lstLogs.Location = New System.Drawing.Point(12, 38)
        Me.lstLogs.Name = "lstLogs"
        Me.lstLogs.Size = New System.Drawing.Size(528, 252)
        Me.lstLogs.TabIndex = 0
        Me.lstLogs.UseCompatibleStateImageBehavior = False
        Me.lstLogs.View = System.Windows.Forms.View.Details
        '
        'ColumnHeader1
        '
        Me.ColumnHeader1.Text = "ID"
        Me.ColumnHeader1.Width = 59
        '
        'ColumnHeader2
        '
        Me.ColumnHeader2.Text = "Site"
        Me.ColumnHeader2.Width = 81
        '
        'ColumnHeader3
        '
        Me.ColumnHeader3.Text = "Date Added"
        Me.ColumnHeader3.Width = 103
        '
        'tbPath
        '
        Me.tbPath.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.tbPath.Location = New System.Drawing.Point(12, 12)
        Me.tbPath.Name = "tbPath"
        Me.tbPath.Size = New System.Drawing.Size(528, 20)
        Me.tbPath.TabIndex = 1
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(552, 302)
        Me.Controls.Add(Me.tbPath)
        Me.Controls.Add(Me.lstLogs)
        Me.Name = "Form1"
        Me.Text = "Titanium Data Sender"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents lstLogs As ListView
    Friend WithEvents tbPath As TextBox
    Friend WithEvents ColumnHeader1 As ColumnHeader
    Friend WithEvents ColumnHeader2 As ColumnHeader
    Friend WithEvents ColumnHeader3 As ColumnHeader
End Class
