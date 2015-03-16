using System;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Linq;
using ZXMAK2.Engine;
using FastColoredTextBoxNS;

namespace ZXMAK2.Hardware.Adlers.Views.AssemblerView
{
    public partial class Z80AsmResources : Form
    {
        private string _configFileName = "code_index.xml";
        private FastColoredTextBox _asmToAddSourceCode;

        public Z80AsmResources(ref FastColoredTextBox i_asmToAddSourceCode)
        {
            InitializeComponent();
            ParseResourceFile();

            _asmToAddSourceCode = i_asmToAddSourceCode;
        }

        private void ParseResourceFile()
        {
            treeZ80Resources.Nodes.Clear();

            if (!File.Exists(Path.Combine(Utils.GetAppFolder(), _configFileName)))
            {
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
                string errMessage;
                string fileContents = TcpHelper.GetFtpFileContents(_configFileName, out errMessage);
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
                if (fileContents != string.Empty)
                {
                    File.WriteAllText(_configFileName, fileContents);
                    this.Focus();
                }
                else
                    return;
            }

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(Path.Combine(Utils.GetAppFolder(), _configFileName));

            XmlNodeList nodes = xmlDoc.DocumentElement.SelectNodes("/Root/libs/item");
            foreach(XmlNode libNodes in nodes)
            {
                TreeNode treeNodeLib = new TreeNode();
                treeNodeLib.Text = libNodes.Attributes["file_name"].InnerText;
                treeNodeLib.Checked = true;
                treeNodeLib.Tag = libNodes;
                //Add lib routines
                foreach (XmlNode routineNodes in libNodes.SelectNodes("routine_list/routine"))
                {
                    TreeNode treeNodeRoutine = new TreeNode();
                    treeNodeRoutine.Text = routineNodes.Attributes["name"].InnerText;
                    treeNodeRoutine.Checked = true;
                    treeNodeRoutine.Tag = routineNodes;
                    treeNodeLib.Nodes.Add(treeNodeRoutine);
                }
                treeZ80Resources.Nodes.Add(treeNodeLib);
            }
            if( treeZ80Resources.Nodes.Count > 0 )
                this.treeZ80Resources.ExpandAll();
        }

        #region GUI
        private void treeZ80Resources_AfterSelect(object sender, TreeViewEventArgs e)
        {
            XmlNode treeNodeXml = (XmlNode)e.Node.Tag;
            htmlItemDesc.DocumentText = GetHtmlFormatted(treeNodeXml.SelectSingleNode("desc").InnerText);
        }

        private void treeZ80Resources_AfterCheck(object sender, TreeViewEventArgs e)
        {
            bool isCheckedActual = e.Node.Checked;
            foreach(TreeNode childNode in e.Node.Nodes)
            {
                if (childNode.Checked != isCheckedActual)
                    childNode.Checked = isCheckedActual;
            }
        }
        //Done
        private void buttonDone_Click(object sender, EventArgs e)
        {
            this.Hide();
            if (this.Owner != null)
                this.Owner.Focus();
        }
        //Add
        private void buttonAdd_Click(object sender, EventArgs e)
        {
            XmlNode xmlNode = (XmlNode)treeZ80Resources.SelectedNode.Tag;
            if (this.checkBoxHeaders.Checked)
            {
                string headerString = xmlNode.SelectSingleNode("header").InnerText;
                int headerLength = headerString.Length + 2;
                string paddingHeaderComment = new String('-', headerLength);
                paddingHeaderComment = "; " + paddingHeaderComment + "\n;\n;  " + headerString + "\n;\n; " + paddingHeaderComment;
                _asmToAddSourceCode.Text += paddingHeaderComment;
            }
            _asmToAddSourceCode.Text += xmlNode.SelectSingleNode("code").InnerText;
            //_asmToAddSourceCode.Text += "this has been added by includes...\n";
        }
        //Refresh
        private void buttonRefreshRoutineList_Click(object sender, EventArgs e)
        {
            ParseResourceFile();
        }
        #endregion GUI

        #region HTML
        private string GetHtmlFormatted(string i_htmlToFormat)
        {
            //string htmlOut = i_htmlToFormat.Replace(" ", @"&nbsp;");
            string css = "<style>table.routine_details {border=\"0\"; width=90%;} table.routine_defs{ border=\"1\"; cellpadding=\"10\"; } p.routineTitle {color:blue;display:inline;}";
            css += "table.routine_details tr td{ font-size: 12px; }";
            css += "</style>";
            string css_bodyStyle = "<body style=\"background-color:lightgrey;font-family:consolas,courier;\">";

            string htmlPrepared = "<!DOCTYPE html><html><head>" + css + "</head>" + css_bodyStyle + i_htmlToFormat + "</body></html>";

            return htmlPrepared;
        }
        #endregion HTML
    }
}
