Imports System.Windows.Forms

Public Class Abort
    Public choice As Integer '1 - discard, 2 - keep, 3 - zip and hash
    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btn_ok.Click
        If choice1_discard.Checked Then choice = 1
        If choice2_keep.Checked Then choice = 2
        If choice3_finish.Checked Then choice = 3

        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btn_cancel.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

End Class
