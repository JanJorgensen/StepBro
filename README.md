# StepBro

StepBro is a .NET7 based automation framework especially for:
 * automated system testing (both desktop and embedded system testing),
 * application scripting and
 * workbench instrument automation

The intension is to make it easy to write highly readable test scripts with as low overhead as possible.

The inspiration sources are C#, CS-Script, TTCN-3, python, Robot framework and SeqZap.

StepBro is both a language definition and a runtime system. 

Scripts can be opened and executed with the __stepbro.exe__ console application.
Plans are to create __Visual Studio Code__ extensions for editing, execution, debugging and more.
The (not finished yet) WPF application, __StepBro Workbench__, is an integreted development/test environment that gives a highly interactive experience.

When executing scripts, an execution log is automatically created without having to add any statements in the script procedures. This eases the script writing and makes the scripts shorter and easier to read and maintain. Additional data can easily be added to the log by inserting simple _log_ statements.


## The StepBro script language

```
/* My first StepBro script, not made to impress. */

procedure void main()
{
    log ("Hello World!");
} 
```

The StepBro language is not a general purpose programming language, but a specialized language for automation.
Some of the characteristics are:
 * Procedure centralized
   * Procedure inheritance can be used to define common behaviour in a procedure, and reused by other procedures.
   * A procedure can be defined to be an 'extension procedure' (like C# extension methods).
   * Procedures can have some associated "helper procedures", much like a class in other languages can have class functions/methods.
   * Procedures defined as 'function' can be used in expressions, to e.g. do calculations or generate data, without writing anything to the log.
   * Special procedure statements ease the script development:
     * _log_ and _report_ statements used to add additional documentation of results and actions during execution.
     * _expect_ statements, to set the script verdict from the expected behaviour of whats being tested.
     * _step_ statements, to divide a procedure into logical high-level steps.
     * _await_ statements, to make the script wait for an asynchronous response or event to occur.
 * Scripts can use .net components defined in the .net framework itself and in any custom (or 3rd party) class libraries.
 * Statement and expression syntax is almost identical to C# syntax.
 * Numeric constant values can be specified with SI metric prefixes (as 12K, 500m)
 * Timespan values can easily be specified as e.g. 50s, 1500ms, @0:10.500 or @199:23:59:59
 * File element named 'testlist' can be used to define test suites.

If interested, please have a look at the files in the examples/scripts folder.


## StepBro Runtime System

StepBro compiles the script files into binary code using LINQ expressions, and not into .net assemblies. Technically, that means StepBro does not have to deal with application domains, and can re-build changed scripts without drawbacks.

The compilation happens automatically when starting a script execution with the console application.
