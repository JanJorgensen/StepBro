using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.Core.Logging;
using StepBro.Core.ScriptData;
using System;
using System.Collections.Generic;
using static StepBro.Core.Data.PropertyBlockDecoder;

namespace StepBro.ToolBarCreator
{
    [Public]
    public class ToolBarMenu : INameable, ISettableFromPropertyBlock
    {
        private string m_name = null;
        private string m_title = null;
        private PropertyBlock m_definition = null;

        private class DefaultScanner : IPropertyBlockDataScanner
        {
            private static Block<object, object> m_decoder;

            static DefaultScanner()
            {
                m_decoder = CreateDecoder();
            }

            public void PreScanData(IScriptFile file, PropertyBlock data, List<Tuple<int, string>> errors)
            {
                ToolBarMenu.DefaultPreScanData(file, data, errors);
            }
            public PropertyBlockDecoder.Element TryGetDecoder()
            {
                return m_decoder;
            }

            private static Block<object, object> CreateDecoder()
            {
                return StepBro.ToolBarCreator.ToolBar.DefaultScanner.GetMenu();
            }
        }

        private static IPropertyBlockDataScanner g_scanner = new DefaultScanner();

        public ToolBarMenu() { }

        public string Name
        {
            get { return m_name; }
            set
            {
                m_name = value;
            }
        }
        public string Title
        {
            get { return m_title; }
            set
            {
                m_title = value;
            }
        }

        public static IPropertyBlockDataScanner PreScanner { get { return g_scanner; } set { g_scanner = value; } }

        public void PreScanData(IScriptFile file, PropertyBlock data, List<Tuple<int, string>> errors)
        {
            g_scanner.PreScanData(file, data, errors);
        }

        public PropertyBlockDecoder.Element TryGetDecoder()
        {
            return g_scanner.TryGetDecoder();
        }

        private static void DefaultPreScanData(IScriptFile file, PropertyBlock data, List<Tuple<int, string>> errors)
        {
            ((Block<object, object>)g_scanner.TryGetDecoder()).DecodeData(file, data, null, errors);
        }

        public void Setup(IScriptFile file, ILogger logger, PropertyBlock data)
        {
            m_definition = data;    // Just save the data reference.
        }

        public PropertyBlock Definition { get { return m_definition; } }
    }
}