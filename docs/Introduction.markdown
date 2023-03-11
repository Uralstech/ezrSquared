---
layout: default
title: "Introduction and Documentation"
nav_order: 2
---

# An Introduction to ezr²
{: .no_toc }

<details open markdown="block">
  <summary>
    Table of contents
  </summary>
  {: .text-delta }
- TOC
{:toc}
</details>

## Why Learn ezr²?
ezr² is a programming language that's **easy to learn** and **practical to use**.
ezr² can be learnt by anyone, **of any age**, in a few minutes. Anyone can extend
the functionalities of ezr² with **libraries**. If you already know C#, you can even
help in ezr² development with **C# Assisted ezr² Libraries** (**CSAELs**)! CSAELs bring
the existing functionality of C# to ezr²! Experienced ezr² programmers can even ditch the
boilerplate syntax for the shorter **QuickSyntax**. The normal syntax satisfies the beginner,
as it is easy to use and QuickSyntax satisfies the expert, as it is very short.

## Documentation
Start coding with the help of ezr²'s official documentation!
(Heavily inspired by [***this Python tutorial***](https://python.land/python-tutorial))

### Hello World, Displaying Text to The Screen and Strings
There's a tradition in which programming tutorials start with a so-called **Hello World** program.
A Hello World program simply prints the words "Hello world" to the screen. The `show()`
function is used to do so. You will learn more about functions later.

```
show("Hello, World!")
```

The `show()` function takes anything you put between the parentheses and prints it to the screen.
But you must feed it the correct type of data for it to work. For example, text in ezr² is always put between **double quotes**.
In the world of computer programming, this is called a **string**.

Double quotes around strings are essential because they precisely **mark the start and end** of a string.
This way, a string is easy to recognize for ezr². Here are a few more examples of valid strings:

```
"Hello world"
"My name is Uday"
"This one is a bit longer. There is no limit to how long a string can be!"
```

You also might have noticed that after printing "Hello, World!" to the screen, the `show()` function also printed "nothing".
This means **the function returned nothing**. If you code in `"Example"` and enter, only `"Example"` will be printed to the screen.
This means **the expression `"Example"` returned the string "Example"** - ezr² is only showing that to the screen. `nothing` is a representation
of, well, nothing - equivalent to null (C#, C, Java, etc) or none (Python).

### Numbers
Now that you've seen strings let's move on to **numbers**. Just like strings,
you can ask ezr² to print numbers using the `show()` function.
Unlike strings, numbers **don't need quotes around them**.
So, to print the number 10, use this:

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

### Floating-point Numbers
While playing around with the division operator, you might have gotten some
rounded off results - for example `3 / 2` returns `1` instead of `1.5` which is correct.
In computer programming, there's a strong distinction between non-fractional numbers like
1, 3, and 42 and fractional numbers like 3.14. The former are called **integers**,
while the latter are called **floats**. Now try this:

```
show(3 / 2.0)
```

This should display `1.5` to the screen, because you have specified that `2.0` is a **float**.
**An integer divided by another integer will always return an integer, but the same done with a float
will always return a float.** This rule is the same for all operators.

### Variables
Wouldn't it be nice to store the results of the calculations you made above? For this, you use **variables**.
**Variables allow you to store things in memory** for as long as the ezr² program runs. Like a restaurant reservation
under your name, a variable is a named reservation of a small part of your computer's memory.

The syntax to assign a variable is so: `item NAME: VALUE`. To assign the number 42 to a variable called age, you would write:

```
item age: 42
```

And just like numbers and strings, you can print a variable with the `show()` function. In the following example, you assign a few
variables and then print them. Here you are also adding strings together - this combines or **concatenates** the two strings.
You have to convert the `age` variable to a string to be able to add it to another string, so you use the `as_string` function
in the integer.

```
item age: 42
item name: "Joe"

show("Hello, my name is " + name)
show("My age is " + age.as_string())
```

### User Input
Let's make a simple adder - the user will enter two numbers, and the adder will return the result of adding them together.
To do so you need to get the user's input. You can use the `get()` function for that. You have to feed it the message to show to the user.
You can enter `nothing` if you don't want any message with the input request.

The code should look something like this:
```
item num1: get("Enter num 1: ")
item num2: get("Enter num 2: ")

show(num1 + num2)
```

If you try the code, you will see that it's not adding the numbers, but concatenating them! As in, if you enter `2` and `4` as `num1`
and `num2` respectively, the result will be `24`, not `6`.

**This is because the `get()` function returns a string.** You need to convert the string into an integer. To do so you use the `as_integer()` function
in the string. You'll immediately convert the results of the `get()` functions to integers. Try the below code:
```
item num1: get("Enter num 1: ").as_integer()
item num2: get("Enter num 2: ").as_integer()

show(num1 + num2)
```

### Conditions
Try entering normal text as the input for the above code - you'll get an error! This is because the `as_integer()` function cannot convert
normal text to integers. To avoid the user seeing the messy error, you can use the `try_as_integer()` function. But, this function returns `nothing`
if it is unable to convert the string to an integer! If you try to add `nothing` and a number together you'll get an error! You have to make sure
that `num1` and `num2` are integers before you add them. You can do this with **if expressions**. The if expression has a body of code that only executes
whenever the if statement's condition is met. The additional else statement, which also has a body of code, runs if the if
condition is false. The else if statement is used when you wish to satisfy one statement while the other is false.

```
item num1: get("Enter num 1: ").try_as_integer()
item num2: get("Enter num 2: ").try_as_integer()

if num1 = nothing do
	show("Num 1 is invalid!")
else if num2 = nothing do
	show("Num 2 is invalid!")
else do
	show(num1 + num2)
end
```

Here are all the comparison operators:

| Operator |         Operation        |                       Example                       |
|----------|:------------------------:|----------------------------------------------------:|
|    =     | equal to                 |  "i" = "i", "i" = "j"                               |
|    !     | not equal to             |  "i" ! "i", "i" ! "j"                               |
|    >     | greater than             |  5 > 8, 9 > 4                                       |
|    <     | less than                |  5 < 8, 9 < 4                                       |
|    >=    | greater than or equal to |  5 >= 5, 5 >= 2, 5 >= 8                             |
|    \<=   | less than or equal to    |  5 \<= 5, 5 \<= 2, 5 \<= 8                          |
|    or    | and                      |  3 = 2 and 2 > 1, 2 = 5 and 5 = 2, 2 = 2 and 2 > 1  |
|    and   | or                       |  3 = 2 or 2 > 1, 2 = 5 or 5 = 2, 2 = 2 or 2 > 1     |

If you use comparisons outside if expressions they will return **booleans**. Booleans are just two values - **true** or **false**.

### One-liners
You might have noticed that the if expression has the **end** keyword at the end. All multi-line ezr² expressions **must** end with the
**end** keyword. Noticed that I said "All multi-line ezr² expressions"? You can write all these in one line - these are called **one-liners**.
They are the same as writing the multi-line version of an expression, but they must fit in a single line. For example -
```
if get("Hello there! ") = "General Kenobi" do show("Nice")
```

Note that new lines can be coded as the semicolon (`;`) symbol. Comments are signified by the at symbol (`@`) at the start. Comments are just more
information about the written code - like how the code works. They do not affect the execution of any code and are ignored by ezr².

### Loops (Part 1)
For the above scripts, you might have found it annoying to have to copy and paste the code again and again to try it out. What if
you want the code to run forever? Or even a set number of times? Do you have to keep copying and pasting it? No! You'll use **loops** for that.
**A loop keeps executing the given body of code till a condition is satisfied.** ezr² has two types of loops - **count loops** and **while loops**.
**Count loops** repeat the given code a **set number of times**.
```
count to 10 do
	show("Hello, World!")
end
```

The count loop can optionally keep account of the **iteration variable**. The iteration variable, starting at zero, is the value that gets incremented each loop or **iteration**.
The count loop checks if the iteration variable is less than the max iterations - if not it stops the loop. Iteration variables are useful if you want to iterate
over a **list** or **array**, using the variable as the index.
```
count to 10 as i do
	show("The iteration variable, named 'i' is: " + i.as_string())
end
```

Now, what if you don't want the loop / iteration variable to start at zero? You can also set the start of a count loop!
```
count from -5 to 5 as i do
	show("The iteration variable, named 'i' is: " + i.as_string())
end
```

Lastly, what if you want the iteration variable to skip a few numbers? Say you only want even numbers? You can set the **step** of the count loop!
The step is what the loop adds to the iteration variable every iteration.
```
count to 10 step 2 as i do
	show("The iteration variable, named 'i' is: " + i.as_string())
end

count from 1 to 10 step 2 as i do
	show("The iteration variable, named 'i' is: " + i.as_string())
end
```

### Arrays
If you try the one-liner version of the count loop, like -
```
count to 10 do show("Hi!")
```
You'll see a whole lot of `nothing`s in parentheses, separated by commas! That's an **array**.

Arrays are used to store multiple items of any type in a single variable. It is ordered and unchangeable, or **immutable**.
When you say that arrays are ordered, it means that the items have a defined order, and that order will not change.
Array items are indexed, the first item has index `0`, the second item has index `1` and so on. Items of an array can be
accessed with the `<=` operator -
```
item array_example: (1, "string", 3.54, nothing, false)
show(array_example <= 0)
show(array_example <= 1)
show(array_example <= 2)
show(array_example <= 3)
show(array_example <= 4)

show(array_example <= "sd")
```
That last line of code should show an error - the item after the `<=` operator must be a valid index in the array!

Here are all the array operators:

| Operator |      Name      |    Example    |
|----------|:--------------:|--------------:|
|     *    | duplication    | (2,3) * 4     |
|     /    | division       | (6,2,3,4) / 2 |
|    \<=   | item access    | (6,2) <= 1    |

Now, what if you want to create an array with one item? Let's try it -
```
item array_example: (3)
show(array_example)
```

This won't work, as ezr² thinks `(3)` is part of an operation - not an array. So, you have to have a comma after the first item!
```
item array_example: (3,)
show(array_example)
```

You can access the length of the array with the `length` variable built into the array -
```
item array_example: (1,2,3,4)
show(array_example.length)
```

### Lists
**Lists** are like arrays, but they are changeable or **mutable**. They are created with square brackets, instead of parentheses.
```
item list_example: [1,"string",1.45,nothing,true]
show(list_example)
```

Here are all the list operators:

| Operator |      Name      |            Example           |
|----------|:--------------:|-----------------------------:|
|     +    | append         | item lst: [1,5]; lst + 4     |
|     -    | remove         | item lst: [1,5]; lst - 1     |
|     *    | duplication    | [2,3] * 4                    |
|     /    | division       | [6,2,3,4] / 2                |
|    \<=   | item access    | [6,2] <= 1                   |

You can access the length of the list with the `length` variable built into the list, just like with arrays.

### Loops (Part 2)
**While loops** repeat the given code **till the given condition turns false**.
```
item password: get("Enter password: ")
while password ! "yeet" do
	item password: get("Password is false! Try again: ")
end
```

While loops can also be used to run code infinitely!
```
while true do
	show("Hello, World!")
end
```

That's it for this tutorial! More coming soon!