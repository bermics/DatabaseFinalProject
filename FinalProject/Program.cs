using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace FinalProject
{
    class Program
    {
        private const string HOST = "localhost";
        private const string USER = "root";
        private const string PASSWORD = "root";
        private const string DATABASE = "final_db";

        private static readonly string CONNECTION_STRING = $"Server={HOST};Database={DATABASE};User ID={USER};Password={PASSWORD};";

        public static void ExecuteQuery(string query, string title)
        {
            using (var conn = new MySqlConnection(CONNECTION_STRING))
            {
                try
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            var schemaTable = reader.GetSchemaTable();
                            var columnCount = schemaTable.Rows.Count;
                            var columnNames = new string[columnCount];
                            var columnWidths = new int[columnCount];
                            for (int i = 0; i < columnCount; i++)
                            {
                                columnNames[i] = schemaTable.Rows[i]["ColumnName"].ToString();
                                columnWidths[i] = columnNames[i].Length;
                            }

                            var rows = new List<string[]>();
                            while (reader.Read())
                            {
                                var row = new string[columnCount];
                                for (int i = 0; i < columnCount; i++)
                                {
                                    var value = reader.IsDBNull(i) ? "NULL" : reader.GetValue(i).ToString();
                                    row[i] = value;
                                    if (value.Length > columnWidths[i])
                                    {
                                        columnWidths[i] = value.Length;
                                    }
                                }
                                rows.Add(row);
                            }

                            Console.WriteLine($"\n{title}");

                            PrintSeparator(columnWidths);
                            PrintRow(columnNames, columnWidths);
                            PrintSeparator(columnWidths);

                            foreach (var row in rows)
                            {
                                PrintRow(row, columnWidths);
                                PrintSeparator(columnWidths);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"MySQL Error: {e.Message}");
                    Console.WriteLine($"Query: {query}");
                }
            }
        }

        private static void PrintRow(string[] row, int[] columnWidths)
        {
            Console.Write("|");
            for (int i = 0; i < row.Length; i++)
            {
                Console.Write($" {row[i].PadRight(columnWidths[i])} |");
            }
            Console.WriteLine();
        }

        private static void PrintSeparator(int[] columnWidths)
        {
            Console.Write("+");
            foreach (var width in columnWidths)
            {
                Console.Write(new string('-', width + 2));
                Console.Write("+");
            }
            Console.WriteLine();
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Initializing Database...");
            DatabaseManager.InitializeDatabase();

            string[] queries = {
                //Query 1: Payments by customer name
                @"SELECT c.name, p.amount, p.payment_date
                  FROM payments p
                  JOIN orders o ON p.order_id = o.order_id
                  JOIN customers c ON o.customer_id = c.customer_id
                  ORDER BY c.name;",

                //Query 2: Customers registered within the past year
                @"SELECT customer_id, name, email, phone, address, registration_date
                  FROM customers
                  WHERE registration_date >= CURDATE() - INTERVAL 1 YEAR;",

                //Query 3: Orders with payment details
                @"SELECT o.order_id, o.order_date, o.total_amount, p.payment_method, p.amount
                  FROM orders o
                  LEFT JOIN payments p ON o.order_id = p.order_id;",

                //Query 4: Products with price between $10 and $100
                @"SELECT product_name, price
                  FROM products
                  WHERE price BETWEEN 10 AND 100;",

                //Query 5: Products and categories
                @"SELECT p.product_name, c.category_name
                  FROM products p
                  JOIN categories c ON p.category_id = c.category_id;",

                //Query 6: Total stock quantity
                @"SELECT SUM(COALESCE(stock_quantity, 0)) AS total_stock
                  FROM products;",

                //Query 7: Total value of unsold stock for each category
                @"SELECT c.category_name, SUM(p.stock_quantity * p.price) AS total_stock_value
                  FROM products p
                  JOIN categories c ON p.category_id = c.category_id
                  GROUP BY c.category_name;",

                //Query 8: Annual revenue for each product category
                @"SELECT c.category_name, YEAR(o.order_date) AS year, SUM(o.total_amount) AS annual_revenue
                  FROM orders o
                  JOIN orderdetails od ON o.order_id = od.order_id
                  JOIN products p ON od.product_id = p.product_id
                  JOIN categories c ON p.category_id = c.category_id
                  GROUP BY c.category_name, year;",

                //Query 9: Customer spending stats
                @"SELECT c.customer_id, c.name, SUM(o.total_amount) AS total_spending,
                          COUNT(o.order_id) AS total_orders, AVG(o.total_amount) AS avg_order_value
                  FROM orders o
                  JOIN customers c ON o.customer_id = c.customer_id
                  GROUP BY c.customer_id, c.name
                  ORDER BY total_spending DESC;",

                //Query 10: Customers at risk of churn
                @"SELECT c.customer_id, c.name, MAX(o.order_date) AS last_order_date,
                          DATEDIFF(CURDATE(), MAX(o.order_date)) AS days_since_last_order
                  FROM orders o
                  JOIN customers c ON o.customer_id = c.customer_id
                  GROUP BY c.customer_id, c.name
                  HAVING days_since_last_order > 180;",

                //Query 11: Stock reorder quantities
                @"SELECT p.product_name, p.stock_quantity, 60 AS stock_needed,
                          GREATEST(0, 60 - p.stock_quantity) AS reorder_quantity
                  FROM products p;",

                //Query 12: Frequently bought product pairs
                @"SELECT p1.product_name AS product1, p2.product_name AS product2, COUNT(*) AS times_bought_together
                  FROM orderdetails od1
                  JOIN orderdetails od2 ON od1.order_id = od2.order_id AND od1.product_id < od2.product_id
                  JOIN products p1 ON od1.product_id = p1.product_id
                  JOIN products p2 ON od2.product_id = p2.product_id
                  GROUP BY p1.product_name, p2.product_name;",

                //Query 13: Reorder points and stock status
                @"SELECT p.product_name, p.stock_quantity, 10 AS avg_daily_sales,
                          70 AS reorder_point,
                          CASE WHEN p.stock_quantity >= 70 THEN 'Sufficient Stock' ELSE 'Reorder Needed' END AS stock_status
                  FROM products p;",

                //Query 14: Revenue contribution by supplier
                @"SELECT s.supplier_name, c.category_name, SUM(p.price * od.quantity) AS total_revenue
                  FROM suppliers s
                  JOIN products p ON s.supplier_id = p.supplier_id
                  JOIN categories c ON p.category_id = c.category_id
                  JOIN orderdetails od ON p.product_id = od.product_id
                  GROUP BY s.supplier_name, c.category_name;",

                //Query 15: Customers ordering the same products
                @"SELECT c1.name AS customer1, c2.name AS customer2, p.product_name
                  FROM orders o1
                  JOIN orders o2 ON o1.order_id <> o2.order_id
                  JOIN orderdetails od1 ON o1.order_id = od1.order_id
                  JOIN orderdetails od2 ON o2.order_id = od2.order_id
                  JOIN customers c1 ON o1.customer_id = c1.customer_id
                  JOIN customers c2 ON o2.customer_id = c2.customer_id
                  JOIN products p ON od1.product_id = od2.product_id
                  GROUP BY c1.name, c2.name, p.product_name
                  LIMIT 10;",

                //Query 16: Category contribution to overall sales
                @"SELECT c.category_name, (SUM(o.total_amount) / (SELECT SUM(total_amount) FROM orders)) *100 AS percentage_contribution
                FROM orders o
                JOIN orderdetails od ON o.order_id = od.order_id
                JOIN products p ON od.product_id = p.product_id
                JOIN categories c ON p.category_id = c.category_id
                GROUP BY c.category_name
                ORDER BY percentage_contribution DESC;",

                //Query 17: Most popular payment method
                @"SELECT p.payment_method, COUNT(*) AS usage_count
                  FROM payments p
                  GROUP BY p.payment_method;",

                //Query 18: Sales report for each product
                @"SELECT p.product_name, c.category_name, SUM(od.quantity) AS total_quantity_sold,
                          SUM(od.quantity * od.price_at_purchase) AS total_revenue
                  FROM products p
                  JOIN categories c ON p.category_id = c.category_id
                  JOIN orderdetails od ON p.product_id = od.product_id
                  GROUP BY p.product_name, c.category_name;",

                //Query 19: Suppliers providing lowest-priced products
                @"SELECT s.supplier_name, p.product_name, p.price
                FROM products p
                JOIN suppliers s ON p.supplier_id = s.supplier_id
                WHERE p.price = (
                    SELECT MIN(price) FROM products
                );",

                //Query 20: Category with highest revenue
                @"SELECT c.category_name, SUM(o.total_amount) AS total_revenue
                FROM orders o
                JOIN orderdetails od ON o.order_id = od.order_id
                JOIN products p ON od.product_id = p.product_id
                JOIN categories c ON p.category_id = c.category_id
                GROUP BY c.category_name
                ORDER BY total_revenue DESC
                LIMIT 1;"
};

            string[] titles = {
                "1. Customer payments listed alphabetically",
                "2. Customers who've registered within the past year",
                "3. Orders and payment details",
                "4. Products priced between $10 and $100",
                "5. Products and their category names",
                "6. Total quantity of all sroducts in stock",
                "7. Total value of unsold stock for each category",
                "8. Annual revenue for each product category",
                "9. Customers ranked based off of total spending, average order value, and number of orders",
                "10. Customers at Risk of Churn",
                "11. Stock reorder quantities",
                "12. Products frequently bought together",
                "13. Reorder points based off of sales trends",
                "14. Revenue contribution by supplier",
                "15. Customers who've ordered the same product",
                "16. Categroty contribution to sales revenue",
                "17. The most popular payment methods",
                "18. Sales report for each product",
                "19. Suppliers with the lowest priced products",
                "20. The category with the highest revenue"
            };

            if (queries.Length != titles.Length)
            {
                Console.WriteLine($"Error: The number of queries ({queries.Length}) does not match the number of titles ({titles.Length}).");
                return;
            }

            for (int i = 0; i < queries.Length; i++)
            {
                ExecuteQuery(queries[i], titles[i]);
            }

            Console.ReadKey();
        }
    }
}


