#include <GUIConstantsEx.au3>
#include <WindowsConstants.au3>
#include <EditConstants.au3>
#include <StaticConstants.au3>


Global $UserInput;
Global $DoNotAskAgainFlag = False;
Global $DoNotAskAgain
Global $mainWindow;
Global $Started = False;

Func InputBoxEx($title, $prompt, $width, $height, $left, $top)
	Opt("GUIOnEventMode", 1)
	$mainWindow = GUICreate($title, $width, $height, $left, $top, -1, BitOR($WS_EX_TOPMOST, $WS_EX_OVERLAPPEDWINDOW))
	GUICtrlCreateLabel($prompt, 15, 10)
	$Remarks = GUICtrlCreateInput("Step Passed", 30, 40, 300, 20, -1)
	$DoNotAskAgain = GUICtrlCreateCheckbox("Do not ask again", 10, 70)
	$Pass = GUICtrlCreateButton("Pass", 220, 70, 50)
	$Fail = GUICtrlCreateButton("Fail", 280, 70, 50)

	; Set accelerators for Alt+p for pass, Alt+f for fail, Alt+d for do not ask again.
	Local $AccelKeys[3][2] = [["!p", $Pass],["!f", $Fail],["!d", $DoNotAskAgain]]
	GUISetAccelerators($AccelKeys)

	GUISetOnEvent($GUI_EVENT_CLOSE, "Cancelled")
	GUICtrlSetOnEvent($Pass, "Passed")
	GUICtrlSetOnEvent($Fail, "Failed")
	GUISetState(@SW_SHOW)

	WinActivate($title)

	;Dismiss the input comments box after a time limit.
	$PauseCount = 1
	While (1)
		Sleep(100);
		$remarkVal = GUICtrlRead($Remarks) ;Remarks
		If $UserInput = 0 Or $UserInput = 1 Or ($remarkVal = "Step Passed" And $PauseCount = $Timeout / 100 + 1) Then
			ExitLoop
		EndIf
		$PauseCount = $PauseCount + 1
	WEnd
	GUIDelete()

	Return $remarkVal
EndFunc   ;==>InputBoxEx

Func Passed()
	If GUICtrlRead($DoNotAskAgain) = 1 Then
		$DoNotAskAgainFlag = True
	EndIf
	$UserInput = 1
EndFunc   ;==>Passed

Func Failed()
	If GUICtrlRead($DoNotAskAgain) = 1 Then
		$DoNotAskAgainFlag = True
	EndIf
	$UserInput = 0
EndFunc   ;==>Failed

Func Cancelled()
	;Note: at this point @GUI_CTRLID would equal $GUI_EVENT_CLOSE,
	;and @GUI_WINHANDLE would equal $mainWindow
	GUIDelete()
EndFunc   ;==>Cancelled
