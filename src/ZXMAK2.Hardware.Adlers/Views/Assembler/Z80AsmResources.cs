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
            this.treeZ80Resources.ExpandAll();

            _asmToAddSourceCode = i_asmToAddSourceCode;
        }

        private void ParseResourceFile()
        {
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
            _asmToAddSourceCode.Text += "this has been added by includes...\n";
        }
        #endregion GUI

        #region HTML
        private string GetHtmlFormatted(string i_htmlToFormat)
        {
            return "<body style=\"background-color:lightgrey;font-family:calibri;courier;\">" + i_htmlToFormat + "</body>";
        }
        #endregion HTML
    }
}
