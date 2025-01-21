using StepBro.Core.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestModule;

public class TestClassWithTextCommands : ITextCommandInput
{
    public string LastCommand { get; set; } = "";
    public long IntProp { get; set; } = 0L;

    bool ITextCommandInput.Enabled
    {
        get { return true; }
    }

    bool ITextCommandInput.AcceptingCommands()
    {
        return true;
    }

    void ITextCommandInput.ExecuteCommand(string command)
    {
        this.LastCommand = command;
    }
}
