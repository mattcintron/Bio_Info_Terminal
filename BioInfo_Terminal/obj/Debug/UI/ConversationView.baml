   M S B A M L   `   `   `  ����K  GBioInfo_Terminal, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null3clr-namespace:BioInfo_TerminalBioInfo_Terminal  5      W0clr-namespace:BioInfo_Terminal.Methods.Messaging"BioInfo_Terminal.Methods.Messaging  5      9!clr-namespace:BioInfo_Terminal.UIBioInfo_Terminal.UI  5      *    $BioInfo_Terminal.UI.ConversationView   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      0localclr-namespace:BioInfo_Terminal     5      F	messaging0clr-namespace:BioInfo_Terminal.Methods.Messaging     5      0ui!clr-namespace:BioInfo_Terminal.UI     5        � 	Resources  5      >   8BioInfo_Terminal.Methods.Messaging.StringFormatConverter   StringFormatConverter �� 
TargetType  TextBlockStyle �� Property �� Value  TimestampStyle  A#   BioInfo_Terminal.UI.GridUtils   RowDefinitions �� Path �� 	Converter �� ConverterParameter  B  C  D��%/	  &	        &	      &	 M    &	 �    7  7  7   &	 �    7  7  7   &	 G    7  7  7   &	     7  7  7     5   
   6*   �� 5   
   " ��6   .�5      ��  � ��
 �   �6+   �� 5       � ��
 ���� 6+   �� 5       � ��$ 5q�6'   �� 5       �@��TextWrapping$
 Wrap=�6-   �� 5       � ��$ 12(�6)   �� 5       � ��$ 0q�60   �� 5       �@��
IsReadOnly . 6+   5      �� 5   
   " ��6   .�5      ��  � ��
 � � �6+   �� 5       � ��$ 5q�6'   �� 5       � ��$ 12(�6)   5      �� 5!   
   .R�5"      � $��30, 30, 5, 0q�6   $ ,,��6)   .+�5#      �� ���	#FFDAE05A$��2��5$      �� 5%      8��  ��6   �� $
 Text�5&      �� 5'      8�� $��1��5*      ��5(      �� $ 	Timestamp�8  $ 
ddd, HH:mm��5*      F� 5+      +����        � $�     � $�  $� �        	���	#FFE0DA5A$��0,-1,5,0q�5-      $��Right=�5.      $��2��5/      50      51      �� 53   
   .R�54      � $��5, 0, 30, 0q�6   $ ,,��6(   .+�55      F� +����        �     $� � $�  $� �        	���	#FF41E6DF$��5,0,0,-1q�57      $
��Left=�58      �� 59      ���	#FF41E6DF$��1��5:      $��2��6)   �� 5;      8��  $��1��5=      ��5;      �� $
 Text�5=      �� 5>      8�� $��2��5A      ��5?      �� $ 	Timestamp�8  $ 
ddd, HH:mm��5A      5B      5C      �� 5E   
   .R�5F      � $��5, 15, 30, 0q�6   $ ,,��5G      .+�5H      F� +����        �     $� � $�  $� �        	���	#FF41E6DF$��5,0,0,-1q�5J      $
��Left=�5K      �� 5L      ���	#FF41E6C8$��1��5M      $��2��6)   �� 5N      8��  $��1��5P      ��5N      �� $
 Text�5P      �� 5Q      8�� $��2��5T      ��5R      �� $ 	Timestamp�8  $ 
ddd, HH:mm��5T      5U      5V      �� 5X   
   .R�5Y      � $��30, 15, 5, 0q�6   $ ,,��5Z      .+�5[      �� ���	#FFDAE05A$��2��5\      �� 5]      8��  ��6   �� $
 Text�5^      �� 5_      8�� $��1��5b      ��5`      �� $ 	Timestamp�8  $ 
ddd, HH:mm��5b      F� 5c      +����        � $�     � $�  $� �        	���	#FFDAE05A$��0,-1,5,0q�5e      $��Right=�5f      $��2��5g      5h      5i      5j      .��5l      � -   5         �Name$ 
LayoutRoot��5l      
���   �.+�5m   
   �� ��6   �� ��5n      @   :BioInfo_Terminal.Methods.Messaging.MessageTemplateSelector  5o      	   A#	 [ 5p      
   B#
 [ 5q         C# [ 5r         D# [ 5s      5u      5v      5x      