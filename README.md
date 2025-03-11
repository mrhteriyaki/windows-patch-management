# Windows Patch Management Tool

Utility to manage windows updates using command line.

Command List / Help:

```
Patch Installer
Install parameters available:
-update      Include Windows Software Updates.
-drivers     Include Hardware driver updates.
-reboot      Reboot if required when updates completed.
-select x    Install single update where X is the index number available in the 'check' list.

Reporting commands available:
history      Show Windows update history for device.
check        Check for available updates and show list.
health       Writes windows update logs and checks for errors.
```