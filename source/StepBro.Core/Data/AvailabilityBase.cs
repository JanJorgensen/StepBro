﻿using System;
using System.ComponentModel;

namespace StepBro.Core.Data
{
    public class AvailabilityBase : IAvailability
    {
        [Browsable(false)]
        public bool IsStillValid { get { return !m_isDisposed; } }

        public event EventHandler Disposing;
        public event EventHandler Disposed;

        #region IDisposable Support
        private bool m_isDisposed = false; // To detect redundant calls

        protected void Dispose(bool disposing)
        {
            if (!m_isDisposed)
            {
                this.Disposing?.Invoke(this, EventArgs.Empty);

                this.DoDispose(disposing);
                m_isDisposed = true;

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
