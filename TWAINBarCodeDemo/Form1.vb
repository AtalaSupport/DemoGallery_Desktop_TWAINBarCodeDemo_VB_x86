Imports Microsoft.VisualBasic
Imports System
Imports System.Drawing
Imports System.Collections
Imports System.ComponentModel
Imports System.Windows.Forms
Imports System.Data

Namespace TWAINBarCodeDemo
    ''' <summary>
    ''' This simple Winform will use the BatchScan class to demonstrate scanning and saving 
    ''' muliple documents, while reading barcode encoded instructions.  By default, all the 
    ''' directories have the base path of "C:\TWAINoutput", and the file names have the format
    ''' "untitled_x.tif".  This is only changed when a correct barcode is recognized.  This
    ''' demo program currently does not account for barcodes that are incorrectly formated.
    ''' 
    ''' Requirements:
    ''' -TWAIN supported scanner with an Automatic Document Feeder (ADF).
    ''' -at least one page with barcodes encoded with Symbology Code39 barcode font.
    ''' -at least one page to be scanned in after the barcodded page (cover sheet).
    ''' 
    ''' Note:  This class does not use any dotTWAIN or DotImage classes directly.  Thoes are all
    ''' used in the BatchScan Class.  This class is simply to show the class running.
    ''' 
    ''' </summary>
    Public Class Form1
        Inherits System.Windows.Forms.Form
        Private saveFileDialog1 As System.Windows.Forms.SaveFileDialog
        Private WithEvents button_startscan As System.Windows.Forms.Button
        Private autodeskew As System.Windows.Forms.CheckBox
        Private autodespeckle As System.Windows.Forms.CheckBox
        Private statusBox As System.Windows.Forms.TextBox
        Private mainMenu1 As System.Windows.Forms.MainMenu
        Private menuItem1 As System.Windows.Forms.MenuItem
        Private WithEvents menuItem2 As System.Windows.Forms.MenuItem

        Private bs As BatchScan = New BatchScan()
        ''' <summary>
        ''' Required designer variable.
        ''' </summary>
        Private components As System.ComponentModel.Container = Nothing

        Public Sub New()
            '
            ' Required for Windows Form Designer support
            '
            InitializeComponent()

            ' register an Event for message printing
            AddHandler bs.ScanMsg, AddressOf OnScanMsg
        End Sub

        ''' <summary>
        ''' Clean up any resources being used.
        ''' </summary>
        Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
            If disposing Then
                If Not components Is Nothing Then
                    components.Dispose()
                End If
            End If
            MyBase.Dispose(disposing)
        End Sub

#Region "Windows Form Designer generated code"
        ''' <summary>
        ''' Required method for Designer support - do not modify
        ''' the contents of this method with the code editor.
        ''' </summary>
        Private Sub InitializeComponent()
            Me.saveFileDialog1 = New System.Windows.Forms.SaveFileDialog()
            Me.button_startscan = New System.Windows.Forms.Button()
            Me.autodeskew = New System.Windows.Forms.CheckBox()
            Me.autodespeckle = New System.Windows.Forms.CheckBox()
            Me.statusBox = New System.Windows.Forms.TextBox()
            Me.mainMenu1 = New System.Windows.Forms.MainMenu()
            Me.menuItem1 = New System.Windows.Forms.MenuItem()
            Me.menuItem2 = New System.Windows.Forms.MenuItem()
            Me.SuspendLayout()
            ' 
            ' button_startscan
            ' 
            Me.button_startscan.Location = New System.Drawing.Point(104, 80)
            Me.button_startscan.Name = "button_startscan"
            Me.button_startscan.Size = New System.Drawing.Size(104, 23)
            Me.button_startscan.TabIndex = 0
            Me.button_startscan.Text = "Start Batch Scan"
            '			Me.button_startscan.Click += New System.EventHandler(Me.OnStartScan);
            ' 
            ' autodeskew
            ' 
            Me.autodeskew.Location = New System.Drawing.Point(48, 32)
            Me.autodeskew.Name = "autodeskew"
            Me.autodeskew.TabIndex = 1
            Me.autodeskew.Text = "Auto Deskew"
            ' 
            ' autodespeckle
            ' 
            Me.autodespeckle.Location = New System.Drawing.Point(184, 32)
            Me.autodespeckle.Name = "autodespeckle"
            Me.autodespeckle.TabIndex = 2
            Me.autodespeckle.Text = "Auto Despeckle"
            ' 
            ' statusBox
            ' 
            Me.statusBox.Location = New System.Drawing.Point(8, 120)
            Me.statusBox.Multiline = True
            Me.statusBox.Name = "statusBox"
            Me.statusBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
            Me.statusBox.Size = New System.Drawing.Size(328, 136)
            Me.statusBox.TabIndex = 3
            Me.statusBox.Text = ""
            ' 
            ' mainMenu1
            ' 
            Me.mainMenu1.MenuItems.AddRange(New System.Windows.Forms.MenuItem() {Me.menuItem1})
            ' 
            ' menuItem1
            ' 
            Me.menuItem1.Index = 0
            Me.menuItem1.MenuItems.AddRange(New System.Windows.Forms.MenuItem() {Me.menuItem2})
            Me.menuItem1.Text = "Help"
            ' 
            ' menuItem2
            ' 
            Me.menuItem2.Index = 0
            Me.menuItem2.Text = "About"
            '			Me.menuItem2.Click += New System.EventHandler(Me.OnHelpSelect);
            ' 
            ' Form1
            ' 
            Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
            Me.ClientSize = New System.Drawing.Size(344, 266)
            Me.Controls.Add(Me.statusBox)
            Me.Controls.Add(Me.autodespeckle)
            Me.Controls.Add(Me.autodeskew)
            Me.Controls.Add(Me.button_startscan)
            Me.Menu = Me.mainMenu1
            Me.Name = "Form1"
            Me.Text = "My TWAIN Demo"
            Me.ResumeLayout(False)

        End Sub
#End Region

        ''' <summary>
        ''' The main entry point for the application.
        ''' </summary>
        <STAThread()> _
        Shared Sub Main()
            Application.Run(New Form1())
        End Sub

        ' EVENT - Start Scan Button Click
        Private Sub OnStartScan(ByVal sender As Object, ByVal e As System.EventArgs) Handles button_startscan.Click
            ' assign comands to preform to images
            bs.Deskew = autodeskew.Checked
            bs.Despeckle = autodespeckle.Checked

            Try
                bs.Process()
            Catch ex As System.Exception
                MessageBox.Show(ex.ToString())
            End Try
        End Sub

        ' EVENT - message comming from BatchScan object, display in the textbox.
        Private Sub OnScanMsg(ByVal sender As Object, ByVal e As BatchScan.BatchScanEventArgs)
            statusBox.AppendText(e.msg & Constants.vbCrLf)
        End Sub

        ' A little bit of info
        Private Sub OnHelpSelect(ByVal sender As Object, ByVal e As System.EventArgs) Handles menuItem2.Click
            Dim aboutBox As AtalaDemos.AboutBox.About = New AtalaDemos.AboutBox.About("About Atalasoft Barcode Sorter Demo", "DotImage and DotTWAIN Barcode Demo")
            aboutBox.Description = "This demo shows how to scan multiple pages (using DotTwain) and file them based on a barcode.  Each group of pages have their own cover sheet with a barcode that holds information about where to save the following scanned images.  Please see the Readme file included in the project for more information on this demo.  Requires DotImage, DotImage BarcodeReader Code39, and DotTwain."
            aboutBox.ShowDialog()

        End Sub
    End Class
End Namespace
