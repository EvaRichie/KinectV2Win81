using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using WindowsPreview.Kinect;

namespace KinectApp1.ViewModel
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private KinectSensor SensorInstance
        {
            get { return KinectSensor.GetDefault(); }
        }

        private MultiSourceFrameReader _MultiSrcReader;
        public MultiSourceFrameReader MultiSrcReader
        {
            get { return _MultiSrcReader; }
            set
            {
                if (_MultiSrcReader != value)
                {
                    _MultiSrcReader = value;
                    NotifyChanged();
                }
            }
        }

        #region IR Sensor properties
        private InfraredFrameReader _IR_Reader;
        public InfraredFrameReader IR_Reader
        {
            get { return _IR_Reader; }
            set
            {
                if (_IR_Reader != value)
                {
                    _IR_Reader = value;
                    NotifyChanged();
                }
            }
        }

        private ushort[] _IR_DataArray;
        public ushort[] IR_DataArray
        {
            get { return _IR_DataArray; }
            set
            {
                if (_IR_DataArray != value)
                {
                    _IR_DataArray = value;
                    NotifyChanged();
                }
            }
        }

        private byte[] _IR_ConvertedData;
        public byte[] IR_ConvertedData
        {
            get { return _IR_ConvertedData; }
            set
            {
                if (_IR_ConvertedData != value)
                {
                    _IR_ConvertedData = value;
                    NotifyChanged();
                }
            }
        }

        private WriteableBitmap _IRwBmp;
        public WriteableBitmap IRwBmp
        {
            get { return _IRwBmp; }
            set
            {
                if (_IRwBmp != value)
                {
                    _IRwBmp = value;
                    NotifyChanged();
                }
            }
        }
        #endregion

        #region Color Sensor properties
        private ColorFrameReader _CfReader;
        public ColorFrameReader CfReader
        {
            get { return _CfReader; }
            set
            {
                if (_CfReader != value)
                {
                    _CfReader = value;
                    NotifyChanged();
                }
            }
        }

        private byte[] _Color_DataArray;
        public byte[] Color_DataArray
        {
            get { return _Color_DataArray; }
            set
            {
                if (_Color_DataArray != value)
                {
                    _Color_DataArray = value;
                    NotifyChanged();
                }
            }
        }

        private WriteableBitmap _CwBmp;
        public WriteableBitmap CwBmp
        {
            get { return _CwBmp; }
            set
            {
                if (_CwBmp != value)
                {
                    _CwBmp = value;
                    NotifyChanged();
                }
            }
        }
        #endregion

        #region Body Scan properties
        private Body[] _Body_DataArray;
        public Body[] Body_DataArray
        {
            get { return _Body_DataArray; }
            set
            {
                if (_Body_DataArray != value)
                {
                    _Body_DataArray = value;
                    NotifyChanged();
                }
            }
        }
        #endregion

        public MainPageViewModel()
        {
            //OpenIRSensor();
            //OpenColorSensor();
            ReadBodyData();
        }

        #region IR Sensor Method
        public void OpenIRSensor()
        {
            var fd = SensorInstance.InfraredFrameSource.FrameDescription;
            IR_Reader = SensorInstance.InfraredFrameSource.OpenReader();
            IR_DataArray = new ushort[fd.LengthInPixels];
            IR_ConvertedData = new byte[fd.LengthInPixels * 4];
            IRwBmp = new WriteableBitmap(fd.Width, fd.Height);
            SensorInstance.Open();
            IR_Reader.FrameArrived += IR_Reader_FrameArrived;
        }

        private void IR_Reader_FrameArrived(InfraredFrameReader sender, InfraredFrameArrivedEventArgs args)
        {
            using (var irFrame = args.FrameReference.AcquireFrame())
            {
                if (irFrame != null)
                {
                    irFrame.CopyFrameDataToArray(IR_DataArray);
                    for (var i = 0; i < IR_DataArray.Length; i++)
                    {
                        var intensity = (byte)(IR_DataArray[i] >> 8);
                        IR_ConvertedData[i * 4] = intensity;
                        IR_ConvertedData[i * 4 + 1] = intensity;
                        IR_ConvertedData[i * 4 + 2] = intensity;
                        IR_ConvertedData[i * 4 + 3] = 255;
                    }
                    IR_ConvertedData.CopyTo(IRwBmp.PixelBuffer);
                    IRwBmp.Invalidate();
                }
            }
        }

        public void ReleaseIRSensor()
        {
            IR_Reader.FrameArrived -= IR_Reader_FrameArrived;
            IR_Reader.Dispose();
            IR_Reader = null;
            IRwBmp = null;
            IR_DataArray = null;
            IR_ConvertedData = null;
        }
        #endregion

        #region Color Sensor Method
        public void OpenColorSensor()
        {
            var fd = SensorInstance.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Rgba);
            CfReader = SensorInstance.ColorFrameSource.OpenReader();
            Color_DataArray = new byte[fd.Height * fd.Width * fd.BytesPerPixel];
            CwBmp = new WriteableBitmap(fd.Width, fd.Height);
            SensorInstance.Open();
            CfReader.FrameArrived += CfReader_FrameArrived;
        }

        private void CfReader_FrameArrived(ColorFrameReader sender, ColorFrameArrivedEventArgs args)
        {
            using (var cFrame = args.FrameReference.AcquireFrame())
            {
                if (cFrame != null)
                {
                    if (cFrame.RawColorImageFormat == ColorImageFormat.Rgba)
                        cFrame.CopyRawFrameDataToArray(Color_DataArray);
                    else
                        cFrame.CopyConvertedFrameDataToArray(Color_DataArray, ColorImageFormat.Bgra);
                    Color_DataArray.CopyTo(CwBmp.PixelBuffer);
                    CwBmp.Invalidate();
                }
            }
        }

        public void ReleaseColorSensor()
        {
            CfReader.FrameArrived -= CfReader_FrameArrived;
            CfReader.Dispose();
            CfReader = null;
            CwBmp = null;
            Color_DataArray = null;
        }
        #endregion

        public void ReadBodyData()
        {
            var fd = SensorInstance.InfraredFrameSource.FrameDescription;
            IR_Reader = SensorInstance.InfraredFrameSource.OpenReader();
            IR_DataArray = new ushort[fd.LengthInPixels];
            IR_ConvertedData = new byte[fd.LengthInPixels * 4];
            IRwBmp = new WriteableBitmap(fd.Width, fd.Height);
            Body_DataArray = new Body[6];
            MultiSrcReader = SensorInstance.OpenMultiSourceFrameReader(FrameSourceTypes.Body | FrameSourceTypes.Infrared);
            MultiSrcReader.MultiSourceFrameArrived += MultiSrcReader_MultiSourceFrameArrived;
            SensorInstance.Open();
        }

        private void MultiSrcReader_MultiSourceFrameArrived(MultiSourceFrameReader sender, MultiSourceFrameArrivedEventArgs args)
        {
            var mainPage = (Windows.UI.Xaml.Window.Current.Content as Windows.UI.Xaml.Controls.Frame).Content as MainPage;
            var bodyCanvas = (mainPage.Content as Windows.UI.Xaml.Controls.Grid).FindName("BodyCanvas") as Windows.UI.Xaml.Controls.Canvas;
            using (var multiFrame = args.FrameReference.AcquireFrame())
            {
                if (multiFrame != null)
                {
                    using (var bodyFrame = multiFrame.BodyFrameReference.AcquireFrame())
                    using (var irFrame = multiFrame.InfraredFrameReference.AcquireFrame())
                    {
                        if (bodyFrame != null && irFrame != null)
                        {
                            irFrame.CopyFrameDataToArray(IR_DataArray);
                            for (var i = 0; i < IR_DataArray.Length; i++)
                            {
                                var intensity = (byte)(IR_DataArray[i] >> 8);
                                IR_ConvertedData[i * 4] = intensity;
                                IR_ConvertedData[i * 4 + 1] = intensity;
                                IR_ConvertedData[i * 4 + 2] = intensity;
                                IR_ConvertedData[i * 4 + 3] = 255;
                            }
                            IR_ConvertedData.CopyTo(IRwBmp.PixelBuffer);
                            IRwBmp.Invalidate();

                            bodyFrame.GetAndRefreshBodyData(Body_DataArray);
                            foreach (var body in Body_DataArray)
                            {
                                if (body.IsTracked)
                                {
                                    //var jointTypes = Enum.GetValues(typeof(JointType));
                                    var headJoint = body.Joints[JointType.Head];
                                    if (headJoint.TrackingState == TrackingState.Tracked)
                                    {
                                        bodyCanvas.Children.Clear();
                                        var dsp = SensorInstance.CoordinateMapper.MapCameraPointToDepthSpace(headJoint.Position);
                                        var ellipse = new Windows.UI.Xaml.Shapes.Ellipse() { Width = 50, Height = 50, Fill = new Windows.UI.Xaml.Media.SolidColorBrush(Windows.UI.Colors.Red) };
                                        bodyCanvas.Children.Add(ellipse);
                                        Windows.UI.Xaml.Controls.Canvas.SetTop(ellipse, dsp.Y - 25);
                                        Windows.UI.Xaml.Controls.Canvas.SetLeft(ellipse, dsp.X - 25);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void ReleaseKinectSensor()
        {
            if (SensorInstance != null)
                SensorInstance.Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyChanged([CallerMemberName]string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
