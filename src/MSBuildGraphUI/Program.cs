using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Build.Locator;

namespace MSBuildGraphUI
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //var msbuildPath = @"D:\src\msbuild.fork\artifacts\bin\bootstrap\net472\MSBuild\Current\Bin";
            //var msbuildPath = @"D:\src\msbuild\artifacts\Debug\bootstrap\net472\MSBuild\15.0\Bin";
            //var msbuildPath = @"D:\src\DomTest\SGEC\src\rps\MSBuild\artifacts\bin\bootstrap\net472\MSBuild\Current\Bin";
            var instances = MSBuildLocator.QueryVisualStudioInstances(VisualStudioInstanceQueryOptions.Default);
            var instance = instances.FirstOrDefault(i => i.Version.Major == 17);
            MSBuildLocator.RegisterInstance(instance);
            //MSBuildLocator.RegisterMSBuildPath(msbuildPath);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MSBuildGraphForm());
        }
    }
}
