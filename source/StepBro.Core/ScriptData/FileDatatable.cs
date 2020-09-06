using System;
using System.Collections.Generic;
using StepBro.Core.Data;

namespace StepBro.Core.ScriptData
{
    internal class FileDatatable : FileElement, IDatatable
    {
        internal class Column : IDatatableColumn
        {
            private readonly FileDatatable m_parent;

            public Column(FileDatatable parent)
            {
                m_parent = parent;
            }

            public long index { get; internal set; }

            public string Name { get; internal set; }

            public string Reference { get; internal set; }

            public string CellType { get; internal set; }
        }

        internal class Cell : IDatatableCell
        {
            private readonly Row m_parent;
            private string m_text = null;
            private TypeReference m_dataType = null;
            private object m_value = null;

            public Cell(Row parent)
            {
                m_parent = parent;
            }
            public string CellText
            {
                get { return m_text; }
                set { m_text = value; }
            }
            public TypeReference DecodedDataType
            {
                get { return m_dataType; }
                set { m_dataType = value; }
            }
            public object DirectValue
            {
                get { return m_value; }
                set { m_value = value; }
            }
        }

        internal class Row : IDatatableRow
        {
            private readonly FileDatatable m_parent;
            private readonly string m_headerText;
            private readonly List<Cell> m_cells = new List<Cell>();

            public Row(FileDatatable parent, long cellCount)
            {
                m_parent = parent;
                for (long i = 0L; i < cellCount; i++) { m_cells.Add(new Cell(this)); }
                m_headerText = null;
            }
            public Row(FileDatatable parent, string rowStart, IEnumerable<Tuple<string, TypeReference, object>> cells)
            {
                m_parent = parent;
                m_headerText = rowStart;
                foreach (var c in cells)
                {
                    var cell = new Cell(this);
                    cell.CellText = c.Item1;
                    cell.DecodedDataType = c.Item2;
                    cell.DirectValue = c.Item3;
                    m_cells.Add(cell);
                }
            }
            public long index { get { return m_parent.GetRowIndex(this); } }

            public string HeaderText { get { return m_headerText; } }

            public IDatatableCell GetCell(long index)
            {
                return m_cells[(int)index];
            }

            public int GetCellCount()
            {
                return m_cells.Count;
            }
        }

        private Data.PropertyBlock m_elementProperties = null;
        private readonly List<Column> m_columns = new List<Column>();
        private readonly List<Row> m_rows = new List<Row>();

        public FileDatatable(
            IScriptFile file,
            int line,
            IDatatable parentElement,
            string @namespace,
            string name) :
                base(file, line, parentElement, @namespace, name, FileElementType.Datatable)
        {
        }

        public void SetElementPropertyBlock(Data.PropertyBlock properties)
        {
            m_elementProperties = properties;
        }

        public IDatatableColumn AddColumn()
        {
            var column = new Column(this);
            m_columns.Add(column);
            return column;
        }

        public long ColumnCount
        {
            get { return m_columns.Count; }
        }

        public long RowCount
        {
            get { return m_rows.Count; }
        }

        public IDatatableCell GetCell(long column, long row)
        {
            var tableRow = m_rows[(int)row];
            return tableRow.GetCell(column);
        }

        internal long GetRowIndex(Row row)
        {
            return m_rows.IndexOf(row);
        }

        protected override TypeReference GetDataType()
        {
            return new TypeReference(typeof(IDatatable), this);
        }

        public IDatatableColumn GetColumn(long index)
        {
            return m_columns[(int)index];
        }

        public void AddRow(object[] cellData)
        {
            throw new NotImplementedException();
        }

        public void AddSourceRow(string start, IEnumerable<Tuple<string, TypeReference, object>> cells)
        {
            var row = new Row(this, start, cells);
            m_rows.Add(row);
        }

        public IDatatableRow GetRow(long index)
        {
            return m_rows[(int)index];
        }

        public void ParseSource()
        {
            if (m_elementProperties != null)
            {
                // Any parent-type for inheritance?
                if (m_elementProperties[0].BlockEntryType == Data.PropertyBlockEntryType.Flag)
                {

                }
            }
            else if (m_rows.Count == 0)
            {
                // Error: If no rows, we MUST have properties with parent definition or column definitions.
                throw new NoColumnsDefinedException();
            }
            if (m_rows.Count > 0)
            {
                List<int> rowCellColumnIndices = new List<int>();
                List<int> rowsToRemove = new List<int>();
                for (int r = 0; r < m_rows.Count; r++)
                {
                    var row = m_rows[r];
                    if (row.HeaderText[1] == '_')
                    {
                        bool isReferenceToo = row.HeaderText.Contains("@");
                        rowsToRemove.Add(r);
                        if (m_columns.Count == 0)
                        {
                            // Create columns
                            for (int i = 0; i < row.GetCellCount(); i++)
                            {
                                var column = new Column(this);
                                column.Name = row.GetCell(i).CellText.Trim();
                                if (isReferenceToo) column.Reference = column.Name;
                                m_columns.Add(column);
                            }
                        }
                        else
                        {
                            // Use existing columns
                            if (m_columns.Count == 0 && this.ParentElement == null)
                            {
                                throw new NoColumnsDefinedException();
                            }
                            throw new NotImplementedException();
                        }
                    }
                }
                rowsToRemove.Reverse();
                foreach (var r in rowsToRemove) m_rows.RemoveAt(r);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }

    public class NoColumnsDefinedException : Exception
    {
        public NoColumnsDefinedException() : base() { }
    }
}
