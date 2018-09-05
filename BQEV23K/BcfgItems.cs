using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BQEV23K
{
    public class BcfgItems
    {
        private int bcfgBaseAddr = 0;
        private List<BcfgItem> bcfgItems;

        #region Properties
        public int DataflashBaseAddr
        {
            get
            {
                return bcfgBaseAddr;
            }

            set
            {
                bcfgBaseAddr = value;
            }
        }

        public List<BcfgItem> DataflashItems
        {
            get
            {
                return bcfgItems;
            }

            set
            {
                bcfgItems = value;
            }
        }
        #endregion

        /// <summary>
        /// Constructor, reading the dataflash configuration file.
        /// </summary>
        /// <param name="bcfgxPath">Full path to dataflash configuration file.</param>
        public BcfgItems(string bcfgxPath)
        {
            string s = string.Empty;
            bool b = false;
            int n = 0;
            double d = 0;
            XmlNode singleNode;

            try
            {
                using (FileStream sbcfgx = new FileStream(bcfgxPath, FileMode.Open))
                {
                    XmlDocument bcfgx = new XmlDocument();
                    bcfgx.Load(sbcfgx);

                    #region bcfgInfo
                    XmlNodeList info = bcfgx.DocumentElement.SelectNodes("/bcfg/bcfgInfo");
                    foreach (XmlNode node in info)
                    {

                        singleNode = node.SelectSingleNode("bcfgxBaseAddr");
                        if (singleNode != null)
                        {
                            s = singleNode.InnerText;
                            if (s != null)
                            {
                                if (s.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    bcfgBaseAddr = Convert.ToInt32(s, 16);
                                }
                                else
                                {
                                    int.TryParse(s, out bcfgBaseAddr);
                                }
                            }
                        }
                    }
                    #endregion

                    #region bcfgItem
                    XmlNodeList items = bcfgx.DocumentElement.SelectNodes("/bcfg/bcfgItem");
                    bcfgItems = new List<BcfgItem>(items.Count);
                    s = null;

                    foreach (XmlNode node in items)
                    {
                        BcfgItem i = new BcfgItem();
                        singleNode = node.SelectSingleNode("class");
                        if (singleNode != null)
                        {
                            s = singleNode.InnerText;
                            if (s != null)
                            {
                                i.Class = s.Trim();
                            }
                        }

                        singleNode = node.SelectSingleNode("subclass");
                        if (singleNode != null)
                        {
                            s = singleNode.InnerText;
                            if (s != null)
                            {
                                i.Subclass = s.Trim();
                            }
                        }

                        singleNode = node.SelectSingleNode("caption");
                        if (singleNode != null)
                        {
                            s = singleNode.InnerText;
                            if (s != null)
                            {
                                i.Caption = s.Trim();
                            }
                        }

                        singleNode = node.SelectSingleNode("offset");
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
                                i.OffsetWithinDataflash = n;
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
                                i.LengthWithinDataflash = n;
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

                        singleNode = node.SelectSingleNode("unit");
                        if (singleNode != null)
                        {
                            s = singleNode.InnerText;
                            if (s != null)
                            {
                                i.Unit = s.Trim();
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

                        singleNode = node.SelectSingleNode("displaytype");
                        if (singleNode != null)
                        {
                            s = singleNode.InnerText;
                            if (s != null)
                            {
                                i.DisplayFormat = s.Trim().ToLower();
                            }
                        }

                        singleNode = node.SelectSingleNode("default");
                        if (singleNode != null)
                        {
                            s = singleNode.InnerText;
                            d = 0.0;
                            if (s != null)
                            {
                                if (s.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    d = Convert.ToInt32(s, 16);
                                }
                                else
                                {
                                    double.TryParse(s, out d);
                                }
                                i.DefaultValue = d;
                            }
                        }

                        singleNode = node.SelectSingleNode("min");
                        if (singleNode != null)
                        {
                            s = singleNode.InnerText;
                            d = 0.0;
                            if (s != null)
                            {
                                if (s.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    d = Convert.ToInt32(s, 16);
                                }
                                else
                                {
                                    double.TryParse(s, out d);
                                }
                                i.MinValue = d;
                            }
                        }

                        singleNode = node.SelectSingleNode("max");
                        if (singleNode != null)
                        {
                            s = singleNode.InnerText;
                            d = 0.0;
                            if (s != null)
                            {
                                if (s.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    d = Convert.ToInt32(s, 16);
                                }
                                else
                                {
                                    double.TryParse(s, out d);
                                }
                                i.MaxValue = d;
                            }
                        }

                        singleNode = node.SelectSingleNode("datatype");
                        if (singleNode != null)
                        {
                            s = singleNode.InnerText;
                            if (s != null)
                            {
                                i.DataType = s.Trim();
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
                                if (s != null)
                                {
                                    int.TryParse(s, out n);
                                }
                                bi.SbsBitPosition = n;
                                bi.SbsBitValue = 0;
                                i.SbsBitItems.Add(bi);
                            }
                        }
                        bcfgItems.Add(i);
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void CreateDataflashModel(object dataBlock, int dataLen)
        {
            if (bcfgItems == null)
                return;

            byte[] b;
            try
            {
                foreach (BcfgItem i in bcfgItems)
                {
                    switch (i.DataType)
                    {
                        case "B":
                            b = new byte[sizeof(UInt32)];
                            Buffer.BlockCopy((byte[])dataBlock, i.OffsetWithinDataflash, b, 0, i.LengthWithinDataflash);
                            i.RawValue = BitConverter.ToUInt32(b, 0);
                            break;
                        case "I":
                            b = new byte[sizeof(Int32)];
                            Buffer.BlockCopy((byte[])dataBlock, i.OffsetWithinDataflash, b, 0, i.LengthWithinDataflash);
                            i.RawValue = BitConverter.ToInt32(b, 0);
                            break;
                        case "U":
                            b = new byte[sizeof(UInt32)];
                            Buffer.BlockCopy((byte[])dataBlock, i.OffsetWithinDataflash, b, 0, i.LengthWithinDataflash);
                            i.RawValue = BitConverter.ToUInt32(b, 0);
                            break;
                        case "F":
                            b = new byte[sizeof(float)];
                            Buffer.BlockCopy((byte[])dataBlock, i.OffsetWithinDataflash, b, 0, i.LengthWithinDataflash);
                            i.RawValue = BitConverter.ToSingle(b, 0);
                            break;
                        case "S":
                            b = new byte[i.LengthWithinDataflash];
                            Buffer.BlockCopy((byte[])dataBlock, i.OffsetWithinDataflash, b, 0, i.LengthWithinDataflash);
                            i.DisplayValue = BitConverter.ToString(b);
                            break;
                        default:
                            Console.WriteLine(i.DataType);
                            break;
                    }

                    i.CalculateBitFields();
                    i.CalculateDisplayValue();
                }
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    public class BcfgItem
    {
        private string _class = string.Empty;
        private string subclass = string.Empty;
        private string caption = string.Empty;
        private int offsetWithinDataflash = 0;
        private int lengthWithinDataflash = 0;
        private string readFormula = "x";
        private string writeFormula = "x";
        private string unit = "-";
        private string displayFormat = "d";
        private string dataType = "I";
        private string mUnit = "-";
        private bool isBitfield = false;
        private List<SbsBitItem> bcfgBitItems = null;
        private double readValue = 0.0;
        private double rawValue = 0.0;
        private double defaultValue = 0.0;
        private double minValue = 0.0;
        private double maxValue = 0.0;
        private string displayValue = string.Empty;

        #region Properties
        /// <summary>
        /// Get or set register item caption.
        /// </summary>
        public string Class
        {
            get
            {
                return _class;
            }

            set
            {
                _class = value;
            }
        }
        /// <summary>
        /// Get or set register item caption.
        /// </summary>
        public string Subclass
        {
            get
            {
                return subclass;
            }

            set
            {
                subclass = value;
            }
        }

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
        /// Get or set the register value offset within a larger data block.
        /// </summary>
        public int OffsetWithinDataflash
        {
            get
            {
                return offsetWithinDataflash;
            }

            set
            {
                offsetWithinDataflash = value;
            }
        }

        /// <summary>
        /// Get or set the register value length within a larger data block.
        /// </summary>
        public int LengthWithinDataflash
        {
            get
            {
                return lengthWithinDataflash;
            }

            set
            {
                lengthWithinDataflash = value;
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
                return bcfgBitItems;
            }

            set
            {
                bcfgBitItems = value;
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

        public string DataType
        {
            get
            {
                return dataType;
            }

            set
            {
                dataType = value;
            }
        }

        public double DefaultValue
        {
            get
            {
                return defaultValue;
            }

            set
            {
                defaultValue = value;
            }
        }

        public double MinValue
        {
            get
            {
                return minValue;
            }

            set
            {
                minValue = value;
            }
        }

        public double MaxValue
        {
            get
            {
                return maxValue;
            }

            set
            {
                maxValue = value;
            }
        }
        #endregion

        /// <summary>
        /// Calculate bit values when register is a bit field.
        /// </summary>
        public void CalculateBitFields()
        {
            if (isBitfield)
            {
                foreach (SbsBitItem sbsBitItem in bcfgBitItems)
                {
                    sbsBitItem.SbsBitValue = (((int)rawValue & 1 << sbsBitItem.SbsBitPosition) >> sbsBitItem.SbsBitPosition);
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
            else if (displayFormat.StartsWith("d") || displayFormat.StartsWith("f") && displayFormat.Contains("."))
            { // Float
                res = calc.Eval(readFormula, rawValue.ToString("F0"));
                i = displayFormat.IndexOf('.');
                i = displayFormat.Length - i - 1;
                res = res / (i * 10);
                readValue = res;
                displayValue = res.ToString("F" + i.ToString());
            }

            if (unit.CompareTo("-") != 0)
            {
                displayValue += " " + unit;
            }
        }

    }
}
