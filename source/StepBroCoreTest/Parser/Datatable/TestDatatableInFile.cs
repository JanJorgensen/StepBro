using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Text;
using StepBro.Core.Parser;
using StepBro.Core.ScriptData;

namespace StepBroCoreTest.Parser
{
    [TestClass]
    public class TestDatatableInFile
    {
        [TestMethod]
        [Ignore]
        public void TestFileWithSimpleDatatable()
        {
            var f = new StringBuilder();
            f.AppendLine("datatable MyTable");
            f.AppendLine("|___| Ax | By    | Cn      |");
            f.AppendLine("|   | 10 | true  | \"Nips\" |");
            f.AppendLine("|   | 63 |       | Misery  |");
            f.AppendLine("|   | 24 | false | 1.4s    |");
            f.AppendLine("|   |  8 |  true | @16:24  |");
            f.AppendLine("int UseTheTable() {");
            f.AppendLine("   return 10;");
            f.AppendLine("}");
            var file = FileBuilder.ParseFile(null, f.ToString());
            Assert.AreEqual(1, file.ListElements().Where(e => e.ElementType == FileElementType.Datatable).Count());
            Assert.AreEqual(1, file.ListElements().Where(e => e.ElementType == FileElementType.ProcedureDeclaration).Count());
            var procedure = file.ListElements().First(p => p.Name == "UseTheTable") as IFileProcedure;
            Assert.AreEqual("UseTheTable", procedure.Name);
            var table = file.GetFileElement<IDatatable>("MyTable");
            Assert.AreEqual("MyTable", table.Name);
            Assert.AreEqual(3L, table.ColumnCount);
            Assert.AreEqual(4L, table.RowCount);

            this.ExpectCell(table, 0, 0, "10", typeof(long), 10L);
            this.ExpectCell(table, 0, 1, "63", typeof(long), 63L);
            this.ExpectCell(table, 0, 2, "24", typeof(long), 24L);
            this.ExpectCell(table, 0, 3, "8", typeof(long), 8L);

            this.ExpectCell(table, 1, 0, "true", typeof(bool), true);
            this.ExpectCell(table, 1, 1, "", typeof(void), null);
            this.ExpectCell(table, 1, 2, "false", typeof(bool), false);
            this.ExpectCell(table, 1, 3, "true", typeof(bool), true);

            this.ExpectCell(table, 2, 0, "\"Nips\"", typeof(string), "Nips");
            this.ExpectCell(table, 2, 1, "Misery", typeof(StepBro.Core.Data.Identifier), "Misery");
            this.ExpectCell(table, 2, 2, "1.4s", typeof(TimeSpan), TimeSpan.FromTicks(TimeSpan.TicksPerMillisecond * 1400));
            this.ExpectCell(table, 2, 3, "@16:24", typeof(TimeSpan), TimeSpan.Parse("16:24:00"));

            //var taskContext = ExecutionHelper.ExeContext();
            //taskContext.Logger.IsDebugging = true;

            //var result = taskContext.CallProcedure(procedure);
            //Assert.AreEqual(20, (long)result);
        }

        private void ExpectCell(IDatatable table, int column, int row, string text, System.Type type, object value)
        {
            var cell = table.GetCell(column, row);
            Assert.AreEqual(text, cell.CellText, $"cell [{column},{row}]");
            if (type != typeof(void))
            {
                Assert.IsNotNull(cell.DecodedDataType);
                Assert.AreEqual(type, cell.DecodedDataType.Type, $"cell [{column},{row}]");
            }
            else
            {
                Assert.IsNull(cell.DecodedDataType);
            }
            Assert.AreEqual(value, cell.DirectValue, $"cell [{column},{row}]");
        }
    }
}
