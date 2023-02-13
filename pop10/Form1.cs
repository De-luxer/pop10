using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;

namespace pop10
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        //Переменная соединения
        MySqlConnection conn;
        //DataAdapter представляет собой объект Command , получающий данные из источника данных.
        private MySqlDataAdapter MyDA = new MySqlDataAdapter();
        //Объявление BindingSource, основная его задача, это обеспечить унифицированный доступ к источнику данных.
        private BindingSource bSource = new BindingSource();
        //DataSet - расположенное в оперативной памяти представление данных, обеспечивающее согласованную реляционную программную 
        //модель независимо от источника данных.DataSet представляет полный набор данных, включая таблицы, содержащие, упорядочивающие 
        //и ограничивающие данные, а также связи между таблицами.
        private DataSet ds = new DataSet();
        //Представляет одну таблицу данных в памяти.
        private DataTable table = new DataTable();
        //Переменная для ID записи в БД, выбранной в гриде. Пока она не содердит значения, лучше его инициализировать с 0
        //что бы в БД не отправлялся null
        string id_selected_rows = "0";


        //Метод получения ID выделенной строки, для последующего вызова его в нужных методах
        public void GetSelectedIDString()
        {
            //Переменная для индекс выбранной строки в гриде
            string index_selected_rows;
            //Индекс выбранной строки
            index_selected_rows = dataGridView1.SelectedCells[0].RowIndex.ToString();
            //ID конкретной записи в Базе данных, на основании индекса строки
            id_selected_rows = dataGridView1.Rows[Convert.ToInt32(index_selected_rows)].Cells[0].Value.ToString();
            //Указываем ID выделенной строки в метке
            label3.Text = id_selected_rows;
        }

        //Метод изменения цвета строк, в зависимости от значения поля записи в таблице
        public void ChangeColorDGV()
        {
            string data1 = DateTime.Now.ToShortDateString();
            int count_rows = dataGridView1.RowCount;
            label1.Text = (count_rows).ToString();
            for (int i = 0; i < count_rows; i++)
            {
                DateTime id_selected_status = DateTime.Parse(dataGridView1.Rows[i].Cells[3].Value.ToString());

                if (id_selected_status < DateTime.Parse(data1))
                {
                    //Красим в красный
                    dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Red;
                }
                if (id_selected_status == DateTime.Parse(data1))
                {
                    //Красим в желтый
                    dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                }
                /*
                if (id_selected_status == DateTime.Parse(data1).AddDays(1))
                {
                    //Красим в желтый
                    dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                }
                */
            }
        }

        public void DeleteClients()
        {
            //Получаем ID изменяемого студента
            string redact_id = id_selected_rows;
            conn.Open();
            // запрос обновления данных
            string query2 = $"DELETE FROM Magaz WHERE Magaz.id_magaz='{redact_id}'";
            // объект для выполнения SQL-запроса
            MySqlCommand command = new MySqlCommand(query2, conn);
            // выполняем запрос
            try
            {
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                //Отображаем ошибку
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка удаления данных", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            // закрываем подключение к БД
            conn.Close();
            //Обновляем DataGrid
            reload_list();
        }

        public bool InsertPrepods(string title, int price, string data)
        {
            //определяем переменную, хранящую количество вставленных строк
            int InsertCount = 0;
            //Объявляем переменную храняющую результат операции
            bool result = false;
            // открываем соединение
            conn.Open();
            // запросы
            // запрос вставки данных
            string query = $"INSERT INTO Magaz (title_magaz, price_magaz, godnost_magaz) VALUES ('{title}', {price}, '{data}')";
            try
            {
                // объект для выполнения SQL-запроса
                MySqlCommand command = new MySqlCommand(query, conn);
                // выполняем запрос
                InsertCount = command.ExecuteNonQuery();
                // закрываем подключение к БД
            }
            catch (Exception ex)
            {
                //Если возникла ошибка, то запрос не вставит ни одной строки
                InsertCount = 0;
                //Отображаем ошибку
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка при добавлении клиента", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                //Но в любом случае, нужно закрыть соединение
                conn.Close();
                //Ессли количество вставленных строк было не 0, то есть вставлена хотя бы 1 строка
                if (InsertCount != 0)
                {
                    //то результат операции - истина
                    result = true;
                }
            }
            //Вернём результат операции, где его обработает алгоритм
            return result;
        }

        //Метод наполнения виртуальной таблицы и присвоение её к датагриду
        public void GetListUsers()
        {
            //Запрос для вывода строк в БД
            string commandStr = "SELECT * FROM Magaz";
            //Открываем соединение
            conn.Open();
            //Объявляем команду, которая выполнить запрос в соединении conn
            MyDA.SelectCommand = new MySqlCommand(commandStr, conn);
            //Заполняем таблицу записями из БД
            MyDA.Fill(table);
            //Указываем, что источником данных в bindingsource является заполненная выше таблица
            bSource.DataSource = table;
            //Указываем, что источником данных ДатаГрида является bindingsource 
            dataGridView1.DataSource = bSource;
            //Закрываем соединение
            conn.Close();
        }

        public void reload_list()
        {
            //Обнуляем id выбраной записи
            id_selected_rows = "0";
            label3.Text = "";
            //Зануляем штуки для фильтров
            textBox1.Text = null;
            //Чистим виртуальную таблицу
            table.Clear();
            //Вызываем метод получения записей, который вновь заполнит таблицу
            GetListUsers();
            ChangeColorDGV();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string connStr = "server=chuc.caseum.ru;port=33333;user=st_3_20_11;database=is_3_20_st11_KURS;password=67959087";
            // создаём объект для подключения к БД
            conn = new MySqlConnection(connStr);
            GetListUsers();
            ChangeColorDGV();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            //Магические строки
            try
            {
                dataGridView1.CurrentCell = dataGridView1[e.ColumnIndex, e.RowIndex];
            }
            catch
            {

            }
            dataGridView1.CurrentRow.Selected = true;
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            GetSelectedIDString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show($"Удалить продукт№ {id_selected_rows}?", "Удаление данных", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                DeleteClients();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            reload_list();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string dataStr = textBox4.Text;
            if (InsertPrepods(textBox2.Text, Convert.ToInt32(textBox3.Text), DateTime.Parse(dataStr).ToString("yyyy-MM-dd")))
            {
                MessageBox.Show("Товар добавлен");
                reload_list();
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            /*
            string s = textBox1.Text;
            string pattern = @"\w";
            for (int i = 0; i < s.Length; i++)
            {
                if (Regex.IsMatch(s[i].ToString(), pattern, RegexOptions.IgnoreCase))
                {
                    Console.WriteLine(s[i]);
                    bSource.Filter = $"title_magaz = '{s[i]}'";
                    label4.Text = $"Совадения найдены";
                }
                else
                {
                    label4.Text = "Совпадений не найдено";
                }
            }
            /////////////////////////////////////////////////////////////////////////////////
            string s = textBox1.Text;
            Regex regex = new Regex(@"(\w*)(\w*)");
            MatchCollection matches = regex.Matches(s);
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    bSource.Filter = $"title_magaz = '{match.Value}'";
                    label4.Text = $"Совадения найдены";
                }
            }
            else
            {
                label4.Text = "Совпадений не найдено";
            }
            */
            bSource.Filter = $"title_magaz LIKE '%{textBox1.Text}%'";
            ChangeColorDGV();
        }
    }
}