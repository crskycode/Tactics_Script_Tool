using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Tactics_Script_Tool
{
    class Script
    {
        readonly Encoding _encoding;

        byte[] _scriptBuffer;

        int _codeSize;
        
        Dictionary<int, StringRef> _stringRefs;
        List<MessageEntry> _messages;

        public Script(Encoding encoding)
        {
            _encoding = encoding;
        }

        public void Load(string filePath)
        {
            _scriptBuffer = File.ReadAllBytes(filePath);

            if (Path.GetFileName(filePath).ToLower() == "scene.bin")
            {
                throw new Exception("Scene manager virtual code not supported.");
            }

            Parse();
        }

        public void Save(string filePath)
        {
            File.WriteAllBytes(filePath, _scriptBuffer);
        }

        void Parse()
        {
            var stream = new MemoryStream(_scriptBuffer);
            var reader = new BinaryReader(stream);

            Console.WriteLine("Analysing virtual code...");

            _stringRefs = new Dictionary<int, StringRef>();
            _messages = new List<MessageEntry>();

            while (stream.Position < stream.Length)
            {
                var addr = stream.Position;
                var code = reader.ReadInt32();

                // Debug.WriteLine($"{addr:X8}");

                if (code >= 512)
                {
                    if (code != 0xFFED)
                    {
                        stream.Position -= 4;
                    }

                    break;
                }

                switch (code)
                {
                    case 0x00: // end
                    {
                        // stop script execution
                        break;
                    }
                    case 0x01:
                    {
                        reader.ReadInt32();
                        break;
                    }
                    case 0x02:
                    {
                        reader.ReadInt32();
                        break;
                    }
                    case 0x03: // jump
                    {
                        // if (isSceneManager)
                        // {
                        // }

                        reader.ReadInt32(); // address

                        break;
                    }
                    case 0x04: // call ?
                    {
                        reader.ReadInt32(); // address
                        break;
                    }
                    case 0x05: // ret ?
                    {
                        break;
                    }
                    case 0x06:
                    {
                        ReadStringRef(reader);
                        reader.ReadInt32();
                        ReadStringRef(reader);
                        break;
                    }
                    case 0x07:
                    {
                        ReadStringRef(reader);
                        break;
                    }
                    case 0x08:
                    {
                        break;
                    }
                    case 0x09: // switch jump
                    case 0x0A:
                    {
                        reader.ReadInt32(); // table offset
                        var tableSize = reader.ReadInt32(); // table size

                        ReadValue(reader); // index in table

                        // Read table
                        for (var i = 0; i < tableSize; i++)
                        {
                            reader.ReadInt32();
                        }

                        break;
                    }
                    case 0x0B: // load script
                    {
                        ReadStringRef(reader);
                        break;
                    }
                    case 0x0C:
                    {
                        break;
                    }
                    case 0x0D:
                    {
                        break;
                    }
                    case 0x0E: // push
                    {
                        ReadValue(reader);
                        break;
                    }
                    case 0x0F:
                    {
                        // Pop
                        ReadValue(reader);
                        break;
                    }
                    case 0x10: // add
                    {
                        break;
                    }
                    case 0x11: // sub
                    {
                        break;
                    }
                    case 0x12: // mul
                    {
                        break;
                    }
                    case 0x13: // div
                    {
                        break;
                    }
                    case 0x14: // mod
                    {
                        break;
                    }
                    case 0x15: // and
                    {
                        break;
                    }
                    case 0x16: // or
                    {
                        break;
                    }
                    case 0x17: // xor
                    {
                        break;
                    }
                    case 0x18: // neg
                    {
                        break;
                    }
                    case 0x19: // not
                    {
                        break;
                    }
                    case 0x1A: // is zero
                    {
                        break;
                    }
                    case 0x1B: // less
                    {
                        break;
                    }
                    case 0x1C: // less or equal
                    {
                        break;
                    }
                    case 0x1D: // greater
                    {
                        break;
                    }
                    case 0x1E: // greater or equal
                    {
                        break;
                    }
                    case 0x1F: // jnz ( jump if not zero )
                    {
                        // Pop a value from the stack. If the value is non-zero, jump to the specified address.
                        reader.ReadInt32(); // address
                        break;
                    }
                    case 0x20: // jz ( jump if zero )
                    {
                        // Pop a value from the stack. If the value is zero, jump to the specified address.
                        reader.ReadInt32(); // address
                        break;
                    }
                    case 0x21: // pop + jump
                    {
                        break;
                    }
                    case 0x22: // pop + call
                    {
                        break;
                    }
                    case 0x23: // switch + push
                    {
                        reader.ReadInt32(); // table offset
                        var tableSize = reader.ReadInt32(); // table size

                        ReadValue(reader); // index in table

                        // Read table
                        for (var i = 0; i < tableSize; i++)
                        {
                            reader.ReadInt32();
                        }

                        break;
                    }
                    case 0x24:
                    {
                        reader.ReadInt32();
                        ReadStringRef(reader);
                        reader.ReadInt32();
                        reader.ReadInt32();
                        break;
                    }
                    case 0x25:
                    {
                        reader.ReadInt32();
                        break;
                    }
                    case 0x26:
                    {
                        break;
                    }
                    case 0x27:
                    {
                        reader.ReadInt32();
                        ReadStringRef(reader);
                        break;
                    }
                    case 0x28:
                    {
                        reader.ReadInt32();
                        reader.ReadInt32();
                        reader.ReadInt32();
                        break;
                    }
                    case 0x29: // ShellExecute with "open"
                    {
                        ReadStringRef(reader);
                        break;
                    }
                    case 0x2A:
                    {
                        break;
                    }
                    case 0x2B: // jump if file exists
                    {
                        ReadStringRef(reader); // string ( file name )
                        reader.ReadInt32(); // address
                        break;
                    }
                    case 0x2C: // minimize game window
                    {
                        break;
                    }
                    case 0x2D: // method override
                    {
                        // if (isSceneManager)
                        // {
                        //     reader.ReadInt32(); // address
                        // }
                        // else
                        {
                            ReadStringRef(reader); // string ( bgm file name )
                        }

                        break;
                    }
                    case 0x2E: // method override
                    {
                        // if (isSceneManager)
                        // {
                        //     reader.ReadInt32(); // address
                        // }
                        // else
                        {
                            reader.ReadInt32();
                            reader.ReadInt32();
                            reader.ReadInt32();
                            reader.ReadInt32();
                            reader.ReadInt32();
                            reader.ReadInt32();
                            reader.ReadInt32();
                        }

                        break;
                    }
                    case 0x2F:
                    {
                        reader.ReadInt32();
                        break;
                    }
                    case 0x30:
                    {
                        break;
                    }
                    case 0x31:
                    {
                        reader.ReadInt32();
                        reader.ReadInt32();
                        break;
                    }
                    case 0x32:
                    {
                        reader.ReadInt32();
                        reader.ReadInt32();
                        break;
                    }
                    case 0x33:
                    {
                        reader.ReadInt32();
                        ReadStringRef(reader); // string ( se file name )
                        break;
                    }
                    case 0x34:
                    {
                        reader.ReadInt32();
                        reader.ReadInt32();
                        reader.ReadInt32();
                        reader.ReadInt32();
                        break;
                    }
                    case 0x35:
                    {
                        reader.ReadInt32();
                        break;
                    }
                    case 0x36:
                    {
                        reader.ReadInt32();
                        reader.ReadInt32();
                        break;
                    }
                    case 0x37:
                    {
                        reader.ReadInt32();
                        reader.ReadInt32();
                        break;
                    }
                    case 0x38:
                    {
                        break;
                    }
                    case 0x39:
                    {
                        reader.ReadSingle();
                        reader.ReadSingle();
                        break;
                    }
                    case 0x3A:
                    {
                        reader.ReadSingle();
                        reader.ReadSingle();
                        break;
                    }
                    case 0x3B:
                    {
                        reader.ReadSingle();
                        reader.ReadSingle();
                        break;
                    }
                    case 0x3C:
                    {
                        break;
                    }
                    case 0x3D:
                    {
                        break;
                    }
                    case 0x3E: // change stack pointer
                    {
                        reader.ReadInt32();
                        break;
                    }
                    case 0x3F:
                    {
                        reader.ReadInt32();
                        break;
                    }
                    case 0x40:
                    {
                        reader.ReadInt32();
                        break;
                    }
                    case 0x41:
                    {
                        break;
                    }
                    case 0x42:
                    {
                        break;
                    }
                    case 0x43:
                    {
                        reader.ReadInt32();
                        break;
                    }
                    case 0x44:
                    {
                        ReadStringRef(reader); // string ( script file name )
                        break;
                    }
                    case 0x45:
                    {
                        break;
                    }
                    case 0x46:
                    {
                        ReadStringRef(reader); // string ( script file name )
                        break;
                    }
                    case 0x47:
                    {
                        break;
                    }
                    case 0x48:
                    {
                        ReadStringRef(reader); // string ( script file name )
                        break;
                    }
                    case 0x49:
                    {
                        break;
                    }
                    case 0x4A:
                    {
                        reader.ReadInt32();
                        break;
                    }
                    case 0x4B:
                    {
                        break;
                    }
                    case 0x4C:
                    {
                        break;
                    }
                    case 0x4D:
                    {
                        reader.ReadInt32();
                        break;
                    }
                    case 0x4E:
                    {
                        break;
                    }
                    case 0x4F:
                    {
                        break;
                    }
                    case 0x50:
                    {
                        ReadStringRef(reader); // string
                        reader.ReadInt32(); // address

                        break;
                    }
                    case 0x51:
                    {
                        ReadStringRef(reader); // string
                        reader.ReadInt32(); // address
                        break;
                    }
                    case 0x52:
                    {
                        break;
                    }
                    case 0x53:
                    {
                        break;
                    }
                    case 0x54:
                    {
                        ReadStringRef(reader);
                        break;
                    }
                    case 0x55:
                    {
                        break;
                    }
                    case 0x56:
                    {
                        ReadStringRef(reader);
                        break;
                    }
                    case 0x57:
                    {
                        ReadStringRef(reader);
                        break;
                    }
                    case 0x58:
                    {
                        reader.ReadInt32();
                        ReadStringRef(reader);
                        reader.ReadSingle();
                        reader.ReadSingle();
                        reader.ReadSingle();
                        reader.ReadInt32();
                        break;
                    }
                    case 0x59:
                    {
                        reader.ReadInt32();
                        break;
                    }
                    case 0x5A:
                    {
                        reader.ReadInt32();
                        reader.ReadSingle();
                        reader.ReadSingle();
                        reader.ReadSingle();
                        break;
                    }
                    case 0x5B:
                    {
                        ReadStringRef(reader);
                        break;
                    }
                    case 0x5C:
                    {
                        reader.ReadInt32();
                        reader.ReadInt32();
                        reader.ReadInt32();
                        break;
                    }
                    case 0x5D:
                    {
                        reader.ReadInt32();
                        ReadStringRef(reader);
                        break;
                    }
                    case 0x5E:
                    {
                        reader.ReadInt32();
                        ReadStringRef(reader);
                        reader.ReadSingle();
                        reader.ReadSingle();
                        break;
                    }
                    case 0x5F:
                    {
                        reader.ReadInt32();
                        ReadStringRef(reader);
                        ReadStringRef(reader);
                        break;
                    }
                    case 0x60:
                    {
                        reader.ReadInt32();
                        ReadStringRef(reader);
                        ReadStringRef(reader);
                        break;
                    }
                    case 0x61:
                    {
                        reader.ReadInt32();
                        ReadStringRef(reader);
                        ReadStringRef(reader);
                        break;
                    }
                    case 0x62:
                    {
                        reader.ReadInt32();
                        ReadStringRef(reader);
                        ReadStringRef(reader);
                        break;
                    }
                    case 0x63:
                    {
                        ReadStringRef(reader); // string ( cg file name )
                        break;
                    }
                    case 0x64:
                    {
                        ReadStringRef(reader); // string
                        reader.ReadInt32(); // address
                        break;
                    }
                    case 0x65:
                    {
                        ReadStringRef(reader); // string
                        reader.ReadInt32(); // address
                        break;
                    }
                    case 0x66:
                    {
                        reader.ReadInt32();
                        reader.ReadInt32();
                        break;
                    }
                    case 0x67:
                    {
                        reader.ReadInt32();
                        reader.ReadInt32();
                        break;
                    }
                    case 0x68:
                    {
                        reader.ReadInt32();
                        reader.ReadInt32();
                        break;
                    }
                    case 0x69: // text
                    {
                        var a1 = Convert.ToInt32(stream.Position);
                        var s1 = reader.ReadInt32(); // string
                        var a2 = Convert.ToInt32(stream.Position);
                        var s2 = reader.ReadInt32(); // string
                        var a3 = Convert.ToInt32(stream.Position);
                        var s3 = reader.ReadInt32(); // string
                        var a4 = Convert.ToInt32(stream.Position);
                        var s4 = reader.ReadInt32(); // string

                        var r1 = new StringRef { Ref = a1, Addr = s1 };
                        var r2 = new StringRef { Ref = a2, Addr = s2 };
                        var r3 = new StringRef { Ref = a3, Addr = s3 };
                        var r4 = new StringRef { Ref = a4, Addr = s4 };

                        _stringRefs.Add(r1.Ref, r1);
                        _stringRefs.Add(r2.Ref, r2);
                        _stringRefs.Add(r3.Ref, r3);
                        _stringRefs.Add(r4.Ref, r4);

                        _messages.Add(new MessageEntry
                        {
                            displayCharNameRef = r1,
                            charNameRef = r2,
                            voideRef = r3,
                            messageRef = r4
                        });

                        break;
                    }
                    case 0x6A:
                    {
                        break;
                    }
                    case 0x6B:
                    {
                        ReadStringRef(reader);
                        break;
                    }
                    case 0x6C:
                    {
                        reader.ReadInt32();
                        break;
                    }
                    case 0x6D:
                    {
                        break;
                    }
                    case 0x6E:
                    {
                        reader.ReadInt32();
                        reader.ReadInt32(); // address
                        break;
                    }
                    case 0x6F:
                    {
                        reader.ReadInt32();
                        reader.ReadInt32(); // address
                        break;
                    }
                    case 0x70:
                    {
                        reader.ReadInt32();
                        ReadStringRef(reader); // string
                        reader.ReadInt32();
                        break;
                    }
                    case 0x71:
                    {
                        reader.ReadInt32();
                        break;
                    }
                    case 0x72:
                    {
                        reader.ReadInt32();
                        reader.ReadInt32();
                        break;
                    }
                    case 0x73:
                    {
                        reader.ReadInt32();
                        break;
                    }
                    case 0x74:
                    {
                        reader.ReadInt32();
                        break;
                    }
                    case 0x75:
                    {
                        ReadStringRef(reader);
                        ReadStringRef(reader);
                        break;
                    }
                    case 0x76:
                    {
                        break;
                    }
                    case 0x77:
                    {
                        reader.ReadInt32();
                        reader.ReadInt32();
                        break;
                    }
                    case 0x78:
                    {
                        reader.ReadInt32();
                        reader.ReadInt32();
                        break;
                    }
                    case 0x79:
                    {
                        reader.ReadInt32();
                        reader.ReadInt32();
                        break;
                    }
                    case 0x7A:
                    {
                        reader.ReadInt32();
                        reader.ReadSingle();
                        reader.ReadSingle();
                        reader.ReadSingle();
                        break;
                    }
                    case 0x7B:
                    {
                        reader.ReadInt32();
                        reader.ReadSingle();
                        reader.ReadSingle();
                        reader.ReadSingle();
                        break;
                    }
                    case 0x7C:
                    {
                        reader.ReadInt32();
                        reader.ReadSingle();
                        reader.ReadSingle();
                        reader.ReadSingle();
                        break;
                    }
                    case 0x7D:
                    {
                        reader.ReadInt32();
                        reader.ReadSingle();
                        reader.ReadSingle();
                        reader.ReadSingle();
                        reader.ReadSingle();
                        break;
                    }
                    case 0x7E:
                    {
                        reader.ReadInt32();
                        reader.ReadInt32();
                        break;
                    }
                    case 0x7F:
                    {
                        reader.ReadInt32();
                        reader.ReadInt32();
                        break;
                    }
                    case 0x80:
                    {
                        break;
                    }
                    case 0x81:
                    {
                        reader.ReadInt32();
                        break;
                    }
                    case 0x82:
                    {
                        reader.ReadSingle();
                        break;
                    }
                    case 0x83:
                    {
                        ReadStringRef(reader);
                        break;
                    }
                    case 0x84:
                    {
                        break;
                    }
                    case 0x85:
                    {
                        reader.ReadInt32();
                        ReadStringRef(reader);
                        break;
                    }
                    case 0x86:
                    {
                        reader.ReadInt32();
                        break;
                    }
                    case 0x87:
                    {
                        ReadStringRef(reader);
                        break;
                    }
                    case 0x88:
                    {
                        reader.ReadSingle();
                        reader.ReadSingle();
                        reader.ReadSingle();
                        reader.ReadInt32();
                        reader.ReadInt32();
                        reader.ReadSingle();
                        reader.ReadSingle();
                        reader.ReadSingle();
                        reader.ReadSingle();
                        for (var i = 0x38C; i < 0x39C; i += 4)
                        {
                            reader.ReadInt32();
                        }
                        break;
                    }
                    case 0x89:
                    {
                        break;
                    }
                    case 0x8A:
                    {
                        reader.ReadInt32();
                        reader.ReadInt32();
                        break;
                    }
                    case 0x8B:
                    {
                        ReadStringRef(reader);
                        break;
                    }
                    case 0x8C:
                    {
                        ReadStringRef(reader);
                        break;
                    }
                    case 0x8D:
                    {
                        reader.ReadInt32();
                        reader.ReadInt32();
                        break;
                    }
                    case 0x8E:
                    {
                        reader.ReadInt32();
                        break;
                    }
                    case 0x8F:
                    {
                        break;
                    }
                    case 0x90:
                    {
                        reader.ReadInt32();
                        reader.ReadInt32();
                        break;
                    }
                    case 0x91:
                    {
                        reader.ReadInt32();
                        break;
                    }
                    case 0x92:
                    {
                        break;
                    }
                    default:
                    {
                        throw new Exception($"{addr:X8} unknown opcode {code:X2}");
                    }
                }
            }

            _codeSize = Convert.ToInt32(stream.Position);

            Console.WriteLine("Read string pool");

            var stringPoolAddr = stream.Position;

            var stringPool = new HashSet<string>();

            while (stream.Position < stream.Length)
            {
                var s = reader.ReadAnsiString(_encoding);
                stringPool.Add(s);
            }

            Console.WriteLine("Read string through references");

            var stringByRef = new HashSet<string>();

            foreach (var r in _stringRefs)
            {
                if (r.Value.Addr < stringPoolAddr || r.Value.Addr >= stream.Length)
                {
                    throw new Exception("string offset out of range");
                }

                stream.Position = r.Value.Addr;

                var s = reader.ReadAnsiString(_encoding);

                r.Value.String = s;

                stringByRef.Add(s);
            }

            Console.WriteLine("Check string references");

            foreach (var s in stringPool)
            {
                if (s.Length == 0)
                {
                    continue;
                }

                if (!stringByRef.Contains(s))
                {
                    throw new Exception("String reference missing.");
                }
            }

            // Done
        }

        void ReadStringRef(BinaryReader reader)
        {
            var r1 = Convert.ToInt32(reader.BaseStream.Position);
            var a1 = reader.ReadInt32();

            _stringRefs.Add(r1, new StringRef {
                Ref = r1,
                Addr = a1
            });
        }

        void ReadValue(BinaryReader reader)
        {
            var source = reader.ReadInt32();

            switch (source)
            {
                case 0: // immediate
                {
                    reader.ReadInt32();
                    break;
                }
                case 1: // offset
                {
                    reader.ReadInt32();
                    break;
                }
                case 2: // unknow
                {
                    reader.ReadInt32();
                    break;
                }
                case 3: // unknow
                {
                    reader.ReadInt32();
                    break;
                }
                default:
                {
                    throw new Exception("unknown value source");
                }
            }
        }

        public void ExportText(string filePath, bool exportAll)
        {
            if (_stringRefs == null || _stringRefs.Count == 0)
            {
                Console.WriteLine("No string to export.");
                return;
            }

            if (_messages == null || _messages.Count == 0)
            {
                Console.WriteLine("No message to export.");
                return;
            }

            using var writer = File.CreateText(filePath);

            if (exportAll)
            {
                // All strings

                foreach (var r in _stringRefs)
                {
                    if (!string.IsNullOrEmpty(r.Value.String))
                    {
                        writer.WriteLine($"◇{r.Value.Ref:X8}◇{EscapeString(r.Value.String)}");
                        writer.WriteLine($"◆{r.Value.Ref:X8}◆{EscapeString(r.Value.String)}");
                        writer.WriteLine();
                    }
                }
            }
            else
            {
                // Message only

                foreach (var msg in _messages)
                {
                    if (!string.IsNullOrEmpty(msg.displayCharNameRef.String))
                    {
                        writer.WriteLine($"◇{msg.displayCharNameRef.Ref:X8}◇{EscapeString(msg.displayCharNameRef.String)}");
                        writer.WriteLine($"◆{msg.displayCharNameRef.Ref:X8}◆{EscapeString(msg.displayCharNameRef.String)}");
                        writer.WriteLine();
                    }

                    if (!string.IsNullOrEmpty(msg.messageRef.String))
                    {
                        writer.WriteLine($"◇{msg.messageRef.Ref:X8}◇{EscapeString(msg.messageRef.String)}");
                        writer.WriteLine($"◆{msg.messageRef.Ref:X8}◆{EscapeString(msg.messageRef.String)}");
                        writer.WriteLine();
                    }
                }
            }

            writer.Flush();

            Console.WriteLine("Done");
        }

        public void ImportText(string filePath, Encoding encoding)
        {
            if (_stringRefs == null || _stringRefs.Count == 0)
            {
                return;
            }

            Console.WriteLine("Loading translation...");

            var strCount = 0;

            using (var reader = File.OpenText(filePath))
            {
                var _lineNo = 0;

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var lineNo = _lineNo++;

                    if (line.Length == 0 || line[0] != '◆')
                    {
                        continue;
                    }

                    var m = Regex.Match(line, @"◆(\w+)◆(.+)$");

                    if (!m.Success || m.Groups.Count != 3)
                    {
                        throw new Exception($"Bad format at line: {lineNo}");
                    }

                    var strIndex = m.Groups[1].Value;
                    var strVal = m.Groups[2].Value;

                    var index = int.Parse(strIndex, NumberStyles.HexNumber);

                    if (!_stringRefs.TryGetValue(index, out StringRef strRef))
                    {
                        throw new Exception($"String ID is not in the script. line: {lineNo}");
                    }

                    strRef.String = UnescapeString(strVal);

                    strCount++;
                }
            }

            Console.WriteLine($"{strCount} translated string loaded");

            Console.WriteLine("Rebuilding script...");

            var stream = new MemoryStream(4 * 1024 * 1024);
            var writer = new BinaryWriter(stream);

            Console.WriteLine("Write virtual code");

            writer.Write(_scriptBuffer, 0, _codeSize);

            Console.WriteLine("Write string");

            writer.Write(0xFFED);

            var strPool = new Dictionary<string, int>();

            foreach (var r in _stringRefs)
            {
                if (strPool.TryGetValue(r.Value.String, out int strAddr))
                {
                    r.Value.Addr = strAddr;
                }
                else
                {
                    strAddr = Convert.ToInt32(stream.Position);

                    strPool.Add(r.Value.String, strAddr);

                    var bytes = encoding.GetBytes(r.Value.String);

                    writer.Write(bytes);
                    writer.Write((byte)0);

                    writer.WriteAlignmentBytes(4);

                    r.Value.Addr = strAddr;
                }
            }

            Console.WriteLine("Update string references");

            foreach (var r in _stringRefs)
            {
                stream.Position = r.Value.Ref;
                writer.Write(r.Value.Addr);
            }

            _scriptBuffer = stream.ToArray();

            Console.WriteLine("Done");
        }

        static string EscapeString(string s)
        {
            s = s.Replace("\r", "\\r");
            s = s.Replace("\n", "\\n");
            return s;
        }

        static string UnescapeString(string s)
        {
            s = s.Replace("\\r", "\r");
            s = s.Replace("\\n", "\n");
            return s;
        }

        class StringRef
        {
            public int Ref;
            public int Addr;
            public string String;
        }

        class MessageEntry
        {
            public StringRef displayCharNameRef;
            public StringRef charNameRef;
            public StringRef voideRef;
            public StringRef messageRef;
        }
    }
}
