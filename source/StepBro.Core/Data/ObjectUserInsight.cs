using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Data
{
    public interface IUserInsightPropertyObject
    {
    }

    public interface IUserInsightHtmlView
    {
        System.IO.Stream Content { get; }
    }

    public interface IUserInsightFileReference
    {
        string FilePath { get; }
        int Line { get; }
        int Column { get; }
    }

    public interface IUserInsights
    {
        IEnumerable<object> ListPropertyObjects();
    }
}
