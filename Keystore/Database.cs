using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SQLite;
using System.IO;

namespace Keystore
{
    public class Product
    {
        int _id;
        string _name;
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
    }

    public class Key
    {
        int _id;
        int _productid;
        string _code;
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }
        public int ProductId
        {
            get { return _productid; }
            set { _productid = value; }
        }
        public string Code
        {
            get { return _code; }
            set { _code = value; }
        }
    }

    public class Database
    {
        public static bool Test(string _password)
        {
            bool result = true;
            try
            {
                string connectionString = "Data Source=./Keystore.db";
                if (_password != string.Empty)
                    connectionString = "Data Source=./Keystore.db;Password=" + _password;

                SQLiteConnection db = new SQLiteConnection(connectionString);

                db.Open();

                using (SQLiteCommand cmd = new SQLiteCommand(db))
                {
                    cmd.CommandText = @"SELECT COUNT(*) FROM products;";
                    cmd.ExecuteNonQuery();
                }

                db.Close();
            }
            catch (SQLiteException)
            {
                result = false;
            }

            return result;
        }
        public static void Create(string _password)
        {
            if (!File.Exists("Keystore.db"))
            {
                SQLiteConnection.CreateFile("Keystore.db");
                try
                {
                    SQLiteConnection db = new SQLiteConnection("Data Source=./Keystore.db");
                    if (_password != string.Empty)
                        db.SetPassword(_password);

                    db.Open();

                    using (SQLiteCommand cmd = new SQLiteCommand(db))
                    {
                        cmd.CommandText = @"CREATE TABLE IF NOT EXISTS products (id INTEGER PRIMARY KEY AUTOINCREMENT, name TEXT UNIQUE);";
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = @"CREATE TABLE IF NOT EXISTS keys (id INTEGER PRIMARY KEY AUTOINCREMENT, productid INTEGER, code TEXT);";
                        cmd.ExecuteNonQuery();
                    }

                    db.Close();
                }
                catch (SQLiteException)
                {
                    //
                }
            }
        }

        public static void SetPassword(string password, string _password = "")
        {
            string connectionString = "Data Source=./Keystore.db";
            if (_password != string.Empty)
                connectionString = "Data Source=./Keystore.db;Password=" + _password;

            try
            {
                using (SQLiteConnection db = new SQLiteConnection(connectionString))
                {
                    db.Open();
                    db.ChangePassword(password);
                    db.Close();
                }
            }
            catch (SQLiteException)
            {
                //
            }
        }

        public static List<Product> GetProducts(string _password = "")
        {
            string connectionString = "Data Source=./Keystore.db";
            if (_password != string.Empty)
                connectionString = "Data Source=./Keystore.db;Password=" + _password;

            List<Product> products = new List<Product>();

            try
            {
                using (SQLiteConnection db = new SQLiteConnection(connectionString))
                {
                    db.Open();
                    string sql = "SELECT * FROM products ORDER BY id ASC";
                    using (SQLiteCommand cmd = new SQLiteCommand(sql, db))
                    {
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Product product = new Product();
                                product.Id = Int32.Parse(reader["id"].ToString());
                                product.Name = reader["name"].ToString();
                                products.Add(product);
                            }
                        }
                    }
                    db.Close();
                }
            }
            catch (SQLiteException)
            {
                //
            }

            return products;
        }

        public static List<Key> GetKeys(int productid, string _password = "")
        {
            string connectionString = "Data Source=./Keystore.db";
            if (_password != string.Empty)
                connectionString = "Data Source=./Keystore.db;Password=" + _password;

            List<Key> keys = new List<Key>();

            try
            {
                using (SQLiteConnection db = new SQLiteConnection(connectionString))
                {
                    db.Open();
                    string sql = "SELECT * FROM keys WHERE productid = " + productid.ToString() + " ORDER BY id ASC";
                    using (SQLiteCommand cmd = new SQLiteCommand(sql, db))
                    {
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Key key = new Key();
                                key.Id = Int32.Parse(reader["id"].ToString());
                                key.ProductId = Int32.Parse(reader["productid"].ToString());
                                key.Code = reader["code"].ToString();
                                keys.Add(key);
                            }
                        }
                    }
                    db.Close();
                }
            }
            catch (SQLiteException)
            {
                //
            }

            return keys;
        }

        public static int AddProduct(string name, string _password = "")
        {
            string connectionString = "Data Source=./Keystore.db";
            if (_password != string.Empty)
                connectionString = "Data Source=./Keystore.db;Password=" + _password;

            int result = -1;

            try
            {
                using (SQLiteConnection db = new SQLiteConnection(connectionString))
                {
                    db.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(db))
                    {
                        cmd.CommandText = "INSERT INTO products (name) VALUES (@name)";
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@name", name);
                        try
                        {
                            result = cmd.ExecuteNonQuery();
                        }
                        catch (SQLiteException)
                        {
                            //
                        }
                    }
                    db.Close();
                }
            }
            catch (SQLiteException)
            {
                //
            }

            return result;
        }

        public static int UpdateProduct(int id, string name, string _password = "")
        {
            string connectionString = "Data Source=./Keystore.db";
            if (_password != string.Empty)
                connectionString = "Data Source=./Keystore.db;Password=" + _password;

            int result = -1;

            try
            {
                using (SQLiteConnection db = new SQLiteConnection(connectionString))
                {
                    db.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(db))
                    {
                        cmd.CommandText = "UPDATE products SET name = @name WHERE id = @id";
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@name", name);
                        cmd.Parameters.AddWithValue("@id", id);
                        try
                        {
                            result = cmd.ExecuteNonQuery();
                        }
                        catch (SQLiteException)
                        {
                            //
                        }
                    }
                    db.Close();
                }
            }
            catch (SQLiteException)
            {
                //
            }

            return result;
        }

        public static int AddKey(int productid, string code, string _password = "")
        {
            string connectionString = "Data Source=./Keystore.db";
            if (_password != string.Empty)
                connectionString = "Data Source=./Keystore.db;Password=" + _password;

            int result = -1;

            try
            {
                using (SQLiteConnection db = new SQLiteConnection(connectionString))
                {
                    db.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(db))
                    {
                        cmd.CommandText = "INSERT INTO keys (productid, code) VALUES (@productid, @code)";
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@productid", productid);
                        cmd.Parameters.AddWithValue("@code", code);
                        try
                        {
                            result = cmd.ExecuteNonQuery();
                        }
                        catch (SQLiteException)
                        {
                            //
                        }
                    }
                    db.Close();
                }
            }
            catch (SQLiteException)
            {
                //
            }

            return result;
        }

        public static int UpdateKey(int id, string code, string _password = "")
        {
            string connectionString = "Data Source=./Keystore.db";
            if (_password != string.Empty)
                connectionString = "Data Source=./Keystore.db;Password=" + _password;

            int result = -1;

            try
            {
                using (SQLiteConnection db = new SQLiteConnection(connectionString))
                {
                    db.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(db))
                    {
                        cmd.CommandText = "UPDATE keys SET code = @code WHERE id = @id";
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@code", code);
                        cmd.Parameters.AddWithValue("@id", id);
                        try
                        {
                            result = cmd.ExecuteNonQuery();
                        }
                        catch (SQLiteException)
                        {
                            //
                        }
                    }
                    db.Close();
                }
            }
            catch (SQLiteException)
            {
                //
            }

            return result;
        }

        public static int DeleteProduct(int id, string _password = "")
        {
            string connectionString = "Data Source=./Keystore.db";
            if (_password != string.Empty)
                connectionString = "Data Source=./Keystore.db;Password=" + _password;

            int result = -1;

            try
            {
                using (SQLiteConnection db = new SQLiteConnection(connectionString))
                {
                    db.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(db))
                    {
                        cmd.CommandText = "DELETE FROM products WHERE id = @id";
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@id", id);
                        try
                        {
                            result = cmd.ExecuteNonQuery();
                        }
                        catch (SQLiteException)
                        {
                            //
                        }

                        cmd.CommandText = "DELETE FROM keys WHERE productid = @productid";
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@productid", id);
                        cmd.ExecuteNonQuery();
                    }
                    db.Close();
                }
            }
            catch (SQLiteException)
            {
                //
            }

            return result;
        }

        public static int DeleteKey(int id, string _password = "")
        {
            string connectionString = "Data Source=./Keystore.db";
            if (_password != string.Empty)
                connectionString = "Data Source=./Keystore.db;Password=" + _password;

            int result = -1;

            try
            {
                using (SQLiteConnection db = new SQLiteConnection(connectionString))
                {
                    db.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(db))
                    {
                        cmd.CommandText = "DELETE FROM keys WHERE id = @id";
                        cmd.Prepare();
                        cmd.Parameters.AddWithValue("@id", id);
                        try
                        {
                            result = cmd.ExecuteNonQuery();
                        }
                        catch (SQLiteException)
                        {
                            //
                        }
                    }
                    db.Close();
                }
            }
            catch (SQLiteException)
            {
                //
            }
            return result;
        }

        public static string RandomKey(bool delete, int productid, string _password = "")
        {
            string result = "";
            int id = 0;

            string connectionString = "Data Source=./Keystore.db";
            if (_password != string.Empty)
                connectionString = "Data Source=./Keystore.db;Password=" + _password;

            try
            {
                using (SQLiteConnection db = new SQLiteConnection(connectionString))
                {
                    db.Open();
                    string sql = "SELECT * FROM keys WHERE productid = " + productid.ToString() + " ORDER BY RANDOM() LIMIT 1";
                    using (SQLiteCommand cmd = new SQLiteCommand(sql, db))
                    {
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Key key = new Key();
                                key.Id = Int32.Parse(reader["id"].ToString());
                                key.ProductId = Int32.Parse(reader["productid"].ToString());
                                key.Code = reader["code"].ToString();
                                result = key.Code;
                                id = key.Id;
                            }
                        }
                    }
                    db.Close();
                }
            }
            catch (SQLiteException)
            {
                //
            }

            if (delete)
            {
                if(id > 0)
                    DeleteKey(id, _password);
            }

            return result;
        }
    }
}
