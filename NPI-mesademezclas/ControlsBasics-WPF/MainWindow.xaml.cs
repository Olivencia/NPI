
/**
* Cristóbal Antonio Olivencia Carrión - Matteo Lucheli
*
* NPI - Nuevos Paradigmas de Interacción
*
* Práctica 2
*/

namespace Microsoft.Samples.Kinect.ControlsBasics
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using Microsoft.Kinect;
    using Microsoft.Kinect.Toolkit;
    using Microsoft.Kinect.Toolkit.Controls;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;
    using System.IO;
    using System.Windows.Forms;
    using System.ComponentModel;
    using System.Diagnostics;
    using WMPLib;

    
    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    /// 
    public partial class MainWindow
    {
        //Variable necesaria para poder usar la región donde se verá la mano y se 
        //realizarán las acciones
        private readonly KinectSensorChooser sensorChooser;
        //Estas variables se han definido para el control de la mano y los movimientos
        private Point posicion_cuadrado, cuadrado_max;
        private HandPointer mano;
        private int error = 35;
        //Variables de tipo boleanas que ayudarán a pasar entre los diferentes estados
        private Boolean debug, puno, play1, play2, fastforward1, fastforward2, fastreverse1, fastreverse2, loop1, loop2,ini1,ini2,visible;
        WindowsMediaPlayer cancion1, cancion2;
        private double rate1, rate2;
        //Variables que se usarán para obtener el path del proyecto
        string currentDir;
        DirectoryInfo Imagedirectory,Musicdirectory;
        string ImageDirectory, MusicDirectory;
        System.Windows.Threading.DispatcherTimer dt1;
        System.Windows.Threading.DispatcherTimer dt2;
        //Variables necesarias para mostrar el reloj
        Stopwatch stopWatch1;
        Stopwatch stopWatch2;
        //Variables de las diferentes imágenes que se usarán
        BitmapImage imgloop, imgloopon1,imgloopon2, imgvinilo1, imgvinilo2, imgplay1, imgplay2, imgfastforward1,imgfastforwardon1, 
            imgfastforward2,imgfastforwardon2, imgfastprevious1,imgfastpreviouson1, imgfastprevious2,imgfastpreviouson2, imgpause1, imgpause2; 

        //Función que actualizará el tiempo de la primera canción. Si la primera canción está en estado de bucle, una vez finalizada comenzará
        //de nuevo con las características que tenia la anterior, pero si no está en estado de bucle se reinician los valores de los efectos de sonido.
        void dt_Tick1(object sender, EventArgs e)
        {
            if (stopWatch1.IsRunning)
            {
                TimeSpan ts = stopWatch1.Elapsed;
                time1.Text = cancion1.controls.currentPosition.ToString();
                if ((int)cancion1.controls.currentPosition == (int)cancion1.controls.currentItem.duration && (int)cancion1.controls.currentPosition!=0)
                {
                    if (!loop1)  restore1();
                }
            }
        }


        //Función que actualizará el tiempo de la segunda canción. Si la segunda canción está en estado de bucle, una vez finalizada comenzará
        //de nuevo con las características que tenia la anterior, pero si no está en estado de bucle se reinician los valores de los efectos de sonido. 
        void dt_Tick2(object sender, EventArgs e)
        {
            if (stopWatch2.IsRunning)
            {
                TimeSpan ts = stopWatch2.Elapsed;
                time2.Text = cancion2.controls.currentPosition.ToString();

                if ((int)cancion2.controls.currentPosition == (int)cancion2.controls.currentItem.duration && !ini2)
                {
                    if (!loop2)  restore2();
                }
            }
        }

        
        public MainWindow()
        {
            this.InitializeComponent(); 
            //Se inicializan las variables necesarias para las acciones
            puno = false;
            debug = true;
            play1 = false;
            play2 = false;
            fastforward1 = false;
            fastforward2 = false;
            fastreverse1 = false;
            fastreverse2 = false;
            loop1 = false;
            loop2 = false;
            ini1 = true;
            ini2 = true;
            visible = false;
            rate1 = 1;
            rate2 = 1;

            //Se inicializan las canciones
            cancion1 = new WindowsMediaPlayer();
            cancion2 = new WindowsMediaPlayer();

            //Se obtiene el path de la música e imagenes
            currentDir = Environment.CurrentDirectory;
            Imagedirectory = new DirectoryInfo(currentDir + @"\..\Images\");
            ImageDirectory = Imagedirectory.FullName;
            Musicdirectory = new DirectoryInfo(currentDir + @"\..\Music\");
            MusicDirectory = Musicdirectory.FullName;
          
            //Se cargan las imagenes que se van a usar
            chargeImages();
            
            // Inicializamos el sensor sensorChooser para que se active
            this.sensorChooser = new KinectSensorChooser();
            this.sensorChooser.KinectChanged += SensorChooserOnKinectChanged;
            this.sensorChooser.Start();
            var regionSensorBinding = new System.Windows.Data.Binding("Kinect") { Source = this.sensorChooser };
            BindingOperations.SetBinding(this.kinectRegion, KinectRegion.KinectSensorProperty, regionSensorBinding);

            //Se inician las funciones necesarias para mostrar la duración de cada canción
            dt1 = new System.Windows.Threading.DispatcherTimer();
            dt2 = new System.Windows.Threading.DispatcherTimer();
            stopWatch1 = new Stopwatch();
            stopWatch2 = new Stopwatch();

            dt1.Tick += new EventHandler(dt_Tick1);
            dt1.Interval = new TimeSpan(0, 0, 0, 0, 1);

            dt2.Tick += new EventHandler(dt_Tick2);
            dt2.Interval = new TimeSpan(0, 0, 0, 0, 1);


        }

        //Función usada para cargar las imágenes que se van a ir cambiando durante el programa
        private void chargeImages(){
            imgloop = new BitmapImage(new Uri(ImageDirectory + "loop.png"));
            imgloopon1 = new BitmapImage(new Uri(ImageDirectory + "loopon1.png"));
            imgloopon2 = new BitmapImage(new Uri(ImageDirectory + "loopon2.png"));
            imgvinilo1 = new BitmapImage(new Uri(ImageDirectory + "disco1.png"));
            imgvinilo2 = new BitmapImage(new Uri(ImageDirectory + "disco2.png"));
            imgplay1 = new BitmapImage(new Uri(ImageDirectory + "play1.png"));
            imgplay2 = new BitmapImage(new Uri(ImageDirectory + "play2.png"));
            imgfastforward1 = new BitmapImage(new Uri(ImageDirectory + "fastforward1.png"));
            imgfastforwardon1 = new BitmapImage(new Uri(ImageDirectory + "fastforwardon1.png"));
            imgfastforward2 = new BitmapImage(new Uri(ImageDirectory + "fastforward2.png"));
            imgfastforwardon2 = new BitmapImage(new Uri(ImageDirectory + "fastforwardon2.png"));
            imgfastprevious1 = new BitmapImage(new Uri(ImageDirectory + "fastprevious1.png"));
            imgfastpreviouson1 = new BitmapImage(new Uri(ImageDirectory + "fastpreviouson1.png"));
            imgfastprevious2 = new BitmapImage(new Uri(ImageDirectory + "fastprevious2.png"));
            imgfastpreviouson2 = new BitmapImage(new Uri(ImageDirectory + "fastpreviouson2.png"));
            imgpause1 = new BitmapImage(new Uri(ImageDirectory + "pause1.png"));
            imgpause2 = new BitmapImage(new Uri(ImageDirectory + "pause2.png"));
    }


        //En ese función se detecta si hay otro nuevo sensor
        private static void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs args)
        {
            if (args.OldSensor != null)
            {
                try
                {
                    args.OldSensor.DepthStream.Range = DepthRange.Default;
                    args.OldSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    args.OldSensor.DepthStream.Disable();
                    args.OldSensor.SkeletonStream.Disable();
                }
                catch (InvalidOperationException)
                {
                }
            }

            if (args.NewSensor != null)
            {
                try
                {
                    args.NewSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    args.NewSensor.SkeletonStream.Enable();

                    try
                    {
                        args.NewSensor.DepthStream.Range = DepthRange.Near;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = true;
                    }
                    catch (InvalidOperationException)
                    {
                        args.NewSensor.DepthStream.Range = DepthRange.Default;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    }
                }
                catch (InvalidOperationException)
                {
                }
            }
        }

        //Función para detener el sensor al cerrar la ventana
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.sensorChooser.Stop();
        }

        //Se define esta función para detectar cuando se ha cerrado la mano
        //Primero se guarda en la variable mano los datos para acceder posteriormente
        //y a continuación se inicializan las variables de posición
        private void mano_cerrada(object sender, RoutedEventArgs e)
        {
            if (this.kinectRegion.HandPointers[0].IsActive) mano = this.kinectRegion.HandPointers[0];
            else mano = this.kinectRegion.HandPointers[1];
            posicion_cuadrado = mano.GetPosition(wrapPanel);
            cuadrado_max = posicion_cuadrado;
            puno = true;
        }


        //Esta función se encarga de detectar cuando la mano está abierta
        private void mano_abierta(object sender, RoutedEventArgs e)
        {
            puno = false;
        }

        //Esta función se llamará cuando la mano esté fuera de la pantalla
        private void mano_out(object sender, RoutedEventArgs e)
        {
            puno = false;

        }


        //Función que detecta en todo momento lo que estamos realiznado con la mano. Se ha optado por
        //cerrar la mano como forma similar a lo que sería hacer un click con el ratón por tanto con 
        //establecer la mano que se ve en el programa sobre un boton y cerrar ésta podremos realizar 
        //las opciones que hay
        private void comprobarGesto(object sender, RoutedEventArgs e)
        {    
            if (puno)
            {
                //En esta sección si la mano está cerrada en la determinada posición donde está el botón de "play" de la primera canción, 
                //comienza la canción y se cambia la imagen a la de pause
                if (mano.GetPosition(wrapPanel).X > 147 && mano.GetPosition(wrapPanel).X < 259 && mano.GetPosition(wrapPanel).Y > 660)
                {

                    if (!play1)
                    {
                        play1 = true;
                        if (ini1)
                        {          
                            cancion1.URL = MusicDirectory + "Equinox.mp3";
                            stopWatch1.Start();
                            dt1.Start();
                            ini1 = false;
                        }
                        
                        cancion1.controls.play();
                        play_1.Source = imgpause1;
                        song1.Text = cancion1.currentMedia.name;
                        cancion1.settings.rate = rate2;
                        stopWatch1.Start();
                    }
                    else
                    {
                        play1 = false;
                        cancion1.controls.pause();
                        stopWatch1.Stop();
                        play_1.Source = imgplay1;
                    }
                }

                //En esta sección si la mano está cerrada en la determinada posición donde está el botón de "play" de la segunda canción, 
                //comienza la canción y se cambia la imagen a la de pause
                else if (mano.GetPosition(wrapPanel).X > 820 && mano.GetPosition(wrapPanel).X < 935 && mano.GetPosition(wrapPanel).Y > 660)
                {
                    if (!play2)
                    {
                        play2 = true;
                        
                        if (ini2)
                        {
                            cancion2.URL = MusicDirectory + "Devilsden.mp3";
                            stopWatch2.Start();
                            dt2.Start();
                            ini2 = false;
                        }
                        cancion2.controls.play();
                        play_2.Source = imgpause2;
                        song2.Text = cancion2.currentMedia.name;
                        cancion2.settings.rate = rate2;
                        stopWatch2.Start();
                    }
                    else
                    {
                        play2 = false;
                        cancion2.controls.pause();
                        stopWatch2.Stop();
                        play_2.Source = imgplay2;
                    }
                }
                //En esta sección si la mano está cerrada en la determinada posición donde está el botón de "fastforward" de la primera canción, 
                //es decir, el avance rápido hacia adelante, comienza la canción y se cambia la imagen por otra determinada que muestre que está pulsada
                else if (mano.GetPosition(wrapPanel).X > 279 && mano.GetPosition(wrapPanel).X < 410 && mano.GetPosition(wrapPanel).Y > 660)
                {


                    if (!fastforward1 && play1)
                    {
                        if (fastreverse1)
                        {
                            fastreverse1 = false;
                            fastprevious_1.Source = imgfastprevious1;
                        }
                        fastforward1 = true;
                        cancion1.controls.fastForward();
                        fastforward_1.Source = imgfastforwardon1;
                        rate_1.Text = "5";
                        rate1 = 5;
                    }
                    else
                    {
                        fastforward1 = false;
                        cancion1.settings.rate = 1;
                        rate1 = 1;
                        fastforward_1.Source = imgfastforward1;

                    }
                }

                //En esta sección si la mano está cerrada en la determinada posición donde está el botón de "fastforward" de la segunda canción, 
                //es decir, el avance rápido hacia adelante, comienza la canción y se cambia la imagen por otra determinada que muestre que está pulsada
                else if (mano.GetPosition(wrapPanel).X > 950 && mano.GetPosition(wrapPanel).X < 1060 && mano.GetPosition(wrapPanel).Y > 660)
                {


                    if (!fastforward2 && play2)
                    {
                        if (fastreverse2)
                        {
                            fastreverse2 = false;
                            fastprevious_2.Source = imgfastprevious2;
                        }
                        fastforward2 = true;
                        cancion2.controls.fastForward();
                        fastforward_2.Source = imgfastforwardon2;
                        rate_2.Text = "5";
                        rate2 = 5;
                    }
                    else
                    {
                        fastforward2 = false;
                        cancion2.settings.rate = 1;
                        rate2 = 1;
                        fastforward_2.Source = imgfastforward2;
                    }
                }

                //En esta sección si la mano está cerrada en la determinada posición donde está el botón de "fastreverse" de la primera canción, 
                //es decir, el avance rápido hacia atrás, comienza la canción y se cambia la imagen por otra determinada que muestre que está pulsada
                else if (mano.GetPosition(wrapPanel).X > 10 && mano.GetPosition(wrapPanel).X < 127 && mano.GetPosition(wrapPanel).Y > 660)
                {


                    if (!fastreverse1 && play1)
                    {
                        if (fastforward1)
                        {
                            fastforward1 = false;
                            fastforward_1.Source = imgfastforward1;
                        }
                        fastreverse1 = true;
                        if (!cancion1.controls.get_isAvailable("fastReverse"))
                        {
                            cancion1.settings.rate = 0.5;
                            rate1 = 0.5;
                            rate_1.Text = rate1.ToString();
                        }
                        else cancion1.controls.fastReverse();
                        fastprevious_1.Source = imgfastpreviouson1;
                    }
                    else
                    {
                        fastreverse1 = false;
                        cancion1.controls.pause();
                        cancion1.controls.play(); 
                        fastprevious_1.Source = imgfastprevious1;

                    }
                }

                //En esta sección si la mano está cerrada en la determinada posición donde está el botón de "fastreverse" de la segunda canción, 
                //es decir, el avance rápido hacia atrás, comienza la canción y se cambia la imagen por otra determinada que muestre que está pulsada
                else if (mano.GetPosition(wrapPanel).X > 670 && mano.GetPosition(wrapPanel).X < 790 && mano.GetPosition(wrapPanel).Y > 660)
                {


                    if (!fastreverse2 && play2)
                    {
                        if (fastforward2)
                        {
                            fastforward2 = false;
                            fastforward_2.Source = imgfastforward2;
                        }
                        fastreverse2 = true;
                        if (!cancion2.controls.get_isAvailable("fastReverse"))
                        {
                            cancion2.settings.rate = 0.5;
                            rate2 = 0.5;
                            rate_2.Text = rate2.ToString();
                        }
                        else cancion2.controls.fastReverse();
                        fastprevious_2.Source = imgfastpreviouson2;
                    }
                    else
                    {
                        fastreverse2 = false;
                        cancion2.controls.pause();
                        cancion2.controls.play();
                        fastprevious_2.Source = imgfastprevious2;
                    }
                }

                //En esta sección si la mano está cerrada en la determinada posición donde está el botón de "loop" de la primera canción, 
                //se activa el modo bucle para que la cancion una vez finalizada vuelva a comenzar de nuevo y se cambia la imagen por otra 
                //determinada que muestre que está pulsada
                else if (mano.GetPosition(wrapPanel).X > 385 && mano.GetPosition(wrapPanel).X < 485 && mano.GetPosition(wrapPanel).Y > 545 && mano.GetPosition(wrapPanel).Y < 645)
                {
                    
                    if (!loop1)
                    {
                        loop1 = true;
                        cancion1.settings.setMode("loop", true);
                        loop_1.Source = imgloopon1;
                    }
                    else
                    {
                        loop1 = false;
                        cancion1.settings.setMode("loop", false);
                        loop_1.Source = imgloop;
                    }
                }

                //En esta sección si la mano está cerrada en la determinada posición donde está el botón de "loop" de la segunda canción, 
                //se activa el modo bucle para que la cancion una vez finalizada vuelva a comenzar de nuevo y se cambia la imagen por otra 
                //determinada que muestre que está pulsada
                else if (mano.GetPosition(wrapPanel).X > 615 && mano.GetPosition(wrapPanel).X < 715 && mano.GetPosition(wrapPanel).Y > 545 && mano.GetPosition(wrapPanel).Y < 645)
                {
                    if (!loop2)
                    {
                        loop2 = true;
                        cancion2.settings.setMode("loop", true);
                        loop_2.Source = imgloopon2;
                    }
                    else
                    {
                        loop2 = false;
                        cancion2.settings.setMode("loop", false);
                        loop_2.Source = imgloop;
                    }
                }


                //En esta sección si la mano está cerrada en la determinada posición donde está la barra central de abajo entre la primera y 
                //segunda canción, según la posición sonará más una canción o la otra (más a la izquierda canción 1, más a la derecha canción 2)
                else if (mano.GetPosition(wrapPanel).X > 450 && mano.GetPosition(wrapPanel).X < 625 && mano.GetPosition(wrapPanel).Y > 660)
                {
                    if (mano.GetPosition(wrapPanel).X < 493.75)
                    {
                        cancion1.settings.volume = 100;
                        cancion2.settings.mute = true;
                        cancion1.settings.mute = false;

                    }
                    else if (mano.GetPosition(wrapPanel).X < 537.5)
                    {
                        cancion1.settings.volume = 75;
                        cancion2.settings.volume = 25;
                        cancion1.settings.mute = false;
                        cancion2.settings.mute = false;

                    }

                    else if (mano.GetPosition(wrapPanel).X > 500 && mano.GetPosition(wrapPanel).X < 550)
                    {
                        cancion1.settings.volume = 50;
                        cancion2.settings.volume = 50;
                        cancion1.settings.mute = false;
                        cancion2.settings.mute = false;

                    }
                    else if (mano.GetPosition(wrapPanel).X < 581.25)
                    {
                        cancion1.settings.volume = 25;
                        cancion2.settings.volume = 75;
                        cancion1.settings.mute = false;
                        cancion2.settings.mute = false;

                    }

                    else if (mano.GetPosition(wrapPanel).X < 625)
                    {
                        cancion1.settings.mute = true;
                        cancion2.settings.volume = 100;
                        cancion2.settings.mute = false;

                    }
                    Canvas.SetLeft(central, mano.GetPosition(wrapPanel).X);
                }


                //En estas dos secciones si la mano está cerrada en la determinada posición donde está el botón de "más" o el botón de menos, 
                //se podrá cambiar la velocidad de la primera canción. Los valores están entre 0.25 - 9 siendo estos los límites
                else if (mano.GetPosition(wrapPanel).X > 425 && mano.GetPosition(wrapPanel).X < 485 && mano.GetPosition(wrapPanel).Y > 205 && mano.GetPosition(wrapPanel).Y < 265)
                {
                    if(rate1>=1 && rate1 < 9){
                        rate1++;
                        rate_1.Text=rate1.ToString();
                        rate_1.Foreground = new SolidColorBrush(Colors.Black);
                    }
                    else if(rate1<1){
                        rate1+=0.25;
                        rate_1.Text=rate1.ToString();
                        rate_1.Foreground = new SolidColorBrush(Colors.Black);
                    }
                    else if(rate1==9) rate_1.Foreground=new SolidColorBrush(Colors.Red);
                    cancion1.settings.rate = rate1;
                    fastforward_1.Source = imgfastforward1;
                    fastprevious_1.Source = imgfastprevious1;

                }

                   
                else if (mano.GetPosition(wrapPanel).X > 425 && mano.GetPosition(wrapPanel).X < 485 && mano.GetPosition(wrapPanel).Y > 460 && mano.GetPosition(wrapPanel).Y < 520)
                {
                    if (rate1 > 0.25 && rate1 <= 1)
                    {
                        rate1 -= 0.25;
                        rate_1.Text = rate1.ToString();
                        rate_1.Foreground = new SolidColorBrush(Colors.Black);
                    }
                    else if (rate1 > 1)
                    {
                        rate1--;
                        rate_1.Text = rate1.ToString();
                        rate_1.Foreground = new SolidColorBrush(Colors.Black);
                    }
                    else if (rate1 == 0.25) rate_1.Foreground = new SolidColorBrush(Colors.Red);
                    cancion1.settings.rate = rate1;
                    fastforward_1.Source = imgfastforward1;
                    fastprevious_1.Source = imgfastprevious1;
                }

                //En estas dos secciones si la mano está cerrada en la determinada posición donde está el botón de "más" o el botón de menos, 
                //se podrá cambiar la velocidad de la segunda canción. Los valores están entre 0.25 - 9 siendo estos los límites
                else if (mano.GetPosition(wrapPanel).X > 580 && mano.GetPosition(wrapPanel).X < 640 && mano.GetPosition(wrapPanel).Y > 205 && mano.GetPosition(wrapPanel).Y < 265)
                {
                    if (rate2 >= 1 && rate2 < 9)
                    {
                        rate2++;
                        rate_2.Text = rate2.ToString();
                        rate_2.Foreground = new SolidColorBrush(Colors.Black);
                    }
                    else if (rate2 < 1)
                    {
                        rate2 += 0.25;
                        rate_2.Text = rate2.ToString();
                        rate_2.Foreground = new SolidColorBrush(Colors.Black);
                    }
                    else if (rate2 == 9) rate_2.Foreground = new SolidColorBrush(Colors.Red);
                    cancion2.settings.rate = rate2;
                    fastforward_2.Source = imgfastforward2;
                    fastprevious_2.Source = imgfastprevious2;
                }

                    
                else if (mano.GetPosition(wrapPanel).X > 580 && mano.GetPosition(wrapPanel).X < 640 && mano.GetPosition(wrapPanel).Y > 460 && mano.GetPosition(wrapPanel).Y < 520)
                {
                    if (rate2 > 0.25 && rate2 <= 1)
                    {
                        rate2 -= 0.25;
                        rate_2.Text = rate2.ToString();
                        rate_2.Foreground = new SolidColorBrush(Colors.Black);
                    }
                    else if (rate2 > 1)
                    {
                        rate2--;
                        rate_2.Text = rate2.ToString();
                        rate_2.Foreground = new SolidColorBrush(Colors.Black);
                    }
                    else if (rate2 == 0.25) rate_2.Foreground = new SolidColorBrush(Colors.Red);
                    cancion2.settings.rate = rate2;
                    fastforward_2.Source = imgfastforward2;
                    fastprevious_2.Source = imgfastprevious2;
                }

                //En esta sección si la mano está cerrada en la determinada posición donde está la barra de la primera canción
                //se podrá cambiar la salida de audio (izquierda altavoces de la izquierda, derecha altavoces de la derecha)
                else if (mano.GetPosition(wrapPanel).X > 60 && mano.GetPosition(wrapPanel).X < 335 && mano.GetPosition(wrapPanel).Y > 590 && mano.GetPosition(wrapPanel).Y < 630)
                {
                    double x = mano.GetPosition(wrapPanel).X-60, val = 335 - 60;
                    cancion1.settings.balance = (int) (x * 100 / val);
                    Canvas.SetLeft(central1, mano.GetPosition(wrapPanel).X-error);
                }

                //En esta sección si la mano está cerrada en la determinada posición donde está la barra de la segunda canción
                //se podrá cambiar la salida de audio (izquierda altavoces de la izquierda, derecha altavoces de la derecha)
                else if (mano.GetPosition(wrapPanel).X > 730 && mano.GetPosition(wrapPanel).X < 1005 && mano.GetPosition(wrapPanel).Y > 590 && mano.GetPosition(wrapPanel).Y < 630)
                {
                    double x = mano.GetPosition(wrapPanel).X - 730, val = 335 - 730;
                    cancion1.settings.balance = (int)(x * 100 / val);
                    Canvas.SetLeft(central2, mano.GetPosition(wrapPanel).X-error);
                }

                //En esta sección si la mano está cerrada en la determinada posición donde está el botón del centro "actualizar"
                //se podrá restablecerán los valores por defecto
                else if (visible && mano.GetPosition(wrapPanel).X > 450 && mano.GetPosition(wrapPanel).X < 550 && mano.GetPosition(wrapPanel).Y > 80 && mano.GetPosition(wrapPanel).Y < 180)
                {
                    restore1();
                    restore2();
                    Canvas.SetLeft(central, 537 - error);
                }

                //En esta sección si la mano está cerrada en la determinada posición donde está el botón de la esquina superior derecha
                //"cerrar" se dará por finalizado el programa y se cerrará
                else if (visible && mano.GetPosition(wrapPanel).X > 1010 && mano.GetPosition(wrapPanel).X < 1110 && mano.GetPosition(wrapPanel).Y > 0 && mano.GetPosition(wrapPanel).Y < 150)
                {
                    restore1();
                    restore2();
                    System.Windows.Forms.Application.Exit();
                    System.Environment.Exit(0);  
                }

            }
            puno = false;
        }

        //Función que restaura los valores por defecto de la primera canción
        private void restore1()
        {
            cancion1.controls.stop();
            cancion1.settings.rate = 1;
            cancion1.settings.setMode("loop", false);
            rate1 = 1;
            play1 = false;
            fastforward1 = false;
            fastreverse1 = false;
            ini1 = true;
            loop1 = false;
            loop_1.Source = imgloop;
            song1.Text = "";
            time1.Text = "";
            stopWatch1.Stop();
            dt1.Stop();
            rate_1.Text = "1";
            fastforward_1.Source = imgfastforward1;
            fastprevious_1.Source = imgfastprevious1;
            play_1.Source = imgplay1;
            Canvas.SetLeft(central1, 197 - error);
        }

        //Función que restaura los valores por defecto de la segunda canción
        private void restore2()
        {
            cancion2.controls.stop();
            cancion2.settings.rate = 1;
            cancion2.settings.setMode("loop", false);
            rate2 = 1;
            play2 = false;
            fastforward2 = false;
            fastreverse2 = false;
            ini2 = true;
            loop2 = false;
            loop_2.Source = imgloop;
            song2.Text = "";
            time2.Text = "";
            stopWatch2.Stop();
            dt2.Stop();
            rate_2.Text = "1";
            play_2.Source = imgplay2;
            fastforward_2.Source = imgfastforward2;
            fastprevious_2.Source = imgfastprevious2;
            Canvas.SetLeft(central2, 867 - error);
        }


        //Función que muestra/oculta ciertos botones ocultos, como los de restaurar valores por defecto y el botón de salir
        private void tile_Click(object sender, RoutedEventArgs e)
        {
            KinectTileButton button = (KinectTileButton)sender;
            if (!visible)
            {
                visible = true;
                exit.Visibility = System.Windows.Visibility.Visible;
                restore.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                visible = false;
                exit.Visibility = System.Windows.Visibility.Hidden;
                restore.Visibility = System.Windows.Visibility.Hidden;
            }
        }



    }
}

