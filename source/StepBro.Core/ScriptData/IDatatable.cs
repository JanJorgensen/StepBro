using System;

namespace StepBro.Core.ScriptData
{
    public interface IDatatable : IFileElement
    {
        long ColumnCount { get; }
        long RowCount { get; }
        IDatatableColumn GetColumn(long index);
        IDatatableCell GetCell(long column, long row);
        void AddRow(object[] cellData);
        IDatatableRow GetRow(long index);
    }

    public interface IDatatableColumn
    {
        long index { get; }
        string Name { get; }
        string Reference { get; }
        string CellType { get; }
    }

    public interface IDatatableRow
    {
        long index { get; }
        IDatatableCell GetCell(long index);
    }

    public interface IDatatableCell
    {
        string CellText { get; set; }
        Data.TypeReference DecodedDataType { get; }
        object DirectValue { get; }
    }
}
