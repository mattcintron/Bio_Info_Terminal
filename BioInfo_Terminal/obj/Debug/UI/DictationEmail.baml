   M S B A M L   `   `   `  ����  BioInfo_Terminal(    "BioInfo_Terminal.UI.DictationEmail   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	     � Title$  Email Dictation��5   	   $��444.093��6!   $	��595��62   ���	#FF020A1D � Icon$6 0pack://siteoforigin:,,,/Resources/brain_icon.ico��5   	   .��5	      �  � RowDefinitions 5
   
   �� 5      $��9* �6   �� 5      $
��121* �6   .+�5   
   �� $��   Email :        ��6   $��6,4,5,0q�6-   ���	#FFEEEEEE���	#FF0E2847$��11(�5   '   $��32��65   $	��Top=�6A   $��2��6Y   �� 5   
   -   5         ��Name$ 	BtnSubmit��5      $
��Send��6$   $��0,0,6,2q�63   $��9(�5      $��Right=�5      $��99��6-   $��20��6S   $��Bottom=�5      $��1��5      �� 5   
   -   5         ��Name$ TbDictation��5      $��	5,47,6,26q�6'    �� TextWrapping$
 Wrap=�6:   $�� ��5      ���	#E8FFFFFF$��1��61    �� AcceptsReturn .  �� 
AcceptsTab . 6S   �� 5   
   -   5        $ TbEmailAddress��5      $��21��6*   $��
59,10,10,0q�66   $
 Wrap=�6J   $�� ��5      $	��Top=�6   ���	#DBFFFFFF$��0��6I   $��2��6V   .��6g   ��5      �� 5       �� EndPoint$
 0,20&�6&   	 � MappingMode$	 Absolute=�66   
 �� 
StartPoint$	
 0,0&�6M   .��5      �  � Color$ 	#FFABADB3��6#   � 5      $ 	#FFE2E3EA��6#   � 5      $ 	#00E3E9EF��6#   5      .��5   #   5       �� 5!   
   -   5         ��Name$ 
LblSubject��5!      $��   Subject :         ��6$   $��6,12,5,0q�6D   $	��Top=�6V   ���	#FFEEEEEE���	#FF0E2847$��30��5#   '   $��10(�63   $��1��6A   �� 5$   
   -   5        $ 	TbSubject��5$      $��24��6%   $��
59,15,10,0q�61   $
 Wrap=�6E   $�� ��5%      $	��Top=�6   ���	#DBFFFFFF$��1��6I   .��6V   ��5&      �� 5'      $
 0,20&�6&   $	 Absolute=�66   $	
 0,0&�6M   .��5(      � $ 	#FFABADB3��6#   � 5)      $ 	#FFE2E3EA��6#   � 5*      $ 	#00E3E9EF��6#   5+      .��5,   #   5-      �� 5.   
   -   5        $ BtnStartRecording��5.      $��Start Recording��6,   $��6,0,0,2q�6F   $��9(�5/      $��1��50      ���	#FF7AE65B$
��Left=�65   $��70��6P   $��20��6[   $��Bottom=�51      6,   �� 52   
   -   5        $ BtnStopRecording��52      $��Stop Recording��6+   $��94,0,0,2q�6D   $��9(�53      $
��Left=�54      $��70��6,   $��20��67   $��Bottom=�6C   $��1��55      ���	#FFE4363665   57      58      