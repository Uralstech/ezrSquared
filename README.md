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
idk lol - but till it fully releases, try the prerelease version! If you find any bugs, report them [here](https://github.com/Uralstech/ezrSquared/issues)

## The name feels familiar...
ezr² is heavily inspired by [ezrlang](https://github.com/Uralstech/ezrlang), which is another one of my projects -
Here are the similarities:
* The syntax is quite similar
* The code is based on the same concept
* Well, the name is basically the same

And the differences:
* ezrlang was made in Python
* QSyntax is not available in ezrlang
* Count loops are different
* All the variable types have had an overhaul in ezr²
* ezr² is much faster

## Progress
**For those confused by the versioning: 1st place -> Major; 2nd place -> Feature; 3rd place -> Function; 4th place -> Library; 5th place -> Patch**

### Released
**Check the [GitHub Commits](https://github.com/Uralstech/ezrSquared/commits) for all changes in source code**

* **prerelease-1.1.0.0.6** - [31-01-23]
    * Fixed character_list comparison and hashing
    * Made shell code simpler

* **prerelease-1.1.0.0.5** - [24-01-23]
    * Improved globalPredefinedContext implementation

* **prerelease-1.1.0.0.4** - [23-01-23]
    * Fixed bug in function `as_integer` in float which crashed ezr² if the value was too large or too small
    * If a number is too large or too small for an integer, lexer now tags it as a float
    * Added new 'overflow' error tag

* **prerelease-1.1.0.0.3** - [23-01-23]
    * Improved count and quick count loops
    * Changed how comparative, bitwise and math operators work for integers

* **prerelease-1.1.0.0.2** - [18-01-23]
    * Fixed dictionary, array and list hashing
    * Added new `equals` special function - used for key comparison in dictionaries
        and as backup if `compare_equal` and / or `compare_not_equal` are / is not defined
    * `type_of` function now returns name of input if input is an object

* **prerelease-1.1.0.0.1** - [18-01-23]
    * Fixed bugs in and improved all comparative, bitwise and math operators

* **prerelease-1.1.0.0.0** - [17-01-23]
    * Builtin `Random` library
    * Small builtin `STD` library
    * CSAELs are back, in a different form - ezr² can now load library DLLs on runtime
    * Inheritance for classes - class definition is now `object (parent) name (with arg1 (, arg2)) do`,
        `!od (parent) name (n arg1 (, arg2)):` - optional code in brackets; A reference to the instance
        of the parent class will be created in the object named `parent`
    * Variable assignment for objects -
        Eg: `item testobj.testvar: 69` where `testobj` is an instance of a user defined class,
        the value of `testvar` in `testobj` will be `69`
    * Self reference in objects - on creation, objects will have a reference of themselfs called `this`;
        This is for better assignment and retrieval of variables inside objects
    * New bitwise operators -
        and (`&`), or (`|`), xor (`\`), not (`~`) and left (`<<`) and right (`>>`) shift
    * New assignment operators for quicker variable assignment - 
        addition (`:+`),
        subtraction (`:-`),
        multiplication (`:*`),
        division (`:/`),
        modulo (`:%`),
        power (`:^`),
        bitwise and (`:&`),
        bitwise or (`:|`),
        bitwise xor (`:\`),
        bitwise left shift (`:<`) and 
        bitwise right shift (`:>`)
    * Special functions - to be used like magic / dunder methods in Python
    * Revamped dictionary type
    * Changed quick count loop syntax to `!c (start ->) end (t step) (n variable): expression` -
        optional code in brackets
    * Count loop now supports float type
    * Include statement now returns `nothing`
    * Added functions `current` and `set_current` to `folder` class in `IO` library - 
        `current` returns path of the current working directory
        `set_current` sets the current working directory to given `folderpath`
    * Added function `abs` to integer and float (get absolute of value)
    * Added new / changed functions to string and character list types -
        `as_boolean` - parse object to a boolean value,
        `try_as_boolean` - try parse object to a boolean value,
        `is_null_or_empty` - check if object is null or empty,
        `is_null_or_spaces` - check if object is null or full of whitspaces and
        `as_boolean_value` - return boolean value of object
    * Other internal changes - check the commits

* **prerelease-1.0.0.0.1** - [30-12-22]
    * Fixed bug with unary operations - using the `+` operator would crash ezr²

* **prerelease-1.0.0.0.0** - [29-12-22]
    * Removed CSAEL support - because I believe CSAELs are unnecessary (check the release notes to know more)
    * Added (UNTESTED) builtin IO libraries (`file`, `folder` and `path`)
    * Added `try_as_integer` and `try_as_float` functions to `strings` and `character_lists`
    * A TON of 'polishing' - take that as you will, check the commits
    * Removed `test` CSAEL

* **beta-1.3.0.0.1** - [28-12-22]
    * Made CSAELs (`CSharp Assisted Ezr² Libraries`) much better - check the `test` CSAEL included in ezr²
    * Some other improvements which I'm too lazy to point out - check the commits

* **beta-1.3.0.0.0** - [26-12-22]
    * Support for `C# assisted ezr² libraries` - like `ezr Python libraries` for `ezrlang`

* **beta-1.2.0.0.0** - [25-12-22]
    * Overhaul of `object` class - object definitions are now the `class` type and instances are the `object` type
    * Made `run` function in `ezr.cs` require own context

* **beta-1.1.0.0.0** - [24-12-22]
    * New `characterList` class - a type of mutable `string`
    * Added `set` function to list - to replace element at given `index` with given `value`
    * Added `remove_at` function to list - to remove element at given `index`
    * Fixed `insert` function in list - changed `index` check to `indexAsInt > ((List<item>)storedValue).Count`
    * Added `as_string` function to `array`, `list` and `dictionary`
    * And many more! Check the commits!

* **beta-1.0.0.0.2** - [23-12-22]
    * Fixed bug in `baseFunction` class - changed `symboltable` creation to `new symbolTable(newContext.parent.symbolTable)`
    * Fixed bug in builtin function `run` - changed `filename` from full path to just file name
    * Fixed bug in `item` class - removed `GenerateContext` definition
    * Fixed bug in `shell` (now `biShell` - builtin shell) for paths given through CMD - added `Replace("\\", "\\\\")`

* **beta-1.0.0.0.1** - [23-12-22]
    * Fixed bug for builtin function `get` - changed message output from `Writeline` to `Write`
    * Changed `value` class' `execute` function check - to `left is value`

* **beta-1.0.0.0.0** - [22-12-22]
    * Initial release!

### TODO
* **More libraries**
* **More testing**