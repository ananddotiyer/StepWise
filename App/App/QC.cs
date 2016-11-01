using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace App
{
    public partial class QC : Form
    {
        //TDAPIOLELib.TDConnection QCObject = new TDAPIOLELib.TDConnection();

        public QC()
        {
            InitializeComponent();
            this.webBrowser1.Navigate("http://plaengqc1.corp.nai.org/qcbin/start_a.htm");
        }

        private void QCConnect()
        {
            //string UserName = "qtpframework";
            //string Password = "Welcome2mcafee";
            //string DomainName = "MCAFEE";
            //string ProjectName = "ENDPOINT";
            //string ServerName = "http://192.168.10.66:8080/qcbin";

            //QCObject.InitConnectionEx(ServerName);
            //QCObject.Login(UserName, Password);
            //QCObject.Connect(DomainName, ProjectName);

            //if (QCObject.Connected)
            //    MessageBox.Show("Connected");
        }
        private void QC_FormClosed(object sender, FormClosedEventArgs e)
        {
            //QCObject.Disconnect();
            //QCObject.Logout();
            //QCObject.ReleaseConnection();
        }

        private void InitiateQC_Click(object sender, EventArgs e)
        {
            //QCConnect();

            ////TDAPIOLELib.TestFactory TestFact = (TDAPIOLELib.TestFactory) QCObject.TestFactory;
            ////TDAPIOLELib.ListClass TestFields = (TDAPIOLELib.ListClass) TestFact.Fields;

            ////for (int idx = 1; idx < TestFields.Count; idx++)

            ////    MessageBox.Show(TestFields[idx].ToString());

            //string setFilter = "Iteration 1";

            //TDAPIOLELib.TestSetFactory testSetFactory;
            //TDAPIOLELib.TestFactory testFactory;

            //// Test and Test Set filters
            //TDAPIOLELib.TDFilter testSetFilter;
            //TDAPIOLELib.TDFilter testFilter;

            //// Test and Test Set lists
            //TDAPIOLELib.List setList;
            //TDAPIOLELib.List testList;

            //// Set up the necessary factories and filters.
            //testSetFactory = (TDAPIOLELib.TestSetFactory)QCObject.TestSetFactory;
            //testSetFilter = (TDAPIOLELib.TDFilter)testSetFactory.Filter;
            //testFactory = (TDAPIOLELib.TestFactory)QCObject.TestFactory;
            //testFilter = (TDAPIOLELib.TDFilter)testFactory.Filter;

            //// Apply the test set filter
            //if (setFilter != "")
            //    testSetFilter["CY_CYCLE"] = "\"" + setFilter + "\"";
            //setList = testSetFactory.NewList(testSetFilter.Text);

            //// Apply the test case filter
            //testFilter.SetXFilter("TEST-TESTID", true, "19006");//testSetFilter.Text
            //testList = testFactory.NewList(testFilter.Text);
        }

        private void QC_FormClosing(object sender, FormClosingEventArgs e)
        {
            Form1.ActiveForm.Show();
            this.Hide();
            e.Cancel = true;
        }
    }
}
