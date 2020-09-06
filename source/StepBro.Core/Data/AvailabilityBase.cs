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
                //if (disposing)
                //{
                //    // TODO: dispose managed state (managed objects).
                //}

                //// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                //// TODO: set large fields to null.

                this.DoDispose(disposing);

                disposedValue = true;
                this.Disposed?.Invoke(this, EventArgs.Empty);
            }
        }

        protected virtual void DoDispose(bool disposing) { }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~AvailabilityBase() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
