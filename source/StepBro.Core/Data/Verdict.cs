namespace StepBro.Core.Data
{
    /// <summary>
    /// Enumerates the different basic results of a script execution or a tested expectation.
    /// </summary>
    public enum Verdict : int
    {
        /// <summary>
        /// The execution has no verdict.
        /// </summary>
        Unset = 0,
        /// <summary>
        /// All test expectations were fulfilled.
        /// </summary>
        Pass = 1,
        /// <summary>
        /// No pass/fail test result could be determined.
        /// </summary>
        Inconclusive = 2,
        /// <summary>
        /// One or more expectations in the test was not fulfilled.
        /// </summary>
        Fail = 3,
        /// <summary>
        /// Indicates that the user chose to abandon/break a running test.
        /// </summary>
        Abandoned = 4,
        /// <summary>
        /// A fatal error in the script execution.
        /// </summary>
        Error = 5
    }
}
