using ELFSharp.ELF;
using ELFSharp.ELF.Sections;
using StepBro.Core.Api;
using StepBro.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Embedded
{
    public class ELFFileReader : IMapFileReader, IDisposable
    {
        private string m_filepath;
        private IELF m_ELF = null;

        public static IMapFileReader OpenFile(string filepath)
        {
            if (File.Exists(filepath))
            {
                var reader = new ELFFileReader(filepath);
                reader.Open();
                return reader;
            }
            throw new FileNotFoundException(filepath);
        }

        private ELFFileReader(string filepath)
        {
            m_filepath = filepath;
        }

        private void Open()
        {
            m_ELF = ELFSharp.ELF.ELFReader.Load(m_filepath);
        }

        public Tuple<uint, uint> GetDataAddressAndSize([Implicit] StepBro.Core.Execution.ICallContext context, string name = null)
        {
            var objects = ((ISymbolTable)m_ELF.GetSection(".symtab")).Entries.Where(x => x.Type == SymbolType.Object);
            if (!String.IsNullOrEmpty(name))
            {
                var found = objects.Cast<SymbolEntry<UInt32>>().Where(s => String.IsNullOrEmpty(name) || s.Name.Contains(name)).FirstOrDefault();
                if (found != null)
                {
                    context.Logger.LogDetail("Object: " + found.Name + " 0x" + found.Value.ToString("X") + " " + found.Size);
                    return new Tuple<uint, uint>(found.Value, found.Size);
                }
            }
            foreach (SymbolEntry<UInt32> o in objects)
            {
                context.Logger.LogDetail("Object: " + o.Name + " size " + o.Size + ", " + o.Value.ToString("X"));
            }
            return new Tuple<uint, uint>(0, 0);     // Not found.
        }

        public void Dispose()
        {
            m_ELF.Dispose();
        }
    }
}
