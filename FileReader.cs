using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCMax.Models
{
    class FileReader
    { //klasa sluzaca do operacji na plikach 
        public static string BaseDir = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName;
        public static List<FileInfo> GetFiles()
        {
            List<FileInfo> files = new List<FileInfo>();
            var path = FileReader.BaseDir + "\\Data";

            DirectoryInfo d = new DirectoryInfo(path);//Assuming Test is your Folder
            FileInfo[] Files = d.GetFiles("*.txt"); //Getting Text files

            string str = "";
            foreach (FileInfo file in Files)
            {
                files.Add(file);
                str = str + ", " + file.Name;
            }
            return files;
        }
        public static AppData GetData(string filePath)
        { //pboieranie danych z pliku i zwracanie obiektu AppData.
            List<FileInfo> files = new List<FileInfo>();
            StreamReader sr = File.OpenText(filePath);

            int numberOfMachines = 0;
            int.TryParse(sr.ReadLine(), out numberOfMachines);

            int numberOfProcess = 0;
            int.TryParse(sr.ReadLine(), out numberOfProcess);

            var processes = new List<Process>();
            int processTotalTime = 0;

            for (int i = 0; i < numberOfProcess; i++)
            {
                var ProccesTime = sr.ReadLine();
                var time = int.Parse(ProccesTime);
                processTotalTime += time;
                processes.Add(new Process(i, time));
            }

            var machines = new List<Machine>();
            for (int i = 0; i < numberOfMachines; i++)
            {
                machines.Add(new Machine(i));
            }

            return new AppData
            {
                Processes = processes,
                Machines = machines,
                MachineNumber = numberOfMachines,
                ProcessNumber = numberOfProcess,
                ProcessTotalTime = processTotalTime,
                ProcessOptimumTime =(processTotalTime / numberOfMachines)
            };
        }


    }
}
