---
layout: landing
---

# The `ezr²` Programming Language

**ezr² is an easy to learn and practical interpreted programming language for beginners and experts alike made in C#!
For more information, check out the [*learn ezr² page*](docsrc/Learn-ezrSquared.md).**

## Advantages

- As close to natural language as possible while being practical, with [***planned multilingual features***](docsrc/Multilingual-ezrSquared.md)
- Easily extendible through C# Assisted ezr² Libraries
- Easily embeddable

## Installation

> [!WARNING]
> ezr² is still in development, and with [ezr² ***REwrite***](https://github.com/Uralstech/ezrSquared/tree/ezrSquared-re) on the horizon,
> there will be some breaking changes in syntax in upcoming versions.

To install ezr², follow the steps for your Operating System:

# [Windows](#tab/windows)

* Download the appropriate installer.
* Run the installer and go through the installation.
<br/>
 
[![ezr² Pre-release v1.5.1.3.0 for Windows x64](https://img.shields.io/badge/ezr%C2%B2_Pre--release_v1.5.1.3.0_%28Windows_x64%29-32CD32?style=for-the-badge&logo=github&logoColor=white)](https://github.com/Uralstech/ezrSquared/releases/download/prereleaseV1.5.1.3.0/ezrSquared.Installer.Windows.64-bit.exe)
[![ezr² Pre-release v1.5.1.3.0 for Windows x86](https://img.shields.io/badge/ezr%C2%B2_Pre--release_v1.5.1.3.0_%28Windows_x86%29-32CD32?style=for-the-badge&logo=github&logoColor=white)](https://github.com/Uralstech/ezrSquared/releases/download/prereleaseV1.5.1.3.0/ezrSquared.Installer.Windows.32-bit.exe)

# [Linux](#tab/linux)

* Download the appropriate build.
* Extract the `tar.xz` file to a folder of your choice.
* Optionally, add the folder to your PATH environment variable.
<br/>

[![ezr² Pre-release v1.5.1.3.0 for Linux x64](https://img.shields.io/badge/ezr%C2%B2_Pre--release_v1.5.1.3.0_%28Linux_x64%29-32CD32?style=for-the-badge&logo=github&logoColor=white)](https://github.com/Uralstech/ezrSquared/releases/download/prereleaseV1.5.1.3.0/ezrSquared-linux-x64.tar.gz)
[![ezr² Pre-release v1.5.1.3.0 for Linux ARM64](https://img.shields.io/badge/ezr%C2%B2_Pre--release_v1.5.1.3.0_%28Linux_ARM64%29-32CD32?style=for-the-badge&logo=github&logoColor=white)](https://github.com/Uralstech/ezrSquared/releases/download/prereleaseV1.5.1.3.0/ezrSquared-linux-arm64.tar.gz)
[![ezr² Pre-release v1.5.1.3.0 for Linux ARM](https://img.shields.io/badge/ezr%C2%B2_Pre--release_v1.5.1.3.0_%28Linux_ARM%29-32CD32?style=for-the-badge&logo=github&logoColor=white)](https://github.com/Uralstech/ezrSquared/releases/download/prereleaseV1.5.1.3.0/ezrSquared-linux-arm.tar.gz)

# [Android](#tab/android)

Download [***ezr² Portable Interpreter on Google Play***](https://play.google.com/store/apps/details?id=com.Uralstech.ezrSquaredPortableInterpreter).

If you are interested in how ezr² was embedded to the portable interpreter (a Unity app), check out the [***C# and ezr² page***](docsrc/CSharp-and-ezrSquared.md).

# [Other](#tab/other)

For other OSes you can clone the [***repository***](https://github.com/Uralstech/ezrSquared/), and compile your own build.

---

## Usage

An ezr² script has the extension `.ezr2`. To run an ezr² script, use the `ezrSquared` command followed by the path to the script file:

```cmd
> ezrSquared hello.ezr2
```

## Documentation

The official documentation for ezr² is available in the [***Learn ezr² page***](docsrc/Learn-ezrSquared.md), but is still **work in progress**.
Meanwhile, check out some example programs in [***GitHub***](https://github.com/Uralstech/ezrSquared/tree/master/Tests).

The offline version of the (old) ezrSquared website was made possible with [***Jekyll Offline***](https://github.com/dohliam/jekyll-offline).
The documentation is packaged with the Windows installer. For other OSes, download and extract the `zip` archive from here:

[![ezr² Pre-release v1.5.1.3.0 Offline Documentation](https://img.shields.io/badge/ezr%C2%B2_Pre--release_v1.5.1.3.0_Offline_Documentation-32CD32?style=for-the-badge&logo=github&logoColor=white)](https://github.com/Uralstech/ezrSquared/releases/download/prereleaseV1.5.1.3.0/ezrSquared.Offline.Documentation.zip)

## ezr² RE

ezr² is being rewritten, with better performance, more features and better compatibility with existing C# libraries!

ezr² RE still has a lot missing features, like the `include` expression and many built in type extension methods like `["list"].insert('2', 0)`.
It is also very unstable, and the API and syntax may change in updates. So, it is not recommended to use ezr² RE for scripting. If you want to try it
out, you can download it from the [***releases page on GitHub***](https://github.com/Uralstech/ezrSquared/releases/), or, from here:

# [Windows](#tab/REwindows)

* Download the appropriate installer.
* Run the installer and go through the installation.
<br/>
 
[![ezr² RE v0.8.5 for Windows x64](https://img.shields.io/badge/ezr%C2%B2_RE_v0.8.5_%28Windows_x64%29-32CD32?style=for-the-badge&logo=github&logoColor=white)](https://github.com/Uralstech/ezrSquared/releases/download/v0.8.5/ezrSquared.Shell.Setup.Win64.exe)
[![ezr² RE v0.8.5 for Windows x86](https://img.shields.io/badge/ezr%C2%B2_RE_v0.8.5_%28Windows_x86%29-32CD32?style=for-the-badge&logo=github&logoColor=white)](https://github.com/Uralstech/ezrSquared/releases/download/v0.8.5/ezrSquared.Shell.Setup.Win32.exe)

# [NuGet](#tab/REnuget)

You can use ezr² RE as a scripting language for your .NET apps by downloading it from NuGet!

[![ezr² RE on NuGet](https://img.shields.io/badge/ezr%C2%B2_RE_on_NuGet-32CD32?style=for-the-badge&logo=nuget&logoColor=white)](https://www.nuget.org/packages/ezrSquared)
[![ezr² RE v0.8.5 (.nupkg download)](https://img.shields.io/badge/ezr%C2%B2_RE_v0.8.5_%28.nupkg_download%29-32CD32?style=for-the-badge&logo=github&logoColor=white)](https://github.com/Uralstech/ezrSquared/releases/download/v0.8.5/ezrSquared.0.8.5-unstable.nupkg)

# [Other](#tab/REother)

For other OSes you can clone the [***branch***](https://github.com/Uralstech/ezrSquared/tree/ezrSquared-re), and compile your own build.

---

## Contributing

> [!NOTE]
> The contribution requirements will be revamped for the full ezr² ***RE*** release.

ezr² is an open source project and welcomes contributions from anyone who wants to improve it.
If you want to contribute to ezr², please contact Uralstech at `info@uralstech.in`.
