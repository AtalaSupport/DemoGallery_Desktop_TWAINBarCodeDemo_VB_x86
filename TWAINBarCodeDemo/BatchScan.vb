Imports Microsoft.VisualBasic
Imports System
Imports System.IO
Imports System.Windows.Forms
Imports Atalasoft.Twain
Imports Atalasoft.Imaging
Imports Atalasoft.Imaging.Codec
Imports Atalasoft.Imaging.ImageProcessing.Document
Imports Atalasoft.Barcoding.Reading

Namespace TWAINBarCodeDemo
    ''' <summary>
    ''' This class preforms the function of scanning in multipule documents using an automated document
    ''' feeder (ADF).  Currently, 2 commands are able to be prefomed, AutoDeskew and AutoDespeckle.
    ''' This class also used the built in Barcode Reader functionality of the dotImage 2.1 toolkit.
    ''' A cover page with 2 barcodes on it will tell where the following documents should be saved
    ''' (in which directory) and what the file format should be.  A ScanMsg Event is raised to signal
    ''' the parent class of a message.  This message should then be displayed as text for the GUI.
    ''' </summary>
    Public Class BatchScan
        ' Params
        Private despeckle_Renamed As Boolean ' turn on/off commands
        Private deskew_Renamed As Boolean
        Private outputpath_Renamed As String ' base filepath to save documents to
        Private filename_format As String
        Private acquisition As Acquisition = New Acquisition() ' Acquisition to handle all scanning
        Private device As Device = Nothing ' object representing the scanner
        Private options As ReadOpts ' Barcoding options
        Private readEngin As BarCodeReader 'barcode reading object

#Region "EventHandler Code"
        Public Class BatchScanEventArgs
            Inherits System.EventArgs
            Public msg As String
            Public Sub New(ByVal m As String)
                msg = m
            End Sub
        End Class

        Public Delegate Sub BatchScanEventHandler(ByVal sender As Object, ByVal e As BatchScanEventArgs)
        Public Event ScanMsg As BatchScanEventHandler

        Protected Overridable Sub OnScanMsg(ByVal e As BatchScanEventArgs)
            If Not ScanMsgEvent Is Nothing Then
                RaiseEvent ScanMsg(Me, e)
            End If
        End Sub

#End Region

        ' Constructor
        Public Sub New()
            init()
            LoadDevice()
        End Sub

        'destructor
        Protected Overrides Sub Finalize()
            ' dispose of the device here
        End Sub
        'Sets values when a new group of documents are to be scanned in the same session.

        Private Sub setDefaults()
            'set paramater defaults
            outputpath_Renamed = "..\\..\\TWAINoutput\"
            filename_format = "untitled"
        End Sub

        ' initialize params
        Private Sub init()
            despeckle_Renamed = False
            deskew_Renamed = False

            setDefaults()

            'Setup AcquisitionEvents
            Me.acquisition.ApplicationIdentity.Info = "My Acquisition"
            Me.acquisition.ApplicationIdentity.Manufacturer = "Atalasoft"
            'this.acquisition.Parent = this;
            Me.acquisition.ApplicationIdentity.ProductFamily = "Imaging"
            Me.acquisition.ApplicationIdentity.ProductName = "Twain"
            Me.acquisition.ApplicationIdentity.VersionMajor = 1
            Me.acquisition.ApplicationIdentity.VersionMinor = 0

            ' Register the events to do with Acquireing images from device
            ' Only use these when needed
            AddHandler acquisition.AcquireFinished, AddressOf acquisition_AcquireFinished
            AddHandler acquisition.DeviceEvent, AddressOf acquisition_DeviceEvent
            AddHandler acquisition.ImageAcquired, AddressOf acquisition_ImageAcquired
            AddHandler acquisition.AcquireCanceled, AddressOf acquisition_AcquireCanceled
            'this.acquisition.FileTransfer += new Atalasoft.Twain.FileTransferEventHandler(this.acquisition_FileTransfer);
        End Sub


        ' Checks to make sure a scanner is recognized by dotTWAIN, and loads the default device,
        ' if there are more than one found.
        Private Sub LoadDevice()
            ' Never assume that a system has any acquisition devices.
            If Me.acquisition.Devices.Count = 0 Then
                OnScanMsg(New BatchScanEventArgs("Devices.Count = 0 -- No devices Found!"))
                Return
            End If

            Dim def As String = Me.acquisition.Devices.Default.Identity.ProductName

            ' Find the default device
            For Each dev As Device In Me.acquisition.Devices
                If dev.Identity.ProductName = def Then
                    Me.device = dev
                    Return
                End If
            Next dev
            OnScanMsg(New BatchScanEventArgs("Could Not Find Default device"))
        End Sub

#Region "Accessor Methods"

        ' Accessor Methods
        Public Property Despeckle() As Boolean
            Get
                Return despeckle_Renamed
            End Get
            Set(ByVal value As Boolean)
                despeckle_Renamed = value
            End Set
        End Property

        Public Property Deskew() As Boolean
            Get
                Return deskew_Renamed
            End Get
            Set(ByVal value As Boolean)
                deskew_Renamed = value
            End Set
        End Property

        Public Property OutputPath() As String
            Get
                Return outputpath_Renamed
            End Get
            Set(ByVal value As String)
                outputpath_Renamed = value
            End Set
        End Property
#End Region

        ' Starts scanning and processing from default device
        Public Sub Process()
            If Me.device Is Nothing Then
                OnScanMsg(New BatchScanEventArgs("Acquisition was canceled -- Device Error."))
                Return
            End If
            'raise an event
            OnScanMsg(New BatchScanEventArgs("Opening Device ..."))
            Me.device.Open()

            ' acquire an image with (showDeviceScrean, SavetoFile)
            ' The ADF will automaticaly read in all documents in scanner
            OnScanMsg(New BatchScanEventArgs("Scanning ..."))
            ' acquire image from scannar without displaying its interface and without saving directly
            ' to file
            Me.device.HideInterface = True
            Me.device.Acquire()

        End Sub

#Region "Acquisition Events"

        Private Sub acquisition_AcquireCanceled(ByVal sender As Object, ByVal e As System.EventArgs)
            OnScanMsg(New BatchScanEventArgs("Acquisition was canceled. -- Acquisition Event"))
        End Sub

        Private Sub acquisition_AcquireFinished(ByVal sender As Object, ByVal e As System.EventArgs)
            ' AcquireFinished fires after all images have been acquired.
            ' dispose of scanner device object
            Me.device.Close()
            OnScanMsg(New BatchScanEventArgs("Closing Device ..." & Constants.vbLf + Constants.vbCr & "Acquisition Finished."))
        End Sub

        ' This event is fired every time an page is scanned in
        Private Sub acquisition_ImageAcquired(ByVal sender As Object, ByVal e As Atalasoft.Twain.AcquireEventArgs)

            If Not e.Image Is Nothing Then
                Try
                    Dim tempImage As AtalaImage = AtalaImage.FromBitmap(e.Image)

                    ' Start a new workspace to apply comands to images.
                    ' make global workspace?
                    Dim theworkspace As Workspace = New Workspace()
                    theworkspace.Image = tempImage

                    'apply Comands
                    If Me.deskew_Renamed Then
                        OnScanMsg(New BatchScanEventArgs("Applying Deskew Command ... "))
                        theworkspace.ApplyCommand(New AutoDeskewCommand())
                        OnScanMsg(New BatchScanEventArgs("......Done"))
                    End If
                    If Me.despeckle_Renamed Then
                        OnScanMsg(New BatchScanEventArgs("Applying Despeckle Command ... "))
                        theworkspace.ApplyCommand(New DocumentDespeckleCommand())
                        OnScanMsg(New BatchScanEventArgs("......Done"))
                    End If

                    OnScanMsg(New BatchScanEventArgs("Image Acquired, Scanning for Barcodes ..."))
                    ' check if page scanned has barcodes.
                    ' isCoverSheet() ?

                    ' Check every page for barcodes
                    Barcoder_Load() ' load options
                    ' load the current image for barcode reading
                    readEngin = New BarCodeReader(theworkspace.Image)
                    ' Read the barcodes into the collection results
                    Dim results As BarCode() = recognizeBarcodes(readEngin, options)
                    ' Decode the barcode, if none are found, nothing will happen
                    decodeResults(results)
                    ' Display results
                    If results.Length = 1 Then
                        OnScanMsg(New BatchScanEventArgs(results.Length & " total barcode" & ("") & " found."))
                    Else
                        OnScanMsg(New BatchScanEventArgs(results.Length & " total barcode" & ("s") & " found."))
                    End If

                    ' Create directory if it does not already exist.  If it does, this
                    ' call does nothing.
                    Dim newpath As DirectoryInfo = New DirectoryInfo(Me.outputpath_Renamed)
                    newpath.Create()

                    ' Save the transformed image
                    ' Default is saveing to C:\TWAINoutput\untitled_xx.Tif
                    Dim filename As String = Me.outputpath_Renamed & filename_format & "_" & (getNumFiles()).ToString() & ".Tif"
                    OnScanMsg(New BatchScanEventArgs("Saving file to: " & filename))
                    theworkspace.Save(filename, New TiffEncoder())

                    ' dispose after we are done
                    theworkspace.Dispose()
                    tempImage.Dispose()
                Catch ex As System.Exception
                    MessageBox.Show(ex.ToString())
                Finally
                End Try
            End If
        End Sub

        '		private void acquisition_FileTransfer(object sender, Atalasoft.Twain.FileTransferEventArgs e)
        '		{
        '			// This will fire before a file transfer takes place.
        '			// Unused in this class.
        '
        '		}


        Private Sub acquisition_DeviceEvent(ByVal sender As Object, ByVal e As Atalasoft.Twain.DeviceEventArgs)
            ' One of the many device events has fired.
            ' You will only receive the events you have set using
            ' the Device.DeviceEvents property.

            ' Currently,  none have been set, but just in case ...
            OnScanMsg(New BatchScanEventArgs("Device Event:  " & e.Event.ToString()))
        End Sub

        ' an unexpected exception has occured, display it
        Private Sub acquisition_AsynchronousException(ByVal sender As Object, ByVal e As Atalasoft.Twain.AsynchronousExceptionEventArgs)
            OnScanMsg(New BatchScanEventArgs("Asynchronous Exception: " & e.Exception.Message))
        End Sub

#End Region

        ' helper methods

        ' Method to find number of untitled files already in directory
        ' returns number of files.  Next file should be named this number
        ' because numbering starts at 0;
        Private Function getNumFiles() As Int32
            Dim path As DirectoryInfo = New DirectoryInfo(outputpath_Renamed)
            Dim f As FileInfo() = path.GetFiles(filename_format & "*.Tif")
            Return f.Length
        End Function

        'Loads the Barcode reader
        Private Sub Barcoder_Load()
            ' set a few reasonable default options
            options = New ReadOpts()
            ' set the barcode font to code39.  This could be changed to account for letters, etc.
            options.Symbology = Symbologies.Code39
            ' read barcodes left to right and right to left
            options.Direction = Directions.East Or Directions.West
            ' counter-intuitive - these defaults get pulled from the UI instead
            ' of being pushed into the UI
            options.ScanInterval = 5 'default value
            options.ScanBarsToRead = 2 ' 2 barcodes expected per page for this example
        End Sub

        ' Read a set of barcodes from an image. 
        Private Function recognizeBarcodes(ByVal reader As BarCodeReader, ByVal optionsIn As ReadOpts) As BarCode()

            Dim results As BarCode() = Nothing
            Dim options As ReadOpts = New ReadOpts(optionsIn)

            If options.Symbology = 0 Then ' a symbology must be specified
                Return Nothing
            End If

            Try
                ' This is where the barcodes are read
                results = reader.ReadBars(options)
            Catch ex As ArgumentOutOfRangeException ' a few errors
                OnScanMsg(New BatchScanEventArgs("Range error in options: " & ex.Message))
            Catch ex As System.Exception
                OnScanMsg(New BatchScanEventArgs("General error: " & ex.Message))
            End Try

            Return results
        End Function

        'Parses the barcode data into the expected params
        ' if there are no barcodes on this page, it returns (not a cover page),
        ' otherwise the new filepath and filename format are saved. An incorrect number
        ' of barcodes on the page displays an error message, and the page is treated like
        ' it does not have any barcodes.
        Private Function decodeResults(ByVal bcInfo As BarCode()) As Boolean
            'bcInfo is a collection containing the information of each barcode found 
            If bcInfo.Length = 0 Then
                Return False
            End If
            If bcInfo.Length = 1 Then
                OnScanMsg(New BatchScanEventArgs("Error in decodeResults -- missing barcode."))
                Return False
            End If
            If bcInfo.Length > 2 Then
                'error
                OnScanMsg(New BatchScanEventArgs("Error in decodeResults -- more than 1 barcode on a page"))
                Return False
            End If
            ' Current Image is a cover Sheet, deal with acordignly

            ' if no barcodes are found on the first page, all pages will be saved to the default 
            ' location (c:\TWAINoutput) with filenames untitled_x.tif	
            setDefaults()

            Dim directory As Integer = 1
            Dim fname As Integer = 0

            If bcInfo(0).DataString.StartsWith("%") Then ' finds out which barcode is the filepath
                directory = 0
                fname = 1
            End If

            Dim info As String = bcInfo(directory).DataString
            ' set filename_format to the specified format (no special chars to parse out)
            filename_format = bcInfo(fname).DataString

            ' set outputpath to the directory structure indicated by the barcode
            For Each c As Char In (info.Substring(1)).ToCharArray()
                If c.Equals("%"c) Then
                    Me.outputpath_Renamed = Me.outputpath_Renamed & "\"
                Else
                    Me.outputpath_Renamed = Me.outputpath_Renamed & c
                End If
            Next c

            ' Create directory if it does not already exist.  If it does, this
            ' call does nothing.
            Dim newpath As DirectoryInfo = New DirectoryInfo(Me.outputpath_Renamed)
            newpath.Create()

            Return True
        End Function


    End Class ' end BatchScan class
End Namespace ' namespace myTWAINdemo
