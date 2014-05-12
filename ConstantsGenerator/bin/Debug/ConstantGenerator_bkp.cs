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

            string strFileContent;
            using (StreamReader sr = new StreamReader(txtFile.Text))
            {
                strFileContent = sr.ReadToEnd();
            }

            string regex = "[^\"\\\r\n]*(?:\\.[^\"\\\r\n]*)*";
            Regex ex = new Regex(regex,RegexOptions.IgnoreCase|RegexOptions.Multiline);
            MatchCollection mc = ex.Matches(strFileContent);

            Dictionary<string, string> strings = new Dictionary<string, string>();
            int counter = 1;
            foreach (Match m in mc)
            {
                if (!strings.ContainsValue(m.Value))
                {
                    strings.Add("STR_CONST_" + counter++, m.Value);
                }
            }

            string strNamespace = Regex.Match(strFileContent,"^[namespace*]$").Value;

            StringBuilder sbConstFileConent = new StringBuilder();

            sbConstFileConent.AppendLine("using System;");
            sbConstFileConent.AppendLine("using System.Collections.Generic;");
            sbConstFileConent.AppendLine();
            sbConstFileConent.AppendLine(strNamespace);
            if(!strNamespace.Contains("{"))
                sbConstFileConent.AppendLine("{");
            sbConstFileConent.AppendLine("\tpublic static class " + txtConstFileName);
            sbConstFileConent.AppendLine("\t{");

            foreach (KeyValuePair<string, string> kvp in strings)
            {
                sbConstFileConent.AppendLine(string.Format("\t\tpublic const string {0} = {1}", kvp.Key, kvp.Value));
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

            //Backup the original code file
            File.Copy(txtFile.Text, Path.GetDirectoryName(txtFile.Text) + "\\" + Path.GetFileNameWithoutExtension(txtFile.Text) + "_bkp" +
                Path.GetExtension(txtFile.Text));

            //Replace the original code file

            foreach (KeyValuePair<string, string> kvp in strings)
            {
                Regex.Replace(strFileContent, kvp.Value, kvp.Key);
            }

            using(StreamWriter sr = new StreamWriter(txtFile.Text,false))
            {
                sr.Write(strFileContent);
            }
        }
    }
}
