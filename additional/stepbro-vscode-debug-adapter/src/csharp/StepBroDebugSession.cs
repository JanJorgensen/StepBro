using System;

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
				SendErrorResponse(response, 3000, "StepBro Debug is not supported on this platform ({_platform}).", new { _platform = os.Platform.ToString() }, true, true);
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
        }

		public override void Attach(Response response, dynamic arguments)
        {
            Program.Log("Attach");
        }

		public override void Disconnect(Response response, dynamic arguments)
        {
            Program.Log("Disconnect");
        }

		public override void SetFunctionBreakpoints(Response response, dynamic arguments)
		{
            Program.Log("SetFunctionBreakpoints");
		}

		public override void SetExceptionBreakpoints(Response response, dynamic arguments)
		{
            Program.Log("SetExceptionBreakpoints");
		}

		public override void SetBreakpoints(Response response, dynamic arguments)
        {
            Program.Log("SetBreakpoints");
        }

		public override void Continue(Response response, dynamic arguments)
        {
            Program.Log("Continue");
        }

		public override void Next(Response response, dynamic arguments)
        {
            Program.Log("Next");
        }

		public override void StepIn(Response response, dynamic arguments)
        {
            Program.Log("StepIn");
        }

		public override void StepOut(Response response, dynamic arguments)
        {
            Program.Log("StepOut");
        }

		public override void Pause(Response response, dynamic arguments)
        {
            Program.Log("Pause");
        }

		public override void StackTrace(Response response, dynamic arguments)
        {
            Program.Log("StackTrace");
        }

		public override void Scopes(Response response, dynamic arguments)
        {
            Program.Log("Scopes");
        }

		public override void Variables(Response response, dynamic arguments)
        {
            Program.Log("Variables");
        }

		public override void Source(Response response, dynamic arguments)
        {
            Program.Log("Source");
        }

		public override void Threads(Response response, dynamic arguments)
        {
            Program.Log("Threads");
        }

		public override void Evaluate(Response response, dynamic arguments)
        {
            Program.Log("Evaluate");
        }
    }
}