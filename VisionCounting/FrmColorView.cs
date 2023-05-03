using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using MetroFramework.Forms;
using MetroFramework.Components;
using MetroFramework;

using DLCounting;

namespace VisionCounting
{
    public partial class FrmColorView : MetroForm
    {

        #region  NOCLOSE_BUTTON
        private const int CP_NOCLOSE_BUTTON = 0x200;
        private const int WS_SYSMENU = 0x80000;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.Style &= ~WS_SYSMENU;
                return cp;
            }
        }
        #endregion


        public Frmmain ParentForm { get; set; }

        public FrmColorView()
        {
            InitializeComponent();
        }

        private void FrmColorView_Load(object sender, EventArgs e)
        {

        }

        private void CkROI_CheckedChanged(object sender, EventArgs e)
        {
            if (ParentForm == null)
            {
                return;
            }

            ParentForm.UpdateROISelect(ckROI.Checked);

        }

        private void BtDone_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void CkTopMost_CheckedChanged(object sender, EventArgs e)
        {
            this.TopMost = ckTopMost.Checked;
        }

        private void BtRecheckColor_Click(object sender, EventArgs e)
        {
            if (ParentForm == null)
            {
                return;
            }

            ParentForm.ReCheckColor();
        }

        private void BtDoneBlob_Click(object sender, EventArgs e)
        {
            this.Hide();

        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (ckTopMost.Checked == true)
            {
                this.TopMost = true;
            }
            else
            {
                this.TopMost = false;
            }
        }

        /////////////////////END///////////////////////////
    }
}
