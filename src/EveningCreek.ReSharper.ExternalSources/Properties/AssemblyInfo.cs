using System.Reflection;

[assembly: AssemblyTitle("resharper-externalsources")]
[assembly: AssemblyDescription("Provides support for including external source files for ReSharper code inspection.")]
[assembly: AssemblyCompany("EveningCreek")]
[assembly: AssemblyProduct("resharper-externalsources")]
[assembly: AssemblyCopyright("Copyright © EveningCreek, 2014")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyVersion("1.0.0.*")]

#if DEBUG
[assembly: AssemblyConfiguration("DEBUG")]
#else
[assembly: AssemblyConfiguration("RELEASE")]
#endif