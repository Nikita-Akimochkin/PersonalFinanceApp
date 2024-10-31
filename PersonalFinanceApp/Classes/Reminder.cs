using System;

namespace PersonalFinanceApp
{
    internal class Reminder
    {
        private int iD;
        private int userID;
        private string description;
        private DateTime date;

        public int ID
        {
            get { return iD; }
            set
            {
                if (value > 0) iD = value;
                else Console.WriteLine("Введите корректное значение");
            }
        }

        public int UserID
        {
            get { return userID; }
            set
            {
                if (value > 0) userID = value;
                else Console.WriteLine("Введите корректное значение");
            }
        }

        public string Description
        {
            get { return description; }
            set
            {
                if (!string.IsNullOrEmpty(value)) description = value;
                else Console.WriteLine("Введите корректное значение");
            }
        }

        public DateTime Date
        {
            get { return date; }
            set
            {
                if (value < DateTime.Now) date = value;
                else Console.WriteLine("Введите корректное значение");
            }
        }

    }
}
