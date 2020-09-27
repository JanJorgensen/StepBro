using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using StepBro.Core.ScriptData;

namespace StepBro.Core.Parser
{
    public interface IErrorData
    {
        int OffendingSymbol { get; }
        int Line { get; }
        int CharPositionInLine { get; }
        bool JustWarning { get; }
        string Message { get; }
    }
    public interface IErrorCollector
    {
        IScriptFile File { get; }
        int ErrorCount { get; }
        IErrorData this[int index] { get; }
        IErrorData[] GetList();
        event EventHandler EventListChanged;
    }

    internal class ErrorCollector : BaseErrorListener, IErrorCollector, IAntlrErrorListener<IToken>, IAntlrErrorListener<int>
    {
        public class ErrorData : IErrorData
        {
            internal IRecognizer Recognizer { get; private set; }
            internal IToken OffendingTokenSymbol { get; private set; }
            public int OffendingSymbol { get; private set; }
            public int Line { get; private set; }
            public int CharPositionInLine { get; private set; }
            public bool JustWarning { get; private set; }
            public string Message { get; private set; }
            internal RecognitionException Exception { get; private set; }
            public ErrorData(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
            {
                this.Recognizer = recognizer;
                this.OffendingTokenSymbol = offendingSymbol;
                this.OffendingSymbol = -1;
                this.Line = line;
                this.CharPositionInLine = charPositionInLine;
                this.Message = msg;
                this.Exception = e;
                this.JustWarning = false;
            }
            public ErrorData(IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
            {
                this.Recognizer = recognizer;
                this.OffendingTokenSymbol = null;
                this.OffendingSymbol = offendingSymbol;
                this.Line = line;
                this.CharPositionInLine = charPositionInLine;
                this.Message = msg;
                this.Exception = e;
                this.JustWarning = false;
            }
            public ErrorData(int line, int charPositionInLine, string msg, bool justWarning)
            {
                this.Recognizer = null;
                this.OffendingTokenSymbol = null;
                this.OffendingSymbol = -1;
                this.Line = line;
                this.CharPositionInLine = charPositionInLine;
                this.Message = msg;
                this.Exception = null;
                this.JustWarning = justWarning;
            }
            public override string ToString()
            {
                return $"Line {this.Line} \"{this.Message}\"";
            }
        }

        IScriptFile m_file;
        private readonly bool m_throwOnSyntax;

        public ErrorCollector(IScriptFile file, bool throwOnSyntax = false)
        {
            m_file = file;
            m_throwOnSyntax = throwOnSyntax;
        }
        private List<ErrorData> m_errors = new List<ErrorData>();

        public IScriptFile File { get { return m_file; } }

        public override void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            m_errors.Add(new ErrorData(recognizer, offendingSymbol, line, charPositionInLine, msg, e));
            this.NotifyListChanged();
            System.Diagnostics.Trace.WriteLine("PARSING ERROR: " + msg);
            if (m_throwOnSyntax) throw new Exception(msg);
        }

        public void SyntaxError(IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            m_errors.Add(new ErrorData(recognizer, offendingSymbol, line, charPositionInLine, msg, e));
            this.NotifyListChanged();
            System.Diagnostics.Trace.WriteLine("LEXER ERROR: " + msg);
            if (m_throwOnSyntax) throw new Exception(msg);
        }

        public void SymanticError(int line, int charPositionInLine, bool justWarning, string description)
        {
            m_errors.Add(new ErrorData(line, charPositionInLine, description, justWarning));
            this.NotifyListChanged();
            if (m_throwOnSyntax) throw new Exception(description);
        }

        public void UnresolvedType(int line, int charPositionInLine, string name)
        {
            this.SymanticError(line, charPositionInLine, false, $"Unresolved type: \"{name}\".");
        }

        public void UnresolvedIdentifier(int line, int charPositionInLine, string name)
        {
            this.SymanticError(line, charPositionInLine, false, $"Unresolved identifier: \"{name}\".");
        }

        public void IncompatibleDataType(int line, int charPositionInLine, string type, string expectedType)
        {
            this.SymanticError(line, charPositionInLine, false, $"Not a compatible type: '{type}'. Expected type: {expectedType}.");
        }

        public void InternalError(int line, int charPositionInLine, string description)
        {
            m_errors.Add(new ErrorData(line, charPositionInLine, "INTERNAL ERROR. " + description, false));
            this.NotifyListChanged();
#if (DEBUG)
            throw new Exception("INTERNAL ERROR. " + description);
#endif
        }

        public int ErrorCount { get { return m_errors.Count; } }

        public IErrorData this[int index] { get { return m_errors[index]; } }

        public IErrorData[] GetList()
        {
            var list = new List<IErrorData>(m_errors);
            return list.ToArray();
        }

        public void Clear()
        {
            if (m_errors.Count > 0)
            {
                m_errors.Clear();
                this.NotifyListChanged();
            }
        }

        private void NotifyListChanged()
        {
            this.EventListChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler EventListChanged;
    }
}
