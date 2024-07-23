using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace NetworkGh.Common
{
    public class ComponentButton : GH_ComponentAttributes
    {
        private const int ButtonSize = 18;

        private readonly string _label;
        private readonly Action _action;

        private RectangleF _buttonBounds;
        private bool _mouseDown;
        /// <summary>
        /// Add a button to a component
        /// </summary>
        public ComponentButton(GH_Component owner, string label, Action action) : base(owner)
        {
            _label = label;
            _action = action;
        }
        protected override void Layout()
        {
            base.Layout();

            const int margin = 3;

            Rectangle bounds = GH_Convert.ToRectangle(Bounds);
            Rectangle button = bounds;

            button.X += margin;
            button.Width -= margin * 2;
            button.Y = bounds.Bottom;
            button.Height = ButtonSize;

            bounds.Height += ButtonSize + margin;

            Bounds = bounds;
            _buttonBounds = button;
        }

        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            base.Render(canvas, graphics, channel);

            if (channel != GH_CanvasChannel.Objects) return;
            Font prototype = GH_FontServer.StandardAdjusted;
            Font font = GH_FontServer.NewFont(prototype, 6f / GH_GraphicsUtil.UiScale);
            const int radius = 3;
            int highlight = !_mouseDown ? 8 : 0;

            using GH_Capsule button = GH_Capsule.CreateTextCapsule(_buttonBounds, _buttonBounds, GH_Palette.Black, _label, font, radius, highlight);
            button.Render(graphics, false, Owner.Locked, false);
        }

        private void SetMouseDown(bool value, GH_Canvas canvas, GH_CanvasMouseEvent e, bool action = true)
        {
            if (Owner.Locked || _mouseDown == value)
                return;

            if (value && e.Button != MouseButtons.Left)
                return;

            if (!_buttonBounds.Contains(e.CanvasLocation))
                return;

            if (_mouseDown && !value && action)
                _action();

            _mouseDown = value;
            canvas.Invalidate();
        }

        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            SetMouseDown(true, sender, e);
            return base.RespondToMouseDown(sender, e);
        }

        public override GH_ObjectResponse RespondToMouseUp(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            SetMouseDown(false, sender, e);
            return base.RespondToMouseUp(sender, e);
        }

        public override GH_ObjectResponse RespondToMouseMove(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            SetMouseDown(false, sender, e, false);
            return base.RespondToMouseMove(sender, e);
        }
    }
}