using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace OtkWpfControl
{
    public class Scene
    {
        private Size size = new Size(1920, 1080);
        private Color backgroundColor = Color.Black;
        private Vector3 cameraPosition = new Vector3(0f, 100f, -200f); // Камера на 10 единиц отдалена по оси Z

        
        

        // Список для хранения объектов, которые нужно отрисовать
        private List<DrawableLine> lines = new List<DrawableLine>();
        private List<DrawableShape> shapes = new List<DrawableShape>(); // Список для хранения фигур

        // Класс для хранения данных о линии
        public class DrawableLine
        {
            public Vector3 Start { get; set; }
            public Vector3 End { get; set; }
            public Color Color { get; set; }
            public float LineWidth { get; set; }

            public DrawableLine(Vector3 start, Vector3 end, Color color, float lineWidth = 1f)
            {
                Start = start;
                End = end;
                Color = color;
                LineWidth = lineWidth;
            }
        }

        // Класс для хранения данных о фигурах
        public class DrawableShape
        {
            public List<Vector3> Vertices { get; set; } // Список вершин
            public Color Color { get; set; }
            public PolygonMode RenderMode { get; set; } = PolygonMode.Fill; // По умолчанию сплошной режим


            public DrawableShape(List<Vector3> vertices, Color color, PolygonMode polygonMode = PolygonMode.Fill)
            {
                Vertices = vertices;
                Color = color;
                RenderMode = polygonMode;
            }
        }

        public void SetBackgroundColor(Color color)
        {
            backgroundColor = color;
        }

        public void DrawLine(Vector3 start, Vector3 end, Color color, float lineWidth = 1f)
        {
            lines.Add(new DrawableLine(start, end, color, lineWidth));
        }

        // Метод для рисования прямоугольника
        public void DrawRectangle(Vector3 topLeft, float width, float height, Color color)
        {
            // Задаем 4 вершины прямоугольника
            List<Vector3> vertices = new List<Vector3>
            {
                topLeft,
                new Vector3(topLeft.X + width, topLeft.Y, topLeft.Z),
                new Vector3(topLeft.X + width, topLeft.Y - height, topLeft.Z),
                new Vector3(topLeft.X, topLeft.Y - height, topLeft.Z)
            };

            shapes.Add(new DrawableShape(vertices, color));
        }

        // Метод для рисования треугольника
        public void DrawTriangle(Vector3 v1, Vector3 v2, Vector3 v3, Color color)
        {
            List<Vector3> vertices = new List<Vector3> { v1, v2, v3 };
            shapes.Add(new DrawableShape(vertices, color));
        }

        // Метод для рисования многоугольника
        public void DrawPolygon(List<Vector3> vertices, Color color)
        {
            if (vertices.Count >= 3)
            {
                shapes.Add(new DrawableShape(vertices, color));
            }
        }
        
        public void DrawCone(Vector3 apex, float baseRadius, float height, int segments, Color color, PolygonMode polygonMode = PolygonMode.Fill)
        {
            if (segments < 3) throw new ArgumentException("Cone must have at least 3 segments.");

            // Вершина конуса
            Vector3 coneApex = apex;

            // Центр основания конуса
            Vector3 baseCenter = new Vector3(apex.X, apex.Y - height, apex.Z);

            // Список вершин основания
            List<Vector3> baseVertices = new List<Vector3>();

            // Расчет точек на окружности основания
            for (int i = 0; i < segments; i++)
            {
                float angle = MathHelper.TwoPi * i / segments;
                float x = baseRadius * (float)Math.Cos(angle) + baseCenter.X;
                float z = baseRadius * (float)Math.Sin(angle) + baseCenter.Z;
                baseVertices.Add(new Vector3(x, baseCenter.Y, z));
            }

            // Добавляем основание как многоугольник
            shapes.Add(new DrawableShape(baseVertices, color, polygonMode));

            // Добавляем треугольники от вершины конуса к граням основания
            foreach (var vertex in baseVertices)
            {
                shapes.Add(new DrawableShape(new List<Vector3> { coneApex, vertex, baseCenter }, color, polygonMode));
            }
        }


        public bool Render(TimeSpan time)
        {
            GL.Enable(EnableCap.DepthTest);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            GL.ClearColor(backgroundColor);
            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);

            SetProjection(size);
            SetCamera();


            // Отрисовываем все линии
            foreach (var line in lines)
            {
                GL.LineWidth(line.LineWidth);
                GL.Begin(PrimitiveType.Lines);
                GL.Color4(line.Color.R / 255f, line.Color.G / 255f, line.Color.B / 255f, line.Color.A / 255f);
                GL.Vertex3(line.Start);
                GL.Vertex3(line.End);
                GL.End();
            }

            // Отрисовываем все фигуры
            foreach (var shape in shapes)
            {
                GL.PolygonMode(MaterialFace.FrontAndBack, shape.RenderMode);
                GL.Begin(PrimitiveType.Polygon); // Используем PrimitiveType.Polygon для многоугольников
                GL.Color4(shape.Color.R / 255f, shape.Color.G / 255f, shape.Color.B / 255f, shape.Color.A / 255f);

                foreach (var vertex in shape.Vertices)
                {
                    GL.Vertex3(vertex);
                }

                GL.End();
            }

            return true; // Продолжаем анимацию
        }

        public void SetSize(float scaleFactor)
        {
            size = new Size((int) (1920 / scaleFactor), (int) (1080 / scaleFactor));
        }
        
        public void MoveCamera(Vector3 position)
        {
            cameraPosition = position;
        }

        private void SetCamera()
        {
            GL.MatrixMode(MatrixMode.Modelview); // Переходим в режим Modelview
            GL.LoadIdentity(); // Сбрасываем матрицу

            // Позиция камеры и точка, на которую она смотрит
            Vector3 target = Vector3.Zero; // Центр сцены
            Vector3 forward = Vector3.Normalize(target - cameraPosition); // Вектор направления камеры

            // Вектор "вправо" (правило правой руки)
            Vector3 right = Vector3.Normalize(Vector3.Cross(Vector3.UnitY, forward));

            // Новый вектор "вверх"
            Vector3 up = Vector3.Cross(forward, right);

            // Создаем матрицу вида (View Matrix)
            Matrix4 viewMatrix = Matrix4.LookAt(
                cameraPosition, // Позиция камеры
                target,         // Точка, на которую камера смотрит
                up              // Динамически вычисленный "вверх"
            );

            GL.LoadMatrix(ref viewMatrix);
        }



        private static void SetProjection(Size size)
        {
            GL.MatrixMode(MatrixMode.Projection); // Устанавливаем режим работы с проекционной матрицей
            GL.LoadIdentity(); // Сбрасываем текущую матрицу

            // Параметры перспективы
            float fieldOfView = 120f; // Угол обзора в градусах (широкий угол = больше сцены видно)
            float aspectRatio = (float)size.Width / size.Height; // Соотношение сторон экрана
            float nearClip = 0.1f; // Ближняя плоскость отсечения (ближе этой точки объекты не видны)
            float farClip = 1000f; // Дальняя плоскость отсечения (дальше этой точки объекты не видны)

            // Создаем матрицу перспективной проекции
            Matrix4 perspective = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(fieldOfView), // Угол обзора в радианах
                aspectRatio, // Соотношение ширины к высоте
                nearClip, // Ближняя плоскость отсечения
                farClip // Дальняя плоскость отсечения
            );

            // Загружаем матрицу в OpenGL
            GL.LoadMatrix(ref perspective);
        }
        
    }
}
