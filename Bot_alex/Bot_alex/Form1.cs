using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bot_alex
{
    //Всем привет! Это моя тпопытка реализовать мой проект с прошлой недели( БОТ Александра, лучший виртуальный помощник на территории СНГ) 
    //в виде телеграмм бота.
   
    //Вот, кстати, адрес бота: @lovelettergame_bot
     
    public partial class Form1 : Form
    {
        BackgroundWorker bw;
        public Form1()
        {
            InitializeComponent();
            this.bw = new BackgroundWorker();
            this.bw.DoWork += bw_DoWork;
        }
        static string weather()
        {
            string HtmlText = string.Empty;
            HttpWebRequest myHttwebrequest = (HttpWebRequest)HttpWebRequest.Create(@"https://sinoptik.ua/%D0%BF%D0%BE%D0%B3%D0%BE%D0%B4%D0%B0-%D0%B4%D0%BD%D0%B5%D0%BF%D1%80-303007131");
            HttpWebResponse myHttpWebresponse = (HttpWebResponse)myHttwebrequest.GetResponse();
            StreamReader strm = new StreamReader(myHttpWebresponse.GetResponseStream());
            HtmlText = strm.ReadToEnd();// код нашей html страницы
            string body = Regex.Match(HtmlText, @"class=""today-temp"">(.*?)<", RegexOptions.Singleline).Groups[1].Value;// получаем температуру с сайта
            string description = Regex.Match(HtmlText, @"class=""today-time"">(.*?)<", RegexOptions.Singleline).Groups[1].Value;// получаем какому времении соответствует температура
            body = body.Replace("&deg;", "°");  // переводим в "читаемый" формат
            return description + " в г. Днепр: " + body + ".";  // резульат
        }
        
       
      async void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            bool a = false, b=false, m=false, d=false, f=false;
            List<string> notes = new List<string>();
            var worker = sender as BackgroundWorker;
            int changenumb = 0;
            var key = e.Argument as String; // получаем ключ из аргументов
            try
            {
                var keyboard = new Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardMarkup
                {
                    Keyboard = new[] {new[] 
                            { new Telegram.Bot.Types.KeyboardButton("Посмотреть мои записи"),
                                                    new Telegram.Bot.Types.KeyboardButton("Редактировать записи") },
                                                new[]
                                                {new Telegram.Bot.Types.KeyboardButton("Статус"),},
                                            },
                    ResizeKeyboard = true
                };
                var keyboard2 = new Telegram.Bot.Types.ReplyMarkups.ReplyKeyboardMarkup
                {
                    Keyboard = new[] {
                                               new[] {new Telegram.Bot.Types.KeyboardButton("Добавить запись")},
                                               new[] {new Telegram.Bot.Types.KeyboardButton("Изменить запись")},
                                               new[]{new Telegram.Bot.Types.KeyboardButton("Удалить запись")},
                                               new[]{new Telegram.Bot.Types.KeyboardButton("В главное меню")},
                                            },
                    ResizeKeyboard = true
                };
                var keyboard3 = new Telegram.Bot.Types.ReplyMarkups.InlineKeyboardMarkup(new Telegram.Bot.Types.InlineKeyboardButton[][] { new[] { new Telegram.Bot.Types.InlineKeyboardButton("OK", "callback1"), }, });
                var Bot = new Telegram.Bot.TelegramBotClient(key); 
                await Bot.SetWebhookAsync("");
                Bot.OnUpdate += async (object su, Telegram.Bot.Args.UpdateEventArgs evu) =>
                {
                    if (evu.Update.CallbackQuery != null || evu.Update.InlineQuery != null) return; // в этом блоке нам келлбэки и инлайны не нужны
                    var update = evu.Update;
                    var message = update.Message;
                    if (message == null) return;
                    if (message.Text == null) return;
                    if (message.Type == Telegram.Bot.Types.Enums.MessageType.TextMessage)
                    if (message.Text == "/start")
                    {
                        await Bot.SendTextMessageAsync(message.Chat.Id, "Добрый день! Меня зовут Александра, я Ваш виртуальный помощник!", false);
                        await Bot.SendTextMessageAsync(message.Chat.Id, "Введите /show, чтоб открыть меню...", false);
                    }
                    if (message.Text == "/show")
                    {
                        await Bot.SendTextMessageAsync(message.Chat.Id, "Чем Вам помочь?", false);
                        await Bot.SendTextMessageAsync(message.Chat.Id, "Что Вы выберите?", false, false, 0, keyboard, Telegram.Bot.Types.Enums.ParseMode.Default);
                    }
                    if (message.Text.ToLower() == "посмотреть мои записи")
                    {
                        if (notes.Count == 0)
                        await Bot.SendTextMessageAsync(message.Chat.Id, "Ваш список пуст", false);
                        foreach (string c in notes)
                        {
                            await Bot.SendTextMessageAsync(message.Chat.Id, Convert.ToString(notes.IndexOf(c) + 1) + ". " + c, false);//вывод наших записей
                        }
                    }
                    if (message.Text.ToLower() == "редактировать записи")
                    {
                        await Bot.SendTextMessageAsync(message.Chat.Id, "Что Вы выберите?", false, false, 0, keyboard2, Telegram.Bot.Types.Enums.ParseMode.Default);
                    }
                    if (message.Text.ToLower() == "статус")
                    {
                        await Bot.SendTextMessageAsync(message.Chat.Id, Convert.ToString(DateTime.Now), false);
                        await Bot.SendTextMessageAsync(message.Chat.Id, weather(), false);
                    }
                    
                    if (message.Text.ToLower() == "добавить запись")
                    {
                       await Bot.SendTextMessageAsync(message.Chat.Id, "Пожалуйста, введите содержимое строки, которую хотите добавить. В формате: Добавить запись:...", false);
                       a = true;
                       
                   }
                    if (message.Text.ToLower() == "изменить запись")
                    {
                        await Bot.SendTextMessageAsync(message.Chat.Id, "Пожалуйста, введите номер строки, которую хотите изменить. В формате: Изменить запись №:...", false);
                        b = true;
                       
                    }
                    if (message.Text.ToLower() == "удалить запись")
                    {
                        await Bot.SendTextMessageAsync(message.Chat.Id,"Пожалуйста, введите номер строки, которую хотите удалить. В формате: Удалить запись №:...", false);
                        d = true;
                        
                    }
                    if (message.Text.ToLower() == "в главное меню")
                    {
                        await Bot.SendTextMessageAsync(message.Chat.Id, "Что Вы выберите?", false, false, 0, keyboard, Telegram.Bot.Types.Enums.ParseMode.Default);
                    }
                   if (a && message.Text.ToLower().IndexOf("добавить запись:")>-1)
                    {
                        string addstr = message.Text.Substring(message.Text.ToLower().IndexOf("добавить запись:") + "добавить запись:".Length); ;
                        notes.Add(addstr);
                        a = false;
                        await Bot.SendTextMessageAsync(message.Chat.Id, "Что Вы выберите?", false, false, 0, keyboard, Telegram.Bot.Types.Enums.ParseMode.Default);
                    
                    }
                   if (b && message.Text.ToLower().IndexOf("изменить запись №:") > -1)
                   {
                       string s = Regex.Match(message.Text.Substring(message.Text.ToLower().IndexOf("изменить запись №:") + "изменить запись №:".Length), @"\d+").Value;
                       if (s != "")
                       {
                           changenumb = Int32.Parse(Regex.Match(message.Text.Substring(message.Text.ToLower().IndexOf("изменить запись №:") + "изменить запись №:".Length), @"\d+").Value);
                           if (notes.Count > changenumb - 1)
                           {
                               await Bot.SendTextMessageAsync(message.Chat.Id, "Пожалуйста, введите содержимое строки, которую хотите изменить. В формате: Новая запись:...", false);
                               m = true;
                           }
                           else
                           {
                               await Bot.SendTextMessageAsync(message.Chat.Id, "Такой записи нет!", false);
                               await Bot.SendTextMessageAsync(message.Chat.Id, "Что Вы выберите?", false, false, 0, keyboard, Telegram.Bot.Types.Enums.ParseMode.Default);


                           }
                       }
                       else await Bot.SendTextMessageAsync(message.Chat.Id, "Такой записи нет!", false);
                         b = false;  
                   }
                   if (m && message.Text.ToLower().IndexOf("новая запись:") > -1)
                   {

                       string changestr = message.Text.Substring(message.Text.ToLower().IndexOf("новая запись:") + "новая запись:".Length ); ;
                       notes.RemoveAt(changenumb - 1);
                       notes.Insert(changenumb - 1, changestr);
                       m= false;
                       await Bot.SendTextMessageAsync(message.Chat.Id, "Что Вы выберите?", false, false, 0, keyboard, Telegram.Bot.Types.Enums.ParseMode.Default);
                    
                   }
                   if (d && message.Text.ToLower().IndexOf("удалить запись №:") > -1)
                   {
                       string s = Regex.Match(message.Text.Substring(message.Text.ToLower().IndexOf("удалить запись №:") + "удалить запись №:".Length), @"\d+").Value;
                       if (s != "")
                       {
                           int delete = Int32.Parse(Regex.Match(message.Text.Substring(message.Text.ToLower().IndexOf("удалить запись №:") + "удалить запись №:".Length), @"\d+").Value);
                           if (notes.Count > delete - 1)
                           {
                               notes.RemoveAt(delete - 1);
                           }
                           else await Bot.SendTextMessageAsync(message.Chat.Id, "Такой записи нет!", false);
                       }else await Bot.SendTextMessageAsync(message.Chat.Id, "Такой записи нет!", false);
                       b = false;
                       await Bot.SendTextMessageAsync(message.Chat.Id, "Что Вы выберите?", false, false, 0, keyboard, Telegram.Bot.Types.Enums.ParseMode.Default);
                   }
                   
                      
                   
                    
                };
               

               Bot.StartReceiving();
            }
            catch (Telegram.Bot.Exceptions.ApiRequestException ex)
            {
                Console.WriteLine(ex.Message); 
            }

        }

        private void BtnRun_Click(object sender, EventArgs e)
        {
                        
            var text = @txtKey.Text; 
            if (text != "" && this.bw.IsBusy != true)
            {
                this.bw.RunWorkerAsync(text);
                BtnRun.Text = "Бот запущен...";
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            BtnRun_Click(sender, e);
        }
    }
}
