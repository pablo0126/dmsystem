   M S B A M L   `   `   `  ����R  NZhaoxi.CourseManagement, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullO,clr-namespace:DataMonitoringSystem.ViewModelDataMonitoringSystem.ViewModel  5      J FZhaoxi.Controls, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullJ6clr-namespace:Zhaoxi.Controls;assembly=Zhaoxi.ControlsZhaoxi.Controls 5      I)clr-namespace:DataMonitoringSystem.CommonDataMonitoringSystem.Common  5      .    (DataMonitoringSystem.View.CoursePageView   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      >local,clr-namespace:DataMonitoringSystem.ViewModel     5      Fzxc6clr-namespace:Zhaoxi.Controls;assembly=Zhaoxi.Controls     5      :comm)clr-namespace:DataMonitoringSystem.Common     5        � 	Resources  5       �� 
TargetType   CategoryItemButtonTemplate �� CornerRadius �� Triggers P� Property P� Value �� 
TargetName �� Property �� Value  courseTemplate	 � ColumnDefinitions
 J� Effect3  -System.Windows.Media.Effects.DropShadowEffect   Color   ShadowDepth   
BlurRadius   Opacity   	Direction j� Stretch �� ImageSource �� Orientation �� Command �� RelativeSource �� AncestorType �� Mode �� CommandParameter �� Triggers }� Location $� Orientation �� 
LineHeight  skeletonTemplate$  Zhaoxi.Controls.SkeletonScreen��% 	  &	        &	 z    &	 �    �� 5   
   " 	�6   .R�5      � 
������ .+�5      �� $
��back��6<   ���#EEE$ 5��6+   ._�5      �� #��z��6    $
��15,4q�6A   $��Center=�6O   $��Center=�6j   5      5       5      P� 5       �@d�	IsChecked$
 TrueY�6/   . �5      �� $
 back��6B    � ��
 � ����� 5       � ��
 �����63   5      5      �� 5   
   .R�5      � .+�5      � $��courseContent��6   	 5      �� 5      $	��150 �6+   �� 5      �� 5       $	��100 �6+   .+�5"      �� $��90��6   $��60��6(   $ 5��64   $
��0,15q�6E   
 5#        5$      $
 Gray��6/   $ 0Q�6<   $ 10Q�6L   $	 0.3Q�6\   $ 0Q�6j   ��5&      �� 5'      $ UniformToFill=�6G    6)   �� *Cover+6G   5)      �� 5*      $��1��6!   $��Center=�61   .+�5+      �� $ 
Horizontal=�6%   $��0,0,0,10q�6>   .+�5,      �� $��16(�6(   �� 5-   "   ���#333 5.   ,   �� *" DataContext.OpenCourseUrlCommand+ �� $ UserControlL�$ FindAncestor=� 5/   ,   �� *Url+.�6M   ��50   &   �� 51   *   .�52   .   ��  � �� 6Z   Z�  53   .   P� 54   2    �@��IsMouseOver . . �55   6   ��  � �� 56   :   |� 57   >   }� 58   B   $ 	Underline=�6Q   59   ?   5;   7   5<   3   5>   +   .�5?   7   .�5@   &   �� ��60   �� *
CourseName+6O   5A      �� 5B      $
��15,0q�6+   ��69   �� *
Teachers+��5C   "   �� 5D   &   .R�5E   *   $� $ 
Horizontal=�6A   5F   '   ��5H   "   �� 5I   &   .R�5J   *   �� ���#EEE$	��5,0q�6C   $ 5��6P   ._�5K   .   �� $
��12,3q�6I   ��68   �� 6I   5L   +   5M   '   5O      5P      �� 5Q      $
��Wrap=�5R   $   $ 23��68   ���#AAA��5Q   $   �� *Description+5R   H   5S      �� 5T      $ 
Horizontal=�6!   $��2��6:   $��Center=�6J   $��Center=�6g   .+�5U      �� $	��5,0q�6$   �� 5V       6(   �� *DataContext.DetailCommand+ �� $ UserControlL�$ FindAncestor=� 6�   �� *
CourseID+.�6�   详情6�   5W      �� 5X      $	��5,0q�6$   �� 5Y      .�6(   删除6,   5Z      5[      5\      5]      5^      �� 5_   
   .R�5`        $��skeleton��6!   5a      5b      .��5c      �� $ 5��6   
�������$��10q�61   ._�5d   
   �  � RowDefinitions 5e      �� 5f      $
��auto �6    �� 5g      .+�5i      �� ���#DDD$��0,0,0,1q�6(   $
��20,0q�6B   ._�5j      � $��10,18q�6    5k      �� 5l      $��35��6(   $
��auto �67   �� 5m      $��35��6(   $
��auto �67   �� 5n      $��35��6(   $
��auto �67   .+�5s      �� $ 
Horizontal=�6!   $��2��6:   $��Center=�6G   .+�5t      �� $��授课老师��6$   $��Center=�60   $��0,0,10,0q�6K   ���#6666]   �� 5u      ��6'   �� *CategoryTeacher+��5v      �� 5w   "   .R�5x   &   �� $ 
Horizontal=�61   5y   #   ��5{      �� 5|   "   .R�5�   &   	� -   5        #��[  5�   t   $	��5,0q�6�    	� 	GroupName$ teacher��5�   2   ��5�   2   �� *CategoryName+ d� 	IsChecked 6S   �� *
IsSelected+5�   2   5�   #   5�      5�      �� 5�      $��2��6   $��+ 添加课程��6*   $	��100��6;   $��30��6G   $��Right=�6S   $��Center=�6o   5�      5�      �� 5�      $��1��6   $��Hidden=�6(   .��5�      �� -   5        $��	icCourses��5�   B   ��6   �� *
CourseList+��5�      �� 5�      .R�5�      $� 5�      ��5�      <   6DataMonitoringSystem.Common.CourseDataTemplateSelector  5�         DefaultTempalte# [ 6:       SkeletonTemplate#  [ 5�   :   5�      5�      5�      5�      5�      