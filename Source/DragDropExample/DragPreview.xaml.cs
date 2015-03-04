using DragDrop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DragDropExample
{
    /// <summary>
    /// Interaction logic for DragPreview.xaml
    /// </summary>
    public partial class DragPreview : DragDropPreviewBase
    {
        public DragPreview()
        {
            InitializeComponent();                        
        }

        public override void StateChangedHandler(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (DragPreview)d;

            //Do custom code-behind things here
            switch ((DropState)e.NewValue)
            {
                case DropState.CanDrop:
                    
                    break;
                case DropState.CannotDrop:
                   
                    break;
            }
        }
    }
}
