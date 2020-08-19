using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCMax.Models
{
    class PCMaxConfig
    { //Klasa odpoweidzialna za algorytm

        public static Random Random { get; set; }
        public AppData appData { get; set; }
        public List<double> WorstAvgTime { get; set; }
        public List<Machine> resultList { get; set; }
        public Dictionary<double, int> AvarageListCounter = new Dictionary<double, int>();
        private int lastIndex;
        public int LastIndex
        {
            get
            {
                return this.lastIndex;
            }
            set
            {
                if ((value >= this.appData.MachineNumber))
                {
                    lastIndex = 0;
                }
                else lastIndex = value;
            }
        }

        public PCMaxConfig(string path)
        { // ##1: Start programu 
            this.appData = FileReader.GetData(path);
            Program.config = this;
            this.getPcMaxResultBruteForce();  // algorytm zachłanny
            this.getPcMaxResultFromBruteForce(10000); // algorytm generyczny nasz
        }

        public void setAvgListCounter(double key)
        {

            if (AvarageListCounter.ContainsKey(key))
            {
                AvarageListCounter[key] = ++AvarageListCounter[key];
            }
            else
            {
                AvarageListCounter[key] = 1;
            }
        }
        public void clearAverage()
        {
            this.AvarageListCounter.Clear();
        }


        public string roznicaMaszyn(bool isLast = false)
        {
            var resultStirng = "";


            this.resultList.ForEach(machine =>
            {
                var machineProcesses = "";
                if (!isLast)
                {

                    machine.Processes.ForEach(p =>
                    {
                        machineProcesses += string.Format("{0},", p.DelayTime);
                    });
                }
                resultStirng += string.Format("Maszyna {0} : Czas działania: {1} /{2} ,{3} Procesy: {4} {5}", machine.Id, machine.Value, machine.Difference, Environment.NewLine, machineProcesses, Environment.NewLine);
            });

            return resultStirng;
        }
        public void clearMachines()
        {
            for (int i = 0; i < this.appData.MachineNumber; i++)
            {
                this.appData.Machines[i].Processes.Clear();

            }
        }
        public void PrintResults(string name, bool isLast = false)
        {
            Console.WriteLine(Environment.NewLine + name + " Wynik: ");
            Console.WriteLine(string.Format("{0}", roznicaMaszyn(isLast)));
            var avarageDifference = this.appData.Machines.Average(m => m.Difference > 0 ? m.Difference : m.Difference * -1);
            if (this.WorstAvgTime == null) { this.WorstAvgTime = new List<double>(); }
            setAvgListCounter(avarageDifference);
            if (isLast) this.WorstAvgTime.Clear();
            this.WorstAvgTime.Add(avarageDifference);

            Console.WriteLine(string.Format("Wynik optymalny: {0}, srednia: {1},{2} Najgorszy: \n {3} {4} ", this.appData.ProcessOptimumTime, avarageDifference, Environment.NewLine, string.Join(" | ", this.WorstAvgTime.ToArray()), Environment.NewLine));
            Console.WriteLine(Environment.NewLine);

        }
        public int RandomNumber(int min, int max)
        {
            return PCMaxConfig.Random.Next(min, max);
        }

        //algorytmy
        public int getMachineIndex(int mode)
        {
            var result = 0;
            switch (mode)
            {
                case 9:
                    result = this.LastIndex++; // indeksowanie nie dziala najlepiej
                    break;
                default:
                    result = 0;
                    break;
            }
            return result;
        }

        public bool getProcessCompare(int a, int b, int machine, int differenceOffset = 0)
        { //porownuje 2 procesy z maszyn do roznicy jednej z nich
            var result = false;
            if (machine > 0)
            {
                result = (a - b - (machine + differenceOffset)) >= 0;
            }
            else if (machine < 0)
            {
                result = (a - b - (machine + differenceOffset)) <= 0;
            }
            return result;
        }

        public void swapProcesses(Machine aMachine, Machine bMachine, Process a, Process b)
        {
            var aProc = aMachine.Processes.FindIndex(p => p.Id == a.Id);
            var bProc = bMachine.Processes.FindIndex(p => p.Id == b.Id);
            if (aProc > -1 && bProc > -1)
            {

                var tmp = bMachine.Processes[bProc];
                bMachine.Processes[bProc] = aMachine.Processes[aProc];
                aMachine.Processes[aProc] = tmp;

                a.Maszyna = bMachine.Id;
                b.Maszyna = aMachine.Id;
            }



        }
        public void orderList(int OrderingCounter)
        { //sortuyje liste result list w odpowiedni sposob, mozna sortowac dane na rozne sposoby i brac wtedy 1 maszyne do porównywaniwa
            //todo: do zaimplementowaniwa jeszcze inne tryby sortowania i wybierania 1 elementu ?
            switch (OrderingCounter % 4)
            {
                //case 1:
                //    this.resultList = this.resultList.OrderBy(m => Math.Abs(m.Difference)).ToList();
                //    break;
                //case 2:
                //    this.resultList = this.resultList.OrderBy(m => m.Difference).ToList();
                //    break;
                default:
                    this.resultList = this.resultList.OrderByDescending(m => Math.Abs(m.Difference)).ToList();
                    break;
            }
        }

        //algorytmy
        public void getPcMaxResultFromBruteForce(int CompareLoopCounter)
        {
            this.clearMachines(); // czyścimy maszyny  z listy procesów
            var sortedProcess = this.appData.Processes.OrderByDescending(a => a.DelayTime).ToList(); //sortujemy procesy malejaco, po czasie opóźnieniea
            #region Sortowanie 1 - wstępne - algorytm brute force
            sortedProcess.ForEach(process =>
            {
                int minValueIndex = 0;
                int minValue = this.appData.Machines[minValueIndex].Value;

                foreach (var item in this.appData.Machines)
                {
                    if (item.Value <= minValue)
                    {
                        minValue = item.Value;
                        minValueIndex = item.Id;
                    }
                }
                this.appData.Machines[minValueIndex].Processes.Add(process);
                process.Maszyna = this.appData.Machines[minValueIndex].Id;
            });
            #endregion

            string mode = "normal"; //ustawiamy na tryb normalny 
            var OrderingCounter = 0; //licznik do zmiany sortowaniwa 
            int OrderingChangeValueCase = 10;
            for (int i = 0; i < CompareLoopCounter; i++)
            {
                #region Zmiana typu sortowniwa i metody Porównywaniwa maszyn

                this.orderList(OrderingCounter++);

                if (this.AvarageListCounter.Any(x => x.Value > OrderingChangeValueCase))
                {
                    if (mode == "normal") mode = "second";
                    else if (mode == "second") mode = "third";
                    else if (mode == "jakisinny") mode = "kolejny";
                    else mode = "normal";

                    this.clearAverage(); //czyscimy licznik srendniej
                }
                #endregion

                switch (mode)
                { // sortowanie maszyn
                    case "second":
                        this.compareMachines(this.getMachineIndex(i));
                        break;
                    case "third":
                        this.compareMachines(this.getMachineIndex(9)); //pobieramy indexyu te 2
                        break;
                    default:
                        this.compareMachines2();
                        break;
                }
                this.orderList(0);
                var isLast = (i == CompareLoopCounter - 1);
                if (this.AvarageListCounter.ContainsKey(0))
                {
                    i = CompareLoopCounter;
                    isLast = true;
                }
                this.PrintResults("Compare machines " + i, isLast);

            }

        }



        public void getPcMaxResultBruteForce()
        { //oblcizanie
            this.clearMachines(); // Czyścimy Procesy maszyn
            this.appData.Processes.ForEach(process =>
            {
                int minValueIndex = 0;
                int minValue = this.appData.Machines[minValueIndex].Value;
                foreach (var item in this.appData.Machines)
                {
                    if (item.Value <= minValue)
                    {
                        minValue = item.Value;
                        minValueIndex = item.Id;
                    }
                }
                this.appData.Machines[minValueIndex].Processes.Add(process);
                process.Maszyna = this.appData.Machines[minValueIndex].Id;
                //Console.WriteLine(string.Format("Dodaje proces o czasie {0} do maszyny {1}.", process.DelayTime.ToString(), minValueIndex.ToString()));
            });
            this.resultList = this.appData.Machines;
            this.PrintResults("BruteForce");
        }
        public void compareMachinesFinal(int index)
        { /*
            1.pobiera/wybiera maszynę B za pomocą indexu przekazanego do funkcji - np 1 z brzegu albo 1 z brzegu po przesortowaniu w jakis psosob
            2. Wybiera 1 z brzegu proces tej maszyny - P1
            3. Poszukuje na liscie procesów ( wszystkich procesów nie przypisanych do B.ID _AND_ Procesów których DelayTime - P1.DelayTime, będzie mieścił się w nadmiarze/niedomiarze tej maszyny.
            4. Iteruje po wszystkich znalezionych procesach z kroku 3 - P2 i za kazdym razem pobiera ich maszyne - A
            5. Dla kazdego procesu sprawdza czy P1.DelayTime - P2.DelayTime miesci sie w zakresie dla nadmiaruniedomiaru maszyny A.
            6. jezeli te kryteria są spełnione, procesy zamieniaja się.
            
             */

            var bMachine = this.resultList[index];
            var machineDifference = (bMachine.Difference); //this.getMachineDifference(bMachine, aMachine);


            for (int i = 0; i < bMachine.Processes.Count; i++)
            {
                var bProces = bMachine.Processes[i];
                var processList = this.appData.Processes.Where(
                    p => p.Maszyna != bMachine.Id &&
                    getProcessCompare(bProces.DelayTime, p.DelayTime, machineDifference)
                    ).ToList();

                for (int j = 0; j < processList.Count; j++)
                {
                    var aProc = processList[j];
                    var aMachine = this.appData.Machines.Find(m => m.Id == aProc.Maszyna);
                    if (getProcessCompare(bProces.DelayTime, aProc.DelayTime, aMachine.Difference, bMachine.Difference))
                    {
                        swapProcesses(bMachine, aMachine, bProces, aProc);
                        machineDifference = (bMachine.Difference);  //this.getMachineDifference(bMachine, aMachine);
                        j = processList.Count;
                        i = bMachine.Processes.Count;
                    }

                }
            }
        }
        public void compareMachines(int index)
        { /*
            1.pobiera/wybiera maszynę B za pomocą indexu przekazanego do funkcji - np 1 z brzegu albo 1 z brzegu po przesortowaniu w jakis psosob
            2. Wybiera 1 z brzegu proces tej maszyny - P1
            3. Poszukuje na liscie procesów ( wszystkich procesów nie przypisanych do B.ID _AND_ Procesów których DelayTime - P1.DelayTime, będzie mieścił się w nadmiarze/niedomiarze tej maszyny.
            4. Iteruje po wszystkich znalezionych procesach z kroku 3 - P2 i za kazdym razem pobiera ich maszyne - A
            5. Dla kazdego procesu sprawdza czy P1.DelayTime - P2.DelayTime miesci sie w zakresie dla nadmiaruniedomiaru maszyny A.
            6. jezeli te kryteria są spełnione, procesy zamieniaja się.
            
             */

            var bMachine = this.resultList[index];
            var machineDifference = (bMachine.Difference); //this.getMachineDifference(bMachine, aMachine);


            for (int i = 0; i < bMachine.Processes.Count; i++)
            {
                var bProces = bMachine.Processes[i];
                var processList = this.appData.Processes.Where(
                    p => p.Maszyna != bMachine.Id &&
                    getProcessCompare(bProces.DelayTime, p.DelayTime, machineDifference)
                    ).ToList();

                for (int j = 0; j < processList.Count; j++)
                {
                    var aProc = processList[j];
                    var aMachine = this.appData.Machines.Find(m => m.Id == aProc.Maszyna);
                    if (getProcessCompare(bProces.DelayTime, aProc.DelayTime, aMachine.Difference) || processList.Count == 1)
                    {
                        swapProcesses(bMachine, aMachine, bProces, aProc);
                        machineDifference = (bMachine.Difference);  //this.getMachineDifference(bMachine, aMachine);
                        j = processList.Count;
                        i = bMachine.Processes.Count;
                    }

                }
            }
        }
        public void compareMachines2()
        {

            /*
            1.Wyniera 2 pierwsze maszyny z brzegu B i A
            2.Nastepnie iteruje po procesach kazdej z nich P1 i P2
            3. Sprawdza czy zamiania procesu P2 i P1 coś da
            4. Jesli tak zamienia procesy

             */
            var bMachine = this.resultList[0];
            var aMachine = this.resultList[1];

            for (int i = 0; i < bMachine.Processes.Count; i++)
            {
                for (int j = 0; j < aMachine.Processes.Count; j++)
                {
                    var processDifference = Math.Abs(bMachine.Processes[i].DelayTime - aMachine.Processes[j].DelayTime);
                    if (processDifference <= Math.Abs(bMachine.Difference) && processDifference <= Math.Abs(aMachine.Difference))
                    {//jezeli miesci sie  w zakresie roznic obu maszyn to zamieniamy ;
                        swapProcesses(bMachine, aMachine, bMachine.Processes[i], aMachine.Processes[j]);
                    }
                }
            }
        }
    }
}
