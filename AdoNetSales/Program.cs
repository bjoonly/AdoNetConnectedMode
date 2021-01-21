using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdoNetSales
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["connStr"].ConnectionString;
            SqlConnection connection = new SqlConnection(connectionString);

            connection.Open();

            bool parse;
            int action;
            string productName, name, surname;
            int sellerId, buyerId;
            float price;
            DateTime firstDt, secondDt;
            try
            {
                do
                {
                    do
                    {
                        Console.Clear();
                        Console.WriteLine("Enter action:\n1.Add new sale\n2.Show all sales in period\n3.Last buy" +
                            "\n4.Remove seller\n5.Remove buyer\n6.Top seller by total sales\n7.Exit");

                        parse = int.TryParse(Console.ReadLine(), out action);
                    } while (!parse);
                    switch (action)
                    {
                        case 1:
                            do
                            {
                                Console.Clear();
                                Console.Write("Enter seller id: ");
                                parse = int.TryParse(Console.ReadLine(), out sellerId);
                            } while (!parse);
                            do
                            {
                                Console.Clear();
                                Console.Write("Enter buyer id: ");
                                parse = int.TryParse(Console.ReadLine(), out buyerId);
                            } while (!parse);
                            Console.Clear();
                            Console.Write("Enter product name: ");
                            productName = Console.ReadLine();
                            do
                            {
                                Console.Clear();
                                Console.Write("Enter price: ");
                                parse = float.TryParse(Console.ReadLine(), out price);
                            } while (!parse);
                            do
                            {
                                Console.Clear();
                                Console.Write("Enter date: ");
                                parse = DateTime.TryParse(Console.ReadLine(), out firstDt);
                            } while (!parse);
                            Console.Clear();
                            AddSales(sellerId, buyerId, productName, price, firstDt, connection);

                            break;
                        case 2:
                            do
                            {
                                Console.Clear();
                                Console.Write("Enter first date: ");
                                parse = DateTime.TryParse(Console.ReadLine(), out firstDt);
                            } while (!parse);
                            do
                            {
                                Console.Clear();
                                Console.Write("Enter second date: ");
                                parse = DateTime.TryParse(Console.ReadLine(), out secondDt);
                            } while (!parse);
                            Console.Clear();
                            SalesPeriod(firstDt, secondDt, connection);
                            Console.ReadKey();
                            break;
                        case 3:
                            Console.Clear();
                            Console.Write("Enter name: ");
                            name = Console.ReadLine();
                            Console.Clear();
                            Console.Write("Enter surname: ");
                            surname = Console.ReadLine();
                            Console.Clear();
                            LastBuyByBuyer(name, surname, connection);
                            Console.ReadKey();
                            break;
                        case 4:
                            do
                            {
                                Console.Clear();
                                Console.Write("Enter seller id: ");
                                parse = int.TryParse(Console.ReadLine(), out sellerId);
                            } while (!parse);
                            RemoveSeller(sellerId, connection);
                            Console.ReadKey();
                            break;

                        case 5:
                            do
                            {
                                Console.Clear();
                                Console.Write("Enter buyer id: ");
                                parse = int.TryParse(Console.ReadLine(), out buyerId);
                            } while (!parse);
                            RemoveBuyer(buyerId, connection);
                            Console.ReadKey();
                            break;
                        case 6:
                            Console.Clear();
                            TopSellerByTotalSales(connection);
                            Console.ReadKey();
                            break;
                        case 7:
                            break;
                        default:
                            Console.Clear();
                            Console.WriteLine("Invalid operation.");
                            Console.ReadKey();
                            break;

                    }

                } while (action != 7);
                connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }




        public static void ShowTable(SqlDataReader reader)
        {
            for (int i = 0; i < reader.FieldCount; i++)
                Console.Write("{0,-30} \t", reader.GetName(i));

            Console.WriteLine('\n' + new string('-', 146));

            while (reader.Read())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                    Console.Write("{0,-30} \t", reader[i]);
                Console.WriteLine();
            }
        }


        //1. Додати нову продажу/покупку
        public static void AddSales(int sellerId, int buyerId, string productName, float price, DateTime date, SqlConnection connection)
        {
            string cmd = @"insert into Sales(SellerId, BuyerId, ProductName, Price, Date)
                           values(@SellerId, @BuyerId, @ProductName, @Price, @Date)";
            SqlCommand command = new SqlCommand(cmd, connection);
            command.Parameters.Add("@SellerId", System.Data.SqlDbType.Int).Value = sellerId;
            command.Parameters.Add("@BuyerId", System.Data.SqlDbType.Int).Value = buyerId;
            command.Parameters.Add("@ProductName", System.Data.SqlDbType.NVarChar).Value = productName;
            command.Parameters.Add("@Price", System.Data.SqlDbType.Int).Value = price;
            command.Parameters.Add("@Date", System.Data.SqlDbType.Date).Value = date;
            SqlDataReader reader = command.ExecuteReader();
            reader.Close();
        }

        //2. Відобразити інформацію про всі продажі за певний період
        public static void SalesPeriod(DateTime firstDate, DateTime secondDate, SqlConnection connection)
        {
            string cmd = @"select s.ProductName, sl.Name+' '+sl.Surname as Seller, b.Name+' '+b.Surname as Buyer, Price, s.Date
                           from Sales as s join Sellers as sl on s.SellerId= sl.Id
                                           join Buyers as b on s.BuyerId= b.Id
                           where s.Date between @firstDate and @secondDate";
            SqlCommand command = new SqlCommand(cmd, connection);
            command.Parameters.Add("@firstDate", System.Data.SqlDbType.Date).Value = firstDate;
            command.Parameters.Add("@secondDate", System.Data.SqlDbType.Date).Value = secondDate;

            SqlDataReader reader = command.ExecuteReader();
            ShowTable(reader);
            reader.Close();
        }

        //3. Показати останню покупку певного покупця по імені та прізвищу
        public static void LastBuyByBuyer(string name, string surname, SqlConnection connection)
        {
            string cmd = @" select top 1 s.ProductName, sl.Name+' '+sl.Surname as Seller, Price, s.Date
                           from Sales as s join Sellers as sl on s.SellerId= sl.Id
                                           join Buyers as b on s.BuyerId= b.Id
                           where b.Name=@name and b.Surname=@surname
                           order by s.Date desc";
            SqlCommand command = new SqlCommand(cmd, connection);
            command.Parameters.Add("@name", System.Data.SqlDbType.NVarChar).Value = name;
            command.Parameters.Add("@surname", System.Data.SqlDbType.NVarChar).Value = surname;

            SqlDataReader reader = command.ExecuteReader();
            ShowTable(reader);
            reader.Close();
        }



        //4. Видалити продавця по Id
        public static void RemoveSeller(int id, SqlConnection connection)
        {
            string cmd = @"delete from Sales
                           from Sales
                           where SellerId=@Id
                           
                           delete from Sellers
                           from Sellers
                           where Id=@Id";
            SqlCommand command = new SqlCommand(cmd, connection);
            command.Parameters.Add("@Id", System.Data.SqlDbType.Int).Value = id;
            SqlDataReader reader = command.ExecuteReader();
            reader.Close();
        }
        //4. Видалити покупця по id
        public static void RemoveBuyer(int id, SqlConnection connection)
        {
            string cmd = @"delete from Sales
                           from Sales
                           where BuyerId=@Id
                           
                           delete from Buyers
                           from Buyers
                           where Id=@Id";
            SqlCommand command = new SqlCommand(cmd, connection);
            command.Parameters.Add("@Id", System.Data.SqlDbType.Int).Value = id;
            SqlDataReader reader = command.ExecuteReader();
            reader.Close();
        }

        //5. Показати продавця, загальна сума продаж якого є найбільшою
        public static void TopSellerByTotalSales(SqlConnection connection)
        {
            string cmd = @"select sl.Name,sl.Surname,sl.Patronymic,sl.Email,sl.PhoneNumber
                            from Sales as s join Sellers as sl on s.SellerId=sl.Id
                            group by sl.Id,sl.Name,sl.Surname,sl.Patronymic,sl.Email,sl.PhoneNumber
                            having Sum(s.Price)=(select top 1 Sum(s1.Price)
					                             from Sales as s1 join Sellers as sl1 on s1.SellerId=sl1.Id
					                             group by sl1.Id
					                             order by Sum(s1.Price) desc)";
            SqlCommand command = new SqlCommand(cmd, connection);
            SqlDataReader reader = command.ExecuteReader();
            ShowTable(reader);
            reader.Close();
        }

    }
}
