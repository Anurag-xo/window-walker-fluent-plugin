# Window Walker (Fluent) for PowerToys Run

A powerful and fluent replacement for the classic Alt-Tab, built for [PowerToys Run](https://docs.microsoft.com/en-us/windows/powertoys/run). This plugin lets you search for and switch to any open window with just a few keystrokes. It's a modern take on the original [Window Walker](http://www.windowwalker.com/) utility.

## ✨ Features

*   **Fuzzy Search:** Quickly find the window you're looking for, even if you only remember part of its title.
*   **Live Previews:** See a live preview of the window before you switch. (Feature to be implemented)
*   **Seamless Integration:** Looks and feels like a native part of the PowerToys Run experience.
*   **Efficient:** Written in C# with a focus on performance to ensure it's always fast and responsive.
*   **Open Source:** Licensed under the MIT license.

## 🚀 Installation

1.  Ensure you have the latest version of [PowerToys](https://github.com/microsoft/PowerToys/releases/latest) installed.
2.  Download the latest release of the Window Walker Fluent plugin from the [Releases](https://github.com/user/repo/releases) page.
3.  Extract the contents of the `.zip` file into your PowerToys Run plugins directory (usually `%LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins`).
4.  Restart PowerToys.
5.  Open the PowerToys settings, navigate to "PowerToys Run", and make sure the "Window Walker Fluent" plugin is enabled.

## ⌨️ How to Use

1.  Open PowerToys Run (default shortcut is `Alt + Space`).
2.  Type the activation keyword (default is `w>`) followed by your search query.
3.  For example: `w> outlook` will show all open windows with "Outlook" in the title.
4.  Select the desired window from the list and press `Enter` to switch to it.

## 🛠️ For Developers

This project is built with C# and requires Visual Studio with the .NET desktop development workload.

### Build Steps

1.  Clone the repository:
    ```bash
    git clone https://github.com/your-username/window-walker-fluent-plugin.git
    ```
2.  Open `WindowWalker.Fluent.Plugin.sln` in Visual Studio.
3.  Build the solution. The output will be generated in the `bin/` directory.

## 📄 License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

---

*Keywords for discoverability: PowerToys, PowerToys Run, Window Walker, Fluent, Plugin, Switch Windows, Alt-Tab, Window Management, C#, .NET*
