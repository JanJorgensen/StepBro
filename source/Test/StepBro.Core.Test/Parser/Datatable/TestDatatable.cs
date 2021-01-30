using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StepBro.Core.Data;
using StepBro.Core.Parser;
using StepBro.Core.ScriptData;
using static StepBroCoreTest.Parser.ExpressionParser;

namespace StepBroCoreTest.Parser
{
    [TestClass]
    public class TestDatatable
    {
        [TestMethod][ExpectedException(typeof(NoColumnsDefinedException))]
        public void TestEmptyTable()
        {
            FileBuilder.ParseDatatable("datatable MyTable;\r\n");
        }

        [TestMethod]
        public void TestTableWithColumnsAndNoRows()
        {
            var table = FileBuilder.ParseDatatable(
                "datatable MyTable\r\n"+
                "|___| Ax | By    |\r\n" +
                "|   | 10 | True  |\r\n" +
                "|   | 63 | False |\r\n" +
                "|   | 24 | False |\r\n");

            Assert.AreEqual(2L, table.ColumnCount);
            Assert.AreEqual(3L, table.RowCount);

            Assert.AreEqual("Ax", table.GetColumn(0).Name);
            Assert.IsNull(table.GetColumn(0).Reference);
            Assert.IsNull(table.GetColumn(0).CellType);

            Assert.AreEqual("By", table.GetColumn(1).Name);
            Assert.IsNull(table.GetColumn(1).Reference);
            Assert.IsNull(table.GetColumn(1).CellType);

            Assert.AreEqual("10", table.GetCell(0, 0).CellText);
            Assert.AreEqual("63", table.GetCell(0, 1).CellText);
            Assert.AreEqual("24", table.GetCell(0, 2).CellText);
            Assert.AreEqual("True", table.GetCell(1, 0).CellText);
            Assert.AreEqual("False", table.GetCell(1, 1).CellText);
            Assert.AreEqual("False", table.GetCell(1, 2).CellText);
        }

        [TestMethod]
        public void TestTableWithAllDifferentRowHeaders()
        {
            var row = FileBuilder.ParseDatatableRow("| | Ax | By    |\r\n");
            Assert.AreEqual(3, row.Count);
            row = FileBuilder.ParseDatatableRow("|       | Ax | By    |\r\n");
            Assert.AreEqual(3, row.Count);

            row = FileBuilder.ParseDatatableRow("|___| Ax | By    |\r\n");
            Assert.AreEqual(3, row.Count);
            row = FileBuilder.ParseDatatableRow("|_| Ax | By    |\r\n");
            Assert.AreEqual(3, row.Count);

            row = FileBuilder.ParseDatatableRow("|_@| Ax | By    |\r\n");
            Assert.AreEqual(3, row.Count);
            row = FileBuilder.ParseDatatableRow("|_____@| Ax | By    |\r\n");
            Assert.AreEqual(3, row.Count);

            row = FileBuilder.ParseDatatableRow("|@| Ax | By    |\r\n");
            Assert.AreEqual(3, row.Count);
            row = FileBuilder.ParseDatatableRow("|    @| Ax | By    |\r\n");
            Assert.AreEqual(3, row.Count);

            row = FileBuilder.ParseDatatableRow("|//| Ax | By    |\r\n");
            Assert.AreEqual(3, row.Count);
            row = FileBuilder.ParseDatatableRow("|  //| Ax | By    |\r\n");
            Assert.AreEqual(3, row.Count);
            row = FileBuilder.ParseDatatableRow("|//  | Ax | By    |\r\n");
            Assert.AreEqual(3, row.Count);

            row = FileBuilder.ParseDatatableRow("| type | Ax | By    |\r\n");
            Assert.AreEqual(3, row.Count);
            row = FileBuilder.ParseDatatableRow("| name | Ax | By    |\r\n");
            Assert.AreEqual(3, row.Count);
            row = FileBuilder.ParseDatatableRow("| ref | Ax | By    |\r\n");
            Assert.AreEqual(3, row.Count);
            row = FileBuilder.ParseDatatableRow("| motorhead | Ax | By    |\r\n");
            Assert.AreEqual(3, row.Count);
        }

        //[TestMethod]
        //public void TestTableRowWithOneEmptyColumn()
        //{
        //    var table = TSharpFileBuilder.ParseDatatableRow("|__| |");
        //    Assert.AreEqual(0L, table.ColumnCount);
        //    Assert.AreEqual(0L, table.RowCount);
        //}
    }
}
