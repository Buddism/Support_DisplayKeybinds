function clientCmdHasDisplayKeybinds()
{
	commandToServer('hasDisplayKeybinds');
}

//division is optional
function findKeybindIndex(%keyName, %division)
{	
	%doKeyCheck = %division  $= "";
	%doDivCheck = %division !$= "";

	for(%i = 0; %i < $RemapCount; %i++)
	{
		if(%doDivCheck && $RemapDivision[%i] !$= "")
		{
			%lastDiv = $RemapDivision[%i];

			%doKeyCheck = %lastDiv $= %division;
		}
		
		if(%doKeyCheck && $RemapName[%i] $= %keyName)
			return %i;
	}

	return -1;
}

function fetchKeybindInformation(%keybindInfo)
{
	%splitPosition = stripos(%keybindInfo, ":");
	if(%splitPosition != -1)
	{
		%divisionName = getSubStr(%keybindInfo, 0, %splitPosition);
		%keybindName = getSubStr(%keybindInfo, %splitPosition + 1, 999);
	} else {
		%keybindName = %keybindInfo;
	}

	%keybindIndex = findKeybindIndex(%keybindName, %divisionName);
	if(%keybindIndex == -1)
		return "Not-Found";
	
	%temp = moveMap.getBinding($RemapCmd[%keybindIndex]);
	if(%temp $= "")
		return "Unbound";

	%device = getField(%temp, 0);
	%object = getField(%temp, 1);

	if(%device !$= "" && %object !$= "")
		return strupr(getMapDisplayName(%device, %object));
}

function filterKeybindsString(%string)
{
	%lastGreatherThan = -1;
	for(%i = strlen(%string) - 1; %i >= 0; %i--)
	{
		%char = getSubStr(%string, %i, 1);
		if(%char $= ">")
		{
			%lastGreatherThan = %i;
			continue;
		}

		if(%char $= "<" && getSubStr(%string, %i, 5) $= "<key:" && %lastGreatherThan != -1)
		{
			%string = getSubStr(%string, 0, %i) @ fetchKeybindInformation(getSubStr(%string, %i + 5, %lastGreatherThan - %i - 5)) @ getSubStr(%string, %lastGreatherThan + 1, 999);
			%lastGreatherThan = -1;
		}
	}
	return %string;
}


package Support_DisplayKeybinds
{
	function clientCmdCenterprint(%string, %time)
	{
		return parent::clientCmdCenterprint(filterKeybindsString(%string), %time);
	}
	function clientCmdBottomPrint(%string, %time, %hidden)
	{
		return parent::clientCmdBottomPrint(filterKeybindsString(%string), %time, %hidden);
	}
	function clientCmdServerMessage (%msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10)
	{
		%msgString = filterKeybindsString(%msgString);
		
		return parent::clientCmdServerMessage (%msgType, %msgString, %a1, %a2, %a3, %a4, %a5, %a6, %a7, %a8, %a9, %a10);
	}
};
activatePackage(Support_DisplayKeybinds);