### Tactics Script Tool

Export and import text from `VistualCode` file of `Tactics ADV/Script Engine`.

#### Usage

Export text

```
Tactics_Script_Tool -e "shift_jis" "c:\game\bin\00_common_01_01.bin"
```

Import text

Make sure `c:\game\bin\00_common_01_01.bin` and `c:\game\bin\00_common_01_01.txt` exists.

```
Tactics_Script_Tool -b "shift_jis" "gbk" "c:\game\bin\00_common_01_01.bin"
```

New script will be created in `c:\game\bin\rebuild` directory.
