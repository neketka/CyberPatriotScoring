using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WindowsScoringEngine
{
    public partial class ScoringService : ServiceBase
    {
        private bool m_enabled;
        private string m_path;
        public ScoringService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            
        }

        protected override void OnStop()
        {
        }

        private void CheckScoreTimer_Tick(object sender, EventArgs e)
        {

        }
    }
}
