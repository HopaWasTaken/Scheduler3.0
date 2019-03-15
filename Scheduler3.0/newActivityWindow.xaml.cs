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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Scheduler3._0
{
    public partial class newActivityWindow : Window
    {

        bool isEditing = false;
        MainWindow.Activity activity = new MainWindow.Activity();
        int initialStart = 0;
        int initialEndTime = 0;

        int previousStart = 0;
        int previousEndTime = 0;

        public newActivityWindow()
        {
            InitializeComponent();

            selectEndTime.ItemsSource = MainWindow.times;
            selectStartTime.ItemsSource = MainWindow.times;

            selectEndTime.SelectedIndex = MainWindow.newActivityEndTime + 1;
            selectStartTime.SelectedIndex = MainWindow.newActivityStartTime;

            previousStart = MainWindow.newActivityStartTime;
            previousEndTime = MainWindow.newActivityEndTime;


        }
        public newActivityWindow(MainWindow.Activity activityToEdit)
        {

            InitializeComponent();

            CreateButton.Content = "Update";
            CancelButton.Content = "Delete";


            isEditing = true;

            activity = activityToEdit;

            initialStart = activity.StartTime;

            initialEndTime = activity.EndTime;

            selectEndTime.ItemsSource = MainWindow.times;
            selectStartTime.ItemsSource = MainWindow.times;

            selectStartTime.SelectedIndex = activity.StartTime;

            selectEndTime.SelectedIndex = activity.EndTime + 1;

            BoxActivityName.Text = activity.Name;
            
            //startTimeBlock.Text = MainWindow.times[MainWindow.allRectangles.IndexOf(MainWindow.SelectedRectangleList[0])].ToString ();
            //endTimeBlock.Text = MainWindow.times [MainWindow.allRectangles.IndexOf(MainWindow.SelectedRectangleList[MainWindow.SelectedRectangleList.Count - 1])].ToString();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            activity.ActivityColor = (sender as Button).Background;
        }

        private void BoxActivityName_TextChanged(object sender, TextChangedEventArgs e)
        {
            activity.Name = (sender as TextBox).Text;
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (activity.ActivityColor != null && activity.Name != null)
            {
                if (!isEditing)
                {
                    if (activity.EndTime >= activity.StartTime)
                    {

                        //calculate start and end times
                        //activity.StartTime = MainWindow.allRectangles.IndexOf(MainWindow.SelectedRectangleList[0]);
                        //activity.EndTime = MainWindow.allRectangles.IndexOf(MainWindow.SelectedRectangleList[MainWindow.SelectedRectangleList.Count - 1]);

                        //if we moved up
                        if (previousStart - activity.StartTime > 0)
                        {
                            MainWindow.activitiesAffected.Clear();
                            ((MainWindow)(this.Owner)).checkEndTimes(activity.StartTime, previousStart - activity.StartTime, activity.EndTime);
                            foreach (MainWindow.Activity act in MainWindow.activitiesAffected)
                            {
                                if (act != null && act!=activity)
                                {
                                    Grid.SetRow(act.ActivityRect, act.StartTime - (previousStart - activity.StartTime));
                                    act.EndTime = act.StartTime + act.Span - 1;
                                }
                            }
                        }

                        //if we move down
                        if (previousEndTime - activity.EndTime < 0)
                        {
                            MainWindow.affectedActivitiesDown.Clear();
                            ((MainWindow)this.Owner).checkStartTimes(activity.EndTime, activity.EndTime - previousEndTime, activity.StartTime);
                            foreach (MainWindow.Activity act in MainWindow.affectedActivitiesDown)
                            {
                                if (act != null && act != activity)
                                {
                                    Grid.SetRow(act.ActivityRect, act.StartTime + (activity.EndTime - previousEndTime));
                                    act.EndTime = act.StartTime + act.Span - 1;
                                }
                            }
                        }

                        ((MainWindow)this.Owner).timeGrid.Children.Remove(((MainWindow)this.Owner).selectionRect);

                        ((MainWindow)this.Owner).CreateActivityBlock(activity);
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Please ensure end time is greater than start time");
                    }
                }
                else
                {
                    if (activity.EndTime - activity.StartTime > -1)
                    {

                        //if we moved up
                        if (initialStart - activity.StartTime > 0)
                        {
                            MainWindow.activitiesAffected.Clear();
                            ((MainWindow)(this.Owner)).checkEndTimes(activity.StartTime, initialStart - activity.StartTime, activity.EndTime);
                            foreach (MainWindow.Activity act in MainWindow.activitiesAffected)
                            {
                                if (act != null && act != activity)
                                {
                                    if (act.StartTime - (initialStart - activity.StartTime) >= 0)
                                    {
                                        Grid.SetRow(act.ActivityRect, act.StartTime - (initialStart - activity.StartTime));
                                    } else
                                    {
                                        Grid.SetRow(act.ActivityRect, 0);
                                    }
                                    act.EndTime = act.StartTime + act.Span - 1;
                                }
                            }

                        }

                        //if we move down
                        if (initialEndTime - activity.EndTime < 0)
                        {
                            MainWindow.affectedActivitiesDown.Clear();
                            ((MainWindow)this.Owner).checkStartTimes(activity.EndTime, activity.EndTime - initialEndTime, activity.StartTime);
                            foreach (MainWindow.Activity act in MainWindow.affectedActivitiesDown)
                            {
                                if (act != null && act != activity)
                                {
                                    Grid.SetRow(act.ActivityRect, act.StartTime + (activity.EndTime - initialEndTime));
                                    act.EndTime = act.StartTime + act.Span - 1;
                                }
                            }
                        }

                        activity.ActivityRect.Fill = activity.ActivityColor;
                        Grid.SetRow(activity.ActivityRect, activity.StartTime);
                        activity.Span = activity.EndTime - activity.StartTime + 1;
                        Grid.SetRowSpan(activity.ActivityRect, activity.Span);

                        MainWindow.ListOfTextBlocks [MainWindow.ListOfActivities.IndexOf (activity)].Text = activity.Name;

                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Please ensure end time is greater than or equal to start time");

                    }
                }
            } else
            {
                MessageBox.Show("Please Fill Everything Out");
            }
        }

        private void selectStartTime_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            activity.StartTime = selectStartTime.SelectedIndex;
        }

        private void selectEndTime_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            activity.EndTime = selectEndTime.SelectedIndex - 1;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (isEditing)
            {
                //deletion
                ((MainWindow)this.Owner).timeGrid.Children.Remove(activity.ActivityRect); //try this on measure backwards
                activity.ActivityRect = null;
                ((MainWindow)this.Owner).timeGrid.Children.Remove(activity.Block);
                MainWindow.ListOfTextBlocks.Remove(activity.Block);
                activity.Block = null;
                MainWindow.ListOfActivities.Remove(activity);
                activity = null;
            } else
            {
                ((MainWindow)this.Owner).timeGrid.Children.Remove(((MainWindow)this.Owner).selectionRect);
                activity = null;
            }

            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (((MainWindow)this.Owner).timeGrid.Children.Contains (((MainWindow)this.Owner).selectionRect))
            {
                ((MainWindow)this.Owner).timeGrid.Children.Remove(((MainWindow)this.Owner).selectionRect);
                activity = null;
            }

        }
    }
}
