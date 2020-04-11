﻿using System;
using System.ComponentModel.Design;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace RapidFire
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class OpenOutputFolderSolutionCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("0679de00-1d16-42b6-8029-1fc3197c99d8");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// The DTE service.
        /// </summary>
        private readonly DTE2 dteService;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenOutputFolderSolutionCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private OpenOutputFolderSolutionCommand(AsyncPackage package, OleMenuCommandService commandService, DTE2 dteService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            this.dteService = dteService ?? throw new ArgumentNullException(nameof(dteService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static OpenOutputFolderSolutionCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in OpenOutputPathCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            var dteService = await package.GetServiceAsync(typeof(DTE)) as DTE2;

            Instance = new OpenOutputFolderSolutionCommand(package, commandService, dteService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (this.dteService?.SelectedItems.Count != 1)
            {
                return;
            }

            var selectedItem = this.dteService.SelectedItems.Item(1);
            if (selectedItem.Project != null)
            {
                OpenOutputFolderHelper.OpenProjectOutputFolder(selectedItem.Project);
            }
            else if (selectedItem.ProjectItem != null)
            {
                OpenOutputFolderHelper.OpenProjectOutputFolder(selectedItem.ProjectItem.ContainingProject);
            }
        }
    }
}
