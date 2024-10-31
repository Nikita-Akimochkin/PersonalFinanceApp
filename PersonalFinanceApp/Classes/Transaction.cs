using System;

namespace PersonalFinanceApp
{
    internal class Transaction
    {
        private static int iD;
        private static int userID;
        private static int amount;
        private static string category;
        private static string type;
        private static DateTime date;

        #region Transaction properties
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

        public int Amount
        {
            get { return amount; }
            set
            {
                if (value > 0) amount = value;
                else Console.WriteLine("Введите корректное значение");
            }
        }

        public string Category
        {
            get { return category; }
            set
            {
                if (!string.IsNullOrEmpty(value)) category = value;
                else Console.WriteLine("Введите корректное значение");
            }
        }

        public string Type
        {
            get { return type; }
            set
            {
                if (!string.IsNullOrEmpty(value)) type = value;
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
        #endregion

    }
}
