using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace MyApp;

public class Menu
{
    interface IUploadInServer
    {
        public void Upload(SqlConnection connection);
    }
    
    public class UserRegistaryForm : IUploadInServer
    {
        public string name { private get; set; }
        public string familyName { private get; set; }
        public string email { private get; set; }
        public ulong numberCard { private get; set; }
        public int cvcCode { private get; set; }
        public DateTime Validity { private get; set; }
        
        public UserRegistaryForm(){}

        public override string ToString()
            => $"{name} {familyName} {email} {numberCard} {cvcCode} {Validity}";


        public void Upload(SqlConnection connection)
        {
            string connectionString =
                "INSERT INTO Wallets (Number_card, THREE_code, Validity) VALUES (@numberCard, @cvcCode, @Validity)";

            using (SqlCommand command = new SqlCommand(connectionString, connection))
            {
                SqlParameter parameter = new SqlParameter("@numberCard", SqlDbType.Decimal);
                parameter.Precision = 16;
                parameter.Scale = 0;
                parameter.Value = numberCard;
                
                command.Parameters.AddWithValue("@cvcCode", cvcCode);
                command.Parameters.AddWithValue("@Validity", Validity);
                
                command.ExecuteNonQuery();
            }
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
        UserRegistaryForm user = new UserRegistaryForm();
        
        Console.Clear();
        Animation.PrintRedText("Регистрация профиля", true, false, 70);
        Animation.PrintSetCursor("Введите ваше имя: ", 22, true, 6);
        user.name = Console.ReadLine();
        Animation.PrintSetCursor("Введите вашу фамилию: ", 22);
        user.familyName = Console.ReadLine();
        Animation.PrintSetCursor("Введите ваш email: ", 22);
        user.email = Console.ReadLine();
        Animation.PrintSetCursor("Введите ваш номер карты (напр. 1234567891234567): ", 22);

        ulong numberCard;

        int cursorPosition = Console.GetCursorPosition().Left;
        
        while (true)
        {
            string temp = Console.ReadLine();
            if (!ulong.TryParse(temp, out numberCard))
            {
                Animation.AnimationText("Ошибка ввода!", true, cursorPosition, temp.Length);
                continue;
            }
            if(numberCard.ToString().Length == 16)
                break;
            
            Animation.AnimationText("Ошибка в цифрах!", true, cursorPosition, temp.Length);
        }
        user.numberCard = numberCard;
        
        Animation.PrintSetCursor("Введите ваш CVC код (напр. 123): ", 22);
        
        cursorPosition = Console.GetCursorPosition().Left;
        
        while (true)
        {
            string temp = Console.ReadLine();
            if (!ulong.TryParse(temp, out numberCard))
            {
                Animation.AnimationText("Ошибка ввода!", true, cursorPosition, temp.Length);
                continue;
            }
            if(numberCard.ToString().Length == 3)
                break;
            
            Animation.AnimationText("Ошибка в цифрах!", true, cursorPosition, temp.Length);
        }

        user.cvcCode = int.Parse(numberCard.ToString());
        
        Animation.PrintSetCursor("Введите срок действия карты (напр. 2030-12-30): ", 22);

        cursorPosition = Console.GetCursorPosition().Left;
        
        while (true)
        {
            string temp = Console.ReadLine();

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
}