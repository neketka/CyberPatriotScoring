using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScoringDesigner
{
    public partial class MainWindow : Form
    {
        private ListBox _sourceControl = null;
        public MainWindow()
        {
            InitializeComponent();
            PlatformTypeBox.SelectedIndex = 0;
        }


        private void ListContextMenu_Opening(object sender, CancelEventArgs e)
        {
            _sourceControl = (ListBox)ListContextMenu.SourceControl;
            editToolStripMenuItem.Enabled = deleteToolStripMenuItem.Enabled = _sourceControl.SelectedIndex >= 0;
        }

        private void AddNewToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
        private void EditToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void ClearAllToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
