using StepBro.Core.Api;
using StepBro.Core.Data;
using StepBro.Core.Execution;
using StepBro.VISA.Compatibility;
using System;

namespace StepBro.VISA
{
    public class Instrument : INameable, INamedObject
    {
        private StepBro.VISA.Compatibility.Instrument m_instrument = new Compatibility.Instrument();
        private string m_name = "instrument";
        private enum VisaLoadState { NotLoaded, Loaded, Failed }
        private VisaLoadState m_visaAssemliesLoaded = VisaLoadState.NotLoaded;

        public string Resource
        {
            get { return m_instrument.Resource; }
            set { m_instrument.Resource = value; }
        }

        public string Name { get { return m_name; } set { m_name = value; } }

        public string ShortName { get { return this.Name; } }

        public string FullName { get { return this.Name; } }

        public bool Open([Implicit] ICallContext context = null)
        {
            if (m_visaAssemliesLoaded == VisaLoadState.NotLoaded)
            {
                try
                {
                    var loaded = StepBro.VISA.Compatibility.GacLoader.LoadInstalledVisaAssemblies(new StepBro.VISA.GacLoader());
                    m_visaAssemliesLoaded = VisaLoadState.Loaded;

                    if (context != null && context.LoggingEnabled)
                    {
                        foreach (var assembly in loaded)
                        {
                            if (assembly.Item1)
                            {
                                context.Logger.LogDetail("Loaded VISA assembly: " + assembly.Item2);
                            }
                            else
                            {
                                context.Logger.LogError(assembly.Item2);
                            }
                        }
                    }
                }
                catch
                {
                    if (context != null)
                    {
                        context.ReportError("Failed to load VISA assemblies.");
                    }
                    m_visaAssemliesLoaded = VisaLoadState.Failed;
                    return false;
                }
            }
            else if (m_visaAssemliesLoaded == VisaLoadState.Failed)
            {
                if (context != null)
                {
                    context.ReportError("VISA assemblies are not loaded.");
                }
                return false;
            }

            if (String.IsNullOrEmpty(this.Resource))
            {
                if (context != null)
                {
                    context.ReportError("VISA resource has not been selected.");
                }
                return false;
            }
            try
            {
                m_instrument.Open();
                return true;
            }
            catch (VisaException ex)
            {
                if (context != null)
                {
                    context.ReportError(ex.Message + " " + ex.InnerException.Message, null, ex.InnerException);
                }
                if (context != null && context.LoggingEnabled)
                {
                    try
                    {
                        var resources = m_instrument.GetResources();
                        if (resources != null)
                        {
                            foreach (var resource in resources)
                            {
                                context.Logger.Log("Available resource: " + resource);
                            }
                        }
                        else
                        {
                            context.Logger.LogError("Could not get list of VISA resources.");
                        }
                    }
                    catch (Exception)
                    {
                        context.Logger.LogError("Could not get list of VISA resources.");
                    }
                    finally { }
                }
                return false;
            }
        }

        public string Query([Implicit] ICallContext context, string command)
        {
            return m_instrument.Query(command);
        }

        public void Write([Implicit] ICallContext context, string command)
        {
            m_instrument.Write(command);
        }

        public string Read([Implicit] ICallContext context)
        {
            return m_instrument.Read();
        }
    }
}
