using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Text.Json;
using System.Xml;

namespace WinFormsApp2
{
    public partial class Form1 : Form
    {
        Aes aes = Aes.Create();
        RSA rsa = RSA.Create();
        ICryptoTransform encryptor;
        ICryptoTransform decryptor;
        bool useRSA = false;
        bool handleCheckChanges = true;
        string locationFromArgs = "";
        public Form1()
        {
            InitializeComponent();
        }

        private void import_Click(object sender, EventArgs e)
        {
            MessageBox.Show(aes.Key.Length.ToString());
        }
 
        
        private void Form1_Load(object sender, EventArgs e)
        {
            locationFromArgs = Environment.GetCommandLineArgs()[1];
            Application.ApplicationExit += new EventHandler(onExit);
        }

        private void encrypt_Click(object sender, EventArgs e)
        {
            try
            {
                if (textBox1.Text == "")
                {
                    encryptor = aes.CreateEncryptor();
                }
                else
                {
                    if (useRSA)
                    {
                        int BytesRead;
                        rsa.ImportRSAPublicKey(Convert.FromBase64String(textBox1.Text.Split("*")[0]), out BytesRead);
                    }
                    else
                    {
                        encryptor = aes.CreateEncryptor(Convert.FromBase64String(textBox1.Text.Split("*")[0]), Convert.FromBase64String(textBox1.Text.Split("*")[1]));
                    }

                }
                if (useRSA)
                {
                    byte[] ciphertext = rsa.Encrypt(Encoding.UTF8.GetBytes(File.ReadAllText(textBox2.Text)), RSAEncryptionPadding.OaepSHA512);
                    File.WriteAllBytes(textBox2.Text, ciphertext);
                }
                else
                {
                    byte[] ciphertext = encryptor.TransformFinalBlock(Encoding.UTF8.GetBytes(File.ReadAllText(textBox2.Text)), 0, Encoding.UTF8.GetBytes(File.ReadAllText(textBox2.Text)).Length);
                    File.WriteAllBytes(textBox2.Text, ciphertext);
                }
                if (textBox1.Text == "")
                {
                    if (useRSA)
                    {
                        Clipboard.SetText(Convert.ToBase64String(rsa.ExportRSAPublicKey()) + "*" + Convert.ToBase64String(rsa.ExportRSAPrivateKey()));
                        MessageBox.Show("Encrypted file successfully, The password was copied to your clipboard!", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        Clipboard.SetText(Convert.ToBase64String(aes.Key) + "*" + Convert.ToBase64String(aes.IV));
                        MessageBox.Show("Encrypted file successfully, The password was copied to your clipboard!", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Encrypted file successfully", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                if (ex is CryptographicException)
                {
                    MessageBox.Show("Invalid key, Maybe try checking to see if the encryption type is correct?", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (ex is FileNotFoundException)
                {
                    MessageBox.Show("No such file exists.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (ex is ArgumentException)
                {
                    MessageBox.Show("Please insert a file name", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("Invalid key, Maybe try checking to see if the encryption type is correct?", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    File.WriteAllText("log.txt", ex.Data + "\nLink for advanced users (might not always be there): " + ex.HelpLink);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (textBox1.Text != "")
                {
                    if(useRSA)
                    {
                        int bytesRead;
                        rsa.ImportRSAPrivateKey(Convert.FromBase64String(textBox1.Text.Split("*")[1]), out bytesRead);
                        File.WriteAllText(textBox3.Text, Encoding.UTF8.GetString(rsa.Decrypt(File.ReadAllBytes(textBox3.Text), RSAEncryptionPadding.OaepSHA512)));
                        MessageBox.Show("Decrypted file successfully", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    } else
                    {
                        decryptor = aes.CreateDecryptor(Convert.FromBase64String(textBox1.Text.Split("*")[0]), Convert.FromBase64String(textBox1.Text.Split("*")[1]));
                        File.WriteAllText(textBox3.Text, Encoding.UTF8.GetString(decryptor.TransformFinalBlock(File.ReadAllBytes(textBox3.Text), 0, File.ReadAllBytes(textBox3.Text).Length)));
                        MessageBox.Show("Decrypted file successfully", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    if (useRSA)
                    {
                        File.WriteAllText(textBox3.Text, Encoding.UTF8.GetString(rsa.Decrypt(File.ReadAllBytes(textBox3.Text), RSAEncryptionPadding.OaepSHA512)));
                        MessageBox.Show("Decrypted file successfully", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        decryptor = aes.CreateDecryptor();
                        File.WriteAllText(textBox3.Text, Encoding.UTF8.GetString(decryptor.TransformFinalBlock(File.ReadAllBytes(textBox3.Text), 0, File.ReadAllBytes(textBox3.Text).Length)));
                        MessageBox.Show("Decrypted file successfully", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }catch(Exception ex)
            {
                if (ex is CryptographicException)
                {
                    MessageBox.Show("Invalid key, Maybe try checking to see if the encryption type is correct?", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (ex is FormatException)
                {
                    MessageBox.Show("Invalid file", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                } else if (ex is FileNotFoundException)
                {
                    MessageBox.Show("No such file exists."s, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                } else if (ex is ArgumentException)
                {
                    MessageBox.Show("Please insert a file name", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("An unknown error has occured, Stacktrace was written to file log.txt", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    File.WriteAllText("log.txt", ex.Message + ex.InnerException + "\n" + ex.StackTrace + "\nLink (might not always be there): " + ex.HelpLink);
                }
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (panel1.Visible)
            {
                panel1.Visible = false;
            }
            else
            {
                panel1.Visible = true;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            panel1.Visible = false;
        }

        private void HidePswds_CheckedChanged(object sender, EventArgs e)
        {
            if (HidePswds.Checked)
            {
                textBox1.PasswordChar = char.Parse("*");
            }
            else
            {
                textBox1.PasswordChar = new char();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if(!useRSA)
            {
                DialogResult reply;
                if (handleCheckChanges)
                {
                    reply = MessageBox.Show("You have enabled a checkbox that isnt recommended, Apply changes?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (reply == DialogResult.Yes)
                    {
                        useRSA = true;
                    }
                    else if (reply == DialogResult.No)
                    {
                        checkBox1.CheckedChanged -= new EventHandler(this.checkBox1_CheckedChanged);
                        checkBox1.Checked = false;
                        checkBox1.CheckedChanged += new EventHandler(this.checkBox1_CheckedChanged);
                    }
                }else
                {
                    useRSA = true;
                }
            } else
            {
                useRSA = false;
            }

        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            DialogResult result = folderDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                string path = folderDialog.SelectedPath + "\\Settings.json";
                try
                {
                    File.WriteAllText(path, "{ \"UseRSA\": " + "\"" + useRSA.ToString() + "\"" + ", \"HidePasswords\": " + "\"" + HidePswds.Checked.ToString() + "\"" + " }");
                }catch(UnauthorizedAccessException)
                {
                    MessageBox.Show("Can not export to folder, Please run the program as administrator.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            DialogResult result = fileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                string path = fileDialog.FileName;
                try
                {
                    var json = JsonDocument.Parse(File.ReadAllText(path));
                    handleCheckChanges = false;
                    checkBox1.Checked = bool.Parse(json.RootElement.GetProperty("UseRSA").ToString());
                    handleCheckChanges = true;
                    HidePswds.Checked = bool.Parse(json.RootElement.GetProperty("HidePasswords").ToString());
                }
                catch (Exception)
                {
                    MessageBox.Show("Invalid File", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();
            if(result == DialogResult.OK)
            {
                if (useRSA)
                {
                    File.WriteAllText(dialog.SelectedPath + "\\Key.txt", Convert.ToBase64String(rsa.ExportRSAPublicKey()) + "*" + Convert.ToBase64String(rsa.ExportRSAPrivateKey()));
                    MessageBox.Show("Exported key successfully", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    File.WriteAllText(dialog.SelectedPath + "\\Key.txt", Convert.ToBase64String(aes.Key) + "*" + Convert.ToBase64String(aes.IV));
                    MessageBox.Show("Exported key successfully", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                }
            }
        }
        private void onExit(object sender, EventArgs e)
        {
            Process process = new Process();
            process.StartInfo.FileName = locationFromArgs + "/WinFormsApp1.exe";
            process.StartInfo.Arguments += Environment.CurrentDirectory;
            process.Start();
            Application.Exit();
        }
    }
}
