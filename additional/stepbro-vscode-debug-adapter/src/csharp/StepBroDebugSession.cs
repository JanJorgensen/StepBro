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
        }

        public override void Initialize(Response response, dynamic args)
        {

        }

		public override void Launch(Response response, dynamic arguments)
        {

        }

		public override void Attach(Response response, dynamic arguments)
        {

        }

		public override void Disconnect(Response response, dynamic arguments)
        {

        }

		public override void SetFunctionBreakpoints(Response response, dynamic arguments)
		{
		}

		public override void SetExceptionBreakpoints(Response response, dynamic arguments)
		{
		}

		public override void SetBreakpoints(Response response, dynamic arguments)
        {

        }

		public override void Continue(Response response, dynamic arguments)
        {

        }

		public override void Next(Response response, dynamic arguments)
        {

        }

		public override void StepIn(Response response, dynamic arguments)
        {

        }

		public override void StepOut(Response response, dynamic arguments)
        {

        }

		public override void Pause(Response response, dynamic arguments)
        {

        }

		public override void StackTrace(Response response, dynamic arguments)
        {

        }

		public override void Scopes(Response response, dynamic arguments)
        {

        }

		public override void Variables(Response response, dynamic arguments)
        {

        }

		public override void Source(Response response, dynamic arguments)
        {

        }

		public override void Threads(Response response, dynamic arguments)
        {

        }

		public override void Evaluate(Response response, dynamic arguments)
        {

        }
    }
}