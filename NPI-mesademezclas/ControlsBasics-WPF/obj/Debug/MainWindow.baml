   M S B A M L   `   `   `  ����M  IControlsBasics-WPF, Version=0.0.0.0, Culture=neutral, PublicKeyToken=nulla5clr-namespace:Microsoft.Samples.Kinect.ControlsBasics'Microsoft.Samples.Kinect.ControlsBasics  5   	   8    2Microsoft.Samples.Kinect.ControlsBasics.MainWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   \ XMicrosoft.Kinect.Toolkit.Controls, Version=0.0.0.0, Culture=neutral, PublicKeyToken=nullS OMicrosoft.Kinect.Toolkit, Version=1.8.0.0, Culture=neutral, PublicKeyToken=null2k(http://schemas.microsoft.com/kinect/2013   5   	   <d2http://schemas.microsoft.com/expression/blend/2008   5   	   Fmc;http://schemas.openxmlformats.org/markup-compatibility/2006   5   	   Clocal5clr-namespace:Microsoft.Samples.Kinect.ControlsBasics   5   	   -   5        $��ControlsBasicsWindow��5   	     � Title$  Controls Basics��5	   	   $	��588��5   	   $	��588��5   	    � 
ResizeMode$ NoResize=�5   	   $��Segoe UI)�5   	   $
��1100��6   $	��800��6/    � WindowStartupLocation$ CenterScreen=�6@   $��753.731��6e   .��5      � .+�5   
   � .+�5      4  .Microsoft.Kinect.Toolkit.Controls.KinectRegion  -   5          Name$ kinectRegion��5      $��-11,0,11,-33q�63   .��5      � -   5         �Name$ 	wrapPanel��5   8   $��Center=�6    � Orientation$ Vertical=�6K   .+�5      8  2Microsoft.Kinect.Toolkit.Controls.KinectTileButton  -   5          Name$
 tile��5   )   $	��728��6K   $
��1079��6X   
���� ����5      �� 5       �� ImageSource$ Images/imagenbase.png��6)   .��5      � .+�5       � $��10,4,-10,-4q�6#   .+�5!   "   �� -   5        $��song1��5!   ,   $��Century Gothic)�69   
��� ���$��31,20,829,599q�6i   6�   5"      � 5#      $��10,4,-10,-4q�6#   .+�5$   "   �� -   5        $��time1��5$   ,   $��Century Gothic)�69   
��� ���$��228,20,751,599q�6i   6�   5%      � 5&      $��10,4,-10,-4q�6#   .+�5'   "   �� -   5        $��song2��5'   ,   $��Century Gothic)�69   
����  �$��719,11,150,599q�6g   6�   5(      � 5)      $��10,4,-10,-4q�6#   .+�5*   "   �� -   5        $��time2��5*   ,   $��Century Gothic)�69   
����  �$��938,17,41,599q�6g   6�   5+      �� 5.      -	   5         ��Name$ myCanvas��5.   %   .+�5/   "   �� -
   5        	 ��Name$	 central��5/   (   $��Bottom=�6:   $��Images\botóncentro.png��6U   $
��Fill=�6u   $��80��6�   $��45��6�   
 �� Left$	
 518��6�   
 �� Top$	 599��6�   50      �� 52   "   -   5        $	 vinilo1��52   (   $��Stretch=�69   $��Stretch=�6W    J� RenderTransformOrigin$ .5,.5&�6t   $��Images\disco1.png��6�   $
��Fill=�6�   $	��400��6�   $	��400��6�   $
 9��6�   $ 41��6�   $��4,70,675,189q�6�   �� 53   "   -   5        $	 vinilo2��53   (   $��Bottom=�69   $��Images\disco2.png��6T   $
��Fill=�6o   $	��400��6~   $	��400��6�   $	
 670��6�   $ 45��6�   $��663,0,4,189q�6�   � 55      $��4,25,-4,-25q�6#   .+�57   "   �� -   5        $	 play_1��57   (   $��Bottom=�69   $��Images\play1.png��6T   $
��Fill=�6n   $��130,0,806,-2q�6~   $	��110��6�   $	��137��6�   �� 58   "   -   5        $	 fastforward_1��58   (   $��Bottom=�6@   $��Images\fastforward1.png��6[   $
��Fill=�6|   $��266,0,670,-3q�6�   $	��110��6�   $	��137��6�   �� 59   "   -   5        $	 fastprevious_1��59   (   $��Bottom=�6A   $��Images\fastprevious1.png��6\   $
��Fill=�6~   $��-5,0,941,-1q�6�   $	��110��6�   $	��137��6�   �� 5:   "   -   5        $	 loop_1��5:   (   $��Bottom=�69   $��Images\loop.png��6T   $
��Fill=�6m   $��358,0,621,126q�6}   $	��100��6�   $	��100��6�   �� 5;   "   -   5        $	 barra_horizontal1��5;   (   $��Bottom=�6D   $ ��Images\barrahorizontal.png��6_   $
��Fill=�6�   $��-10,0,689,68q�6�   $	��200��6�   $	��400��6�   �� 5<   "   -   5        $	 	masmenos1��5<   (   $��Bottom=�6<   $��Images\masmenos.png��6W   $
��Fill=�6t   $��400,0,564,233q�6�   $	��350��6�   $	��100��6�   �� 5=   "   .+�5>   &   �� -   5        $	 central1��5>   ,   $��Bottom=�6?   $��Images\botóncentro.png��6Z   $
��Fill=�6z   $��80��6�   $��40��6�   $	
 165��6�   $	 459��6�   5?   #   �� 5A   "   -   5        $	 play_2��5A   (   $��Bottom=�6:   $��Images\play2.png��6U   $
��Fill=�6o   $��804,0,132,-2q�6   $	��110��6�   $	��137��6�   �� 5B   "   -   5        $	 fastforward_2��5B   (   $��Bottom=�6A   $��Images\fastforward2.png��6\   $
��Fill=�6}   $��940,0,-4,-2q�6�   $	��110��6�   $	��137��6�   �� 5C   "   -   5        $	 fastprevious_2��5C   (   $��Bottom=�6A   $��Images\fastprevious2.png��6\   $
��Fill=�6~   $��668,0,268,-1q�6�   $	��110��6�   $	��137��6�   �� 5D   "   -   5        $	 loop_2��5D   (   $��Bottom=�69   $��Images\loop.png��6T   $
��Fill=�6m   $��599,0,380,126q�6}   $	��100��6�   $	��100��6�   �� 5E   "   -   5        $	 barra_horizontal2��5E   (   $��Bottom=�6D   $ ��Images\barrahorizontal.png��6_   $
��Fill=�6�   $��
675,0,4,68q�6�   $	��200��6�   $	��400��6�   �� 5F   "   -   5        $	 	masmenos2��5F   (   $��Bottom=�6<   $��Images\masmenos.png��6W   $
��Fill=�6t   $��535,0,389,233q�6�   $	��350��6�   $	��100��6�   �� 5H   "   .+�5I   &   �� -   5        $	 central2��5I   ,   $��Bottom=�6?   $��Images\botóncentro.png��6Z   $
��Fill=�6z   $��80��6�   $��40��6�   $	
 854��6�   $	 457��6�   5J   #   �� 5K   "   -   5        $	 restore��5K   (   $y�Hidden=�69   $��Bottom=�6M   $��Images\restore.png��6h   $
��Fill=�6�   $��487,-41,492,600q�6�   $	��100��6�   $	��100��6�   �� 5L   "   -   5        $
	 exit��5L   (   $y�Hidden=�66   $��Bottom=�6K   $��Images\cruz.png��6f   $
��Fill=�6   $��1013,-60,6,659q�6�   $��60��6�   $��60��6�   5N      � 5O      $��10,4,-10,-4q�6#   .+�5P   "   �� -   5        $��rate_1��5P   ,   $��Century Gothic)�6:   
���   �$��442,247,566,372q�6i   16�   5Q      � 5R      $��10,4,-10,-4q�6#   .+�5S   "   �� -   5        $��rate_2��5S   ,   $��Century Gothic)�6:   
���   �$��596,246,413,373q�6i   16�   5T      5V      5W      (� 5X      $��1��6    (� Stretch$ Uniform=�6+   $��Center=�6=   5Y      5Z      5\      5`      5b      