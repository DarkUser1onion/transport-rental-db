using System;
using System.Text;
using Dapper;
using Microsoft.Data.SqlClient;

namespace MyApp;


internal class Program
{
    private const string IP = "192.168.8.103"; 
    private const string DATABASE = "Rental_system"; 
    private const string USER = "sa"; 
    
    public class ConnectionString
    {
        string Server;
        string Database;
        string User;
        string Password;
        
        public ConnectionString(string server, string database, string user, string password)
        {
            Server = server;
            Database = database;
            User = user;
            Password = password;
        }

        public override string ToString()
        {
            return $"Server={Server};Database={Database};User={User};Password={Password};TrustServerCertificate=True;";
        }
    }
    
    private static void ConsoleOnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
    {
        Animation.Exit();
    }

    public static List<string> logo = new List<string>();
    
    static void Main(string[] args)
    {
        
        logo.Add("▄▄▄  ▄▄▄ . ▐ ▄ ▄▄▄▄▄ ▄▄▄· ▄▄▌    .▄▄ ·  ▄· ▄▌.▄▄ · ▄▄▄▄▄▄▄▄ .• ▌ ▄ ·.      ");
        logo.Add("▀▄ █·▀▄.▀·•█▌▐█•██  ▐█ ▀█ ██•    ▐█ ▀. ▐█▪██▌▐█ ▀. •██  ▀▄.▀··██ ▐███▪     ");
        logo.Add("▀▀▄ ▐▀▀▪▄▐█▐▐▌ ▐█.▪▄█▀▀█ ██ ▪   ▄▀▀▀█▄▐█▌▐█▪▄▀▀▀█▄ ▐█.▪▐▀▀▪▄▐█ ▌▐▌▐█·      ");
        logo.Add("█•█▌▐█▄▄▌██▐█▌ ▐█▌·▐█▪ ▐▌▐█▌ ▄  ▐█▄▪▐█ ▐█▀·.▐█▄▪▐█ ▐█▌·▐█▄▄▌██ ██▌▐█▌      ");
        logo.Add("         ▄▄▄▄·  ▄· ▄▌  ▄▄▌ ▐ ▄▌ ▄ .▄       ▄▄▄· • ▌ ▄ ·. ▪      ");
        logo.Add("         ▐█ ▀█▪▐█▪██▌  ██· █▌▐███▪▐█ ▄█▀▄ ▐█ ▀█ ·██ ▐███▪██     ");
        logo.Add("         ▐█▀▀█▄▐█▌▐█▪  ██▪▐█▐▐▌██▀▀█▐█▌.▐▌▄█▀▀█ ▐█ ▌▐▌▐█·▐█·    ");
        logo.Add("         ██▄▪▐█ ▐█▀·.  ▐█▌██▐█▌██▌▐▀▐█▌.▐▌▐█▪ ▐▌██ ██▌▐█▌▐█▌    ");
        logo.Add("         ·▀▀▀▀   ▀ •    ▀▀▀▀ ▀▪▀▀▀ · ▀█▄▀▪ ▀  ▀ ▀▀  █▪▀▀▀▀▀▀    ");
        
        

        
        Console.Clear();

        foreach (var item in logo)
        {
            Animation.PrintRedText(item, true, false, 3, true, true);
        }
        Thread.Sleep(1000);
        Console.Clear();
        
        
        Console.CancelKeyPress += ConsoleOnCancelKeyPress;
        while (true)
        {
            StringBuilder pass = new StringBuilder();
            Animation.PrintRedText("Введите пароль от базы данных: ", true, true, 50, true);

            int counter = 0;
            while (true)
            {
                var key = Console.ReadKey(intercept:true);
                if (key.Key == ConsoleKey.Enter)
                    break;
                
                if(key.Key == ConsoleKey.Escape)
                    continue;

                
                if (key.Key == ConsoleKey.Backspace && counter > 0)
                {
                    pass.Remove(pass.Length - 1, 1);
                    Console.Write("\b \b");
                    counter--;
                    
                    continue;
                }

                if (key.Key != ConsoleKey.Backspace)
                {
                    pass.Append(key.KeyChar.ToString());
                    Console.Write("*");
                    counter++;
                }
                
            }
            
            ConnectionString cs = new(IP, DATABASE, USER, pass.ToString());
            var con = new SqlConnection(cs.ToString());

            Animation.LoadingDatabase();
            
            
            try
            {
                con.Open();
            }
            catch (Exception)
            {
                Console.Clear();
                Animation.PrintRedText("Неверно указан пароль или данные для входа!", true, true, 50);
                Console.Clear();
                return;
            }
            
            Animation.Welcome();
            
            Menu menu = new Menu();
            
            while (true)
            {
                Menu.StartMenu();
                if (!int.TryParse(Console.ReadLine(), out var input))
                    input = Int32.MaxValue;

                
                switch ((CaseMenu)input)
                {
                    case CaseMenu.RegistrationProfile:
                        menu.RegistrationProfile().Upload(con);
                        break;
                    case CaseMenu.ManagementProfiles:
                        break;
                    case CaseMenu.ManagementPark:
                        break;
                    case CaseMenu.StartRent:
                        break;
                    case CaseMenu.EndRent:
                        break;
                    case CaseMenu.FindGeo:
                        break;
                    case CaseMenu.Report:
                        break;
                    case CaseMenu.Leave:
                        con.Close();
                        Animation.Exit();
                        return;
                    default:
                        Console.Clear();
                        Animation.PrintRedText("Неверно указано!",true, true, 100);
                        Console.Clear();
                        break;
                }
            }
            ConsoleOnCancelKeyPress(new object(), null);
        }
    }
}
