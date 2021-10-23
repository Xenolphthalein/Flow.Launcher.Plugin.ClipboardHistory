using System;
using System.Linq;
using System.Collections.Generic;
using Flow.Launcher.Plugin;
using System.Threading.Tasks;
using WindowsInput;
using WindowsInput.Native;

namespace Flow.Launcher.Plugin.ClipboardHistory
{
    public class ClipboardHistory : IPlugin
    {
        private const int MaxDataCount = 5000;
        //private readonly KeyboardSimulator keyboardSimulator = new KeyboardSimulator(new InputSimulator());
        private readonly InputSimulator inputSimulator = new InputSimulator();
        private PluginInitContext context;
        LinkedList<string> dataList = new LinkedList<string>();

        public List<Result> Query(Query query)
        {
            var results = new List<Result>();
            IEnumerable<string> displayData;

            if (query.Terms.Length == 0)
            {
                displayData = dataList;
            }
            else
            {
                displayData = dataList.Where(i => i.ToLower().Contains(query.SecondToEndSearch.ToLower()));
            }

            results.AddRange(displayData.Select(o => new Result
            {
                Title = o.Trim().Replace("\r\n", " ").Replace('\n', ' '),
                IcoPath = "Images\\clipboard.png",
                Action = c =>
                {
                    if (!ClipboardMonitor.ClipboardWrapper.SetText(o))
                        return false;

                    Task.Delay(50).ContinueWith(t => inputSimulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_V));
                    return true;
                }
            }));
            return results;
        }

        public void Init(PluginInitContext context)
        {
            this.context = context;
            ClipboardMonitor.OnClipboardChange += ClipboardMonitor_OnClipboardChange;
            ClipboardMonitor.Start();
        }

        void ClipboardMonitor_OnClipboardChange(ClipboardFormat format, object data)
        {
            if (format == ClipboardFormat.Html ||
                format == ClipboardFormat.SymbolicLink ||
                format == ClipboardFormat.Text ||
                format == ClipboardFormat.UnicodeText)
            {
                if (data != null && !string.IsNullOrEmpty(data.ToString().Trim()))
                {
                    LinkedListNode<string> node = dataList.Find(data.ToString());
                    if (node != null)
                    {
                        dataList.Remove(node);
                    }
                    dataList.AddFirst(data.ToString());

                    if (dataList.Count > MaxDataCount)
                    {
                        dataList.RemoveLast();
                    }
                }
            }
        }
    }
}