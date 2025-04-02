using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StepBro.Core.Data;
using StepBro.Core.Execution;

namespace StepBro.Core.ScriptData
{
    public interface ITestList : IFileElement
    {
        int EntryCount { get; }
        IEnumerable<ITestListEntry> ListEntries();
        ITestListEntry this[int index] { get; }
        ITestListIterator GetProcedureIterator(/*bool loop = false, Predicate<IProcedureReference> filter = null*/);
    }

    public enum TestListEntryType
    {
        Unresolved,
        Unknown,
        TestCase,
        TestList
    }

    public interface ITestListEntry
    {
        ITestList Home { get; }
        IFileElement Reference { get; }
        string ReferenceName { get; }
        string Title { get; }
        TestListEntryType Type { get; }
        ArgumentList Arguments { get; }
    }

    public interface ITestListEntryTestList : ITestListEntry
    {
        ITestList TestListReference { get; }
    }

    public interface ITestListEntryTestCase : ITestListEntry
    {
        IProcedureReference ProcedureReference { get; }
    }

    public interface ITestListIterator
    {
        void Reset();
        bool GetNext();
        string Title { get; }
        IProcedureReference Procedure { get; }
        ArgumentList Arguments { get; }
    }
}
