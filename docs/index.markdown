---
layout: default
title: The ezr² Programming Language
nav_order: 2
---

# The `ezr²` Programming Language
{: .no_toc }

**ezr² is an easy to learn and practical interpreted programming language for beginners and experts alike made in C#!
For more information check out the [*Learn ezr²*](https://uralstech.github.io/ezrSquared/Learn-ezrSquared) page.**

<details open markdown="block">
  <summary>
    Table of contents
  </summary>
  {: .text-delta }
- TOC
{:toc}
</details>

## Advantages

- As close to natural language as possible while being practical, with [***planned multilingual features***](https://uralstech.github.io/ezrSquared/Multilingual-ezrSquared)
- Easily extendible through C# Assisted ezr² Libraries
- Easily embeddable

## PC Installation

To install ezr² on your PC, follow these steps:

#### Windows (x86 and x64)

* Download the appropriate installer from down below.
* Run the installer and go through the installation.
<br/><br/>

[ezr² Pre-release v1.5.1.3.0 for 32-bit Windows](https://github.com/Uralstech/ezrSquared/releases/download/prereleaseV1.5.1.3.0/ezrSquared.Installer.Windows.64-bit.exe){: .btn }<br/>
[ezr² Pre-release v1.5.1.3.0 for 64-bit Windows](https://github.com/Uralstech/ezrSquared/releases/download/prereleaseV1.5.1.3.0/ezrSquared.Installer.Windows.32-bit.exe){: .btn }

#### Linux (x64, ARM64 and ARM)

* Download the appropriate build from down below.
* Extract the `tar.xz` file to a folder of your choice.
* Optionally, add the folder to your PATH environment variable.
<br/><br/>

[ezr² Pre-release v1.5.1.3.0 for Linux x64](https://github.com/Uralstech/ezrSquared/releases/download/prereleaseV1.5.1.3.0/ezrSquared-linux-x64.tar.gz){: .btn }<br/>
[ezr² Pre-release v1.5.1.3.0 for Linux ARM64](https://github.com/Uralstech/ezrSquared/releases/download/prereleaseV1.5.1.3.0/ezrSquared-linux-arm64.tar.gz){: .btn }<br/>
[ezr² Pre-release v1.5.1.3.0 for Linux ARM](https://github.com/Uralstech/ezrSquared/releases/download/prereleaseV1.5.1.3.0/ezrSquared-linux-arm.tar.gz){: .btn }

### Check the installation

---
* Run the `ezrSquared` command in a terminal (PowerShell, CMD, Bash, etc.) and the ezr² Shell should open. You can press Ctrl+C to exit.
* Start coding!

## Android Installation

Download [***ezr² Portable Interpreter on Google Play***](https://play.google.com/store/apps/details?id=com.Uralstech.ezrSquaredPortableInterpreter).

If you are interested in how ezr² was embedded to the portable interpreter (a Unity app), check out the [***C# and ezr² page***](https://uralstech.github.io/ezrSquared/CSharp-and-ezrSquared).

## Other Operating Systems

For other OSes you can clone the [***repository***](https://github.com/Uralstech/ezrSquared/), and compile your own build. If you're a contributor, feel free to add the build to the latest release.

## Usage

An ezr² script has the extension `.ezr2`. To run an ezr² script, use the `ezrSquared` command followed by the path to the script file:

```cmd
> ezrSquared hello.ezr2
```

## Documentation

The official documentation for ezr² is available in the [***Learn ezr² page***](https://uralstech.github.io/ezrSquared/Learn-ezrSquared), but is still **work in progress**.
Meanwhile, check out some example programs in [***GitHub***](https://github.com/Uralstech/ezrSquared/tree/master/Tests).

The offline version of the ezrSquared website was made possible with [***Jekyll Offline***](https://github.com/dohliam/jekyll-offline).
The documentation is packaged with the Windows installer. For other OSes, download and extract the `zip` archive from here:

[ezr² Offline Documentation](https://github.com/Uralstech/ezrSquared/releases/download/prereleaseV1.5.1.3.0/ezrSquared.Offline.Documentation.zip){: .btn }

## Latest Updates

The latest version of ezr² ***RE*** has been released! ezr² ***RE***, or ***REwrite***, is the project's initiative to rewrite ezr². The latest working version of ezr² ***RE***
has many more features than the latest version of ezr²! But, it is still in development, has some essential features missing. Like the `include` expression, or any built-in object
methods like `"a string".length` or `["a", "list"].insert`. If you want to help in testing it out and fixing bugs, feel free to download the latest version of ezr² ***RE*** from
the ezr² GitHub releases page and compiling it using the .NET SDK and/or Visual Studio. 

ezr² now uses [***Semantic Versioning 2.0.0***](https://semver.org/) for new releases. ezr² development updates will be posted on [***my blog***](https://uralstech.github.io/).

## Contributing

ezr² is an open source project and welcomes contributions from anyone who wants to improve it.

If you want to contribute to ezr², please contact Uralstech at `info@uralstech.in`.

The contribution requirements will be revamped for the full release.
