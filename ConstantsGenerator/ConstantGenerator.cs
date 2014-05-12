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

namespace ConstantsGenerator
{
    public partial class frmConstantGenerator : Form
    {
        public frmConstantGenerator()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFile.Text))
            {
                MessageBox.Show("Select a code file to extract constants");
                return;
            }

            if (!File.Exists(txtFile.Text))
            {
                MessageBox.Show("File does not exists at the specified path");
                return;
            }

            if(string.IsNullOrWhiteSpace(txtConstFileName.Text))
            {
                MessageBox.Show("Provide a constant file name");
                return;
            }
            txtConstFileName.Text = txtConstFileName.Text.Trim();

            //Backup the original code file
            File.Copy(txtFile.Text, Path.GetDirectoryName(txtFile.Text) + "\\" + Path.GetFileNameWithoutExtension(txtFile.Text) + "_bkp" +
                Path.GetExtension(txtFile.Text));

            string strCurrentLine;
            List<string> consts = new List<string>();
            StringBuilder sbFileContent = new StringBuilder();
            int startQuotePos = -1;
            int endQuotePos = -1;
            Dictionary<string, string> strings = new Dictionary<string, string>();
            int counter = 1;
            string strNamespace = string.Empty;
            string strConstKey;
            using (StreamReader sr = new StreamReader(txtFile.Text))
            {
                //strFileContent = sr.ReadToEnd();
                while ((strCurrentLine = sr.ReadLine()) != null)
                {
                    sbFileContent.AppendLine(strCurrentLine);

                    if (strCurrentLine.Contains("namespace "))
                        strNamespace = strCurrentLine;

                    startQuotePos = strCurrentLine.IndexOf('"');
                    if (startQuotePos >= 0)
                    {
                        //search next quote until it is not escaped
                        while (startQuotePos > 0 && strCurrentLine.ElementAt(startQuotePos - 1) == '\\'
                            && strCurrentLine.ElementAt(startQuotePos - 2) == '\\')
                        {
                            startQuotePos = strCurrentLine.IndexOf('"', startQuotePos + 1);
                        }

                        //search end quote until it is not escaped
                        endQuotePos = strCurrentLine.IndexOf('"', startQuotePos + 1);
                        while (endQuotePos > 0 && strCurrentLine.ElementAt(endQuotePos - 1) == '\\'
                            && strCurrentLine.ElementAt(startQuotePos - 2) == '\\')
                        {
                            endQuotePos = strCurrentLine.IndexOf('"', endQuotePos + 1);
                        }

                        if (!strings.ContainsValue(strCurrentLine.Substring(startQuotePos, endQuotePos - startQuotePos)))
                        {
                            strConstKey = "STR_CONST_" + counter++;
                            strings.Add(strConstKey, strCurrentLine.Substring(startQuotePos, endQuotePos - startQuotePos + 1));


                        }
                    }
                }
            }

            string strFileContent = sbFileContent.ToString();

            StringBuilder sbConstFileConent = new StringBuilder();

            sbConstFileConent.AppendLine("using System;");
            sbConstFileConent.AppendLine("using System.Collections.Generic;");
            sbConstFileConent.AppendLine();
            sbConstFileConent.AppendLine(strNamespace);
            if(!strNamespace.Contains("{"))
                sbConstFileConent.AppendLine("{");
            sbConstFileConent.AppendLine("\tpublic static class " + txtConstFileName.Text);
            sbConstFileConent.AppendLine("\t{");

            foreach (KeyValuePair<string, string> kvp in strings)
            {
                sbConstFileConent.AppendLine(string.Format("\t\tpublic const string {0} = {1};", kvp.Key, kvp.Value));
            }

            sbConstFileConent.AppendLine("\t}");
            sbConstFileConent.AppendLine("}");


            //Write to Const file
            string strConstFilePath = Path.GetDirectoryName(txtFile.Text) + "\\" +  txtConstFileName.Text + Path.GetExtension(txtFile.Text);
            using (StreamWriter sr = new StreamWriter(strConstFilePath))
            {
                sr.Write(sbConstFileConent.ToString());
                sr.Flush();
            }

            ////Backup the original code file
            //File.Copy(txtFile.Text, Path.GetDirectoryName(txtFile.Text) + "\\" + Path.GetFileNameWithoutExtension(txtFile.Text) + "_bkp" +
            //    Path.GetExtension(txtFile.Text));

            //Replace the original code file

            foreach (KeyValuePair<string, string> kvp in strings)
            {
                strFileContent = strFileContent.Replace(kvp.Value, txtConstFileName.Text + "." + kvp.Key);
            }

            using(StreamWriter sr = new StreamWriter(txtFile.Text,false))
            {
                sr.Write(strFileContent);
                sr.Flush();
            }

            MessageBox.Show("Generation Complete");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog FD = new OpenFileDialog();
            if (FD.ShowDialog() == DialogResult.OK) 
                txtFile.Text = FD.FileName;


        }
    }
}
