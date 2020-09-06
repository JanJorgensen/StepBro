using System;
using System.Collections.Generic;
using StepBro.Core.Data;
using StepBro.Core.Execution;

namespace StepBro.Core.ScriptData
{
    internal class FileTestList : FileElement, ITestList
    {
        private class EntryBase : ITestListEntry
        {
            private readonly FileTestList m_home;
            private readonly TestListEntryType m_type;
            private readonly string m_reference;
            private readonly ArgumentList m_arguments;
            public EntryBase(FileTestList home, TestListEntryType type, string reference)
            {
                m_home = home;
                m_type = type;
                m_reference = reference;
                m_arguments = new ArgumentList(home.m_arguments);
            }

            public ITestList Home { get { return m_home; } }
            public virtual IFileElement Reference { get { throw new NotSupportedException("No reference set;"); } }

            public string ReferenceName { get { return m_reference; } }

            public TestListEntryType Type { get { return m_type; } }

            public ArgumentList Arguments { get { return m_arguments; } }
        }

        private class EntryTestCase : EntryBase, ITestListEntryTestCase
        {
            private IProcedureReference m_procedure;

            public EntryTestCase(FileTestList home, string referenceName, IProcedureReference procedure) : base(home, TestListEntryType.TestCase, referenceName)
            {
                m_procedure = procedure;
            }

            public override IFileElement Reference { get { return m_procedure.ProcedureData; } }
            public IProcedureReference ProcedureReference { get { return m_procedure; } }
        }

        private class EntryTestList : EntryBase, ITestListEntryTestList
        {
            private readonly ITestList m_list;
            public EntryTestList(FileTestList home, string referenceName, ITestList list) : base(home, TestListEntryType.TestList, referenceName)
            {
                m_list = list;
            }

            public override IFileElement Reference { get { return m_list; } }
            public ITestList TestListReference { get { return m_list; } }
        }

        private ArgumentList m_arguments = null;
        private List<ITestListEntry> m_entries = new List<ITestListEntry>();

        public FileTestList(IScriptFile file, int line, IFileElement parentElement, string @namespace, string name) :
            base(file, line, parentElement, @namespace, name, FileElementType.TestList)
        {
        }

        public ITestListEntry this[int index]
        {
            get
            {
                return m_entries[index];
            }
        }

        public int EntryCount { get { return m_entries.Count; } }

        public IEnumerable<ITestListEntry> ListEntries()
        {
            foreach (var e in m_entries)
            {
                yield return e;
            }
        }

        protected override TypeReference GetDataType()
        {
            return new TypeReference(typeof(ITestList), this);
        }

        public void AddTestCase(string name, IProcedureReference procedure)
        {
            m_entries.Add(new EntryTestCase(this, name, procedure));
        }

        public void AddTestEntry(string target)
        {
            m_entries.Add(new EntryBase(this, TestListEntryType.Unresolved, target));
        }

        public void AddTestList(string name, ITestList list)
        {
            m_entries.Add(new EntryTestList(this, name, list));
        }

        public ITestListIterator GetProcedureIterator(/*bool loop = false, Predicate<IProcedureReference> filter = null*/)
        {
            return new NormalIterator(this);
        }

        private class NormalIterator : ITestListIterator
        {
            private readonly ITestList m_list;
            private int m_index = -1;
            private List<ITestListEntryTestCase> m_testCases = null;
            private IProcedureReference m_currentProcedure = null;
            private ArgumentList m_currentProcedureArguments = null;

            public NormalIterator(ITestList list)
            {
                if (list == null) throw new ArgumentNullException("list");
                m_list = list;
            }

            public IProcedureReference Procedure
            {
                get
                {
                    return m_currentProcedure;
                }
            }

            public ArgumentList Arguments
            {
                get
                {
                    return m_currentProcedureArguments;
                }
            }

            public void Reset()
            {
                m_index = -1;
            }

            private static void GetTestCases(ITestList list, List<ITestListEntryTestCase> collected)
            {
                foreach (var entry in list.ListEntries())
                {
                    if (entry.Type == TestListEntryType.TestCase)
                    {
                        collected.Add(entry as ITestListEntryTestCase);
                    }
                    else if (entry.Type == TestListEntryType.TestList)
                    {
                        GetTestCases((entry as ITestListEntryTestList).TestListReference, collected);
                    }
                }
            }

            public bool GetNext()
            {
                if (m_testCases == null)
                {
                    m_testCases = new List<ITestListEntryTestCase>();
                    GetTestCases(m_list, m_testCases);
                }

                if (m_index < (m_testCases.Count - 1))
                {
                    m_index++;
                    m_currentProcedure = m_testCases[m_index].ProcedureReference;
                    m_currentProcedureArguments = m_testCases[m_index].Arguments;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
