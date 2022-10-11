using System.Drawing;
using System.Windows.Forms;
using Tao.FreeGlut;
using Tao.OpenGl;
using Tao.Platform;

namespace EasyGameEngine
{
    class render
    {
        /// <summary>
        /// Инициализация 3d окна
        /// </summary>
        /// <param name="anT">3d окно</param>
        public static void initializationGraph(Tao.Platform.Windows.SimpleOpenGlControl anT)
        {
            Glut.glutInit(); // инициализацию Glut
            Glut.glutInitDisplayMode(Glut.GLUT_RGB | Glut.GLUT_DOUBLE | Glut.GLUT_DEPTH); // Режим отображения
            Gl.glClearColor(255, 255, 255, 1); // Очистка окна
            Gl.glViewport(0, 0, anT.Width, anT.Height); // Порт для вывода изображения по всей области anT
            Gl.glMatrixMode(Gl.GL_PROJECTION); // Матричный режим
            Gl.glLoadIdentity(); // Очиска матрицы
            Glu.gluPerspective(45, (float)anT.Width / (float)anT.Height, 0.1, 200); // Пирамида видимости визуального охвата
            Gl.glMatrixMode(Gl.GL_MODELVIEW); // Объектно-видовая матрица
            Gl.glLoadIdentity(); // Очистка матрицы
            Gl.glEnable(Gl.GL_DEPTH_TEST); // Тест глубины
            Gl.glEnable(Gl.GL_COLOR_MATERIAL); // Тест цвета
        }
    }
}
