---
layout: default
title: The ezr² Programming Language
nav_order: 1
---

# The `ezr²` Programming Language
{: .no_toc }

**ezr², or ezrSquared when you can't use the `²` symbol - is an easy to learn and practical interpreted programming language for beginners and experts alike made in C#!
For more information check out the [*Introduction and Documentation page*](https://uralstech.github.io/ezrSquared/Introduction).**

<details open markdown="block">
  <summary>
    Table of contents
  </summary>
  {: .text-delta }
- TOC
{:toc}
</details>

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

{: .warning }
**BEFORE YOU START CODING:** ezr² is still in **pre-release** status. That means **ezr² might get backwards-incompatible updates every now and then**.

Now, to install ezr² on your PC, follow these steps:

### Windows (32 and 64 bit)
{: .no_toc }

1. Download the latest release of ezr² (`ezrSquared.Installer.Windows.32-bit.exe` for 32-bit systems or `ezrSquared.Installer.Windows.64-bit.exe` for 64-bit systems) from [***GitHub***](https://github.com/Uralstech/ezrSquared/releases).
2. Run the installer and go through the installation.
3. Verify that ezr² is installed by running `ezrSquared` in a terminal - the ezr² biShell (built-in shell) should open (press Ctrl+C to exit).
4. Start coding!

### Linux (64 bit)
{: .no_toc }

1. Download the latest release of ezr² (`ezrSquared.Linux.tar.xz`) from [***GitHub***](https://github.com/Uralstech/ezrSquared/releases).
2. Extract the `tar.xz` file to a folder of your choice.
3. Add the folder to your PATH environment variable.
4. Verify that ezr² is installed by running `ezrSquared` in a terminal - the ezr² biShell (built-in shell) should open (press Ctrl+C to exit).
5. Start coding!

### Android
{: .no_toc }

Check out [***ezr² Portable Interpreter on Google Play***](https://play.google.com/store/apps/details?id=com.Uralstech.ezrSquaredPortableInterpreter), an interpreter made with the help of
[***ezr² Net4.8***](https://github.com/Uralstech/ezrSquaredNet4.8) in [***Unity***](https://unity.com/)!

### Other
{: .no_toc }

For other OSes you can clone the [***repository***](https://github.com/Uralstech/ezrSquared/), and compile your own build. If you're a contributor, feel free to add the build to the latest [***release***](https://github.com/Uralstech/ezrSquared/releases).

If you are interested in how ezr² was embedded to the portable interpreter, check out the [***C# in ezr² page***](https://uralstech.github.io/ezrSquared/CSAELs).

## Usage

An ezr² script has the extension `.ezr2`. To run an ezr² script, use the `ezrSquared` command followed by the path to the script file:

```cmd
> ezrSquared hello.ezr2
Use the "ezrSquared" command without any arguments to start the biShell:

> ezrSquared
You can type any valid ezr² expression in the shell and see its result.
```

## Documentation
The official documentation for ezr² is available in the [***Introduction and Documentation page***](https://uralstech.github.io/ezrSquared/Introduction), but is still **work in progress**.
Meanwhile, check out some example programs in [***GitHub***](https://github.com/Uralstech/ezrSquared/tree/master/Tests).

The offline version of the ezrSquared website was made possible with [***Jekyll Offline***](https://github.com/dohliam/jekyll-offline).
The documentation is packaged with the Windows installer. For other OSes, download and extract `ezrSquared.Offline.Documentation.zip`, which is available for download with each release on GitHub.

## Latest Updates
**For those confused by the versioning: 1st place -> Major; 2nd place -> Feature; 3rd place -> Quality of Life; 4th place -> Library; 5th place -> Patch**. I plan to switch to [***Semantic Versioning 2.0.0***](https://semver.org/) for the first stable release.

* **prerelease-1.5.1.3.0** - [03-05-23]
    * Added new functions `get_window_size`, `set_window_size`, `get_window_position` and `set_window_position` -
        to `Console` class in the IO library
    * Function `set_buffer_size` in `Console` class in the IO library now uses the `SetBufferSize` C# function

* **prerelease-1.5.1.2.0** - [01-04-23]
    * Added new functions `get_buffer_size` and `set_buffer_size` to `Console` class in the IO library
    * Unsupported functions in `Console` class in the IO library will now show new error if called

* **prerelease-1.5.1.1.0** - [27-03-23]
    * `Console.set_cursor_position` in the IO library now accepts two seperate integers

* **prerelease-1.5.1.0.1** - [25-03-23]
    * Fixed bug in special function `equals`

* **prerelease-1.5.1.0.0** - [23-03-23]
    * "all" keyword and ',' symbol now interchangeable in normal and QuickSyntax `include` expressions

* **prerelease-1.5.0.0.0** - [21-03-23]
    * More major changes to module/library system and `include` expression
    * Context name of value-derived types now the name of the derived type

## Contributing
ezr² is an open source project and welcomes contributions from anyone who wants to improve it.

If you want to contribute to ezr², please contact Uralstech at `info@uralstech.in`.
