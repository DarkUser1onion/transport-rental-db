namespace MyApp;

public class Menu
{
    public static void StartMenu()
    {
        Console.Clear();
        Animation.PrintRedText("Управление", true, false, 0, true, true);
        
        Animation.PrintCenterTerminal("1) Регистрация профиля");
        Animation.PrintCenterTerminal("2) Управление профилями");
        Animation.PrintCenterTerminal("3) Управление автопарком");
        Animation.PrintCenterTerminal("4) Оформление аренды");
        Animation.PrintCenterTerminal("5) Завершение аренды");
        Animation.PrintCenterTerminal("6) Узнать геолокацию транспортов");
        Animation.PrintCenterTerminal("7) Сформировать отчет");
        Animation.PrintCenterTerminal("8) Выход");
        
        Console.WriteLine();
        Animation.PrintRedText("\nВыбор: ");
    }

    public static void RegistrationProfile()
    {
        Console.Clear();
        Animation.PrintRedText("Регистрация профиля", true, false, 200);
        Console.Write("Введите ваше имя: ");
        Console.Write("Введите вашу фамилию: ");
        Console.Write("Введите ваш email: ");
        Console.Write("Введите ваш номер карты (напр. 1234567891234567): ");
        Console.Write("Введите ваш CVC код (напр. 123): ");
        Console.Write("Введите срок действия карты (напр. 01/11): ");

    }
}