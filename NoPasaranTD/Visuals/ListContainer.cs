using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace NoPasaranTD.Visuals
{
    /// <summary>
    /// Eine Generische Listbox welche jeden beliebigen Item Container Typen annehmen kann
    /// </summary>
    /// <typeparam name="T">Model Typ (Welches Model soll genutzt werden z.b Tower)</typeparam>
    /// <typeparam name="R">Item Container Typ (Welcher Containertyp wird Verwendet z.b TowerItemContainer)</typeparam>
    public class ListContainer<T,R> : GuiComponent where R : new()
    {
        #region Events

        public delegate void SelectionChangedHandler();
        /// <summary>
        /// Wird beim einer neuen Auswahl eines Items in einem ListContainer ausgelöst
        /// </summary>
        public event SelectionChangedHandler SelectionChanged;

        #endregion
        #region Constructor

        public ListContainer()
            => Items.CollectionChanged += ItemsCollectionChanged;

        #endregion
        #region Private Members

        private List<ItemContainer<T>> items = new List<ItemContainer<T>>(); // Liste von allen Visuellen Item Containern
        private ItemContainer<T> selectedItem; // Der derzeitige ausgewählte Item Container

        #endregion

        #region Public Properties
        /// <summary>
        /// Orientation beim scrollen
        /// </summary>
        public Orientation Orientation { get; set; } = Orientation.Vertical;

        /// <summary>
        /// Die Größe des Containers
        /// </summary>
        public Size ContainerSize
        {
            get => new Size(Bounds.Width, Bounds.Height); 
            set => Bounds = new Rectangle(Position, value); 
        }

        /// <summary>
        /// Die Größe eines Item Objektes
        /// </summary>
        public Size ItemSize { get; set; }

        /// <summary>
        /// Die Hintergrund Farbe des Containers
        /// </summary>
        public Brush BackgroundColor { get; set; } = Brushes.White;
       
        /// <summary>
        /// Der Abstand eines Items zu den Containern Grenzen und anderen Items
        /// </summary>
        public int Margin { get; set; }

        /// <summary>
        /// Die Scrittgröße beim Scrollen
        /// </summary>
        public int ScrollSteps { get; set; } = 20;

        /// <summary>
        /// Die Position des Containers auf dem Screen
        /// </summary>
        public Point Position
        {
            get => new Point(Bounds.X, Bounds.Y); 
            set => Bounds = new Rectangle(value, ContainerSize); 
        }

        private NotifyCollection<T> _contextItems = new NotifyCollection<T>();
        /// <summary>
        /// Model Item Sammlung
        /// Hier befinden sich alle Model Objekte mit einem Bezug zur Box
        /// </summary>
        public NotifyCollection<T> Items
        {
            get => _contextItems; 
            set { _contextItems = value; _contextItems.CollectionChanged += ItemsCollectionChanged; }
        }

        /// <summary>
        /// Das dezeitige Ausgewählte Model Objekt
        /// </summary>
        public T SelectedItem { get => selectedItem.DataContext; }
        
        #endregion      
        #region Public Methods

        /// <summary>
        /// Zu jedem Model-Objekt wird ein eigener Item Container erstellt
        /// </summary>
        public void DefineItems()
        {
            items.Clear();

            int factorX = Orientation == Orientation.Horizontal ? 1 : 0;
            int factorY = Orientation == Orientation.Vertical ? 1 : 0;

            for (int i = 0; i < Items.Count; i++)
            {
                // Platziert für jedes Model Objekt einen eigenen Container im List-Container
                ItemContainer<T> item = (new R() as ItemContainer<T>);

                item.ParentBounds = Bounds;
                item.DataContext = Items[i];
                item.ItemSize = new Size(ItemSize.Width, ItemSize.Height);

                item.Position = new Point(
                    Position.X + i*(ItemSize.Width + Margin)*factorX + Margin,
                    Position.Y + i*(ItemSize.Height + Margin)*factorY + Margin
                );
                
                items.Add(item);
            }                                
        }

        public override void Update()
        {
            for (int i = items.Count - 1; i >= 0; i--)
                items[i].Update();
        }

        public override void Render(Graphics g)
        {
            g.FillRectangle(BackgroundColor, Bounds);

            Region clip = g.Clip; // Speichere ursprüngliche Region
            g.Clip = new Region(Bounds); // Entferne alles was außerhalb ist

            // Render Items innerhalb dieser Region
            for (int i = items.Count - 1; i >= 0; i--)
                items[i].Render(g);

            g.Clip = clip; // Setze ursprüngliche Region zurück
        }

        public override void KeyUp(KeyEventArgs e)
        {
            for (int i = items.Count - 1; i >= 0; i--)
                items[i].KeyUp(e);
        }

        public override void KeyDown(KeyEventArgs args)
        {
            for (int i = items.Count - 1; i >= 0; i--)
                items[i].KeyDown(args);
        }

        public override void MouseUp(MouseEventArgs e)
        {
            for (int i = items.Count - 1; i >= 0; i--)
                items[i].MouseUp(e);
        }

        public override void MouseDown(MouseEventArgs e)
        {
            for (int i = items.Count - 1; i >= 0; i--)
            {
                items[i].MouseDown(e);
                if (items[i].IsMouseOver)
                {
                    selectedItem = items[i];
                    SelectionChanged?.Invoke();
                }
            }
        }

        public override void MouseMove(MouseEventArgs e)
        {
            for (int i = items.Count - 1; i >= 0; i--)
                items[i].MouseMove(e);
        }

        public override void MouseWheel(MouseEventArgs e)
        {
            int factorX = Orientation == Orientation.Horizontal ? 1 : 0;
            int factorY = Orientation == Orientation.Vertical ? 1 : 0;

            int delta = e.Delta < 0 ? -1 : 1;
            for (int i = items.Count - 1; i >= 0; i--)
            {
                items[i].MouseWheel(e);
                items[i].TranslateTransform(
                    delta * ScrollSteps * factorX,
                    delta * ScrollSteps * factorY
                );
            }
        }

        #endregion
        #region Private Methods

        private void ItemsCollectionChanged() =>
            DefineItems();

        #endregion
    }

    /// <summary>
    /// Normale Generische Liste mit NotifyListChangedEvent
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NotifyCollection<T> : ICollection<T>
    {
        private ArrayList arrayList = new ArrayList();

        public delegate void NotifyOnListChanged();
        /// <summary>
        /// Wird bei einer Änderung der Liste ausgelöst
        /// </summary>
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
                OnListChanged();
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
