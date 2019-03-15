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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace Scheduler3._0
{
    public partial class MainWindow : Window
    {
        double oldMouseYPos = 0;

        int previousNumber = 0;

        public static List<int> times = new List<int>();

        int direction;

        double currentPositionY = 0;
        public static List<Rectangle> allRectangles = new List<Rectangle>();
        bool MovingBlock;

        public static List<Activity> ListOfActivities = new List<Activity>();

        public static List<TextBlock> ListOfTextBlocks = new List<TextBlock>();
        Activity activityToMove;

        Brush rectBackgrounds = Brushes.White;
        int zindex = 20;

        static public List<Activity> affectedActivitiesDown = new List<Activity>();

        static public List<Activity> activitiesAffected = new List<Activity>();

        bool selecting = false;

        double initialYPosition;

        Class1 notifyClass = new Class1();

        int maxStartTime = -1;

        int initialYOffset = 0;

        double initialPixelYOffset = 0;

        int correctACTPosition;




        public MainWindow()
        {
            InitializeComponent();

            timeGrid.MouseMove += TimeGrid_MouseMove;
            timeGrid.MouseLeave += TimeGrid_MouseLeave;

            for (int i = 0; i < 96; i++)
            {
                if (i == 0)
                {
                    times.Add(previousNumber);
                }
                else
                {
                    if ((previousNumber % 100) > 44)
                    {
                        times.Add(previousNumber + 55);
                        previousNumber = previousNumber + 55;
                    }
                    else
                    {
                        times.Add(previousNumber + 15);
                        previousNumber = previousNumber + 15;
                    }
                }
            }

            //generates rows
            for (int i = 0; i < 96; i++)
            {
                //create new row
                RowDefinition row = new RowDefinition();
                row.Height = GridLength.Auto;
                timeGrid.RowDefinitions.Add(row);

                //create rectangles for the rows we made
                Rectangle rect = new Rectangle();
                rect.Fill = rectBackgrounds;
                rect.Height = 15;
                rect.Stroke = Brushes.Gray;
                rect.StrokeThickness = 0.1;
                rect.MouseDown += Rect_MouseDown;
                timeGrid.Children.Add(rect);
                allRectangles.Add(rect);

                //create textblocks that display times 
                if (i % 4 == 0)
                {

                    TextBlock text = new TextBlock();

                    text.FontFamily = new FontFamily("Verdana");


                    text.FontWeight = FontWeights.Light;

                    if (i > 48)
                    {
                        text.Text = times[i - 48].ToString() + " PM";

                    } else
                    {
                        text.Text = times[i].ToString() + " AM";
                    }


                    text.FontSize = 10;
                    text.TextAlignment = TextAlignment.Center;
                    timeGrid.Children.Add(text);

                    Grid.SetRow(text, i);
                    Grid.SetColumn(text, 0);
                }
                Grid.SetRow(rect, i);
                Grid.SetColumn(rect, 1);
            }
        }       
        
        private void TimeGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            if (MovingBlock)
            {
                if (e.GetPosition (this).Y < 50)
                {
                    Activity actToMove = findActivityOverlap (activityToMove.EndTime);
                    if (actToMove != null)
                    {
                        Grid.SetRow(activityToMove.Block, actToMove.StartTime - activityToMove.Span);
                        activityToMove.EndTime = activityToMove.StartTime + activityToMove.Span;
                    }
                }
            }
        }

        private Activity findActivityOverlap (int time)
        {
            foreach (Activity act in ListOfActivities)
            {
                if (act != activityToMove)
                {
                    if (time >= act.StartTime && time <= act.EndTime)
                    {
                        return act;
                    }                    
                }
            }
            return null;
        }

        private void TimeGrid_MouseMove(object sender, MouseEventArgs e)
        {

            if (MovingBlock)
            {
                setZIndex(activityToMove.Block);

                assignActivityEndTime();

                int newStartTime = initialYOffset + (int)Math.Round((e.GetPosition(this).Y - initialPixelYOffset) / 15);
                int newEndTime = newStartTime + activityToMove.Span - 1;

                foreach (Activity act in ListOfActivities)
                {
                    if (act != activityToMove)
                    {
                        if (direction == 1)
                        {
                            if (newStartTime == act.EndTime)
                            {
                                correctACTPosition = activityToMove.StartTime - act.Span;
                                try
                                {
                                    Grid.SetRow(act.ActivityRect, act.StartTime + activityToMove.Span);
                                } catch (Exception ex)
                                {
                                    MessageBox.Show("There was an error: " + ex.ToString());
                                }
                                act.EndTime = act.StartTime + act.Span - 1;
                                SetCursorPos((int)this.PointToScreen(e.GetPosition(this)).X, (int)this.PointToScreen(e.GetPosition(this)).Y - (act.Span * 13));
                            }
                        }
                        else
                        {
                            if (newEndTime == act.StartTime)
                            {
                                correctACTPosition = activityToMove.StartTime + act.Span;
                                try
                                {
                                    Grid.SetRow(act.ActivityRect, act.StartTime - activityToMove.Span);
                                } catch (Exception ex)
                                {
                                    MessageBox.Show("There was an error: " + ex.ToString());
                                }
                                act.EndTime = act.StartTime + act.Span - 1;
                                SetCursorPos((int)this.PointToScreen(e.GetPosition(this)).X, (int)this.PointToScreen(e.GetPosition(this)).Y + (act.Span * 13));
                            }
                        }
                    }
                }

                if (newStartTime < 0)
                {
                    newStartTime = 0;
                }
                try
                {
                    Grid.SetRow(activityToMove.Block, newStartTime);
                } catch (Exception ex)
                {
                    MessageBox.Show("There was an error: " + ex.ToString());
                }
                activityToMove.EndTime = newEndTime;

            }
        }

        private bool loadActivities ()
        {
            OpenFileDialog openFile = new OpenFileDialog();

            openFile.Filter = "XML File (*.xml)|*.xml";

            openFile.InitialDirectory = System.AppDomain.CurrentDomain.BaseDirectory + "saveFiles";

            if (openFile.ShowDialog() == true)
            {
                //make sure the file isn't empty
                if (new FileInfo(openFile.FileName).Length != 0) //System.AppDomain.CurrentDomain.BaseDirectory + "//saveFile.xml").Length != 0)
                {
                    //if it exists, de serialize
                    List<SaveableActivity> loadedActivityList;
                    XmlSerializer serializer = new XmlSerializer(typeof(List<SaveableActivity>));
                    FileStream stream = new FileStream(openFile.FileName, FileMode.Open);
                    loadedActivityList = (List<SaveableActivity>)serializer.Deserialize(stream);

                    //now that we have the list, we can re-assign the other list
                    foreach (SaveableActivity savedAct in loadedActivityList)
                    {
                        Activity act = new Activity();
                        act.ActivityColor = savedAct.Color;
                        act.EndTime = savedAct.EndTime;
                        act.Name = savedAct.Name;
                        act.StartTime = savedAct.StartTime;

                        CreateActivityBlock(act);
                    }
                    return true;

                    //now we need to assign the activities for the list of timeSlots
                    //just pass the activity to createactivityblock - really should be renamed

                } else
                {
                    return false;
                }
            } else
            {
                return false;
            }
        }
        private void setZIndex(TextBlock sender) { 
            Grid.SetZIndex(sender, zindex);
            Grid.SetZIndex(activityToMove.ActivityRect, zindex);
            zindex += 1;
        }

        private void assignActivityEndTime()
        {
            activityToMove.EndTime = activityToMove.StartTime + activityToMove.Span - 1;
        }

        private void Text_MouseLeave(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = null;
        }

        private void Text_MouseEnter(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.SizeAll;
        }

        private void Block_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            oldMouseYPos = e.GetPosition(this).Y;
            
            //so if we're not already moving, then start moving and assign activity
            if (MovingBlock == false)
            {
                //lately I've been having problems with this line
                activityToMove = ListOfActivities[ListOfTextBlocks.IndexOf((sender as TextBlock))];
                initialYOffset = activityToMove.StartTime;
                initialPixelYOffset = e.GetPosition(this).Y;

                MovingBlock = true;

            }
        }
        public void CreateActivityBlock(Activity _activity)
        {
            //Not incredibly readable...

            //assign activity span
            _activity.Span = _activity.EndTime - _activity.StartTime + 1;
            Rectangle rect = new Rectangle();

            _activity.EndTime = _activity.StartTime + _activity.Span - 1;

            //This is the fill binding
            Binding binding = createBinding("ActivityColor", _activity);
            BindingOperations.SetBinding(rect, Rectangle.FillProperty, binding);
            //since I have the binding I shouldn't need to explicity set the color

            //binds width and position
            timeGrid.Children.Add(rect);

            rect.MouseDown += EditActivity;
            rect.MouseEnter += changeCursor;
            rect.MouseLeave += changeCursorBack;

            binding = createBinding("Span", _activity);
            BindingOperations.SetBinding(rect, Grid.RowSpanProperty, binding);

            binding = createBinding("StartTime", _activity);
            BindingOperations.SetBinding(rect, Grid.RowProperty, binding);

            Grid.SetColumn(rect, 1);

            //now we need a textblock
            TextBlock block = new TextBlock();
            block.MouseEnter += Text_MouseEnter;
            block.MouseLeave += Text_MouseLeave;
            block.MouseLeftButtonDown += Block_MouseLeftButtonDown;
            block.MouseLeftButtonUp += Block_MouseLeftButtonUp;

            binding = createBinding("Name", _activity);
            BindingOperations.SetBinding(block, TextBlock.TextProperty, binding);
            
            _activity.Block = block;

            //new
            //_activity.Name = _activity.Name + " - " + times [_activity.StartTime].ToString() + " to " + times[_activity.EndTime + 1].ToString();
            //_activity.Block.Text = _activity.Name;

            //bind position in the grid
            timeGrid.Children.Add(block);

            binding = createBinding("StartTime", _activity);
            BindingOperations.SetBinding(block, Grid.RowProperty, binding);

            Grid.SetColumn(block, 1);

            //ClearSelectionList();

            ListOfActivities.Add(_activity);

            ListOfTextBlocks.Add(block);

            _activity.ActivityRect = rect;
        }

        private void Block_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //better idea: check mouse position, if it hasn't moved in the y direction then we're good.
            if (Math.Abs(e.GetPosition (this).Y - oldMouseYPos) < 0.01)
            {

                //open the editing dialog

                foreach (Activity act in ListOfActivities)
                {
                    if (act.Block == (sender as TextBlock))
                    {
                        newActivityWindow actWindow = new newActivityWindow(act);
                        actWindow.Owner = this;
                        actWindow.Show();
                        break;
                    }
                }
            }
        }

        private void EditActivity(object sender, MouseButtonEventArgs e)
        {
            foreach (Activity act in ListOfActivities)
            {
                if (act.ActivityRect == (sender as Rectangle))
                {
                    newActivityWindow actWindow = new newActivityWindow(act);
                    actWindow.Owner = this;
                    actWindow.Show();
                    break;
                }
            }
        }

        private void changeCursor(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void changeCursorBack(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = null;
        }

        /*
        void ClearSelectionList()
        {
            foreach (Rectangle rect in SelectedRectangleList)
            {
                rect.Fill = rectBackgrounds;

            }
            SelectedRectangleList.Clear();
        }
        */
        Binding createBinding(string _path, object _source)
        {
            Binding myBinding = new Binding();
            myBinding.Source = _source;
            myBinding.Path = new PropertyPath(_path);
            myBinding.Mode = BindingMode.TwoWay;
            myBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            return myBinding;
        }

        public class Activity
        {
            public string Name { get; set; }
            public Brush ActivityColor { get; set; }
            public int StartTime { get; set; }
            public int EndTime { get; set; }
            public int Span { get; set; }
            public Rectangle ActivityRect { get; set; }
            public TextBlock Block { get; set; }

        }

        int displacementFactor = 0;

        private void Rect_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((sender as Rectangle).Fill == rectBackgrounds)
            {
                selecting = true;

                initialYPosition = e.GetPosition(this).Y;
                if (!timeGrid.Children.Contains(selectionRect))
                {
                    timeGrid.Children.Add(selectionRect);
                }

                displacementFactor = allRectangles.IndexOf(sender as Rectangle);

            }
        }

        //moving up
        public void checkEndTimes(int timeToCheck, int unitsUp, int actEndTime)
        {
            foreach (Activity act in ListOfActivities)
            {
                if (actEndTime >= act.EndTime && timeToCheck <= act.EndTime)
                {
                    if (activitiesAffected.Contains(act) == false)
                    {
                        Console.WriteLine("added to list");
                        activitiesAffected.Add(act);
                        checkEndTimes(act.StartTime - unitsUp, unitsUp, act.EndTime);
                    }
                }
            }
        }

        //moving down
        public void checkStartTimes(int timeToCheck, int unitsUp, int actStartTime)
        {
            foreach (Activity act in ListOfActivities)
            {
                if (actStartTime <= act.StartTime && timeToCheck >= act.StartTime)
                {
                    if (affectedActivitiesDown.Contains (act) == false)
                    {
                        affectedActivitiesDown.Add(act);
                        checkStartTimes(act.EndTime + unitsUp, unitsUp, act.StartTime);
                    }
                }
            }
        }

        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        public Rectangle selectionRect = new Rectangle();

        public static int newActivitySpan = 0;
        public static int newActivityStartTime = 0;
        public static int newActivityEndTime = 0;

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                double deltaDirection = currentPositionY - e.GetPosition(this).Y;
                direction = deltaDirection > 0 ? 1 : -1;
                currentPositionY = e.GetPosition(this).Y;

                if (selecting)
                {
                    if (maxStartTime == -1)
                    {
                        if (ListOfActivities.Count > 0)
                        {
                            foreach (Activity act in ListOfActivities)
                            {
                                if (act.StartTime > displacementFactor)
                                {
                                    if (maxStartTime == -1)
                                    {
                                        maxStartTime = act.StartTime;
                                    }
                                    else
                                    {
                                        if (maxStartTime > act.StartTime)
                                        {
                                            maxStartTime = act.StartTime;
                                        }
                                    }
                                }
                            }
                            if (maxStartTime == -1)
                            {
                                maxStartTime = 96;
                            }
                        }
                        else
                        {
                            maxStartTime = 96;
                        }
                    }

                    selectionRect.Fill = Brushes.BlanchedAlmond;

                    Grid.SetColumn(selectionRect, 1);
                    if (initialYPosition < e.GetPosition(this).Y && (((int)Math.Round((e.GetPosition(this).Y - initialYPosition) / 15) + 1 + displacementFactor) <= maxStartTime))
                    {

                        //this, and newendtime, is a static variable used in newActivityWindow to assign the new ... activity
                        newActivityStartTime = displacementFactor;

                        Grid.SetRow(selectionRect, newActivityStartTime);

                        newActivitySpan = (int)Math.Round((e.GetPosition(this).Y - initialYPosition) / 15) + 1;

                        Grid.SetRowSpan(selectionRect, newActivitySpan);

                        newActivityEndTime = newActivitySpan + newActivityStartTime - 1;

                    }
                }
            }
            else
            {
                if (selecting)
                {
                    newActivityWindow propertiesWindow = new newActivityWindow();
                    propertiesWindow.Owner = this;
                    propertiesWindow.Show();

                }

                maxStartTime = -1;

                currentPositionY = e.GetPosition(this).Y;
                selecting = false;
                MovingBlock = false;
            }
        }

        [XmlInclude(typeof(SolidColorBrush))]
        [XmlInclude(typeof(MatrixTransform))]

        public class SaveableActivity
        {
            public int StartTime { get; set; }
            public int EndTime { get; set; }
            public string Name { get; set; }
            public Brush Color { get; set; }

        }

        bool SaveActivities()
        {
            List<SaveableActivity> saveList = new List<SaveableActivity>();
            foreach (Activity act in ListOfActivities)
            {
                SaveableActivity actToSave = new SaveableActivity();
                actToSave.Color = act.ActivityColor;
                actToSave.EndTime = act.EndTime;
                actToSave.StartTime = act.StartTime;
                actToSave.Name = act.Name;
                saveList.Add(actToSave);
            }
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "XML File (*.xml)|*.xml";
            save.InitialDirectory = System.AppDomain.CurrentDomain.BaseDirectory + "saveFiles";

            if (save.ShowDialog() == true)
            {
                Console.WriteLine("showing dialog"); //only printed once you press "ok"
                FileStream file = File.Create(save.FileName);
                var xmlSerializer = new XmlSerializer(typeof(List<SaveableActivity>));
                xmlSerializer.Serialize(file, saveList);
                return true;
            }
            else
            {
                return false;
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            notifyClass = null;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            //open
            ClearTimeGrid();
            if (loadActivities())
            {
                MessageBox.Show("Success");
            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            //save
            if (SaveActivities())
            {
                MessageBox.Show("Success");
            }
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            //help tab: I just need to open a new window
            HelpWindow hw = new HelpWindow();
            hw.Owner = this;
            hw.Show();
        }

        private void ClearTimeGrid()
        {
            if (ListOfActivities.Count > 0)
            {
                foreach (Activity act in ListOfActivities)
                {
                    timeGrid.Children.Remove(act.ActivityRect);
                    act.ActivityRect = null;
                    timeGrid.Children.Remove(act.Block);
                    act.Block = null;
                }
                ListOfActivities.Clear();
                ListOfTextBlocks.Clear();
            }
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            ClearTimeGrid();

        }
    }
}

