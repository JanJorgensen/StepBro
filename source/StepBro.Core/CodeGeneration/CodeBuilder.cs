using System;
using System.Text;

namespace StepBro.Core.CodeGeneration
{
    public class CodeBuilder
    {
        private StringBuilder m_Code;
        private int indent = 0;

        public CodeBuilder(StringBuilder code)
        {
            m_Code = code;
        }

        public CodeBuilder(StringBuilder code, int indent)
        {
            m_Code = code;
            this.indent = indent;
        }

        public int IndentLevel { get { return indent; } }
        public string Indention { get { return new string('\t', indent); } }

        public void SetIndentLevel(CodeBuilder other)
        {
            indent = other.indent;
        }

        public void S(string s)
        {
            m_Code.Append(s);
        }

        public void SF(string s, params object[] args)
        {
            m_Code.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, s, args);
        }

        public void I(string s)
        {
            m_Code.Append(Indention + s);
        }

        public void IL(string s)
        {
            m_Code.Append(Indention + s + "\r\n");
        }

        /// <summary>
        /// Does the same as IL, but with two newlines instead of one.
        /// </summary>
        public void IL2(string s)
        {
            m_Code.Append(Indention + s + "\r\n\r\n");
        }

        public void IFL(string s, params object[] args)
        {
            m_Code.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, Indention + s + "\r\n", args);
        }

        /// <summary>
        /// Does the same as IFL, but with two newlines instead of one.
        /// </summary>
        public void IFL2(string s, params object[] args)
        {
            m_Code.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, Indention + s + "\r\n\r\n", args);
        }

        public void FL(string s, params object[] args)
        {
            m_Code.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, s + "\r\n", args);
        }

        /// <summary>
        /// Does the same as FL, but with two newlines instead of one.
        /// </summary>
        public void FL2(string s, params object[] args)
        {
            m_Code.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, s + "\r\n\r\n", args);
        }

        public void IF(string s, params object[] args)
        {
            m_Code.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, Indention + s, args);
        }

        public void F(string s, params object[] args)
        {
            m_Code.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, s, args);
        }

        public void A(string s)
        {
            m_Code.Append(s);
        }

        public void A(StringBuilder s)
        {
            m_Code.Append(s);
        }

        public void A(CodeBuilder s)
        {
            m_Code.Append(s.m_Code);
        }

        public void L(string s)
        {
            m_Code.Append(s + "\r\n");
        }

        public void L()
        {
            m_Code.Append("\r\n");
        }

        public void Indent()
        {
            indent++;
        }

        public void Unindent()
        {
            indent--;
        }

        public override string ToString()
        {
            return m_Code.ToString();
        }

        public CodeScope SpecialScope(string start, string end)
        {
            return new CodeScope(this, start, end);
        }

        public CodeScope Scope
        {
            get { return this.SpecialScope("{", "}"); }
        }

        public CheckScopeHelper CheckScope
        {
            get { return new CheckScopeHelper(this); }
        }

        public CodeScope IndentedSection { get { return this.SpecialScope(null, null); } }


        public class CheckScopeHelper : IDisposable
        {
            int m_Level;
            CodeBuilder m_Code;
            public CheckScopeHelper(CodeBuilder code)
            {
                m_Level = code.IndentLevel;
                m_Code = code;
            }

            public void Dispose()
            {
                CheckScope(m_Level, m_Code.IndentLevel, m_Code);
            }

            public static bool CheckScope(int entry, int exit, CodeBuilder code)
            {
                if (exit != entry)
                {
#if (DEBUG)
                    string content = code.ToString();
                    using (System.IO.FileStream dump = new System.IO.FileStream(
                       System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\IndentFailed Code.cs",
                       System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write,
                       System.IO.FileShare.None,
                       content.Length,
                       false))
                    {
                        using (System.IO.StreamWriter writer = new System.IO.StreamWriter(dump))
                        {
                            writer.WriteLine(content);
                        }
                    }
#endif
                    //Sequanto.Debug.Fail(String.Format("Indention error. entry {0}, exit {1}", entry, exit));
                    return false;
                }
                return true;
            }
        }

        public class CodeScope : IDisposable
        {
            int m_Level;
            CodeBuilder m_Code;
            string m_End;
            public CodeScope(CodeBuilder code, string start, string end)
            {
                m_Level = code.IndentLevel;
                m_Code = code;
                m_End = end;
                if (!string.IsNullOrEmpty(start))
                    code.IL(start);
                code.Indent();
            }

            public void Dispose()
            {
                m_Code.Unindent();
                CheckScopeHelper.CheckScope(m_Level, m_Code.IndentLevel, m_Code);
                if (!string.IsNullOrEmpty(m_End))
                    m_Code.IL(m_End);
            }
        }
    }
}
