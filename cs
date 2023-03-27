using DJI.WindowsSDK;
using DJIUWPSample.Commands;
using DJIUWPSample.ViewModels;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Popups;
using DateTime = System.DateTime;

namespace DJIWindowsSDKSample.ViewModels
{
    class ComponentViewModel : ViewModelBase
    {
        public String AircraftSetName { set; get; }
        String _aircraftGetName = "";
        public String AircraftGetName
        {
            get
            {
                return _aircraftGetName;
            }
            set
            {
                _aircraftGetName = value;
                OnPropertyChanged("AircraftGetName");
            }
        }
        Velocity3D _aircraftVelocity3D;
        public Velocity3D AircraftVelocity
        {
            get
            {
                return _aircraftVelocity3D;
            }
            set
            {
                _aircraftVelocity3D = value;
                guardarDatos(value);
                //prueba();
                //
                OnPropertyChanged("AircraftVelocityXString");
                OnPropertyChanged("AircraftVelocityYString");
                OnPropertyChanged("AircraftVelocityZString");

            }
        }
        public String AircraftVelocityXString
        {
            get {

                Console.WriteLine(_aircraftVelocity3D.x.ToString() + " m/s");
                return _aircraftVelocity3D.x.ToString() + " m/s"; }
        }
        public String AircraftVelocityYString
        {
            get { 
                return _aircraftVelocity3D.y.ToString() + " m/s"; }
        }
             
        public String AircraftVelocityZString
        {
            get {return _aircraftVelocity3D.z.ToString() + " m/s";}
        }

        public ICommand _registerVelocityChangedObserver;
        public ICommand RegisterVelocityChangedObserver
        {
            get
            {
                if (_registerVelocityChangedObserver == null)
                {
                    _registerVelocityChangedObserver = new RelayCommand(delegate ()
                    {
                        watch = System.Diagnostics.Stopwatch.StartNew();
                        watch.Start();
                        uint index = 0;
                        uint comp_index = 0;
                        //Velocidad
                        //DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(index, comp_index).VelocityChanged += ComponentHandingPage_VelocityChanged;
                           
                        //Posicion
                        //DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(index, comp_index).AircraftLocationChanged += ComponentHandingPage_LocationChanged;
                        DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(index, comp_index).AltitudeChanged += ComponentHandingPage_AltitudeChanged;

                        //Rotaciones
                        //DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(index, comp_index).AttitudeChanged += ComponentHandingPage_AttitudeChanged;
                        tiempo_previo = DateTime.Now;


                        //Joystick izquierdo
                        DJISDKManager.Instance.ComponentManager.GetRemoteControllerHandler(index, comp_index).RCStickLeftVerticalChanged += ComponentHandingPage_RCStickLeftVerticalChanged;
                        //guardarDatos(AircraftVelocity);
                    }, delegate () { return true; });
                }
                return _registerVelocityChangedObserver;
            }
        }

        int valor_izquierdo = 0;
        bool se_tiene_joy_izquierdo;
        private void ComponentHandingPage_RCStickLeftVerticalChanged(object sender, IntMsg? value)
        {
            valor_izquierdo = value.Value.value;
            se_tiene_joy_izquierdo = true;
        }

        DateTime tiempo_previo;
        bool se_tiene_altura = false;
        private async void ComponentHandingPage_AttitudeChanged(object sender, Attitude? value)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                AircraftVelocity = new Velocity3D
                {
                    x = value.Value.yaw,
                    y = value.Value.pitch,
                    z = value.Value.roll
                };
                //guardarDatos(AircraftVelocity);
            });
        }

        private async void ComponentHandingPage_AltitudeChanged(object sender, DoubleMsg? value)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                AircraftVelocity = new Velocity3D
                {
                    x = AircraftVelocity.x,
                    y = AircraftVelocity.y,
                    z = AircraftVelocity.z + value.Value.value
                };
                se_tiene_altura = true;



            });
        }

        private async void ComponentHandingPage_LocationChanged(object sender, LocationCoordinate2D? value)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                AircraftVelocity = new Velocity3D{
                    x = AircraftVelocity.x + value.Value.latitude,
                    y = AircraftVelocity.y + value.Value.longitude,
                    z = AircraftVelocity.z
                };
            });
        }

        private async void ComponentHandingPage_VelocityChanged(object sender, Velocity3D? value){

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                AircraftVelocity = value.Value;
                //guardarDatos(AircraftVelocity);
            });
        }
        public ICommand _setAircraftName;
        public ICommand SetAircraftName
        {
            get
            {
                if (_setAircraftName == null)
                {
                    _setAircraftName = new RelayCommand(async delegate()
                    {
                        String message = "";
                        do
                        {
                            var toSet = AircraftSetName;
                            if (toSet==null || toSet.Length == 0)
                            {
                                message = "Input your name first!";
                                break;
                            }
                            var res = await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).SetAircraftNameAsync(new StringMsg { value = toSet });
                            message = String.Format("Set aircraft's name: {0}", res.ToString());
                        } while (false);
                        var messageDialog = new MessageDialog(message);
                        await messageDialog.ShowAsync();
                    }, delegate () { return true; });
                }
                return _setAircraftName;
            }
        }
        public ICommand _getAircraftName;
        public ICommand GetAircraftName
        {
            get
            {
                if (_getAircraftName == null)
                {
                    _getAircraftName = new RelayCommand(async delegate ()
                    {
                        String message = "";
                        do
                        {
                            var res = await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).GetAircraftNameAsync();
                            if (res.error != SDKError.NO_ERROR)
                            {
                                message = String.Format("Get aircraft's name: {0}", res.error.ToString());
                                break;
                            }
                            if (res.value != null)
                            {
                                AircraftGetName = res.value.Value.value;
                            }
                            message = String.Format("Get aircraft's name Success.");
                        } while (false);

                        var messageDialog = new MessageDialog(message);
                        await messageDialog.ShowAsync();
                    }, delegate () { return true; });
                }
                return _getAircraftName;
            }
        }
        public ICommand _startTakeoff;
        public ICommand StartTakeoff
        {
            get
            {
                if (_startTakeoff == null)
                {
                    _startTakeoff = new RelayCommand(async delegate ()
                    {
                        var res = await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).StartTakeoffAsync();
                        var messageDialog = new MessageDialog(String.Format("Start send takeoff command: {0}", res.ToString()));
                        await messageDialog.ShowAsync();
                    }, delegate () { return true; });
                }
                return _startTakeoff;
            }
        }

        public void prueba()
        {
            for (int i = 0; i<10; i++)
            {
                                System.Diagnostics.Debug.WriteLine("fdsfghdfkjghdkf ghf");
            }
            
        }

        System.Diagnostics.Stopwatch watch;
        public async void guardarDatos(Velocity3D? value){
            
            DateTime ahora = DateTime.Now;
            TimeSpan ts = ahora - tiempo_previo;
            /*
            String line;
            try
            {
                //Pass the file path and file name to the StreamReader constructor
                StreamReader sr = new StreamReader("E:\\Temp\\MyTest.txt");
                //Read the first line of text
                line = sr.ReadLine();
                //Continue to read until you reach end of file
                while (line != null)
                {
                    //write the line to console window
                    Console.WriteLine(line);
                    //Read the next line
                    line = sr.ReadLine();
                }
                //close the file
                sr.Close();
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                Console.WriteLine("Executing finally block.");
            }

            File.AppendAllText(@"E:\temp\MyTest.txt", "text content" + Environment.NewLine);
            
            // This will create a file named sample.txt
            // at the specified location 
            StreamWriter sw = File.CreateText("C:\\Users\\Public\\Documents\\geeksforgeeks.txt");

            // To write on the console screen
            Console.WriteLine("Enter the Text that you want to write on File");

            // To read the input from the user
            string str = Console.ReadLine();

            // To write a line in buffer
            sw.WriteLine(str);

            // To write in output stream
            sw.Flush();

            // To close the stream
            sw.Close();

            
            
            string path = @"E:\Temp\MyTest.txt";
            
            if (!File.Exists(path)){
                // Create a file to write to.
                using (StreamWriter swr = File.CreateText(path)){
                    swr.WriteLine(ts.Milliseconds + "," + value.Value.x + "," + value.Value.y + "," + value.Value.z);
                }
            }
            using (StreamWriter swr = File.AppendText(path)){
                swr.WriteLine(ts.Milliseconds + "," + value.Value.x + "," + value.Value.y + "," + value.Value.z);
            }*/
            //ts = DateTime.Now;
            
            tiempo_previo = DateTime.Now;
            watch.Stop();
            System.Diagnostics.Debug.WriteLine(watch.ElapsedMilliseconds + "," + value.Value.z + "," + valor_izquierdo);
            //watch.Restart();
            watch.Start();
        }

    }
}
