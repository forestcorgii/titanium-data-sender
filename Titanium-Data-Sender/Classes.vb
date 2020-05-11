Imports System.IO

Public Class Classes
#Region "classes"

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

        Public Overrides Function ToString() As String
            Dim postData As String = "token=" & Uri.EscapeDataString(token) &
            "&action=" & Uri.EscapeDataString(action) &
            "&site=" & Uri.EscapeDataString(site) &
            "&bio_id=" & Uri.EscapeDataString(bio_id) &
            "&added_ts=" & Uri.EscapeDataString(added_ts)
            Return postData
        End Function
    End Class

    Public Class LogInf
        <System.Xml.Serialization.XmlIgnore>
        Public Path As String = ""
        Public BioMDBPath As String = ""
        Public LastLog As String = ""
        Public Site As Integer = 0
        Public Queues As New List(Of TitaniumData)
        Public Departments As List(Of String)

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
#End Region

End Class
