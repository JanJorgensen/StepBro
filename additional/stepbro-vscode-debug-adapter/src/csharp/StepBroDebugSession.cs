using System;
using System.Collections.Generic;

namespace VSCodeDebug
{
	public class StepBroDebugSession : DebugSession
	{
        public StepBroDebugSession() : base()
		{
            // TODO
            Program.Log("Constructor");
        }

        public override void Initialize(Response response, dynamic args)
        {
            Program.Log("Initialize");

            OperatingSystem os = Environment.OSVersion;
			if (os.Platform != PlatformID.MacOSX && os.Platform != PlatformID.Unix && os.Platform != PlatformID.Win32NT) {
				SendErrorResponse(response, 3000, "StepBro Debug Adapter is not supported on this platform ({_platform}).", new { _platform = os.Platform.ToString() }, true, true);
				return;
			}

			SendResponse(response, new Capabilities() {
				// This debug adapter does not need the configurationDoneRequest.
				supportsConfigurationDoneRequest = false,

				// This debug adapter does not support function breakpoints.
				supportsFunctionBreakpoints = false,

				// This debug adapter doesn't support conditional breakpoints.
				supportsConditionalBreakpoints = false,

				// This debug adapter does not support a side effect free evaluate request for data hovers.
				supportsEvaluateForHovers = false,

                // This debug adapter does not support exception filters.
				supportsExceptionFilterOptions = false
			});

			// Mono Debug is ready to accept breakpoints immediately
			SendEvent(new InitializedEvent());
        }

		public override void Launch(Response response, dynamic arguments)
        {
            Program.Log("Launch");
            SendResponse(response);
        }

		public override void Attach(Response response, dynamic arguments)
        {
            Program.Log("Attach");
            SendResponse(response);
        }

		public override void Disconnect(Response response, dynamic arguments)
        {
            Program.Log("Disconnect");
            SendResponse(response);
        }

		public override void SetFunctionBreakpoints(Response response, dynamic arguments)
		{
            Program.Log("SetFunctionBreakpoints");
            SendResponse(response);
		}

		public override void SetExceptionBreakpoints(Response response, dynamic arguments)
		{
            Program.Log("SetExceptionBreakpoints");
            SendResponse(response);
		}

		public override void SetBreakpoints(Response response, dynamic arguments)
        {
            Program.Log("SetBreakpoints");
            SendResponse(response);
        }

		public override void Continue(Response response, dynamic arguments)
        {
            Program.Log("Continue");
            SendResponse(response);
        }

		public override void Next(Response response, dynamic arguments)
        {
            Program.Log("Next");
            SendResponse(response);
        }

		public override void StepIn(Response response, dynamic arguments)
        {
            Program.Log("StepIn");
            SendResponse(response);
        }

		public override void StepOut(Response response, dynamic arguments)
        {
            Program.Log("StepOut");
            SendResponse(response);
        }

		public override void Pause(Response response, dynamic arguments)
        {
            Program.Log("Pause");
            SendResponse(response);
        }

		public override void StackTrace(Response response, dynamic arguments)
        {
            Program.Log("StackTrace");
            SendResponse(response);
        }

		public override void Scopes(Response response, dynamic arguments)
        {
            Program.Log("Scopes");
            SendResponse(response);
        }

		public override void Variables(Response response, dynamic arguments)
        {
            Program.Log("Variables");
            SendResponse(response);
        }

		public override void Source(Response response, dynamic arguments)
        {
            Program.Log("Source");
            SendResponse(response);
        }

		public override void Threads(Response response, dynamic arguments)
        {
            Program.Log("Threads");
            // We need to respond with a thread so the debugger knows which thread to pause
            var threads = new List<Thread>
            {
                new Thread(1, "thread 1")
            };
            SendResponse(response, new ThreadsResponseBody(threads));
        }

		public override void Evaluate(Response response, dynamic arguments)
        {
            Program.Log("Evaluate");
            SendResponse(response);
        }
    }
}