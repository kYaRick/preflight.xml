## What this section controls

Windows Setup reads three locale-related settings from `autounattend.xml` and applies them
at different points during installation:

- **Display language** &mdash; the language of the Windows UI itself (Settings, File Explorer,
  notifications). Emitted inside `Microsoft-Windows-International-Core-WinPE` so the Setup UI
  follows the user's choice, and again in `Microsoft-Windows-International-Core` for the final
  user session.
- **Input language** &mdash; the keyboard layout active on first boot. Listed separately
  because a user in Germany often prefers a US-English display language with a German keyboard.
- **Home location** &mdash; the country / region code used for formatting (dates, currency, units)
  and for region-aware features like the Microsoft Store catalogue. Maps to a numeric "geo ID"
  value in the XML.

All three have opinionated defaults that match schneegans' generator output, so the most common
case &mdash; English UI, US keyboard, US region &mdash; works without any edits.

## Tips

- The **display language** determines which language packs Windows expects to find in the image.
  Picking a language the install media doesn't ship forces a fallback to English.
- The **input language** accepts a simple BCP-47 tag (`en-US`, `uk-UA`) for common cases; more
  elaborate setups (primary + secondary keyboard, custom KLID) are reachable via the
  `InputLocale` string syntax &mdash; see the Microsoft reference linked on each option below.
