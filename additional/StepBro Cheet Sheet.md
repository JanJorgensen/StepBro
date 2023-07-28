# Introduction

Most of the StepBro scripting language syntax is identical to C# syntax.

All .net types (almost) can be used in StepBro. New .net types cannot be defined directly in StepBro script files.

## Hello World example

```C
/*
This is a
multi-line
comment.
*/
procedure void main()
{
    // Send the greeting. (single-line comment)
    log ("Hello World!");
}
```

# Data Types

Built-in data types:

| **Type** | **Description** | **Examples** | **.net type** |
| -------- | --------------- | ----------- | ------------- |
| bool | Boolean value. | true<br>false | System.Bool |
| int, integer | 64-bit signed integer value. | 2332<br>-10K | System.Int64 |
| decimal, double | Floating point decimal value. | 3.1416<br>24m<br>5.2e9 | System.Double |
| string | Text, as a sequence of UTF-16 characters. | "Birdie!"<br>"c:\\temp" | System.String |
| verdict | Enumeration of test verdict values. | pass<br>fail | StepBro.Core.Data.Verdict |
| datetime | Represents an instant in time, expressed as a date and time of day.| @2023-07-27 09:25:00 UTC | System.DateTime |
| timespan | Represents a time interval. | 10s<br>@0:02.400 | System.TimeSpan |

## Using .net types

StepBro can use all .net data types, by using their full type name.

Example: ```System.Globalization.CultureInfo```

## Verdict values

**Value** | **Description**
--------- | ---------------
unset / Verdict.Unset | The execution has no verdict.
pass / Verdict.Pass | All test expectations were fulfilled.
inconclusive / Verdict.Inconclusive| A pass/fail test result could not be determined.
fail / Verdict.Fail | One or more expectations in the test was not fulfilled.
Verdict.Abandoned | Indicates that the user chose to abandon/break a running test.
error / Verdict.Error | A fatal error in the script execution.

## Numeric value format



### SI Prefixes

The supported SI prefixes, used for numeric values, are:

| P | T | G | M | K | m | u | n | p |
|
| Peta | Tera | Giga  | Mega | Kilo | milli | micro | nano | pico |
| 1e15 | 1e12 | 1e9 | 1e6 | 1e3 | 1e-3 | 1e-6 | 1e-9 | 1e-12 |

Examples:
60K, 1.21G, 16.6m

## Timespan value format

Supported timespan value formats are supported:

**Format** | **Description** | **Example**
---------- | --------------- | -----------
xs | Integer or decimal number of seconds | 14.4s
xps | Integer or decimal number of SI-prefixed seconds. <br>x: numeric value <br>p: SI prefix <br>s: 's' for 'seconds'. | 4ms<br>22.8ns
@m:ss | Minutes and seconds | @8:30
@m:ss.f | Minutes, seconds and fraction-of-a-second | @0:03.218
@h:mm:ss | Hours, minutes and seconds | @2:15:00
@h:mm:ss.f | Hours, minutes, seconds and fraction-of-a-second | @7:24:56.62

## Type Definition

```
type MySpecialConnection : SerialTestConnection;        // Create a new type that is based directly on another type.
```

# Aritmetic operators

**Operator** | **Examples** | **Description**
------------ | ------------ | ---------------
| + | a + b<br>"Number: " + value | Addition |
| - | a - b | Subtraction, multiplication, division  |
| * | a * b<br>"Go" * 16 | multiplication |
| / | a / b | division |
| % | a % b | modulus |

# Comparison operators

**Operator** | **Examples** | **Description**
------------ | ------------ | ---------------
| == | a == b | Equal to |
| != | a != b | Not equal to |
| > | a > b | Greater than |
| < | a < b | Less than |
| >= | a >= b | Greater than or equal to |
| \<= | a \<= b | Less than or equal to |
| ~= | a ~= b | Approximately equal to |
| ± | a == b ± c | Equal to within tolerance |
| ~=, ± | a ~= b ± c | Approximately equal to within tolerance |
| | a \< b \< c | Is within range, limits not included |
| | a \<= b \<= c | Is within range, limits included |
| | a \<~ b \<~ c | Is within range, approximate limits included |


# Logical operators

**Operator** | **Examples** | **Description**
------------ | ------------ | ---------------
| && | a && b | Logical AND |
| \|\| | a \|\| b | Logical OR |
| ! | !a | Logical NOT |

# Binary operators

**Operator** | **Examples** | **Description**
------------ | ------------ | ---------------
| & | a + b | Binary AND |
| \| | a \| b | Binary OR |
| ! | !a | Binary NOT |
| << | a << b | Shift a left b bits |
| >> | a >> b | Shift a right b bits |

# Usings

```
using "CalendarTests.sbs";
using @"..\..\library\Algorithms.sbs";
using StepBro.Streams;
using StringList = System.Collections.Generics.List<string>;
```

# Variables

```
int myInteger = 420;
double myDecimal = 582.3;
bool myBoolean = true;
string myString = "I didn't do it!";

SerialPort port = SerialPort("COM4", 115200)
{
    Handshake: Handshake.None,
    StopBits: StopBits.One
}

var a = 15;
var b = "Stanley";
var c = false;
```

# Procedures

```
procedure int CountStuff( bool includeHiddenElements ) { ... }
void SendMessage( string receiver, string title, string message ) { ... }       // Also a 'procedure'.
function decimal CalculateSomething( decimal height, decimal speed ) { ... }    // Has no logging or verdict.
public void ProcA() { ... }                                                     // Explicitely statint that it is public.
private void ProcB() { ... }                                                    // Private; not visible from other files.
void SomeTest() : TestBaseProcedure { ... }                                     // Inheriting stuff from another procedure.
void Send( this SerialPort port, string text ) { ... }                          // Extension procedure for SerialPort objects.
```

## Logging

```
log ("Some extra log information.");
log error ("Something is very wrong!");
log warning ("Operation took longer time than expected.");
```

## Sub steps

```
step;
step 14;
step "Warming up";
step 6, "Inspecting received data";
```

## Testing and reporting

```
expect ( value < 60.0 );                            // Verdict 'pass' if fulfilled, otherwise 'fail'.
expect "Temperature Low Limit" ( temp > 4.5 );      // Added name/tag for the test.
assert ( value < 400.0 );                           // Verdict 'error' if not fulfilled.
```

## Control flow statements

```
if ( x > 5 )
{
}
else
{
}

while ( i < 10 )
{
    if (x < 0) continue;
    if (y > 6000) break;
}
```

### While loop attributes
```
while ( i < 10K ) :
    Title: "Awaiting the thing to happen"       // Added to the log and shown in UI while looping.
    Timeout: 30s,                               // Looping breaks after time has elapsed.
    Break: "I saw it!",                         // User option for manually breaking loop.
    Break: "Error",                             // Another break option.
    CountVar: i                                 // Variable thta counts the loop iterations.
{
}
```


# Test Lists

```
testlist allTests : BaseTestList, timeout: 10s, user: "Anders"
{
    * SmokeTest
    * CalendarTest
    * CalculatorTest
    * CommunicationTest( repetitions: 100, timeout: 20s )
}
```

# Partners

```
void DoSomehing() : BaseProcedure,
    partner OperationA: ProcedureAX,            // Define new partner procedure.
    partner override OperationB: ProcedureB3    // Redirect inherited partner to another procedure.
{
}
```

# File Element Override

```
override connection                         // Override data for a script variable.
{
    BaudRate: 460800
}
override connection as TargetConnection;    // Override type for a script variable.
```

# Test Framework
The file *scripts\TestFramework.sbs* contains a general framework for test setups. Most important elements are:
* **TestCase**, a base procedure for all test cases / test procedures.
* **TestSuite**, a base test list for test suites.

```
using "TestFramework.sbs";

testlist AllTests : TestSuite
{
    * TestIntegerExpressions
    * TestBooleanExpressions
}

public procedure void TestIntegerExpressions() : TestCase
{
    -
    -
}
public procedure void TestBooleanExpressions() : TestCase
{
    -
    -
}
```
