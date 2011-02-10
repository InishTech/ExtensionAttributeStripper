ExtensionAttributeStripper is a small Cecil based command line utility that allows you to strip a custom definition of ExtensionAttribute and all its usages from an assembly.
This is useful in scenarios where you need to use .NET 2.0 specific libraries that employ this hack together with .NET > 2 libraries, such as when your product needs to support .NET 2.0 but you have a test suite written in 3.5. 

## Usage:
Display the Help text: 

* **ExtensionAttributeStripper -help**

Strip ExtensionAttribute from "inputAssembly" and put the resultant assembly into "targetDirectory":

* **ExtensionAttributeStripper -i "inputAssembly.dll" -o "targetDirectory"**