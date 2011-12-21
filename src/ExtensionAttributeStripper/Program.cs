using System;
using System.IO;
using System.Linq;

using Mono.Cecil;
using CommandLine;
using CommandLine.Text;
using System.Reflection;

namespace Stripper
{
    public class Program
    {
        public class CommandLineOptions
        {
            [HelpOption( HelpText = "Display this help screen." )]
            public string GetUsage()
            {
                HelpText text = new HelpText( "AttributeStripper 1.0" );
                
                text.Copyright = new CopyrightInfo( "InishTechnology Ventures Ltd.", new int[] { 2011 } );
                text.AddPreOptionsLine( " " );
                text.AddPreOptionsLine( "This is free software. You may redistribute copies of it under the terms of" );
                text.AddPreOptionsLine( "the MIT License <http://www.opensource.org/licenses/mit-license.php>." );
                text.AddOptions( this );

                return text;
            }

            [Option( "i", "assembly", HelpText = "Input assembly path", Required = true )]
            public string AssemblyPath;

            [Option( "o", "outputDirectory", HelpText = "Output directory for processed assembly", Required = true )]
            public string OutputDirectory;

			[Option( "s", "signingKeyPairFile", HelpText = "SNK File with Signing keys for resigning the modified assembly", Required = false )]
			public string SigningKeyPairFile;
        }

        public static int Main( string[] args )
        {
            Console.WriteLine();
            CommandLineOptions options = new CommandLineOptions();

            ICommandLineParser parser = new CommandLineParser( new CommandLineParserSettings( Console.Out ) );
            if (!parser.ParseArguments( args, options ))
                return 1;

            ProcessAssembly( options );

            return 0;
        }

		private static void ProcessAssembly( CommandLineOptions options )
		{
			AssemblyDefinition assembly = ReadAssembly( options.AssemblyPath );

			string attributeToStrip = "System.Runtime.CompilerServices.ExtensionAttribute";

			var query = assembly.MainModule.Types.Where( x => x.FullName == attributeToStrip );

			if (!query.Any())
			{
				Console.WriteLine( "Nothing to do, no definition of {0} found.", attributeToStrip );
				Console.WriteLine( "Just round-tripping the assembly." );
			}
			else
			{
				AttributeStripper stripper = new AttributeStripper( query.Single() );
				stripper.Process( assembly );
			}

			PrepareOutputDirectory( options.OutputDirectory );

			string targetPath = Path.Combine( options.OutputDirectory, Path.GetFileName( options.AssemblyPath ) );
			StrongNameKeyPair snk = options.SigningKeyPairFile != null ? new StrongNameKeyPair( File.ReadAllBytes( options.SigningKeyPairFile ) ) : null;

			WriteAssembly( assembly, targetPath, snk );
		}

        private static void PrepareOutputDirectory( string outdir )
        {
            if (!Directory.Exists( outdir ))
                Directory.CreateDirectory( outdir );
        }

        private static AssemblyDefinition ReadAssembly( string path )
        {
            ReaderParameters rp = new ReaderParameters()
            {
                ReadSymbols = true
            };

            var assembly = AssemblyDefinition.ReadAssembly( path, rp );
            return assembly;
        }

		static void WriteAssembly( AssemblyDefinition assembly, string targetPath, StrongNameKeyPair snk )
		{
            WriterParameters wp = new WriterParameters()
            {
                WriteSymbols = true,
				StrongNameKeyPair = snk
            };

            assembly.Write( targetPath, wp );
        }
    }
}