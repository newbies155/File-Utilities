using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        bool locationSuppliedByArgs = false;
        string locationFromArgs = "";
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            if (Environment.GetCommandLineArgs().Length > 2)
            {
                locationSuppliedByArgs = true;
                locationFromArgs = Environment.GetCommandLineArgs()[2];
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Process process = new Process();
            if (locationSuppliedByArgs)
            {
                process.StartInfo.FileName = locationFromArgs + "WinFormsApp2.exe";
            } else
            {
                process.StartInfo.FileName = Environment.CurrentDirectory + "/encryptor/WinFormsApp2.exe";
            }
            if (checkBox1.Checked)
            {
                process.StartInfo.Arguments += " " + "FLATMODE";
            }
            if (checkBox2.Checked)
            {
                process.StartInfo.Arguments += "OPTIN";
            }
            process.Start();
            Application.Exit();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            panel1.Visible = !panel1.Visible;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                label4.Text = "You are currently opted-in to data collection, The data collected will be logged to data.log.";
            } else
            {
                label4.Text = "You are currently not opted-in to data collection";
            }
        }
    }
}
