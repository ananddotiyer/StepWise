using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Utilities;
using gma.System.Windows;
using System.Text.RegularExpressions;
//using MouseKeyboardLibrary;
using System.IO;
using System.Diagnostics;
using Microsoft.Win32;


namespace key_preview {
	public partial class Form1 : Form {
		globalKeyboardHook gkh = new globalKeyboardHook();
        //MouseHook mouseHook = new MouseHook();
        UserActivityHook mouseHook = new UserActivityHook(); // crate an instance with global hooks

        bool FlagKeys = false;
        bool FlagMouse = false;

        int CountKeys = 0;
        //New code start

        [DllImport("user32.dll")]
        public static extern int FindWindow(string lpClassName, string lpWindowName);
        
        [DllImport("user32.dll")]
        public static extern int SendMessage(int hWnd, uint Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetClientRect (IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner  
            public int Top;         // y position of upper-left corner  
            public int Right;       // x position of lower-right corner  
            public int Bottom;      // y position of lower-right corner  
        }

        RECT rct = new RECT();

        public const int WM_SYSCOMMAND = 0x0112;
        public const int SC_CLOSE = 0xF060;
        //New code end

        IntPtr WindowHandle = (IntPtr) 0;
        IntPtr OldHandle = (IntPtr) 0;

        bool PossibleExit = false;

        Dictionary<string, string> Dict = new Dictionary<string, string>();

        StreamWriter sw = new StreamWriter(Application.ExecutablePath + "\\..\\Log.txt");

        string OldX = "", OldY = "";

        bool IgnoreAbsoluteMouse = true; //by default, ignore mouse co-ordinates recorded using screen co-ordinates.

        bool RecordWindowLocation = false; //by default, don't record window location.

		public Form1() {
            InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e) {
            try
            {
                Keys[] KeysCollection = { Keys.A ,Keys.B ,Keys.C ,Keys.D ,Keys.E ,Keys.F ,Keys.G ,Keys.H ,Keys.I ,Keys.J ,Keys.K ,Keys.L ,Keys.M ,Keys.N ,Keys.O ,Keys.P ,Keys.Q ,Keys.R ,Keys.S ,Keys.T ,Keys.U ,Keys.V ,Keys.W ,Keys.X ,Keys.Y ,Keys.Z, 
                                        Keys.D1 ,Keys.D2 ,Keys.D3 ,Keys.D4 ,Keys.D5 ,Keys.D6 ,Keys.D7 ,Keys.D8 ,Keys.D9 ,Keys.D0,
                                        Keys.F1, Keys.F7, Keys.F8, Keys.F9, Keys.F10, Keys.F11, Keys.F12, Keys.Delete, Keys.Home, Keys.End,
                                        //Keys.Escape, Keys.F2, Keys.F3, Keys.F4, Keys.F5, Keys.F6, Keys.F10, Commented because these are used as short-cut keys in the main app.
                                        Keys.Oemtilde, Keys.OemMinus, Keys.Oemplus, Keys.Back, 
                                        Keys.Tab, Keys.OemOpenBrackets, Keys.Oem6, Keys.Oem5, 
                                        Keys.Oem1, Keys.Oem7, Keys.Return, //Strangely Keys.LButton is mapped to the {ENTER} key in keyboard. 
                                        Keys.LShiftKey, Keys.Oemcomma, Keys.OemPeriod, Keys.OemQuestion, 
                                        Keys.LControlKey, Keys.LWin, Keys.LMenu,Keys.Space, Keys.Up, Keys.Left, Keys.Right, Keys.Down};

                List<Keys> KeyList = new List<Keys>(KeysCollection);

                Dict.Add("A", "a"); Dict.Add("B", "b"); Dict.Add("C", "c"); Dict.Add("D", "d"); Dict.Add("E", "e"); Dict.Add("F", "f"); Dict.Add("G", "g"); Dict.Add("H", "h"); Dict.Add("I", "i"); Dict.Add("J", "j"); Dict.Add("K", "k"); Dict.Add("L", "l"); Dict.Add("M", "m");
                Dict.Add("N", "n"); Dict.Add("O", "o"); Dict.Add("P", "p"); Dict.Add("Q", "q"); Dict.Add("R", "r"); Dict.Add("S", "s"); Dict.Add("T", "t"); Dict.Add("U", "u"); Dict.Add("V", "v"); Dict.Add("W", "w"); Dict.Add("X", "x"); Dict.Add("Y", "y"); Dict.Add("Z", "z");
                Dict.Add("D1", "1"); Dict.Add("D2", "2"); Dict.Add("D3", "3"); Dict.Add("D4", "4"); Dict.Add("D5", "5"); Dict.Add("D6", "6"); Dict.Add("D7", "7"); Dict.Add("D8", "8"); Dict.Add("D9", "9"); Dict.Add("D0", "0");
                Dict.Add("F1", "{F1}"); Dict.Add("F7", "{F7}"); Dict.Add("F8", "{F8}"); Dict.Add("F9", "{F9}"); Dict.Add("F11", "{F11}"); Dict.Add("F12", "{F12}"); Dict.Add("Delete", "{DELETE}"); Dict.Add("Home", "{HOME}"); Dict.Add("End", "{END}");
                //Dict.Add("Escape", "{ESCAPE}"); Dict.Add("F2", "{F2}"); Dict.Add("F3", "{F3}"); Dict.Add("F4", "{F4}"); Dict.Add("F5", "{F5}"); Dict.Add("F6", "{F6}"); Dict.Add("F10", "{F10}"); Commented because these are used as short-cut keys in the main app.
                Dict.Add("Oemtilde", "`"); Dict.Add("OemMinus", "-"); Dict.Add("Oemplus", "="); Dict.Add("Back", "{BACKSPACE}");
                Dict.Add("Tab", "{TAB}"); Dict.Add("OemOpenBrackets", "["); Dict.Add("Oem6", "]"); Dict.Add("Oem5", "\\");
                Dict.Add("Oem1", ";"); Dict.Add("Oem7", "'"); Dict.Add("Return", "{ENTER}");
                Dict.Add("LShiftKey-Down", "{SHIFTDOWN}"); Dict.Add("LShiftKey-Up", "{SHIFTUP}"); Dict.Add("Oemcomma", ","); Dict.Add("OemPeriod", "."); Dict.Add("OemQuestion", "/");
                Dict.Add("LControlKey-Down", "{CTRLDOWN}"); Dict.Add("LControlKey-Up", "{CTRLUP}"); Dict.Add("LWin", "{LWIN}"); Dict.Add("LMenu", "!"); Dict.Add("Space", "{SPACE}"); Dict.Add("Up", "{UP}"); Dict.Add("Down", "{DOWN}"); Dict.Add("Left", "{LEFT}"); Dict.Add("Right", "{RIGHT}");


                gkh.HookedKeys.AddRange(KeyList);
                gkh.KeyDown += new KeyEventHandler(gkh_KeyDown);
                gkh.KeyUp += new KeyEventHandler(gkh_KeyUp);
                gkh.unhook();

                // mouse events
                mouseHook.OnMouseDown += new MouseEventHandler(mouseHook_MouseDown);
                mouseHook.OnMouseUp += new MouseEventHandler(mouseHook_MouseUp);
                mouseHook.Stop();

                Keyboarded.Checked = true;
                Moused.Checked = true;

                if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\StepWise", "IgnoreAbsoluteMouse", null).ToString() == "0")
                    IgnoreAbsoluteMouse = false;

                if (Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\StepWise", "RecordWindowLocation", null).ToString() == "1")
                    RecordWindowLocation = true;

            }
            catch (Exception Ex)
            {
                sw.WriteLine("Form1_Load: " + Ex.Message);
                sw.WriteLine(Ex.StackTrace);
            }
        }

		void gkh_KeyUp(object sender, KeyEventArgs e) 
        {
            try
            {
                WindowHandle = GetForegroundWindow();
                if (OldHandle != WindowHandle)
                {
                    if (Regex.IsMatch(GetActiveWindowTitle(WindowHandle), "\\w+")) //added condition 4.10.12
                    {
                        GetWindowRect(WindowHandle, out rct);
                        richTextBox1.Text += ";\nWinWaitActive " + GetActiveWindowTitle(WindowHandle);

                        if (RecordWindowLocation)
                            richTextBox1.Text += "," + rct.Left + "," + rct.Top + "," + (rct.Right - rct.Left).ToString() + "," + (rct.Bottom - rct.Top).ToString();
                    }
                    richTextBox1.Text += ";\nSend ";
                    OldHandle = WindowHandle;
                }

                if (e.KeyCode.ToString() == "LControlKey" || e.KeyCode.ToString() == "LShiftKey")
                {
                    string KeyCodeString;
                    string ValueCode = "";

                    KeyCodeString = e.KeyCode.ToString() + "-Up";
                    Dict.TryGetValue(KeyCodeString, out ValueCode);
                    if (!FlagKeys)
                    {
                        richTextBox1.Text += ";\nSend ";
                        FlagKeys = true;
                        FlagMouse = false;
                    }
                    richTextBox1.Text += "" + ValueCode;
                    //e.Handled = true;
                }
            }
            catch (Exception Ex)
            {
                sw.WriteLine("gkh_KeyUp: " + Ex.Message);
                sw.WriteLine(Ex.StackTrace);
            }
		}

		void gkh_KeyDown(object sender, KeyEventArgs e) 
        {
            try
            {
                string KeyCodeString;
                string ValueCode = "";

                if (e.KeyCode.ToString() == "LControlKey" || e.KeyCode.ToString() == "LShiftKey")
                {
                    KeyCodeString = e.KeyCode.ToString() + "-Down";
                }
                else
                {
                    KeyCodeString = e.KeyCode.ToString();
                }

                Dict.TryGetValue(KeyCodeString, out ValueCode);

                //Trapping the custom form close event (Delete+Esc)
                if (ValueCode == "{DELETE}" && PossibleExit)
                {
                    int hWnd = FindWindow(null, "Recording keystrokes/mouseclicks");
                    if (hWnd > 0)
                    {
                        CleanUp();
                        SendMessage(hWnd, WM_SYSCOMMAND, SC_CLOSE, 0);
                    }
                }

                if (ValueCode == "{DELETE}")
                    PossibleExit = true;
                else
                    PossibleExit = false;
                //newcode

                if (!FlagKeys)
                {
                    richTextBox1.Text += ";\nSend ";
                    FlagKeys = true;
                    FlagMouse = false;
                }

                if (!PossibleExit)
                    richTextBox1.Text += "" + ValueCode;
                
                //e.Handled = true;
            }
            catch (Exception Ex)
            {
                sw.WriteLine("gkh_KeyDown: " + Ex.Message);
                sw.WriteLine(Ex.StackTrace);
            }
		}

         void mouseHook_MouseUp(object sender, MouseEventArgs e)
        {
            ConvertToClient(
                mouseHook.UpDown,
                e.Clicks,
                e.Button.ToString().ToLower(),
                Int32.Parse(e.X.ToString()),
                Int32.Parse(e.Y.ToString()),
                !Screen.Checked
                );
        }

        void mouseHook_MouseDown(object sender, MouseEventArgs e)
        {
            ConvertToClient(
                mouseHook.UpDown,
                e.Clicks, //added
                e.Button.ToString().ToLower(),
                Int32.Parse(e.X.ToString()),
                Int32.Parse(e.Y.ToString()),
                !Screen.Checked
                );
        }

        void AddMouseEvent(string eventType, int NumClicks, string button, string x, string y, bool Client)
        {
            int ClientValue = 1;

            try
            {
                if (!Client)
                {
                    ClientValue = 0;
                }

                switch (eventType)
                {
                    case "MouseDown":
                        if (IgnoreAbsoluteMouse)
                        {
                            if (ClientValue != 0)
                            {
                                OldX = x; OldY = y;
                                richTextBox1.Text += ";\nMouseClickEx " + button + "," + x.ToString() + "," + y.ToString() + "," + NumClicks.ToString() + "," + ClientValue + ";\n";
                                sw.WriteLine(";\nMouseClickEx " + button + "," + x.ToString() + "," + y.ToString() + "," + ClientValue + ";\n");
                            }
                        }
                        else
                        {
                            OldX = x; OldY = y;
                            richTextBox1.Text += ";\nMouseClickEx " + button + "," + x.ToString() + "," + y.ToString() + "," + NumClicks.ToString() + "," + ClientValue + ";\n";
                            sw.WriteLine(";\nMouseClickEx " + button + "," + x.ToString() + "," + y.ToString() + "," + ClientValue + ";\n");
                        }
                        break;
                    case "MouseUp":
                        if (IgnoreAbsoluteMouse)
                        {
                            if (ClientValue != 0)
                            {
                                if ((OldX != x && Int32.Parse(OldX) > 0) && (OldY != y && Int32.Parse(OldY) > 0)) //&& Client
                                {
                                    richTextBox1.Text += ";\nMouseClickDragEx " + button + ","
                                        + OldX + "," + OldY + ","
                                        + x + "," + y + "," + ClientValue + ";\n";
                                    sw.WriteLine(";\nMouseClickDragEx " + button + ","
                                        + OldX + "," + OldY + ","
                                        + x + "," + y + "," + ClientValue + ";\n");
                                    OldX = ""; OldY = "";
                                }
                            }
                        }
                        else
                        {
                            if ((OldX != x && Int32.Parse(OldX) > 0) && (OldY != y && Int32.Parse(OldY) > 0)) //&& Client
                            {
                                richTextBox1.Text += ";\nMouseClickDragEx " + button + ","
                                    + OldX + "," + OldY + ","
                                    + x + "," + y + "," + ClientValue + ";\n";
                                sw.WriteLine(";\nMouseClickDragEx " + button + ","
                                    + OldX + "," + OldY + ","
                                    + x + "," + y + "," + ClientValue + ";\n");
                                OldX = ""; OldY = "";
                            }
                        }
                        break;
                    //case "MouseMove":
                    //richTextBox1.Text = "MouseMove (" + x.ToString() + ", " + y.ToString() + ")";
                    //break;
                }
            }
            catch (Exception Ex)
            {
                sw.WriteLine("AddMouseEvent:eventType: " + Ex.Message);
                sw.WriteLine(Ex.StackTrace);
            }
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText (richTextBox1.Text);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            CleanUp();
        }

        private void ConvertToClient (string UpDown, int NumClicks, string MouseButton, int X, int Y, bool Client)
        {
            WindowHandle = GetForegroundWindow();
            if (OldHandle != WindowHandle)
            {
                Client = false; //Now Client variable indicates same window.
            }
            else
                if (Client)
                {
                    GetWindowRect(WindowHandle, out rct);

                    sw.WriteLine();
                    sw.WriteLine(GetActiveWindowTitle(OldHandle) + " = " + GetActiveWindowTitle(WindowHandle));
                    sw.WriteLine(rct.Left + "\t" + rct.Top);
                    X = Math.Abs(X - rct.Left);
                    Y = Math.Abs(Y - rct.Top);
                }

            sw.WriteLine("AddMouseEvent\t" + UpDown + "\t" + MouseButton + "\t" + X.ToString() + "\t" + Y.ToString() + "\t" + Client);

            //new
            try
            {
                if (OldHandle != WindowHandle && UpDown == "MouseUp")
                {
                    if (Regex.IsMatch(GetActiveWindowTitle(WindowHandle), "\\w+")) //added condition 4.10.12
                    {
                        GetWindowRect(WindowHandle, out rct);
                        richTextBox1.Text += ";\nWinWaitActive " + GetActiveWindowTitle(WindowHandle);

                        if (RecordWindowLocation)
                            richTextBox1.Text += "," + rct.Left + "," + rct.Top + "," + (rct.Right - rct.Left).ToString() + "," + (rct.Bottom - rct.Top).ToString();

                        richTextBox1.Text += ";\n";
                    }
                    OldHandle = WindowHandle;
                }
            }
            catch
            {
                //do nothing
            }
            //new

            AddMouseEvent(
                UpDown,
                NumClicks,
                MouseButton,
                X.ToString(),
                Y.ToString(),
                Client
                );

            if (!FlagMouse)
            {
                richTextBox1.Text += ";\n";
                FlagMouse = true;
                FlagKeys = false;
            }
        }

        private string GetActiveWindowTitle(IntPtr WindowHandle)
        {
            const int nChars = 256;
            IntPtr handle = IntPtr.Zero;
            StringBuilder Buff = new StringBuilder(nChars);

            if (GetWindowText(WindowHandle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
        }

        private void Keyboarded_CheckedChanged(object sender, EventArgs e)
        {
            richTextBox1.Enabled = true;

            if (Keyboarded.Checked)
            {
                if (Moused.Checked)
                    Form1.ActiveForm.Text = "Recording keystrokes/mouseclicks";
                else
                    Form1.ActiveForm.Text = "Recording keystrokes";
                gkh.hook();
            }
            else
            {
                if (Moused.Checked)
                    Form1.ActiveForm.Text = "Recording mouseclicks";
                else
                    Form1.ActiveForm.Text = "Not recording";
                gkh.unhook();
            }
        }

        private void Moused_CheckedChanged(object sender, EventArgs e)
        {
            richTextBox1.Enabled = true;

            if (Moused.Checked)
            {
                if (Keyboarded.Checked)
                    Form1.ActiveForm.Text = "Recording keystrokes/mouseclicks";
                else
                    Form1.ActiveForm.Text = "Recording mouseclicks";
                mouseHook.Start();
            }
            else
            {
                if (Keyboarded.Checked)
                    Form1.ActiveForm.Text = "Recording keystrokes";
                else
                    Form1.ActiveForm.Text = "Not recording";
                mouseHook.Stop();
            }
        }

        private void CleanUp()
        {
            Clipboard.SetText (richTextBox1.Text);
            if (richTextBox1.Text == "")
                Clipboard.Clear();
        }
	}
}