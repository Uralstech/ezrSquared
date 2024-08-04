---
layout: default
title: "Multilingual ezr²"
nav_order: 3
---

# Multilingual ezr²

## Why?

Most programming languages like C, C#, Python, and Java, just to name a few, are English-based. Of course, they do not follow the standard grammar for natural English, but, you would agree that it would be much easier to learn these languages if you knew English. This can be a huge barrier to those who have never spoken a single word of English.

## How's it different from other multilingual (programming) languages?

There are a few examples of programming languages which may not require you to know English:

#### [***Scratch Jr.***](https://www.scratchjr.org/)

This is a programming language that is based on symbols and pictures. How far can you go with pictures?

Now, you can have language made up completely of symbols, Sign Language is a good example. But then, the user is forced learning a different language. Why not just learn English at that point and do conventional coding?

#### [***Hedy***](https://www.hedy.org/)

Hedy is a transpiler - something that converts Hedy code to Python code, and executes it. There is no method in which the Python code can be converted back to Hedy code in the way Hedy code is converted into Python code. Why is this important?

Well, let's assume the role of Programmer A. They have made a project which is written in French Hedy code. They are satisfied with the project and publish it on GitHub.

Here comes Programmer B, who likes A's project and downloads it, uses it and loves it. But, they have found a few bugs in the code! Sadly, they cannot fix the code as it is written in French Hedy, and they only know Hindi Hedy.

As you can see, the language barrier has prevented the potential collaboration between two programmers.

### So how *is* it different?

Imagine a programming language, which:
- Can be written in one natural language syntax;
- Can be published in a natural-language-neutral intermediate syntax (QuickSyntax);
- Can be converted back to another natural language syntax; And,
- Can be converted to QuickSyntax again to publish!

This means the natural language barrier is completely and utterly shattered! Yeah!

## How?

Well, this is still in a planning stage, but the structure should look like this:

### The Syntax Checkers

This includes the Lexer and Parser. There will be custom versions of these for each language, so that the translations are perfect.

### The Converter

This converts the Parsed nodes from/into QuickSyntax or serialized binaries.

### The Interpreter

The interpreter only interprets nodes that have been given to it. It does not know which language the source code is in.

Of course, this is quite a high-level summary, and, again, this is only in the planning stage.
