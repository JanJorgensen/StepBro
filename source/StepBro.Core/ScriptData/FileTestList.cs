using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            private readonly string m_title;
            private readonly ArgumentList m_arguments;
            public EntryBase(FileTestList home, TestListEntryType type, string reference, string title, ArgumentList arguments = null)
            {
                m_home = home;
                m_type = type;
                m_reference = reference;
                m_title = title;
                m_arguments = (arguments != null) ? new ArgumentList(home.m_arguments, arguments) : new ArgumentList(home.m_arguments);
            }

            public ITestList Home { get { return m_home; } }
            public virtual IFileElement Reference { get { throw new NotSupportedException("No reference set;"); } }

            public string ReferenceName { get { return m_reference; } }
            public string Title { get { return m_title; } }

            public TestListEntryType Type { get { return m_type; } }

            public ArgumentList Arguments
            {
                get
                {
                    return m_arguments;
                }
            }
        }

        private class EntryTestCase : EntryBase, ITestListEntryTestCase
        {
            private IProcedureReference m_procedure;

            public EntryTestCase(FileTestList home, string referenceName, string title, IProcedureReference procedure, ArgumentList arguments = null) : 
                base(home, TestListEntryType.TestCase, referenceName, title, arguments)
            {
                m_procedure = procedure;
            }

            public override IFileElement Reference { get { return m_procedure.ProcedureData; } }
            public IProcedureReference ProcedureReference { get { return m_procedure; } }
        }

        private class EntryTestList : EntryBase, ITestListEntryTestList
        {
            private readonly ITestList m_list;
            public EntryTestList(FileTestList home, string referenceName, ITestList list) : base(home, TestListEntryType.TestList, referenceName, referenceName)
            {
                m_list = list;
            }

            public override IFileElement Reference { get { return m_list; } }
            public ITestList TestListReference { get { return m_list; } }
        }

        private ArgumentList m_arguments = null;
        private List<ITestListEntry> m_entries = new List<ITestListEntry>();

        public FileTestList(IScriptFile file, AccessModifier access, int line, IFileElement parentElement, string @namespace, string name) :
            base(file, line, parentElement, @namespace, name, access, FileElementType.TestList)
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

        public ITestListEntry AddTestCase(string name, IProcedureReference procedure, ArgumentList arguments)
        {
            var entry = new EntryTestCase(this, name, name, procedure, arguments);
            m_entries.Add(entry);
            return entry;
        }

        public void AddTestEntry(string target)
        {
            m_entries.Add(new EntryBase(this, TestListEntryType.Unresolved, target, target));
        }

        public ITestListEntry AddTestList(string name, ITestList list)
        {
            var entry = new EntryTestList(this, name, list);
            m_entries.Add(entry);
            return entry;
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
            private string m_title = null;
            private IProcedureReference m_currentProcedure = null;
            private ArgumentList m_currentProcedureArguments = null;

            public NormalIterator(ITestList list)
            {
                if (list == null) throw new ArgumentNullException("list");
                m_list = list;
            }

            public string Title
            {
                get
                {
                    return m_title;
                }
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

            private static void GetTestCases(string path, ITestList list, List<ITestListEntryTestCase> collected)
            {
                foreach (var entry in list.ListEntries())
                {
                    if (entry.Type == TestListEntryType.TestCase)
                    {
                        var newEntry = new EntryTestCase(
                            entry.Home as FileTestList, 
                            entry.ReferenceName, 
                            path + CreateTitle((entry as EntryTestCase).ProcedureReference, entry.Arguments), 
                            (entry as EntryTestCase).ProcedureReference, 
                            entry.Arguments);
                        collected.Add(newEntry);
                    }
                    else if (entry.Type == TestListEntryType.TestList)
                    {
                        GetTestCases(entry.ReferenceName + " - ", (entry as ITestListEntryTestList).TestListReference, collected);
                    }
                }
            }

            private static string CreateTitle(IProcedureReference procedure, ArgumentList arguments)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(procedure.FullName);
                if (arguments != null)
                {
                    var list = arguments.Select(a => (a.Name + ": " + StringUtils.ObjectToString(a.Value))).ToList();
                    if (list.Count > 0)
                    {
                        sb.Append("( ");
                        sb.Append(String.Join(", ", list));
                        sb.Append(" )");
                    }
                }
                return sb.ToString();
            }


            public bool GetNext()
            {
                if (m_testCases == null)
                {
                    m_testCases = new List<ITestListEntryTestCase>();
                    GetTestCases(m_list.Name + " - ", m_list, m_testCases);
                }

                if (m_index < (m_testCases.Count - 1))
                {
                    m_index++;
                    m_title = m_testCases[m_index].Title;
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
