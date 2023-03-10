# The `ezr�` Programming Language
**ezr�, or ezrSquared when you can't use the `�` symbol - is an easy to learn and practical interpreted programming language for beginners and experts alike made in C#!
For more information check out the [*Introduction and Documentation page*](https://uralstech.github.io/ezrSquared/Introduction).**

## Update!
All information and documentation about ezr� has been moved to ***https://uralstech.github.io/ezrSquared/***!

## Advantages

- High-level
- Interpreted
- As close to "natural" language as possible while being practical
- Easily embeddable
- Easily extensible through C#
- Free and open source

## Features (Existing and Planned)

- Interactive shell
- Dynamic typing
- Object-oriented programming
- Error handling
- Rich set of operators
- Support for modules
- Support for C# through CSAELS
- Multiple paradigms
- Automatic memory management

## Installation
**BEFORE YOU START CODING:** ezr� is still in **pre-release** status. That means **ezr� might get backwards-incompatible updates every now and then**.

Now, to install ezr� on your PC, follow these steps:

### Windows
1. Download the latest release of ezr� (`ezrSquared.Windows.Installer.exe`) from [***GitHub***](https://github.com/Uralstech/ezrSquared/releases).
2. Run the installer and go through the installation.
3. Add the folder where ezr� has been installed (`C:\Users\[USER]\AppData\Local\Programs\ezr�` or `C:\Program Files (x86)\ezr�` by default) to your PATH environment variable.
4. Verify that ezr� is installed by running `ezrSquared` in a terminal - the ezr� biShell (built-in shell) should open (press Ctrl+C to exit).
5. Start coding!

### Ubuntu
1. Download the latest release of ezr� (`ezrSquared.Ubuntu.tar.xz`) from [***GitHub***](https://github.com/Uralstech/ezrSquared/releases).
2. Extract the `tar.xz` file to a folder of your choice.
3. Add the folder to your PATH environment variable.
4. Verify that ezr� is installed by running `ezrSquared` in a terminal - the ezr� biShell (built-in shell) should open (press Ctrl+C to exit).
5. Start coding!

### Android
Check out [***ezr� Portable Interpreter on Google Play***](https://play.google.com/store/apps/details?id=com.Uralstech.ezrSquaredPortableInterpreter), an interpreter made with the help of
[***ezr� Net4.8***](https://github.com/Uralstech/ezrSquaredNet4.8) in [***Unity***](https://unity.com/)!

## Usage

An ezr� script has the extension `.ezr2`. To run an ezr� script, use the `ezrSquared` command followed by the path to the script file:

```cmd
> ezrSquared hello.ezr2
Use the "ezrSquared" command without any arguments to start the biShell:

> ezrSquared
You can type any valid ezr� expression in the shell and see its result.
```

## Documentation
The official documentation for ezr� is available in the [***Introduction and Documentation page***](https://uralstech.github.io/ezrSquared/Introduction), but is still **work in progress**.
While I finish that, check out some example programs in [***GitHub***](https://github.com/Uralstech/ezrSquared/tree/master/Tests).

## Latest Updates
**For those confused by the versioning: 1st place -> Major; 2nd place -> Feature; 3rd place -> Quality of Life; 4th place -> Library; 5th place -> Patch**. I plan to switch to [***Semantic Versioning 2.0.0***](https://semver.org/) for the first stable release.

* **prerelease-1.3.3.0.0** - [05-03-23]
    * New `console` module for IO library
    * New `simple_show` builtin function
    * Fixed predefined function execution
    * Better error messages
    * Added changes for Linux version of builtin function `clear`

* **prerelease-1.3.2.0.0** - [15-02-23]
    * New runtimeRunError error class!

* **prerelease-1.3.1.0.0** - [14-02-23]
    * Fixed a count loop error message
    * Better error message for else if statements

* **prerelease-1.3.0.0.3** - [12-02-23]
    * Better `undefined` error message for special functions

* **prerelease-1.3.0.0.2** - [11-02-23]
    * Fixed object definition QuickSyntax

* **prerelease-1.3.0.0.1** - [07-02-23]
    * Fixed `in` expression
    * character_list `remove` operation now returns the removed character

* **prerelease-1.3.0.0.0** - [03-02-23]
    * ezr� now searches for name of DLL file as the main class in CSAELs
    * Error tags now correspond to their errors
    * New error tag `length-error`
    * Error tag `overflow` now `overflow-error`
    * New global variable for error tag `overflow`
    * Global variables `err_illop` and `err_undef` are now `err_illegalop` and `err_undefined`
    * Functions `insert`, `remove` in character_list now support string / character_list values of length > 1
    * Other new errors - check the commits

## Contributing
ezr� is an open source project and welcomes contributions from anyone who wants to improve it.

If you want to contribute to ezr�, please contact Uralstech at `info@uralstech.in`.

## License
ezr� is licensed under the Apache 2.0 license. See LICENSE.txt for more details.