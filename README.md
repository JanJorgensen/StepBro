# StepBro

StepBro is a .net based automation framework especially for:
 * automated system testing (both desktop and embedded system testing),
 * application scripting and
 * workbench instrument automation

The inspiration sources are C#, CS-Script, TTCN-3, python, Robot framework and SeqZap.

StepBro is both a language definition and a runtime system. 
Scripts can be opened and executed both with the __stepbro.exe__ console application when needing a minimal solution, and the __StepBro Workbench__, which is an integreted development/test environment that gives a highly interactive experience.

## The StepBro script language
The StepBro language is not a general purpose programming language, but a specialized language for automation.
Some of the characteristics are:
 * Procedure centralized 
   - A procedure can inherit from another procedure
   - Procedures can have associated "helper procedures", much like a class in other languages can have class functions/methods.
   - A procedure can be defined to be an extension procedure (like C# extension methods).
 * Can use .net types defined in the .net framework and in custom (or 3rd party) assemblies.
 * Expressions have almost the same syntax as in C#.
 * Numeric constant values can be specified with SI metric prefixes (as 12K, 500m)
 * Timespan values can easily be specified as e.g. 1500ms, 0:10.500, 50s or 199:23:59:59

### A StepBro "Hello World" script

```
/* My first StepBro script */

procedure void main()
{
    log ("Hello World!");
} 
```

## What's special about StepBro

### Special Language Features

#### Procedures

The content of procedures are very much like the content of methods in C# and other languages, but with a few additions.

 * alt and interleave statements (event handling much like in TTCN-3)
 * Await statements
 * Expect statements
 * step statements, to divide a procedure into logical high-level steps
 * Log and report statements used to document the results and actions during execution.

#### Test Lists

A _Test List_ is like an array of procedure references and test list references. The (used define test suites and test cases)

#### Interactive interface objects 

Facilitated by the runtime system, like:
   - Communication ports (serial, CAN, etc.)
   - Data acquisition interfaces
   - Data storages
   - UI automation interfaces (WEB, desktop, embedded)

#### Data Tables

(a more readable table than data in normal source code)
TODO: add better description

#### Configuration Properties

In StepBro, _Cofiguration Properties_ are like constants, but with a few added features:
 * values can be overwritten locally by the user
 * values can be changed at initialization by a selected plugin
 * values can be changed through command line arguments

### StepBro Runtime System

StepBro compiles the scripts into binary code using LINQ expressions, and not into .net assemblies. That means StepBro does not have to deal with application domains, and can re-build changed scripts without drawbacks.
