using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCMax.Models
{
    class AppData
    { //Root aplikacji
        public List<Process> Processes { get; set; } // Lista procesów 
        public List<Machine> Machines { get; set; } // Lista procesów 
        public int ProcessNumber { get; set; }
        public int MachineNumber { get; set; }
        public int ProcessTotalTime { get; set; } // wartość czasu dla wsyzstkich procesów
        public int ProcessOptimumTime { get; set; } // wartość optymalna dla kazdej z maszyn totaltime/liczba maszyb
        public AppData()
        {

        }

    }
}
