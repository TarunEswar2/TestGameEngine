using GameEngineEditor.ContentToolsAPIStructs;
using GameEngineEditor.DllWrapper;
using GameEngineEditor.Editors;
using GameEngineEditor.utilities.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GameEngineEditor.Content
{
    /// <summary>
    /// Interaction logic for PrimitiveMeshDialog.xaml
    /// </summary>
    public partial class PrimitiveMeshDialog : Window
    {
        public PrimitiveMeshDialog()
        {
            InitializeComponent();
            Loaded += (s,e) => UpdatePrimitve();
        }
        static PrimitiveMeshDialog()
        {
            LoadTextures();
        }
        private static void LoadTextures()
        {
            var uris = new List<Uri>
            {
                new Uri("pack://application:,,,/Resources/PrimitiveMeshView/planeTexture.png"),
                new Uri("pack://application:,,,/Resources/PrimitiveMeshView/planeTexture.png"),
                new Uri("pack://application:,,,/Resources/PrimitiveMeshView/planeTexture.png"),
                new Uri("pack://application:,,,/Resources/PrimitiveMeshView/planeTexture.png"),
                new Uri("pack://application:,,,/Resources/PrimitiveMeshView/planeTexture.png"),
            };
            _textures.Clear();

            foreach(var uri in uris)
            {
                var resource = Application.GetResourceStream(uri);
                using var reader = new BinaryReader(resource.Stream);
                var data = reader.ReadBytes((int)resource.Stream.Length);
                var imageSource =(BitmapSource)new ImageSourceConverter().ConvertFrom(data);
                imageSource.Freeze();
                var brush = new ImageBrush(imageSource);
                brush.Transform = new ScaleTransform(1, -1, 0.5, 0.5);
                brush.ViewportUnits = BrushMappingMode.Absolute;
                brush.Freeze();
                _textures.Add(brush);

            }
        }

        private void UpdatePrimitve()
        {
            Debug.Assert(IsInitialized != false);
            if (!IsInitialized) return;

            var primitiveType = (PrimitiveMeshType)primTypeComboBox.SelectedItem;
            var info = new PrimitiveInitInfo { Type = primitiveType };

            switch(primitiveType)
            {
                case PrimitiveMeshType.Plane:
                    {
                        info.SegmentX = (int)xSliderPlane.Value;
                        info.SegmentZ = (int)ySliderPlane.Value;
                        info.Size.X = Value(widthScalarBoxPlane,0.001f);
                        info.Size.Z = Value(lengthScalarBoxPlane,0.001f);
                    }
                    break;
                case PrimitiveMeshType.Cube:
                    return;
                case PrimitiveMeshType.UvSphere:
                    {
                        info.SegmentX = (int)(xSliderUVSphere.Value);
                        info.SegmentY = (int)(ySliderUVSphere.Value);
                        info.Size.X = Value(xScalarBoxUvSphere, 0.001f);
                        info.Size.Y = Value(yScalarBoxUvSphere, 0.001f);
                        info.Size.Z = Value(zScalarBoxUvSphere, 0.001f);
                    }
                    break;
                case PrimitiveMeshType.IcoSphere:
                    return;
                case PrimitiveMeshType.Cylinder:
                    return;
                case PrimitiveMeshType.Capsule:
                    return;
                default: 
                    break;
            }

            var geometry = new Geometry();
            ContentToolsAPI.CreatePrimitiveMesh(geometry, info);
            (DataContext as GeometryEditor).SetAsset(geometry);
            onTexture_CheckBox_Click(textureCheckBox, null);
        }

        private static readonly List<ImageBrush> _textures = new List<ImageBrush>();
        private void OnPrimitiveType_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdatePrimitve();

        private void onSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => UpdatePrimitve();

        private void OnScalarBox_ValueChanged(object sender, RoutedEventArgs e) => UpdatePrimitve();
        private float Value(ScalarBox scalarBox, float min)
        {
            float.TryParse(scalarBox.Value, out var result);
            return Math.Max(result, min);
        }

        private void onTexture_CheckBox_Click(object sender, RoutedEventArgs e)
        {
            Brush brush = Brushes.White;

            if((sender as CheckBox).IsChecked == true)
            {
                brush = _textures[(int)primTypeComboBox.SelectedIndex];
            }

            var vm = DataContext as GeometryEditor;
            foreach(var mesh in vm.MeshRenderer.Meshes)
            {
                mesh.Diffuse = brush;
            }
        }
    }
}
