using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.Core.Data
{
    public interface IReadBuffer<T>
    {
        int Count { get; }
        T this[int index] { get; }
        void Eat(int length);
        T[] Get(int index, int length, int total);
        T[] Get(int length);
        bool AwaitNewData(int knownCount, int timeout = 0);
    }
}
