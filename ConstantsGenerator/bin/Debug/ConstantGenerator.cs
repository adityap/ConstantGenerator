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
                MessageBox.Show(ClsConst.STR_CONST_1);
                return;
            }

            if (!File.Exists(txtFile.Text))
            {
                MessageBox.Show(ClsConst.STR_CONST_2);
                return;
            }

            string strFileContent;
            using (StreamReader sr = new StreamReader(txtFile.Text))
            {
                strFileContent = sr.ReadToEnd();
            }

            string regex = ClsConst.STR_CONST_3\\\r\n]*(?:\\.[^\"\\\r\n]*)*";
            Regex ex = new Regex(regex,RegexOptions.IgnoreCase|RegexOptions.Multiline);
            MatchCollection mc = ex.Matches(strFileContent);

            Dictionary<string, string> strings = new Dictionary<string, string>();
            int counter = 1;
            foreach (Match m in mc)
            {
                if (!strings.ContainsValue(m.Value))
                {
                    strings.Add(ClsConst.STR_CONST_4 + counter++, m.Value);
                }
            }

            string strNamespace = Regex.Match(strFileContent,ClsConst.STR_CONST_5).Value;

            StringBuilder sbConstFileConent = new StringBuilder();

            sbConstFileConent.AppendLine(ClsConst.STR_CONST_6);
            sbConstFileConent.AppendLine(ClsConst.STR_CONST_7);
            sbConstFileConent.AppendLine();
            sbConstFileConent.AppendLine(strNamespace);
            if(!strNamespace.Contains(ClsConst.STR_CONST_8))
                sbConstFileConent.AppendLine(ClsConst.STR_CONST_8);
            sbConstFileConent.AppendLine(ClsConst.STR_CONST_10 + txtConstFileName);
            sbConstFileConent.AppendLine(ClsConst.STR_CONST_11);

            foreach (KeyValuePair<string, string> kvp in strings)
            {
                sbConstFileConent.AppendLine(string.Format(ClsConst.STR_CONST_12, kvp.Key, kvp.Value));
            }

            sbConstFileConent.AppendLine(ClsConst.STR_CONST_13);
            sbConstFileConent.AppendLine(ClsConst.STR_CONST_14);


            //Write to Const file
            string strConstFilePath = Path.GetDirectoryName(txtFile.Text) + ClsConst.STR_CONST_15 +  txtConstFileName.Text + Path.GetExtension(txtFile.Text);
            using (StreamWriter sr = new StreamWriter(strConstFilePath))
            {
                sr.Write(sbConstFileConent.ToString());
                sr.Flush();
            }

            //Backup the original code file
            File.Copy(txtFile.Text, Path.GetDirectoryName(txtFile.Text) + ClsConst.STR_CONST_15 + Path.GetFileNameWithoutExtension(txtFile.Text) + "_bkp" +
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
