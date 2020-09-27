# periodic-backup
Do you need frequent backups of a few files? Maybe backup of a game save? Then this is the right tool for you!

`> PeriodicBackup <time in minutes> <files to backup separated by semicolon> [<directory to backup>] [<name of output file>]`

When backuping multiple files last parameter `<name of output file>` is required and files are put in a zip archive with this name.

Backups are rolling and timestamp is appended to file name.

Backup is skipped when files hasn't changed since last backup.
