using System;

namespace VSCodeDebug
{
	public class StepBroDebugSession : DebugSession
	{
        private readonly string[] STEPBRO_EXTENSIONS = new String[] {
			".sbs",
		};

        public StepBroDebugSession() : base()
		{
            // TODO
            Console.WriteLine("Constructor");
        }

        public override void Initialize(Response response, dynamic args)
        {
            Console.WriteLine("Initialize");

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
            Console.WriteLine("Launch");
        }

		public override void Attach(Response response, dynamic arguments)
        {
            Console.WriteLine("Attach");
        }

		public override void Disconnect(Response response, dynamic arguments)
        {
            Console.WriteLine("Disconnect");
        }

		public override void SetFunctionBreakpoints(Response response, dynamic arguments)
		{
            Console.WriteLine("SetFunctionBreakpoints");
		}

		public override void SetExceptionBreakpoints(Response response, dynamic arguments)
		{
            Console.WriteLine("SetExceptionBreakpoints");
		}

		public override void SetBreakpoints(Response response, dynamic arguments)
        {
            Console.WriteLine("SetBreakpoints");
        }

		public override void Continue(Response response, dynamic arguments)
        {
            Console.WriteLine("Continue");
        }

		public override void Next(Response response, dynamic arguments)
        {
            Console.WriteLine("Next");
        }

		public override void StepIn(Response response, dynamic arguments)
        {
            Console.WriteLine("StepIn");
        }

		public override void StepOut(Response response, dynamic arguments)
        {
            Console.WriteLine("StepOut");
        }

		public override void Pause(Response response, dynamic arguments)
        {
            Console.WriteLine("Pause");
        }

		public override void StackTrace(Response response, dynamic arguments)
        {
            Console.WriteLine("StackTrace");
        }

		public override void Scopes(Response response, dynamic arguments)
        {
            Console.WriteLine("Scopes");
        }

		public override void Variables(Response response, dynamic arguments)
        {
            Console.WriteLine("Variables");
        }

		public override void Source(Response response, dynamic arguments)
        {
            Console.WriteLine("Source");
        }

		public override void Threads(Response response, dynamic arguments)
        {
            Console.WriteLine("Threads");
        }

		public override void Evaluate(Response response, dynamic arguments)
        {
            Console.WriteLine("Evaluate");
        }
    }
}