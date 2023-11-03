# Multi-Screen Profile Switcher

This is a portable tray application for Windows that allows users to save, remove, and switch between multiple screen profiles with a single click. Each profile includes settings for resolution, position, orientation, and refresh rate, making it an essential tool for anyone who uses multi-monitor setups.

## Features

- **Portable Application**: No installation necessary.
- **Save Screen Profiles**: Easily save the current configuration of your screens as a profile.
- **Remove Screen Profiles**: Delete profiles that are no longer needed.
- **Switch Profiles with One Click**: Seamlessly switch between saved profiles directly from the tray.
- **Auto Open on Startup**: Option to automatically start the application when Windows boots up.
- **Multiple Screen Attributes**: Manage settings including resolution, position, orientation, and refresh rate.

*Note: The current version of the application does not support changing the "scale" settings for screens due to limitations in the Windows API.*

## Prerequisites

Before running this application, please ensure you have the following:
- A Windows operating system with .NET Framework installed.

## Usage

To use the application:

1. **Starting the Application**: Simply double-click the executable to run. If you want the application to open at startup, enable the 'Auto Open at Startup' option within the app settings.

2. **Creating a Profile**: Adjust your screen settings as desired, right-click the tray icon, and select 'Save Current Profile'. Give your profile a name, and it's ready to use.

3. **Switching Profiles**: Right-click the tray icon, navigate to 'Load Profiles' and select one of the saved profiles to apply those settings immediately.

4. **Removing a Profile**: Right-click the tray icon, navigate to 'Remove Profiles', and select the profile you wish to delete.

5. **Reset to Default**: Double-click the tray icon, the profile would be switch as the program just started.

## Download

You can download the latest version of the application from the [Releases](https://github.com/Hocti/Screen-Profile-Switcher) page.

## Contributing

Contributions are welcome! If you have a bug to report or a feature to suggest, please open an issue in this repository.

## License

This project is licensed under the MIT License.

## Acknowledgments

Newtonsoft.Json
ChatGPT