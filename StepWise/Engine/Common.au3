#cs ----------------------------------------------------------------------------

	AutoIt Version: 3.2.2.0
	Author:         Anand Iyer

	Script Function: Common Functions
	StepWise5.5

#ce ----------------------------------------------------------------------------

Func C_Run($Arg1)
	SleepMgmt()
	$retVal = Run($Arg1)
	WriteLog("Run", $Arg1, $retVal, $retVal, False)
	$BaseResult = $retVal
EndFunc   ;==>C_Run

Func C_RunEx($Arg1)
	SleepMgmt()
	$retVal = Run($Arg1, "", @SW_MAXIMIZE)
	WriteLog("Run", $Arg1, $retVal, $retVal, False)
	$BaseResult = $retVal
EndFunc   ;==>C_RunEx

Func C_OpenFile($Arg1)
	SleepMgmt()
	$result = StringRegExp($Arg1, "(.*)", 1)
	$err = @error
	If $err = 0 Then
		$retVal = ShellExecute($result[0])
		WriteLog("ShellExecute", $Arg1, $retVal, $retVal, False)
		$BaseResult = $retVal
	EndIf
EndFunc   ;==>C_OpenFile

Func C_WinMove($Arg1)
	SleepMgmt()
	$result = StringRegExp($Arg1, "(.*),(.*),(.*)", 1)
	$err = @error
	If $err = 0 Then
		$retVal = WinMove($result[0], "", 0, 0, $result[1], $result[2])
		WriteLog("WinMove", $Arg1, $retVal, $retVal, False)
		$BaseResult = $retVal
	EndIf
EndFunc   ;==>C_WinMove

Func C_WinWaitActive($Arg1)
	SleepMgmt()
	if StringInStr ($Arg1, ",") > 1 Then
		$result = StringRegExp($Arg1, "(.*),(.*),(.*),(.*),(.*)", 1)
	Else
		$result = StringRegExp($Arg1, "(.*)", 1)
	EndIf

	$err = @error
	If $err = 0 Then
		If WinExists($result[0]) Then
			$retVal = WinActivate($result[0])
			$FunctionName = "WinActivate"
		Else
			$retVal = WinWaitActive($result[0])
			$FunctionName = "WinWaitActive"
		EndIf
		If $retVal = 0 Then
			$pass = 0
		Else
			$pass = 1
		EndIf
		WriteLog($FunctionName, $result[0], $retVal, $pass, True)
		$BaseResult = $BaseResult * $pass

		$retVal = WinSetState($result[0], "", @SW_RESTORE)
		If $retVal = 0 Then
			$pass = 0
		Else
			$pass = 1
		EndIf
		WriteLog("WinSetState", $result[0], $retVal, $pass, True)
		$BaseResult = $BaseResult * $pass

		If UBound ($result) > 1 Then
			$retVal = WinMove($result[0], "", $result[1],$result[2],$result[3],$result[4])
			If $retVal = 0 Then
				$pass = 0
			Else
				$pass = 1
			EndIf
			WriteLog("WinMove", $Arg1, $retVal, $pass, True)
			$BaseResult = $BaseResult * $pass
		EndIf
	EndIf
EndFunc   ;==>C_WinWaitActive

Func C_WinWaitActiveEx($Arg1)
	SleepMgmt()
	$result = StringRegExp($Arg1, "(.*),(\d),(\d)", 1)
	$err = @error
	If $err = 0 Then
		$retVal = Opt("WinTitleMatchMode", $result[1])
		WriteLog("Opt", "WinTitleMatchMode," & $result[1], $retVal, $retVal, False)
		$BaseResult = $BaseResult * $retVal

		;this is a workaround for not handling exceptions.  Certain windows popup sometimes, so WinWaitActive for such windows with the last parameter set to 1.
		If StringInStr($result[0], "Internet Explorer") > 1 Then
			$retVal = WinWaitActive_IE(2)
		Else
			If WinExists($result[0]) Then
				$retVal = WinActivate($result[0])
			Else
				$retVal = WinWaitActive($result[0])
			EndIf
		EndIf
		If $result[2] = 0 Then
			$pass = 1
		Else
			If $retVal = 0 Then
				$pass = 0
			Else
				$pass = 1
			EndIf
		EndIf
		WriteLog("WinWaitActive", $result[0] & ", 5", $retVal, $pass, True)
		$BaseResult = $BaseResult * $pass

		$retVal = WinMove($result[0], "", 0, 0, @DesktopWidth, @DesktopHeight)
		If $result[2] = 0 Then
			$pass = 1
		Else
			If $retVal = 0 Then
				$pass = 0
			Else
				$pass = 1
			EndIf
		EndIf
		WriteLog("WinWaitActive", $result[0] & ", 0, 0, 1440, 900", $retVal, $pass, True)
		$BaseResult = $BaseResult * $pass

		$retVal = Opt("WinTitleMatchMode", 1)
		WriteLog("Opt", "WinTitleMatchMode, 1", $retVal, $retVal, True)
		$BaseResult = $BaseResult * $retVal
	EndIf
EndFunc   ;==>C_WinWaitActiveEx

Func WinWaitActive_IE($idxTimes)
	$idx = 0
	While ($idx < 30)
		$retVal = (StatusbarGetText("Internet Explorer") = "Done")
		If $retVal = 1 Then
			;wait another two seconds and check, since Done status keeps changing.
			$idxTimes = $idxTimes - 1
			If $idxTimes > 0 Then
				Sleep(2000)
				$retVal = WinWaitActive_IE($idxTimes) ;recursive
			EndIf
			ExitLoop
		Else
			Sleep(1000)
			$idx = $idx + 1
		EndIf
	WEnd
	Return $retVal;
EndFunc   ;==>WinWaitActive_IE

Func C_WinActivate($Arg1)
	SleepMgmt()
	$retVal = WinActivate($Arg1)
	WriteLog("WinActivate", $Arg1, $retVal, $retVal, False)
	$BaseResult = $retVal
EndFunc   ;==>C_WinActivate

Func C_WinActivateEx($Arg1)
	SleepMgmt()
	$result = StringRegExp($Arg1, "(.*),(\d+)", 1)
	$err = @error
	If $err = 0 Then
		$retVal = Opt("WinTitleMatchMode", $result[1])
		WriteLog("Opt", "WinTitleMatchMode," & $result[1], $retVal, $retVal, False)
		$BaseResult = $BaseResult * $retVal

		$retVal = WinActivate($result[0])
		If $retVal = 0 Then
			$pass = 0
		Else
			$pass = 1
		EndIf
		WriteLog("WinActivate", $result[0], $retVal, $pass, True)
		$BaseResult = $BaseResult * $pass

		$retVal = Opt("WinTitleMatchMode", 1)
		WriteLog("Opt", "WinTitleMatchMode, 1", $retVal, $retVal, True)
		$BaseResult = $BaseResult * $retVal
	EndIf
EndFunc   ;==>C_WinActivateEx

Func C_WinClose($Arg1)
	SleepMgmt()
	$retVal = WinClose($Arg1)
	WriteLog("WinClose", $Arg1, $retVal, $retVal, False)
	$BaseResult = $retVal
EndFunc   ;==>C_WinClose

Func C_Send($Arg1)
	SleepMgmt()
	$retVal = Send("" & $Arg1 & "")
	WriteLog("Send", $Arg1, $retVal, $retVal, False)
	$BaseResult = $retVal
EndFunc   ;==>C_Send

Func C_RunCmd($Arg1)
	SleepMgmt()
	$retVal = _RunDos("" & $Arg1 & "")
	WriteLog("_RunDOS", $Arg1, $retVal, $retVal, False)
	$BaseResult = $retVal
EndFunc   ;==>C_RunCmd

Func C_RegWrite($Arg1)
	SleepMgmt()
	$result = StringRegExp($Arg1, "(.*),(.*),(.*),(.*)", 1)
	$err = @error
	If $err = 0 Then
		$retVal = RegWrite($result[0], $result[1], $result[2], $result[3])
		WriteLog("RegWrite", $result[0] & ", " & $result[1] & ", " & $result[2] & ", " & $result[3], $retVal, $retVal, False)
		$BaseResult = $retVal
	EndIf
EndFunc   ;==>C_RegWrite

Func C_RegDelete($Arg1)
	SleepMgmt()
	$result = StringRegExp($Arg1, "(.*),(.*)", 1)
	$err = @error
	If $err = 0 Then
		$retVal = RegDelete ($result[0], $result[1])
		WriteLog("RegDelete", $result[0] & ", " & $result[1], $retVal, $retVal, False)
		$BaseResult = $retVal
	EndIf
EndFunc   ;==>C_RegDelete

Func C_Sleep($Arg1)
	SleepMgmt()
	$result = StringRegExp($Arg1, "(.*),(-?\d+)", 1)
	$err = @error
	If $err = 0 Then
		$SleepTime = $result[0] * 1000
		$SleepCount = $result[1]
		$idxSleepCount = 0

		$retVal = 1 ;Successfully set the flag

		If $result[1] = -1 Then
			$appendtolog = " all lines"
		Else
			$appendtolog = "next " & $result[1] & " lines"
		EndIf
		WriteLog("Sleep", "Sleep time of " & $result[0] & "s inserted after every line for " & $appendtolog, $retVal, $retVal, False)
	EndIf
EndFunc   ;==>C_Sleep

Func C_LogoffSystem ()
	Shutdown(0) ;Logs off the system
EndFunc

Func C_RestartSystem ()
	Shutdown(6) ;Force a reboot
EndFunc

Func C_ShutdownSystem ()
	Shutdown(1) ;Shuts down the system
EndFunc

Func C_ShowDesktop()
	SleepMgmt()
	$retVal = WinMinimizeAll()
	WriteLog("WinMinimizeAll", "", $retVal, $retVal, False)
EndFunc   ;==>C_ShowDesktop

Func C_MouseClickDragEx($Arg1)
	SleepMgmt()
	$result = StringRegExp($Arg1, "(\w+),(\d+),(\d+),(\d+),(\d+),(\d)", 1)
	$err = @error
	If $err = 0 Then
		$FrontWindow = _WinAPI_GetForegroundWindow()

		$rect = _WinAPI_GetWindowRect($FrontWindow)

		$left = DllStructGetData($rect, 1) ;left
		$top = DllStructGetData($rect, 2) ;top

		$X1 = $result[1]
		$X2 = $result[3]
		$Y1 = $result[2]
		$Y2 = $result[4]

		If ($result[5] = 1) Then
			$screenX1 = $left + $result[1]
			$screenX2 = $left + $result[3]
			$screenY1 = $top + $result[2]
			$screenY2 = $top + $result[4]
		Else
			$screenX1 = $result[1]
			$screenX2 = $result[3]
			$screenY1 = $result[2]
			$screenY2 = $result[4]
		EndIf
		$retVal = MouseClickDrag($result[0], $screenX1, $screenY1, $screenX2, $screenY2)
		WriteLog("MouseClickDrag", $result[0] & ", " & $X1 & ", " & $Y1 & ", " & $X2 & ", " & $Y2, $retVal, $retVal, False)
		$BaseResult = $retVal
	EndIf
EndFunc   ;==>C_MouseClickDragEx

Func C_MouseClickEx($Arg1)
	SleepMgmt()
	$result = StringRegExp($Arg1, "(\w+),(\d+),(\d+),(\d),(\d)", 1)
	$err = @error
	If $err = 0 Then
		$FrontWindow = _WinAPI_GetForegroundWindow()

		$rect = _WinAPI_GetWindowRect($FrontWindow)

		$left = DllStructGetData($rect, 1) ;left
		$top = DllStructGetData($rect, 2) ;top

		$X = $result[1]
		$Y = $result[2]

		If ($result[4] = 1) Then
			$screenX = $left + $result[1]
			$screenY = $top + $result[2]
		Else
			$screenX = $result[1]
			$screenY = $result[2]
		EndIf

		$retVal = MouseClick("" & $result[0] & "", $screenX, $screenY, $result[3])
		WriteLog("MouseClick", $result[0] & ", " & $X & ", " & $Y & ", " & $result[3], $retVal, $retVal, False)
		$BaseResult = $retVal
	EndIf
EndFunc   ;==>C_MouseClickEx

Func C_CallInAutoIt()
	SleepMgmt()
	$CustomMethod = "C_" & $BaseMethod
	$result = StringSplit($BaseArgs, ",")
	$err = @error
	If $err = 0 Then
		Dim $BaseArgArray[$result[0] + 1]
		$BaseArgArray[0] = "CallArgArray"
		For $ArrayIndex = 1 To $result[0] ;number of elements
			$BaseArgArray[$ArrayIndex] = $result[$ArrayIndex]
		Next
		Call($CustomMethod, $BaseArgArray)
		If @error = 0xDEAD And @extended = 0xBEEF Then
			$SystemAbort = True
			Terminate()
		EndIf
	Else
		;MsgBox (0, "Called", $CustomMethod & " " & $BaseArgs)
		Call($CustomMethod, $BaseArgs)
		If @error = 0xDEAD And @extended = 0xBEEF Then
			$SystemAbort = True
			Terminate()
		EndIf
	EndIf
EndFunc   ;==>C_CallInAutoIt

;Use the following function as a template.  Function name is what follows "C_"
Func C_InputBox($Arg1, $Arg2, $Arg3)
	SleepMgmt()
	$retVal = InputBox($Arg1, $Arg2, $Arg3)
	WriteLog("InputBox", $Arg1, $retVal, $retVal, False)
	$BaseResult = $retVal
EndFunc   ;==>C_InputBox

Func C_Breakpoint($Arg1)
	If Not $batch Then
		If Not $breaks_disabled Then
			$StepDesc = "Script is Paused - " & $Arg1 & @CRLF & "Press F5 to proceed or F10 to step through [Line" & $lineno & "]"
			$AskComments = False;hard-code
			TogglePause()
			GetAskComments();Get from registry
		EndIf
	EndIf
EndFunc   ;==>C_Breakpoint

;Except Control_Send, most other AutoIt Control functions have been used in the following.
Func C_SelectItemInList($Arg1)
	$result = StringRegExp($Arg1, "(.*),(.*),(.*)", 1)
	$err = @error
	If $err = 0 Then
		ControlFocus($result[0], "", $result[1])
		$CurrentItem = Number(ControlListView($result[0], "", $result[1], "GetSelected"))
		$FindItem = ControlListView($result[0], "", $result[1], "FindItem", $result[2])
		If $FindItem < $CurrentItem Then
			$Direction = "{UP}"
		Else
			$Direction = "{DOWN}"
		EndIf

		While (1)
			If ControlListView($result[0], "", $result[1], "GetText", $CurrentItem, 0) = $result[2] Then
				ExitLoop
			Else
				Send($Direction);
				Sleep(500);
				$CurrentItem = Number(ControlListView($result[0], "", $result[1], "GetSelected"))
			EndIf
		WEnd
	EndIf
	WriteLog("SelectItemInList", $Arg1, 1, 1, True)
	$BaseResult = 1
EndFunc   ;==>C_SelectItemInList

Func C_SelectTab($Arg1)
	$result = StringRegExp($Arg1, "(.*),(.*),(\d+)", 1)
	$err = @error
	If $err = 0 Then
		ControlFocus($result[0], "", $result[1])
		If ControlCommand($result[0], "", $result[1], "CurrentTab", "") < $result[2] Then
			$Direction = "TabRight"
		Else
			$Direction = "TabLeft"
		EndIf

		While (1)
			If ControlCommand($result[0], "", $result[1], "CurrentTab", "") = $result[2] Then
				ExitLoop
			Else
				ControlCommand($result[0], "", $result[1], $Direction, "")
				Sleep(500)
			EndIf
		WEnd
	EndIf

	WriteLog("SelectTab", $Arg1, 1, 1, True)
	$BaseResult = 1
EndFunc   ;==>C_SelectTab

Func C_SelectItemInCombo($Arg1)
	$result = StringRegExp($Arg1, "(.*),(.*),(.*)", 1)
	$err = @error
	If $err = 0 Then
		ControlFocus($result[0], "", $result[1])
		ControlCommand($result[0], "", $result[1], "SelectString", $result[2])
	EndIf

	WriteLog("SelectItemInCombo", $Arg1, 1, 1, True)
	$BaseResult = 1
EndFunc   ;==>C_SelectItemInCombo

Func C_EditText($Arg1)
	$result = StringRegExp($Arg1, "(.*),(.*),(.*)", 1)
	$err = @error
	If $err = 0 Then
		ControlFocus($result[0], "", $result[1])
		ControlSetText($result[0], "", $result[1], $result[2])
	EndIf

	WriteLog("EditText", $Arg1, 1, 1, True)
	$BaseResult = 1
EndFunc   ;==>C_EditText

Func C_VerifyText($Arg1)
	$result = StringRegExp($Arg1, "(.*),(.*),(.*)", 1)
	$err = @error
	If $err = 0 Then
		If (ControlGetText($result[0], "", $result[1]) = $result[2]) Then
			$retVal = 1;
		Else
			$retVal = 0;
		EndIf
	EndIf

	WriteLog("VerifyText", $Arg1, $retVal, $retVal, True)
	$BaseResult = 1
EndFunc   ;==>C_VerifyText

Func C_Check($Arg1)
	$result = StringRegExp($Arg1, "(.*),(.*)", 1)
	$err = @error
	If $err = 0 Then
		ControlFocus($result[0], "", $result[1])
		ControlCommand($result[0], "", $result[1], "Check", "")
	EndIf

	WriteLog("Check", $Arg1, 1, 1, True)
	$BaseResult = 1
EndFunc   ;==>C_Check

Func C_Uncheck($Arg1)
	$result = StringRegExp($Arg1, "(.*),(.*)", 1)
	$err = @error
	If $err = 0 Then
		ControlFocus($result[0], "", $result[1])
		ControlCommand($result[0], "", $result[1], "Uncheck", "")
	EndIf

	WriteLog("Uncheck", $Arg1, 1, 1, True)
	$BaseResult = 1
EndFunc   ;==>C_Uncheck

Func C_VerifyCheck($Arg1)
	$result = StringRegExp($Arg1, "(.*),(.*),(\d)", 1)
	$err = @error
	If $err = 0 Then
		If (ControlCommand($result[0], "", $result[1], "IsChecked", "") = $result[2]) Then
			$retVal = 1;
		Else
			$retVal = 0;
		EndIf
	EndIf

	WriteLog("VerifyCheck", $Arg1, $retVal, $retVal, True)
	$BaseResult = 1
EndFunc   ;==>C_VerifyCheck

Func C_VerifySelectInCombo($Arg1)
	$result = StringRegExp($Arg1, "(.*),(.*),(.*)", 1)
	$err = @error
	If $err = 0 Then

		If (ControlCommand($result[0], "", $result[1], "GetCurrentSelection", "") = $result[2]) Then
			$retVal = 1;
		Else
			$retVal = 0;
		EndIf
	EndIf

	WriteLog("VerifySelectInCombo", $Arg1, $retVal, $retVal, True)
	$BaseResult = 1
EndFunc   ;==>C_VerifySelectInCombo

Func C_VerifySelectInList($Arg1)
	$result = StringRegExp($Arg1, "(.*),(.*),(.*)", 1)
	$err = @error
	If $err = 0 Then
		$SelItem = Number(ControlListView($result[0], "", $result[1], "GetSelected"))
		If (ControlListView($result[0], "", $result[1], "GetText", $SelItem) = $result[2]) Then
			$retVal = 1;
		Else
			$retVal = 0;
		EndIf
	EndIf

	WriteLog("VerifySelectInList", $Arg1, $retVal, $retVal, True)
	$BaseResult = 1
EndFunc   ;==>C_VerifySelectInList

Func C_VerifySubitemInList($Arg1)
	$result = StringRegExp($Arg1, "(.*),(.*),(.*),(.*)", 1)
	$err = @error
	If $err = 0 Then
		$ItemIdx = Number(ControlListView($result[0], "", $result[1], "FindItem", $result[2]))
		If ControlListView($result[0], "", $result[1], "GetText", $ItemIdx, 1) = $result[3] Then
			$retVal = 1;
		Else
			$retVal = 0;
		EndIf
	EndIf

	WriteLog("C_VerifySubitemInList", $Arg1, $retVal, $retVal, True)
	$BaseResult = 1
	Return $retVal
EndFunc   ;==>C_VerifySubitemInList

Func C_Focus($Arg1)
	$result = StringRegExp($Arg1, "(.*),(.*)", 1)
	$err = @error
	If $err = 0 Then
		$retVal = ControlFocus($result[0], "", $result[1])
	EndIf
	WriteLog("C_Focus", $Arg1, $retVal, $retVal, True)
	$BaseResult = 1
EndFunc   ;==>C_Focus
