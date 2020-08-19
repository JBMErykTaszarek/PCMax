using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCMax.Models
{
    class Process
    { //klasa procesu
        public int Id { get; set; }
        public int DelayTime { get; set; }
        public int Maszyna { get; set; }
        public Process(int id, int delayTime)
        {
            this.Id = id;
            this.DelayTime = delayTime;
        }
    }
}
