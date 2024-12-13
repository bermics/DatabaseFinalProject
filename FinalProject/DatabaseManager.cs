using System;
using MySql.Data.MySqlClient;

namespace FinalProject
{
    public class DatabaseManager
    {
        private const string HOST = "localhost";
        private const string USER = "root";
        private const string PASSWORD = "root";
        private const string DATABASE = "final_db";

        private static readonly string CONNECTION_STRING = $"Server={HOST};Database={DATABASE};User ID={USER};Password={PASSWORD};";

        public static void InitializeDatabase()
        {
            using (var conn = new MySqlConnection(CONNECTION_STRING))
            {
                try
                {
                    conn.Open();
                    Console.WriteLine("Connected to MySQL Server.");

                    //Create database
                    string createDatabaseQuery = $"CREATE DATABASE IF NOT EXISTS {DATABASE};";
                    ExecuteNonQuery(conn, createDatabaseQuery, "Database Creation");

                    //Drop tables
                    DropTables(conn);

                    //Create tables
                    CreateTables(conn);

                    //Populate tables with data
                    PopulateTables(conn);

                    Console.WriteLine("Database initialization complete.");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error: {e.Message}");
                }
            }
        }

        private static void DropTables(MySqlConnection conn)
        {
            string[] dropTableQueries = {
                "DROP TABLE IF EXISTS payments;",
                "DROP TABLE IF EXISTS orderdetails;",
                "DROP TABLE IF EXISTS orders;",
                "DROP TABLE IF EXISTS products;",
                "DROP TABLE IF EXISTS categories;",
                "DROP TABLE IF EXISTS suppliers;",
                "DROP TABLE IF EXISTS customers;"
            };

            foreach (var query in dropTableQueries)
            {
                ExecuteNonQuery(conn, query, "Drop Table");
            }
        }

        private static void CreateTables(MySqlConnection conn)
        {
            string[] tableCreationQueries = {
                @"CREATE TABLE IF NOT EXISTS customers (
                    customer_id INT AUTO_INCREMENT PRIMARY KEY,
                    name VARCHAR(100) NOT NULL,
                    email VARCHAR(100),
                    phone VARCHAR(15),
                    address VARCHAR(255),
                    registration_date DATE
                );",
                @"CREATE TABLE IF NOT EXISTS suppliers (
                    supplier_id INT AUTO_INCREMENT PRIMARY KEY,
                    supplier_name VARCHAR(100) NOT NULL,
                    contact_email VARCHAR(100),
                    contact_phone VARCHAR(15),
                    address VARCHAR(255)
                );",
                @"CREATE TABLE IF NOT EXISTS categories (
                    category_id INT AUTO_INCREMENT PRIMARY KEY,
                    category_name VARCHAR(100) NOT NULL
                );",
                @"CREATE TABLE IF NOT EXISTS products (
                    product_id INT AUTO_INCREMENT PRIMARY KEY,
                    product_name VARCHAR(100) NOT NULL,
                    category_id INT,
                    price DECIMAL(10, 2),
                    stock_quantity INT,
                    supplier_id INT,
                    FOREIGN KEY (category_id) REFERENCES categories(category_id),
                    FOREIGN KEY (supplier_id) REFERENCES suppliers(supplier_id)
                );",
                @"CREATE TABLE IF NOT EXISTS orders (
                    order_id INT AUTO_INCREMENT PRIMARY KEY,
                    customer_id INT,
                    order_date DATE,
                    total_amount DECIMAL(10, 2),
                    FOREIGN KEY (customer_id) REFERENCES customers(customer_id)
                );",
                @"CREATE TABLE IF NOT EXISTS orderdetails (
                    order_detail_id INT AUTO_INCREMENT PRIMARY KEY,
                    order_id INT,
                    product_id INT,
                    quantity INT,
                    price_at_purchase DECIMAL(10, 2),
                    FOREIGN KEY (order_id) REFERENCES orders(order_id),
                    FOREIGN KEY (product_id) REFERENCES products(product_id)
                );",
                @"CREATE TABLE IF NOT EXISTS payments (
                    payment_id INT AUTO_INCREMENT PRIMARY KEY,
                    order_id INT,
                    payment_date DATE,
                    payment_method VARCHAR(50),
                    amount DECIMAL(10, 2),
                    FOREIGN KEY (order_id) REFERENCES orders(order_id)
                );"
            };

            foreach (var query in tableCreationQueries)
            {
                ExecuteNonQuery(conn, query, "Table Creation");
            }
        }

        private static void PopulateTables(MySqlConnection conn)
        {
            string[] dataInsertionQueries = {
                @"INSERT IGNORE INTO customers (name, email, phone, address, registration_date) VALUES
                ('Alice Brown', 'alice@example.com', '1234567890', '123 Elm Street', '2024-01-15'),
                ('Bob Smith', 'bob@example.com', '1234567891', '456 Maple Avenue', '2024-02-20'),
                ('Charlie Davis', 'charlie@example.com', '1234567892', '789 Oak Lane', '2024-03-10'),
                ('Diana Green', 'diana@example.com', '1234567893', '101 Pine Road', '2024-01-25'),
                ('Ethan White', 'ethan@example.com', '1234567894', '202 Cedar Street', '2024-03-15'),
                ('Fiona Black', 'fiona@example.com', '1234567895', '303 Birch Avenue', '2024-02-10'),
                ('George Blue', 'george@example.com', '1234567896', '404 Walnut Drive', '2024-03-20'),
                ('Hannah Gold', 'hannah@example.com', '1234567897', '505 Chestnut Lane', '2024-01-05'),
                ('Ian Silver', 'ian@example.com', '1234567898', '606 Ash Street', '2024-02-25'),
                ('Julia Violet', 'julia@example.com', '1234567899', '707 Spruce Road', '2024-03-05');",

                @"INSERT IGNORE INTO suppliers (supplier_name, contact_email, contact_phone, address) VALUES
                ('Tech Supplies Inc.', 'contact@techsupplies.com', '9876543210', '1 Tech Park'),
                ('Home Essentials Co.', 'support@homeessentials.com', '9876543211', '2 Home Street'),
                ('Office Depot', 'sales@officedepot.com', '9876543212', '3 Office Lane'),
                ('Green Gadgets', 'info@greengadgets.com', '9876543213', '4 Gadget Avenue'),
                ('Smart Electronics', 'support@smartelectronics.com', '9876543214', '5 Smart Road'),
                ('Kitchen Wonders', 'hello@kitchenwonders.com', '9876543215', '6 Kitchen Street'),
                ('Furniture Mart', 'contact@furnituremart.com', '9876543216', '7 Furniture Way'),
                ('Book Haven', 'sales@bookhaven.com', '9876543217', '8 Book Alley'),
                ('Fashion World', 'info@fashionworld.com', '9876543218', '9 Fashion Lane'),
                ('Toy Universe', 'support@toyuni.com', '9876543219', '10 Toy Street');",

                @"INSERT IGNORE INTO categories (category_name) VALUES
                ('Electronics'), ('Home Goods'), ('Books'), ('Furniture'),
                ('Fashion'), ('Toys'), ('Kitchenware'), ('Office Supplies'),
                ('Outdoor'), ('Fitness');",

                @"INSERT IGNORE INTO products (product_name, category_id, price, stock_quantity, supplier_id) VALUES
                ('Smartphone', 1, 699.99, 50, 1),
                ('Laptop', 1, 1199.99, 30, 1),
                ('Microwave Oven', 2, 299.99, 20, 2),
                ('Fiction Book', 3, 19.99, 100, 8),
                ('Office Chair', 4, 99.99, 40, 3),
                ('T-shirt', 5, 15.99, 150, 9),
                ('Toy Car', 6, 12.99, 200, 10),
                ('Blender', 7, 49.99, 60, 6),
                ('Printer', 8, 89.99, 25, 3),
                ('Camping Tent', 9, 199.99, 15, 4);",

                @"INSERT IGNORE INTO orders (customer_id, order_date, total_amount) VALUES
                (1, '2024-04-01', 719.98),
                (2, '2024-04-05', 299.99),
                (3, '2024-04-10', 1299.99),
                (4, '2024-04-15', 15.99),
                (5, '2024-04-20', 99.99),
                (6, '2024-04-25', 49.99),
                (7, '2024-04-30', 199.99),
                (8, '2024-05-01', 119.99),
                (9, '2024-05-05', 299.99),
                (10, '2024-05-10', 12.99);",

                @"INSERT IGNORE INTO orderdetails (order_id, product_id, quantity, price_at_purchase) VALUES
                (1, 1, 1, 699.99), (1, 4, 1, 19.99),
                (2, 3, 1, 299.99), (3, 2, 1, 1299.99),
                (4, 6, 1, 15.99), (5, 5, 1, 99.99),
                (6, 8, 1, 49.99), (7, 10, 1, 199.99),
                (8, 9, 1, 119.99), (9, 3, 1, 299.99);",

                @"INSERT IGNORE INTO payments (order_id, payment_date, payment_method, amount) VALUES
                (1, '2024-04-02', 'Credit Card', 719.98),
                (2, '2024-04-06', 'PayPal', 299.99),
                (3, '2024-04-11', 'Credit Card', 1299.99),
                (4, '2024-04-16', 'Cash', 15.99),
                (5, '2024-04-21', 'Credit Card', 99.99),
                (6, '2024-04-26', 'PayPal', 49.99),
                (7, '2024-05-01', 'Credit Card', 199.99),
                (8, '2024-05-02', 'Debit Card', 119.99),
                (9, '2024-05-06', 'PayPal', 299.99),
                (10, '2024-05-11', 'Credit Card', 12.99);"
            };

            foreach (var query in dataInsertionQueries)
            {
                ExecuteNonQuery(conn, query, "Data Insertion");
            }
        }

        private static void ExecuteNonQuery(MySqlConnection conn, string query, string description)
        {
            try
            {
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.ExecuteNonQuery();
                    Console.WriteLine($"{description}: Success");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{description}: Failed - {e.Message}");
            }
        }
    }
}
