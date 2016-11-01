Func V_VerifyWinText($Arg1)
	$result = StringRegExp ($Arg1, "(.*),(\d+),(.*)", 1)
	$err = @error
	if $err = 0 then
		Opt ("WinTitleMatchMode", $result[1])
		
		$retVal = WinGetText ($result[0])
		WriteLog ("WinGetText", $result[0], $retVal, $retVal, False)
		Report_Results ("WinGetText", StringInStr($retVal, $result[2]))
		$BaseResult = $retVal
	Endif
EndFunc

Func V_VerifyFileExists($Arg1)
	$result = StringRegExp ($Arg1, "(.*),(\d)", 1)
	$err = @error
	if $err = 0 then
		$retVal = FileExists($result[0])
		WriteLog ("FileExists", $result[0], $retVal, $retVal, False)
		Report_Results ("FileExists", ($result[1] = $retVal))
		$BaseResult = $retVal
	EndIf
EndFunc

Func V_VerifyRegistry($Arg1)
	$split1_parse_result = StringRegExp ($Arg1, "(.*),(.*)", 1)
	$split2_parse_result = StringRegExp ($split1_parse_result[0], "(.*)\\(.*)", 1)
	$err = @error
	if $err = 0 then
		$retVal = RegRead($split2_parse_result[0], $split2_parse_result[1])
		WriteLog ("RegRead", $split2_parse_result[0] & ", " & $split2_parse_result[1], $retVal, $retVal, False)
		Report_Results ("RegRead", ($retVal = $split1_parse_result[1]))
		$BaseResult = $retVal
	Endif
EndFunc