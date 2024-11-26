namespace StepBro.SimpleWorkbench
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ActiproSoftware.Products.ActiproLicenseManager.RegisterLicense("Jan Jørgensen", "WIN241-WGKNK-4Y8ER-0QQR4-0KGG");
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}