using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("OpenNETCF.MTConnect.Agent")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Microsoft")]
[assembly: AssemblyProduct("OpenNETCF.MTConnect.Agent")]
[assembly: AssemblyCopyright("Copyright © Microsoft 2010")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("5ef4e156-f843-44d5-9720-447329a8d243")]


// see http://modland.nascom.nasa.gov/browse/calendar.html
[assembly: AssemblyVersion("0.9.10336")]

#if !WindowsCE
[assembly: AssemblyFileVersion("0.9.10336")]
#endif

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("OpenNETCF.MTConnect.Test")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("OpenNETCF.MTConnect.VirtualAgent")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("OpenNETCF.VirtualAgentCE")]
