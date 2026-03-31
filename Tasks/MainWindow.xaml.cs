using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Tasks
{
    
    public partial class MainWindow : Window
    {
        private const string SaveFile = "tasks.json";

        public MainWindow()
        {
            InitializeComponent();
            LoadTasks();
        }

        private void OnResize(object sender, RoutedEventArgs e)
        {
            var childrenHeight = ListContainer.Children.OfType<StackPanel>().Count() > 0 ? ListContainer.Children.OfType<StackPanel>().First().ActualHeight : 0;
            Scroller.Height = this.Height - Header.ActualHeight - childrenHeight;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void RemoveEmptyItems()
        {
            var toRemove = new List<StackPanel>();
            foreach (var sp in ListContainer.Children.OfType<StackPanel>())
            {
                foreach (var textBox in sp.Children.OfType<TextBox>())
                {
                    if (textBox.Text == "")
                    {
                        toRemove.Add(sp);
                        break;
                    }
                }
            }
            foreach (var item in toRemove)
            {
                ListContainer.Children.Remove(item);
            }
        }

        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            RemoveEmptyItems();
            CreateItem("");
        }

        private void HideDone(object sender, RoutedEventArgs e)
        {
            if (ListContainer.Children.Count == 0)
            {
                return;
            }
            if (DoneButton.Content.ToString() == "H")
            {
                foreach (var sp in ListContainer.Children.OfType<StackPanel>())
                {
                    foreach (var cb in sp.Children.OfType<CheckBox>())
                    {
                        if (cb.IsChecked == true && cb.IsVisible)
                        {
                            sp.Visibility = Visibility.Collapsed;
                        }
                    }
                }
                DoneButton.Content = "S";
            }
            else
            {
                foreach (var sp in ListContainer.Children.OfType<StackPanel>())
                {
                    sp.Visibility = Visibility.Visible;
                }
                DoneButton.Content = "H";
            }
        }

        private void CreateItem(string text, DateTime? checkedTime = null)
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2) };

            var checkbox = new CheckBox { VerticalAlignment = VerticalAlignment.Center };
            if (checkedTime != null) {
                checkbox.IsChecked = true;
            }
            var textbox = new TextBox
            {
                Margin = new Thickness(5, 0, 0, 0),
                Text = text
            };

            checkbox.Checked += (s, args) =>
            {
                SaveTasks();
            };

            textbox.TextChanged += (s, args) => SaveTasks();
            textbox.KeyDown += (s, args) =>
            {
                if (args.Key.Equals(Key.Enter))
                {
                    RemoveEmptyItems();
                    Keyboard.ClearFocus();
                }
            };

            panel.Children.Add(checkbox);
            panel.Children.Add(textbox);

            ListContainer.Children.Add(panel); 
            textbox.Focus();
            textbox.Select(0, 0);
        }

        private void SaveTasks()
        {
            var tasks = new List<TaskItem>();

            foreach (StackPanel panel in ListContainer.Children)
            {
                if (panel.Children[1] is TextBox tb && !string.IsNullOrWhiteSpace(tb.Text))
                {
                    if (panel.Children[0] is CheckBox cb)
                    {
                        var item = new TaskItem();
                        item.Name = tb.Text;
                        item.Done = cb.IsChecked == true ? DateTime.Now : null;
                        tasks.Add(item);
                    }
                }
            }
            using (FileStream fs = new FileStream(SaveFile, FileMode.OpenOrCreate))
            {
                using (TextWriter tw = new StreamWriter(fs, Encoding.UTF8, 1024, true))
                {
                    tw.WriteLine(JsonSerializer.Serialize(tasks));
                }
                fs.SetLength(fs.Position);
            }
            
            FileAttributes attributes = File.GetAttributes(SaveFile);
            if ((attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
            {
                File.SetAttributes(SaveFile, attributes | FileAttributes.Hidden);
            }
        }

        private void LoadTasks()
        {
            if (!File.Exists(SaveFile)) return;

            var json = File.ReadAllText(SaveFile);

            List<TaskItem> tasksNew;
            try
            {
                tasksNew = JsonSerializer.Deserialize<List<TaskItem>>(json);
            } catch {
                tasksNew = new List<TaskItem>();
            }

            if (tasksNew == null) return;

            foreach (var task in tasksNew)
            {
                CreateItem(task.Name, task.Done);
            }
        }
    }
}