using FastColoredTextBoxNS;
using System;
using System.IO;

namespace StepBro.Core.Controls
{
    public class FCTBRangeStream : Stream
    {
        private readonly FastColoredTextBoxNS.Range m_range;
        private FastColoredTextBoxNS.Place m_currentPosition;

        public FCTBRangeStream(FastColoredTextBoxNS.Range range)
        {
            m_range = range;
            m_currentPosition = range.Start;
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override long Length
        {
            get { return (long)m_range.TextLength; }
        }

        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }

    public class FCTBRangeTextReader : TextReader
    {
        private readonly FastColoredTextBoxNS.Range m_range;
        private Place m_from;
        private Place m_to;
        private int m_currentPosition = 0;
        private readonly int[] m_startIndices;
        private readonly int m_size;

        public FCTBRangeTextReader(FastColoredTextBoxNS.Range range)
        {
            m_range = range;
            var tb = range.tb;
            /*
            m_currentPosition = range.Start;
            int c = range.End.iLine - range.Start.iLine + 1;
            m_startIndices = new int[c+1];
            int current = 0 - range.Start.iChar;
            int iLine = m_range.Start.iLine;
            var tb = m_range.tb;
            for (int i = 0; i < c; i++)
            {
                m_startIndices[i] = current;
                if (iLine < tb.LinesCount - 1)
                {
                    current += range.tb[iLine++].Count + 2; // Characters + newline
                }
            }
            m_startIndices[m_startIndices.Length - 1] = Int32.MaxValue;
            */
            m_from = range.Start;
            m_to = range.End;
            if (m_from.iLine > m_to.iLine || (m_from.iLine == m_to.iLine && m_from.iChar > m_to.iChar))
            {
                m_from = range.End;
                m_to = range.Start;
            }

            m_size = 0;
            if (m_from.iLine >= 0)
            {
                int i = 0;
                int current = 0 - m_from.iChar;
                m_startIndices = new int[(m_to.iLine - m_from.iLine) + 2];
                for (int y = m_from.iLine; y <= m_to.iLine; y++)
                {
                    m_startIndices[i++] = current;
                    int fX = y == m_from.iLine ? m_from.iChar : 0;
                    int tX = y == m_to.iLine ? Math.Min(tb[y].Count - 1, m_to.iChar - 1) : tb[y].Count - 1;
                    var lineSize = tX - fX + 1;

                    if (y != m_to.iLine && m_from.iLine != m_to.iLine)
                    {
                        lineSize += Environment.NewLine.Length;
                    }
                    m_size += lineSize;
                    current += lineSize;
                }
                m_startIndices[i] = Int32.MaxValue;
                if (m_size != range.Length)
                {
                    throw new Exception("Error in calculation");
                }
            }
            else
            {
                m_startIndices = new int[0];
            }
        }

        public override int Read(char[] buffer, int index, int count)
        {
            if (m_range.ColumnSelectionMode)
            {
                throw new NotImplementedException();
            }

            if (index >= m_size) return 0;

            var pos = index + m_currentPosition;
            int writeIndex = 0;
            var iStartLine = 0;
            while (m_startIndices[iStartLine + 1] <= index) iStartLine++;
            var iLine = iStartLine;

            var tb = m_range.tb;
            var blockSize = Math.Min(count, m_size - pos);
            var charsLeft = blockSize;
            var first = pos - m_startIndices[iLine];
            while (charsLeft > 0 && iLine <= m_to.iLine)
            {
                var line = tb[iLine + m_from.iLine];
                for (int i = first; i < line.Count && charsLeft > 0; i++, charsLeft--)
                {
                    buffer[writeIndex++] = line[i].c;
                    charsLeft--;
                }
                if (iLine != m_to.iLine && m_from.iLine != m_to.iLine)
                {
                    if (charsLeft > 0)
                    {
                        buffer[writeIndex++] = '\r';
                        charsLeft--;
                    }
                    if (charsLeft > 0)
                    {
                        buffer[writeIndex++] = '\n';
                        charsLeft--;
                    }
                }
                iLine++;
                first = 0;
            }
            var size = blockSize - charsLeft;
            m_currentPosition += size;
            return size;


            //int i = 0;
            //for (int y = fromLine; y <= toLine; y++)
            //{
            //    int fromX = y == fromLine ? fromChar : 0;
            //    int toX = y == toLine ? Math.Min(tb[y].Count - 1, toChar - 1) : tb[y].Count - 1;
            //    for (int x = fromX; x <= toX; x++)
            //        sb.Append(tb[y][x].c);
            //    if (y != toLine && fromLine != toLine)
            //        sb.AppendLine();
            //}
            //return sb.ToString();


            throw new NotImplementedException();
        }
    }
}
