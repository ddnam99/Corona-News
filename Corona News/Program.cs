﻿using System;
using System.Text;
using System.Threading;

namespace Corona_News
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.Unicode;

            Helper.ConsoleLogs("Corona News: Started !");

            while (true)
            {
                try
                {
                    nCoV.NotifyNews();

                    double minutes = 35 - (DateTime.Now.Minute % 30); bool stop = false;
                    if (DateTime.Now.Hour < 6)
                    {
                        var now = DateTime.Now;
                        minutes = (new DateTime(now.Year, now.Month, now.Day).AddHours(6) - DateTime.Now).TotalMinutes;
                        Helper.ConsoleLogs("Corona News: Tạm dừng check nCoV cho đến 6h sáng.");
                        stop = true;
                    }

                    Thread.Sleep(TimeSpan.FromMinutes(minutes));

                    if (stop)
                    {
                        Helper.ConsoleLogs("Corona News: Bắt đầu check nCoV.");
                        stop = false;
                    }
                }
                catch (Exception e) { Helper.LogError($"Error: nCoV News...\n{e.ToString()}"); }
            }
        }
    }
}