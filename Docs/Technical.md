# Components Of TopNotify

- ## The Daemon
  The part of TopNotify that runs in the background, does all the heavy lifting.
  It's responsible for moving notifications, applying styles to them, and providing a tray icon.

- ## The GUI
  The user interface of TopNotify, allows the user to change settings.
  Runs on IgniteView and React, using npm/vite for development.

- ## IVPluginTopNotify
  A native C++ DLL to provide some helper functions, allows for certain API/library calls that won't work with C#.

- ## TopNotifyHook
  A native C++ DLL that gets injected into certain apps (eg. Microsoft Teams) to provide support for unique notification GUIs.
  The code in this DLL hooks certain windows API functions (eg. SetWindowPos) and reports window handles back to the daemon.

# Finding Window Handles

To operate, TopNotify must find the handle (window ID) of the notification window.
This window is not a normal window, it's a special CoreWindow that cannot be found using simple methods.

- ## Standard Interception
  This interception method will try to find the window based on it's title, "New notification".
  The problem with this method is that the title "New notification" varies by system language, and therefore won't work with all system configurations.

- ## Fallback Interception
  This method of interception is less stable and less performant, but works on a wider range of system languages.
  It works by searching all the CoreWindows and selects the one with a matching size.

- ## Microsoft Teams Interception
  Microsoft Teams uses custom notifications, so finding and modifying them is non-trivial.
  The TopNotifyHook DLL is injected into Microsoft Teams, which then hooks SetWindowPos.
  The overriden SetWindowPos function then sets the position to the chosen position instead of the regular position.

# IPC

The GUI and the hook both need to communicate with the daemon, for which they use websockets.
The daemon runs a websocket server on port 27631, and the clients are the GUI and hook.

The websocket server is used for a variety of actions:

- GUI tells daemon that the settings file has changes.
- Daemon provides the settings file to the hook (because the hook doesn't have access to it directly).
- Hook provides window handles to the daemon, for it to apply styling like transparency.
- Daemon tells the GUI a list of errors to display.
