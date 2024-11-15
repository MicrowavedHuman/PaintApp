using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Ink;
using System.Drawing.Drawing2D;
using System.IO;
using Microsoft.Win32;


namespace PaintApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Color color = Colors.Red;
        int size = 10;
        public static bool penMode = true;
        public static bool eraserMode = false;
        public static bool eraseAllMode = false;
        public static bool whiteBackgroundAdded = false;

        public MainWindow()
        {
            InitializeComponent();
            drawingArea1.DefaultDrawingAttributes.Color = color;
            whiteBackground.Visibility = Visibility.Hidden;
            UpdateUI();
        }
        private void red_Click(object sender, RoutedEventArgs e)
        {
            UpdateColor(Colors.Red);
        }

        private void blue_Click(object sender, RoutedEventArgs e)
        {
            UpdateColor(Colors.Blue);
        }

        private void Green_Click(object sender, RoutedEventArgs e)
        {
            UpdateColor(Colors.DarkGreen);
        }

        private void sizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            size = Convert.ToInt32(sizeSlider.Value);
            int newSize = Convert.ToInt32(sizeSlider.Value);
            size = newSize;
            drawingArea1.DefaultDrawingAttributes.Width = newSize;
            drawingArea1.DefaultDrawingAttributes.Height = newSize;
            if (sizeLabel != null)
            {
                sizeLabel.Content = string.Format("{0:F0}", sizeSlider.Value);
            }
            else
            {
                Console.WriteLine("sizeLabel is null");
            }
        }

        private void UpdateColor(Color newColor)
        {
            color = newColor;
            drawingArea1.DefaultDrawingAttributes.Color = color;
        }
        private void colorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            UpdateColor(colorPicker.SelectedColor ?? Colors.Black);
        }

        private void PenButton_Click(object sender, RoutedEventArgs e)
        {
            eraserMode = false;
            penMode = true;
            UpdateUI();
        }

        private void EraserButton_Click(object sender, RoutedEventArgs e)
        {
            eraserMode = true;
            penMode = false;
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (penMode)
            {
                drawingArea1.EditingMode = InkCanvasEditingMode.Ink;
            }
            else if (eraserMode && !eraseAllMode)
            {
                drawingArea1.EditingMode = InkCanvasEditingMode.EraseByPoint;
                drawingArea1.EraserShape = new EllipseStylusShape(size, size);
            }
            else if (eraserMode && eraseAllMode)
            {
                drawingArea1.EditingMode = InkCanvasEditingMode.EraseByStroke;
                drawingArea1.EraserShape = new EllipseStylusShape(size, size);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!eraseAllMode)
            {
                eraseAllMode = true;
            } else if (eraseAllMode)
            {
                eraseAllMode = false;
            }
            UpdateUI();
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PNG Files (*.png)|*.png";
            saveFileDialog.FileName = "Drawing";

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Create a temporary InkCanvas to hold the strokes
                    var tempCanvas = new InkCanvas();
                    tempCanvas.Strokes = drawingArea1.Strokes;

                    // Get the bounds of the strokes
                    Rect rect = tempCanvas.Strokes.GetBounds();

                    // Create a RenderTargetBitmap
                    RenderTargetBitmap rtb = new RenderTargetBitmap(
                        (int)Math.Max(rect.Right, whiteBackground.ActualWidth),
                        (int)Math.Max(rect.Bottom, whiteBackground.ActualHeight),
                        96d,
                        96d,
                        PixelFormats.Default);

                    // Create a DrawingVisual to combine the white background and ink strokes
                    var visual = new DrawingVisual();
                    using (var dc = visual.RenderOpen())
                    {
                        // Draw the white background if it's added
                        if (whiteBackgroundAdded)
                        {
                            dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, rtb.PixelWidth, rtb.PixelHeight));
                        }

                        // Draw the ink strokes
                        foreach (Stroke stroke in tempCanvas.Strokes)
                        {
                            stroke.Draw(dc);
                        }
                    }

                    // Render the combined visual to the RenderTargetBitmap
                    rtb.Render(visual);

                    // Encode the RenderTargetBitmap as a PNG
                    PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
                    pngEncoder.Frames.Add(BitmapFrame.Create(rtb));

                    // Save the encoded image to the selected file
                    using (var fileStream = File.Create(saveFileDialog.FileName))
                    {
                        pngEncoder.Save(fileStream);
                    }

                    MessageBox.Show("File saved successfully.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving file: {ex.Message}");
                }
            }
        }

        private void addWhiteBackground_Click(object sender, RoutedEventArgs e)
        {
            whiteBackgroundAdded = !whiteBackgroundAdded;
            whiteBackground.Visibility = whiteBackgroundAdded ? Visibility.Visible : Visibility.Hidden;
            addWhiteBackground.Content = whiteBackgroundAdded ? "Remove White Background" : "Add White Background";
        }
    }
}