using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NoPasaranTD.Engine.Visuals
{
    /// <summary>
    /// Eine Generische Listbox welche jeden beliebigen Item Container Typen annehmen kann
    /// </summary>
    /// <typeparam name="T">Model Typ</typeparam>
    /// <typeparam name="R">Item Container Typ</typeparam>
    public class ListContainer<T,R> where R : new()
    {
        #region Privat Member

        public delegate void SelectionChangedHandler();
        /// <summary>
        /// Wird beim einer neuen Auswahl eines Items in einem ListContainer ausgelöst
        /// </summary>
        public event SelectionChangedHandler SelectionChanged;

        private List<ItemContainer<T>> items = new List<ItemContainer<T>>();
        private ItemContainer<T> selectedItem;
        private Rectangle background = new Rectangle();

        #endregion

        #region Public Properties
        public Size ContainerSize
        {
            get { return new Size(background.Width, background.Height); }
            set 
            { 
                background.Width = value.Width; 
                background.Height = value.Height; 
            }
        }

        public Size ItemSize { get; set; }

        private Brush backgroundColor = Brushes.White;
        public Brush BackgroundColor { get; set; }
       
        public int Margin { get; set; }

        public Point Position
        {
            get { return new Point(background.X, background.Y); }
            set { background.X = value.X; background.Y = value.Y; }
        }

        public Graphics Graphics { get; set; }

        private NotifyCollection<T> _contextItems = new NotifyCollection<T>();
        public NotifyCollection<T> Items
        {
            get { return _contextItems; }
            set { _contextItems = value; _contextItems.CollectionChanged += ItemsCollectionChanged; }
        }

        public T SelectedItem { get => (selectedItem as ItemContainer<T>).DataContext; }
        
        #endregion

        public ListContainer()
        {
            Items.CollectionChanged += ItemsCollectionChanged;
            Engine.OnRender += Render;
            Engine.OnMouseDown += MouseLeftButton;
        }

        private void ItemsCollectionChanged() =>
            DrawItems();

        /// <summary>
        /// Zu jedem Model-Objekt wird ein eigener Item Container erstellt
        /// </summary>
        public void DrawItems()
        {
            items.Clear();
            for (int i = 0; i < Items.Count; i++)
            {
                ItemContainer<T> item = (new R() as ItemContainer<T>);
                item.DataContext = Items[i];
                item.ItemSize = ItemSize;
                item.Graphics = Graphics;
                item.Position = new Point(Position.X + i*(ItemSize.Width + Margin) + Margin, Position.Y);
                items.Add(item);
            }                                
        }

        private void Render(Graphics g) 
        {
            g.FillRectangle(BackgroundColor, background);
        }

        
        private void MouseLeftButton(MouseEventArgs args)
        {
            for (int i = 0; i < items.Count; i++)
                if (items[i].IsMouseOver)
                {
                    selectedItem = items[i];
                    SelectionChanged?.Invoke();
                }
                    
        }
    }

    

    /// <summary>
    /// Normale Generische Liste mit NotifyListChangedEvent
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NotifyCollection<T> : ICollection<T>
    {
        private ArrayList arrayList = new ArrayList();

        public delegate void NotifyOnListChanged();
        public event NotifyOnListChanged CollectionChanged;

        public int Count => arrayList.Count;

        public bool IsReadOnly { get; set; }

        public T this[int index] { get => (T)arrayList[index]; set => arrayList[index] = value; }

        public void Add(T item)
        {
            arrayList.Add(item);
            OnListChanged();
        }

        public void Clear() 
        {
            arrayList.Clear();
            OnListChanged();
        }

        public bool Contains(T item) =>
            arrayList.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) { }

        public IEnumerator<T> GetEnumerator() =>
            (IEnumerator<T>)arrayList.GetEnumerator();

        public bool Remove(T item)
        {
            try
            {
                arrayList.Remove(item);
                return true;
            }
            catch (Exception) { return false; }          
        }

        protected virtual void OnListChanged()
        {
            if (CollectionChanged != null)
                CollectionChanged();
        }
            

        IEnumerator IEnumerable.GetEnumerator() =>
             arrayList.GetEnumerator();
    }
}
