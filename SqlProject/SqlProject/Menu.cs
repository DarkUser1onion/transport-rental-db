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
        
        int cursorPosition = Console.GetCursorPosition().Left;
        
        while (true)
        {
            string tempStr = ReadLineWithCancel();

            if (!tempStr.Contains('@') || !tempStr.Contains('.'))
            {
                Animation.AnimationText("Ошибка ввода!", true, cursorPosition, tempStr.Length);
            }
            else
            {
                break;
            }
            
            if (exit)
                return null;
        }
        Animation.PrintSetCursor("Введите ваш номер карты (напр. 1234567891234567): ", 27);

        Decimal numberCard;

        cursorPosition = Console.GetCursorPosition().Left;

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
        Animation.PrintSetCursor("4) Удалить пользователя", 27);
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

            if (int.Parse(temp) >= 1 && int.Parse(temp) <= 4)
                break;
            

            Animation.AnimationText("Ошибка в выборе!", true, cursorPosition, temp.Length);
        }


        switch ((CaseChangeProfile)choice)
        {
            case CaseChangeProfile.Wallet:
                ManagementChangeWallet();
                break;
            case CaseChangeProfile.Name:
                ManagementChangeName();
                break;
            case CaseChangeProfile.FamilyName:
                ManagementChangeFamilyName();
                break;
            case CaseChangeProfile.Delete:
                ManagementRemoveUser();
                break;
            default:
                throw CheckoutException.Canceled;
        }
    }

    public bool ManagementRemoveUser()
    {
        exit = false;
        RemoveUser remover = new RemoveUser();
        
        Console.Clear();
        Animation.PrintRedText("Удаление пользователя", true, false, 50);
        Console.WriteLine();
        Animation.PrintRedText("Выход на Esc", true, false, 50);
        
        Animation.PrintSetCursor("Введите данные пользователя для удаления", 35, true, 4);
        Console.WriteLine();
        Animation.PrintSetCursor("Имя: ", 35);
        remover.name = ReadLineWithCancel();
        if (exit)
            return false;
        Animation.PrintSetCursor("Фамилия: ", 35);
        remover.familyName = ReadLineWithCancel();
        if (exit)
            return false;
        Animation.PrintSetCursor("Email: ", 35);
        remover.email = ReadLineWithCancel();
        if (exit)
            return false;
        
        Console.Clear();
        Animation.AnimationText("Поиск пользователя...", false, 0, 0, true);
        
        if (!remover.SearchData(conn))
        {
            Animation.PrintRedText("Пользователь не найден!", true, true, 50);
            return false;
        }
        
        if (remover.CheckRentals(conn))
        {
            Animation.PrintRedText("У пользователя есть активные аренды! Удаление невозможно.", true, true, 50);
            return false;
        }
        
        Console.Clear();
        Animation.PrintRedText("Вы уверены, что хотите удалить пользователя?", true, false, 50);
        Console.WriteLine();
        Animation.PrintSetCursor("1) Да", 18, true, 3);
        Console.WriteLine();
        Animation.PrintSetCursor("2) Нет", 18);
        Console.WriteLine();
        Animation.PrintSetCursor("Выбор: ", 18);
        
        int cursorPosition = Console.GetCursorPosition().Left;
        int choice;
        while (true)
        {
            string temp = ReadLineWithCancel();
            if (exit)
                return false;
            if (!int.TryParse(temp, out choice))
            {
                Animation.AnimationText("Ошибка ввода!", true, cursorPosition, temp.Length);
                continue;
            }
            if (choice == 1 || choice == 2)
                break;
            Animation.AnimationText("Ошибка в выборе!", true, cursorPosition, temp.Length);
        }
        
        if (choice == 2)
        {
            Console.Clear();
            Animation.PrintRedText("Удаление отменено!", true, true, 50);
            return false;
        }
        
        Console.Clear();
        Animation.AnimationText("Удаление пользователя...", false, 0, 0, true);
        
        if (!remover.Upload(conn))
        {
            Console.Clear();
            Animation.PrintRedText("Ошибка удаления пользователя!", true, true, 50);
            return false;
        }
        else
        {
            Console.Clear();
            Animation.PrintRedText("Пользователь успешно удален!", true, true, 50);
            return true;
        }
    }
    
    public bool ManagementChangeFamilyName()
    {
        exit = false;
        ChangeNameOrFamilyName changer = new ChangeNameOrFamilyName();
        
        Console.Clear();
        Animation.PrintRedText("Смена фамилии", true, false, 50);
        Console.WriteLine();
        Animation.PrintRedText("Выход на Esc", true, false, 50);
        
        Animation.PrintSetCursor("Введите данные", 18, true, 4);
        Console.WriteLine();
        Animation.PrintSetCursor("Имя пользователя: ", 18);
        changer.name = ReadLineWithCancel();
        if(exit)
            return false;
        Animation.PrintSetCursor("Почта пользователя: ", 18);
        changer.email = ReadLineWithCancel();
        if(exit)
            return false;
        
        Console.Clear();
        
        Animation.AnimationText("Запрос данных...", false, 0, 0, true);
        
        if (changer.SearchDataFamily(conn))
        {
            Console.Clear();
            Animation.PrintRedText("Ввод новых данных", true, false, 50);
            Console.WriteLine();
            Animation.PrintRedText("Выход на Esc", true, false, 50);
            
            Animation.PrintSetCursor("Введите ваше новую фамилию: ", 27, true, 1);
            changer.familyName = ReadLineWithCancel();
            
            
            Console.Clear();
            Animation.AnimationText("Загрузка данных на сервер...", false, 0, 0, true);
            
            if (!changer.Upload(conn))
            {
                Console.Clear();
                Animation.PrintRedText("Ошибка загрузки данных!", true, true, 50);
                
                return false;
            }
            else
            {
                Console.Clear();
                Animation.PrintRedText("Успешно!", true, true, 50);
                
                return true;
            }
        }
        else
        {
            Animation.PrintRedText("Такого пользователя нету!", true, true, 50);
            return false;
        }
    }
    
    public bool ManagementChangeName()
    {
        exit = false;
        ChangeNameOrFamilyName changer = new ChangeNameOrFamilyName();
        
        Console.Clear();
        Animation.PrintRedText("Смена имени", true, false, 50);
        Console.WriteLine();
        Animation.PrintRedText("Выход на Esc", true, false, 50);
        
        Animation.PrintSetCursor("Введите данные", 18, true, 4);
        Console.WriteLine();
        Animation.PrintSetCursor("Фамилия пользователя: ", 18);
        changer.familyName = ReadLineWithCancel();
        if(exit)
            return false;
        Animation.PrintSetCursor("Почта пользователя: ", 18);
        changer.email = ReadLineWithCancel();
        if(exit)
            return false;
        
        Console.Clear();
        
        Animation.AnimationText("Запрос данных...", false, 0, 0, true);
        
        
        
        Console.Clear();
        if (changer.SearchData(conn))
        {
            Console.Clear();
            Animation.PrintRedText("Ввод новых данных", true, false, 50);
            Console.WriteLine();
            Animation.PrintRedText("Выход на Esc", true, false, 50);
            
            Animation.PrintSetCursor("Введите ваше новое имя: ", 27, true, 1);
            changer.name = ReadLineWithCancel();
            
            
            Console.Clear();
            Animation.AnimationText("Загрузка данных на сервер...", false, 0, 0, true);
            

            if (!changer.Upload(conn))
            {
                Console.Clear();
                Animation.PrintRedText("Ошибка загрузки данных!", true, true, 50);
                
                return false;
            }
            else
            {
                Console.Clear();
                Animation.PrintRedText("Успешно!", true, true, 50);
                
                return true;
            }
        }
        else
        {
            Animation.PrintRedText("Такого пользователя нету!", true, true, 50);
            return false;
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

        if (wallet.SearchData(conn))
        {
            Console.Clear();
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
            Animation.AnimationText("Загрузка данных на сервер...", false, 0, 0, true);

            
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
    
    public void ManagementFleetMenu(SqlConnection connection)
    {
        exit = false;
        
        Console.Clear();
        Animation.PrintRedText("Управление автопарком", true, false, 50);
        Console.WriteLine();
        Animation.PrintRedText("Выход на Esc", true, false, 50);
        
        Animation.PrintSetCursor("Что нужно сделать?", 27, true, 6);
        Console.WriteLine();
        Animation.PrintSetCursor("1) Добавить транспорт", 27);
        Console.WriteLine();
        Animation.PrintSetCursor("2) Удалить транспорт", 27);
        Console.WriteLine();
        Animation.PrintSetCursor("3) Изменить статус транспорта", 27);
        Console.WriteLine();
        Animation.PrintSetCursor("4) Просмотреть весь транспорт", 27);
        Console.WriteLine();
        Animation.PrintSetCursor("5) Управление статусами", 27);
        Console.WriteLine();
        Animation.PrintSetCursor("6) Добавить парковочную зону", 27);
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
                Animation.AnimationText("Ошибка ввода!", true, cursorPosition, temp.Length);
                continue;
            }

            if (choice >= 1 && choice <= 6)
                break;

            Animation.AnimationText("Ошибка в выборе!", true, cursorPosition, temp.Length);
        }

        switch ((CaseFleetManagement)choice)
        {
            case CaseFleetManagement.AddVehicle:
                ManagementAddVehicle();
                break;
            case CaseFleetManagement.RemoveVehicle:
                ManagementRemoveVehicle();
                break;
            case CaseFleetManagement.ChangeStatus:
                ManagementChangeVehicleStatus();
                break;
            case CaseFleetManagement.ViewAll:
                ManagementViewAllVehicles();
                break;
            case CaseFleetManagement.ManageStatuses:
                ManagementStatusMenu(connection);
                break;
            case CaseFleetManagement.AddParkingZone:
                ManagementAddParkingZone();
                break;
            default:
                throw CheckoutException.Canceled;
        }
    }

    public bool ManagementAddVehicle()
    {
        exit = false;
        AddVehicle vehicle = new AddVehicle();
        
        Console.Clear();
        Animation.PrintRedText("Добавление транспорта", true, false, 50);
        Console.WriteLine();
        Animation.PrintRedText("Выход на Esc", true, false, 50);
        
        Animation.PrintSetCursor("Введите QR-код транспорта (19 символов): ", 27, true, 1);
        
        int cursorPosition = Console.GetCursorPosition().Left;
        string qrCode;
        while (true)
        {
            qrCode = ReadLineWithCancel();
            if (exit) return false;
            
            if (qrCode.Length == 19)
                break;
                
            Animation.AnimationText("QR-код должен быть 19 символов!", true, cursorPosition, qrCode.Length);
        }
        vehicle.QrCode = qrCode;
        
        Animation.PrintSetCursor("Введите тип транспорта: ", 27);
        vehicle.TypeTransport = ReadLineWithCancel();
        if (exit) return false;
        
        Console.Clear();
        Animation.AnimationText("Загрузка списка статусов...", false, 0, 0, true);
        
        var statuses = conn.Query("SELECT * FROM Status").ToList();
        
        Console.Clear();
        Animation.PrintRedText("Выберите статус транспорта", true, false, 50);
        Console.WriteLine();
        Animation.PrintRedText("Выход на Esc", true, false, 50);
        Console.WriteLine();
        Console.WriteLine();
        
        for (int i = 0; i < statuses.Count; i++)
        {
            Animation.PrintSetCursor($"{i + 1}) {statuses[i].Type_status} - {statuses[i].Description}", 27);
            Console.WriteLine();
        }
        Console.WriteLine();
        
        Animation.PrintSetCursor("Выбор: ", 27);
        
        cursorPosition = Console.GetCursorPosition().Left;
        int choice;
        while (true)
        {
            string temp = ReadLineWithCancel();
            if (exit) return false;
            if (!int.TryParse(temp, out choice))
            {
                Animation.AnimationText("Ошибка ввода!", true, cursorPosition, temp.Length);
                continue;
            }
            if (choice >= 1 && choice <= statuses.Count)
                break;
            Animation.AnimationText("Ошибка в выборе!", true, cursorPosition, temp.Length);
        }
        
        vehicle.StatusId = statuses[choice - 1].id;
        
        Console.Clear();
        Animation.AnimationText("Добавление транспорта...", false, 0, 0, true);
        
        if (!vehicle.Upload(conn))
        {
            Console.Clear();
            Animation.PrintRedText("Ошибка добавления транспорта!", true, true, 50);
            return false;
        }
        else
        {
            Console.Clear();
            Animation.PrintRedText("Транспорт успешно добавлен!", true, true, 50);
            return true;
        }
    }

    public bool ManagementRemoveVehicle()
    {
        exit = false;
        RemoveVehicle remover = new RemoveVehicle();
        
        Console.Clear();
        Animation.PrintRedText("Удаление транспорта", true, false, 50);
        Console.WriteLine();
        Animation.PrintRedText("Выход на Esc", true, false, 50);
        
        Animation.PrintSetCursor("Введите QR-код транспорта для удаления (19 символов): ", 27, true, 6);
        
        int cursorPosition = Console.GetCursorPosition().Left;
        string qrCode;
        while (true)
        {
            qrCode = ReadLineWithCancel();
            if (exit) return false;
            
            if (qrCode.Length == 19)
                break;
                
            Animation.AnimationText("QR-код должен быть 19 символов!", true, cursorPosition, qrCode.Length);
        }
        remover.QrCode = qrCode;
        
        Console.Clear();
        Animation.AnimationText("Поиск транспорта...", false, 0, 0, true);
        
        if (!remover.SearchData(conn))
        {
            Animation.PrintRedText("Транспорт не найден!", true, true, 50);
            return false;
        }
        
        if (remover.CheckRentals(conn))
        {
            Animation.PrintRedText("У транспорта есть активные аренды! Удаление невозможно.", true, true, 50);
            return false;
        }
        
        Console.Clear();
        Animation.PrintRedText("Вы уверены, что хотите удалить транспорт?", true, false, 50);
        Console.WriteLine();
        Animation.PrintSetCursor("1) Да", 18, true, 3);
        Console.WriteLine();
        Animation.PrintSetCursor("2) Нет", 18);
        Console.WriteLine();
        Animation.PrintSetCursor("Выбор: ", 18);
        
        cursorPosition = Console.GetCursorPosition().Left;
        int choice;
        while (true)
        {
            string temp = ReadLineWithCancel();
            if (exit) return false;
            if (!int.TryParse(temp, out choice))
            {
                Animation.AnimationText("Ошибка ввода!", true, cursorPosition, temp.Length);
                continue;
            }
            if (choice == 1 || choice == 2)
                break;
            Animation.AnimationText("Ошибка в выборе!", true, cursorPosition, temp.Length);
        }
        
        if (choice == 2)
        {
            Console.Clear();
            Animation.PrintRedText("Удаление отменено!", true, true, 50);
            return false;
        }
        
        Console.Clear();
        Animation.AnimationText("Удаление транспорта...", false, 0, 0, true);
        
        if (!remover.Upload(conn))
        {
            Console.Clear();
            Animation.PrintRedText("Ошибка удаления транспорта!", true, true, 50);
            return false;
        }
        else
        {
            Console.Clear();
            Animation.PrintRedText("Транспорт успешно удален!", true, true, 50);
            return true;
        }
    }

    public bool ManagementChangeVehicleStatus()
    {
        exit = false;
        ChangeVehicleStatus changer = new ChangeVehicleStatus();
        
        Console.Clear();
        Animation.PrintRedText("Изменение статуса транспорта", true, false, 50);
        Console.WriteLine();
        Animation.PrintRedText("Выход на Esc", true, false, 50);
        
        Animation.PrintSetCursor("Введите QR-код транспорта (19 символов): ", 27, true, 1);
        
        int cursorPosition = Console.GetCursorPosition().Left;
        string qrCode;
        while (true)
        {
            qrCode = ReadLineWithCancel();
            if (exit) return false;
            
            if (qrCode.Length == 19)
                break;
                
            Animation.AnimationText("QR-код должен быть 19 символов!", true, cursorPosition, qrCode.Length);
        }
        changer.QrCode = qrCode;
        
        Console.Clear();
        Animation.AnimationText("Поиск транспорта...", false, 0, 0, true);
        
        if (!changer.SearchData(conn))
        {
            Animation.PrintRedText("Транспорт не найден!", true, true, 50);
            return false;
        }
        
        var statuses = conn.Query("SELECT * FROM Status").ToList();
        
        Console.Clear();
        Animation.PrintRedText("Выберите новый статус транспорта", true, false, 50);
        Console.WriteLine();
        
        for (int i = 0; i < statuses.Count; i++)
        {
            Animation.PrintSetCursor($"{i + 1}) {statuses[i].Type_status} - {statuses[i].Description}", 27);
            Console.WriteLine();
        }
        
        Animation.PrintSetCursor("Выбор: ", 27);
        
        cursorPosition = Console.GetCursorPosition().Left;
        int choice;
        while (true)
        {
            string temp = ReadLineWithCancel();
            if (exit) return false;
            if (!int.TryParse(temp, out choice))
            {
                Animation.AnimationText("Ошибка ввода!", true, cursorPosition, temp.Length);
                continue;
            }
            if (choice >= 1 && choice <= statuses.Count)
                break;
            Animation.AnimationText("Ошибка в выборе!", true, cursorPosition, temp.Length);
        }
        
        changer.NewStatusId = statuses[choice - 1].id;
        
        Console.Clear();
        Animation.AnimationText("Изменение статуса...", false, 0, 0, true);
        
        if (!changer.Upload(conn))
        {
            Console.Clear();
            Animation.PrintRedText("Ошибка изменения статуса!", true, true, 50);
            return false;
        }
        else
        {
            Console.Clear();
            Animation.PrintRedText("Статус успешно изменен!", true, true, 50);
            return true;
        }
    }

    public void ManagementViewAllVehicles()
    {
        Console.Clear();
        Animation.AnimationText("Загрузка данных...", false, 0, 0, true);
        
        try
        {
            var vehicles = conn.Query(@"
                SELECT V.id, V.QrCode, V.Type_Transport, S.Type_status, S.Description
                FROM Vehicles V 
                INNER JOIN Status S ON V.Status = S.id").ToList();
            
            Console.Clear();
            Animation.PrintRedText("Список всего транспорта", true, false, 50);
            Console.WriteLine();
            
            if (vehicles.Count == 0)
            {
                Animation.PrintSetCursor("Транспорт не найден", 20, true, 2);
            }
            else
            {
                Animation.PrintSetCursor("ID".PadRight(5) + "QR-код".PadRight(21) + "Тип".PadRight(20) + "Статус".PadRight(15) + "Описание", 80, true, 2);
                Console.WriteLine();
                Animation.PrintSetCursor(new string('-', 80), 80);
                Console.WriteLine();
                
                foreach (var vehicle in vehicles)
                {
                    Animation.PrintSetCursor(
                        vehicle.id.ToString().PadRight(5) + 
                        vehicle.QrCode.PadRight(21) + 
                        vehicle.Type_Transport.PadRight(20) + 
                        vehicle.Type_status.PadRight(15) + 
                        vehicle.Description, 80);
                    Console.WriteLine();
                }
            }
            
            Console.WriteLine("\n\n");
            Animation.PrintSetCursor("Нажмите любую клавишу для продолжения...", 80);
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.Clear();
            Animation.PrintRedText($"Ошибка загрузки данных: {ex.Message}", true, true, 50);
        }
    }
    
    public void ManagementStatusMenu(SqlConnection connection)
    {
        exit = false;
    
        while (!exit)
        {
            Console.Clear();
            Animation.PrintRedText("Управление статусами", true, false, 50);
            Console.WriteLine();
            Animation.PrintRedText("Выход на Esc", true, false, 50);
        
            Animation.PrintSetCursor("Что нужно сделать?", 27, true, 6);
            Console.WriteLine();
            Animation.PrintSetCursor("1) Добавить новый статус", 27);
            Console.WriteLine();
            Animation.PrintSetCursor("2) Просмотреть все статусы", 27);
            Console.WriteLine();
            Animation.PrintSetCursor("3) Просмотреть парковочные зоны", 27);
            Console.WriteLine();
            Animation.PrintSetCursor("4) Назад в меню автопарка", 27);
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
                    Animation.AnimationText("Ошибка ввода!", true, cursorPosition, temp.Length);
                    continue;
                }

                if (choice >= 1 && choice <= 4)
                    break;

                Animation.AnimationText("Ошибка в выборе!", true, cursorPosition, temp.Length);
            }

            switch ((CaseStatusManagement)choice)
            {
                case CaseStatusManagement.AddStatus:
                    ManagementAddStatus();
                    break;
                case CaseStatusManagement.ViewStatuses:
                    ManagementViewAllStatuses();
                    break;
                case CaseStatusManagement.ViewParkingZones:
                    ManagementViewAllParkingZones();
                    break;
                case CaseStatusManagement.Back:
                    return;
                default:
                    throw new Exception("Неизвестный выбор");
            }
        }
    }
    
    public bool ManagementAddStatus()
    {
        exit = false;
        AddStatus status = new AddStatus();
    
        Console.Clear();
        Animation.PrintRedText("Добавление нового статуса", true, false, 50);
        Console.WriteLine();
        Animation.PrintRedText("Выход на Esc", true, false, 50);
    
        Animation.PrintSetCursor("Введите название статуса: ", 27, true, 6);
        status.TypeStatus = ReadLineWithCancel();
        if (exit)
            return false;
    
        Animation.PrintSetCursor("Введите описание статуса: ", 27);
        status.Description = ReadLineWithCancel();
        if (exit)
            return false;
    
        Console.Clear();
        Animation.AnimationText("Добавление статуса...", false, 0, 0, true);
    
        if (!status.Upload(conn))
        {
            Console.Clear();
            Animation.PrintRedText("Ошибка добавления статуса!", true, true, 50);
            return false;
        }
        else
        {
            Console.Clear();
            Animation.PrintRedText("Статус успешно добавлен!", true, true, 50);
            return true;
        }
    }
    
    public void ManagementViewAllStatuses()
    {
        Console.Clear();
        Animation.AnimationText("Загрузка данных...", false, 0, 0, true);
    
        try
        {
            var statuses = conn.Query("SELECT * FROM Status").ToList();
        
            Console.Clear();
            Animation.PrintRedText("Список всех статусов", true, false, 50);
            Console.WriteLine();
        
            if (statuses.Count == 0)
            {
                Animation.PrintSetCursor("Статусы не найдены", 20, true, 2);
            }
            else
            {
                Animation.PrintSetCursor("ID".PadRight(5) + "Название".PadRight(20) + "Описание", 20, true, 2);
                Console.WriteLine();
                Animation.PrintSetCursor(new string('-', 60), 20);
                Console.WriteLine();
            
                foreach (var status in statuses)
                {
                    Animation.PrintSetCursor(
                        status.id.ToString().PadRight(5) + 
                        status.Type_status.PadRight(20) + 
                        status.Description, 20);
                    Console.WriteLine();
                }
            }
        
            Console.WriteLine("\n\n");
            Animation.PrintSetCursor("Нажмите любую клавишу для продолжения...", 20);
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.Clear();
            Animation.PrintRedText($"Ошибка загрузки данных: {ex.Message}", true, true, 50);
        }
    }
    

    public bool StartRent()
    {
        exit = false;
        StartRental rental = new StartRental();
        
        Console.Clear();
        Animation.PrintRedText("Оформление аренды", true, false, 50);
        Console.WriteLine();
        Animation.PrintRedText("Выход на Esc", true, false, 50);
        
        Console.Clear();
        Animation.AnimationText("Проверка данных...", false, 0, 0, true);
        
        try
        {
            var userCount = conn.QueryFirstOrDefault<int>("SELECT COUNT(*) FROM Users");
            var vehicleCount = conn.QueryFirstOrDefault<int>(@"
                    SELECT COUNT(*) FROM Vehicles V 
                    INNER JOIN Status S ON V.Status = S.id 
                    WHERE S.Type_status = N'Свободен'");
            var zoneCount = conn.QueryFirstOrDefault<int>("SELECT COUNT(*) FROM Parking_Zones");
            
            Console.Clear();
            Animation.PrintRedText("Статус системы", true, false, 50);
            Console.WriteLine("\n\n\n");

            Animation.PrintSetCursor($"Пользователей: {userCount}", 20);
            Console.WriteLine();
            Animation.PrintSetCursor($"Доступного транспорта: {vehicleCount}", 20);
            Console.WriteLine();
            Animation.PrintSetCursor($"Парковочных зон: {zoneCount}", 20);
            Console.WriteLine("\n\n");
            Console.WriteLine("\n\n");
            Animation.PrintSetCursor("Нажмите любую клавишу для продолжения...", 40);
            Console.ReadKey();
            
            if (userCount == 0)
            {
                Animation.PrintRedText("В системе нет зарегистрированных пользователей!", true, true, 50);
                Animation.PrintRedText("Сначала зарегистрируйте пользователя.", true, true, 50);
                return false;
            }
            
            if (vehicleCount == 0)
            {
                Animation.PrintRedText("Нет доступного транспорта для аренды!", true, true, 50);
                return false;
            }
            
            if (zoneCount == 0)
            {
                Animation.PrintRedText("В системе нет парковочных зон!", true, true, 50);
                Animation.PrintRedText("Сначала добавьте парковочные зоны.", true, true, 50);
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.Clear();
            Animation.PrintRedText($"Ошибка проверки данных: {ex.Message}", true, true, 50);
            return false;
        }
        
        Console.Clear();
        Animation.AnimationText("Загрузка списка пользователей...", false, 0, 0, true);
        
        var users = conn.Query("SELECT id, Name, Family_Name, Email FROM Users").ToList();
        
        Console.Clear();
        Animation.PrintRedText("Выберите пользователя", true, false, 50);
        Console.WriteLine("\n\n\n");
        
        for (int i = 0; i < users.Count; i++)
        {
            string displayText = $"{i + 1}) ID:{users[i].id} {users[i].Name} {users[i].Family_Name} ({users[i].Email})";

            Animation.PrintSetCursor(displayText, 30);
            
            Console.WriteLine();
        }
        
        Animation.PrintSetCursor("Выбор: ", 30);
        
        int cursorPosition = Console.GetCursorPosition().Left;
        int userChoice;
        while (true)
        {
            string temp = ReadLineWithCancel();
            if (exit) return false;
            if (!int.TryParse(temp, out userChoice))
            {
                Animation.AnimationText("Ошибка ввода!", true, cursorPosition, temp.Length);
                continue;
            }
            if (userChoice >= 1 && userChoice <= users.Count)
                break;
            Animation.AnimationText("Ошибка в выборе!", true, cursorPosition, temp.Length);
        }
        
        rental.UserId = users[userChoice - 1].id;
        
        Console.Clear();
        Animation.AnimationText("Загрузка свободного транспорта...", false, 0, 0, true);
        
        var vehicles = conn.Query(@"SELECT V.id, V.QrCode, V.Type_Transport 
                                   FROM Vehicles V, Status S
                                   WHERE S.Type_status = N'Свободен' AND V.Status = S.id").ToList();
        
        if (vehicles.Count == 0)
        {
            Animation.PrintRedText("Нет свободного транспорта!", true, true, 50);
            return false;
        }
        
        Console.Clear();
        Animation.PrintRedText("Выберите транспорт", true, false, 50);
        Console.WriteLine("\n\n\n");
        
        for (int i = 0; i < vehicles.Count; i++)
        {
            Animation.PrintSetCursor($"{i + 1}) {vehicles[i].Type_Transport} (QR: {vehicles[i].QrCode})", 27);
            Console.WriteLine();
        }
        
        Animation.PrintSetCursor("Выбор: ", 27);
        
        cursorPosition = Console.GetCursorPosition().Left;
        int vehicleChoice;
        while (true)
        {
            string temp = ReadLineWithCancel();
            if (exit) return false;
            if (!int.TryParse(temp, out vehicleChoice))
            {
                Animation.AnimationText("Ошибка ввода!", true, cursorPosition, temp.Length);
                continue;
            }
            if (vehicleChoice >= 1 && vehicleChoice <= vehicles.Count)
                break;
            Animation.AnimationText("Ошибка в выборе!", true, cursorPosition, temp.Length);
        }
        
        rental.VehicleId = vehicles[vehicleChoice - 1].id;
        
        Console.Clear();
        Animation.AnimationText("Загрузка парковочных зон...", false, 0, 0, true);
        
        var parkingZones = conn.Query("SELECT id, Approximate_address FROM Parking_Zones").ToList();
        
        Console.Clear();
        Animation.PrintRedText("Выберите парковочную зону", true, false, 50);
        Console.WriteLine("\n\n\n");
        
        for (int i = 0; i < parkingZones.Count; i++)
        {
            string displayText = $"{i + 1}) {parkingZones[i].Approximate_address}";

            Animation.PrintSetCursor(displayText, 30);
            Console.WriteLine();
            
        }
        
        Console.WriteLine();
        Animation.PrintSetCursor("Выбор: ", 30);
        
        cursorPosition = Console.GetCursorPosition().Left;
        int zoneChoice;
        while (true)
        {
            string temp = ReadLineWithCancel();
            if (exit) return false;
            if (!int.TryParse(temp, out zoneChoice))
            {
                Animation.AnimationText("Ошибка ввода!", true, cursorPosition, temp.Length);
                continue;
            }
            if (zoneChoice >= 1 && zoneChoice <= parkingZones.Count)
                break;
            Animation.AnimationText("Ошибка в выборе!", true, cursorPosition, temp.Length);
        }
        
        rental.ParkingZoneId = parkingZones[zoneChoice - 1].id;
        
        rental.StartTrip = DateTime.Now;
        
        Console.Clear();
        Animation.AnimationText("Оформление аренды...", false, 0, 0, true);
        
        if (!rental.Upload(conn))
        {
            Console.Clear();
            Animation.PrintRedText("Ошибка оформления аренды!", true, true, 50);
            return false;
        }
        else
        {
            Console.Clear();
            Animation.PrintRedText("Аренда успешно оформлена!", true, true, 50);
            return true;
        }
    }
    

    public bool EndRent()
    {
        exit = false;
        
        Console.Clear();
        Animation.PrintRedText("Завершение аренды", true, false, 50);
        Console.WriteLine();
        Animation.PrintRedText("Выход на Esc", true, false, 50);
        
        Console.Clear();
        Animation.AnimationText("Загрузка активных аренд...", false, 0, 0, true);
        
        var activeRentals = conn.Query(@"
            SELECT 
                R.id as RentalId,
                R.Vehicle as VehicleId,
                U.Name,
                U.Family_Name, 
                V.Type_Transport, 
                V.QrCode, 
                R.Start_trip,
                PZ.Approximate_address as StartLocation
            FROM Rentals R
            INNER JOIN Users U ON R.UserId = U.id
            INNER JOIN Vehicles V ON R.Vehicle = V.id
            INNER JOIN Parking_Zones PZ ON R.Parking_zone = PZ.id
            WHERE R.End_trip IS NULL").ToList();
        
        if (activeRentals.Count == 0)
        {
            Animation.PrintRedText("Нет активных аренд!", true, true, 50);
            return false;
        }
        
        Console.Clear();
        Animation.PrintRedText("Выберите аренду для завершения", true, false, 50);
        Console.WriteLine("\n\n\n\n\n");
        
        for (int i = 0; i < activeRentals.Count; i++)
        {
            var rental = activeRentals[i];
            string displayText = $"{i + 1}) ID аренды: {rental.RentalId} - {rental.Name} {rental.Family_Name} - " +
                               $"{rental.Type_Transport} (QR: {rental.QrCode})";
            
            Animation.PrintSetCursor(displayText, 50);
            Console.WriteLine();
            Animation.PrintSetCursor($"Начало: {rental.Start_trip:g}, Место: {rental.StartLocation}", 50);
            Console.WriteLine();
        }
        
        Animation.PrintSetCursor("Выбор: ", 50);
        
        int cursorPosition = Console.GetCursorPosition().Left;
        int rentalChoice;
        while (true)
        {
            string temp = ReadLineWithCancel();
            if (exit) return false;
            if (!int.TryParse(temp, out rentalChoice))
            {
                Animation.AnimationText("Ошибка ввода!", true, cursorPosition, temp.Length);
                continue;
            }
            if (rentalChoice >= 1 && rentalChoice <= activeRentals.Count)
                break;
            Animation.AnimationText("Ошибка в выборе!", true, cursorPosition, temp.Length);
        }
        
        var selectedRental = activeRentals[rentalChoice - 1];
        
        Console.Clear();
        Animation.AnimationText("Загрузка парковочных зон...", false, 0, 0, true);
        
        var parkingZones = conn.Query("SELECT id, Approximate_address FROM Parking_Zones").ToList();
        
        if (parkingZones.Count == 0)
        {
            Animation.PrintRedText("Нет доступных парковочных зон!", true, true, 50);
            return false;
        }
        
        Console.Clear();
        Animation.PrintRedText("Выберите парковочную зону для возврата транспорта", true, false, 50);
        Console.WriteLine();
        Animation.PrintRedText($"Возвращаемый транспорт: {selectedRental.Type_Transport} (QR: {selectedRental.QrCode})", true, false, 50);
        Console.WriteLine("\n\n\n\n\n");
        
        for (int i = 0; i < parkingZones.Count; i++)
        {
            Animation.PrintSetCursor($"{i + 1}) {parkingZones[i].Approximate_address}", 27);
            Console.WriteLine();
        }
        
        Animation.PrintSetCursor("Выбор: ", 27);
        
        cursorPosition = Console.GetCursorPosition().Left;
        int zoneChoice;
        while (true)
        {
            string temp = ReadLineWithCancel();
            if (exit) return false;
            if (!int.TryParse(temp, out zoneChoice))
            {
                Animation.AnimationText("Ошибка ввода!", true, cursorPosition, temp.Length);
                continue;
            }
            if (zoneChoice >= 1 && zoneChoice <= parkingZones.Count)
                break;
            Animation.AnimationText("Ошибка в выборе!", true, cursorPosition, temp.Length);
        }
        
        EndRental endRental = new EndRental
        {
            RentalId = selectedRental.RentalId,
            NewParkingZoneId = parkingZones[zoneChoice - 1].id,
            ActualEndTime = DateTime.Now
        };
        
        Console.Clear();
        Animation.AnimationText("Завершение аренды...", false, 0, 0, true);
        
        if (!endRental.Upload(conn))
        {
            return false;
        }
        else
        {
            Console.Clear();
            Animation.PrintRedText("Аренда успешно завершена!", true, true, 50);
            Animation.PrintRedText($"Транспорт возвращен в: {parkingZones[zoneChoice - 1].Approximate_address}", true, true, 50);
            Animation.PrintRedText("Статус транспорта изменен на 'Свободен'.", true, true, 50);
            return true;
        }
    }
    
    public void FindGeo()
    {
        Console.Clear();
        Animation.AnimationText("Загрузка местоположения транспорта...", false, 0, 0, true);
        
        try
        {
            var vehicleLocations = conn.Query(@"
                SELECT 
                    V.id,
                    V.QrCode,
                    V.Type_Transport,
                    S.Type_status as VehicleStatus,
                    CASE 
                        WHEN R.id IS NOT NULL AND R.End_trip IS NULL THEN
                            PZ.Approximate_address
                        ELSE 
                            ISNULL((SELECT TOP 1 PZ2.Approximate_address 
                                   FROM Rentals R2 
                                   INNER JOIN Parking_Zones PZ2 ON R2.Parking_zone = PZ2.id 
                                   WHERE R2.Vehicle = V.id 
                                   ORDER BY R2.End_trip DESC), 'Локация неизвестна')
                    END as CurrentLocation,
                    CASE 
                        WHEN R.id IS NOT NULL AND R.End_trip IS NULL THEN 'Арендован'
                        ELSE 'Свободен'
                    END as RentalStatus,
                    CASE 
                        WHEN R.id IS NOT NULL AND R.End_trip IS NULL THEN
                            U.Name + ' ' + U.Family_Name
                        ELSE ''
                    END as RentedBy
                FROM Vehicles V
                INNER JOIN Status S ON V.Status = S.id
                LEFT JOIN Rentals R ON R.Vehicle = V.id AND R.End_trip IS NULL
                LEFT JOIN Parking_Zones PZ ON R.Parking_zone = PZ.id
                LEFT JOIN Users U ON R.UserId = U.id
                ORDER BY V.id").ToList();
            
            Console.Clear();
            Animation.PrintRedText("Геолокация транспорта", true, false, 50);
            Console.WriteLine("\n\n\n");
            
            if (vehicleLocations.Count == 0)
            {
                Animation.PrintSetCursor("Транспорт не найден", 20, true, 2);
            }
            else
            {
                Animation.PrintSetCursor("ID".PadRight(5) + "QR-код".PadRight(21) + "Тип".PadRight(15) + 
                                       "Статус".PadRight(12) + "Местоположение", 50, true, 2);
                Console.WriteLine();
                Animation.PrintSetCursor(new string('-', 80), 50);
                Console.WriteLine();
                
                foreach (var vehicle in vehicleLocations)
                {
                    Animation.PrintSetCursor(
                        vehicle.id.ToString().PadRight(5) + 
                        vehicle.QrCode.PadRight(21) + 
                        vehicle.Type_Transport.PadRight(15) + 
                        vehicle.VehicleStatus.PadRight(12) + 
                        vehicle.CurrentLocation, 50);
                    Console.WriteLine();
                    
                    if (vehicle.RentalStatus == "Арендован" && !string.IsNullOrEmpty(vehicle.RentedBy))
                    {
                        Animation.PrintSetCursor("     Арендован: " + vehicle.RentedBy, 20);
                        Console.WriteLine();
                    }
                }
            }
            
            Console.WriteLine("\n\n");
            Animation.PrintSetCursor("Нажмите любую клавишу для продолжения...", 20);
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.Clear();
            Animation.PrintRedText($"Ошибка загрузки геолокации: {ex.Message}", true, true, 50);
        }
    }
    
    public bool ManagementAddParkingZone()
    {
        exit = false;
        AddParkingZone parkingZone = new AddParkingZone();
        
        Console.Clear();
        Animation.PrintRedText("Добавление парковочной зоны", true, false, 50);
        Console.WriteLine();
        Animation.PrintRedText("Выход на Esc", true, false, 50);
        
        Animation.PrintSetCursor("Введите ширину зоны (например: 12.345678): ", 27, true, 6);
        
        int cursorPosition = Console.GetCursorPosition().Left;
        decimal width;
        while (true)
        {
            string temp = ReadLineWithCancel();
            if (exit) return false;
            if (!decimal.TryParse(temp, out width))
            {
                Animation.AnimationText("Ошибка ввода! Введите число.", true, cursorPosition, temp.Length);
                continue;
            }
            break;
        }
        parkingZone.Width = width;
        
        Animation.PrintSetCursor("Введите длину зоны (например: 12.345678): ", 27);
        
        cursorPosition = Console.GetCursorPosition().Left;
        decimal length;
        while (true)
        {
            string temp = ReadLineWithCancel();
            if (exit) return false;
            if (!decimal.TryParse(temp, out length))
            {
                Animation.AnimationText("Ошибка ввода! Введите число.", true, cursorPosition, temp.Length);
                continue;
            }
            break;
        }
        parkingZone.Length = length;
        
        Animation.PrintSetCursor("Введите приблизительный адрес: ", 27);
        parkingZone.ApproximateAddress = ReadLineWithCancel();
        if (exit) return false;
        
        Console.Clear();
        Animation.AnimationText("Добавление парковочной зоны...", false, 0, 0, true);
        
        if (!parkingZone.Upload(conn))
        {
            Console.Clear();
            Animation.PrintRedText("Ошибка добавления парковочной зоны!", true, true, 50);
            return false;
        }
        else
        {
            Console.Clear();
            Animation.PrintRedText("Парковочная зона успешно добавлена!", true, true, 50);
            return true;
        }
    }
    
    public void ManagementViewAllParkingZones()
    {
        Console.Clear();
        Animation.AnimationText("Загрузка данных...", false, 0, 0, true);
    
        try
        {
            var parkingZones = conn.Query("SELECT * FROM Parking_Zones").ToList();
        
            Console.Clear();
            Animation.PrintRedText("Список всех парковочных зон", true, false, 50);
            Console.WriteLine();
        
            if (parkingZones.Count == 0)
            {
                Animation.PrintSetCursor("Парковочные зоны не найдены", 20, true, 2);
            }
            else
            {
                Animation.PrintSetCursor("ID".PadRight(5) + "Ширина".PadRight(12) + "Длина".PadRight(12) + "Адрес", 20, true, 2);
                Console.WriteLine();
                Animation.PrintSetCursor(new string('-', 80), 20);
                Console.WriteLine();
            
                foreach (var zone in parkingZones)
                {
                    Animation.PrintSetCursor(
                        zone.id.ToString().PadRight(5) + 
                        zone.Width.ToString().PadRight(12) + 
                        zone.Length.ToString().PadRight(12) + 
                        zone.Approximate_address, 20);
                    Console.WriteLine();
                }
            }
        
            Console.WriteLine("\n\n");
            Animation.PrintSetCursor("Нажмите любую клавишу для продолжения...", 20);
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.Clear();
            Animation.PrintRedText($"Ошибка загрузки данных: {ex.Message}", true, true, 50);
        }
    }
    
    public void GenerateReport()
    {
        Console.Clear();
        Animation.AnimationText("Формирование полного отчета...", false, 0, 0, true);
        
        try
        {
            var generalStats = conn.Query(@"
                SELECT 
                    (SELECT COUNT(*) FROM Users) as TotalUsers,
                    (SELECT COUNT(*) FROM Vehicles) as TotalVehicles,
                    (SELECT COUNT(*) FROM Parking_Zones) as TotalParkingZones,
                    (SELECT COUNT(*) FROM Rentals) as TotalRentals,
                    (SELECT COUNT(*) FROM Rentals WHERE End_trip IS NULL) as ActiveRentals,
                    (SELECT COUNT(*) FROM Status) as TotalStatuses,
                    (SELECT COUNT(*) FROM Wallets) as TotalWallets
            ").First();
            
            Console.Clear();
            Animation.PrintRedText("Полный отчет системы аренды", true, false, 50);
            Console.WriteLine("\n\n\n");
            
            Animation.PrintCenterTerminal("=== ОБЩАЯ СТАТИСТИКА ===");
            Console.WriteLine("\n\n\n");
            Animation.PrintSetCursor($"Пользователей: {generalStats.TotalUsers}", 40);
            Console.WriteLine();
            Animation.PrintSetCursor($"Транспортных средств: {generalStats.TotalVehicles}", 40);
            Console.WriteLine();
            Animation.PrintSetCursor($"Парковочных зон: {generalStats.TotalParkingZones}", 40);
            Console.WriteLine();
            Animation.PrintSetCursor($"Всего аренд: {generalStats.TotalRentals}", 40);
            Console.WriteLine();
            Animation.PrintSetCursor($"Активных аренд: {generalStats.ActiveRentals}", 40);
            Console.WriteLine();
            Animation.PrintSetCursor($"Статусов: {generalStats.TotalStatuses}", 40);
            Console.WriteLine();
            Animation.PrintSetCursor($"Кошельков: {generalStats.TotalWallets}", 40);
            Console.WriteLine("\n\n");
            
            Animation.PrintCenterTerminal("Нажмите любую клавишу для продолжения...");
            Console.ReadKey();
            Console.Clear();
            Animation.AnimationText("Формирование отчета...", false, 0, 0, true);
            
            var users = conn.Query(@"
                SELECT U.id, U.Name, U.Family_Name, U.Email, W.Number_card, W.Validity,
                       (SELECT COUNT(*) FROM Rentals R WHERE R.UserId = U.id) as RentalCount
                FROM Users U 
                INNER JOIN Wallets W ON U.Wallet = W.id
                ORDER BY U.id").ToList();
            
            Console.Clear();
            Animation.PrintRedText("ОТЧЕТ: ПОЛЬЗОВАТЕЛИ", true, false, 50);
            Console.WriteLine("\n\n\n");
            
            if (users.Count == 0)
            {
                Animation.PrintSetCursor("Пользователи не найдены", 20, true, 2);
            }
            else
            {
                Animation.PrintCenterTerminal($"Всего пользователей: {users.Count}");
                Console.WriteLine("\n\n\n");
                Animation.PrintSetCursor("ID".PadRight(5) + "Имя".PadRight(15) + "Фамилия".PadRight(15) + 
                                       "Email".PadRight(25) + "Аренд".PadRight(8) + "Номер карты".PadRight(20) + "Срок действия", 80);
                Console.WriteLine();
                Animation.PrintSetCursor(new string('-', 110), 80);
                Console.WriteLine();
                
                foreach (var user in users)
                {
                    Animation.PrintSetCursor(
                        user.id.ToString().PadRight(5) + 
                        user.Name.PadRight(15) + 
                        user.Family_Name.PadRight(15) + 
                        user.Email.PadRight(25) + 
                        user.RentalCount.ToString().PadRight(8) + 
                        user.Number_card.ToString().PadRight(20) + 
                        user.Validity.ToString("MM/yyyy"), 80);
                    Console.WriteLine();
                }
            }
            
            Console.WriteLine("\n\n");
            Animation.PrintCenterTerminal("Нажмите любую клавишу для продолжения...");
            Console.ReadKey();
            Console.Clear();
            Animation.AnimationText("Формирование отчета...", false, 0, 0, true);
            
            var vehicles = conn.Query(@"
                SELECT V.id, V.QrCode, V.Type_Transport, S.Type_status as Status, S.Description,
                       (SELECT COUNT(*) FROM Rentals R WHERE R.Vehicle = V.id) as RentalCount
                FROM Vehicles V 
                INNER JOIN Status S ON V.Status = S.id
                ORDER BY V.id").ToList();
            
            Console.Clear();
            Animation.PrintRedText("ОТЧЕТ: ТРАНСПОРТ", true, false, 80);
            Console.WriteLine("\n\n\n");
            
            if (vehicles.Count == 0)
            {
                Animation.PrintSetCursor("Транспорт не найден", 20, true, 2);
            }
            else
            {
                Animation.PrintCenterTerminal($"Всего транспортных средств: {vehicles.Count}");
                Console.WriteLine("\n\n\n");
                Animation.PrintSetCursor("ID".PadRight(5) + "QR-код".PadRight(21) + "Тип".PadRight(15) + 
                                       "Статус".PadRight(12) + "Аренд".PadRight(8) + "Описание", 80);
                Console.WriteLine();
                Animation.PrintSetCursor(new string('-', 90), 80);
                Console.WriteLine();
                
                foreach (var vehicle in vehicles)
                {
                    Animation.PrintSetCursor(
                        vehicle.id.ToString().PadRight(5) + 
                        vehicle.QrCode.PadRight(21) + 
                        vehicle.Type_Transport.PadRight(15) + 
                        vehicle.Status.PadRight(12) + 
                        vehicle.RentalCount.ToString().PadRight(8) + 
                        vehicle.Description, 80);
                    Console.WriteLine();
                }
            }
            
            Console.WriteLine("\n\n");
            Animation.PrintCenterTerminal("Нажмите любую клавишу для продолжения...");
            Console.ReadKey();
            Console.Clear();
            Animation.AnimationText("Формирование отчета...", false, 0, 0, true);
            
            var parkingZones = conn.Query(@"
                SELECT PZ.id, PZ.Width, PZ.Length, PZ.Approximate_address,
                       (SELECT COUNT(*) FROM Rentals R WHERE R.Parking_zone = PZ.id) as RentalCount
                FROM Parking_Zones PZ
                ORDER BY PZ.id").ToList();
            
            Console.Clear();
            Animation.PrintRedText("ОТЧЕТ: ПАРКОВОЧНЫЕ ЗОНЫ", true, false, 50);
            Console.WriteLine("\n\n\n");
            
            if (parkingZones.Count == 0)
            {
                Animation.PrintSetCursor("Парковочные зоны не найдены", 80, true, 2);
            }
            else
            {
                Animation.PrintCenterTerminal($"Всего парковочных зон: {parkingZones.Count}");
                Console.WriteLine("\n\n\n");
                Animation.PrintSetCursor("ID".PadRight(5) + "Ширина".PadRight(12) + "Длина".PadRight(12) + 
                                       "Аренд".PadRight(8) + "Адрес", 80);
                Console.WriteLine();
                Animation.PrintSetCursor(new string('-', 80), 80);
                Console.WriteLine();
                
                foreach (var zone in parkingZones)
                {
                    Animation.PrintSetCursor(
                        zone.id.ToString().PadRight(5) + 
                        zone.Width.ToString().PadRight(12) + 
                        zone.Length.ToString().PadRight(12) + 
                        zone.RentalCount.ToString().PadRight(8) + 
                        zone.Approximate_address, 80);
                    Console.WriteLine();
                }
            }
            
            Console.WriteLine("\n\n");
            Animation.PrintCenterTerminal("Нажмите любую клавишу для продолжения...");
            Console.ReadKey();
            Console.Clear();
            Animation.AnimationText("Формирование отчета...", false, 0, 0, true);
            
            var rentals = conn.Query(@"
                SELECT 
                    R.id,
                    U.Name + ' ' + U.Family_Name as UserName,
                    V.Type_Transport as VehicleType,
                    V.QrCode,
                    PZ.Approximate_address as ParkingLocation,
                    R.Start_trip,
                    R.End_trip,
                    CASE 
                        WHEN R.End_trip IS NULL THEN 'Активна'
                        ELSE 'Завершена'
                    END as Status,
                    DATEDIFF(MINUTE, R.Start_trip, ISNULL(R.End_trip, GETDATE())) as DurationMinutes
                FROM Rentals R
                INNER JOIN Users U ON R.UserId = U.id
                INNER JOIN Vehicles V ON R.Vehicle = V.id
                INNER JOIN Parking_Zones PZ ON R.Parking_zone = PZ.id
                ORDER BY R.Start_trip DESC").ToList();
            
            Console.Clear();
            Animation.PrintRedText("ОТЧЕТ: ИСТОРИЯ АРЕНД", true, false, 50);
            Console.WriteLine("\n\n\n");
            
            if (rentals.Count == 0)
            {
                Animation.PrintSetCursor("Аренды не найдены", 80, true, 2);
            }
            else
            {
                Animation.PrintCenterTerminal($"Всего аренд в истории: {rentals.Count}");
                Console.WriteLine("\n\n\n");
                Animation.PrintSetCursor("ID".PadRight(5) + "Пользователь".PadRight(25) + "Транспорт".PadRight(15) + 
                                       "Начало".PadRight(20) + "Окончание".PadRight(20) + "Статус".PadRight(10) + "Длительность", 120);
                Console.WriteLine();
                Animation.PrintSetCursor(new string('-', 120), 120);
                Console.WriteLine();
                
                foreach (var rental in rentals)
                {
                    string endTime = rental.End_trip?.ToString("dd.MM.yyyy HH:mm") ?? "---";
                    string duration = rental.DurationMinutes >= 60 
                        ? $"{rental.DurationMinutes / 60}ч {rental.DurationMinutes % 60}м"
                        : $"{rental.DurationMinutes}м";
                    
                    Animation.PrintSetCursor(
                        rental.id.ToString().PadRight(5) + 
                        rental.UserName.PadRight(25) + 
                        rental.VehicleType.PadRight(15) + 
                        rental.Start_trip.ToString("dd.MM.yyyy HH:mm").PadRight(20) + 
                        endTime.PadRight(20) + 
                        rental.Status.PadRight(10) + 
                        duration, 120);
                    Console.WriteLine();
                }
            }
            
            var statuses = conn.Query("SELECT * FROM Status ORDER BY id").ToList();
            
            Console.WriteLine("\n\n");
            Animation.PrintSetCursor("=== СТАТУСЫ ТРАНСПОРТА ===", 20);
            Console.WriteLine("\n\n\n");
            
            if (statuses.Count == 0)
            {
                Animation.PrintSetCursor("Статусы не найдены", 20);
            }
            else
            {
                foreach (var status in statuses)
                {
                    Animation.PrintSetCursor($"ID: {status.id} - {status.Type_status}: {status.Description}", 80);
                    Console.WriteLine();
                }
            }
            
            Console.WriteLine("\n\n");
            Animation.PrintSetCursor("=== ОТЧЕТ СФОРМИРОВАН ===", 20);
            Console.WriteLine("\n\n");
            Animation.PrintCenterTerminal("Нажмите любую клавишу для возврата в меню...");
            Console.ReadKey();
            Console.Clear();
        }
        catch (Exception ex)
        {
            Console.Clear();
            Animation.PrintRedText($"Ошибка формирования отчета: {ex.Message}", true, true, 50);
            Animation.PrintRedText("Нажмите любую клавишу для продолжения...", true, true, 50);
            Console.ReadKey();
        }
    }
    
}
