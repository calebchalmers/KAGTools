﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace KAGTools.Windows
{
    /// <summary>
    /// Interaction logic for ApiWindow.xaml
    /// </summary>
    public partial class ApiWindow : Window
    {
        public ApiWindow()
        {
            InitializeComponent();

            serverList.Items.SortDescriptions.Add(new SortDescription("PlayerCount", ListSortDirection.Descending));
        }
    }
}
