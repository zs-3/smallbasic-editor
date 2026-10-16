# Small Basic Language Extensions Documentation

This repository contains modern, powerful extensions to the Small Basic compiler and runtime. Below is a comprehensive guide to all newly added language features, control flow constructs, file module imports, and .NET interop libraries.

---

## 1. Functions with Parameters and Return Statements

Small Basic now supports standard parameter-passing subroutines and functions with return values.

### Keywords
- `Function` or `Fn`
- `EndFunction` or `EndFn`
- `Return`

### Syntax & Examples

```smallbasic
Function Add(a, b)
    Return a + b
EndFunction

sum = Add(15, 25) ' sum = 40
```

Recursive functions are fully supported:

```smallbasic
Fn Factorial(n)
    If n <= 1 Then
        Return 1
    EndIf
    Return n * Factorial(n - 1)
EndFn

result = Factorial(5) ' result = 120
```

---

## 2. File Module Import System (`Import`)

Organize Small Basic code into modular files using relative or absolute path imports.

### Syntax
```smallbasic
Import "helpers/math_utils.sb"

res = Square(6)
```

Imports are recursively resolved relative to the current file's directory path. Cyclic imports are automatically detected and safely skipped.

---

## 3. Control Flow Constructs

### A. `Select Case` Statements
Replace long `If/ElseIf` chains with clean pattern matching:

```smallbasic
val = 2

Select Case val
    Case 1
        TextWindow.WriteLine("One")
    Case 2
        TextWindow.WriteLine("Two")
    Else
        TextWindow.WriteLine("Other")
EndSelect
```

### B. `Try / Catch / Finally` Error Handling
Handle errors gracefully without terminating execution:

```smallbasic
Try
    res = File.ReadContents("data.txt")
Catch ex
    res = "Default Content"
Finally
    TextWindow.WriteLine("Completed read attempt.")
EndTry
```

### C. `ForEach` Loops
Iterate over collections easily:

```smallbasic
ForEach item In list
    TextWindow.WriteLine(item)
EndFor
```

---

## 4. .NET / C# Reflection Interop (`System` Library)

Directly invoke static methods and read static properties from any available .NET type directly in Small Basic code.

### Methods
- `System.Call(typeName, methodName, argument)`
- `System.GetProperty(typeName, propertyName)`
- `System.Exists(filePath)`

### Example Usage

```smallbasic
' Call System.Math.Abs
absVal = System.Call("System.Math", "Abs", -42) ' returns 42

' Access .NET System.Environment property
os = System.GetProperty("System.Environment", "OSVersion")
```

---

## 5. Extended Math Utilities (`Math` Library)

- `Math.Clamp(value, min, max)`: Clamps `value` between `min` and `max`.
- `Math.Lerp(start, end, amount)`: Performs linear interpolation between `start` and `end`.
- `Math.Atan2(y, x)`: Returns the 2-argument arctangent of `y` and `x`.

---

## Summary of All Added Keywords
`Function`, `Fn`, `EndFunction`, `EndFn`, `Return`, `Import`, `Select`, `Case`, `EndSelect`, `Try`, `Catch`, `Finally`, `EndTry`, `ForEach`, `In`.
