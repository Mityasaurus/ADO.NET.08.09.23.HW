using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.Metrics;

namespace _08._09._23.HW
{
    internal class Program
    {
        private static string ConnString => ConfigurationManager.ConnectionStrings["Default"].ToString();
        private static Dictionary<string, List<string>> Countries = new Dictionary<string, List<string>>();
        private static Dictionary<string, List<string>> Capitals = new Dictionary<string, List<string>>();
        private static Dictionary<string, List<string>> MajorCities = new Dictionary<string, List<string>>();
        private static Dictionary<string, List<string>> Regions = new Dictionary<string, List<string>>();
        static void Main(string[] args)
        {
            try
            {
                using(SqlConnection conn = new SqlConnection(ConnString))
                {
                    conn.Open();
                    Console.WriteLine("Connection success");

                    UpdateData(conn, new string[] { ConstNamesTables.Countries, ConstNamesTables.Capitals, ConstNamesTables.MajorCities, ConstNamesTables.Regions }, Countries, Capitals, MajorCities, Regions);

                    int choice = -1;
                    while(choice != 0)
                    {
                        Console.WriteLine("\nВведiть свiй вибiр");
                        Console.WriteLine("1 - Показати всю iнформацiю про країни");
                        Console.WriteLine("2 - Показати назви усiх країн");
                        Console.WriteLine("3 - Показати назви усiх столиць");
                        Console.WriteLine("4 - Показати назви великих мiст певної країни");
                        Console.WriteLine("5 - Показати назви столиць з населенням понад 5 мiльйонiв");
                        Console.WriteLine("6 - Показати назви усiх европейських країн");
                        Console.WriteLine("7 - Показати назви усiх країн з площею, бiльшою нiж задана");
                        Console.WriteLine();
                        Console.WriteLine("8 - Показати усi столицi, в назвах яких є лiтери 'a' та 'p'");
                        Console.WriteLine("9 - Показати усi столицi, якi починаються на певну букву");
                        Console.WriteLine("10 - Показати усi країни, площа яких зазначена у вказаному дiапазонi");
                        Console.WriteLine("11 - Показати назви усiх країн з населенням, бiльшим нiж задане");
                        Console.WriteLine();
                        Console.WriteLine("0 - Вихiд");

                        choice = int.Parse(Console.ReadLine());

                        switch(choice)
                        {
                            case 1:
                                Console.Clear();
                                Display(Countries);
                                break;
                            case 2:
                                Console.Clear();
                                Display(GetDictionaryWith(Countries, "CountryName"));
                                break;
                            case 3:
                                Console.Clear();
                                Display(Capitals);
                                break;
                            case 4:
                                Console.Clear();
                                Console.WriteLine("Введiть назву бажаної країни");

                                string countryName4case = Console.ReadLine();
                                Display(GetMajorCitiesByCountry(countryName4case));
                                break;
                            case 5:
                                Console.Clear();
                                Display(GetCapitalsMore5MilPopulation());
                                break;
                            case 6:
                                Console.Clear();
                                Display(GetEuropeanCountries());
                                break;
                            case 7:
                                Console.Clear();
                                Console.WriteLine("Введiть мiнiмальну площу");

                                int area7Case;

                                if(int.TryParse(Console.ReadLine(), out area7Case))
                                {
                                    Display(GetCountriesAreaBiggerThan(area7Case));
                                }
                                break;
                            case 8:
                                Console.Clear();
                                Display(GetCapitalsWithAAndPInName());
                                break;
                            case 9:
                                Console.Clear();
                                Console.WriteLine("Введiть бажану букву");

                                char letter9Case;

                                if(char.TryParse(Console.ReadLine(), out letter9Case))
                                {
                                    Display(GetCapitalsStartsWitn(letter9Case));
                                }

                                break;
                            case 10:
                                Console.Clear();
                                Console.WriteLine("Введiть початок дiапазону");
                                int start10case;

                                if(int.TryParse(Console.ReadLine(), out start10case) == false)
                                {
                                    break;
                                }
                                Console.WriteLine("Введiть кiнець дiапазону");
                                int end10case;

                                if (int.TryParse(Console.ReadLine(), out end10case) == false)
                                {
                                    break;
                                }
                                Display(GetCountriesWithAreaInRange(start10case, end10case));
                                break;
                            case 11:
                                Console.Clear();
                                Console.WriteLine("Введiть мiнiмальну кiлькiсть населення");

                                int population11Case;

                                if(int.TryParse(Console.ReadLine(), out population11Case))
                                {
                                    Display(GetCountriesPopulationBiggerThan(population11Case));
                                }
                                break;
                            case 0:
                                break;
                            default:
                                Console.WriteLine("Помилковий вибiр!");
                                break;
                        }
                    }
                    conn.Close();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void UpdateData(SqlConnection conn, string[] tbNames, params Dictionary<string, List<string>>[] tables)
        {
            try
            {
                for (int i = 0; i < tables.Length; i++)
                {
                    tables[i].Clear();
                    GetAllValues(conn, tbNames[i], tables[i]);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private static string GetSelectQuery(string tbName) =>
            $"select * from {tbName}";
        private static List<string> GetTableNames(SqlConnection sqlConnection, params string[] name)
        {
            var tables = new List<string>();

            if (name.Length > 0)
            {
                foreach (DataRow dr in sqlConnection.GetSchema("Tables").Rows)
                {
                    if (name.Contains(dr[2].ToString()))
                    {
                        tables.Add($"[{dr[1]}].[{dr[2]}]");
                    }
                }
            }
            else
            {
                foreach (DataRow dr in sqlConnection.GetSchema("Tables").Rows)
                {
                    tables.Add($"[{dr[1]}].[{dr[2]}]");
                }
            }

            return tables;
        }
        private static Dictionary<string, List<string>> GetAllValues(SqlConnection sqlConnection, string tbName, Dictionary<string, List<string>> dict)
        {
            var tables = GetTableNames(sqlConnection, tbName);


            using (SqlCommand cmd = new SqlCommand(GetSelectQuery(tables[0]), sqlConnection))
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        dict.Add(reader.GetName(i), new List<string>());
                    }

                    while (reader.Read())
                    {
                        dict.Keys.ToList().ForEach(key => dict[$"{key}"].Add(reader[$"{key}"].ToString()));
                    }
                }
            }
            return dict;
        }
        private static Dictionary<string, List<string>> GetDictionaryWith(Dictionary<string, List<string>> dict, string columnName, List<string> value)
        {
            var result = new Dictionary<string, List<string>>();
            try
            {
                dict.Keys.ToList().ForEach(key => result.Add(key, new List<string>()));

                for (int i = 0; i < dict["ID"].Count; i++)
                {
                    if (value.Contains(dict[columnName][i]))
                    {
                        dict.Keys.ToList().ForEach(key => result[key].Add(dict[key][i]));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return result;
        }
        private static Dictionary<string, List<string>> GetDictionaryWith(Dictionary<string, List<string>> dict, params string[] columnName)
        {
            var result = new Dictionary<string, List<string>>();
            try
            {
                dict.Keys.ToList().Where(key => columnName.Contains(key)).ToList().ForEach(key => result.Add(key, new List<string>()));

                for (int i = 0; i < dict["ID"].Count; i++)
                {
                    dict.Keys.ToList().Where(key => columnName.Contains(key)).ToList().ForEach(key => result[key].Add(dict[key][i]));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return result;
        }
        private static void Display(Dictionary<string, List<string>> dict)
        {
            dict.Keys.ToList().ForEach(key => Console.Write($"{key.PadRight(20)}"));

            Console.WriteLine("\n");

            var keyOrder = dict.Keys.ToList();
            var columnCount = dict[keyOrder.First()].Count;

            for (int i = 0; i < columnCount; i++)
            {
                var row = string.Join("", keyOrder.Select(key => dict[key][i].PadRight(20)));
                Console.WriteLine(row);
            }
        }

        private static Dictionary<string, List<string>> GetMajorCitiesByCountry(string country)
        {
            var id = Countries["CountryName"]
            .Select((name, index) => new { Name = name, ID = Countries["ID"][index] })
            .Where(countryInfo => countryInfo.Name == country)
            .Select(countryInfo => countryInfo.ID)
            .ToList();

            return GetDictionaryWith(GetDictionaryWith(MajorCities, "CountryID", id), "CityName");
        }
        private static Dictionary<string, List<string>> GetCapitalsMore5MilPopulation()
        {
            var id = Capitals["Population"]
            .Select((population, index) => new { Population = population, ID = Capitals["CountryID"][index] })
            .Where(info => int.Parse(info.Population) >= 5000000)
            .Select(info => info.ID)
            .ToList();

            return GetDictionaryWith(GetDictionaryWith(Capitals, "CountryID", id), "CapitalName");
        }
        private static Dictionary<string, List<string>> GetEuropeanCountries()
        {
            var id = Regions["RegionName"]
                        .Select((name, index) => new { Name = name, ID = Regions["ID"][index] })
                        .Where(info => info.Name == "Europe")
                        .Select(info => info.ID)
                        .ToList();
            return GetDictionaryWith(Countries, "RegionID", id);
        }
        private static Dictionary<string, List<string>> GetCountriesAreaBiggerThan(int _area)
        {
            var id = Countries["Area"]
            .Select((area, index) => new { Area = area, ID = Countries["ID"][index] })
            .Where(countryInfo => int.Parse(countryInfo.Area) >= _area)
            .Select(countryInfo => countryInfo.ID)
            .ToList();

            return GetDictionaryWith(Countries, "ID", id);
        }
        private static Dictionary<string, List<string>> GetCapitalsWithAAndPInName()
        {
            var id = Capitals["CapitalName"]
                    .Select((name, index) => new { Name = name, ID = Capitals["ID"][index] })
                    .Where(info => info.Name.ToLower().Contains('a') && info.Name.ToLower().Contains('p'))
                    .Select(info => info.ID)
                    .ToList();

            return GetDictionaryWith(Capitals, "ID", id);
        }
        private static Dictionary<string, List<string>> GetCapitalsStartsWitn(char letter)
        {
            var id = Capitals["CapitalName"]
                    .Select((name, index) => new { Name = name, ID = Capitals["ID"][index] })
                    .Where(info => info.Name.ToLower()[0] == char.ToLower(letter))
                    .Select(info => info.ID)
                    .ToList();

            return GetDictionaryWith(Capitals, "ID", id);
        }
        private static Dictionary<string, List<string>> GetCountriesWithAreaInRange(int start, int end)
        {
            var id = Countries["Area"]
                    .Select((area, index) => new { Area = area, ID = Capitals["ID"][index] })
                    .Where(info => int.Parse(info.Area) >= start && int.Parse(info.Area) <= end)
                    .Select(info => info.ID)
                    .ToList();

            return GetDictionaryWith(GetDictionaryWith(Countries, "ID", id), "CountryName");
        }
        private static Dictionary<string, List<string>> GetCountriesPopulationBiggerThan(int _population)
        {
            var id = Countries["Population"]
            .Select((population, index) => new { Population = population, ID = Countries["ID"][index] })
            .Where(countryInfo => int.Parse(countryInfo.Population) >= _population)
            .Select(countryInfo => countryInfo.ID)
            .ToList();

            return GetDictionaryWith(Countries, "ID", id);
        }
    }
}