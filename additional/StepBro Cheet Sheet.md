# Introduction

Most of the StepBro scripting language syntax is identical to C# syntax.

All (almost) .net types can be used in StepBro, though some cannot be defined in StepBro and therefore have to be defined in plugin code modules (.net assemblies).
## Hello World example

```
procedure void main()
{
    log ("Hello World!");
}
```

# Data Types

Build-in data types:

**Type** | **Description** | **Example** | **.net type**
-------- | --------------- | ----------- | -------------
bool | Boolean value. | true, false | System.Bool
int, integer | 64-bit signed integer value. | 2332, -10K | System.Int64
decimal, double | Floating point decimal value. | 3.1416, 24m, 5.2E9 | System.Double
string | Text, as a sequence of UTF-16 characters. | "Birdie again!", "c:\\temp" | System.String
verdict | Enumeration of test verdict values. | pass, fail | StepBro.Core.Data.Verdict
datetime | Represents an instant in time, expressed as a date and time of day.| @2016-11-23 | System.DateTime
timespan | Represents a time interval. | 10s, @0:02.400 | System.TimeSpan

## Using .net types

StepBro can use all .net data types, by using their full type name.

Example: ```System.Globalization.CultureInfo```

## Verdict values

**Value** | **Description**
--------- | ---------------
unset / Verdict.Unset | The execution has no verdict.
pass / Verdict.Pass | All test expectations were fulfilled.
inconclusive / Verdict.Inconclusive| A pass/fail test result could be determined.
fail / Verdict.Fail | One or more expectations in the test was not fulfilled.
Verdict.Abandoned | Indicates that the user chose to abandon/break a running test.
error / Verdict.Error | A fatal error in the script execution.

## Numeric value format



### SI Prefixes

The supported SI prefixes, used for numeric values, are:

| P | T | G | M | K | m | u | n | p |
|
| Penta | Tera | Giga  | Mega | Kilo | milli | micro | nano | pico |
| 1e15 | 1e12 | 1e9 | 1e6 | 1e3 | 1e-3 | 1e-6 | 1e-9 | 1e-12 |

Examples:
60K, 1.21G, 16.6m

## Timespan value format

Supported timespan value formats are supported:

**Format** | **Description** | **Example**
---------- | --------------- | -----------
ns | Integer or decimal number of seconds | 14.4s
nPs | Integer or decimal number of SI-prefixed seconds | 4ms, 22.8ns
@m:ss | Minutes and seconds | @8:30
@m:ss.f | Minutes, seconds and fraction-of-a-second | @0:03.218
@h:mm:ss | Hours, minutes and seconds | @2:15:00
@h:mm:ss.f | Hours, minutes, seconds and fraction-of-a-second | @7:24:56.62

# Variables

```
int myInteger = 420;
double myDecimal = 582.3;
bool myBoolean = true;
string myString = "I didn't do it!";

SerialPort port = SerialPort("COM4, "115200); 

var a = 15;
var b = "Stanley";
var c = false;
```

# Procedures

# Test Lists

```
testlist allTests
{
    * SmokeTest
    * CalendarTest
    * CalculatorTest
    * CommunicationTest( repetitions: 100, timeout: 20s )
}
```
