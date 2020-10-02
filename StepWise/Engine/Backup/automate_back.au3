#cs ----------------------------------------------------------------------------

	AutoIt Version: 3.2.2.0
	Author:         Anand Iyer

	Script Function: Main automation script
	StepWise

#ce ----------------------------------------------------------------------------

; Script Start - Add your code below here
#include <Process.au3>
#include <Date.au3>
#include <Common.au3>
#include <Verification.au3>
#include <UDF.au3>
#include <WinAPI.au3>
#include <Misc.au3>
#include <File.au3>
#include <Array.au3>
#include <UserInput.au3>
#include <ScreenCapture.au3>

Global $Pause
Global $Debug
Global $breaks_disabled
Global $batch
Global $StepDesc
Global $file
Global $Line
Global $dbgLine
Global $lineno = 1
Global $BaseMethod
Global $BaseArgs
Global $BaseResult
Global $RunResult
Global $AppendedLogText
Global $CustomMethod
Global $SystemAbort
Global $SleepTime = 500
Global $SleepCount = 0
Global $idxSleepCount = -1
Global $Timeout = 120;120 seconds default
Global $TimedOut = False
Global $AskComments = True
Global $err1, $err2, $err3, $err4

Dim $szDrive, $szDir, $szFName, $szExt
$FilePathParts = _PathSplit($CmdLine[1], $szDrive, $szDir, $szFName, $szExt)

SplashTextOn("", "CopyRight 2012-2014 Anand Iyer", 200, 40, 1300, 5, 5, "", 8);titleless, left justified

HotKeySet("{F5}", "TogglePause")
HotKeySet("{F10}", "TogglePause")
HotKeySet("{ESC}", "Terminate")

$rect = DllStructCreate("int;int;int;int")

$RunResult = "PASS"; ;default to pass.

$dbglogfile = FileOpen(@ScriptDir & "\..\debuglog.txt", 1)
If $dbglogfile = -1 Then
	MsgBox(0, "Error", "Unable to open logfile.  Cannot proceed")
	Exit
EndIf
FileWriteLine($dbglogfile, "")
FileWriteLine($dbglogfile, "**********************")
FileWriteLine($dbglogfile, _Now())
FileWriteLine($dbglogfile, "**********************")

If $CmdLine[1] = "-help" Then
	$help = "Usage: " & """automate.exe <script-file> [-debug|-nobreaks|-batch]"""
	ToolTip($help, 0, 0)
	FileWriteLine($dbglogfile, $help)
	Sleep(1000)
	Exit
EndIf

$file = FileOpen($CmdLine[1], 0)
If $file = -1 Then
	WriteLog("Unable to open script file.", "", "", 0, False)
	Exit
EndIf

$Line = FileReadLine($file)
$lineno = $lineno + 1
;Don't proceed to run if the first line of the text file is not ";Test script file ;"
If ($Line <> ";Test script file ;") Then
	WriteLog("The script file doesn't conform to the required format." & @CRLF & "Can't execute.", "", "", 0, False)
	Exit
EndIf

$dll = DllOpen("user32.dll")
If $dll = -1 Then
	WriteLog("Unable to open specified dll", "", "", 0, False)
	Exit
EndIf

;Separate logfile with PASS/FAIL status
$FromFile = _PathFull(@ScriptDir & "\..\ScriptLog")
$logfile = FileOpen($FromFile, 2) ;this will be moved into $logfilename after the execution is completed.
If $logfile = -1 Then
	WriteLog("Unable to open script log file.", "", "", 0, False)
	Exit
EndIf

;If debug (F10) option is chosen instead of Run (F5) from the app.
If ($CmdLine[0] == 2) Then
	If $CmdLine[2] == "-debug" Then
		$Debug = True;
	EndIf
	If $CmdLine[2] == "-nobreaks" Then
		$breaks_disabled = True;
	EndIf
	If $CmdLine[2] == "-batch" Then
		$breaks_disabled = True;
		$batch = True;
	EndIf
EndIf

;Create the log folder
Dim $logFolder = @ScriptDir & "\..\Logs\" & $szFName
DirCreate(_PathFull($logFolder))

;Get timeout value from registry
GetTimeout()

;Get AskComments value from registry
GetAskComments()

; Read in lines of text until the EOF is reached
While 1
	HandleExceptions()

	$Line = FileReadLine($file)
	$EndFile = @error

	;Avoid lines that starts with a semicolon.
	If (StringRegExp($Line, "^;", 0)) Then
		$lineno = $lineno + 1
		ContinueLoop
	EndIf

	$parse_result = StringRegExp($Line, "(\w+) (.*[^;])?;(.*)", 1)
	$err1 = @error
	If $err1 = 0 Then
		$BaseMethod = $parse_result[0]
		$BaseArgs = $parse_result[1]
		$CombString = $BaseMethod & " " & $BaseArgs
		$BaseResult = 1 ;assume the function is a pass, before it's called.

		Switch $BaseMethod
			;Methods-
			;dummy-don't delete, provides a blank line for a category.
			;Methods-Common-Basic-Basic
			Case "Run"
				;Run specified program ;Program to run [Browse File]
				C_Run($BaseArgs)
			Case "RunEx"
				;Run specified program with maximised window ;Program to run [Browse File]
				C_RunEx($BaseArgs)
			Case "Record"
				;Record keystrokes and mouse movements;Title of window to focus on+[Select Window];Keystrokes and mouse movements+[Record]
			Case "WinWaitActive"
				;Pauses execution until specified window is active ;Window title [Select Window]
				C_WinWaitActive($BaseArgs)
			Case "WinWaitActiveEx"
				;Pauses execution until specified window is active ;Window title [Select Window];Title match mode [1,2,3]+(1: Partial, 2: Substring, 3: Exact) ;Must exist [1,0]+(1: True, 0: False)
				C_WinWaitActiveEx($BaseArgs)
			Case "WinActivate"
				;Activate specified window;Window title [Select Window]
				C_WinActivate($BaseArgs)
			Case "WinActivateEx"
				;Activate specified window;Window title [Select Window];Title match mode [1,2,3]+(1: Partial, 2: Substring, 3: Exact)
				C_WinActivateEx($BaseArgs)
			Case "WinClose"
				;Close specified window ;Window title [Select Window]
				C_WinClose($BaseArgs)
			Case "WinMove";only execute
				;Move/resize specified window ;Window title [Select Window];Window width (pixels);Window height (pixels)
				C_WinMove($BaseArgs)
			Case "Send"
				;Send specified keystrokes,no parameters ;Keystrokes
				C_Send($BaseArgs)
			Case "MouseClickEx";only execute
				;Send specified mouse movements,no parameters
				C_MouseClickEx($BaseArgs)
			Case "MouseClickDragEx";only execute
				;Send specified mouse movements,no parameters
				C_MouseClickDragEx($BaseArgs)

			;Methods-Common-Basic-System
			Case "ShowDesktop"
				;Minimize all windows
				C_ShowDesktop()
			Case "SystemRestart"
				;Restart the system
				C_RestartSystem()
			Case "SystemShutdown"
				;Shutdown the system
				C_ShutdownSystem()
			Case "SystemLogoff"
				;Logs off the system
				C_LogoffSystem()

			;Methods-Common-Basic-StepWise
			Case "InsertSleep"
				;Sleep for specified time after given number of lines;Time to sleep (sec)+[0.5,1,2,3,4,5];For how many lines?+(Type -1 for All lines)
				C_Sleep($BaseArgs)
			Case "Breakpoint"
				;Inserts a breakpoint after execution of previous statement;Text to display while+script is paused.
				C_Breakpoint($BaseArgs)
			Case "Parameterize"
				;Add Parameters (To be used in the context of am existing test script);Parameter order[1,2,3,4,5,6];Parameter name;Parameter type[Select Window,Select Process,Browse File,none]
			Case ";Test script file"
				;Script Header

			;Methods-Common-Basic-Verification
			Case "VerifyRegistry"
				;Verify if the registry key has the value as specified;Key full path;Expected value
				V_VerifyRegistry($BaseArgs)
			Case "VerifyFileExists"
				;Verify if the specified file exists; Filename [Browse File]; Condition [1,0]+(True/False)
				V_VerifyFileExists($BaseArgs)

			;Methods-Common-Basic-Others
			Case "OpenFile"
				;Open specified file in associated program;File to open [Browse File]
				C_OpenFile($BaseArgs)
			Case "RegWrite"
				;Write to registry;Key name to write to;Value name to write to;Type of key to write;Value to write
				C_RegWrite($BaseArgs)
			Case "RegDelete"
				;Delete from registry;Key name;Value name to be deleted
				C_RegDelete($BaseArgs)
			Case "RunCmd"
				;Run specified command;Command text
				C_RunCmd($BaseArgs)

			;Methods-Common-Components-Basic
			Case "SelectTab"
				;Focus on specified tab ;Window title [Select Window];Name of the tab control+(Use 'Tools->Window Info');Tab number to focus on
				C_SelectTab($BaseArgs)
			Case "SelectItemInList"
				;Select specified item in console(not working presently);Window title [Select Window];Name of the control+(Use 'Tools->Window Info');Name of the item to select
				C_SelectItemInList($BaseArgs)
			Case "SelectItemInCombo"
				;Select specified item in a combo-box;Window title [Select Window];Name of the control+(Use 'Tools->Window Info');Name of the item to select
				C_SelectItemInCombo($BaseArgs)
			Case "EditTextBox"
				;Edits text in textbox;Window title [Select Window];Name of the control+(Use 'Tools->Window Info');Text to type
				C_EditText($BaseArgs)
			Case "CheckBox"
				;Check specified radiobutton/checkbox;Window title [Select Window];Name of the control+(Use 'Tools->Window Info')
				C_Check($BaseArgs)
			Case "UncheckBox"
				;Uncheck specified radiobutton/checkbox;Window title [Select Window];Name of the control+(Use 'Tools->Window Info')
				C_Uncheck($BaseArgs)
			Case "FocusObject"
				;Focuses on specified control;Window title [Select Window];Name of the control+(Use 'Tools->Window Info')
				C_Focus($BaseArgs)

			;Methods-Common-Components-Verification
			Case "VerifySelectInList"
				;Verify if list box has the specified selection (not working presently);Window title [Select Window];Name of the control+(Use 'Tools->Window Info');Expected item
				C_VerifySelectInList($BaseArgs)
			Case "VerifySubitemInList"
				;Verify sub-item of specified item in list box(not working presently);Window title [Select Window];Name of the control+(Use 'Tools->Window Info');Item (First column);Expected subitem+(Second column)
				C_VerifySubitemInList($BaseArgs)
			Case "VerifySelectInCombo"
				;Verify if combo-box has the specified selection;Window title [Select Window];Name of the control+(Use 'Tools->Window Info');Expected item
				C_VerifySelectInCombo($BaseArgs)
			Case "VerifyTextBox"
				;Verify if textbox has specified text;Window title [Select Window];Name of the control+(Use 'Tools->Window Info');Expected text
				C_VerifyText($BaseArgs)
			Case "VerifyCheckBox"
				;Verify if checkbox is checked/unchecked;Window title [Select Window];Name of the control+(Use 'Tools->Window Info');Expected status[0,1]+(0:Unchecked,1:Checked)
				C_VerifyCheck($BaseArgs)
			Case "VerifyWinText"
				;Verify if the text in the window is as specified; Window name [Select Window];Expected text
				V_VerifyWinText($BaseArgs)

			;Methods-Browser
			Case "IE_Navigate"
				;Opens IE and navigates to specified URL;URL to navigate to
				IE_Navigate($BaseArgs);

			;Methods-UDF
			Case "UDF1"
				;
				UDF1($BaseArgs)

			;every other case
			Case Else
				C_CallInAutoIt()
		EndSwitch

		;Writing basemethod result
		WriteLog("Line " & $lineno & ": [" & $BaseMethod & "]", "", "", $BaseResult, False)

		;Check if we execution needs to pause.  Same section handles comments
		If $Debug Then
			$StepDesc = "Debug session in progress...Press F5 to proceed or F10 to step through [Line:" & $lineno & "]"
			TogglePause()
		EndIf
	EndIf

	$lineno = $lineno + 1

	Sleep (500);500 ms is default sleep time between each step

	If $EndFile = -1 Then ExitLoop ;
WEnd

ToolTip("Run completed.  Check debuglog.txt", 0, 0)
Sleep(2000)

CleanUp()

Func CleanUp()
	FileClose($file)
	FileClose($logfile)
	FileClose($dbglogfile)
	FileClose($dll)

	GUIDelete($mainWindow)

	$logfilename = _PathFull($logFolder & "\" & StringRegExpReplace(_Now(), "/", "-"))
	$logfilename = StringRegExpReplace($logfilename, "(\d+):(\d+):(\d+)", "\1\2\3");
	$ToFile = $logfilename & "." & $RunResult

	$FMR = FileMove($FromFile, $ToFile, 9)
EndFunc   ;==>CleanUp

Func TogglePause()
	$Pause = Not $Pause

	If $Pause And Not $DoNotAskAgainFlag And $AskComments Then
		WaitForInput("Input comments")
	EndIf

	$PauseCount = 1
	While ($Pause And $PauseCount <= $Timeout / 100)
		If $StepDesc <> "" Then
			ToolTip($StepDesc, 0, 0)
		Else
			$StepDesc = "Debug session in progress...Press F5 to proceed or F10 to step through [Line:" & $lineno & "]" ;so that the while loop satisfies the if condition, during next iteration
		EndIf
		Sleep(100)
		$PauseCount = $PauseCount + 1
	WEnd

	;Terminate execution after certain time limit
	If $PauseCount = ($Timeout / 100) + 1 Then
		$TimedOut = True;
		TakeScreenShot()
		Terminate();
	EndIf

	ToolTip("")

	If _IsPressed("74", $dll) Then ;F5 pressed
		$Debug = False;
	EndIf
	If _IsPressed("79", $dll) Then ;F10 pressed
		$Debug = True;
	EndIf
EndFunc   ;==>TogglePause

Func WaitForInput($StepDesc)
	$UserInput = -1 ;Reset
	$answer = InputBoxEx("Script is Paused - " & $StepDesc, "Enter your observations (Will be logged).", 350, 100, 0, 0)
	$answer = "[" & $BaseMethod & "] User said: " & '"' & $answer & '"'
	WriteLog($answer, "", "", $UserInput, False)
EndFunc   ;==>WaitForInput

Func Report_Results($Method, $Result)
	If $Result Then
		$pass = 1
	Else
		$pass = 0
	EndIf
	WriteLog($Method & " is verified.", "", "", $pass, False)
EndFunc   ;==>Report_Results

Func Terminate()
	If $TimedOut Then
		CleanUp();
		Exit (0)
	EndIf

	If $SystemAbort Then
		$Title = "System Aborted."
		$Message = "Function (" & $CustomMethod & ") does not exist or wrong number of parameters passed." & @CRLF & @CRLF & "Did the test pass?"
	Else
		$Title = "Esc key pressed."
		$Message = "Did the test pass?"
	EndIf

	$ButtonPressed = MsgBox(4387, $Title, $Message)
	If $ButtonPressed = 6 Then
		$answer = "[" & $BaseMethod & "]" & $Title & " User said: ""Test passed"""
		WriteLog($answer, "", "", 1, False)
		$RunResult = "PASS"
		CleanUp()
		Exit 0
	Else
		If $ButtonPressed = 7 Then
			$answer = "[" & $BaseMethod & "]" & $Title & " User said: ""Test failed"""
			WriteLog($answer, "", "", 0, False)
			$RunResult = "FAIL"
			CleanUp()
			Exit 0
		Else
			Return ;Cancel - Do nothing else
		EndIf
	EndIf
EndFunc   ;==>Terminate

Func WriteLog($Method, $Args, $retVal, $pass, $Append)
	$Line = $Method;
	$dbgLine = $Method;
	If ($Args <> "") Then
		$dbgLine = $dbgLine & " (" & $Args
		$dbgLine = $dbgLine & ") returned " & $retVal
	EndIf

	If $pass == 0 Then
		TrayTip("Message", "Failed at line no. " & $lineno & " (" & $BaseMethod & ")" & @CRLF & "Execution has been paused (Esc to quit, F5 to execute)", 5, 3)
		$Debug = True;
		Sleep(1000)
		If (StringRegExp($Method, "\[" & $BaseMethod & "\]", 0)) Then
			$dbgLine = $dbgLine & " --FAILED"
			$Line = $dbgLine
			$RunResult = "FAIL"
			TakeScreenShot()
		EndIf
	Else
		If (StringRegExp($Method, "\[" & $BaseMethod & "\]", 0)) Then
			$dbgLine = $dbgLine & " --PASSED"
			$Line = $dbgLine
			$RunResult = "PASS"
		EndIf
	EndIf

	FileWriteLine($dbglogfile, $dbgLine)
	If $Line <> $Method Then
		FileWriteLine($logfile, $Line)
	EndIf

	If $Args <> "" Then
		If $Append Then
			$AppendedLogText = $AppendedLogText & @CRLF & $dbgLine
		Else
			$AppendedLogText = $dbgLine
		EndIf

		TraySetState()
		TraySetToolTip($AppendedLogText)
	EndIf
EndFunc   ;==>WriteLog

;Use in conjunction with AdlibEnable if required.
Func HandleExceptions()
	Local $ExceptionWindows[10] = ["User access control", "Confirm Save As", "Default Browser"]
	Local $ExceptionHandlers[10] = ["{ENTER}", "{LEFT}{ENTER}", "{RIGHT}{ENTER}"]

	Sleep(100)
	For $index = 0 To 9
		If WinActive($ExceptionWindows[$index]) Then
			Send($ExceptionHandlers[$index])
		EndIf
	Next
EndFunc   ;==>HandleExceptions

Func SleepMgmt()
	;if $SleepCount = -1, it means insert sleep time after every line in the script.
	If ((($idxSleepCount <= $SleepCount) Or $SleepCount = -1) And ($idxSleepCount <> -1)) Then
		Sleep($SleepTime)
		$idxSleepCount = $idxSleepCount + 1
	Else
		$idxSleepCount = -1
	EndIf
EndFunc   ;==>SleepMgmt

Func TakeScreenShot()
	$ScreenCapture = RegRead("HKEY_LOCAL_MACHINE\SOFTWARE\StepWise", "ScreenCapture")
	If $ScreenCapture = 1 Then
		$screenshotname = _PathFull($logFolder & "\" & StringRegExpReplace(_Now(), "/", "-"))
		$screenshotname = StringRegExpReplace($screenshotname, "(\d+):(\d+):(\d+)", "\1\2\3" & ".png")
		_ScreenCapture_Capture($screenshotname)
		Sleep(1000)
	EndIf
EndFunc   ;==>TakeScreenShot

Func GetTimeout()
	$Timeout = RegRead("HKEY_LOCAL_MACHINE\SOFTWARE\StepWise", "Timeout")
	If $Timeout <> "" Then
		$Timeout = $Timeout * 1000;convert to ms
	Else
		$Timeout = 120 * 1000;2 minutes is default
	EndIf
EndFunc   ;==>GetTimeout

Func GetAskComments()
	$AskComments = RegRead("HKEY_LOCAL_MACHINE\SOFTWARE\StepWise", "AskComments")
	If $AskComments = "" Or $AskComments <> "0" Then
		$AskComments = True;
	Else
		$AskComments = False;
	EndIf
EndFunc   ;==>GetAskComments
