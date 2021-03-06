﻿using System;
using GameLib;
using SolverLib;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Util;

namespace NonogramSolver
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly BackgroundWorker _solveBW = new BackgroundWorker();
        private readonly BackgroundWorker _fillBW = new BackgroundWorker();
        private const int Waittime = 10;
        private bool _running;
        private List<Result> _resultQueue;
        public MainWindow()
        {
            InitializeComponent();

            _solveBW.WorkerReportsProgress = false;
            _solveBW.RunWorkerCompleted += _solveBW_RunWorkerCompleted;
            _solveBW.DoWork += _solveBW_DoWork;

            _fillBW.WorkerReportsProgress = true;
            _fillBW.RunWorkerCompleted += _fillBW_RunWorkerCompleted;
            _fillBW.ProgressChanged += _fillBW_ProgressChanged;
            _fillBW.DoWork += _fillBW_DoWork;
        }

        /// <summary>
        /// Event handler that gets called when a nonogram text file gets dropped.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_Drop(object sender, DragEventArgs e)
        {
            if (_running) return;
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            _running = true;
            try
            {
                Nonogram ng = NonoGramFactory.ParseFromFile(((DataObject) e.Data).GetFileDropList()[0]);
                MakeGrid(ng);
                stateBox.Text = "Solving...";
                _solveBW.RunWorkerAsync(ng);
            }
            catch (ArgumentException ex)
            {
                stateBox.Text = ex.Message;
                _running = false;
            }
            catch (Exception)
            {
                stateBox.Text = "Unexpected exception while solving";
                _running = false;
            }
            e.Handled = true;
        }

        /// <summary>
        /// Sets up a new grid in the window based on the given nonogram
        /// </summary>
        /// <param name="ng">Nonogram to solve</param>
        private void MakeGrid(Nonogram ng)
        {
            wGrid.Children.Clear();
            wGrid.RowDefinitions.Clear();
            wGrid.ColumnDefinitions.Clear();
            GridLength gl = new GridLength(15);
            for (int i = 0; i < ng.Width; i++)
            {
                ColumnDefinition cd = new ColumnDefinition {Width = gl};
                wGrid.ColumnDefinitions.Add(cd);
            }
            
            for (int i = 0; i < ng.Height; i++)
            {
                RowDefinition rd = new RowDefinition {Height = gl};
                wGrid.RowDefinitions.Add(rd);
            }
        }

        /// <summary>
        /// BackgroundWorker thread for keeping the window responsive while solving.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _solveBW_DoWork(object sender, DoWorkEventArgs e)
        {
            ISolver ss = new SerialSolver();
            ss.Run((Nonogram)e.Argument);
            e.Result = ss;
        }

        /// <summary>
        /// Sets the status text for the window and calls the drawing thread.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _solveBW_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ISolver solver = (ISolver)e.Result;
            if (solver.Solved())
            {
                stateBox.Text = "Solved in " + solver.BenchTime().TotalMilliseconds + "ms.";
                _resultQueue = solver.Results();
                _fillBW.RunWorkerAsync(_resultQueue.Count);
            }
            else
            {
                stateBox.Text = "Solving failed. Time: " + solver.BenchTime().TotalMilliseconds + "ms.";
                _resultQueue = solver.Results();
                _fillBW.RunWorkerAsync(_resultQueue.Count);
            }
        }

        /// <summary>
        /// BackgroundWorer thread for keeping the window responsive while drawing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _fillBW_DoWork(object sender, DoWorkEventArgs e)
        {
            int resLen = (int)e.Argument;
            for (int i = 0; i < resLen; i++)
            {
                _fillBW.ReportProgress(i);
                Thread.Sleep(Waittime);
            }
        }

        /// <summary>
        /// Draws the next resolved tile
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _fillBW_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Result res = _resultQueue[e.ProgressPercentage];
            Rectangle r = new Rectangle {Fill = res.State ? Brushes.Black : Brushes.White};
            Grid.SetColumn(r, res.Column);
            Grid.SetRow(r, res.Row);
            wGrid.Children.Add(r);
        }

        /// <summary>
        /// Just sets dunning to false so that a new nonogram can be added.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _fillBW_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _running = false;
        }

        /// <summary>
        /// Event handler of cursor control of drag/drop event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_PreviewDrag(object sender, DragEventArgs e)
        {
            if (_running)
            {
                e.Effects = DragDropEffects.None;
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            e.Handled = true;
        }
    }
}
