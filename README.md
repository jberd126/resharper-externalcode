# resharper-externalcode

ExternalCode is a ReSharper plugin that adds support for source files that are not part of a project. 

## What does it do? ##

This plugin allows adding source files to ReSharper's code analysis by specifying folder paths. This is particularly helpful in situations where code is included at build time but not necessarily available during development, such as code generation.

**NOTE:** Exclusions specified in **Code Inspection | Generated Code** still apply to external code.

## How do I get it? ##

You can install directly using ReSharper's extension manager.

From menu bar, select **ReSharper | Manage Extensions**.
In the left pane, select **Online**, search for "externalcode", then click **Install** in the results.

## Configuration ##

ReSharper supports the concept of setting layers where settings can be **personal**, **team-shared**, or **this computer**. By default, settings are **this computer** and apply to any project loaded on the local machine. You will generally want to use **team-shared** project settings for this plugin. This allows project-specific settings while allowing the settings to be added to source control and shared with other users of that project. 

### Adding a Team-Shared, Project-Level Settings Layer ###

1. On the main menu, choose **ReSharper | Manage Options**. The **Settings Layers** dialog box opens.

2. Select the **Solution team-shared** layer and click **Add Layer**, then click **Create Settings File**. The **Save As** dialog box opens.

3. Set the settings file name and path and click **Save**. Save the settings file in project folder and name it the same as the project file and append `.DotSettings` extension.<br/>For example, the project `MySolution\MyProject.csproj` has its settings file named `MySolution\MyProject.csproj.DotSettings`.

### Editing External Code Settings ###

1. On the main menu, choose **ReSharper | Manage Options**. The **Settings Layers** dialog box opens.

2. In the **Settings Layers** dialog, select a layer to be edited then click **Edit Layer**. The Options dialog opens.

3. In the left pane, click **Code Inspection | Include External Sources**.

4. In the right pane, add/edit/remove paths that contain external source files.

5. Click **Save** to save the changes and close the dialog. <br />**Changes do not take affect until project is reloaded.**

Paths may be relative or absolute to files or directories. Relative paths are relative to the directory containing the project.

For more information regarding ReSharper's options sharing, refer to [Managing and Sharing Options](http://www.jetbrains.com/resharper/webhelp/Sharing_Configuration_Options.html).

## Building ##

Building from the source can be done from Visual Studio or the command line. 

### Visual Studio ###
Requirements:

-  Visual Studio 2012 or 2013. 

Procedure:

- From within Visual Studio, open **src\EveningCreek.ReSharper.ExternalSources.sln** solution file and build.

### Command Line ###

Building from the command line also generates the ReSharper extension NuGet package. 

Requirements:

- .NET Framework 3.5

Procedure:

- From a command line, run *build.bat*
