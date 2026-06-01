# 📄 Using your autounattend.xml

You generated an answer file. On its own it does nothing - Windows Setup has to
find it while installing. This page shows where to put it.

## 🔍 How Setup finds the file

During an unattended install, Windows Setup automatically searches for a file
named **exactly** `autounattend.xml` in the **root** of every available drive,
including removable USB sticks. The first match wins. So in almost every case the
job is simply: *get `autounattend.xml` onto the root of a drive that is plugged in
while Setup runs.*

Two rules that trip people up:

- The name must be `autounattend.xml` - not `unattend.xml`, not `autounattend (1).xml`.
- It must sit at the drive **root** (`E:\autounattend.xml`), not in a subfolder.

---

## 🔌 Method 1 - Separate USB stick (easiest, no ISO editing)

Works with *any* boot media: a Windows DVD, a Rufus stick, a Ventoy drive, even a
network/PXE boot. You never touch the install media.

1. Take a spare USB stick. Format it **FAT32**.
2. Copy `autounattend.xml` to its **root**.
3. Boot the target PC from your normal Windows install media, with this stick
   plugged in.
4. Setup finds the file and runs unattended.

> Keep the stick small/cheap - only the one file matters. The volume label is
> irrelevant.

---

## 🛠️ Method 2 - On the install USB itself (Rufus)

If you make the install stick with [Rufus](https://rufus.ie):

1. Write the Windows ISO to USB with Rufus as usual.
2. When it finishes, open the USB drive in Explorer.
3. Copy `autounattend.xml` into the **root** of that USB (next to `setup.exe`).
4. Boot from it.

Newer Rufus versions also offer their *own* "remove requirements / create local
account" customisation dialog. That is separate from this file - skip it and use
your `autounattend.xml` instead, or it may inject a competing answer file.

---

## 🧰 Method 3 - Ventoy

Two ways, pick one.

**A. Separate stick (simplest).** Ventoy boots the ISO; Setup still scans every
drive. So just follow **Method 1** - drop `autounattend.xml` on any other FAT32
stick plugged in at install time. Nothing to configure in Ventoy.

**B. Ventoy Auto-Installation plugin (one drive, repeatable).**

1. Copy `autounattend.xml` somewhere on the Ventoy data partition, e.g.
   `\templates\autounattend.xml`.
2. Create or edit `\ventoy\ventoy.json` on that partition:

   ```json
   {
     "auto_install": [
       {
         "image": "/ISO/Win11.iso",
         "template": "/templates/autounattend.xml"
       }
     ]
   }
   ```

   - `image` is the path to your ISO **on the Ventoy drive** (leading `/`).
   - `template` is the path to the answer file on the same drive.
3. Boot Ventoy, pick the ISO. Ventoy injects the template and Setup runs
   unattended.

Use a [JSON validator](https://jsonlint.com) on `ventoy.json` - a stray comma
silently disables the plugin.

---

## 💿 Method 4 - Embed it inside the ISO (one self-contained image)

Useful for archiving or for environments where you can't add a second drive
(some VMs, some PXE setups). You unpack the ISO, add the file, repack it as
bootable.

**GUI (easiest):** open the ISO in a tool that supports bootable rebuild -
**AnyBurn**, **UltraISO**, or **NTLite** - add `autounattend.xml` to the root,
save as a new ISO. Keep the boot record intact.

**Command line (oscdimg, from the Windows ADK):**

1. Install the *Deployment Tools* feature of the
   [Windows ADK](https://learn.microsoft.com/windows-hardware/get-started/adk-install).
2. Extract the ISO contents to a folder, e.g. `C:\iso`.
3. Copy `autounattend.xml` to `C:\iso\autounattend.xml` (root).
4. Repack as a bootable UEFI+BIOS ISO:

   ```powershell
   oscdimg -m -o -u2 -udfver102 `
     -bootdata:2#p0,e,bC:\iso\boot\etfsboot.com#pEF,e,bC:\iso\efi\microsoft\boot\efisys.bin `
     C:\iso C:\Windows-unattended.iso
   ```

   Run it from the *Deployment and Imaging Tools Environment* shortcut so
   `oscdimg` is on the PATH.

---

## 🖥️ Method 5 - Virtual machines

A VM can't have a "second USB stick", so attach the file as a small drive:

- **Easiest:** make a tiny ISO containing just `autounattend.xml` at its root
  (any ISO tool, or `oscdimg -n -m C:\xmlfolder C:\answer.iso`), then mount it as
  a **second** DVD drive alongside the Windows install ISO. Setup scans it.
- Or attach a small **FAT32 virtual disk** with the file at its root.
- Hyper-V / VirtualBox / VMware all work the same way - the file just needs to be
  on *a* drive at install time.

For a one-and-done VM image, **Method 4** (embed in the ISO) avoids the second
drive entirely.

---

## ✅ Verify it worked

- If Setup pauses and asks for language, edition, partitions, or a username - the
  file was **not** found or **not** read. Check the name and that it's at the root.
- Setup logs live at `X:\Windows\Panther\` during install and
  `C:\Windows\Panther\` after. `setupact.log` / `setuperr.log` record which answer
  file was picked up and any parse errors.

---

## ⚠️ Before you boot a real machine

If your config wipes the disk (Disk → automatic / wipe modes), the install will
**erase the target drive with no prompt**. Always dry-run in a virtual machine
first, and double-check which disk Setup will target before deploying to
hardware.
