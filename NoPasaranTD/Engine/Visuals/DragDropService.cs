using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NoPasaranTD.Engine.Visuals
{
    public class DragDropService
    {
        public delegate void DragDropFinishHandler(DragDropArgs args);
        public event DragDropFinishHandler DragDropFinish;

        public DragDropService()
        {
            Engine.OnUpdate += Update;
            Engine.OnMouseDown += MouseDown;
            Engine.OnMouseUp += MouseUp;
        }

        public DragDropMode ApplySetting { get; set; } = DragDropMode.MouseRightButtonDown;
        public DragDropMode LeaveSetting { get; set; } = DragDropMode.MouseRightButtonDown;
        public DragDropMoveMode MoveSetting { get; set; } = DragDropMoveMode.Pressed;
        public Rectangle MovedObject { get; set; } = Rectangle.Empty;
        public bool IsMoving { get; private set; } = false;

        private void Update()
        {
            if (IsMoving)
                MovedObject = new Rectangle(Engine.MouseX - MovedObject.Width/2, Engine.MouseY - MovedObject.Height / 2, MovedObject.Width, MovedObject.Height);
        }

        public void Start(Rectangle visual)
        {
            MovedObject = visual;
            Engine.OnUpdate += Update;
            Engine.OnMouseDown += MouseDown;
            Engine.OnMouseUp += MouseUp;
            IsMoving = true;
        }
            

        public void Stop()
        {
            Engine.OnUpdate -= Update;
            Engine.OnMouseDown -= MouseDown;
            Engine.OnMouseUp -= MouseUp;
            IsMoving = false;
            DragDropFinish?.Invoke(new DragDropArgs() { MovedObject = MovedObject});
        }
            

        private void MouseDown(MouseEventArgs args)
        {
            //if ((ApplySetting == DragDropMode.MouseLeftButtonDown && args.Button == MouseButtons.Left) || (ApplySetting == DragDropMode.MouseRightButtonDown && args.Button == MouseButtons.Right))
            //    Stop();
        }

        private void MouseUp(MouseEventArgs args)
        {
            if (args.Button == MouseButtons.Left && MoveSetting == DragDropMoveMode.Pressed)
                Stop();
        }
    }

    public class DragDropArgs
    {
        public Rectangle MovedObject { get; set; }
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
