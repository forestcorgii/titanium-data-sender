<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
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
        Me.components = New System.ComponentModel.Container()
        Me.lstLogs = New System.Windows.Forms.ListView()
        Me.ColumnHeader1 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader2 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader3 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader4 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.tmBuffer = New System.Windows.Forms.Timer(Me.components)
        Me.OPFD = New System.Windows.Forms.OpenFileDialog()
        Me.tmTracker = New System.Windows.Forms.Timer(Me.components)
        Me.btnFilter = New System.Windows.Forms.Button()
        Me.cbSite = New System.Windows.Forms.ComboBox()
        Me.FBD = New System.Windows.Forms.FolderBrowserDialog()
        Me.tmResender = New System.Windows.Forms.Timer(Me.components)
        Me.SuspendLayout()
        '
        'lstLogs
        '
        Me.lstLogs.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lstLogs.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader1, Me.ColumnHeader2, Me.ColumnHeader3, Me.ColumnHeader4})
        Me.lstLogs.HideSelection = False
        Me.lstLogs.Location = New System.Drawing.Point(12, 39)
        Me.lstLogs.Name = "lstLogs"
        Me.lstLogs.Size = New System.Drawing.Size(528, 251)
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
        'ColumnHeader4
        '
        Me.ColumnHeader4.Text = "Message"
        Me.ColumnHeader4.Width = 266
        '
        'tmBuffer
        '
        Me.tmBuffer.Interval = 10000
        '
        'OPFD
        '
        Me.OPFD.Filter = "MDB Files| *.mdb"
        '
        'tmTracker
        '
        Me.tmTracker.Interval = 5000
        '
        'btnFilter
        '
        Me.btnFilter.Location = New System.Drawing.Point(99, 10)
        Me.btnFilter.Name = "btnFilter"
        Me.btnFilter.Size = New System.Drawing.Size(75, 23)
        Me.btnFilter.TabIndex = 4
        Me.btnFilter.Text = "Filter"
        Me.btnFilter.UseVisualStyleBackColor = True
        '
        'cbSite
        '
        Me.cbSite.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbSite.FormattingEnabled = True
        Me.cbSite.Items.AddRange(New Object() {"MANILA", "LEYTE"})
        Me.cbSite.Location = New System.Drawing.Point(12, 12)
        Me.cbSite.Name = "cbSite"
        Me.cbSite.Size = New System.Drawing.Size(81, 21)
        Me.cbSite.TabIndex = 2
        '
        'tmResender
        '
        Me.tmResender.Interval = 10000
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(552, 302)
        Me.Controls.Add(Me.btnFilter)
        Me.Controls.Add(Me.cbSite)
        Me.Controls.Add(Me.lstLogs)
        Me.KeyPreview = True
        Me.Name = "Form1"
        Me.Text = "Titanium Data Sender"
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents lstLogs As ListView
    Friend WithEvents ColumnHeader1 As ColumnHeader
    Friend WithEvents ColumnHeader2 As ColumnHeader
    Friend WithEvents ColumnHeader3 As ColumnHeader
    Friend WithEvents tmBuffer As Timer
    Friend WithEvents OPFD As OpenFileDialog
    Friend WithEvents tmTracker As Timer
    Friend WithEvents ColumnHeader4 As ColumnHeader
    Friend WithEvents btnFilter As Button
    Friend WithEvents cbSite As ComboBox
    Friend WithEvents FBD As FolderBrowserDialog
    Friend WithEvents tmResender As Timer
End Class
