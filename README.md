# ezr²

## Warning!
ezr² is still in beta! Huge changes can occur between updates and code written in one version may not work in the next or previous ones!

## What is it?
ezr² is an easy to learn and practical programming language made in C#

## Download link?
All releases of the shell and source code are available [here](https://github.com/Uralstech/ezrSquared/releases)

## Where can I learn ezr²?
The documentation won't be out till the full, stable release

## Are there any example scripts?
Yeah! Check them out [here](https://github.com/Uralstech/ezrSquared/tree/master/Tests)

## When will ezr² release?
idk lol

## The name feels familiar...
ezr² is heavily inspired by [ezr](https://github.com/Uralstech/ezrlang), which is another one of my projects -
Here are the similarities:
* The syntax is quite similar
* The code is based on the same concept
* Well, the name is basically the same

And the differences:
* ezr was made in Python
* QSyntax is not available in ezr
* Count loops are different
* All the variable types have had an overhaul in ezr²
* ezr² is much faster

## Progress
* **beta-1.0.0.0.2** - [23-12-22]
	* Fixed bug in `baseFunction` class - Changed `newContext.symbolTable = new symbolTable(context.parent.symbolTable)` to `newContext.symbolTable = new symbolTable(newContext.parent.symbolTable)`
	* For builtin function `run`, changed filename from full path to just file name
	* Removed unused `GenerateContext` definition in `item` class
	* Fixed bug in `shell` (now `biShell` - builtin shell) for paths given through command line - `Replace("\\", "\\\\")`

* **beta-1.0.0.0.1** - [23-12-22]
	* Fixed bug for builtin function `get` - Changed message output from `Writeline` to `Write`
	* Changed `value` class' `execute` function check from `left is not baseFunction` to `left is value`

* **beta-1.0.0.0.0** - [22-12-22]
	* Initial release!