using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Automation;
using System.Diagnostics;
using Microsoft.VisualBasic;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace App
{
    public partial class Form1 : Form
    {
        string TestScriptFile = "";
        string TempScriptFile = Application.ExecutablePath + "\\..\\Scripts\\" + "Script.txt";

        string Automateau3Path = Application.ExecutablePath + "\\..\\Engine\\automate.au3";
        string UDFPath = Application.ExecutablePath + "\\..\\Engine\\UDF.au3";
        string backup_automateau3 = Application.ExecutablePath + "\\..\\Engine\\Backup\\automate_back.au3";
        string backup_UDF = Application.ExecutablePath + "\\..\\Engine\\Backup\\UDF_back.au3";
        
        string au3Builder = ""; //obtained from registry when the form is constructed.
        //string au2Builder = "C:\\Program Files\\AutoIt3\\Aut2exe\\Aut2exe"; //.exe will be appended in SaveAsUDF()
        string auBuilder = "";
        string au3Help = "";
        
        string AutoItPrefix = "++";

        Dictionary<string, string> Dict = new Dictionary<string, string>();

        List<string> NoArgFunctions = new List<string>();

        string AppName = "StepWise6.4";
        
        int NumParams = 0;  //used in UDF creation
        Dictionary<string, string> UDFParamDict = new Dictionary<string, string>();

        ComboBox CParam;

        int OldScriptFileIndex = 0;

        private class SearchData
        {
            public int StartIndex { get; set; }
            public int Length { get; set; }
            public string SelectionRTF { get; set; }
        }

        List<SearchData> searchList = null;

        public delegate bool EnumDelegate(IntPtr hWnd, int lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "GetWindowText", ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpWindowText, int nMaxCount);

        [DllImport("user32.dll", EntryPoint = "EnumDesktopWindows", ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumDelegate lpEnumCallbackFunction, IntPtr lParam);

        [DllImportAttribute("User32.dll")]
        private static extern IntPtr FindWindow(String ClassName, String WindowName);

        [DllImportAttribute("User32.dll")]
        private static extern IntPtr SetForegroundWindow(IntPtr hWnd);

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWow64Process([In] IntPtr hProcess, [Out] out bool lpSystemInfo);

        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        private const int SW_RESTORE = 9;

        public Form1()
        {
            InitializeComponent();
            ReadFile();//List in the dropdown
            ReadConfiguration(); //Read from registry

            TestScript.Text += "\n";
            TestScript.SelectionStart = 20;

            PopulateDict();

            foreach (string ScriptFile in Directory.GetFiles(Application.ExecutablePath + "\\..\\Scripts\\", "*.txt"))
            {
                FileInfo file = new FileInfo(ScriptFile);
                ScriptFiles.Items.Add(file.Name);
            }
        }

        public void ReadConfiguration()
        {
            string AutoIt_installpath = "";
            try
            {
                AutoIt_installpath = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\AutoIt v3\AutoIt", "InstallDir", null).ToString();
            }
            catch {
                try
                {
                    AutoIt_installpath = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\AutoIt v3\AutoIt", "InstallDir", null).ToString();
                }
                catch
                {
                }
            }
            //au3Builder = AutoIt_installpath + "\\SciTE\\AutoIt3Wrapper\\AutoIt3Wrapper.exe";
            au3Builder = AutoIt_installpath + "\\Aut2Exe\\Aut2exe.exe";
            au3Help = AutoIt_installpath + "\\AutoIt3.chm";
        }
        public void ReadFile ()
        {
            string rx = ";Methods-(.*)$";
            string Au3File = "automate.au3";
            bool CategoryPresent = false;
            string ToolTipText = "";

            ToolStripMenuItem CategoryItems = new ToolStripMenuItem();

            StreamReader sr = new StreamReader(Application.ExecutablePath + "\\..\\Engine\\" + Au3File);

            //Read the first line of text
            string line = sr.ReadLine();

            //Continue to read until you reach end of file
            while (line != null)
            {
                Match m = Regex.Match(line, rx);
                if (m.Success == true)
                {
                    ToolTipText = "";
                    string[] Categories = m.Groups[1].ToString().Split('-');
                    CategoryItems = toolCategoryItems;
                    foreach (string Category in Categories)
                    {
                        ToolStripMenuItem CategoryItem = new ToolStripMenuItem();

                        CategoryPresent = false;
                        CategoryItem.Text = Category;
                        CategoryItem.Click += new System.EventHandler(this.ToolStripMenuItem_Click);

                        foreach (ToolStripMenuItem DropDownItem in CategoryItems.DropDownItems)
                        {
                            if (DropDownItem.Text == CategoryItem.Text)
                            {
                                CategoryItem = DropDownItem;
                                CategoryPresent = true;
                                break;
                            }
                        }
                        if (!CategoryPresent)
                        {
                            CategoryItems.DropDownItems.Add(CategoryItem);
                        }

                        CategoryItems = CategoryItem;
                        //ToolTipText stores the full path, and serves as a persistent storage.
                        if (ToolTipText != "")
                            ToolTipText += "-";
                        ToolTipText += CategoryItem.Text;
                        CategoryItem.ToolTipText = ToolTipText;
                    }
                }
                line = sr.ReadLine();
            }
            //close the file
            sr.Close();
        }

        private void Functions_SelectedValueChanged(object sender, EventArgs e)
        {
            Param1Txt.Visible = false; Param1.Visible = false; CParam1.Visible = false; CParam1.Items.Clear(); CParam1.Text = ""; Param1.Clear(); Browse1.Visible = false; Browse1.Text = "...";
            Param2Txt.Visible = false; Param2.Visible = false; CParam2.Visible = false; CParam2.Items.Clear(); CParam2.Text = ""; Param2.Clear(); Browse2.Visible = false; Browse2.Text = "...";
            Param3Txt.Visible = false; Param3.Visible = false; CParam3.Visible = false; CParam3.Items.Clear(); CParam3.Text = ""; Param3.Clear(); Browse3.Visible = false; Browse3.Text = "...";
            Param4Txt.Visible = false; Param4.Visible = false; CParam4.Visible = false; CParam4.Items.Clear(); CParam4.Text = ""; Param4.Clear(); Browse4.Visible = false; Browse4.Text = "...";
            Param5Txt.Visible = false; Param5.Visible = false; CParam5.Visible = false; CParam5.Items.Clear(); CParam5.Text = ""; Param5.Clear(); Browse5.Visible = false; Browse5.Text = "...";
            Param6Txt.Visible = false; Param6.Visible = false; CParam6.Visible = false; CParam6.Items.Clear(); CParam6.Text = ""; Param6.Clear(); Browse6.Visible = false; Browse6.Text = "...";

            Description.Text = "";

            //Reset properties of Param2, which were set when the function was 'Record'
            Param2.Multiline = false;
            Param2.Height = 20;

            string SelectedName = Functions.SelectedItem.ToString();
            
            if (SelectedName == "Parameterize")
                AddStep.Text = "&Replace";
            else
                AddStep.Text = "&Add step";

            string rx = "case \"" + SelectedName + "\"";
            string rx_Desc = ";?(.*)";

            StreamReader sr = new StreamReader(Automateau3Path);
            
            //Read the first line of text
            string line = sr.ReadLine();

            //Continue to read until you reach end of file
            while (line != null)
            {
                Match m = Regex.Match(line, rx, RegexOptions.IgnoreCase);
                if (m.Success == true)
                {
                    //Next line will contain the description, and parameters of the selected function
                    line = sr.ReadLine();
                    Match m_Desc = Regex.Match(line, rx_Desc);

                    string[] words = m_Desc.Groups[1].ToString().Split(';');
                    
                    int IdxWords = 1;
                    int IdxSplit = 0;
                    string FinalWord;
                    while (IdxWords < words.Count())
                    {
                        IdxSplit = 0;
                        FinalWord = "";
                        string[] splitwords = words[IdxWords].Split('+');
                        while (IdxSplit < splitwords.Count())
                        {
                            FinalWord += "\n" + splitwords[IdxSplit];
                            FinalWord = Regex.Replace(FinalWord, @"\[.+\]", "");
                            IdxSplit++;
                        }

                        if (IdxWords >= 2)
                        {
                            Regex rx_Browse = new Regex(@"\[Browse File\]");
                            Regex rx_Record = new Regex(@"\[Record\]");

                            Button BrowseButton = null;
                            if (rx_Browse.IsMatch(words[IdxWords]) || rx_Record.IsMatch(words[IdxWords]))
                            {
                                Control[] Controls = this.groupBox1.Controls.Find("Browse" + (IdxWords - 1).ToString(), false); //IdxWords - 1 because the first Word indicates function descriptoin.
                                BrowseButton = (Button)Controls[0];
                                BrowseButton.Visible = true;
                                BrowseButton.Text = "Rec";
                            }
                            if (rx_Browse.IsMatch(words[IdxWords]))
                            {
                                BrowseButton.Text = "...";
                            }

                            //bool FlagIt = false;
                            string WhichList = "";

                            Regex rx_Process = new Regex(@"\[Select Process\]");
                            Regex rx_Window = new Regex(@"\[Select Window\]");
                            Regex rx_List = new Regex(@"\[(.+,.+)\]");
                            if (rx_Process.IsMatch(words[IdxWords]) || rx_Window.IsMatch(words[IdxWords]) || rx_List.IsMatch(words[IdxWords]))
                            {
                                Control[] Controls = this.groupBox1.Controls.Find("CParam" + (IdxWords - 1).ToString(), false); //IdxWords - 1 because the first Word indicates function descriptoin.
                                CParam = (ComboBox)Controls[0];
                                CParam.Visible = true;
                                CParam1.Focus();
                                if (rx_Window.IsMatch(words[IdxWords]) && WhichList == "") //!FlagIt)
                                {
                                    //FlagIt = true;
                                    WhichList = "Windows";

                                    //List all open windows, including all levels.
                                    ListAllWindows(CParam);
                                    //Below is an alternative mechanism to get all open (top-level) windows in the system.  Use items object to fill the drop-down list.
                                    //// get reference to desktop
                                    ////AutomationElement desktop = AutomationElement.RootElement;
                                    //// get list of all children items of the desktop
                                    ////AutomationElementCollection items = desktop.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, System.Windows.Automation.ControlType.Window));
                                }
                                if (rx_Process.IsMatch(words[IdxWords]) && WhichList == "") //!FlagIt)
                                {
                                    WhichList = "Processes";

                                    //List processes
                                    Process[] processlist = Process.GetProcesses();

                                    foreach (Process process in processlist.Distinct())
                                    {
                                        if (!String.IsNullOrEmpty(process.ProcessName))
                                        {
                                            CParam.Items.Add(process.ProcessName);
                                        }
                                    }
                                }

                                //string OptionsList = "[(.+)]";
                                //Match m_List = Regex.Match(words[IdxWords], OptionsList);
                                Match m_List = rx_List.Match(words[IdxWords]);
                                if (m_List.Success && WhichList == "") //!FlagIt)
                                {
                                    WhichList = "Options";

                                    string[] SplitList = m_List.Groups[1].ToString().Split(',');
                                    foreach (string ListItem in SplitList)
                                    {
                                        CParam.Items.Add(ListItem);
                                    }
                                }
                            }
                            else
                            {
                                Control[] Controls = this.groupBox1.Controls.Find("Param" + (IdxWords - 1).ToString(), false); //IdxWords - 1 because the first Word indicates function descriptoin.
                                TextBox ParamText = (TextBox)Controls[0];
                                ParamText.Visible = true;
                            }
                        }
                        FinalWord = FinalWord.TrimStart('\n');
                        switch (IdxWords)
                        {
                            case 1: Description.Text = FinalWord; break;
                            case 2: Param1Txt.Text = FinalWord + " :"; Param1Txt.Visible = true; Param1.Visible = true; Param1.SendToBack(); Param1.Focus(); break;
                            case 3: Param2Txt.Text = FinalWord + " :"; Param2Txt.Visible = true; Param2.Visible = true; Param2.SendToBack(); break;
                            case 4: Param3Txt.Text = FinalWord + " :"; Param3Txt.Visible = true; Param3.Visible = true; Param3.SendToBack(); break;
                            case 5: Param4Txt.Text = FinalWord + " :"; Param4Txt.Visible = true; Param4.Visible = true; Param4.SendToBack(); break;
                            case 6: Param5Txt.Text = FinalWord + " :"; Param5Txt.Visible = true; Param5.Visible = true; Param5.SendToBack(); break;
                            case 7: Param6Txt.Text = FinalWord + " :"; Param6Txt.Visible = true; Param6.Visible = true; Param6.SendToBack(); break;
                        }
                        IdxWords++;
                    }
                    break;
                }
                line = sr.ReadLine();
            }
            //close the file
            sr.Close();
        }

        private void AddStep_Click(object sender, EventArgs e)
        {
            string Function = Functions.Text;
            string StepString = Function;
            string MethodText = "";
            int NumParams = 0;

            //In case of some functions
            if (Function == "Record")
            {
                if (CParam1.Text != "")
                    StepString = "WinWaitActive";
                else
                    StepString = "";
            }

            if (Function == "Parameterize")
            {
                StepString = "<p" + CParam1.Text + "," + Param2.Text + "," + CParam3.Text + ">";
                TestScript.SelectedText = StepString;
                return;
            }

            if (Param1.Text != "") { MethodText = Param1.Text; NumParams += 1; } else { if (CParam1.Text != "") { MethodText = CParam1.Text; NumParams += 1; } }
            if (MethodText != "") { StepString = AddToStep(StepString, MethodText, true); };

            MethodText = "";

            if (Param2.Text != "") { MethodText = Param2.Text; NumParams += 1; } else { if (CParam2.Text != "") { MethodText = CParam2.Text; NumParams += 1; } }
            if (MethodText != "") { StepString = AddToStep(StepString, MethodText, false); };

            MethodText = "";

            if (Param3.Text != "") { MethodText = Param3.Text; NumParams += 1; } else { if (CParam3.Text != "") { MethodText = CParam3.Text; NumParams += 1; } }
            if (MethodText != "") { StepString = AddToStep(StepString, MethodText, false); };

            MethodText = "";

            if (Param4.Text != "") { MethodText = Param4.Text; NumParams += 1; } else { if (CParam4.Text != "") { MethodText = CParam4.Text; NumParams += 1; } }
            if (MethodText != "") { StepString = AddToStep(StepString, MethodText, false); };

            MethodText = "";

            if (Param5.Text != "") { MethodText = Param5.Text; NumParams += 1; } else { if (CParam5.Text != "") { MethodText = CParam5.Text; NumParams += 1; } }
            if (MethodText != "") { StepString = AddToStep(StepString, MethodText, false); };

            MethodText = "";

            if (Param6.Text != "") { MethodText = Param6.Text; NumParams += 1; } else { if (CParam6.Text != "") { MethodText = CParam6.Text; NumParams += 1; } }
            if (MethodText != "") { StepString = AddToStep(StepString, MethodText, false); };

            //if (StepString != Functions.Text)
            //    StepString += ");";
            //else
            //    StepString += ";";

            if (NumParams == 0)
                StepString += " ;";
            else
                StepString += ";";

            if (PauseStep.Checked)
            {
                toolCategoryItems.Text = "Common-Basic-System";
                Functions.SelectedIndex = 3;
            }

            //TestScript.SelectionStart += TestScript.SelectionLength;
            //TestScript.SelectionLength = 0;
            TestScript.SelectedText = StepString + Comments.Text + "\n;\n";

            if (Function == "Record")
                Param2.Text = "";
        }

        public string AddToStep(string StepString, string Param, bool Start)
        {
            int IsNumeric;

            if (Start)
            {
                StepString += " ";
            }
            else
            {
                //In case of 'Record' function, it's WinWaitActive with no second parameter - no "," needed.  See AddStep_Click.
                if (Functions.Text == "Record")
                    StepString += " ";
                else
                    StepString += ",";
            }
            StepString += Param;

            return StepString;
        }

        private bool RecordKeys(string ButtonText)
        {
            if (ButtonText == "System.Windows.Forms.Button, Text: Rec")
            {
                IntPtr hWnd = FindWindow(null, CParam.Text);
                if (hWnd != null) //If found
                {
                    ShowWindowAsync(hWnd, SW_RESTORE);
                    SetForegroundWindow(hWnd); //Activate it
                }

                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo(Application.ExecutablePath + "\\..\\Record.exe");
                
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();

                //after Record.exe exits.
                Param2.Text = Clipboard.GetText(); //if you don't want to overwrite, change to Param2.Text += Clipboard.GetText();
                Param2.Multiline = true;
                Param2.Height = 250;
                Clipboard.Clear();

                //List all open windows, including all levels.
                ListAllWindows(CParam);

                hWnd = FindWindow (null, AppName);
                if (hWnd != null) //If found
                {
                    ShowWindowAsync(hWnd, SW_RESTORE);
                    //SetForegroundWindow(hWnd); //Activate it
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        private void Browse1_Click(object sender, EventArgs e)
        {
            if (RecordKeys(sender.ToString()))
            {
                return;
            }
            OpenFileDialog.Filter = "Executable files|*.exe|All files|*.*";
            OpenFileDialog.ShowDialog();
            Param1.Text = OpenFileDialog.FileName;
        }

        private void Browse2_Click(object sender, EventArgs e)
        {
            if (RecordKeys(sender.ToString()))
            {
                return;
            }
            OpenFileDialog.Filter = "Executable files|*.exe|All files|*.*";
            OpenFileDialog.ShowDialog();
            Param2.Text = OpenFileDialog.FileName;
        }

        private void Browse3_Click(object sender, EventArgs e)
        {
            if (RecordKeys(sender.ToString()))
            {
                return;
            }
            OpenFileDialog.Filter = "Executable files|*.exe|All files|*.*";
            OpenFileDialog.ShowDialog();
            Param3.Text = OpenFileDialog.FileName;
        }

        private void Browse4_Click(object sender, EventArgs e)
        {
            if (RecordKeys(sender.ToString()))
            {
                return;
            }
            OpenFileDialog.Filter = "Executable files|*.exe|All files|*.*";
            OpenFileDialog.ShowDialog();
            Param4.Text = OpenFileDialog.FileName;
        }

        private void Browse5_Click(object sender, EventArgs e)
        {
            if (RecordKeys(sender.ToString()))
            {
                return;
            }
            OpenFileDialog.Filter = "Executable files|*.exe|All files|*.*";
            OpenFileDialog.ShowDialog();
            Param5.Text = OpenFileDialog.FileName;
        }

        private void Browse6_Click(object sender, EventArgs e)
        {
            if (RecordKeys(sender.ToString()))
            {
                return;
            }
            OpenFileDialog.Filter = "Executable files|*.exe|All files|*.*";
            OpenFileDialog.ShowDialog();
            Param6.Text = OpenFileDialog.FileName;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog.Filter = "Text files|*.txt|All files|*.*";
                OpenFileDialog.ShowDialog();
                TestScriptFile = OpenFileDialog.FileName;
                TestScript.Lines = File.ReadAllLines(TestScriptFile);
            }
            catch (Exception Ex)
            {
            }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScriptFiles.SelectedIndex = 0;

            TestScriptFile = "";
            TestScript.Clear();
            TestScript.Text = ";Test script file ;\n";
            TestScript.SelectionStart = 20;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (TestScriptFile != "")
                {
                    TestScript.SaveFile(TestScriptFile, RichTextBoxStreamType.PlainText);
                }
                else
                {
                    SaveScript();
                }
            }
            catch (Exception Ex)
            {
            }
        }

        private void SaveScript ()
        {
            string PathToScripts = Application.ExecutablePath + "\\..\\Scripts\\";
            SaveScriptDialog.InitialDirectory = Path.GetFullPath(PathToScripts); //Application.ExecutablePath + "\\..\\Scripts\\";
            SaveScriptDialog.RestoreDirectory = true;
            SaveScriptDialog.ShowDialog();
            TestScript.SaveFile(SaveScriptDialog.FileName, RichTextBoxStreamType.PlainText);
            TestScriptFile = SaveScriptDialog.FileName;
        }

        private void showDesktopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Shell32.ShellClass shell = new Shell32.ShellClass();
            ((Shell32.IShellDispatch5)shell).ToggleDesktop(); //show desktop

            //shell.MinimizeAll();
        }

        private bool HandleAutoItFound()
        {
            foreach (string Line in TestScript.Lines)
                if (Line.StartsWith(AutoItPrefix))
                {
                    DialogResult Result = MessageBox.Show("Test script contains embedded logic, which will not be executed if you continue.\n\n" +
                                        "Choose 'Save as UDF' and use it build your script if you want to execute the embedded logic.\n\n" +
                                        "Do you want to continue?",
                                        "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (Result == DialogResult.No)
                        return false;

                    if (Result == DialogResult.Yes)
                        return true;
                }
            return true;
        }

        private void runScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string Args = "";

                if (!HandleAutoItFound())
                    return;

                TestScript.SaveFile(TempScriptFile, RichTextBoxStreamType.PlainText);
                if (TestScriptFile != "")
                    Args = "\"" + TestScriptFile + "\"";
                else
                    Args = "\"" + TempScriptFile + "\"";

                RunTestScript(Args);
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message + "\n" + Ex.StackTrace);

            }
        }

        private void RunTestScript(string Args)
        {
            try
            {
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo("\"" + Application.ExecutablePath + "\\..\\Engine\\automate.exe\"");

                startInfo.Arguments = Args;

                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message + "\n" + Ex.StackTrace);
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string FunctionName = SaveAsUDF(TestScript.Lines);
        }

        private string SaveAsUDF(string[] tempArray)
        {
            NumParams = 0;
            UDFParamDict.Clear();

            if (MessageBox.Show("You're about to save the script content as a user defined function.\n\n" +
                                "Please note that you must have AutoIt installed in your system in order to proceed",
                                "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.Cancel)
            {
                return "";
            }

            //Writing method content
            string TheBigString = GetUDFContent(tempArray);
            
            if (TheBigString != "")
                Clipboard.SetText(TheBigString);

            //Finalizing...
            //First display the lines and ask for confirmation.

            DialogResult uResult = MessageBox.Show("Following statements have been generated: \n\n" +
                                    TheBigString + "\n\n" +
                                    "Do you confirm creation of this UDF?\n\n" +
                                    "NOTE: Above statements is now available in the clipboard.",
                                    "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (uResult == DialogResult.No)
                return "";

            if (File.Exists(au3Builder))
                auBuilder = au3Builder;
            //else
            //    //If AutoIt3.2 is installed, it will have both au3Builder and au2Builder!
            //    if (File.Exists(au2Builder + ".exe"))
            //        if (Is64Bit())
            //            auBuilder = au2Builder + "_x64.exe";
            //        else
            //            auBuilder += au2Builder + ".exe";

            if (auBuilder == "")
            {
                MessageBox.Show("You don't seem to have AutoIt installed in your system.\n\n" +
                                "Cannot proceed with generation of user defined function",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "";
            }

            string FunctionName = Microsoft.VisualBasic.Interaction.InputBox(
                                "What name would you like to give to the function?", "Input", "UDF1", 0, 0);

            while ((FunctionName == "") || Dict.Keys.Contains(FunctionName))
            {
                DialogResult Result = MessageBox.Show("Function name already exists or you provided an invalid name", "Error", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error);
                if (Result == DialogResult.Abort)
                    return "";
                if (Result == DialogResult.Retry)
                    PopulateDict();
                if (Result == DialogResult.Ignore)
                    break;
            }

            //Start creating the UDF
            //Backup the older automate.au3 and UDF.au3, so that in case of a build error they can be restored.
            File.Copy(Automateau3Path, backup_automateau3, true); 
            File.Copy (UDFPath, backup_UDF, true); 
            
            PopulateDict();

            StreamWriter sw = File.AppendText(UDFPath);

            //Writing method name
            sw.WriteLine("\n");
            sw.WriteLine("Func " + FunctionName + "($Arg1)");

            //Now, the header line.
            string HeaderLine = "\t$result = StringRegExp($Arg1, ";
            if (NumParams >= 0)
                HeaderLine += "\"(.*)";
            for (int IdxParams = 1; IdxParams < NumParams; IdxParams++)
            {
                HeaderLine += ",";
                HeaderLine += "(.*)";
            }
            if (NumParams >= 0)
                HeaderLine += "\"";

            HeaderLine += ",1)";

            sw.WriteLine(HeaderLine);

            //Adding InsertSleep, by default.
            sw.WriteLine ("\tC_Sleep (\"0.5\" & \",\" & \"-1\")");

            //Writing the UDF content.  Returned from GetUDFContent.
            sw.WriteLine(TheBigString);

            sw.WriteLine("EndFunc");
            sw.Close();

            if (!WriteHandlerAndBuild(FunctionName))
                MessageBox.Show("User defined function " + FunctionName + "() could not be created due to errors.\n\n" +
                                    "Try again!", 
                                "UDF",MessageBoxButtons.OK,MessageBoxIcon.Error);
            else
                MessageBox.Show("User defined function " + FunctionName + "() created",
                                "UDF", MessageBoxButtons.OK, MessageBoxIcon.Information);

            PopulateDict(); //Re-populate the function map.

            return FunctionName;
        }

        private string GetUDFContent(string[] tempArray)
        {
            int FinalStringCount = 0;
            bool AutoItString = false;
            string[] Args = { "" };
            string MethodName = "";
            string Comments = "";
            bool SingleArgument = false;
            bool NoArgument = false;
            string[] FinalStrings = new string[1000];

            string rx = @"(\w+) (.*[^;])?(;(.*))";

            MatchEvaluator myEvaluator = new MatchEvaluator(ReplaceToken);

            for (int Count = 0; Count < tempArray.Length; Count++)
            {
                if (tempArray[Count] == ";Test script file ;")
                    continue;

                FinalStringCount++;

                //Handle the v3 functions and usages
                string v3String = tempArray[Count].Replace(AutoItPrefix, "");
                if (tempArray[Count] != v3String)
                    AutoItString = true;
                else
                    AutoItString = false;

                foreach (Match match in Regex.Matches(tempArray[Count], rx, RegexOptions.IgnoreCase)) //there will be only one item in this collection.
                {
                    if (AutoItString)
                        Args[0] = match.Groups[2].Value;
                    else
                        Args = match.Groups[2].Value.Split(',');
                    MethodName = match.Groups[1].Value.ToString();
                    Comments = match.Groups[3].Value.ToString();

                    //Starting to construct the final string
                    int Counter = 1;
                    string InternalName = "";

                    if (Dict.TryGetValue(MethodName, out InternalName))
                    {
                        MethodName = InternalName;
                        SingleArgument = true;
                    }

                    if (NoArgFunctions.IndexOf(MethodName) >= 0)
                        NoArgument = true;

                    //Store in an array, since header needs to be written first.  Once the header is written, array gets dumped into the same file.
                    FinalStrings[FinalStringCount] = "\t" + MethodName + " ";
                    if (!AutoItString)
                        FinalStrings[FinalStringCount] += "(";

                    if (NoArgument)
                        NoArgument = false;  //reset
                    else
                    {
                        string LineArgs = match.Groups[2].Value.Trim();

                        SingleArgument = false; //reset

                        //Following piece of code handles the parameterization
                        Regex Pattern = new Regex("<p(\\d),(.*?),(.*?)>");
                        LineArgs = Pattern.Replace(LineArgs, myEvaluator);

                        if (!AutoItString)
                        {
                            LineArgs = Regex.Replace(LineArgs, ",", "\"&\""); //06-07
                            LineArgs = "\"" + LineArgs; //Prepend a "
                            LineArgs += "\""; //Append a "
                        }

                        Pattern = new Regex("\"*\\$result\\[(\\d)\\]\"*");
                        LineArgs = Pattern.Replace(LineArgs, delegate(Match m) { return "$result[" + m.Groups[1].ToString() + "]"; });

                        Pattern = new Regex("&\"");
                        LineArgs = Pattern.Replace(LineArgs, delegate(Match m) { return " & \",\" & \""; });

                        Pattern = new Regex("\\+");
                        LineArgs = Pattern.Replace(LineArgs, delegate(Match m) { return "\" & \""; });

                        Pattern = new Regex("]\\s*\"");
                        LineArgs = Pattern.Replace(LineArgs, delegate(Match m) { return "] "; });

                        Pattern = new Regex("\"\\s*\\$");
                        LineArgs = Pattern.Replace(LineArgs, delegate(Match m) { return " $"; });
                        //until here

                        FinalStrings[FinalStringCount] += LineArgs; //all Arguments together.
                    }
                }

                if (FinalStrings[FinalStringCount] != null && !AutoItString)
                {
                    FinalStrings[FinalStringCount] += ")" + Comments;
                }
            }

            string TheBigString = "";
            foreach (string FinalString in FinalStrings)
            {
                if (FinalString != null)
                    TheBigString += FinalString + "\n";
            }

            return TheBigString;
        }

        private string ReplaceToken(Match m)
        {
            try
            {
                UDFParamDict.Add(m.Groups[2].ToString(), m.Groups[3].ToString());
                NumParams++;
            }
            catch
            {
            }
            //when 1st paramter is found, it should be named $result[0] and not $result[1].  So, reducing by 1.
            return "$result[" + (Int32.Parse (m.Groups[1].ToString()) - 1).ToString() + "]";
        }

        private bool WriteHandlerAndBuild(string FunctionName)
        {
            string[] Handler = new string[3];
            bool ProgramBuilt = false;
            string build_output = Automateau3Path + "\\..\\build_output.txt";

            Handler[0] = "\t\t\tCase \"" + FunctionName + "\"";
            Handler[1] = "\t\t\t\t;";
            foreach (string UDFParam in UDFParamDict.Keys)
            {
                //Select Window, Browse File, Select Process, Custom list
                Handler[1] += ";" + UDFParam + "[" + UDFParamDict[UDFParam] + "]";
            }

            Handler[2] = "\t\t\t\t" + FunctionName + "($BaseArgs)";

            string[] automateau3 = File.ReadAllLines(Automateau3Path);

            StreamWriter sw = new StreamWriter(Automateau3Path);

            foreach (string automateau3line in automateau3)
            {
                sw.WriteLine (automateau3line);
                if (automateau3line.IndexOf ("Methods-UDF") != -1)
                {
                    foreach (string HandlerLine in Handler)
                    {
                        sw.WriteLine (HandlerLine);
                    }
                }
            }
            sw.Close();

            //Build automate.exe
            //string Arguments = "/prod /in \"" + Path.GetFullPath(Automateau3Path) + "\"";
            string Arguments = "/in \"" + Path.GetFullPath(Automateau3Path) + "\"";
            try
            {
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo("\"" + auBuilder + "\"");
                startInfo.Arguments = Arguments;
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();
                
                string build_output_line = process.StandardOutput.ReadToEnd();

                //Check the build_output.txt to see if there are errors.  The OR condition was added in AutoIt3.3.10.2, where there is no output.
                if (Regex.IsMatch(build_output_line, "Packed 1 file") || build_output_line == "")
                    ProgramBuilt = true;

                //Restore the backed up automate.au3 and UDF.au3, if the build raised errors.
                if (!ProgramBuilt)
                {
                    File.Copy(backup_automateau3, Automateau3Path, true);
                    File.Copy(backup_UDF, UDFPath, true);
                    return false;
                }
                else
                    return true;
            }
            catch (Exception Ex)
            {
                return false;
            }
        }
        private void CategoryChanged(object sender, EventArgs e)
        {
            bool Start = false;

            StreamReader sr = new StreamReader(Automateau3Path);

            Functions.Items.Clear(); //Clear previous items.
            Functions.Items.Add(""); //so that you can enter just a comment.

            //Read the first line of text
            string line = sr.ReadLine();

            //Continue to read until you reach end of file
            while (line != null)
            {
                foreach (Match match in Regex.Matches(line, ";Methods-(.*)$", RegexOptions.IgnoreCase)) //there will be only one item in this collection.
                {
                    if (match.Groups[1].Value.ToString() == sender.ToString())
                    {
                        Start = true;
                        break;
                    }
                    else
                    {
                        Start = false;
                        break;
                    }
                }

                string rx = "case \"(.*)\"$";
                Match m = Regex.Match(line, rx, RegexOptions.IgnoreCase);
                if (m.Success == true && Start)
                {
                    Functions.Items.Add(m.Groups[1]);
                }
                line = sr.ReadLine();
            }
            //close the file
            sr.Close();
        }

        private void debugScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (!HandleAutoItFound())
                    return;

                TestScript.SaveFile(TempScriptFile, RichTextBoxStreamType.PlainText);
                
                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo("\"" + Application.ExecutablePath + "\\..\\Engine\\automate.exe\"");
                startInfo.Arguments = "\"" + TempScriptFile + "\" -debug";

                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();

            }
            catch (Exception Ex)
            {
            }
        }

        private void RunButton_Click(object sender, EventArgs e)
        {
            toolCategoryItems.Text = "Common-Basic-Basic";
            Functions.SelectedIndex = 1; //Run
        }
        private void WinWaitActiveButton_Click(object sender, EventArgs e)
        {
            toolCategoryItems.Text = "Common-Basic-Basic";
            Functions.SelectedIndex = 4; //WinWaitActive
        }

        private void ActivateWindowButton_Click(object sender, EventArgs e)
        {
            toolCategoryItems.Text = "Common-Basic-Basic";
            Functions.SelectedIndex = 6; //WinActivate
        }

        private void WinCloseButton_Click(object sender, EventArgs e)
        {
            toolCategoryItems.Text = "Common-Basic-Basic";
            Functions.SelectedIndex = 8; //WinClose
        }

        private void RecordButton_Click(object sender, EventArgs e)
        {
            toolCategoryItems.Text = "Common-Basic-Basic";
            Functions.SelectedIndex = 3; //Record
        }

        private void openLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo("notepad.exe");
            startInfo.Arguments = Application.ExecutablePath + "\\..\\debuglog.txt";
            process.StartInfo = startInfo;
            process.Start();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form AboutBox = new AboutBox();
            AboutBox.Show();
        }

        private void ScriptFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (ScriptFiles.SelectedIndex == OldScriptFileIndex)
                    return;

                DialogResult Result = MessageBox.Show("Do you want to save currently opened file?", "Warning", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

                if (Result == DialogResult.Yes)
                {
                    saveToolStripMenuItem_Click(sender, EventArgs.Empty);
                }
                else if (Result == DialogResult.Cancel)
                {
                    ScriptFiles.SelectedIndex = OldScriptFileIndex;
                    return;
                }

                OldScriptFileIndex = ScriptFiles.SelectedIndex; //saving it for future use.

                TestScriptFile = Application.ExecutablePath + "\\..\\Scripts\\" + ScriptFiles.Text;

                TestScript.Lines = File.ReadAllLines(TestScriptFile);
                ColorFunctions ();
                
                TestScript.Enabled = true;
            }
            catch (Exception Ex)
            {
            }
        }

        private void Refresh_Click(object sender, EventArgs e)
        {
            try
            {
                ScriptFiles.Items.Clear();
                ScriptFiles.Items.Add (new ComboBoxItem(""));
                foreach (string ScriptFile in Directory.GetFiles(Application.ExecutablePath + "\\..\\Scripts\\", "*.txt"))
                {
                    FileInfo file = new FileInfo(ScriptFile);
                    ScriptFiles.Items.Add(new ComboBoxItem(string.Format(file.Name, 1), Color.Green));
                }
            }
            catch (Exception Ex)
            {
                
            }
        }

        private void ScriptFiles_MeasureItem(object sender, MeasureItemEventArgs e)
        {

            switch (e.Index)
            {
                case 0:
                    e.ItemHeight = 20;
                    break;
                case 1:
                    e.ItemHeight = 20;
                    break;
                case 2:
                    e.ItemHeight = 20;
                    break;
            }
            e.ItemWidth = 260;

        }

        private void RunResults_DrawItem(object sender, DrawItemEventArgs e)
        {
            Graphics g = e.Graphics;
            ComboBoxItem cbItem = (ComboBoxItem)RunResults.Items[e.Index];

            //SizeF mySize = e.Graphics.MeasureString(cbItem.Text, RunResults.Font);
            e.Graphics.FillRectangle(new SolidBrush(cbItem.BackColor), new Rectangle(e.Bounds.X, e.Bounds.Y, 221, 17));//e.Bounds.X, e.Bounds.Y, Convert.ToInt32(mySize.Width), Convert.ToInt32(mySize.Height)
            e.Graphics.DrawString(cbItem.Text, RunResults.Font, new SolidBrush(Color.Yellow), e.Bounds);

            if (RunResults.Text == cbItem.Text)
                e.Graphics.DrawRectangle(new Pen(Color.DeepPink), new Rectangle(e.Bounds.X, e.Bounds.Y, 221, 17));
            
        }

        private void RunResults_SelectedIndexChanged(object sender, EventArgs e)
        {
            FileInfo TestFile = new FileInfo(TestScriptFile);
            string RunLogFile = Application.ExecutablePath + "\\..\\Logs\\" + TestFile.Name.Replace(".txt", "") + "\\" + RunResults.Text;

            TestScript.Lines = File.ReadAllLines(RunLogFile);
            TestScript.Enabled = false;
        }

        private void RefreshResults_Click(object sender, EventArgs e)
        {
            try
            {
                //Refresh individual logs for script files
                RunResults.Items.Clear();
                FileInfo TestFile = new FileInfo(TestScriptFile);
                foreach (string RunResult in Directory.GetFiles(Application.ExecutablePath + "\\..\\Logs\\" + TestFile.Name.Replace(".txt", "") + "\\", "*.*"))
                {
                    FileInfo file = new FileInfo(RunResult);

                    if (Regex.IsMatch(RunResult, "PASS"))
                    {
                        RunResults.Items.Add(new ComboBoxItem(file.Name, Color.Green));
                    }
                    else
                    {
                        RunResults.Items.Add(new ComboBoxItem(file.Name, Color.Red));
                    }
                }
            }
            catch (Exception Ex)
            {
            }
        }

        private void CurrentScript_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (ScriptFiles.Text == "")
                return;

            TestScriptFile = Application.ExecutablePath + "\\..\\Scripts\\" + ScriptFiles.Text;

            TestScript.Lines = File.ReadAllLines(TestScriptFile);
            TestScript.Enabled = true;
        }

        private void CurrentResults_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                FileInfo TestFile = new FileInfo(TestScriptFile);
                string RunLogFile = Application.ExecutablePath + "\\..\\Logs\\" + TestFile.Name.Replace(".txt", "") + "\\" + RunResults.Text;

                TestScript.Lines = File.ReadAllLines(RunLogFile);
                TestScript.Enabled = false;
            }
        }

        private void ClearLogs_Click(object sender, EventArgs e)
        {
            try
            {
                FileInfo TestFile = new FileInfo(TestScriptFile);
                string RunLogFile = Application.ExecutablePath + "\\..\\Logs\\" + TestFile.Name.Replace(".txt", "") + "\\" + RunResults.Text;

                new FileInfo(RunLogFile).Delete();
            }
            catch (Exception Ex)
            {
            }
        }

        private void ClearAllLogs_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult Result = MessageBox.Show("Are you sure you want to delete all log files?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (Result == DialogResult.Yes)
                {
                    FileInfo TestFile = new FileInfo(TestScriptFile);
                    DirectoryInfo LogFolder = new DirectoryInfo(Application.ExecutablePath + "\\..\\Logs\\" + TestFile.Name.Replace(".txt", "") + "\\");
                    foreach (FileInfo RunLogFile in LogFolder.GetFiles())
                    {
                        RunLogFile.Delete();
                    }
                }
            }
            catch (Exception Ex)
            {
            }
        }

        private void ListAllWindows(ComboBox CParam)
        {
            var collection = new List<string>();
            EnumDelegate filter = delegate(IntPtr hWnd, int lParam)
            {
                StringBuilder strbTitle = new StringBuilder(255);
                int nLength = GetWindowText(hWnd, strbTitle, strbTitle.Capacity + 1);
                string strTitle = strbTitle.ToString();

                if (IsWindowVisible(hWnd) && string.IsNullOrEmpty(strTitle) == false)
                {
                    collection.Add(strTitle);
                }
                return true;
            };

            CParam.Items.Clear();

            if (EnumDesktopWindows(IntPtr.Zero, filter, IntPtr.Zero))
            {
                foreach (var item in collection)
                {
                    CParam.Items.Add (item);
                }
            }
        }

        private void RunFromCursor_Click(object sender, EventArgs e)
        {
            int idxTestScript = 0;
            string[] tempArray = TestScript.Lines;

            int index = TestScript.SelectionStart;
            int line = TestScript.GetLineFromCharIndex(index);

            StreamWriter swRun = new StreamWriter(TempScriptFile);
            swRun.WriteLine (";Test script file ;"); //header, without which the file won't run.
            for (idxTestScript = line; idxTestScript < tempArray.Length; idxTestScript++)
                swRun.WriteLine(tempArray[idxTestScript]);

            swRun.Close();

            RunTestScript("\"" + TempScriptFile + "\"");
        }

        private void RunToCursor_Click(object sender, EventArgs e)
        {
            int idxTestScript = 0;
            string[] tempArray = TestScript.Lines;

            int index = TestScript.SelectionStart;
            int line = TestScript.GetLineFromCharIndex(index);

            StreamWriter swRun = new StreamWriter(TempScriptFile);
            for (idxTestScript = 0; idxTestScript <= line; idxTestScript++)
                swRun.WriteLine(tempArray[idxTestScript]);

            swRun.Close();

            RunTestScript("\"" + TempScriptFile + "\"");
        }

        private void RunThisBlock_Click(object sender, EventArgs e)
        {
            SaveThisBlock();
            RunTestScript("\"" + TempScriptFile + "\"");

        }

        private void SaveThisBlock()
        {
            string[] tempArray = TestScript.Lines;

            string SelText = TestScript.SelectedText;

            StreamWriter swRun = new StreamWriter(TempScriptFile);
            swRun.WriteLine(";Test script file ;"); //header, without which the file won't run.

            foreach (string SelTextLine in SelText.Split('\n'))
                swRun.WriteLine(SelTextLine);

            swRun.Close();
        }

        private void FindText_TextChanged(object sender, EventArgs e)
        {
            ClearHighlight(TestScript, searchList);
            searchList = HighlightAll(TestScript, FindText.Text);
        }

        private static List<SearchData> HighlightAll(RichTextBox rtb, string searchText)
        {
            List<SearchData> list = new List<SearchData>();
            if (searchText.Length >= 0)
            {
                int startindex = 0;
                int searchlength = searchText.Length;
                while ((startindex < rtb.TextLength) && 0 <= (startindex = rtb.Find(searchText, startindex, RichTextBoxFinds.None)))
                {
                    SearchData item;
                    item = new SearchData();
                    item.StartIndex = startindex;
                    item.Length = searchlength;
                    item.SelectionRTF = rtb.SelectedRtf;
                    list.Insert(0, item);

                    rtb.SelectionBackColor = Color.Yellow;
                    startindex += searchlength;
                }
            }
            return list;
        }

        private void ColorFunctions ()
        {
            foreach (string FunctionName in Dict.Keys)
            {
                int startindex = 0;
                int searchlength = FunctionName.Length;
                while ((startindex < TestScript.TextLength) && 0 <= (startindex = TestScript.Find(FunctionName, startindex, RichTextBoxFinds.None)))
                {
                    TestScript.SelectionColor = Color.DarkGreen;

                    if (TestScript.SelectionFont != null)
                    {
                        Font currentFont = TestScript.SelectionFont;
                        FontStyle newFontStyle = FontStyle.Bold;

                        TestScript.SelectionFont = new Font(
                        currentFont.FontFamily,
                        currentFont.Size,
                        newFontStyle
                        );
                    }
                    startindex += searchlength;
                }
            }
        }

        private void ClearHighlight(RichTextBox rtb, List<SearchData> list)
        {
            if (list != null)
            {
                foreach (SearchData item in list)
                {
                    rtb.Select(item.StartIndex, item.Length);
                    rtb.SelectedRtf = item.SelectionRTF;
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!(e.CloseReason.ToString () == "UserClosing")) //Application.OpenForms.Count > 1
            {
                this.Hide();
                e.Cancel = true;
            }
        }

        private void SyncLines_Click(object sender, EventArgs e)
        {
            Point TopLeft = new Point(0, 10);
            int index = TestScript.GetCharIndexFromPosition(TopLeft);
            int line = TestScript.GetLineFromCharIndex(index);
            LineNum.Text = (line + 1).ToString();
            int LineNumber = Int32.Parse(LineNum.Text);
            for (int idx = LineNumber + 1; idx < (LineNumber + 35); idx++)
            {
                LineNum.Text += "\n" + idx.ToString();
            }

        }
        private void PopulateDict()
        {
            string rx = "case \"(.*)\"";
            string rx1 = "(.*)\\((.*)\\)";
            string KeyName = "", ValueName = "";

            Dict.Clear();
            NoArgFunctions.Clear();

            StreamReader sr = new StreamReader(Automateau3Path);

            //Read the first line of text
            string line = sr.ReadLine();
            Match m = null, m1 = null; 
            while (line != null)
            {
                if (line.Trim ().StartsWith(";")) //ignore and proceed
                {
                    line = sr.ReadLine();
                    continue;
                }

                m = Regex.Match(line, rx, RegexOptions.IgnoreCase);
                if (m.Success)
                    KeyName = m.Groups[1].ToString(); //Function name
                else
                {
                    m1 = Regex.Match(line, rx1, RegexOptions.IgnoreCase);
                    if (m1.Success)
                    {
                        ValueName = m1.Groups[1].ToString().Trim(); //Internal name
                        if ((m1.Groups[2].ToString() == "") && (KeyName != "")) //no arguments for this function
                            NoArgFunctions.Add (ValueName);
                        if (KeyName != "" && ValueName != "")
                            try
                            {
                                Dict.Add(KeyName, ValueName);
                            }
                            catch
                            {
                            }
                        KeyName = ""; ValueName = "";
                    }
                }
                line = sr.ReadLine();
            }
            //close the file
            sr.Close();
        }

        private void au3WindowInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process process = new Process();
            string ToolPath = Application.ExecutablePath + "\\..\\Tools\\";

            if (Is64Bit())
                ToolPath += "Au3Info_x64.exe";
            else
                ToolPath += "Au3Info.exe";

            ProcessStartInfo startInfo = new ProcessStartInfo(ToolPath);
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
        }

        private bool Is64Bit()
        {
            if (IntPtr.Size == 8 || (IntPtr.Size == 4 && Is32BitProcessOn64BitProcessor()))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool Is32BitProcessOn64BitProcessor()
        {
            bool retVal;

            IsWow64Process(Process.GetCurrentProcess().Handle, out retVal);

            return retVal;
        }

        private void ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem TS = (ToolStripMenuItem)sender;
            toolCategoryItems.Text = TS.ToolTipText;
        }

        private void toolStripMenuItem1_TextChanged(object sender, EventArgs e)
        {
            CategoryChanged(sender, e);
        }

        private void helpToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Process.Start(Application.ExecutablePath + "\\..\\Help\\readme.mht");
        }

        private void BreakpointButton_Click(object sender, EventArgs e)
        {
            toolCategoryItems.Text = "Common-Basic-StepWise";
            Functions.SelectedIndex = 2; //Breakpoint
        }

        private void InsertSleepButton_Click(object sender, EventArgs e)
        {
            toolCategoryItems.Text = "Common-Basic-StepWise";
            Functions.SelectedIndex = 1; //InsertSleep
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                ShowInTaskbar = false;
                notifyIcon1.Visible = true;
            }
            else
            {
                ShowInTaskbar = true;
                notifyIcon1.Visible = false;
            }
        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Normal;
        }

        private void Parametrize_Click(object sender, EventArgs e)
        {
            toolCategoryItems.Text = "Common-Basic-StepWise";
            Functions.SelectedIndex = 3; //Parameterize
        }

        private void macrosList_TextChanged(object sender, EventArgs e)
        {
        }

        private void autoItv3HelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!File.Exists(au3Help))
            {
                MessageBox.Show("You don't seem to have AutoItv3 installed in your system.\n\n",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Process process = new Process();

            ProcessStartInfo startInfo = new ProcessStartInfo(au3Help);
            process.StartInfo = startInfo;
            process.Start();
        }

        private void FunctionsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            string EmbeddedText = TestScript.SelectedText;

            if (FunctionsList.Text == "If")
            {
                //toolCategoryItems.Text = "AutoIt syntax";
                //Functions.SelectedIndex = FunctionsList.SelectedIndex + 1; //AutoIt
                TestScript.SelectedText = AutoItPrefix + FunctionsList.Text + " <expression> then ;\n" +
                                            EmbeddedText + "\n" +
                                            AutoItPrefix + "Else ;\n" +
                                            AutoItPrefix + "EndIf ;";
            }

            if (FunctionsList.Text == "While")
            {
                TestScript.SelectedText = AutoItPrefix + FunctionsList.Text + " <expression> ;\n" +
                                            EmbeddedText + "\n" +
                                            AutoItPrefix + "Wend ;";
            }

            if (FunctionsList.Text == "Do")
            {
                TestScript.SelectedText = AutoItPrefix + FunctionsList.Text + " ;\n" +
                                            EmbeddedText + "\n" +
                                            AutoItPrefix + "Until <expression> ;";
            }

            if (FunctionsList.Text == "For...Next")
            {
                TestScript.SelectedText = AutoItPrefix + FunctionsList.Text + " <variable> = <start> To <stop> [Step <stepval>] ;\n" +
                                            EmbeddedText + "\n" +
                                            AutoItPrefix + "Next ;";
            }

            if (FunctionsList.Text == "For...In...Next")
            {
                TestScript.SelectedText = AutoItPrefix + FunctionsList.Text + " <$Variable> In <expression> ;\n" +
                                            EmbeddedText + "\n" +
                                            AutoItPrefix + "Next ;";
            }

            contextMenuStrip2.Close();
        }

        private void StmtList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (StmtList.Text != "")
            {
                TestScript.SelectedText = AutoItPrefix + StmtList.Text + "\n";
            }

            contextMenuStrip2.Close();
        }

        private void FunctionsList_Paint(object sender, PaintEventArgs e)
        {
            FunctionsList.Text = "Control-Flow";
        }

        private void StmtList_Paint(object sender, PaintEventArgs e)
        {
            StmtList.Text = "Statements";
        }

        private void highlightFunctionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorFunctions();
        }

        private void TestScript_Click(object sender, EventArgs e)
        {
            FindText.Clear();
        }

        private void convertBlockToFunctionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveThisBlock();
            string[] TempScriptLines = File.ReadAllLines(TempScriptFile);
            string FunctionName = SaveAsUDF(TempScriptLines);

            toolCategoryItems.Text = "UDF";
            
            int IdxUDF = 0;
            foreach (object Item in Functions.Items)
            {
                if (Item.ToString () == FunctionName)
                {
                    Functions.SelectedIndex = IdxUDF; //new UDF
                    break;
                }
                IdxUDF++;
            }
        }

        private void OpenEngineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog.Filter = "au3 files|*.au3|All files|*.*";
            OpenFileDialog.InitialDirectory = Path.GetFullPath(Automateau3Path + "\\..\\");
            if (OpenFileDialog.ShowDialog() == DialogResult.OK)
                Process.Start(OpenFileDialog.FileName);
        }

        private void saveAsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SaveScript();
        }

        private void SystemMacros_Paint(object sender, PaintEventArgs e)
        {
            SystemMacros.Text = "System";
        }

        private void AutoItMacros_Paint(object sender, PaintEventArgs e)
        {
            AutoItMacros.Text = "Script";
        }

        private void DirectoryMacros_Paint(object sender, PaintEventArgs e)
        {
            DirectoryMacros.Text = "Directory";
        }

        private void DateTimeMacros_Paint(object sender, PaintEventArgs e)
        {
            DateTimeMacros.Text = "Date and Time";
        }

        private void SystemMacros_SelectedIndexChanged(object sender, EventArgs e)
        {
            ToolStripComboBox macrosCombo = (ToolStripComboBox)sender;

            string macro = macrosCombo.SelectedItem.ToString();
            if (macro != "")
            {
                TestScript.SelectedText = macro;
            }
            contextMenuStrip2.Close();
        }

        private void locateCodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string InternalName = "";
            int counter = 0;
            string line;
            string FoundInFile = "";

            if (Dict.TryGetValue(TestScript.SelectedText, out InternalName))
            {
                foreach (string au3File in Directory.GetFiles (Path.GetFullPath (Automateau3Path + "\\..\\")))
                {
                    counter = 0;
                    StreamReader file = new System.IO.StreamReader(au3File);
                    while((line = file.ReadLine()) != null)
                    {
                        counter++;
                        if (line.StartsWith("Func " + InternalName))
                        {
                            FoundInFile = au3File;
                            break;
                        }
                    }

                    file.Close();
                    if (FoundInFile != "") break;
                }
            }

            DialogResult Result;
            if (FoundInFile == "")
                MessageBox.Show("Function (" + TestScript.SelectedText + ") could not be located in any of the .au3 files", "Locate code");
            else
            {
                FileInfo file = new FileInfo(FoundInFile);
                Result = MessageBox.Show("Selected function (" + TestScript.SelectedText + ") located at " +
                                    "line " + counter + " in " + file.Name + "\n\n" +
                                    "Do you want to open the file?", "Locate code", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (Result == DialogResult.Yes)
                    Process.Start(FoundInFile);
            }
        }

        private void insertBreakpointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int index = TestScript.SelectionStart;
            int currentline = TestScript.GetLineFromCharIndex(index);
            TestScript.SelectionStart = TestScript.GetFirstCharIndexFromLine (currentline);
            TestScript.SelectedText = "\n";
            
            currentline = TestScript.GetLineFromCharIndex(index);
            TestScript.SelectionStart = TestScript.GetFirstCharIndexFromLine(currentline - 1);

            toolCategoryItems.Text = "Common-Basic-System";
            Functions.SelectedIndex = 2; //Breakpoint
        }
    }

    public class ComboBoxItem
    {
        public string Text;
        public Color BackColor;

        public ComboBoxItem()
        {
            Text = string.Empty;
            BackColor = Color.Black;
        }
        public ComboBoxItem(string text)
        {
            Text = text;
            BackColor = Color.Black;
        }
        public ComboBoxItem(string text, Color color)
        {
            Text = text;
            BackColor = color;
        }
        public override string ToString()
        {
            return Text;
        }
       
    }
}
