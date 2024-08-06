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

To install ezr² on your PC, follow the steps for your Operating System:

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

For other OSes you can clone the [***repository***](https://github.com/Uralstech/ezrSquared/), and compile your own build. If you're a contributor, feel free to add the build to the latest release.

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

## Latest Updates

More frequent development updates will be posted on [***my blog***](https://uralstech.github.io/).

### New website

The website has been updated! Now, this website also contains the reference documentation! Please note that the reference documentation is only for ezr² ***RE***, not for older versions.

### ezr² ***RE*** Release

The latest version of ezr² ***RE*** has been released! ezr² ***RE***, or ***REwrite***, is the project's initiative to rewrite ezr². The latest working version of ezr² ***RE***
has many more features than the latest version of ezr²! But, it is still in development, has some essential features missing. Like the `include` expression, or any built-in object
methods like `"a string".length` or `["a", "list"].insert`. If you want to help in testing it out and fixing bugs, feel free to download the latest version of ezr² ***RE*** from
the ezr² GitHub releases page and compiling it using the .NET SDK and/or Visual Studio. 

### New Versioning Format

ezr² now uses [***Semantic Versioning 2.0.0***](https://semver.org/) for new releases.

## Contributing

> [!NOTE]
> The contribution requirements will be revamped for the full ezr² ***RE*** release.

ezr² is an open source project and welcomes contributions from anyone who wants to improve it.
If you want to contribute to ezr², please contact Uralstech at `info@uralstech.in`.
