//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

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

    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    /// 
    public partial class MainWindow
    {
        //Variable necesaria para poder usar la región donde se verá la mano y se 
        //realizarán los movimientos
        private readonly KinectSensorChooser sensorChooser;

        //Estas variables se han definido para el control de la mano y los movimientos
        private Boolean puno;
        private Point posicion_cuadrado;
        private Point cuadrado_max;
        private Boolean cuadrado_horizontal_d;
        private Boolean cuadrado_vertical_ab;
        private Boolean cuadrado_horizontal_i;
        private Boolean cuadrado_vertical_ar;
        private int error;
        private Boolean debug;
        HandPointer mano;
        private int[] resultado;
        private Boolean cambioGesto;

        //Para poder visualizar el esqueleto usaremos las siguientes:
        private const float RenderWidth = 640.0f;
        private const float RenderHeight = 480.0f;
        private const double JointThickness = 3;
        private const double BodyCenterThickness = 10;
        private const double ClipBoundsThickness = 10;
        private readonly Brush centerPointBrush = Brushes.Blue;
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));      
        private readonly Brush inferredJointBrush = Brushes.Yellow;
        private readonly Pen trackedBonePen = new Pen(Brushes.Green, 6);
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

       //Para visualizar la imagen real (color) de la kinect:
        private KinectSensor sensor;
        private DrawingGroup drawingGroup;
        private DrawingImage imageSource;
        private WriteableBitmap colorBitmap;
        private byte[] colorPixels;

        public MainWindow()
        {
            this.InitializeComponent();


            //Se inicializan las variables necesarias para el control del gesto
            puno = false;
            cuadrado_horizontal_d = true;
            cuadrado_vertical_ab = false;
            cuadrado_horizontal_i = false;
            cuadrado_vertical_ar = false;
            debug = false;
            error = 100;
            resultado = new int[2];
            cambioGesto = false;

            // Inicializamos el sensor sensorChooser para que se active
            this.sensorChooser = new KinectSensorChooser();
            this.sensorChooser.KinectChanged += SensorChooserOnKinectChanged;
            this.sensorChooserUi.KinectSensorChooser = this.sensorChooser;
            this.sensorChooser.Start();
            var regionSensorBinding = new Binding("Kinect") { Source = this.sensorChooser };
            BindingOperations.SetBinding(this.kinectRegion, KinectRegion.KinectSensorProperty, regionSensorBinding);
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
        //Primero se guarda en la variable mano los datos para acceder posteriromente
        //y a continuación se inicializan las variables de posición
        private void mano_cerrada(object sender, RoutedEventArgs e)
        {
            if (this.kinectRegion.HandPointers[0].IsActive) mano = this.kinectRegion.HandPointers[0];
            else mano = this.kinectRegion.HandPointers[1];
            posicion_cuadrado = mano.GetPosition(wrapPanel);
            cuadrado_max = posicion_cuadrado;
            puno = true;
            this.wrapPanel.Children.Add(new SelectionDisplay(("Mano cerrada, puede comenzar el cuadrado")));
        }


        //Esta es una función que se llama para reiniciar las variables en caso
        //de inicio del programa o error a la hora de realizar el movimiento
        private void ajustarVariablesInicio()
        {
            puno = false;
            cuadrado_horizontal_d = true;
            cuadrado_vertical_ar = false;
            cuadrado_horizontal_i = false;
            cuadrado_vertical_ab = false;
            resultado[0] = 0;
            resultado[1] = 0;
            cambioGesto = false;

        }
        //Esta función se encarga de detectar cuando la mano está abierta
        private void mano_abierta(object sender, RoutedEventArgs e)
        {
            ajustarVariablesInicio(); 
            var converter = new System.Windows.Media.BrushConverter();
            this.tile.Background = (System.Windows.Media.Brush)converter.ConvertFromString("#FF52318F");
        }

        //Esta función se llamará cuando la mano esté fuera de la pantalla
        private void mano_out(object sender, RoutedEventArgs e)
        {
            ajustarVariablesInicio();
        }


        //Aquí es donde está definido el gesto del cuadrado comenzando por el lado horizontal.
        //Consta de varias partes, 2 por cada movimiento que se va a realizar. En este caso son 
        //4 movimientos: derecha, abajo, izquierda, arriba. Entre cada movimiento hay que dejar un
        //margen de error de pasarse en esa dirección. Se usan 4 booleanos para pasar a las diferentes
        //partes del gesto, cuando se esta realizando el movimiento solo hay activo 1 booleano que 
        //será el que le corresponde. Si hemos terminado un movimiento y vamos a empezar otro, habrá
        //dos boleanos activos, el booleano que se ha usado en el movimiento finalizado y el boleano
        //del movimiento que se realizará a continuación. La función devuelve un entero, -1 si ha 
        //habido algun error, 0 si todo va correctamente pero no ha terminado el gesto y 1 cuando
        //se ha reconocido el gesto.

        private int GestoCuadrado1()
        {
            int salida = 0;
            //Movimiento horizontal derecha
            if (cuadrado_horizontal_d && !cuadrado_vertical_ab && !cuadrado_horizontal_i && !cuadrado_vertical_ar)
            {
                //Este será el margen de error que debe cumplir con respecto al eje Y
                //para que se siga en este movimiento
                if (Math.Abs(posicion_cuadrado.Y - mano.GetPosition(wrapPanel).Y) < error)
                {
                    //En cuanto se realice una determinada distancia de movimiento hacia la derecha
                    //habremos realizado el movimiento correctamente
                    if (mano.GetPosition(wrapPanel).X - posicion_cuadrado.X > 100)
                    {
                        this.wrapPanel.Children.Add(new SelectionDisplay("Movimiento hacia la derecha completado"));
                        var converter = new System.Windows.Media.BrushConverter();
                        this.tile.Background = (System.Windows.Media.Brush)converter.ConvertFromString("#FF2E9AFE");
                        cuadrado_vertical_ab = true;
                    }
                    //Si hemos ido en dirección contraria se interrumpe el gesto
                    if (mano.GetPosition(wrapPanel).X - posicion_cuadrado.X < -error)
                    {
                        salida = -1;
                        if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Error 1 1"));
                        var converter = new System.Windows.Media.BrushConverter();
                        this.tile.Background = (System.Windows.Media.Brush)converter.ConvertFromString("#FFA51E16");
                    }
                }
                else
                {
                    //Si ha sobrepasado el margen de error con respecto al eje x e y se interrumpe
                    //el gesto
                    if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Error 1 2"));
                    var converter = new System.Windows.Media.BrushConverter();
                    this.tile.Background = (System.Windows.Media.Brush)converter.ConvertFromString("#FFA51E16");
                    salida = -1;
                }
            }

        //Movimiento anterior al vertical hacia abajo
            if (cuadrado_horizontal_d && cuadrado_vertical_ab && !cuadrado_horizontal_i && !cuadrado_vertical_ar)
            {
                //Este será el máximo de margen de error que dejaremos sobrepasar
                if (mano.GetPosition(wrapPanel).X - posicion_cuadrado.X < -200)
                {
                    salida = -1;
                    var converter = new System.Windows.Media.BrushConverter();
                    this.tile.Background = (System.Windows.Media.Brush)converter.ConvertFromString("#FFA51E16");
                    if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Fallido 4 1"));
                }
                else
                {
                    //Si se detecta un movimiento en vertical hacia arriba se interrumpe
                    //la detección del gesto
                    if (mano.GetPosition(wrapPanel).Y - posicion_cuadrado.Y < -error)
                    {
                        salida = -1;
                        var converter = new System.Windows.Media.BrushConverter();
                        this.tile.Background = (System.Windows.Media.Brush)converter.ConvertFromString("#FFA51E16");
                        if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Fallido 4 2"));
                    }
                    //Si se ha detectado un ligero movimiento vertical hacia abajo podremos
                    //comenzar con el siguiente movimiento y guardamos el nuevo valor que tendrá
                    //posicion_cuadrado (nueva posición inicial del siguiente movimiento)
                    if (mano.GetPosition(wrapPanel).Y - posicion_cuadrado.Y > 50)
                    {
                        cuadrado_horizontal_d = false;
                        posicion_cuadrado = mano.GetPosition(wrapPanel);
                        var converter = new System.Windows.Media.BrushConverter();
                        this.tile.Background = (System.Windows.Media.Brush)converter.ConvertFromString("#FF52318F");
                        this.wrapPanel.Children.Add(new SelectionDisplay("Sigue moviendo la mano hacia abajo"));
                    }
                }
            }

            //Movimiento vertical hacia abajo
            if (!cuadrado_horizontal_d && cuadrado_vertical_ab && !cuadrado_horizontal_i && !cuadrado_vertical_ar)
            {
                //Este será el margen de error que debe cumplir con respecto al eje x para que 
                //se siga en este movimiento
                if (Math.Abs(posicion_cuadrado.X - mano.GetPosition(wrapPanel).X) < error)
                {
                    //En cuanto se realice una determinada distancia de movimiento hacia abajo habremos 
                    //realizado el movimiento correctamente
                    if (mano.GetPosition(wrapPanel).Y - posicion_cuadrado.Y > 100)
                    {
                        cuadrado_horizontal_i = true;
                        var converter = new System.Windows.Media.BrushConverter();
                        this.tile.Background = (System.Windows.Media.Brush)converter.ConvertFromString("#FF2E9AFE");
                        this.wrapPanel.Children.Add(new SelectionDisplay("Movimiento hacia abajo completado"));
                    }
                    //Si hemos ido en dirección contraria se interrumpe el gesto
                    if (mano.GetPosition(wrapPanel).Y - posicion_cuadrado.Y < -error)
                    {
                        salida = -1;
                        var converter = new System.Windows.Media.BrushConverter();
                        this.tile.Background = (System.Windows.Media.Brush)converter.ConvertFromString("#FFA51E16");
                        if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Fallido 1 1"));
                    }
                }
                else
                {
                    //Si ha sobrepasado el margen de error con respecto al eje x e y se interrumpe
                    //el gesto
                    salida = -1;
                    var converter = new System.Windows.Media.BrushConverter();
                    this.tile.Background = (System.Windows.Media.Brush)converter.ConvertFromString("#FFA51E16");
                    if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Fallido 1 2"));
                }
            }

            //Movimiento anterior al horizontal izquierda
            if (!cuadrado_horizontal_d && cuadrado_vertical_ab && cuadrado_horizontal_i && !cuadrado_vertical_ar)
            {
                //Este será el máximo de margen de error que dejaremos sobrepasar
                if (mano.GetPosition(wrapPanel).Y - posicion_cuadrado.Y < -200)
                {
                    salida = -1;
                    var converter = new System.Windows.Media.BrushConverter();
                    this.tile.Background = (System.Windows.Media.Brush)converter.ConvertFromString("#FFA51E16");
                    if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Fallido 6 1"));
                }
                else
                {
                    //Si se detecta un movimiento en vertical hacia la derecha se interrumpe
                    //la detección del gesto
                    if (mano.GetPosition(wrapPanel).X - posicion_cuadrado.X > error)
                    {
                        salida = -1;
                        var converter = new System.Windows.Media.BrushConverter();
                        this.tile.Background = (System.Windows.Media.Brush)converter.ConvertFromString("#FFA51E16");
                        if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Fallido 6 2"));
                    }
                    //Si se ha detectado un ligero movimiento horizontal hacia la izquierda podremos
                    //comenzar con el siguiente movimiento y guardamos el nuevo valor que tendrá
                    //posicion_cuadrado (nueva posición inicial del siguiente movimiento)
                    if (mano.GetPosition(wrapPanel).X - posicion_cuadrado.X < -error)
                    {
                        cuadrado_vertical_ab = false;
                        posicion_cuadrado = mano.GetPosition(wrapPanel);
                        var converter = new System.Windows.Media.BrushConverter();
                        this.tile.Background = (System.Windows.Media.Brush)converter.ConvertFromString("#FF52318F");
                        this.wrapPanel.Children.Add(new SelectionDisplay("Sigue moviendo la mano hacia la izquierda"));
                    }
                }
            }

            //Movimiento horizontal izquierda
            if (!cuadrado_horizontal_d && !cuadrado_vertical_ab && cuadrado_horizontal_i && !cuadrado_vertical_ar)
            {
                //Este será el margen de error que debe cumplir con respecto al eje Y
                //para que se siga en este movimiento
                if (Math.Abs(posicion_cuadrado.Y - mano.GetPosition(wrapPanel).Y) < error)
                {
                    //En cuanto se realice una determinada distancia de movimiento hacia la izquierda
                    //habremos realizado el movimiento correctamente
                    if (mano.GetPosition(wrapPanel).X - posicion_cuadrado.X < -100)
                    {
                        var converter = new System.Windows.Media.BrushConverter();
                        this.tile.Background = (System.Windows.Media.Brush)converter.ConvertFromString("#FF2E9AFE");
                        this.wrapPanel.Children.Add(new SelectionDisplay("Movimiento hacia la izquierda completado"));
                        cuadrado_vertical_ar = true;
                    }
                    //Si hemos ido en dirección contraria se interrumpe el gesto
                    if (mano.GetPosition(wrapPanel).X - posicion_cuadrado.X > error)
                    {
                        salida = -1;
                        var converter = new System.Windows.Media.BrushConverter();
                        this.tile.Background = (System.Windows.Media.Brush)converter.ConvertFromString("#FFA51E16");
                        if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Error 7 1"));
                    }
                }
                else
                {
                    //Si ha sobrepasado el margen de error con respecto al eje x e y se interrumpe
                    //el gesto
                    var converter = new System.Windows.Media.BrushConverter();
                    this.tile.Background = (System.Windows.Media.Brush)converter.ConvertFromString("#FFA51E16");
                    if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Error 7 2"));
                    salida = -1;
                }
            }

            //Movimiento anterior al vertical hacia arriba
            if (!cuadrado_horizontal_d && !cuadrado_vertical_ab && cuadrado_horizontal_i && cuadrado_vertical_ar)
            {
                //Este será el máximo de margen de error que dejaremos sobrepasar
                if (mano.GetPosition(wrapPanel).X - posicion_cuadrado.X > 200)
                {
                    salida = -1;
                    var converter = new System.Windows.Media.BrushConverter();
                    this.tile.Background = (System.Windows.Media.Brush)converter.ConvertFromString("#FFA51E16");
                    if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Fallido 4 1"));
                }
                else
                {
                    //Si se detecta un movimiento en vertical hacia abajo se interrumpe
                    //la detección del gesto
                    if (mano.GetPosition(wrapPanel).Y - posicion_cuadrado.Y > error)
                    {
                        salida = -1;
                        var converter = new System.Windows.Media.BrushConverter();
                        this.tile.Background = (System.Windows.Media.Brush)converter.ConvertFromString("#FFA51E16");
                        if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Fallido 4 2"));
                    }
                    //Si se ha detectado un ligero movimiento vertical hacia arriba podremos
                    //comenzar con el siguiente movimiento y guardamos el nuevo valor que tendrá
                    //posicion_cuadrado (nueva posición inicial del siguiente movimiento)
                    if (mano.GetPosition(wrapPanel).Y - posicion_cuadrado.Y < -error)
                    {
                        cuadrado_horizontal_i = false;
                        posicion_cuadrado = mano.GetPosition(wrapPanel);
                        var converter = new System.Windows.Media.BrushConverter();
                        this.tile.Background = (System.Windows.Media.Brush)converter.ConvertFromString("#FF52318F");
                        this.wrapPanel.Children.Add(new SelectionDisplay("Sigue moviendo la mano hacia arriba"));
                    }
                }
            }

            //Movimiento vertical hacia arriba
            if (!cuadrado_horizontal_d && !cuadrado_vertical_ab && !cuadrado_horizontal_i && cuadrado_vertical_ar)
            {
                //Este será el margen de error que debe cumplir con respecto al eje X
                //para que se siga en este movimiento
                if (Math.Abs(posicion_cuadrado.X - mano.GetPosition(wrapPanel).X) < error)
                {
                    //En cuanto se realice una determinada distancia de movimiento hacia arriba
                    //habremos realizado el movimiento correctamente
                    if (mano.GetPosition(wrapPanel).Y - posicion_cuadrado.Y < -100)
                    {
                        salida = 1;
                        cuadrado_horizontal_i = true;
                    }
                    //Si hemos ido en dirección contraria se interrumpe el gesto
                    if (mano.GetPosition(wrapPanel).Y - posicion_cuadrado.Y > error)
                    {
                        salida = -1;
                        var converter = new System.Windows.Media.BrushConverter();
                        this.tile.Background = (System.Windows.Media.Brush)converter.ConvertFromString("#FFA51E16");
                        if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Fallido 5 1"));
                    }
                }
                else
                {
                    //Si ha sobrepasado el margen de error con respecto al eje x e y se interrumpe
                    //el gesto
                    salida = -1;
                    var converter = new System.Windows.Media.BrushConverter();
                    this.tile.Background = (System.Windows.Media.Brush)converter.ConvertFromString("#FFA51E16");
                    if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Fallido 5 2"));
                }
            }

        return salida;
        }

        //Aquí es donde está definido el gesto del cuadrado comenzando por el lado horizontal.
        //Consta de varias partes, 2 por cada movimiento que se va a realizar. En este caso son 
        //4 movimientos: abajo, derecha, arriba, izquierda. Entre cada movimiento hay que dejar un
        //margen de error de pasarse en esa dirección. Se usan 4 booleanos para pasar a las diferentes
        //partes del gesto, cuando se esta realizando el movimiento solo hay activo 1 booleano que 
        //será el que le corresponde. Si hemos terminado un movimiento y vamos a empezar otro, habrá
        //dos boleanos activos, el booleano que se ha usado en el movimiento finalizado y el boleano
        //del movimiento que se realizará a continuación. La función devuelve un entero, -1 si ha 
        //habido algun error, 0 si todo va correctamente pero no ha terminado el gesto y 1 cuando
        //se ha reconocido el gesto.
        private int GestoCuadrado2()
        {
            int salida = 0;
            //Movimiento vertical hacia abajo
            if (cuadrado_vertical_ab && !cuadrado_horizontal_d && !cuadrado_vertical_ar && !cuadrado_horizontal_i)
            {
                //Este será el margen de error que debe cumplir con respecto al eje X para que 
                //se siga en este movimiento
                if (Math.Abs(posicion_cuadrado.X - mano.GetPosition(wrapPanel).X) < error)
                {
                    //En cuanto se realice una determinada distancia de movimiento hacia abajo habremos 
                    //realizado el movimiento correctamente
                    if (mano.GetPosition(wrapPanel).Y - posicion_cuadrado.Y > 100)
                    {
                        cuadrado_vertical_ab = false;
                        var converter = new System.Windows.Media.BrushConverter();
                        this.tile.Background = (System.Windows.Media.Brush)converter.ConvertFromString("#FF2E9AFE");
                        this.wrapPanel.Children.Add(new SelectionDisplay("Movimiento hacia abajo completado"));
                    }
                    //Si hemos ido en dirección contraria se interrumpe el gesto
                    if (mano.GetPosition(wrapPanel).Y - posicion_cuadrado.Y < -error)
                    {
                        salida = -1;
                        var converter = new System.Windows.Media.BrushConverter();
                        this.tile.Background = (System.Windows.Media.Brush)converter.ConvertFromString("#FFA51E16");
                        if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Fallido 1 1"));
                    }
                }
                //Si ha sobrepasado el margen de error con respecto al eje x e y se interrumpe
                //el gesto
                else
                {
                    salida = -1;
                    var converter = new System.Windows.Media.BrushConverter();
                    this.tile.Background = (System.Windows.Media.Brush)converter.ConvertFromString("#FFA51E16");
                    if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Fallido 1 2"));
                }
            }

            //Movimiento anterior al horizontal derecha
            if (!cuadrado_vertical_ab && !cuadrado_horizontal_d && !cuadrado_vertical_ar && !cuadrado_horizontal_i)
            {
                //Este será el máximo de margen de error que dejaremos sobrepasar
                if (mano.GetPosition(wrapPanel).Y - posicion_cuadrado.Y > 200)
                {
                    salida = -1;
                    var converter = new System.Windows.Media.BrushConverter();
                    this.tile.Background = (System.Windows.Media.Brush)converter.ConvertFromString("#FFA51E16");
                    if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Fallido 2 1"));
                }
                else
                {
                    //Si se detecta un movimiento en vertical hacia la izquierda se interrumpe
                    //la detección del gesto
                    if (mano.GetPosition(wrapPanel).X - posicion_cuadrado.X < -error)
                    {
                        salida = -1;
                        var converter = new System.Windows.Media.BrushConverter();
                        this.tile.Background = (System.Windows.Media.Brush)converter.ConvertFromString("#FFA51E16");
                        if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Fallido 2 2"));
                    }
                    //Si se ha detectado un ligero movimiento horizontal hacia la derecha podremos
                    //comenzar con el siguiente movimiento y guardamos el nuevo valor que tendrá
                    //posicion_cuadrado (nueva posición inicial del siguiente movimiento)
                    if (mano.GetPosition(wrapPanel).X - posicion_cuadrado.X > error)
                    {
                        cuadrado_horizontal_d = true;
                        posicion_cuadrado = mano.GetPosition(wrapPanel);
                        var converter = new System.Windows.Media.BrushConverter();
                        this.tile.Background = (System.Windows.Media.Brush)converter.ConvertFromString("#FF52318F");
                        this.wrapPanel.Children.Add(new SelectionDisplay("Sigue moviendo la mano hacia la derecha"));
                    }
                }
            }

            //Movimiento horizontal derecha
            if (!cuadrado_vertical_ab && cuadrado_horizontal_d && !cuadrado_vertical_ar && !cuadrado_horizontal_i)
            {
                //Este será el margen de error que debe cumplir con respecto al eje Y
                //para que se siga en este movimiento
                if (Math.Abs(posicion_cuadrado.Y - mano.GetPosition(wrapPanel).Y) < error)
                {
                    //En cuanto se realice una determinada distancia de movimiento hacia la derecha
                    //habremos realizado el movimiento correctamente
                    if (mano.GetPosition(wrapPanel).X - posicion_cuadrado.X > 100)
                    {
                        var converter = new System.Windows.Media.BrushConverter();
                        this.tile.Background = (System.Windows.Media.Brush)converter.ConvertFromString("#FF2E9AFE");
                        this.wrapPanel.Children.Add(new SelectionDisplay("Movimiento hacia la derecha completado"));
                        cuadrado_vertical_ar = true;
                    }
                    //Si hemos ido en dirección contraria se interrumpe el gesto
                    if (mano.GetPosition(wrapPanel).X - posicion_cuadrado.X < -error)
                    {
                        salida = -1;
                        var converter = new System.Windows.Media.BrushConverter();
                        this.tile.Background = (System.Windows.Media.Brush)converter.ConvertFromString("#FFA51E16");
                        if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Error 3 1"));
                    }
                }
                //Si ha sobrepasado el margen de error con respecto al eje x e y se interrumpe
                //el gesto
                else
                {
                    var converter = new System.Windows.Media.BrushConverter();
                    this.tile.Background = (System.Windows.Media.Brush)converter.ConvertFromString("#FFA51E16");
                    if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Error 3 2"));
                    salida = -1;
                }
            }

            //Movimiento vertical hacia arriba
            if (!cuadrado_vertical_ab && cuadrado_horizontal_d && cuadrado_vertical_ar && !cuadrado_horizontal_i)
            {
                //Este será el máximo de margen de error que dejaremos sobrepasar
                if (mano.GetPosition(wrapPanel).X - posicion_cuadrado.X > 200)
                {
                    salida = -1;
                    var converter = new System.Windows.Media.BrushConverter();
                    this.tile.Background = (System.Windows.Media.Brush)converter.ConvertFromString("#FFA51E16");
                    if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Fallido 4 1"));
                }
                else
                {
                    //Si se detecta un movimiento en vertical hacia abajo se interrumpe
                    //la detección del gesto
                    if (mano.GetPosition(wrapPanel).Y - posicion_cuadrado.Y > error)
                    {
                        salida = -1;
                        var converter = new System.Windows.Media.BrushConverter();
                        this.tile.Background = (System.Windows.Media.Brush)converter.ConvertFromString("#FFA51E16");
                        if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Fallido 4 2"));
                    }
                    //Si se ha detectado un ligero movimiento vertical hacia arriba podremos
                    //comenzar con el siguiente movimiento y guardamos el nuevo valor que tendrá
                    //posicion_cuadrado (nueva posición inicial del siguiente movimiento)
                    if (mano.GetPosition(wrapPanel).Y - posicion_cuadrado.Y < -error)
                    {
                        cuadrado_horizontal_d = false;
                        posicion_cuadrado = mano.GetPosition(wrapPanel);
                        var converter = new System.Windows.Media.BrushConverter();
                        this.tile.Background = (System.Windows.Media.Brush)converter.ConvertFromString("#FF52318F");
                        this.wrapPanel.Children.Add(new SelectionDisplay("Sigue moviendo la mano hacia arriba"));
                    }
                }
            }

            //Movimiento vertical hacia arriba
            if (!cuadrado_vertical_ab && !cuadrado_horizontal_d && cuadrado_vertical_ar && !cuadrado_horizontal_i)
            {
                //Este será el margen de error que debe cumplir con respecto al eje X
                //para que se siga en este movimiento
                if (Math.Abs(posicion_cuadrado.X - mano.GetPosition(wrapPanel).X) < error)
                {
                    //En cuanto se realice una determinada distancia de movimiento hacia arriba
                    //habremos realizado el movimiento correctamente
                    if (mano.GetPosition(wrapPanel).Y - posicion_cuadrado.Y < -100)
                    {
                        cuadrado_horizontal_i = true;
                        var converter = new System.Windows.Media.BrushConverter();
                        this.tile.Background = (System.Windows.Media.Brush)converter.ConvertFromString("#FF2E9AFE");
                        this.wrapPanel.Children.Add(new SelectionDisplay("Movimiento hacia arriba completado"));

                    }
                    //Si hemos ido en dirección contraria se interrumpe el gesto
                    if (mano.GetPosition(wrapPanel).Y - posicion_cuadrado.Y > error)
                    {
                        salida = -1;
                        var converter = new System.Windows.Media.BrushConverter();
                        this.tile.Background = (System.Windows.Media.Brush)converter.ConvertFromString("#FFA51E16");
                        if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Fallido 5 1"));
                    }
                }
                //Si ha sobrepasado el margen de error con respecto al eje x e y se interrumpe
                //el gesto
                else
                {
                    salida = -1;
                    var converter = new System.Windows.Media.BrushConverter();
                    this.tile.Background = (System.Windows.Media.Brush)converter.ConvertFromString("#FFA51E16");
                    if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Fallido 5 2"));
                }
            }


            //Movimiento anterior al horizontal izquierda
            if (!cuadrado_vertical_ab && !cuadrado_horizontal_d && cuadrado_vertical_ar && cuadrado_horizontal_i)
            {
                //Este será el máximo de margen de error que dejaremos sobrepasar
                if (mano.GetPosition(wrapPanel).Y - posicion_cuadrado.Y < -200)
                {
                    salida = -1;
                    if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Fallido 6 1"));
                }
                else
                {
                    //Si se detecta un movimiento en vertical hacia la derecha se interrumpe
                    //la detección del gesto
                    if (mano.GetPosition(wrapPanel).X - posicion_cuadrado.X > error)
                    {
                        salida = -1;
                        if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Fallido 6 2"));
                    }
                    //Si se ha detectado un ligero movimiento horizontal hacia la izquierda podremos
                    //comenzar con el siguiente movimiento y guardamos el nuevo valor que tendrá
                    //posicion_cuadrado (nueva posición inicial del siguiente movimiento)
                    if (mano.GetPosition(wrapPanel).X - posicion_cuadrado.X < -error)
                    {
                        cuadrado_vertical_ar = false;
                        posicion_cuadrado = mano.GetPosition(wrapPanel);
                        var converter = new System.Windows.Media.BrushConverter();
                        this.tile.Background = (System.Windows.Media.Brush)converter.ConvertFromString("#FF52318F");
                        this.wrapPanel.Children.Add(new SelectionDisplay("Sigue moviendo la mano hacia la izquierda"));
                    }
                }
            }

            //Movimiento horizontal izquierda
            if (!cuadrado_vertical_ab && !cuadrado_horizontal_d && !cuadrado_vertical_ar && cuadrado_horizontal_i)
            {
                //Este será el margen de error que debe cumplir con respecto al eje Y
                if (Math.Abs(posicion_cuadrado.Y - mano.GetPosition(wrapPanel).Y) < error)
                {
                    //En cuanto se realice una determinada distancia de movimiento hacia la izquierda
                    //habremos realizado el movimiento correctamente
                    if (mano.GetPosition(wrapPanel).X - posicion_cuadrado.X < -100)
                    {
                        salida = 1;
                        this.wrapPanel.Children.Add(new SelectionDisplay("Movimiento hacia la izquierda completado"));
                        cuadrado_vertical_ar = true;
                    }
                    //Si hemos ido en dirección contraria se interrumpe el gesto
                    if (mano.GetPosition(wrapPanel).X - posicion_cuadrado.X > error)
                    {
                        salida = -1;
                        if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Error 7 1"));
                    }
                }
                //Si ha sobrepasado el margen de error con respecto al eje x e y se interrumpe
                //el gesto
                else
                {
                    if (debug) this.wrapPanel.Children.Add(new SelectionDisplay("Error 7 2"));
                    salida = -1;
                }
            }
            return salida;
        }

        //Esta función será la que irá comprobando si el gesto se ha realizado correctamente
        private void comprobarGesto(object sender, RoutedEventArgs e)
        {
            //La mano tiene que estar cerrada para que se pueda comprobar que se ha realizado el gesto
            if (puno){
                //Como hay dos formas de realizar el gesto, comenzando por la derecha o por abajo,
                //por defecto se han dejado las variables preparadas para ir hacia la derecha
                //pero en el caso de que haya un ligero movimiento hacia abajo se cambian todas
                //estas variables de forma que el gesto que se vaya a a tener en cuenta sea
                //el que empieza con el primer movimiento hacia abajo.
                if (cuadrado_horizontal_d && !cuadrado_horizontal_i && !cuadrado_vertical_ar && !cuadrado_vertical_ab && (mano.GetPosition(wrapPanel).Y - posicion_cuadrado.Y > error/2))
                {
                    cuadrado_horizontal_d = false;
                    cuadrado_horizontal_i = false;
                    cuadrado_vertical_ar = false;
                    cuadrado_vertical_ab = true;
                    cambioGesto = true;
                }

                if (resultado[0] != -1 && !cambioGesto) resultado[0] = GestoCuadrado1();
                if (resultado[1] != -1 && cambioGesto) resultado[1] = GestoCuadrado2();

                //Cuando uno de los dos gestos se ha completado, cambiaremos el color del fondo
                //y se mostrará un mensaje
                if (resultado[0] == 1 || resultado[1] == 1)
                {
                    var converter = new System.Windows.Media.BrushConverter();
                    this.tile.Background = (System.Windows.Media.Brush)converter.ConvertFromString("#FF0A501C");
                    puno = false;
                    this.wrapPanel.Children.Add(new SelectionDisplay("Se ha reconocido el gesto cuadrado"));
                } 

                //Si el gesto ha fallado en algún momento la mano se forzará a estar abierta para que
                //se comience de nuevo el gesto
                if (resultado[0] == -1 && resultado[1] == -1)
                {
                    puno = false;
                    var converter = new System.Windows.Media.BrushConverter();
                    this.tile.Background = (System.Windows.Media.Brush)converter.ConvertFromString("#FF52318F");
                }
                
            }
        }
    

        //Funciones tomadas para la visualización de la imagen real (color) y el esqueleto
        private static void RenderClippedEdges(Skeleton skeleton, DrawingContext drawingContext)
        {
            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Bottom))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, RenderHeight - ClipBoundsThickness, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Top))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Left))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, ClipBoundsThickness, RenderHeight));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Right))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(RenderWidth - ClipBoundsThickness, 0, ClipBoundsThickness, RenderHeight));
            }
        }

        /// <summary>
        /// Execute startup tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            // Create the drawing group we'll use for drawing
            this.drawingGroup = new DrawingGroup();

            // Create an image source that we can use in our image control
            this.imageSource = new DrawingImage(this.drawingGroup);

            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (this.sensor != null)
            {
                // Display the drawing using our image control
                this.Image.Source = this.imageSource;

                // Turn on the skeleton stream to receive skeleton frames
                this.sensor.SkeletonStream.Enable();

                // Add an event handler to be called whenever there is new color frame data
                this.sensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;


                // Turn on the color stream to receive color frames
                this.sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

                // Allocate space to put the pixels we'll receive
                this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];

                // This is the bitmap we'll display on-screen
                this.colorBitmap = new WriteableBitmap(this.sensor.ColorStream.FrameWidth, this.sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

                // Set the image we display to point to the bitmap where we'll put the image data
                this.ImageColor.Source = this.colorBitmap;

                // Add an event handler to be called whenever there is new color frame data
                this.sensor.ColorFrameReady += this.SensorColorFrameReady;

                // Start the sensor!
                try
                {
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }


        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>


        /// <summary>
        /// Event handler for Kinect sensor's ColorFrameReady event
        /// Se añade la funcion para mostrar la imagen en color captada por la kinect
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {
                    // Copy the pixel data from the image to a temporary array
                    colorFrame.CopyPixelDataTo(this.colorPixels);

                    // Write the pixel data into our bitmap
                    this.colorBitmap.WritePixels(
                        new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                        this.colorPixels,
                        this.colorBitmap.PixelWidth * sizeof(int),
                        0);
                }
            }
        }

        /// <summary>
        /// Event handler for Kinect sensor's SkeletonFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skeletons = new Skeleton[0];

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                }
            }

            using (DrawingContext dc = this.drawingGroup.Open())
            {
                // Draw a transparent background to set the render size
                dc.DrawRectangle(Brushes.Transparent, null, new Rect(0.0, 0.0, RenderWidth, RenderHeight));

                if (skeletons.Length != 0)
                {
                    foreach (Skeleton skel in skeletons)
                    {
                        RenderClippedEdges(skel, dc);

                        if (skel.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            this.DrawBonesAndJoints(skel, dc);
                        }
                        else if (skel.TrackingState == SkeletonTrackingState.PositionOnly)
                        {
                            dc.DrawEllipse(
                            this.centerPointBrush,
                            null,
                            this.SkeletonPointToScreen(skel.Position),
                            BodyCenterThickness,
                            BodyCenterThickness);
                        }
                    }
                }

                // prevent drawing outside of our render area
                this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));
            }
        }
        /// <summary>
        /// Draws a skeleton's bones and joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private void DrawBonesAndJoints(Skeleton skeleton, DrawingContext drawingContext)
        {
            // Render Torso
            this.DrawBone(skeleton, drawingContext, JointType.Head, JointType.ShoulderCenter);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderRight);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.Spine);
            this.DrawBone(skeleton, drawingContext, JointType.Spine, JointType.HipCenter);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipLeft);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipRight);

            // Left Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderLeft, JointType.ElbowLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowLeft, JointType.WristLeft);
            this.DrawBone(skeleton, drawingContext, JointType.WristLeft, JointType.HandLeft);

            // Right Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderRight, JointType.ElbowRight);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowRight, JointType.WristRight);
            this.DrawBone(skeleton, drawingContext, JointType.WristRight, JointType.HandRight);

            // Left Leg
            this.DrawBone(skeleton, drawingContext, JointType.HipLeft, JointType.KneeLeft);
            this.DrawBone(skeleton, drawingContext, JointType.KneeLeft, JointType.AnkleLeft);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleLeft, JointType.FootLeft);

            // Right Leg
            this.DrawBone(skeleton, drawingContext, JointType.HipRight, JointType.KneeRight);
            this.DrawBone(skeleton, drawingContext, JointType.KneeRight, JointType.AnkleRight);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleRight, JointType.FootRight);


            // Render Joints
            foreach (Joint joint in skeleton.Joints)
            {
                Brush drawBrush = null;

                if (joint.TrackingState == JointTrackingState.Tracked)
                {
                    drawBrush = this.trackedJointBrush;
                }
                else if (joint.TrackingState == JointTrackingState.Inferred)
                {
                    drawBrush = this.inferredJointBrush;
                }

                if (drawBrush != null)
                {
                    drawingContext.DrawEllipse(drawBrush, null, this.SkeletonPointToScreen(joint.Position), JointThickness, JointThickness);
                }
            }
        }

        /// <summary>
        /// Maps a SkeletonPoint to lie within our render space and converts to Point
        /// </summary>
        /// <param name="skelpoint">point to map</param>
        /// <returns>mapped point</returns>
        private Point SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
            // Convert point to depth space.  
            // We are not using depth directly, but we do want the points in our 640x480 output resolution.
            DepthImagePoint depthPoint = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
            return new Point(depthPoint.X, depthPoint.Y);
        }

        /// <summary>
        /// Draws a bone line between two joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw bones from</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// <param name="jointType0">joint to start drawing from</param>
        /// <param name="jointType1">joint to end drawing at</param>
        private void DrawBone(Skeleton skeleton, DrawingContext drawingContext, JointType jointType0, JointType jointType1)
        {
            Joint joint0 = skeleton.Joints[jointType0];
            Joint joint1 = skeleton.Joints[jointType1];

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == JointTrackingState.NotTracked ||
                joint1.TrackingState == JointTrackingState.NotTracked)
            {
                return;
            }

            // Don't draw if both points are inferred
            if (joint0.TrackingState == JointTrackingState.Inferred &&
                joint1.TrackingState == JointTrackingState.Inferred)
            {
                return;
            }

            // We assume all drawn bones are inferred unless BOTH joints are tracked
            Pen drawPen = this.inferredBonePen;
            if (joint0.TrackingState == JointTrackingState.Tracked && joint1.TrackingState == JointTrackingState.Tracked)
            {
                drawPen = this.trackedBonePen;
            }

            drawingContext.DrawLine(drawPen, this.SkeletonPointToScreen(joint0.Position), this.SkeletonPointToScreen(joint1.Position));
        }

        //Comprueba en que estado se encuentra el ejercicio

        private void tile_Click(object sender, RoutedEventArgs e)
        {

        }



    }  
}
