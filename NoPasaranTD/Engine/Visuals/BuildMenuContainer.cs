using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoPasaranTD.Engine.Visuals
{
    public class ListContainer<T,R> where R : new()
    {
        #region Privat Member

        private ContainerCollection<R> items = new ContainerCollection<R>();
        private R selectedItem;
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
        public Brush BackgroundColor
        {
            get { return backgroundColor; }
            set 
            { 
                backgroundColor = value; 
                Graphics.FillRectangle(backgroundColor, background); 
            }
        }
       
        public int Margin { get; set; }

        public Point Position
        {
            get { return new Point(background.X, background.Y); }
            set { background.X = value.X; background.Y = value.Y; }
        }

        public Graphics Graphics { get; set; }
        public List<T> Items { get; set; }
        
        #endregion

        public ListContainer()
        {
            items.CollectionChanged += ItemsCollectionChanged;
        }

        private void ItemsCollectionChanged()
        {

        }
        public void Draw()
        {
            DrawItems();
        }

        void DrawItems()
        {
            for (int i = 0; i < Items.Count; i++)
            {
                ItemContainer<T> item = (new R() as ItemContainer<T>);
                item.DataContext = Items[i];
                item.ItemSize = ItemSize;
                item.Graphics = Graphics;
                item.Position = new Point(Position.X + i*ItemSize.Width + Margin, Position.Y);
                item.Draw();
            }                                
        }
    }

    public class ContainerCollection<T> : ICollection<T>
    {
        private ArrayList arrayList = new ArrayList();

        public event Action CollectionChanged;

        public int Count => throw new NotImplementedException();

        public bool IsReadOnly => throw new NotImplementedException();

        public T this[int index] { get => (T)arrayList[index]; set => arrayList[index] = value; }

        public void Add(T item)
        {
            arrayList.Add(item);
            CollectionChanged.Invoke();
        }

        public void Clear() 
        {
            arrayList.Clear();
            CollectionChanged.Invoke();
        }

        public bool Contains(T item)
        {
            return arrayList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            //throw new NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            try
            {
                arrayList.Remove(item);
                CollectionChanged.Invoke();
                return true;
            }
            catch (Exception) { return false; }          
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
