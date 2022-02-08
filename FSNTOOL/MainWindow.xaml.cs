using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Windows.Input;
using System.Text.RegularExpressions;
namespace FSNTOOL
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        class JClass
        {
            public string Revision { get; set; }
            public string Title { get; set; }
            public Object DUT_FSN { get; set; }
            
        }

        class FClass
        {
            unsafe public char* Length { get; set; }             
        }


        // Check for valid formats
        const int FSN_LENGTH_NEW_FORMAT = 16;
        private unsafe bool M_IsFSNValid_NewFormat(string szFSN) 
        {
            // must be true
            bool bRet = true;
            int i = 0;
            // string to char array
            char[] charArr = szFSN.ToCharArray();
            // verify NULL and minimum acceptable length
            if(szFSN == null || szFSN.Length < 14)
            {
                return false;
            }
        
            // verify the actual length: only accept 16 or 14 characters (i.e. with or without BOM)
            if ((szFSN.Length != FSN_LENGTH_NEW_FORMAT) && szFSN.Length != FSN_LENGTH_NEW_FORMAT - 2)
            {
                return false;
            }


            // now check our standard format
            for(i = 0; i < 2; i++) 
            {
                string temp = charArr[i].ToString();
                if(!isalnum(temp)) //start with 2 alphanumeric
                {
                    bRet = false;
                }
            }
                
            // check if all other characters are digits
            for (; i < 14; i++) //all other characters are digits
            {
                string temp = charArr[i].ToString(); 
                if (!isalnum(temp))
                {
                    bRet = false;
                }
            }

            if (szFSN.Length == 16)
            {
                for(; i < 16; i++)
                {
                    string temp = charArr[i].ToString(); 
                    if (!isalnum(temp))
                    {
                        bRet = false;
                    }
                }
            }
            
            return bRet;
        }

        // Save button actions
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // store textbox inputs as strings
            string dut2 = FSN2.Text;
            string dut1 = FSN1.Text;
            string dut4 = FSN4.Text;
            string dut3 = FSN3.Text;

            // create list to check for duplicates
            List<string> CheckEmpty = new List<string>();
            CheckEmpty.Add(dut1);
            CheckEmpty.Add(dut2);
            CheckEmpty.Add(dut3);
            CheckEmpty.Add(dut4);

            // check for empty inputs
            bool isEmpty = false;
            bool isDuplicate = false;
            for (int i = 0; i < CheckEmpty.Count; i++)
            {
                if (CheckEmpty.ElementAt(i) == "")
                {
                    isEmpty = true; 
                    break;
                } 
            }
            if (!isEmpty)
            {
                dut1 = dut1.Substring(0, dut1.Length - 2);
                dut2 = dut2.Substring(0, dut2.Length - 2);
                dut3 = dut3.Substring(0, dut3.Length - 2);
                dut4 = dut4.Substring(0, dut4.Length - 2);
            }

            // check if format of all four inputs are valid
            bool format = M_IsFSNValid_NewFormat(dut1) && M_IsFSNValid_NewFormat(dut2) && M_IsFSNValid_NewFormat(dut3) && M_IsFSNValid_NewFormat(dut4);    


            List<string> CheckDuplicates = new List<string>();
            CheckDuplicates.Add(dut1);
            CheckDuplicates.Add(dut2);
            CheckDuplicates.Add(dut3);
            CheckDuplicates.Add(dut4);

            // check for duplicates
            if (CheckDuplicates.Count != CheckDuplicates.Distinct().Count())
            {
                isDuplicate = true;
            }

            // DEBUG
            System.Diagnostics.Debug.WriteLine("DUT4 = " + dut4);
            System.Diagnostics.Debug.WriteLine("DUT3 = " + dut3);
            System.Diagnostics.Debug.WriteLine("DUT2 = " + dut2);
            System.Diagnostics.Debug.WriteLine("DUT1 = " + dut1);

            // create DUT_FSN object
            JObject DUT = new JObject 
            {
                {"FSN_1", dut1},
                {"FSN_2", dut2},
                {"FSN_3", dut3},
                {"FSN_4", dut4}
            };
           
            // create outer layer
            List<JClass> list = new List<JClass>();
            list.Add(new JClass()
            {
                Revision = "1.00",
                Title = "FSN Configuration File",
                DUT_FSN = DUT
            });
           
            // serialize json and save to path
            string msgBox = "";
            string json = JsonConvert.SerializeObject(list[0], Formatting.Indented);
            

            // string fileName = @"C:\Users\ctong\Desktop\Cindy_Testing\FSN_Config.json"; //This path is for debugging, use the other one for testing
            string fileName = @"C:\SWIMFT\FSN_Config.json";
            

            // check if all conditions are valid before saving to file
            if (isEmpty)
            {
                msgBox = "Contains Empty Input, ";
            } else if (isDuplicate)
            {
                msgBox = "Duplicated Input, ";
            } else if (!format)
            {
                msgBox = "Format Issues, ";
            } else
            {
                msgBox = "Multiple Issues, ";
            }

            // save file and delete current file if already exists
            bool goodCondition = !isDuplicate && !isEmpty && format;
            if (File.Exists(fileName) && goodCondition)
            {
                File.Delete(fileName);
                MessageBox.Show("File exists, deleting current file...");
                File.WriteAllText(fileName, json);
                MessageBox.Show("File Saved Successfully");
            }
            else if (goodCondition)
            {
                File.WriteAllText(fileName, json);
                MessageBox.Show("File Saved Successfully");
            }
            else
            {
                MessageBox.Show( msgBox + "File Failed To Save");
            }

            // reset textboxes after saving file
            FSN1.Text = "";
            FSN2.Text = "";
            FSN3.Text = "";
            FSN4.Text = "";

        }

        // Input validation functions
        public static Boolean isalnum(string s)
        {
            Regex rg = new Regex(@"^[a-zA-Z0-9\s,]*$");
            return rg.IsMatch(s);
        }

        // Send key method
        public static void Send(Key key)
        {
            if (Keyboard.PrimaryDevice != null)
            {
                if (Keyboard.PrimaryDevice.ActiveSource != null)
                {
                    var e = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, key)
                    {
                        RoutedEvent = Keyboard.KeyDownEvent
                    };
                    InputManager.Current.ProcessInput(e);
                }
            }
        }

        // Textboxes
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            String dut = FSN2.Text;
            dut = dut.Substring(Math.Max(0, dut.Length - 1));
            if (dut.Contains("n"))
            {
                Send(Key.Tab);
            }

        }

        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            String dut = FSN1.Text;
            dut = dut.Substring(Math.Max(0, dut.Length - 1));
            if (dut.Contains("n"))
            {
                Send(Key.Tab);
            }
        }

        private void TextBox_TextChanged_2(object sender, TextChangedEventArgs e)
        {
            String dut = FSN4.Text;
            dut = dut.Substring(Math.Max(0, dut.Length - 1));
            if (dut.Contains("n"))
            {
                Send(Key.Tab);
            }
        }

        private void TextBox_TextChanged_3(object sender, TextChangedEventArgs e)
        {

        }
       

    }
}
