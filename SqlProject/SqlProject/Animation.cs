using System.Text;

namespace MyApp;

public class Animation
{
    public static void Welcome()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.White;
        string welcome = "Добро пожаловать в Rental system!";
        
        for (int i = 0; i < Console.WindowHeight/2; i++)
            Console.WriteLine();
        
        Console.SetCursorPosition((Console.WindowWidth - welcome.Length) / 2, Console.CursorTop);
        
        for (int i = 0; i < welcome.Length; i++)
        {
            if (welcome[i] == 'R')
                Console.ForegroundColor = ConsoleColor.Red;
            
            Console.Write(welcome[i]);
            Thread.Sleep(100);
            
            Console.Beep();
        }
        
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine();
        Thread.Sleep(2000);
        Console.Clear();
    }

    public static void PrintRedText(string text, bool center = false, bool centerTerminal = false, int durability = 0, bool fastEnd = false, bool newLine = false)
    {
        if(centerTerminal)
            if (centerTerminal)
                for (int i = 0; i < Console.WindowHeight/2; i++)
                    Console.WriteLine();
        
        if (center)
            Console.SetCursorPosition((Console.WindowWidth - text.Length) / 2, Console.CursorTop);
        
        Console.ForegroundColor = ConsoleColor.Red;

        for (int i = 0; i < text.Length; i++)
        {
            Console.Write(text[i]);
            Thread.Sleep(durability);
        }

        if (!fastEnd)
            Thread.Sleep(2000);
        
        if(newLine)
            Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.White;
    }

    public static void PrintCenterTerminal(string text, bool centerTerminal = false)
    {
        if (centerTerminal)
            for (int i = 0; i < Console.WindowHeight/2; i++)
                Console.WriteLine();
        
        
        Console.SetCursorPosition((Console.WindowWidth - text.Length) / 2, Console.CursorTop);

        Console.WriteLine(text);
    }

    public static void Exit()
    {
        Console.Clear();
        PrintRedText("Прощай!", true, true, 200);
        Console.Clear();
    }

    public static void LoadingDatabase()
    {
        Console.Clear();
        StringBuilder str = new("Загрузка базы данных");

        PrintCenterTerminal(str.ToString(), true);
        Thread.Sleep(1000);

        for (int k = 0; k < 5; k++)
        {
            for (int i = 1; i < str.Length;i++)
            {
                str[i] = char.Parse(str[i].ToString().ToUpper());
                Thread.Sleep(50);
                Console.Clear();
                
                PrintCenterTerminal(str.ToString(), true);
                str[i] = char.Parse(str[i].ToString().ToLower());
            }
            str.Append('.');
        }
    }
}