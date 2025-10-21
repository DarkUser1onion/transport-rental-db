using System.ComponentModel.Design;
using System.Data;
using System.Data.SqlTypes;
using System.Text;
using Dapper;
using Microsoft.Data.SqlClient;

namespace MyApp;

public class Menu
{
    private bool exit = false;
    
    public SqlConnection conn {private get; set;}
    interface IUploadInServer
    {
        public bool Upload(SqlConnection connection);
    }

    public Menu(SqlConnection con) 
        => conn = con;
    
    
    private string ReadLineWithCancel()
    {
        var input = new StringBuilder();
        while (true)
        {
        
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);
            
                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    return input.ToString();
                }
                else if (key.Key == ConsoleKey.Backspace && input.Length > 0)
                {
                    input.Remove(input.Length - 1, 1);
                    Console.Write("\b \b");
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    input.Append(key.KeyChar);
                    Console.Write(key.KeyChar);
                }
                else if (key.Key == ConsoleKey.Escape)
                {
                    exit = true;
                    return string.Empty;
                }
            }
            else
            {
                Thread.Sleep(50);
            }
        }
    }
    
    public class UserRegistaryForm : IUploadInServer
    {
        public string name { private get; set; }
        public string familyName { private get; set; }
        public string email { private get; set; }
        public Decimal numberCard { private get; set; }
        public int cvcCode { private get; set; }
        public DateTime Validity { private get; set; }
        
        public UserRegistaryForm(){}

        public override string ToString()
            => $"{name} {familyName} {email} {numberCard} {cvcCode} {Validity}";


        public bool Upload(SqlConnection connection)
        {
            try
            {
                string connectionString =
                    "INSERT INTO Wallets (Number_card, THREE_code, Validity) VALUES (@numberCard, @cvcCode, @Validity)";

                using (SqlCommand command = new SqlCommand(connectionString, connection))
                {
                    command.Parameters.AddWithValue("@cvcCode", cvcCode);
                    command.Parameters.AddWithValue("@Validity", Validity);
                    command.Parameters.AddWithValue("@numberCard", numberCard);

                    command.ExecuteNonQuery();
                }

                int id = connection.Query("SELECT id FROM Wallets WHERE Number_card = @numberCards",
                    new { @numberCards = numberCard }).First().id;

                connectionString =
                    "INSERT INTO Users (Name, Family_Name, Email, Wallet) VALUES (@name, @familyName, @email, @Wallet)";

                using (SqlCommand command = new SqlCommand(connectionString, connection))
                {
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@familyName", familyName);
                    command.Parameters.AddWithValue("@email", email);
                    command.Parameters.AddWithValue("@Wallet", id);
                    
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception)
            {
                return false;
            }
            
            return true;
        }
    }

    public class ChangeWallet : IUploadInServer
    {
        public string name;
        public string email;

        
        private int idWallet;
        
        public decimal numberCard { private get; set; }
        public int cvcCode {private get; set; }
        public DateTime Validity {private get; set;}

        public ChangeWallet() { }

        public bool SearchData(SqlConnection connection)
        {
            try
            {
                var result = connection.Query<string>("SELECT W.id FROM Wallets as W, Users as U" +
                                                      " WHERE U.Email = @email AND U.Wallet = W.id AND U.Name = @name", new {@email = email, @name = name}).First().ToString();

                idWallet = int.Parse(result);

            }
            catch (Exception)
            {
                return false;
            }
            
            return true;
        }
        public bool Upload(SqlConnection connection)
        {
            try
            {
                string connectStr = "UPDATE Wallets SET Number_card = @numberCard, THREE_code = @three_code, Validity = @Validity WHERE Id = @id";
                using (SqlCommand command = new SqlCommand(connectStr, connection))
                {
                    command.Parameters.AddWithValue("@numberCard", numberCard);
                    command.Parameters.AddWithValue("@three_code", cvcCode);
                    command.Parameters.AddWithValue("@Validity", Validity);
                    command.Parameters.AddWithValue("@id", idWallet);

                    command.ExecuteNonQuery();
                }
                
                
            }
            catch (Exception)
            {
                return false;
            }
            
            return true;
        }
    }
    
    public static void StartMenu()
    {
        Console.Clear();
        Animation.PrintRedText("Управление", true, false, 70);
        
        Animation.PrintSetCursor("1) Регистрация профиля", 22, true, 9);
        Console.WriteLine();
        Animation.PrintSetCursor("2) Управление профилями", 22);
        Console.WriteLine();
        Animation.PrintSetCursor("3) Управление автопарком", 22);
        Console.WriteLine();
        Animation.PrintSetCursor("4) Оформление аренды", 22);
        Console.WriteLine();
        Animation.PrintSetCursor("5) Завершение аренды", 22);
        Console.WriteLine();
        Animation.PrintSetCursor("6) Узнать геолокацию транспортов", 22);
        Console.WriteLine();
        Animation.PrintSetCursor("7) Сформировать отчет", 22);
        Console.WriteLine();
        Animation.PrintSetCursor("8) Выход", 22);
        Console.WriteLine("\n\n");
        Animation.PrintSetCursor("Выбор: ", 22);
    }

    public UserRegistaryForm RegistrationProfile()
    {
        exit = false;
        UserRegistaryForm user = new UserRegistaryForm();
        
        Console.Clear();
        
        Animation.PrintRedText("Регистрация профиля", true, false, 50);
        Console.WriteLine();
        Animation.PrintRedText("Выход на Esc", true, false, 50);
        
        Animation.PrintSetCursor("Введите ваше имя: ", 27, true, 6);
        user.name = ReadLineWithCancel();
        if (exit)
            return null;
        Animation.PrintSetCursor("Введите вашу фамилию: ", 27);
        user.familyName = ReadLineWithCancel();
        if (exit)
            return null;
        Animation.PrintSetCursor("Введите ваш email: ", 27);
        user.email = ReadLineWithCancel();
        if (exit)
            return null;
        Animation.PrintSetCursor("Введите ваш номер карты (напр. 1234567891234567): ", 27);

        Decimal numberCard;

        int cursorPosition = Console.GetCursorPosition().Left;

        while (true)
        {
            string temp = ReadLineWithCancel();;
            if (!Decimal.TryParse(temp, out numberCard))
            {
                if (exit)
                    return null;
                
                Animation.AnimationText("Ошибка ввода!", true, cursorPosition, temp.Length);
                continue;
            }

            if (numberCard.ToString().Length == 16)
                break;

            Animation.AnimationText("Ошибка в цифрах!", true, cursorPosition, temp.Length);
        }

        user.numberCard = numberCard;

        Animation.PrintSetCursor("Введите ваш CVC код (напр. 123): ", 27);

        cursorPosition = Console.GetCursorPosition().Left;

        int number;
        while (true)
        {
            string temp = ReadLineWithCancel();
            if (!int.TryParse(temp, out number))
            {
                if (exit)
                    return null;
                
                Animation.AnimationText("Ошибка ввода!", true, cursorPosition, temp.Length);
                continue;
            }

            if (number.ToString().Length == 3)
                break;

            Animation.AnimationText("Ошибка в цифрах!", true, cursorPosition, temp.Length);
        }

        user.cvcCode = number;

        Animation.PrintSetCursor("Введите срок действия карты (напр. 2030-12): ", 27);

        cursorPosition = Console.GetCursorPosition().Left;

        while (true)
        {
            string temp = ReadLineWithCancel();
            if (exit)
                return null;
            
            try
            {
                user.Validity = DateTime.Parse(temp);
                break;
            }
            catch (Exception)
            {
                Animation.AnimationText("Ошибка даты!", true, cursorPosition, temp.Length);
            }

        }

        Console.Clear();
        
        Animation.AnimationText("Загрузка данных на сервер", false, 0, 0, true);
        
        return user;
    }
    

    public void ManagementProfileMenu(SqlConnection connection)
    {
        exit = false;
        
        Console.Clear();
        
        Animation.PrintRedText("Управление профилем", true, false, 50);
        Console.WriteLine();
        Animation.PrintRedText("Выход на Esc", true, false, 50);
        
        Animation.PrintSetCursor("Что нужно сделать?", 27, true, 6);
        Console.WriteLine();
        Animation.PrintSetCursor("1) Сменить кошелек", 27);
        Console.WriteLine();
        Animation.PrintSetCursor("2) Сменить имя пользователя", 27);
        Console.WriteLine();
        Animation.PrintSetCursor("3) Сменить фамилию пользователя", 27);
        Console.WriteLine();
        Animation.PrintSetCursor("4) Сменить почту пользователя", 27);
        Console.WriteLine();
        Animation.PrintSetCursor("5) Удалить пользователя", 27);
        Console.WriteLine("\n\n");
        Animation.PrintSetCursor("Выбор: ", 27);

        int cursorPosition = Console.GetCursorPosition().Left;
        int choice;
        while (true)
        {
            string temp = ReadLineWithCancel();
            if(exit)
                return;
            if (!int.TryParse(temp, out choice))
            {
                if (exit)
                    return;
                
                Animation.AnimationText("Ошибка ввода!", true, cursorPosition, temp.Length);
                continue;
            }

            if (int.Parse(temp) >= 1 && int.Parse(temp) <= 5)
                break;
            

            Animation.AnimationText("Ошибка в выборе!", true, cursorPosition, temp.Length);
        }


        switch ((CaseChangeProfile)choice)
        {
            case CaseChangeProfile.Wallet:
                ManagementChangeWallet();
                break;
            case CaseChangeProfile.Name:
                break;
            case CaseChangeProfile.FamilyName:
                break;
            case CaseChangeProfile.Email:
                break;
            case CaseChangeProfile.Delete:
                break;
            default:
                throw CheckoutException.Canceled;
        }
    }
    
    public bool ManagementChangeWallet()
    {
        exit = false;
        ChangeWallet wallet = new ChangeWallet();
        Console.Clear();
        
        Animation.PrintRedText("Смена кошелька", true, false, 50);
        Console.WriteLine();
        Animation.PrintRedText("Выход на Esc", true, false, 50);
        
        
        Animation.PrintSetCursor("Введите данные", 18, true, 4);
        Console.WriteLine();
        Animation.PrintSetCursor("Имя пользователя: ", 18);
        wallet.name = ReadLineWithCancel();
        if(exit)
            return false;
        Animation.PrintSetCursor("Почта пользователя: ", 18);
        wallet.email = ReadLineWithCancel();
        if(exit)
            return false;
        
        Console.Clear();
        
        Animation.AnimationText("Запрос данных...", false, 0, 0, true);

        Console.Clear();
        if (wallet.SearchData(conn))
        {
            Animation.PrintRedText("Ввод данных кошелька", true, false, 50);
            Console.WriteLine();
            Animation.PrintRedText("Выход на Esc", true, false, 50);
            
            Animation.PrintSetCursor("Введите ваш номер карты (напр. 1234567891234567): ", 40, true, 3);

            Decimal numberCard;

            int cursorPosition = Console.GetCursorPosition().Left;

            while (true)
            {
                string temp = ReadLineWithCancel();;
                if (!Decimal.TryParse(temp, out numberCard))
                {
                    if (exit)
                        return false;
                    
                    Animation.AnimationText("Ошибка ввода!", true, cursorPosition, temp.Length);
                    continue;
                }

                if (numberCard.ToString().Length == 16)
                    break;

                Animation.AnimationText("Ошибка в цифрах!", true, cursorPosition, temp.Length);
            }

            wallet.numberCard = numberCard;

            Animation.PrintSetCursor("Введите ваш CVC код (напр. 123): ", 40);

            cursorPosition = Console.GetCursorPosition().Left;

            int number;
            while (true)
            {
                string temp = ReadLineWithCancel();
                if (!int.TryParse(temp, out number))
                {
                    if (exit)
                        return false;
                    
                    Animation.AnimationText("Ошибка ввода!", true, cursorPosition, temp.Length);
                    continue;
                }

                if (number.ToString().Length == 3)
                    break;

                Animation.AnimationText("Ошибка в цифрах!", true, cursorPosition, temp.Length);
            }

            wallet.cvcCode = number;

            Animation.PrintSetCursor("Введите срок действия карты (напр. 2030-12): ", 40);

            cursorPosition = Console.GetCursorPosition().Left;

            while (true)
            {
                string temp = ReadLineWithCancel();
                if (exit)
                    return false;
                
                try
                {
                    wallet.Validity = DateTime.Parse(temp);
                    break;
                }
                catch (Exception)
                {
                    Animation.AnimationText("Ошибка даты!", true, cursorPosition, temp.Length);
                }

            }

            Console.Clear();
            
            Animation.PrintCenterTerminal("Загрузка данных на сервер...", true);
            Thread.Sleep(2000);
            
            if (!wallet.Upload(conn))
            {
                Console.Clear();
            
                Animation.PrintRedText("Ошибка загрузки данных!", true, true, 50);
                return false;
            }
            else
            {
                Console.Clear();
                Animation.PrintRedText("Успешно!", true, true, 50);
            }
            return true;
        }
        else
        {
            Animation.PrintRedText("Такого кошелька нету!", true, true, 50);
            return false;
        }
    }
    
}
