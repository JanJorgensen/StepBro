









### Procedures

The content of procedures are very much like the content of methods in C# and other languages.
The most important procedure statements are:

 * _log_ and _report_ statements used to document the results and actions during execution.
 * _expect_ statements, to set the script verdict from the expected behaviour of whats being tested.
 * _step_ statements, to divide a procedure into logical high-level steps.
 * _await_ statements, to make the script wait for an asynchronous response or event to occur.


 #### Inheritance

A procedure can inherit properties and behaviour from another procedure.

If a test framework script defines a procedure named 'TestCase' with the general functionality of a test case,
a test case can be defined like this:

```
// Procedure being a test case.
procedure void TestEverythingIsFine() : TestCase
{
    string state = GetStateOfEverything();
    expect (state == "Good");
} 
```
#### Extension Procedures

A procedure can be defined to be an 'extension procedure' (like C# extension methods). 
The _this_ keyword is used on the first parameter, to indicate that the procedure should be used as if it was a method on an instance of that data type.
The name of that paramameter is 

```
procedure void RunProcedureTenTimes( this MyProcedureType proc )
{ 
    int i = 0;
    while (i < 10)
    {
        testcase();
        i++;
    }
}
```
The extension procedure is then used like this:
```
myProcedure.RunProcedureTenTimes();
```


#### Partner Procedures

Procedures can have some associated "helper procedures", much like a class in other languages can have class functions/methods.

```
procedure MyProcedure() :
    partner Setup:      SetupDevicesAndData,
    partner Loop:       LoopProcedureForever
{
}
```

### Test Lists

A _Test List_ is like an array of procedure references and test list references.

In this example, the test list 'AllTests' inherits from another testlist named 'TestSuite' with the general functionality of a test suite. This example test suite includes three test cases and another test suite. 
```
// A simple test suite.
testlist AllTests : TestSuite
{
    * BasicTest
    * AdvancedTest1
    * AdvancedTest2
    * UserInteractionTests
} 
```
