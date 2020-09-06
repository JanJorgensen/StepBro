using stream;		// tool
using Kamstrup;		// tool
using #System.Text;	// .net namespace

config int XMAX = 20;	// Adornment shows if private value is set.
config bool UserPresent = true;	// Adornment checkbox for easy mouse access.
config file ConfigurationFile = "[this]\myConfiguration.xml";	// Adornment shows browse-button. Tooltip or gray text shows the full path.

// Single-selection group. Adornment radio button shown for easy mouse access. Parser warning if none selected or more than one.
config option [GroupName] OptionA = false;
config option [GroupName] OptionB = false;
config option [GroupName] OptionC = true;

config option OptionK = false;
config option OptionL = false;
config option OptionM = false;

config METER
{
	
}

tool kmp1 ();
tool kmp2 - Setup() 
	data {
		"anders": 15,
		"bent": "Hallo der!",
		"christian": 
	}

datatable MyTable : MyTableModel
{ 
	HasHeader, HasColumnNames, HasRowNames,
	column A: {Type: integer, Justify: left, DefaultValue: -1 }
}
 @| A   | B  | C      | D                 |
__| Abc | De | Fgh    | Ijk               |
  |  12 | 34 | Mogens | "Henrik andersen" |
//|   0 | 94 |    Jan | Frode             |
  | 659 |  7 |  Mette | "Leslie"          |
  |  19 |  4 | Dennis | ""                |
  |   - | 20 |   Erik | ""                |


procedure My Procedure ( integer a, string b, KMP kmp )
{
	var MyArray = {1,2,3,4};
	var i = 16;
	int j = i * 13;
	decimal d = 100m;
	int k = 100M;
	
	// Mogensen
	
	foreach ( int x in myArray )
	{
		i += x;
	}
	
	if (i > 8) i = 0;
	else if (i > 2)
	{
		i *= k;
	}
	
	
}


private procedure My Procedure ( integer a, string b, KMP kmp ) : MyModel,
	Description: "This is the descriptive description for the fine procedure.",
	partners: { Start: MyFineProcedure, "Run 10 Times": Proc10 },
{
}

private procedure My Procedure ( integer a, string b, KMP kmp ) :
	Description: "This is the descriptive description for the fine procedure.",
	Partners: 
	{ 
		override Start: MyFineProcedure, 
		"Run 10 Times": Proc10 
	},
	Model: MyModel,
	"Profile State": ""
{
}

/// <summary>
/// This is the descriptive description for the fine procedure.
/// </summary>
private procedure My Procedure ( integer a, string b, KMP kmp ) : Model4
[
	Partner("Start", MyFineProcedure), 
	Partner("Run 10 Times", Proc10),
	ProfileState = Mogens, Flag1, Flag2, Flag3,
	Custom("TargetFile", @"[this]\mydata.csv")
]
{
}