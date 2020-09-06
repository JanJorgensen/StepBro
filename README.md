# StepBro

StepBro is a .net based automation framework especially for:
 * automated system testing (both desktop and embedded system testing),
 * application scripting and
 * workbench instrument automation

The inspiration sources are C#, CS-Script, TTCN-3, python, Robot framework and SeqZap.

StepBro is both a language definition and a runtime system. 

The user gets both the __stepbro.exe__ console application and the __StepBro Workbench__ integreted development/test environment.

## StepBro Script Language Definition
The StepBro language is not a general purpose language, but a specialized language for automation.
Some of the characteristics are:
 * Procedure centralized 
   - Procedures can inherit from another
   - Procedures can have "helper procedures", much like methods on a class in other languages. 
 * Classes cannot be defined (.net classes defined in used assemblies can be used in scripts)
 * Expressions have almost the same syntax as in C#.
 * Numeric constant values can be specified with SI metric prefixes (as 12K, 500m)
 * Timespan values can easily be specified as e.g. 1500ms, 0:10.500, 50s or 199:23:59:59

### Elements of a StepBro script file

 * Usings (dependencies to other script files and to .net assemblies)
 * Procedures
 * Test lists (used define test suites and test cases)
 * Interactive interface objects (facilitated by the runtime system), like:
   - Communication ports (serial, CAN, etc.)
   - Data acquisition interfaces
   - Data storages
   - UI automation interfaces (WEB, desktop, embedded)
 * Data tables (a more readable table than data in normal source code)
 * Configuration properties (constants, but configurable through plugins or command line arguments)

### Procedure Statements

The content of procedures are very much like the content of methods in C#, but with a few additions.

 * Variables definitions
 * Expression statements
 * Procedure calls statements
 * Control flow statements
   - if and else statements
   - while, do-while and for statements
   - switch-case statements
 * alt and interleave statements (event handling much like in TTCN-3)
 * Await statements
 * Expect statements
 * Log, report and step statements

## StepBro Runtime System

StepBro compiles the scripts into binary code using LINQ expressions, and not into .net assemblies. Procedure definitions are build into delegates using LINQ expressions, and all other file elements are build into .net objects managed by the runtime system.
That means StepBro does not have to deal with application domains, and can re-build scripts and unload older versions of the executable code when script files are changed in the host application or StepBro Workbench.
