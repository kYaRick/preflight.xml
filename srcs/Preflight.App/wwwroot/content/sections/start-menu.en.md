# Start Menu

## What this section changes
Controls how the **Start menu** and **taskbar** look on first boot — search box,
pinned icons, widgets, tiles (Windows 10) and Start pins (Windows 11).

## Taskbar search box variants
Windows gives four choices for how the search element is rendered next to Start.
Pick the one that matches your install:

![Full search box](content/images/taskbar-search-box.png)
*Full box — the default on fresh Windows 11 installs.*

![Search icon + label](content/images/taskbar-search-label.png)
*Icon + label — a compact variant that still shouts "search here".*

![Search icon only](content/images/taskbar-search-icon.png)
*Icon only — the Windows 10 default.*

![Search hidden](content/images/taskbar-search-hide.png)
*Hidden — zero taskbar space used; search is still reachable via `Win` key.*

## Why you might use it
- Keep the Start/taskbar chrome consistent across many devices.
- Replace Microsoft's pinned apps with your own `taskbar-layout.xml` / `start2.json`.
- Hide Widgets / Task View / Bing results without a post-install tweak script.

## Notes and risks
- Start Tiles (the Windows 10 XML) are ignored on Windows 11.
- Start Pins (Win 11 `start2.json`) ignore Windows 10 builds.
- "Remove all" empties the layout but users can re-pin anything after first logon.

## External references
- [Microsoft Learn: LayoutModificationTemplate schema](https://learn.microsoft.com/windows/configuration/customize-and-export-start-layout)
- [Microsoft Learn: Customize the Windows 11 Start layout](https://learn.microsoft.com/windows/configuration/customize-start-menu-layout-windows-11)
