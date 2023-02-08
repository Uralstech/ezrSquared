# ezr² Wiki

## Why Learn ezr²?
ezr² is a programming language that's **easy to learn** and **practical to use**.
ezr² can be learnt by anyone, **of any age**, in a few minutes. Anyone can extend
the functionalities of ezr² with **libraries**. If you already know C#, you can even
help in ezr² development with **C# Assisted ezr² Libraries** (**CSAELs**)! CSAELs bring
the existing functionality of C# to ezr²! Experianced ezr² programmers can even ditch the
boilerplate syntax for the shorter **QuickSyntax**. The normal syntax satisfies the beginner,
as it is easy to use and QuickSyntax satisfies the expert, as it is very short.

## Tutorial For Beginners
This section kicks off an extensive tutorial that will get you up and running with ezr²!
(Heavily inspired by [this Python tutorial](https://python.land/python-tutorial))

### Hello World (Displaying Text To The Screen)
There’s a tradition in which programming tutorials start with a so-called **Hello World** program.
A Hello World program simply prints the words “Hello world” to the screen. We use the `show()`
function to do so.

```
show("Hello, World!")
```

The `show()` function takes anything you put between the parentheses and prints it to the screen.
But we must feed it the correct type of data for it to work. For example, text in ezr² is always put between **double quotes**.
In the world of computer programming, we call this a **string**.

Double quotes around strings are essential because they precisely **mark the start and end** of a string.
This way, a string is easy to recognize for ezr². Here are a few more examples of valid strings:

```
"Hello world"
"My name is Uday"
"This one is a bit longer. There is no limit to how long a string can be!"
```

You also might have noticed that after printing "Hello, World!" to the screen, the `show()` function also printed "nothing".
This means **the function returned nothing**. If you code in `"Example"` and enter, only `"Example"` will be printed to the screen.
This means **the expression `"Example"` returned the string "Example"** - ezr² is only showing that to the screen.

### Numbers
Now that we’ve seen strings let’s move on to **numbers**. Just like strings,
we can ask ezr² to print numbers using the `show()` function.
Unlike strings, numbers **don’t need quotes around them**.
So to print the number 10, use this:

```
show(10)
```

Try some of these operators on numbers!

| Operator |      Name      | Example |
|----------|:--------------:|--------:|
|     +    | addition       |  2 + 6  |
|     -    | subtraction    |  8 - 4  |
|     *    | multiplication |  3 * 23 |
|     /    | division       |  6 / 2  |
|     %    | modulo         |  9 % 2  |
|     ^    | powered by     |  4 ^ 2  |

### Floating point numbers
While playing around with the division operator, you might have gotten some
rounded off results - for example `3 / 2` returns `1` instead of `1.5` which is correct.
In computer programming, there’s a strong distinction between non-fractional numbers like
1, 3, and 42 and fractional numbers like 3.14. The former are called **integers**,
while the latter are called **floats**. Now try this:

```
show(3/2.0)
```

This should display `1.5` to the screen, because we have specified that `2.0` is a **float**.
**An integer divided by another integer will always return an integer, but the same done with a float
will always return a float.** This rule is the same for all operators.

### Variables
Wouldn’t it be nice to store the results of the calculations we made above? For this, we use **variables**.
**Variables allow us to store things in memory** for as long as the ezr² program runs. Like a restaurant reservation
under your name, a variable is a named reservation of a small part of your computer’s memory.

The syntax to assign a variable is so: `item NAME: VALUE`. To assign the number 42 to a variable called age, we would write:

```
item age: 42
```

And just like numbers and strings, we can print a variable with the `show()` function. In the following example, we assign a few
variables and then print them. Here we are also adding strings together - this combines or **concatenates** the two strings.
We have to convert the `age` variable to a string to be able to add it to another string, so we use the `as_string` function
in the integer. You can learn more about this in the documentation.

```
item age: 42
item name: "Joe"

show("Hello, my name is " + name)
show("My age is " + age.as_string())
```

That's it for this tutorial! Check out the documentation if you want to know more.

## Documentation
Coming soon!