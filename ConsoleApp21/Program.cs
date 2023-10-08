using Microsoft.EntityFrameworkCore;

namespace ns
{
    class tClass
    {
        public static void Main()
        {
            var dbService = new DBService();

            var data = dbService.GetCheques();

            Console.Clear();
            Console.WriteLine("Enter \'a\' to add new cheques to collection \'t\' to get all info \'e\' to get most expensive \'avg\' to get avg spent sum \'q\'to exit:");
            var input = Console.ReadLine();
            if (input == "a")
            {
                do
                {
                    Console.Clear();
                    Console.WriteLine("Press \'q\' to exit");
                    Console.WriteLine("");
                    Console.WriteLine("Enter date: ");
                    var date = Console.ReadLine();
                    if (date == "q")
                    {
                        break;
                    }
                    if (!DateTime.TryParse(date, out var parsed))
                    {
                        Console.WriteLine("date parsing failure");
                        Console.WriteLine("adding rollbacked");
                        Console.ReadLine();
                        continue;
                    }
                    Console.WriteLine("Enter sum: ");
                    var sum = Console.ReadLine();
                    if (sum == "q")
                    {
                        break;
                    }
                    if (!decimal.TryParse(sum, out var _))
                    {
                        Console.WriteLine("sum parsing failure");
                        Console.WriteLine("adding rollbacked");
                        Console.ReadLine();
                        continue;
                    }
                    Console.WriteLine("Enter comm: ");
                    var comm = Console.ReadLine();
                    if (comm == "q")
                    {
                        break;
                    }
                    if (String.IsNullOrEmpty(comm) || String.IsNullOrWhiteSpace(comm))
                    {
                        comm = "no comment";
                    }
                    Console.WriteLine("Enter typeTransf : Income or Spending");
                    var typeTransf = Console.ReadLine();
                    if (typeTransf == "q")
                    {
                        break;
                    }
                    if (!Enum.IsDefined(typeof(TypeTransf), typeTransf))
                    {
                        Console.WriteLine("typeTransf parsing failure");
                        Console.WriteLine("adding rollbacked");
                        Console.ReadLine();
                        continue;
                    }

                    data.Add(new Cheque { date = parsed, sum = decimal.Parse(sum), comm = comm, typeTransf = (TypeTransf)Enum.Parse(typeof(TypeTransf), typeTransf) });

                    Console.WriteLine("Added successfully");
                    Console.ReadLine();
                }
                while (true);

                dbService.SaveCheques(data);
            }
            if (input == "t")
            {
                ShowAllData(data);
                Console.WriteLine();
                GetTotal(data);
            }
            if (input == "e")
            {
                GetMostExpensive(data);
            }
            if (input == "avg")
            {
                Console.Clear();
                Console.WriteLine("Enter dateFrom");
                var dateFrom = Console.ReadLine();
                if (!DateTime.TryParse(dateFrom, out var df))
                {
                    Console.WriteLine("date parsing failure");
                    return;
                }
                Console.WriteLine("Enter dateTo");
                var dateTo = Console.ReadLine();
                if (!DateTime.TryParse(dateTo, out var dt))
                {
                    Console.WriteLine("date parsing failure");
                    return;
                }
                GetAvgSpent(data, df, dt);
            }
            if (input == "q")
            {
                return;
            }
        }

        public static void ShowAllData(List<Cheque> data)
        {
            data.ForEach(pr =>
            {
                Console.WriteLine($"transaction date: {pr.date.ToShortDateString()} transaction type: {pr.typeTransf.ToString()} sum: {pr.sum} comm: {pr.comm}");
            });
        }

        public static void GetTotal(List<Cheque> data)
        {
            Console.WriteLine($"Total income: {data.Where(w => w.typeTransf == TypeTransf.Income).Select(s => s.sum).Sum()}");
            Console.WriteLine($"Total spent: {data.Where(w => w.typeTransf == TypeTransf.Spending).Select(s => s.sum).Sum()}");
            Console.WriteLine($"Total: {(data.Where(w => w.typeTransf == TypeTransf.Income).Select(s => s.sum).Sum()) - data.Where(w => w.typeTransf == TypeTransf.Spending).Select(s => s.sum).Sum()}");
        }

        public static void GetMostExpensive(List<Cheque> data)
        {
            Console.WriteLine($"Most expensive: {data.ElementAt(data.FindIndex(pr => pr.typeTransf == TypeTransf.Spending && pr.sum == data.Where(w => w.typeTransf == TypeTransf.Spending).Select(s => s.sum).Max())).comm} - {data.Where(w => w.typeTransf == TypeTransf.Spending).Select(s => s.sum).Max()}");
        }

        public static void GetAvgSpent(List<Cheque> data, DateTime dateFrom, DateTime dateTo)
        {
            var filteredData = data.Where(w => w.date >= dateFrom && w.date <= dateTo && w.typeTransf == TypeTransf.Spending);
            if (filteredData.Count() < 1)
            {
                Console.WriteLine("No data for this period");
            }
            else
            {
                Console.WriteLine($"Average spent sum between {dateFrom.ToShortDateString()} and {dateTo.ToShortDateString()} - {data.Where(w => w.date >= dateFrom && w.date <= dateTo && w.typeTransf == TypeTransf.Spending).Select(s => s.sum).Average()}");
            }
        }
    }
    public class Cheque
    {
        public int id { get; set; }

        public decimal sum { get; set; }

        public DateTime date { get; set; }

        public string comm { get; set; }

        public TypeTransf typeTransf { get; set; }
    }

    public enum TypeTransf
    {
        Income,
        Spending
    }

    public class DBService
    {
        private DataContext _dataContext;

        public DBService()
        {
            _dataContext = new DataContext();
        }

        public List<Cheque> GetCheques()
        {
            var data = new List<Cheque>();
            try
            {
                data = _dataContext.Cheques.ToList();
            }
            catch { }
            return data;
        }

        public void SaveCheques(List<Cheque> cheques)
        {
            if (cheques.Count < 1)
            {
                return;
            }
            _dataContext.Cheques.AddRange(cheques.ToArray());
            _dataContext.SaveChanges();
        }
    }

    public class DataContext : DbContext
    {

        public DbSet<Cheque> Cheques { get; set; }
        public DataContext() => Database.EnsureCreated();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=MySqliteDB.db");
        }

    }
}