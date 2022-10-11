using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Tao.OpenGl;
using l = EasyGameEngine.log;

namespace EasyGameEngine
{
    public partial class main : Form
    {
        // массив вершин создаваемого геометрического объекта 
        private static float[,] GeomObject = new float[32, 3];

        // счетчик его вершин 
        private int count_elements = 0;

        // Куда перемещается мышь
        private MousePos mousePos { get; set; }

        // Для захвата кнопок мыши
        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(System.Windows.Forms.Keys vKey);
        bool isMouseButtonDown(Keys keys)
        {
            Int16 state = GetAsyncKeyState(keys);
            return (state & 0x8000) != 0;
        }

        public main()
        {
            InitializeComponent();
            anT.InitializeContexts();
        }

        private void main_Load(object sender, EventArgs e)
        {
            render.initializationGraph(anT);
            GeomObject[0, 0] = -0.7f;
            GeomObject[0, 1] = 0;
            GeomObject[0, 2] = 0;
            GeomObject[1, 0] = 0.7f;
            GeomObject[1, 1] = 0;
            GeomObject[1, 2] = 0;
            GeomObject[2, 0] = 0.0f;
            GeomObject[2, 1] = 0;
            GeomObject[2, 2] = 1.0f;
            GeomObject[3, 0] = 0;
            GeomObject[3, 1] = 0.7f;
            GeomObject[3, 2] = 0.3f; // количество вершин рассматриваемого геометрического объекта 
            count_elements = 4; // устанавливаем ось X по умолчанию 
            comboBox1.SelectedIndex = 0; // начало визуализации (активируем таймер) 
            RenderTime.Start();
        }

        private static void Draw()
        {
            // очистка буфера цвета и буфера глубины 
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
            Gl.glClearColor(255, 255, 255, 1); // очищение текущей матрицы 
            Gl.glLoadIdentity(); // установка черного цвета
            Gl.glColor3f(0, 0, 0); // помещаем состояние матрицы в стек матриц 
            Gl.glPushMatrix(); // перемещаем камеру для более хорошего обзора объекта 
            Gl.glTranslated(0, 0, -7); // поворачиваем ее на 15 градусов 
            Gl.glRotated(15, 1, 1, 0); // помещаем состояние матрицы в стек матриц 
            Gl.glPushMatrix(); // начинаем отрисовку объекта 
            Gl.glBegin(Gl.GL_LINE_LOOP); // геометрические данные мы берем из массива GeomObject 
            // рисуем основание с помощью зацикленной линии 
            Gl.glVertex3d(GeomObject[0, 0], GeomObject[0, 1], GeomObject[0, 2]);
            Gl.glVertex3d(GeomObject[1, 0], GeomObject[1, 1], GeomObject[1, 2]);
            Gl.glVertex3d(GeomObject[2, 0], GeomObject[2, 1], GeomObject[2, 2]);
            // завершаем отрисовку примитивов 
            Gl.glEnd(); // рисуем линии от вершин основания к вершине пирамиды 
            Gl.glBegin(Gl.GL_LINES);
            Gl.glVertex3d(GeomObject[0, 0], GeomObject[0, 1], GeomObject[0, 2]);
            Gl.glVertex3d(GeomObject[3, 0], GeomObject[3, 1], GeomObject[3, 2]);
            Gl.glVertex3d(GeomObject[1, 0], GeomObject[1, 1], GeomObject[1, 2]);
            Gl.glVertex3d(GeomObject[3, 0], GeomObject[3, 1], GeomObject[3, 2]);
            Gl.glVertex3d(GeomObject[2, 0], GeomObject[2, 1], GeomObject[2, 2]);
            Gl.glVertex3d(GeomObject[3, 0], GeomObject[3, 1], GeomObject[3, 2]); // завершаем отрисовку примитивов 
            Gl.glEnd(); // возвращаем состояние матрицы 
            Gl.glPopMatrix(); // возвращаем состояние матрицы 
            Gl.glPopMatrix(); // отрисовываем геометрию 
            Gl.glFlush(); // обновляем состояние элемента AnT.Invalidate();

        }

        private void RenderTime_Tick(object sender, EventArgs e)
        {
            Draw();
            anT.Invalidate();
            anT_KeyMouseDown();
        }

        // функция масштабирования 
        private void CreateZoom(float coef, int os)
        {
            // создаем матрицу 
            float[,] Zoom3D = new float[3, 3];
            Zoom3D[0, 0] = 1;
            Zoom3D[1, 0] = 0;
            Zoom3D[2, 0] = 0;
            Zoom3D[0, 1] = 0;
            Zoom3D[1, 1] = 1;
            Zoom3D[2, 1] = 0;
            Zoom3D[0, 2] = 0;
            Zoom3D[1, 2] = 0;
            Zoom3D[2, 2] = 1;
            // устанавливаем коэффициент масштабирования для необходимой (выбранной и переданной в качестве параметра) оси 
            Zoom3D[os, os] = coef;
            // вызываем функцию для выполнения умножения матриц, представляющих собой координаты вершин геометрического объекта 
            // на созданную в данной функции матрицу 
            multiply(GeomObject, Zoom3D);
        }

        // перенос 
        private void CreateTranslate(float translate, int os)
        {
            // в виду простоты данного алгоритма, мы упростили его обработку - достаточно прибавить изменение (перенос) в координатах объекта по выбранной и переданной оси 
            for (int ax = 0; ax < count_elements; ax++)
            {
                // обновление координат (для выбранной оси) 
                GeomObject[ax, os] += translate;
            }
        }

        // реализация поворота 
        private void CreateRotate(float angle, int os)
        {
            // массив, который будет содержать матрицу 
            float[,] Rotate3D = new float[3, 3]; // в зависимости от оси, матрицы будут кардинально различаться, 
            // поэтому создаем необходимую матрицу в зависимости от оси, используя 
            switch (os)
            {
                case 0: // вокруг оси Х 
                    {
                        Rotate3D[0, 0] = 1;
                        Rotate3D[1, 0] = 0;
                        Rotate3D[2, 0] = 0;
                        Rotate3D[0, 1] = 0;
                        Rotate3D[1, 1] = (float)Math.Cos(angle);
                        Rotate3D[2, 1] = (float)-Math.Sin(angle);
                        Rotate3D[0, 2] = 0;
                        Rotate3D[1, 2] = (float)Math.Sin(angle);
                        Rotate3D[2, 2] = (float)Math.Cos(angle);
                        break;
                    }
                case 1: // вокруг оси Y 
                    {
                        Rotate3D[0, 0] = (float)Math.Cos(angle);
                        Rotate3D[1, 0] = 0; Rotate3D[2, 0] = (float)Math.Sin(angle);
                        Rotate3D[0, 1] = 0; Rotate3D[1, 1] = 1; Rotate3D[2, 1] = 0;
                        Rotate3D[0, 2] = (float)-Math.Sin(angle); Rotate3D[1, 2] = 0;
                        Rotate3D[2, 2] = (float)Math.Cos(angle);
                        break;
                    }
                case 2: // вокруг оси Z 
                    {
                        Rotate3D[0, 0] = (float)Math.Cos(angle);
                        Rotate3D[1, 0] = (float)-Math.Sin(angle);
                        Rotate3D[2, 0] = 0;
                        Rotate3D[0, 1] = (float)Math.Sin(angle);
                        Rotate3D[1, 1] = (float)Math.Cos(angle);
                        Rotate3D[2, 1] = 0; Rotate3D[0, 2] = 0;
                        Rotate3D[1, 2] = 0; Rotate3D[2, 2] = 1;
                        break;
                    }
            } 
            // вызываем функцию для выполнения умножения матриц, представляющих собой координаты вершин геометрического объекта // на созданную в данной функции матрицу 
            multiply(GeomObject, Rotate3D);
        }
        // функция умножения матриц 
        private void multiply(float[,] obj, float[,] matrix)
        {
            // временные переменные 
            float res_1, res_2, res_3;
            // проходим циклом по всем координатам (представляющие собой матрицу A [x,y,z]) 
            // и умножаем каждую матрицу на матрицу B (переданную) // результат сразу заносим в массив геометрии 
            for (int ax = 0; ax < count_elements; ax++)
            {
                res_1 = (obj[ax, 0] * matrix[0, 0] + obj[ax, 1] * matrix[0, 1] + obj[ax, 2] * matrix[0, 2]);
                res_2 = (obj[ax, 0] * matrix[1, 0] + obj[ax, 1] * matrix[1, 1] + obj[ax, 2] * matrix[1, 2]);
                res_3 = (obj[ax, 0] * matrix[2, 0] + obj[ax, 1] * matrix[2, 1] + obj[ax, 2] * matrix[2, 2]);
                obj[ax, 0] = res_1; obj[ax, 1] = res_2; obj[ax, 2] = res_3;
            }
        }

        private void anT_KeyMouseDown()
        {
            // Нажатие средней кнопки мыши
            if (isMouseButtonDown(Keys.MButton))
            {
                l.Log("MButton");
                if (mousePos == MousePos.Right)
                {
                    CreateTranslate(-0.05f, 0);
                }
                if (mousePos == MousePos.Left)
                {
                    CreateTranslate(0.05f, 0);
                }
                if (mousePos == MousePos.Up)
                {
                    CreateTranslate(0.05f, 1);
                }
                if (mousePos == MousePos.Down)
                {
                    CreateTranslate(-0.05f, 1);
                }
            }
            // Нажатие левой кнопки мыши
            if (isMouseButtonDown(Keys.RButton))
            {
                l.Log("RButton");
                Console.WriteLine(Cursor.Position);
            }
        }

        private void anT_KeyDown(object sender, KeyEventArgs e)
        {
            // Z и X отвечают за масштабирование 
            if (e.KeyCode == Keys.Z)
            {
                // вызов функции, в которой мы реализуем масштабирование - передаем коэффициент масштабирования и выбранную ось в окне программы 
                CreateZoom(1.05f, comboBox1.SelectedIndex);
            }
            if (e.KeyCode == Keys.X)
            {
                // вызов функции, в которой мы реализуем масштабирование - передаем коэффициент масштабирования и выбранную ось в окне программы 
                CreateZoom(0.95f, comboBox1.SelectedIndex);
            } 

            // W и S отвечают за перенос 
            if (e.KeyCode == Keys.W)
            { 
                // вызов функции, в которой мы реализуем перенос - передаем значение перемещения и выбранную ось в окне программы 
                CreateTranslate(0.05f, comboBox1.SelectedIndex);
            }
            if (e.KeyCode == Keys.S)
            { 
                // вызов функции, в которой мы реализуем перенос - передаем значение перемещения и выбранную ось в окне программы 
                CreateTranslate(-0.05f, comboBox1.SelectedIndex);
            } 

            // A и D отвечают за поворот 
            if (e.KeyCode == Keys.A)
            { 
                // вызов функции, в которой мы реализуем поворот - передаем значение для поворота и выбранную ось 
                CreateRotate(0.05f, comboBox1.SelectedIndex);
            }
            if (e.KeyCode == Keys.D)
            {
                CreateRotate(-0.05f, comboBox1.SelectedIndex);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            anT.Focus();
        }

        enum MousePos
        {
            Up,
            Down,
            Right,
            Left
        }

        private Point p = new Point();
        private Point p1 = new Point();
        private void anT_MouseMove(object sender, MouseEventArgs e)
        {
            p = new Point(e.Location.X, e.Location.Y);
            if (p1.Y > p.Y)
            {
                mousePos = MousePos.Up;
                l.Log("Вверх");
            }
            if (p1.Y < p.Y)
            {
                mousePos = MousePos.Down;
                l.Log("Вниз");
            }
            if (p1.X > p.X)
            {
                mousePos = MousePos.Left;
                l.Log("Влево");
            }
            if (p1.X < p.X)
            {
                mousePos = MousePos.Right;
                l.Log("Вправо");
            }
            p1 = p;
        }
    }
}
