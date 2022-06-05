using System.Diagnostics;
using System.Reflection;
using System.Windows;

[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, //where theme specific resource dictionaries are located
                                     //(used if a resource is not found in the page,
                                     // or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
                                              //(used if a resource is not found in the page,
                                              // app, or any theme specific resource dictionaries)
)]



namespace WorkTimer4
{
    internal static class AssemblyInfo
    {
        public static string? ProductName { get; }
        public static string? ProductVersion { get; }

        static AssemblyInfo()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            //var companyName = fvi.CompanyName;
            ProductName = fvi.ProductName;
            ProductVersion = fvi.ProductVersion;
        }
    }
}