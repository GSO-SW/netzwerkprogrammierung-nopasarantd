using NoPasaranTD.Engine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace NoPasaranTD.Visuals
{
    public class GuiComponent : List<GuiComponent>, IDisposable
    {
        /// <summary>
        /// Ist das Control sichtbar?
        /// </summary>
        public bool Visible { get; set; } = true;

        /// <summary>
        /// Die Grenzen des Controls
        /// </summary>
        public Rectangle Bounds { get; set; }

        /// <summary>
        /// Befindet sich die Maus über dem Control zurzeit?
        /// </summary>
        public bool IsMouseOver => Bounds.Contains(StaticEngine.MouseX, StaticEngine.MouseY);

        public virtual void Update() { }
        public virtual void Render(Graphics g) { }

        public virtual void KeyUp(KeyEventArgs e) { }
        public virtual void KeyPress(KeyPressEventArgs e) { }
        public virtual void KeyDown(KeyEventArgs e) { }

        public virtual void MouseUp(MouseEventArgs e) { }
        public virtual void MouseDown(MouseEventArgs e) { }
        public virtual void MouseMove(MouseEventArgs e) { }
        public virtual void MouseWheel(MouseEventArgs e) { }

        public virtual void Dispose() { }

        /// <summary>
        /// Überprüft ob irgendein Rechteck mit einem UI Element Collidiert eines GUI Components
        /// </summary>
        /// <param name="rectangle"></param>
        /// <returns></returns>
        public bool CollidesWithUI(Rectangle rectangle)
        {
            for (int i = 0; i < Count; i++)
            {
                if (this[i].Bounds.IntersectsWith(rectangle) && this[i].Visible)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Überprüft ob die derzeitige Mausposition auf einem UI Compnent ist
        /// </summary>
        /// <returns></returns>
        public bool IsMouseOnUI()
        {
            for (int i = 0; i < Count; i++)
            {
                if (this[i].Bounds.Contains(StaticEngine.MouseX, StaticEngine.MouseY) && this[i].Visible)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Fügt alle Properties eines GUI Components, welche ebenfalls ein GUI Component sind zur Class Collection hinzu
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="typeIn"></param>
        public void GetGUIComponents(object obj, Type typeIn)
        {
            Type type = typeIn;
            PropertyInfo[] properties = type.GetProperties();
            for (int i = 0; i < properties.Length - 1; i++)
            {
                if (properties[i].PropertyType.IsSubclassOf(typeof(GuiComponent)))
                {
                    Add((GuiComponent)properties[i].GetValue(obj));
                }
            }
        }

        public static Font StandartIconFont = new Font("Arial", 40);
        public static Font StandartHeader1Font = new Font("Arial", 24);
        public static Font StandartHeader2Font = new Font("Arial", 16, FontStyle.Bold, GraphicsUnit.Point);
        public static Font StandartText1Font = new Font("Arial", 11, FontStyle.Regular, GraphicsUnit.Point);
        public static Font StandartText2Font = new Font("Arial", 9, FontStyle.Regular, GraphicsUnit.Point);
        public static Font StandartText3Font = new Font("Arial", 7, FontStyle.Regular, GraphicsUnit.Point);
        public static Font StandartText4Font = new Font("Arial", 5, FontStyle.Regular, GraphicsUnit.Point);
    }
}
