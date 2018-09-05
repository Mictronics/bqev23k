using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using System.IO;
using System.Xml;
using System.Runtime.InteropServices;

namespace BQEV23K
{
    /// <summary>
    /// This class creates and hold a device configration out of an TI BQStudio .bqz config file.
    /// Each device has its own config file. For BQ40Z80 that is 4800_0_02-bq40z80.bqz.
    /// </summary>
    public class SbsItems
    {
        private int targetAdress = 0;
        private int targetEndianess = 0;
        private int targetMacAddress = 0x44;
        private int mac = 0;
        private List<SbsRegisterItem> sbsRegister;
        private List<SbsCommandItem> sbsCommands;

        #region Properties
        /// <summary>
        /// Get target device address.
        /// </summary>
        public short TargetAdress
        {
            get
            {
                return (short)targetAdress;
            }
        }

        /// <summary>
        /// Get target device endianess.
        /// </summary>
        public int TargetEndianess
        {
            get
            {
                return targetEndianess;
            }
        }

        /// <summary>
        /// Get target device ManufacturerAccessCommand.
        /// </summary>
        public short TargetMacAdress
        {
            get
            {
                return (short)targetMacAddress;
            }
        }

        /// <summary>
        /// Get target device available register list.
        /// </summary>
        public List<SbsRegisterItem> SbsRegister
        {
            get
            {
                return sbsRegister;
            }
        }

        /// <summary>
        /// Get target device available command list.
        /// </summary>
        public List<SbsCommandItem> SbsCommands
        {
            get
            {
                return sbsCommands;
            }
        }
        #endregion

        /// <summary>
        /// Constructor, reading the device configuration file.
        /// </summary>
        /// <param name="bqzPath">Full path to configuration file.</param>
        public SbsItems(string bqzPath)
        {
            string s = string.Empty;
            bool b = false;
            int n = 0;
            XmlNode singleNode;

            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(bqzPath))
                {
                    #region default.sbsx
                    ZipArchiveEntry e = archive.GetEntry(@"sbs/default.sbsx");
                    using(Stream sbsx = e.Open())
                    {
                        XmlDocument sbs = new XmlDocument();
                        sbs.Load(sbsx);

                        #region sbsInfo
                        XmlNodeList info = sbs.DocumentElement.SelectNodes("/sbs/sbsInfo");
                        foreach (XmlNode node in info)
                        {
                            singleNode = node.SelectSingleNode("targetAddress");
                            if (singleNode != null)
                            {
                                s = singleNode.InnerText;
                                if (s != null)
                                {
                                    if (s.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        targetAdress = Convert.ToInt32(s, 16);
                                    }
                                    else
                                    {
                                        int.TryParse(s, out targetAdress);
                                    }
                                }
                            }

                            singleNode = node.SelectSingleNode("targetEndianess");
                            if (singleNode != null)
                            {
                                s = singleNode.InnerText;
                                if (s != null)
                                {
                                    if (s.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        targetEndianess = Convert.ToInt32(s, 16);
                                    }
                                    else
                                    {
                                        int.TryParse(s, out targetEndianess);
                                    }
                                }
                            }

                            singleNode = node.SelectSingleNode("mac");
                            if (singleNode != null)
                            {
                                if (s != null)
                                {
                                    if (s.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        mac = Convert.ToInt32(s, 16);
                                    }
                                    else
                                    {
                                        int.TryParse(s, out mac);
                                    }
                                }
                            }

                            singleNode = node.SelectSingleNode("SMB_NewMacCMD");
                            if (singleNode != null)
                            {
                                s = singleNode.InnerText;
                                if (s != null)
                                {
                                    if (s.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        targetMacAddress = Convert.ToInt32(s, 16);
                                    }
                                    else
                                    {
                                        int.TryParse(s, out targetMacAddress);
                                    }
                                }
                            }
                        }
                        #endregion

                        #region sbsItem
                        XmlNodeList items = sbs.DocumentElement.SelectNodes("/sbs/sbsItem");
                        sbsRegister = new List<SbsRegisterItem>(items.Count);
                        s = null;

                        foreach (XmlNode node in items)
                        {
                            SbsRegisterItem i = new SbsRegisterItem();
                            singleNode = node.SelectSingleNode("caption");
                            if (singleNode != null)
                            {
                                s = singleNode.InnerText;
                                if (s != null)
                                {
                                    i.Caption = s.Trim();
                                }
                            }

                            singleNode = node.SelectSingleNode("logcaption");
                            if (singleNode != null)
                            {
                                s = singleNode.InnerText;
                                if (s != null)
                                {
                                    i.LogCaption = s.Trim();
                                }
                            }

                            singleNode = node.SelectSingleNode("isstatic");
                            if (singleNode != null)
                            {
                                s = singleNode.InnerText;
                                if (s != null)
                                {
                                    bool.TryParse(s, out b);
                                    i.IsStatic = b;
                                }
                            }

                            singleNode = node.SelectSingleNode("isvisible");
                            if (singleNode != null)
                            {
                                s = singleNode.InnerText;
                                if (s != null)
                                {
                                    bool.TryParse(s, out b);
                                    i.IsVisible = b;
                                }
                            }

                            singleNode = node.SelectSingleNode("ismac");
                            if (singleNode != null)
                            {
                                s = singleNode.InnerText;
                                if (s != null)
                                {
                                    bool.TryParse(s, out b);
                                    i.IsMac = b;
                                }
                            }

                            singleNode = node.SelectSingleNode("readstyle");
                            if (singleNode != null)
                            {
                                s = singleNode.InnerText;
                                n = 0;
                                if (s != null)
                                {
                                    if (s.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        n = Convert.ToInt32(s, 16);
                                    }
                                    else
                                    {
                                        int.TryParse(s, out n);
                                    }
                                    i.ReadStyle = n;
                                }
                            }

                            singleNode = node.SelectSingleNode("offsetwithinblock");
                            if (singleNode != null)
                            {
                                s = singleNode.InnerText;
                                n = 0;
                                if (s != null)
                                {
                                    if (s.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        n = Convert.ToInt32(s, 16);
                                    }
                                    else
                                    {
                                        int.TryParse(s, out n);
                                    }
                                    i.OffsetWithinBlock = n;
                                }
                            }

                            singleNode = node.SelectSingleNode("lengthwithinblock");
                            if (singleNode != null)
                            {
                                s = singleNode.InnerText;
                                n = 0;
                                if (s != null)
                                {
                                    if (s.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        n = Convert.ToInt32(s, 16);
                                    }
                                    else
                                    {
                                        int.TryParse(s, out n);
                                    }
                                    i.LengthWithinBlock = n;
                                }
                            }

                            singleNode = node.SelectSingleNode("length");
                            if (singleNode != null)
                            {
                                s = singleNode.InnerText;
                                n = 0;
                                if (s != null)
                                {
                                    if (s.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        n = Convert.ToInt32(s, 16);
                                    }
                                    else
                                    {
                                        int.TryParse(s, out n);
                                    }
                                    i.Length = n;
                                }
                            }

                            singleNode = node.SelectSingleNode("command");
                            if (singleNode != null)
                            {
                                s = singleNode.InnerText;
                                n = 0;
                                if (s != null)
                                {
                                    if (s.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        n = Convert.ToInt32(s, 16);
                                    }
                                    else
                                    {
                                        int.TryParse(s, out n);
                                    }
                                    i.Command = (short)n;
                                }
                            }

                            singleNode = node.SelectSingleNode("readformula");
                            if (singleNode != null)
                            {
                                s = singleNode.InnerText;
                                if (s != null)
                                {
                                    i.ReadFormula = s.Trim();
                                }
                            }

                            singleNode = node.SelectSingleNode("writeformula");
                            if (singleNode != null)
                            {
                                s = singleNode.InnerText;
                                if (s != null)
                                {
                                    i.WriteFormula = s.Trim();
                                }
                            }

                            singleNode = node.SelectSingleNode("writable");
                            if (singleNode != null)
                            {
                                s = singleNode.InnerText;
                                if (s != null)
                                {
                                    bool.TryParse(s, out b);
                                    i.Writable = b;
                                }
                            }

                            singleNode = node.SelectSingleNode("unit");
                            if (singleNode != null)
                            {
                                s = singleNode.InnerText;
                                if (s != null)
                                {
                                    i.Unit = s.Trim();
                                }
                            }

                            singleNode = node.SelectSingleNode("displayformat");
                            if (singleNode != null)
                            {
                                s = singleNode.InnerText;
                                if (s != null)
                                {
                                    i.DisplayFormat = s.Trim();
                                }
                            }

                            singleNode = node.SelectSingleNode("mreadformula");
                            if (singleNode != null)
                            {
                                s = singleNode.InnerText;
                                if (s != null)
                                {
                                    i.MReadFormula = s.Trim();
                                }
                            }

                            singleNode = node.SelectSingleNode("mwriteformula");
                            if (singleNode != null)
                            {
                                s = singleNode.InnerText;
                                if (s != null)
                                {
                                    i.MWriteFormula = s.Trim();
                                }
                            }

                            singleNode = node.SelectSingleNode("munit");
                            if (singleNode != null)
                            {
                                s = singleNode.InnerText;
                                if (s != null)
                                {
                                    i.MUnit = s.Trim();
                                }
                            }

                            singleNode = node.SelectSingleNode("isbitfield");
                            if (singleNode != null)
                            {
                                s = singleNode.InnerText;
                                if (s != null)
                                {
                                    bool.TryParse(s, out b);
                                    i.IsBitfield = b;
                                }
                            }

                            if (i.IsBitfield)
                            {
                                XmlNodeList bits = node.SelectSingleNode("fields").ChildNodes;
                                i.SbsBitItems = new List<SbsBitItem>(bits.Count);
                                foreach (XmlNode bit in bits)
                                {
                                    SbsBitItem bi = new SbsBitItem();
                                    n = 0;
                                    bi.SbsCaption = bi.SbsBitDescription = bit.InnerText;
                                    s = bit.Name.Substring(3);
                                    if(s != null)
                                    {
                                        int.TryParse(s, out n);
                                    }
                                    bi.SbsBitPosition = n;
                                    bi.SbsBitValue = 0;
                                    i.SbsBitItems.Add(bi);
                                }
                            }

                            sbsRegister.Add(i);
                        }
                        #endregion
                    }
                    #endregion

                    #region commands.xml
                    e = archive.GetEntry(@"toolcustomization/commands.xml");
                    using (Stream strm = e.Open())
                    {
                        XmlDocument xml = new XmlDocument();
                        xml.Load(strm);

                        #region Commands
                        XmlNodeList items = xml.DocumentElement.SelectNodes("/commands/commandItem");
                        sbsCommands = new List<SbsCommandItem>(items.Count);
                        s = null;

                        foreach (XmlNode node in items)
                        {
                            SbsCommandItem i = new SbsCommandItem();
                            singleNode = node.SelectSingleNode("caption");
                            if (singleNode != null)
                            {
                                s = singleNode.InnerText;
                                if (s != null)
                                {
                                    i.Caption = s.Trim();
                                }
                            }

                            singleNode = node.SelectSingleNode("description");
                            if (singleNode != null)
                            {
                                s = singleNode.InnerText;
                                if (s != null)
                                {
                                    i.Description = s.Trim();
                                }
                            }

                            singleNode = node.SelectSingleNode("result");
                            if (singleNode != null)
                            {
                                s = singleNode.InnerText;
                                if (s != null)
                                {
                                    bool.TryParse(s, out b);
                                    i.HasResult = b;
                                }
                            }

                            singleNode = node.SelectSingleNode("sealedaccess");
                            if (singleNode != null)
                            {
                                s = singleNode.InnerText;
                                if (s != null)
                                {
                                    bool.TryParse(s, out b);
                                    i.SealedAccess = b;
                                }
                            }

                            singleNode = node.SelectSingleNode("private");
                            if (singleNode != null)
                            {
                                s = singleNode.InnerText;
                                if (s != null)
                                {
                                    bool.TryParse(s, out b);
                                    i.IsPrivate = b;
                                }
                            }

                            singleNode = node.SelectSingleNode("databigendian");
                            if (singleNode != null)
                            {
                                s = singleNode.InnerText;
                                if (s != null)
                                {
                                    bool.TryParse(s, out b);
                                    i.IsBigEndian = b;
                                }
                            }

                            singleNode = node.SelectSingleNode("writestyle");
                            if (singleNode != null)
                            {
                                s = singleNode.InnerText;
                                n = 0;
                                if (s != null)
                                {
                                    if (s.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        n = Convert.ToInt32(s, 16);
                                    }
                                    else
                                    {
                                        int.TryParse(s, out n);
                                    }
                                    i.WriteStyle = n;
                                }
                            }

                            singleNode = node.SelectSingleNode("offsetwithinblock");
                            if (singleNode != null)
                            {
                                s = singleNode.InnerText;
                                n = 0;
                                if (s != null)
                                {
                                    if (s.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        n = Convert.ToInt32(s, 16);
                                    }
                                    else
                                    {
                                        int.TryParse(s, out n);
                                    }
                                    i.OffsetWithinBlock = n;
                                }
                            }

                            singleNode = node.SelectSingleNode("lengthwithinblock");
                            if (singleNode != null)
                            {
                                s = singleNode.InnerText;
                                n = 0;
                                if (s != null)
                                {
                                    if (s.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        n = Convert.ToInt32(s, 16);
                                    }
                                    else
                                    {
                                        int.TryParse(s, out n);
                                    }
                                    i.LengthWithinBlock = n;
                                }
                            }

                            singleNode = node.SelectSingleNode("length");
                            if (singleNode != null)
                            {
                                s = singleNode.InnerText;
                                n = 0;
                                if (s != null)
                                {
                                    if (s.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        n = Convert.ToInt32(s, 16);
                                    }
                                    else
                                    {
                                        int.TryParse(s, out n);
                                    }
                                    i.Length = n;
                                }
                            }

                            singleNode = node.SelectSingleNode("writevalue");
                            if (singleNode != null)
                            {
                                s = singleNode.InnerText;
                                n = 0;
                                if (s != null)
                                {
                                    if (s.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        n = Convert.ToInt32(s, 16);
                                    }
                                    else
                                    {
                                        int.TryParse(s, out n);
                                    }
                                    i.Command = (short)n;
                                }
                            }

                            singleNode = node.SelectSingleNode("delayms");
                            if (singleNode != null)
                            {
                                s = singleNode.InnerText;
                                n = 0;
                                if (s != null)
                                {
                                    if (s.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        n = Convert.ToInt32(s, 16);
                                    }
                                    else
                                    {
                                        int.TryParse(s, out n);
                                    }
                                    i.Delayms = n;
                                }
                            }

                            sbsCommands.Add(i);
                        }
                        #endregion
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Clear all raw, read and display values in registers and commands.
        /// </summary>
        public void ClearAllValues()
        {
            foreach(SbsRegisterItem i in sbsRegister)
            {
                i.RawValue = 0.0;
                i.ReadValue = 0.0;
                i.DisplayValue = string.Empty;
            }

            foreach (SbsCommandItem c in sbsCommands)
            {
                c.ReadValue = 0;
                c.DisplayValue = string.Empty;
            }
        }
    }

    /// <summary>
    /// A single target device command item.
    /// </summary>
    /// <remarks>Properties are created for use in BQStudio.</remarks>
    public class SbsCommandItem
    {
        private string caption = string.Empty;
        private string description = string.Empty;
        private int length = 0;
        private int offsetWithinBlock = 0;
        private int lengthWithinBlock = 0;
        private int command = 0;
        private int writeStyle = 0;
        private bool hasResult = false;
        private bool sealedAccess = false;
        private int delayms = 0;
        private bool isPrivate = false;
        private bool isBigEndian = false;
        private int rawValue = 0;
        private string displayValue = string.Empty;

        #region Properties
        /// <summary>
        /// Get or set command caption.
        /// </summary>
        public string Caption
        {
            get
            {
                return caption;
            }

            set
            {
                caption = value;
            }
        }

        /// <summary>
        /// Get or set command description.
        /// </summary>
        public string Description
        {
            get
            {
                return description;
            }

            set
            {
                description = value;
            }
        }

        /// <summary>
        /// get or set command result block length.
        /// </summary>
        public int Length
        {
            get
            {
                return length;
            }

            set
            {
                length = value;
            }
        }

        /// <summary>
        /// Get or set command result offset within larger data block.
        /// </summary>
        public int OffsetWithinBlock
        {
            get
            {
                return offsetWithinBlock;
            }

            set
            {
                offsetWithinBlock = value;
            }
        }

        /// <summary>
        /// Get or set command result length within larger data block.
        /// </summary>
        public int LengthWithinBlock
        {
            get
            {
                return lengthWithinBlock;
            }

            set
            {
                lengthWithinBlock = value;
            }
        }

        /// <summary>
        /// Get or set command read/write value.
        /// </summary>
        public short Command
        {
            get
            {
                return (short)command;
            }

            set
            {
                command = value;
            }
        }

        /// <summary>
        /// Get or set required command write style.
        /// </summary>
        public int WriteStyle
        {
            get
            {
                return writeStyle;
            }

            set
            {
                writeStyle = value;
            }
        }

        /// <summary>
        /// Get or set true when command returns a result.
        /// </summary>
        public bool HasResult
        {
            get
            {
                return hasResult;
            }

            set
            {
                hasResult = value;
            }
        }

        /// <summary>
        /// Get or set true when command is available in sealed access mode.
        /// </summary>
        public bool SealedAccess
        {
            get
            {
                return sealedAccess;
            }

            set
            {
                sealedAccess = value;
            }
        }

        /// <summary>
        /// Get or set delay after command issue. In milliseconds.
        /// </summary>
        public int Delayms
        {
            get
            {
                return delayms;
            }

            set
            {
                delayms = value;
            }
        }

        /// <summary>
        /// Get or set true when command is a private one.
        /// </summary>
        public bool IsPrivate
        {
            get
            {
                return isPrivate;
            }

            set
            {
                isPrivate = value;
            }
        }

        /// <summary>
        /// Get or set true when command must be send in big endian.
        /// </summary>
        public bool IsBigEndian
        {
            get
            {
                return isBigEndian;
            }

            set
            {
                isBigEndian = value;
            }
        }

        /// <summary>
        /// Get or set commands scaled read value.
        /// </summary>
        public int ReadValue
        {
            get
            {
                return rawValue;
            }

            set
            {
                rawValue = value;
            }
        }

        /// <summary>
        /// Get or set commands formated display value.
        /// </summary>
        public string DisplayValue
        {
            get
            {
                return displayValue;
            }

            set
            {
                displayValue = value;
            }
        }
        #endregion

        [StructLayout(LayoutKind.Explicit)]
        struct UnionArray
        {
            [FieldOffset(0)]
            public Byte[] Bytes;

            [FieldOffset(0)]
            public short[] Shorts;
        }

        /// <summary>
        /// Format commands raw value to string.
        /// </summary>
        public void CalculateDisplayValue()
        {
            displayValue = rawValue.ToString("X4");
        }

        /// <summary>
        /// Get a raw value out of a larger data block.
        /// </summary>
        /// <param name="dataBlock">Data block to read from.</param>
        /// <param name="dataLength">Data block length.</param>
        /// <param name="offset">Raw value offset within data block.</param>
        /// <param name="length">Raw value length.</param>
        public void GetRawValueFromDataBlock(object dataBlock, int dataLength, int offset, int length)
        {
            var union = new UnionArray() { Bytes = (byte[])dataBlock };

            if (offset >= dataLength || (offset + length) >= dataLength)
                return;

            Buffer.BlockCopy((byte[])dataBlock, 2 + offset, union.Bytes, 0, length);

            if(isBigEndian)
            {
                Array.Reverse(union.Bytes, 0, 2);
            }

            rawValue = union.Shorts[0];
        }
    }

    /// <summary>
    /// A single target device register item.
    /// </summary>
    /// <remarks>Properties are created for use in BQStudio.</remarks>
    public class SbsRegisterItem
    {
        private string caption = string.Empty;
        private string logCaption = string.Empty;
        private bool isStatic = false;
        private bool isVisible = false;
        private bool isMac = false;
        private int readStyle = 0;
        private int offsetWithinBlock = 0;
        private int lengthWithinBlock = 0;
        private int length = 0;
        private short command = 0;
        private string readFormula = "x"; 
        private string writeFormula = "x";
        private bool writable = false;
        private string unit = "-";
        private string displayFormat = "d";
        private string mReadFormula = "x";
        private string mWriteFormula = "x";
        private string mUnit = "-";
        private bool isBitfield = false;
        private List<SbsBitItem> sbsBitItems = null;
        private double readValue = 0;
        private double rawValue = 0;
        private string displayValue = string.Empty;

        #region Properties
        /// <summary>
        /// Get or set register item caption.
        /// </summary>
        public string Caption
        {
            get
            {
                return caption;
            }

            set
            {
                caption = value;
            }
        }
        
        /// <summary>
        /// Get or set register item short log caption.
        /// </summary>
        public string LogCaption
        {
            get
            {
                return logCaption;
            }

            set
            {
                logCaption = value;
            }
        }
        
        /// <summary>
        /// Get or set true when register item is a static value.
        /// </summary>
        public bool IsStatic
        {
            get
            {
                return isStatic;
            }

            set
            {
                isStatic = value;
            }
        }

        /// <summary>
        /// Get or set true if visible in register view.
        /// </summary>
        public bool IsVisible
        {
            get
            {
                return isVisible;
            }

            set
            {
                isVisible = value;
            }
        }

        /// <summary>
        /// Get or set true if item requires ManufacturerAccess.
        /// </summary>
        public bool IsMac
        {
            get
            {
                return isMac;
            }

            set
            {
                isMac = value;
            }
        }

        /// <summary>
        /// Get or set the require read style for this register.
        /// </summary>
        public int ReadStyle
        {
            get
            {
                return readStyle;
            }

            set
            {
                readStyle = value;
            }
        }

        /// <summary>
        /// Get or set the register value offset within a larger data block.
        /// </summary>
        public int OffsetWithinBlock
        {
            get
            {
                return offsetWithinBlock;
            }

            set
            {
                offsetWithinBlock = value;
            }
        }

        /// <summary>
        /// Get or set the register value length within a larger data block.
        /// </summary>
        public int LengthWithinBlock
        {
            get
            {
                return lengthWithinBlock;
            }

            set
            {
                lengthWithinBlock = value;
            }
        }

        /// <summary>
        /// Get or set the data block length.
        /// </summary>
        public int Length
        {
            get
            {
                return length;
            }

            set
            {
                length = value;
            }
        }

        /// <summary>
        /// Get or set the registers read/write command.
        /// </summary>
        public short Command
        {
            get
            {
                return command;
            }

            set
            {
                command = value;
            }
        }

        /// <summary>
        /// Get or set the register read value scaling formula.
        /// </summary>
        public string ReadFormula
        {
            get
            {
                return readFormula;
            }

            set
            {
                readFormula = value;
            }
        }

        /// <summary>
        /// Get or set the register write value scaling formula.
        /// </summary>
        public string WriteFormula
        {
            get
            {
                return writeFormula;
            }

            set
            {
                writeFormula = value;
            }
        }

        /// <summary>
        /// Get or set true when register is writable.
        /// </summary>
        public bool Writable
        {
            get
            {
                return writable;
            }

            set
            {
                writable = value;
            }
        }

        /// <summary>
        /// Get or set the register value unit string.
        /// </summary>
        public string Unit
        {
            get
            {
                return unit;
            }

            set
            {
                unit = value;
            }
        }

        /// <summary>
        /// Get or set the register display format string.
        /// </summary>
        public string DisplayFormat
        {
            get
            {
                return displayFormat;
            }

            set
            {
                displayFormat = value;
            }
        }

        public string MReadFormula
        {
            get
            {
                return mReadFormula;
            }

            set
            {
                mReadFormula = value;
            }
        }

        public string MWriteFormula
        {
            get
            {
                return mWriteFormula;
            }

            set
            {
                mWriteFormula = value;
            }
        }

        public string MUnit
        {
            get
            {
                return mUnit;
            }

            set
            {
                mUnit = value;
            }
        }

        /// <summary>
        /// Get or set true when register value is a bit field.
        /// </summary>
        public bool IsBitfield
        {
            get
            {
                return isBitfield;
            }

            set
            {
                isBitfield = value;
            }
        }

        /// <summary>
        /// Get or set list of bit field items.
        /// </summary>
        public List<SbsBitItem> SbsBitItems
        {
            get
            {
                return sbsBitItems;
            }

            set
            {
                sbsBitItems = value;
            }
        }

        /// <summary>
        /// Get or set registers scaled read value.
        /// </summary>
        public double ReadValue
        {
            get
            {
                return readValue;
            }

            set
            {
                readValue = value;
            }
        }

        /// <summary>
        /// Get or set registers raw value.
        /// </summary>
        public double RawValue
        {
            get
            {
                return rawValue;
            }

            set
            {
                rawValue = value;
            }
        }

        /// <summary>
        /// Get or set registers formated display string including unit.
        /// </summary>
        public string DisplayValue
        {
            get
            {
                return displayValue;
            }

            set
            {
                displayValue = value;
            }
        }
        #endregion

        [StructLayout(LayoutKind.Explicit)]
        struct UnionArray
        {
            [FieldOffset(0)]
            public Byte[] Bytes;

            [FieldOffset(0)]
            public short[] Shorts;
        }

        /// <summary>
        /// Calculate bit values when register is a bit field.
        /// </summary>
        public void CalculateBitFields()
        {
            if (isBitfield)
            {
                foreach(SbsBitItem sbsBitItem in sbsBitItems)
                {
                    sbsBitItem.SbsBitValue = (((int)readValue & 1 << sbsBitItem.SbsBitPosition) >> sbsBitItem.SbsBitPosition);
                }
            }
        }

        /// <summary>
        /// Scale and format registers raw value to string also including the unit.
        /// </summary>
        public void CalculateDisplayValue()
        {
            FormulaEval calc = new FormulaEval();
            double res = 0.0;
            int i = 0;
            string s = string.Empty;

            if (displayFormat.StartsWith("u") || displayFormat.StartsWith("d") && !displayFormat.Contains("."))
            { // Integer
                res = calc.Eval(readFormula, rawValue.ToString("F0"));
                readValue = res;
                if (isBitfield)
                {
                    i = (int)res;
                    displayValue = i.ToString("X4");
                }
                else
                {
                    displayValue = res.ToString("F0");
                }
            }
            else if (displayFormat.StartsWith("h"))
            { // Hex
                readValue = rawValue;
                i = (int)rawValue;
                displayValue = i.ToString("X4");
            }
            else if (displayFormat.StartsWith("d") && displayFormat.Contains("."))
            { // Float
                res = calc.Eval(readFormula, rawValue.ToString("F0"));
                i = displayFormat.IndexOf('.');
                i = displayFormat.Length - i - 1;
                res = res / (i * 10);
                readValue = res;
                displayValue = res.ToString("F" + i.ToString());
            }
            else if (displayFormat.CompareTo("z") == 0)
            { // Date
                
            }

            if(unit.CompareTo("-") != 0)
            {
                displayValue += " " + unit;
            }
        }

        /// <summary>
        /// Get a raw value out of a larger data block.
        /// </summary>
        /// <param name="dataBlock">Data block to read from.</param>
        /// <param name="dataLength">Data block length.</param>
        /// <param name="offset">Raw value offset within data block.</param>
        /// <param name="length">Raw value length.</param>
        public void GetRawValueFromDataBlock(object dataBlock, int dataLength, int offset, int length)
        {
            var union = new UnionArray() { Bytes = (byte[])dataBlock };

            if (offset >= dataLength || (offset + length) >= dataLength)
                return;

            if (displayFormat.CompareTo("s") == 0)
            {
                displayValue = Encoding.ASCII.GetString((byte[])dataBlock);
                rawValue = 0;
            }
            else
            {
                Buffer.BlockCopy((byte[])dataBlock, 2 + offset, union.Bytes, 0, length);
                rawValue = union.Shorts[0];
            }
        }

    }

    /// <summary>
    /// This class defines a single Sbs bit item from device configuration.
    /// </summary>
    public class SbsBitItem
    {
        private string sbsCaption = string.Empty;
        private int sbsBitPosition = 0;
        private int sbsBitValue = 0;
        private string sbsBitDescription = string.Empty;

        #region Properties
        /// <summary>
        /// Get or set item caption.
        /// </summary>
        public string SbsCaption
        {
            get
            {
                return sbsCaption;
            }

            set
            {
                sbsCaption = value;
            }
        }
        /// <summary>
        /// Get or set bit position in register value.
        /// </summary>
        public int SbsBitPosition
        {
            get
            {
                return sbsBitPosition;
            }

            set
            {
                sbsBitPosition = value;
            }
        }

        /// <summary>
        /// Get or set the bit value. (0 or 1)
        /// </summary>
        public int SbsBitValue
        {
            get
            {
                return sbsBitValue;
            }

            set
            {
                sbsBitValue = value;
            }
        }

        /// <summary>
        /// Get or set the bit description.
        /// </summary>
        public string SbsBitDescription
        {
            get
            {
                return sbsBitDescription;
            }

            set
            {
                sbsBitDescription = value;
            }
        }
        #endregion
    }
}
