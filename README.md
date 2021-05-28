# ChatRoller Plugin

This unofficial TaleSpire plugin is for adding chat rolls with optional character sheet lookups.

## Install

Go to the releases folder and download the latest and extract to the contents of your TaleSpire game folder.

## Usage

Pick up and drop a mini for whom you want to roll. This is a required step.
In the chat type @ followed by a space and then a die roll expression using the regular D noration (e.g. 3D6).
The expression can use basic math operations like plus, minus, multiplication and division. If the selected
mini has a defined character sheet in the TaleSpire_CustomData folder then the character sheet replacement
will be applied when evaluating the roll. For example, one can define a Stealth roll which would equate to
1D20+PB+StealthBonus where PB and StealthBonus are also defined in the character sheet.

The character sheet is a file with a .CHS extension and contains one key/value pairs line separated by the
equal sign. Each key (the left side of the equal sign) will be replace with the corresponding value (the right
side of the equal sign) before evaluating the roll. Text can be added using single quotes and replacement will
not be done with text. When adding text numeric values after text, surround the values in brackets. For example:
Stealth='Stealth:'+(1D20+PB+StealthBonus)  

## Limitations

The character sheet lookup replacement are applied in the order that they are listed in the key/value pair
character sheet. This means attention has to be paid to the order of the replacement if some replacements
contain other replacements. For example:

````CHS
wis=18
wis mod=4
wis save=1D20+wis mod
````
Will not work correctly because "wis save" will replace "wis" with "18" to become "18 save". Thus we need to
re-order the items in ordert to make them work. Such as:
````CHS
wis save=1D20+wis mod
wis mod=4
wis=18
````
Now "wis" will get replaced with "18" only if it was not part of "wis save" or "wis mod".






