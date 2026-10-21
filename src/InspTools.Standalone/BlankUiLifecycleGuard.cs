using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;

namespace InspTools.Standalone
{
    /// <summary>
    /// Original ToolsWindow Closing/Closed callbacks assume eight fully built
    /// CVInspection objects. The R1 blank shell intentionally has no AI/native
    /// runtime, so it must close as a plain WPF window until a real Recipe loads.
    /// The original callbacks are restored as soon as Recipe is initialized.
    /// </summary>
    internal sealed class BlankUiLifecycleGuard
    {
        private readonly Window _window;
        private CancelEventHandler _originalClosing;
        private EventHandler _originalClosed;

        private BlankUiLifecycleGuard(Window window)
        {
            _window = window;
        }

        internal static BlankUiLifecycleGuard Attach(Window window)
        {
            var guard = new BlankUiLifecycleGuard(window);
            EventHandlerList events = GetEvents(window);
            if (events == null) return guard;

            object closingKey = GetEventKey("EVENT_CLOSING");
            object closedKey = GetEventKey("EVENT_CLOSED");
            if (closingKey == null || closedKey == null) return guard;
            var closing = events[closingKey];
            var closed = events[closedKey];
            if (closing != null)
            {
                foreach (Delegate cb in closing.GetInvocationList())
                {
                    if (cb.Method.DeclaringType != null &&
                        cb.Method.DeclaringType.FullName == "InspTools.ToolsWindow" &&
                        cb.Method.Name == "Window_Closing")
                    {
                        guard._originalClosing = (CancelEventHandler)cb;
                        window.Closing -= guard._originalClosing;
                    }
                }
            }
            if (closed != null)
            {
                foreach (Delegate cb in closed.GetInvocationList())
                {
                    if (cb.Method.DeclaringType != null &&
                        cb.Method.DeclaringType.FullName == "InspTools.ToolsWindow" &&
                        cb.Method.Name == "Window_Closed")
                    {
                        guard._originalClosed = (EventHandler)cb;
                        window.Closed -= guard._originalClosed;
                    }
                }
            }
            return guard;
        }

        internal void Restore()
        {
            if (_originalClosing != null)
            {
                _window.Closing += _originalClosing;
                _originalClosing = null;
            }
            if (_originalClosed != null)
            {
                _window.Closed += _originalClosed;
                _originalClosed = null;
            }
        }

        private static EventHandlerList GetEvents(Window window)
        {
            FieldInfo field = typeof(Window).GetField("_events",
                BindingFlags.Instance | BindingFlags.NonPublic);
            return field == null ? null : field.GetValue(window) as EventHandlerList;
        }

        private static object GetEventKey(string name)
        {
            FieldInfo field = typeof(Window).GetField(name,
                BindingFlags.Static | BindingFlags.NonPublic);
            return field == null ? null : field.GetValue(null);
        }
    }
}
