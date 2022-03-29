using NoPasaranTD.Engine;
using System.Drawing;
using System.Windows.Forms;

namespace NoPasaranTD.Visuals
{
    /// <summary>
    /// Serviceklasse für einen Drag Drop Vorgang innerhalb der UI
    /// </summary>
    public class DragDropService
    {
        public delegate void DragDropFinishHandler(DragDropArgs args);
        public delegate void DragDropLeftHandler(DragDropArgs args);

        /// <summary>
        /// Dieses Event wird beim beenden eines DragDrop Vorganges ausgelöst
        /// </summary>
        public event DragDropFinishHandler DragDropFinish;

        public event DragDropLeftHandler DragDropLeft;
        /// <summary>
        /// Wie kann der Drag Vorgang als Drop bestätigt werden?
        /// </summary>
        public DragDropMode ApplySetting { get; set; } = DragDropMode.MouseLeftButtonUp;

        /// <summary>
        /// Wie kann der Drag Vorgang abgebrochen werden
        /// </summary>
        public DragDropMode LeaveSetting { get; set; } = DragDropMode.MouseRightButtonDown;

        /// <summary>
        /// Wie soll der Drag Vorgang umgesetzt werden
        /// </summary>
        public DragDropMoveMode MoveSetting { get; set; } = DragDropMoveMode.Pressed;

        /// <summary>
        /// Das zu bewegende Objekt als Rechteck
        /// </summary>
        public Rectangle MovedObject { get; set; } = Rectangle.Empty;

        /// <summary>
        /// Bewegt sich das Objekt zurzeit
        /// </summary>
        public bool IsMoving { get; private set; } = false;

        /// <summary>
        /// Was soll gezogen werden und beim setzen übergeben werden
        /// </summary>
        public object Context { get; set; }

        public void Update()
        {
            if (IsMoving)
            {
                MovedObject = new Rectangle(StaticEngine.MouseX - MovedObject.Width / 2, StaticEngine.MouseY - MovedObject.Height / 2, MovedObject.Width, MovedObject.Height);
            }
        }

        /// <summary>
        /// Startet den Drag Vorgang
        /// </summary>
        /// <param name="visual"></param>
        public void Start(Rectangle visual)
        {
            MovedObject = visual;
            IsMoving = true;
        }

        /// <summary>
        /// Stopt den Drag Vorgang 
        /// </summary>
        public void StopSuccessfully()
        {
            IsMoving = false;
            DragDropFinish?.Invoke(new DragDropArgs() { MovedObject = MovedObject, Context = Context });
            Context = null;
        }

        /// <summary>
        /// Stopt und verlässt den DragDrop Vorgang
        /// </summary>
        public void Leave()
        {
            IsMoving = false;
            DragDropLeft?.Invoke(new DragDropArgs() { MovedObject = MovedObject, Context = Context });
            Context = null;
        }

        public void MouseDown(MouseEventArgs args)
        {
            if (LeaveSetting == DragDropMode.MouseRightButtonDown && args.Button == MouseButtons.Right)
            {
                Leave();
            }
            else if (LeaveSetting == DragDropMode.MouseLeftButtonDown && args.Button == MouseButtons.Left)
            {
                Leave();
            }
        }

        public void MouseUp(MouseEventArgs args)
        {
            if (MoveSetting == DragDropMoveMode.Pressed)
            {
                if (ApplySetting == DragDropMode.MouseLeftButtonUp && args.Button == MouseButtons.Left)
                {
                    StopSuccessfully();
                }
                else if (ApplySetting == DragDropMode.MouseRightButtonUp && args.Button == MouseButtons.Right)
                {
                    StopSuccessfully();
                }
            }
        }
    }

    /// <summary>
    /// Argumentclasse für DragDrop Events
    /// </summary>
    public class DragDropArgs
    {
        /// <summary>
        /// Das bewegte Objekt
        /// </summary>
        public Rectangle MovedObject { get; set; }

        /// <summary>
        /// Das zu übergebende Objekt
        /// </summary>
        public object Context { get; set; }
    }

    public enum DragDropMode
    {
        MouseLeftButtonDown,
        MouseLeftButtonUp,
        MouseRightButtonUp,
        MouseRightButtonDown,
    }

    public enum DragDropMoveMode
    {
        Pressed,
        None,
    }
}
