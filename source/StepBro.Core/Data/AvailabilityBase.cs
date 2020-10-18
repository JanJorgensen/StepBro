using System;

namespace StepBro.Core.Data
{
    public class AvailabilityBase : IAvailability
    {
        public bool IsStillValid { get { return !disposedValue; } }

        public event EventHandler Disposing;
        public event EventHandler Disposed;

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                this.Disposing?.Invoke(this, EventArgs.Empty);

                this.DoDispose(disposing);

                disposedValue = true;
                this.Disposed?.Invoke(this, EventArgs.Empty);
            }
        }

        protected virtual void DoDispose(bool disposing) { }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);
        }
        #endregion
    }
}
