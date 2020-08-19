using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCMax.Models
{
    class Machine
    {// Klasa maszyny
        public int Id { get; set; }
        public int Value
        {
            get
            {
                return this.Processes.Sum(p => p.DelayTime);
            }
        }
        public List<Process> Processes { get; set; }
        
        public int Difference
        {
            get
            {
                return this.Value - Program.config.appData.ProcessOptimumTime;
            }
        }

        //public int MaxTime { get; set; } //
        public Machine(int id)
        {
            this.Id = id;

            this.Processes = new List<Process>();

        }
    }
}
