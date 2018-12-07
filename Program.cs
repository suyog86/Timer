using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using System.Xml;

namespace Timer
{
    class Program
    {
        System.Timers.Timer testTimer = new System.Timers.Timer();
        List<string> arrPrinters = new List<string>();
        Dictionary<string, int> perfmonValues = new Dictionary<string, int>();
        bool bDeletePrevious = false;
	//adding another comment
	//adding sample comment
        static void Main(string[] args)
        {
            Program test = new Program();
            test.testTimer.Enabled = true;
            test.testTimer.Interval = 10000;    // Execute timer every
            // five seconds
            test.testTimer.Elapsed += new
               System.Timers.ElapsedEventHandler(test.testTimer_Elapsed);


            test.GetPrinterData();

            // Sit and wait so we can see some output
            Console.ReadLine();
        }
        private void testTimer_Elapsed(object sender,
         System.Timers.ElapsedEventArgs e)
        {
            System.Console.WriteLine("myTimer event occurred");

            if (bDeletePrevious == true)
            {
                DeletePreviousDaysRecord();
                //DeletePreviousXMLRecord();
            }

            foreach (string str in arrPrinters)
            {
                using (PerformanceCounter pc = new PerformanceCounter("Print Queue", "Total Pages Printed", str))
                {
                    SaveXMLPrinterData(str, pc.NextValue().ToString());
                }
            }
        }

        public void GetPrinterData()
        {
            //get Printers
            foreach (string printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
            {
                arrPrinters.Add(printer.ToString());
                perfmonValues.Add(printer.ToString(), 0);
            }

            //perfmonValues = new int[arrPrinters.Count];


        }

        void AddDatatoArray(string sPrinterName, int totalPrintedPages)
        {
            var printsnew = from p in perfmonValues
                            where p.Key == sPrinterName
                            select p;
            if (printsnew.Count() == 0)
            {
                perfmonValues.Add(sPrinterName, totalPrintedPages);
            }
            else
            {
                perfmonValues[sPrinterName] = totalPrintedPages;
            }
        }
        
        private void SaveXMLPrinterData(string sPrinterName, string sData)
        {
            string filePath="E:\\temp\\Monitor.xml";
            XDocument xDoc = null;
            if (!File.Exists(filePath))
            {
                xDoc = new XDocument(
                            new XDeclaration("1.0", "UTF-16", null),
                            new XElement("PrintersData",
                                new XElement("PrinterData",
                                    new XElement("PrinterName", sPrinterName),
                                    new XElement("Date", DateTime.Now.ToString()),
                                    new XElement("PrintedPages", sData)
                                    )));
                AddDatatoArray(sPrinterName, Convert.ToInt32(sData));
                // Save to Disk
                xDoc.Save(filePath);
                Console.WriteLine("Saved");
            }
            else
            {
                xDoc = XDocument.Load(filePath);

                var printsnew = from nm in xDoc.Element("PrintersData").Elements()
                             where nm.Element("PrinterName").Value == sPrinterName
                             select nm;

                //var prints = from nm in xDoc.Element("PrintersData").Elements()
                //             where nm.Element("PrinterName").Value == sPrinterName && nm.Element("PrintedPages").Value!=sData
                //             select nm;

                XElement xele = printsnew.LastOrDefault();
                if (printsnew.Count() == 0)
                {
                    //int delta = Math.Abs(Convert.ToInt32(sData) - Convert.ToInt32(prints.ElementAt(0).Value));

                    

                    XElement xEle = new XElement("PrinterData",
                                        new XElement("PrinterName", sPrinterName),
                                        new XElement("Date", DateTime.Now.ToString()),
                                        new XElement("PrintedPages", sData)
                                        );
                    xDoc.Element("PrintersData").Add(xEle);
                    xDoc.Save(filePath);
                    AddDatatoArray(sPrinterName, Convert.ToInt32(sData));
                    Console.WriteLine("New Element Added");
                }
                else
                {
                    int delta = GetDelta(Convert.ToInt32(sData), sPrinterName);
                    if (delta != 0)
                    {
                        XElement xEle = new XElement("PrinterData",
                                        new XElement("PrinterName", sPrinterName),
                                        new XElement("Date", DateTime.Now.ToString()),
                                        new XElement("PrintedPages", delta)
                                        );
                        xDoc.Element("PrintersData").Add(xEle);
                        xDoc.Save(filePath);
                        AddDatatoArray(sPrinterName, Convert.ToInt32(sData));
                        Console.WriteLine("Element with delta Added");
                    }
                }
            }

            bDeletePrevious = true;
        }

        public int GetDelta(int newPrints, string sPrinterName)
        {
            var printsnew = from p in perfmonValues
                            where p.Key == sPrinterName
                            select p;
            int delta = 0;
            
            if (printsnew.Count() != 0)
            {
                delta=Math.Abs(newPrints - Convert.ToInt32(perfmonValues[sPrinterName]));
                return delta;
            }
            else
            {
                return 0;
            }
        }

        private void DeletePreviousXMLRecord()
        {
            string filePath="E:\\temp\\Monitor.xml";

            int printersCount = arrPrinters.Count;
            XDocument xDoc = null;

            if (File.Exists(filePath))
            {
                xDoc = XDocument.Load(filePath);

                //code to delete data before 8 days


                var emps = xDoc.Element("PrintersData").Descendants("PrinterData");


                emps.Reverse().Take(printersCount).Remove();

                xDoc.Save(filePath);
                Console.WriteLine("Element removed");
            }
            
        }

        private void DeletePreviousDaysRecord()
        {
            string filePath = "E:\\temp\\Monitor.xml";

            XDocument xDoc = null;

            DateTime dtOldDate = DateTime.Now.AddDays(-7);

            if (File.Exists(filePath))
            {
                xDoc = XDocument.Load(filePath);

                //code to delete data before 8 days
                var prints = from nm in xDoc.Element("PrintersData").Elements()
                           where Convert.ToDateTime( nm.Element("Date").Value) < dtOldDate
                           select nm;
                prints.Remove();

                xDoc.Save(filePath);
                Console.WriteLine("Previous days data removed");
            }

        }

        
    }
}
