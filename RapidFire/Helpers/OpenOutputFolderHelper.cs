using System;
using System.IO;
using EnvDTE;
using Microsoft.VisualStudio.Shell;

using Process = System.Diagnostics.Process;

namespace RapidFire
{
    public static class OpenOutputFolderHelper
    {
        /// <summary>
        /// Opens the given <see cref="Project"/> project's output folder in File Explorer.
        /// </summary>
        /// <param name="project">The given project.</param>
        public static void OpenProjectOutputFolder(Project project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var projectDir = Path.GetDirectoryName(project.FullName);
            if (projectDir == null)
            {
                throw new InvalidOperationException("Unable to find project directory");
            }

            var relativeOutputPath = project.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value.ToString();
            var projectOutputPath = Path.Combine(projectDir, relativeOutputPath);

            var assemblyName = project.Properties.Item("AssemblyName").Value.ToString();
            var outputType = (int)project.Properties.Item("OutputType").Value;

            var projectFileOutputPath = Path.Combine(projectOutputPath, $"{assemblyName}{(outputType == 2 ? ".dll" : "exe")}");
            if (File.Exists(projectFileOutputPath))
            {
                Process.Start("explorer.exe", $"/select, \"{projectFileOutputPath}\"");
            }
            else
            {
                Process.Start(projectOutputPath);
            }
        }
    }
}
