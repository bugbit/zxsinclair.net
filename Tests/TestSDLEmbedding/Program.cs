using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace TestSDLEmbedding
{
    class Program
    {
        /// <summary>
		/// Used by DllImport to load the native library.
		/// </summary>
		private const string nativeLibName = "SDL2.dll";

        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        public static string LoadUnmanagedLibraryFromResource(Assembly assembly,
           string libraryResourceName,
           string libraryName)
        {
            string tempDllPath = string.Empty;
            using (Stream s = assembly.GetManifestResourceStream(libraryResourceName))
            {
                byte[] data = new BinaryReader(s).ReadBytes((int)s.Length);

                string assemblyPath = Path.GetDirectoryName(assembly.Location);
                tempDllPath = Path.Combine(assemblyPath, libraryName);
                File.WriteAllBytes(tempDllPath, data);

            }

            LoadLibrary(libraryName);

            return tempDllPath;
        }

        static void Main(string[] args)
        {
            string resourceName = "TestSDLEmbedding.lib.x86.SLD2.dll";
            string libraryName = "SLD2.dll";

            // create and load library from the resource
            string tempDllPath = LoadUnmanagedLibraryFromResource(Assembly.GetExecutingAssembly(),
                resourceName,
                libraryName);
        }
    }
}
