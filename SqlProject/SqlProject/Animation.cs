using System.Text;

namespace MyApp;

public class Animation
{
    
    public static List<string> logo = new List<string>()
    {
        "▄▄▄  ▄▄▄ . ▐ ▄ ▄▄▄▄▄ ▄▄▄· ▄▄▌    .▄▄ ·  ▄· ▄▌.▄▄ · ▄▄▄▄▄▄▄▄ .• ▌ ▄ ·.      ",
        "▀▄ █·▀▄.▀·•█▌▐█•██  ▐█ ▀█ ██•    ▐█ ▀. ▐█▪██▌▐█ ▀. •██  ▀▄.▀··██ ▐███▪     ",
        "▀▀▄ ▐▀▀▪▄▐█▐▐▌ ▐█.▪▄█▀▀█ ██ ▪   ▄▀▀▀█▄▐█▌▐█▪▄▀▀▀█▄ ▐█.▪▐▀▀▪▄▐█ ▌▐▌▐█·      ",
        "█•█▌▐█▄▄▌██▐█▌ ▐█▌·▐█▪ ▐▌▐█▌ ▄  ▐█▄▪▐█ ▐█▀·.▐█▄▪▐█ ▐█▌·▐█▄▄▌██ ██▌▐█▌      ",
        "         ▄▄▄▄·  ▄· ▄▌  ▄▄▌ ▐ ▄▌ ▄ .▄       ▄▄▄· • ▌ ▄ ·. ▪      ",
        "         ▐█ ▀█▪▐█▪██▌  ██· █▌▐███▪▐█ ▄█▀▄ ▐█ ▀█ ·██ ▐███▪██     ",
        "         ▐█▀▀█▄▐█▌▐█▪  ██▪▐█▐▐▌██▀▀█▐█▌.▐▌▄█▀▀█ ▐█ ▌▐▌▐█·▐█·    ",
        "         ██▄▪▐█ ▐█▀·.  ▐█▌██▐█▌██▌▐▀▐█▌.▐▌▐█▪ ▐▌██ ██▌▐█▌▐█▌    ",
        "         ·▀▀▀▀   ▀ •    ▀▀▀▀ ▀▪▀▀▀ · ▀█▄▀▪ ▀  ▀ ▀▀  █▪▀▀▀▀▀▀    "
    };
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
            Thread.Sleep(70);
            
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
            Thread.Sleep(1000);
        
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

    public static void PrintSetCursor(string text, int length, bool center = false, int durabily = 0)
    {
        if(center)
            if (center)
                for (int i = 0; i < Console.WindowHeight/2 - durabily/2; i++)
                    Console.WriteLine();
        
        Console.SetCursorPosition((Console.WindowWidth - length) / 2, Console.CursorTop);
        Console.Write(text);
    }

    public static void Exit()
    {
        Console.Clear();
        Thread.Sleep(500);
        PrintRedText("Прощай!", true, true, 100);
        Console.Clear();
    }

    public static void LoadingDatabase()
    {
        Console.Clear();
        StringBuilder str = new("Загрузка базы данных");

        PrintCenterTerminal(str.ToString(), true);

        for (int k = 0; k < 4; k++)
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
    
    public static void AnimationText(string text, bool deleteWord = false, int cursorPosition = 0, int sizeOldText = 0, bool center = false)
    {
        StringBuilder str = new(text);
        
        int getCursor = cursorPosition;

        if (!center)
        {
            Console.SetCursorPosition(getCursor, Console.CursorTop - 1);
            for (int j = 0; j < str.Length + 4 + sizeOldText; j++)
            {
                Console.Write(' ');
            }
        }
        
        
        for (int k = 0; k < 6; k++)
        {
            for (int i = 1; i < str.Length;i++)
            {
                str[i] = char.Parse(str[i].ToString().ToUpper());

                if (!center)
                {
                    Console.SetCursorPosition(getCursor, Console.CursorTop);
                    Console.Write("::: " + str);
                    Thread.Sleep(50);
                }
                else
                {
                    for (int z = 0; z < Console.WindowHeight/2; z++)
                        Console.WriteLine();
                    
                    Console.SetCursorPosition((Console.WindowWidth - text.Length) / 2, Console.CursorTop);
                    
                    Console.Write(str);
                    Thread.Sleep(50);
                    Console.Clear();
                }
                

                
                str[i] = char.Parse(str[i].ToString().ToLower());

            }
        }

        if (deleteWord && !center)
        {
            Console.SetCursorPosition(getCursor, Console.CursorTop);
            for (int j = 0; j < str.Length + 4 + sizeOldText; j++)
            {
                Console.Write(' ');
            }
            Console.SetCursorPosition(getCursor, Console.CursorTop);
        }
        
    }
}
