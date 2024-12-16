using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Color = System.Drawing.Color;


namespace OtkWpfControl
{
	public partial class MainWindow : Window
	{
		
		private readonly Scene mScene = new Scene();
		private PolygonMode _renderMode = PolygonMode.Fill; // По умолчанию сплошной режим
		
		public MainWindow()
		{
			InitializeComponent();
		}

		private void OtkWpfControl_Initialized( object sender , EventArgs e )
		{
		}


		private void OtkWpfControl_OpenGLDraw( object sender , OpenTK.WPF.OtkWpfControl.OpenGLDrawEventArgs e )
		{
			var ctrl = sender as OpenTK.WPF.OtkWpfControl;
			if ( ctrl != null )
			{
				e.Redrawn = mScene.Render( e.RenderingTime );
			}
		}
		
		private void CameraPositionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			// Получение текущих значений ползунков
			double x = CameraPosXSlider.Value;
			double y = CameraPosYSlider.Value;
			double z = CameraPosZSlider.Value;

			// Пример: Логика перемещения камеры
			MoveCameraToPosition(x, y, z);
		}

		private void MoveCameraToPosition(double angleY, double angleZ, double distance)
		{
			// Углы вращения в радианах
			double radiansY = Math.PI / 180 * angleY; // Вращение вокруг оси Y (горизонтальное вращение)
			double radiansZ = Math.PI / 180 * angleZ; // Вращение вокруг оси Z (вертикальное вращение)

			// Вычисляем новую позицию камеры в сферических координатах
			float x = (float)(distance * Math.Cos(radiansY) * Math.Cos(radiansZ));
			float y = (float)(angleZ);
			float z = (float)(distance * Math.Sin(radiansY) * Math.Cos(radiansZ));

			// Устанавливаем позицию камеры
			mScene.MoveCamera(new Vector3((float) x,(float) y,(float) z));

		}


		
		
		private void SetBackgroundButton_Click(object sender, RoutedEventArgs e)
		{
			// Устанавливаем цвет фона из ColorPicker
			var selectedColor = BgColorPicker.SelectedColor;
			if (selectedColor.HasValue)
			{
				mScene.SetBackgroundColor(selectedColor.Value.ToDrawingColor()); // Преобразуем в System.Drawing.Color
			}
		}

		private void DrawLineButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				// Parse line start and end coordinates
				float startX = float.Parse(StartXTextBox.Text, CultureInfo.InvariantCulture);
				float startY = float.Parse(StartYTextBox.Text, CultureInfo.InvariantCulture);
				float startZ = float.Parse(StartZTextBox.Text, CultureInfo.InvariantCulture);
				float endX = float.Parse(EndXTextBox.Text, CultureInfo.InvariantCulture);
				float endY = float.Parse(EndYTextBox.Text, CultureInfo.InvariantCulture);
				float endZ = float.Parse(EndZTextBox.Text, CultureInfo.InvariantCulture);
				float thickness = float.Parse(ThicknessTextBox.Text, CultureInfo.InvariantCulture);

				// Parse line color
				var lineColor = LineColorPicker.SelectedColor ?? Colors.Red;
				mScene.DrawLine(new Vector3(startX, startY, startZ), new Vector3(endX, endY, endZ), lineColor.ToDrawingColor(), thickness);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Invalid input: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		
		private void DrawRectangleButton_Click(object sender, RoutedEventArgs e)
		{
		    try
		    {
		        // Parse rectangle parameters
		        float x = float.Parse(RectXTextBox.Text, CultureInfo.InvariantCulture);
		        float y = float.Parse(RectYTextBox.Text, CultureInfo.InvariantCulture);
		        float z = float.Parse(RectZTextBox.Text, CultureInfo.InvariantCulture);
		        float width = float.Parse(RectWidthTextBox.Text, CultureInfo.InvariantCulture);
		        float height = float.Parse(RectHeightTextBox.Text, CultureInfo.InvariantCulture);

		        // Get the rectangle color
		        var color = RectColorPicker.SelectedColor ?? Colors.Red;

		        mScene.DrawRectangle(new Vector3(x, y, z), width, height, color.ToDrawingColor());
		    }
		    catch (Exception ex)
		    {
		        MessageBox.Show($"Invalid input: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
		    }
		}

		private void DrawTriangleButton_Click(object sender, RoutedEventArgs e)
		{
		    try
		    {
		        // Parse triangle vertices
		        float v1X = float.Parse(TriV1XTextBox.Text, CultureInfo.InvariantCulture);
		        float v1Y = float.Parse(TriV1YTextBox.Text, CultureInfo.InvariantCulture);
		        float v1Z = float.Parse(TriV1ZTextBox.Text, CultureInfo.InvariantCulture);
		        float v2X = float.Parse(TriV2XTextBox.Text, CultureInfo.InvariantCulture);
		        float v2Y = float.Parse(TriV2YTextBox.Text, CultureInfo.InvariantCulture);
		        float v2Z = float.Parse(TriV2ZTextBox.Text, CultureInfo.InvariantCulture);
		        float v3X = float.Parse(TriV3XTextBox.Text, CultureInfo.InvariantCulture);
		        float v3Y = float.Parse(TriV3YTextBox.Text, CultureInfo.InvariantCulture);
		        float v3Z = float.Parse(TriV3ZTextBox.Text, CultureInfo.InvariantCulture);

		        // Get the triangle color
		        var color = TriColorPicker.SelectedColor ?? Colors.Green;

		        mScene.DrawTriangle(
		            new Vector3(v1X, v1Y, v1Z),
		            new Vector3(v2X, v2Y, v2Z),
		            new Vector3(v3X, v3Y, v3Z),
		            color.ToDrawingColor());
		    }
		    catch (Exception ex)
		    {
		        MessageBox.Show($"Invalid input: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
		    }
		}

		private void DrawPolygonButton_Click(object sender, RoutedEventArgs e)
		{
		    try
		    {
		        // Parse polygon vertices
		        var verticesStr = PolygonVerticesTextBox.Text.Split(';');
		        var vertices = new List<Vector3>();

		        foreach (var vertexStr in verticesStr)
		        {
		            var coords = vertexStr.Split(',');
		            if (coords.Length == 3)
		            {
		                vertices.Add(new Vector3(
		                    float.Parse(coords[0], CultureInfo.InvariantCulture),
		                    float.Parse(coords[1], CultureInfo.InvariantCulture),
		                    float.Parse(coords[2], CultureInfo.InvariantCulture)));
		            }
		        }

		        // Get the polygon color
		        var color = PolygonColorPicker.SelectedColor ?? Colors.Blue;

		        mScene.DrawPolygon(vertices, color.ToDrawingColor());
		    }
		    catch (Exception ex)
		    {
		        MessageBox.Show($"Invalid input: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
		    }
		}
		
		private void DrawConeButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				// Получение данных из полей ввода
				float apexX = float.Parse(ConeApexXTextBox.Text, CultureInfo.InvariantCulture);
				float apexY = float.Parse(ConeApexYTextBox.Text, CultureInfo.InvariantCulture);
				float apexZ = float.Parse(ConeApexZTextBox.Text, CultureInfo.InvariantCulture);
				float baseRadius = float.Parse(BaseRadiusTextBox.Text, CultureInfo.InvariantCulture);
				float height = float.Parse(ConeHeightTextBox.Text, CultureInfo.InvariantCulture);
				int segments = int.Parse(ConeSegmentsTextBox.Text, CultureInfo.InvariantCulture);
				
				

				// Получение цвета из ColorPicker
				var selectedColor = ConeColorPicker.SelectedColor;
				Color color = selectedColor.HasValue
					? System.Drawing.Color.FromArgb(selectedColor.Value.A, selectedColor.Value.R, selectedColor.Value.G, selectedColor.Value.B)
					: Color.Blue;

				// Вызов метода DrawCone
				mScene.DrawCone(new Vector3(apexX, apexY, apexZ), baseRadius, height, segments, color, _renderMode);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		
		
		// Обработчик для изменения выбранного режима отрисовки
		private void RenderModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			// Получаем выбранный элемент ComboBox
			if (RenderModeComboBox.SelectedItem is ComboBoxItem selectedItem)
			{
				// Извлекаем строку, которая является текстом выбранного элемента
				string selectedMode = selectedItem.Content.ToString();


				// Для отладки можно вывести выбранный режим в консоль
				Console.WriteLine(selectedMode);

				switch (selectedMode)
				{
					case "Fill":
						SetRenderMode(PolygonMode.Fill);
						break;
					case "Line":
						SetRenderMode(PolygonMode.Line);
						break;
					case "Point":
						SetRenderMode(PolygonMode.Point);
						break;
				}
			}
		}

		
		

		public void SetRenderMode(PolygonMode mode)
		{
			_renderMode = mode;
		}



	}
}
