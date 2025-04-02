using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StepBro.HostSupport.Models
{
    public class InteractiveHostAppModel : ObservableObject
    {
        private HostAppModel m_main;

        internal InteractiveHostAppModel(HostAppModel main) 
        {
            m_main = main;
            m_main.UpdatingCommandStates += Main_UpdatingCommandStates;
        }

        private void Main_UpdatingCommandStates(object sender, EventArgs e)
        {
            this.UpdateCommandStates();
        }

        private void UpdateCommandStates()
        {

        }
    }
}
