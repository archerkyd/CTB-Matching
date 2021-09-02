using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using HalconDotNet;

namespace BaslerGIGE_Test1.lib
{
    public class XMLToList
    {
        public XMLToList()
        {
            xmlList = new List<Data1>();
            XS = new string[10];
            xslist = new Data1[10];

        }
        private string[] XS;
        public Data1[] xslist;


        //xml文件中读取出来的值，放在这个list里储存。
        public List<Data1> xmlList;

        public void XMLRead(string path)
        {
            using (XmlReader xread = XmlReader.Create(path))
            {
                while (xread.Read())
                {
                    if (xread.NodeType == XmlNodeType.Element)
                    {
                        if (xread.Name == "MATNR")
                            switch (xread.ReadElementContentAsString())
                            {
                                case "KM796311G11005":
                                    XS[0] = "KM796311G11005";
                                    break;
                                case "KM796311G12005":
                                    XS[1] = "KM796311G12005";
                                    break;
                                case "KM796311G13005":
                                    XS[2]= "KM796311G13005";
                                    break;
                                case "KM796311G14005":
                                    XS[3]= "KM796311G14005";
                                    break;
                                case "KM796311G15005":
                                    XS[4]= "KM796311G15005";
                                    break;
                                case "KM796311G16005":
                                    XS[5]= "KM796311G16005";
                                    break;
                                case "KM796311G17005":
                                    XS[6]= "KM796311G17005";
                                    break;
                                case "KM796311G18005":
                                    XS[7]= "KM796311G18005";
                                    break;
                                case "KM796311G19005":
                                    XS[8] = "KM796311G19005";
                                    break;
                                case "KM796311G10005":
                                    XS[9] = "KM796311G10005";
                                    break;
                            }

                    }
                }
            }
        }

        public void XMLInfo()
        {
            xmlList.Clear();
            for (int i = 0; i < 10; i++)
            {
                if (xslist[i]==null)
                    xslist[i] = new Data1();
                xslist[i].ID = "XS" + (i+1).ToString();
                if (XS[i] != null)
                    xslist[i].Material = XS[i];
                else
                    xslist[i].Material = "无";
                xmlList.Add(xslist[i]);
            }
        }
    }
}

